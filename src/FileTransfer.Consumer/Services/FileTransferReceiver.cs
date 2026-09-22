using FileTransfer.Consumer.Configuration;
using FileTransfer.Consumer.Interfaces;
using FileTransfer.Contracts;
using FileTransfer.Infrastructure.Helpers;
using FileTransfer.Infrastructure.Storage;
using FileTransfer.Infrastructure.Transport;
using Microsoft.Extensions.Options;
using static System.Net.WebRequestMethods;

namespace FileTransfer.Consumer.Services
{
    public class FileTransferReceiver : IFileTransferReceiver
    {
        private readonly IChunkTransport _transport;
        private readonly IFileChunkStorage _storage;
        private readonly IFileChunkAssembler _reassembler;
        private readonly IFailedChunkStorage _failedChunkStorage;

        public FileTransferReceiver(IChunkTransport transport, IFileChunkStorage storage, IFileChunkAssembler reassembler, IFailedChunkStorage failedChunkStorage)
        {
            _transport = transport;
            _storage = storage;
            _reassembler = reassembler;
            _failedChunkStorage = failedChunkStorage;
        }
        public async Task ReceiveFileChunks(string destinationPath, CancellationToken cancellationToken = default)
        {
            await foreach (var chunk in _transport.ReceiveChunk(cancellationToken))
            {
                Console.WriteLine($"[{chunk.FileName}]: Position: {chunk.ChunkIndex*chunk.Data.Length} Checksum: {chunk.ChunkChecksum}");

                try
                {
                    ValidateChecksum(chunk);
                    ValidateChunk(chunk);
                }
                catch (InvalidDataException ex)
                {
                    Console.WriteLine(ex.Message);
                    Console.WriteLine($"Failed chunk received: FileId: {chunk.FileId}, FileName: {chunk.FileName}, ChunkIndex: {chunk.ChunkIndex}");
                    await _failedChunkStorage.StoreAsync(
                        new FailedChunk
                        {
                            FileId = chunk.FileId,
                            FileName = chunk.FileName,
                            ChunkIndex = chunk.ChunkIndex
                        },
                        cancellationToken);

                    continue;
                }

                if (await _storage.ExistsAsync(
                        chunk.FileId,
                        chunk.ChunkIndex,
                        cancellationToken))
                {
                    continue;
                }

                await _storage.StoreAsync(chunk, cancellationToken);

                if (await HasAllChunksAsync(chunk, cancellationToken))
                {
                    var outputPath = Path.Combine(destinationPath, chunk.FileName);

                    await _reassembler.ReassembleAsync(chunk.FileId, chunk.TotalChunks, outputPath, cancellationToken);

                    var actualChecksum = await ChecksumHelper.ComputeSha256(outputPath, cancellationToken);
                    var result = chunk.FileChecksum == actualChecksum ? "passed" : "failed";

                    Console.WriteLine($"File checksum check {result}: \nExpected:{chunk.FileChecksum} \nActual: {actualChecksum}");
                }
            }
        }

        private async Task<bool> HasAllChunksAsync(FileChunk chunk, CancellationToken cancellationToken)
        {
            for (var index = 0; index < chunk.TotalChunks; index++)
            {
                if (!await _storage.ExistsAsync(chunk.FileId, index, cancellationToken))
                {
                    return false;
                }
            }

            return true;
        }
            
        private static void ValidateChunk(FileChunk chunk)
        {
            if (chunk.ChunkIndex < 0)
            {
                throw new InvalidOperationException(
                    "Chunk index cannot be negative.");
            }

            if (chunk.TotalChunks <= 0)
            {
                throw new InvalidOperationException(
                    "Total chunks must be greater than zero.");
            }

            if (chunk.ChunkIndex >= chunk.TotalChunks)
            {
                throw new InvalidOperationException(
                    "Chunk index is outside the expected range.");
            }
        }

        private static void ValidateChecksum(FileChunk chunk)
        {
            var calculatedChecksum = ChecksumHelper.ComputeMd5(chunk.Data);

            if (!string.Equals(
                    calculatedChecksum,
                    chunk.ChunkChecksum,
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidDataException($"Checksum validation failed for file {chunk.FileName} with FileId {chunk.FileId}, chunk {chunk.ChunkIndex}.");
            }
        }
    }
}
