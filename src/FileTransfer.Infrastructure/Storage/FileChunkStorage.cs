using FileTransfer.Contracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace FileTransfer.Infrastructure.Storage
{
    public class FileChunkStorage : IFileChunkStorage
    {
        private string _storageDirectory;

        public FileChunkStorage(string storageDirectory)
        {
            _storageDirectory = storageDirectory;
        }

        public Task<bool> ExistsAsync(Guid fileId, int chunkIndex, CancellationToken cancellationToken = default)
        {
        var path = GetChunkPath(fileId, chunkIndex);

        return Task.FromResult(File.Exists(path));
        }

        public async Task<FileChunk> GetAsync(Guid fileId, int chunkIndex, CancellationToken cancellationToken = default)
        {
            var path = GetChunkPath(fileId, chunkIndex);

            var data = await File.ReadAllBytesAsync(path, cancellationToken);

            return new FileChunk() {
                FileId = fileId,
                ChunkIndex = chunkIndex,
                TotalChunks = 0,
                FileSize = 0,
                Data = data,
                Checksum = string.Empty
            };
        }

        public async Task StoreAsync(FileChunk chunk, CancellationToken cancellationToken = default)
        {
            var directory = Path.Combine(_storageDirectory, chunk.FileId.ToString());

            Directory.CreateDirectory(directory);

            var path = Path.Combine(directory, $"{chunk.ChunkIndex}.chunk");

            await File.WriteAllBytesAsync(path, chunk.Data, cancellationToken);
        }

        private string GetChunkPath(Guid fileId, int chunkIndex)
        {
            return Path.Combine(_storageDirectory, fileId.ToString(), $"{chunkIndex}.chunk");
        }
    }
}
