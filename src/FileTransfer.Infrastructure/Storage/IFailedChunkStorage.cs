using FileTransfer.Contracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace FileTransfer.Infrastructure.Storage
{
    public interface IFailedChunkStorage
    {
        Task StoreAsync(FailedChunk failedChunk, CancellationToken cancellationToken = default);

        IEnumerable<FailedChunk> GetFailedChunks();

        void Delete(FailedChunk chunk);
    }
}
