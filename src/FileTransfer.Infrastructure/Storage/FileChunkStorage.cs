using FileTransfer.Contracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace FileTransfer.Infrastructure.Storage
{
    public class FileChunkStorage : IFileChunkStorage
    {
        public Task<bool> ExistsAsync(Guid fileId, int chunkIndex, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<FileChunk> GetAsync(Guid fileId, int chunkIndex, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task StoreAsync(FileChunk chunk, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
