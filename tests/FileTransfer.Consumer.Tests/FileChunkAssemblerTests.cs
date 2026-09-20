using FileTransfer.Consumer.Services;
using FileTransfer.Contracts;
using FileTransfer.Infrastructure.Storage;
using System.Text;
using FluentAssertions;
using Moq;

namespace FileTransfer.Consumer.Tests
{
    public class FileChunkAssemblerTests
    {
        [Fact]
        public async Task Should_ReassembleFileChunks()
        {
            // Arrange
            var fileId = Guid.NewGuid();

            var chunks = new Dictionary<int, FileChunk>
            {
                [0] = CreateChunk(fileId, 0, "Hello "),
                [1] = CreateChunk(fileId, 1, "World "),
                [2] = CreateChunk(fileId, 2, "from "),
                [3] = CreateChunk(fileId, 3, "chunks")
            };

            var storageMock = new Mock<IFileChunkStorage>();

            storageMock
                .Setup(x => x.GetAsync(
                    fileId,
                    It.IsAny<int>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((
                    Guid _,
                    int chunkIndex,
                    CancellationToken _) => chunks[chunkIndex]);

            var sut = new FileChunkAssembler(storageMock.Object);

            var outputPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.txt");

            // Act
            await sut.ReassembleAsync(fileId, totalChunks: 4, outputPath);

            // Assert
            var result = await File.ReadAllTextAsync(outputPath);

            Assert.Equal("Hello World from chunks", result);
        }

        private static FileChunk CreateChunk(Guid fileId, int chunkIndex, string content)
        {
            var data = Encoding.UTF8.GetBytes(content);

            return new FileChunk()
            {
                FileId = fileId,
                ChunkIndex = chunkIndex,
                TotalChunks = 4,
                FileSize = data.Length,
                Data = data,
                Checksum = "checksum"
            };
        }
    }

}
