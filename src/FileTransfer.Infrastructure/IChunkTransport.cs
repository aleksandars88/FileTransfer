using FileTransfer.Contracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace FileTransfer.Infrastructure
{
    public interface IChunkTransport
    {
        ValueTask SendChunk(FileChunk chumk, CancellationToken token);
        IAsyncEnumerable<FileChunk> ReceiveChunk(CancellationToken token);
    }
}
