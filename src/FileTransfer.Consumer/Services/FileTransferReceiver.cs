using FileTransfer.Consumer.Configuration;
using FileTransfer.Consumer.Interfaces;
using FileTransfer.Contracts;
using FileTransfer.Infrastructure.Helpers;
using FileTransfer.Infrastructure.Storage;
using FileTransfer.Infrastructure.Transport;
using Microsoft.Extensions.Options;

namespace FileTransfer.Consumer.Services
{
    public class FileTransferReceiver : IFileTransferReceiver
    {
        private readonly IChunkTransport _transport;
        private readonly IFileChunkStorage _storage;
        private readonly IFileChunkAssembler _reassembler;
        private readonly IOptions<FileTransferConsumerOptions> _options;

        public FileTransferReceiver(IChunkTransport transport, IFileChunkStorage storage, IFileChunkAssembler reassembler, IOptions<FileTransferConsumerOptions> options)
        {
            _transport = transport;
            _storage = storage;
            _reassembler = reassembler;
            _options = options;
        }
        public async Task ReceiveFileChunks(CancellationToken cancellationToken = default)
        {
            await foreach (var chunk in _transport.ReceiveChunk(cancellationToken))
            {
                Console.WriteLine($"[{chunk.FileName}]: Received chunk index: {chunk.ChunkIndex} of total chunks {chunk.TotalChunks}");

                ValidateChecksum(chunk);
                ValidateChunk(chunk);

                if (await _storage.ExistsAsync(chunk.FileId, chunk.ChunkIndex, cancellationToken))
                {
                    continue;
                }

                await _storage.StoreAsync(chunk, cancellationToken);

                if (await HasAllChunksAsync(chunk, cancellationToken))
                {
                    var outputPath = Path.Combine(_options.Value.OutputDirectory, chunk.FileName);

                    await _reassembler.ReassembleAsync(chunk.FileId, chunk.TotalChunks, outputPath, cancellationToken);
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
            var calculatedChecksum = ChecksumHelper.ComputeSha256(chunk.Data);

            if (!string.Equals(
                    calculatedChecksum,
                    chunk.Checksum,
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidDataException($"Checksum validation failed for file {chunk.FileName} with FileId {chunk.FileId}, chunk {chunk.ChunkIndex}.");
            }
        }
    }
}
