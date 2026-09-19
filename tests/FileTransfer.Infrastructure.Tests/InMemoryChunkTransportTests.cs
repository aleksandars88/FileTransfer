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
            await transport.SendChunk(new Contracts.FileChunk { FileId = "test-file", FileSize = 1024, TotalChunks = 4, ChunkIndex = 0, Checksum = "test-checksum", Data = new byte[] { 1, 2, 3 } }, CancellationToken.None);

            await using var enumerator = transport.ReceiveChunk(CancellationToken.None).GetAsyncEnumerator(CancellationToken.None);

            var moved = await enumerator.MoveNextAsync();
            var receivedChunk = moved ? enumerator.Current : null;
        
            Assert.True(moved);
            Assert.NotNull(receivedChunk);
            Assert.Equal("test-file", receivedChunk.FileId);
            Assert.Equal(1024, receivedChunk.FileSize);
            Assert.Equal(4, receivedChunk.TotalChunks);
            Assert.Equal(0, receivedChunk.ChunkIndex);
            Assert.Equal("test-checksum", receivedChunk.Checksum);
            Assert.Equal(new byte[] { 1, 2, 3 }, receivedChunk.Data);
        }
    }
}
