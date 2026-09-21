using FileTransfer.Consumer.Configuration;
using FileTransfer.Consumer.Interfaces;
using FileTransfer.Consumer.Services;
using FileTransfer.Contracts;
using FileTransfer.Infrastructure.Helpers;
using FileTransfer.Infrastructure.Storage;
using FileTransfer.Infrastructure.Transport;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Moq;
using System.Text;

namespace FileTransfer.Consumer.Tests
{
    public class FileTransferReceiverTests
    {
        private readonly Mock<IChunkTransport> _transportMock;
        private readonly Mock<IFileChunkStorage> _storageMock;
        private readonly Mock<IFileChunkAssembler> _reassemblerMock;
        private readonly Mock<IOptions<FileTransferConsumerOptions>> _optionsMock;
        private readonly FileTransferReceiver _receiver;
        public FileTransferReceiverTests()
        {
            _transportMock = new Mock<IChunkTransport>();
            _storageMock = new Mock<IFileChunkStorage>();
            _reassemblerMock = new Mock<IFileChunkAssembler>();
            _optionsMock = new Mock<IOptions<FileTransferConsumerOptions>>();

            _optionsMock
                .Setup(x => x.Value)
                .Returns(new FileTransferConsumerOptions
                {
                    OutputDirectory = "output"
                });

            _receiver = new FileTransferReceiver(
                _transportMock.Object,
                _storageMock.Object,
                _reassemblerMock.Object,
                _optionsMock.Object);
        }

        [Fact]
        public async Task ReceiveAsync_ShouldStoreReceivedChunk()
        {
            // Arrange
            var fileId = Guid.NewGuid();

            var chunk = new FileChunk()
            {
                FileId = fileId,
                ChunkIndex = 0,
                TotalChunks = 2,
                FileSize = 10,
                Data = new byte[] { 1, 2, 3, 4, 5 },
                Checksum =  ChecksumHelper.ComputeSha256(new byte[] { 1, 2, 3, 4, 5 })
            };

            _transportMock
                .Setup(x => x.ReceiveChunk(It.IsAny<CancellationToken>()))
                .Returns(ToAsyncEnumerable(chunk));

            // Act
            await _receiver.ReceiveFileChunks();

            // Assert
            _storageMock.Verify(x => x.StoreAsync(chunk,It.IsAny<CancellationToken>()),Times.Once);
        }

        [Fact]
        public async Task ReceiveFileChunks_InvalidChecksum_ThrowsInvalidDataException()
        {
            var chunk = new FileChunk
            {
                FileId = Guid.NewGuid(),
                ChunkIndex = 0,
                TotalChunks = 1,
                FileSize = 5,
                Data = "Hello"u8.ToArray(),
                Checksum = "invalid-checksum"
            };

            _transportMock.Setup(x => x.ReceiveChunk(It.IsAny<CancellationToken>())).Returns(ToAsyncEnumerable(chunk));

            var receiver = new FileTransferReceiver(
                _transportMock.Object,
                _storageMock.Object,
                _reassemblerMock.Object,
                _optionsMock.Object);

            Func<Task> act = () => receiver.ReceiveFileChunks();
            await act.Should().ThrowAsync<InvalidDataException>();   

        }

        private static async IAsyncEnumerable<FileChunk> ToAsyncEnumerable(params FileChunk[] chunks)
        {
            foreach (var chunk in chunks)
            {
                yield return chunk;
                await Task.CompletedTask;
            }
        }

    }
}