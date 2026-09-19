using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace FileTransfer.Infrastructure.Tests
{
    public class InMemoryChunkTransportTests
    {
        [Fact]
        public async Task ShouldSendAndReceiveChunkedMessage()
        {
            var transport = new InMemoryChunkTransport();
            await transport.SendChunk(new Contracts.FileChunk { ChunkId = 1, Data = new byte[] { 1, 2, 3 }, Size = 3 }, CancellationToken.None);

            await using var enumerator = transport.ReceiveChunk(CancellationToken.None).GetAsyncEnumerator(CancellationToken.None);

            var moved = await enumerator.MoveNextAsync();
            var receivedChunk = moved ? enumerator.Current : null;
        
            Assert.True(moved);
            Assert.NotNull(receivedChunk);
            Assert.Equal(1, receivedChunk.ChunkId);
            Assert.Equal(3, receivedChunk.Size);
        }
    }
}
