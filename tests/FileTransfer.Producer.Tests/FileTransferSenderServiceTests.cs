using FileTransfer.Contracts;
using FileTransfer.Infrastructure.Transport;
using FileTransfer.Producer.Interfaces;
using FileTransfer.Producer.Services;
using FluentAssertions;
using Moq;

namespace FileTransfer.Producer.Tests
{
    public class FileTransferSenderServiceTests
    {

        [Fact]
        public async Task Should_SendFile_With_Multiple_Chunks_Successfully()
        {
            Mock<IFileChunkerService> _fileChunkerMock = new Mock<IFileChunkerService>();
            Mock<IChunkTransport> _chunkTransportMock = new Mock<IChunkTransport>();

            var chunks = new[]
{
                new FileChunk()
                {
                    Data = new byte[] { 7, 8, 9 }
                }
            };

            _fileChunkerMock
                .Setup(x => x.GetFileChunks(
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
                .Returns(chunks.ToAsyncEnumerable());

            _chunkTransportMock
                .Setup(x => x.SendChunk(
                    It.IsAny<FileChunk>(),
                    It.IsAny<CancellationToken>()))
                .Returns(new ValueTask());

            var fileTransferSenderService = new FileTransferSenderService(_chunkTransportMock.Object, _fileChunkerMock.Object);
            var success = await fileTransferSenderService.SendFile("Data/sample-10mb.jpg");
            success.Should().BeTrue();
        }

        [Fact]
        public async Task Should_Indicate_Unsuccessfull_SendFile_When_Zero_Chunks()
        {
            Mock<IFileChunkerService> _fileChunkerMock = new Mock<IFileChunkerService>();
            Mock<IChunkTransport> _chunkTransportMock = new Mock<IChunkTransport>();

            var chunks = new FileChunk[0];

            _fileChunkerMock
                .Setup(x => x.GetFileChunks(
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
                .Returns(chunks.ToAsyncEnumerable());

            _chunkTransportMock
                .Setup(x => x.SendChunk(
                    It.IsAny<FileChunk>(),
                    It.IsAny<CancellationToken>()))
                .Returns(new ValueTask());

            var fileTransferSenderService = new FileTransferSenderService(_chunkTransportMock.Object, _fileChunkerMock.Object);
            var success = await fileTransferSenderService.SendFile("Data/non-existent-file.jpg");
            success.Should().BeFalse();
        }

        [Fact]
        public async Task Should_Indicate_Unsuccessful_SendFile_When_Invalid_FilePath_Argument_Provided()
        {
            Mock<IFileChunkerService> _fileChunkerMock = new Mock<IFileChunkerService>();
            Mock<IChunkTransport> _chunkTransportMock = new Mock<IChunkTransport>();

            var chunks = new[]
{
                new FileChunk()
                {
                    Data = new byte[] { 7, 8, 9 }
                }
            };

            _fileChunkerMock
                .Setup(x => x.GetFileChunks(
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
                .Returns(chunks.ToAsyncEnumerable());

            _chunkTransportMock
                .Setup(x => x.SendChunk(
                    It.IsAny<FileChunk>(),
                    It.IsAny<CancellationToken>()))
                .Returns(new ValueTask());

            var fileTransferSenderService = new FileTransferSenderService(_chunkTransportMock.Object, _fileChunkerMock.Object);
            var success = await fileTransferSenderService.SendFile("");
            success.Should().BeFalse();
        }

        [Fact]
        public async Task Should_Indicate_Unsuccessful_SendFile_When_File_Not_Found()
        {
            Mock<IFileChunkerService> _fileChunkerMock = new Mock<IFileChunkerService>();
            Mock<IChunkTransport> _chunkTransportMock = new Mock<IChunkTransport>();

            var chunks = new FileChunk[0];

            _fileChunkerMock
                .Setup(x => x.GetFileChunks(
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
                .Returns(chunks.ToAsyncEnumerable());

            _chunkTransportMock
                .Setup(x => x.SendChunk(
                    It.IsAny<FileChunk>(),
                    It.IsAny<CancellationToken>()))
                .Returns(new ValueTask());

            var fileTransferSenderService = new FileTransferSenderService(_chunkTransportMock.Object, _fileChunkerMock.Object);
            var success = await fileTransferSenderService.SendFile("Data/non-existent-file.jpg");
            success.Should().BeFalse();
        }
    }
}
