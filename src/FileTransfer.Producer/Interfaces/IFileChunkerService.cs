using FileTransfer.Contracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace FileTransfer.Producer.Interfaces
{
    public interface IFileChunkerService
    {
        IAsyncEnumerable<FileChunk> GetFileChunks(string filePath, CancellationToken cancellationToken = default);
        IAsyncEnumerable<FileChunk> GetFileChunks(string filePath, Guid fileId, CancellationToken cancellationToken = default);
    }
}
