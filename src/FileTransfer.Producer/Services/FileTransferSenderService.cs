using FileTransfer.Contracts;
using FileTransfer.Infrastructure.Transport;
using FileTransfer.Producer.Configuration;
using FileTransfer.Producer.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FileTransfer.Producer.Services
{
    public class FileTransferSenderService : IFileTransferSenderService
    {
        private IChunkTransport _chunkTransport;
        private IFileChunkerService _fileChunker;

        private bool _corrupted;

        public FileTransferSenderService(IChunkTransport chunkTransport, IFileChunkerService fileChunker)
        {
            _chunkTransport = chunkTransport;
            _fileChunker = fileChunker;
        }

        public async Task<bool> SendFile(string filePath, CancellationToken cancellationToken = default)
        {
            try
            {
                if(string.IsNullOrEmpty(filePath))
                {
                    throw new ArgumentException("File path cannot be null or empty.", nameof(filePath));
                }

                if(!File.Exists(filePath))
                {
                    throw new FileNotFoundException("File not found.", filePath);
                }

                var chunks = _fileChunker.GetFileChunks(filePath, cancellationToken);
                var hasChunks = false;

                await foreach (var chunk in chunks)
                {
                    var chunkToSend = chunk;

                    // Simulation: Corrupt the 3rd chunk to test error handling and retry logic
                    if (chunk.ChunkIndex == 3 && !_corrupted)
                    {
                        _corrupted = true;

                        var corruptedData =
                            (byte[])chunk.Data.Clone();

                        corruptedData[0] ^= 0xFF;

                        chunkToSend = new FileChunk
                        {
                            FileId = chunk.FileId,
                            FileName = chunk.FileName,
                            ChunkIndex = chunk.ChunkIndex,
                            TotalChunks = chunk.TotalChunks,
                            FileSize = chunk.FileSize,
                            Data = corruptedData,
                            ChunkChecksum = chunk.ChunkChecksum,
                            FileChecksum = chunk.FileChecksum
                        };

                        Console.WriteLine(
                            $"SIMULATION: Corrupting chunk {chunk.ChunkIndex}");
                    }
                    await _chunkTransport.SendChunk(chunkToSend, cancellationToken);
                    hasChunks = true;
                }


                return hasChunks == true;
            }
            catch (Exception ex)
            {

                Console.WriteLine($"Error sending file: {ex.Message}");
                return false;
            }
        }

        public async Task ResendChunk(string filePath,
            FailedChunk failedChunk,
            CancellationToken cancellationToken = default)
        {
            await foreach (var chunk in _fileChunker.GetFileChunks(
                filePath,
                failedChunk.FileId,
                cancellationToken))
            {
                if (chunk.ChunkIndex != failedChunk.ChunkIndex)
                {
                    continue;
                }

                await _chunkTransport.SendChunk(
                    chunk,
                    cancellationToken);

                Console.WriteLine(
                    $"Resent chunk {chunk.ChunkIndex} of {chunk.FileName}");

                break;
            }
        }
    }
}
