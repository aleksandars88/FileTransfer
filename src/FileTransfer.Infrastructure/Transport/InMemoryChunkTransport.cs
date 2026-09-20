using FileTransfer.Contracts;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Channels;

namespace FileTransfer.Infrastructure.Transport
{
    public class InMemoryChunkTransport : IChunkTransport
    {
        private readonly Channel<FileChunk> _channel;

        public InMemoryChunkTransport()
        {
            _channel = Channel.CreateUnbounded<FileChunk>();
        }
        public IAsyncEnumerable<FileChunk> ReceiveChunk(CancellationToken token)
        {
            return _channel.Reader.ReadAllAsync(token);
        }

        public ValueTask SendChunk(FileChunk chumk, CancellationToken token)
        {
            return _channel.Writer.WriteAsync(chumk, token);
        }
    }
}
