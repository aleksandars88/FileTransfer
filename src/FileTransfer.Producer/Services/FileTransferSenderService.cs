using FileTransfer.Infrastructure;
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
                    await _chunkTransport.SendChunk(chunk, cancellationToken);
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
        }
    }
