using FileTransfer.Contracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace FileTransfer.Infrastructure.Storage
{
    public interface IFileChunkStorage
    {
        Task StoreAsync(
            FileChunk chunk,
            CancellationToken cancellationToken = default);

        Task<FileChunk> GetAsync(
            Guid fileId,
            int chunkIndex,
            CancellationToken cancellationToken = default);

        Task<bool> ExistsAsync(
            Guid fileId,
            int chunkIndex,
            CancellationToken cancellationToken = default);
    }
}
