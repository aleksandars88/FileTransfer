using FileTransfer.Infrastructure.Storage;

namespace FileTransfer.Infrastructure.Tests
{
    public class FileChunkStorageTests
    {
        [Fact]
        public async Task Should_StoreAndRetrieveFileChunk()
        {
            var storage = new FileChunkStorage("./test-chunks");
            var chunk = new Contracts.FileChunk { FileId = Guid.NewGuid(), FileSize = 1024, TotalChunks = 4, ChunkIndex = 0, ChunkChecksum = "test-checksum", Data = new byte[] { 1, 2, 3 } };

            await storage.StoreAsync(chunk);

            var retrievedChunk = await storage.GetAsync(chunk.FileId, chunk.ChunkIndex);

            Assert.NotNull(retrievedChunk);
            Assert.Equal(chunk.FileId, retrievedChunk.FileId);
            Assert.Equal(chunk.Data, retrievedChunk.Data);
        }

        [Fact]
        public async Task Should_CheckIfFileChunkExists()
        {
            var storage = new FileChunkStorage("./test-chunks");
            var chunk = new Contracts.FileChunk { FileId = Guid.NewGuid(), FileSize = 1024, TotalChunks = 4, ChunkIndex = 0, ChunkChecksum = "test-checksum", Data = new byte[] { 1, 2, 3 } };
            await storage.StoreAsync(chunk);
            var exists = await storage.ExistsAsync(chunk.FileId, chunk.ChunkIndex);
            Assert.True(exists);
        }
    }
}