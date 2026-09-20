using FileTransfer.Producer.Services;
using FileTransfer.Producer.Tests.Fakes;
using FluentAssertions;

namespace FileTransfer.Producer.Tests
{
    public class FileChunkerServiceTests
    {
        [Fact]
        public async Task Should_GetFileChunks()
        {
            var fileChunkerService = new FileChunkerService(new FakeTransferConfiguration());

            var fileInfo = new FileInfo("Data/sample-10mb.jpg");
            fileInfo.Exists.Should().BeTrue("The test file should exist for the test to run.");

            var fileChunks = await fileChunkerService.GetFileChunks("Data/sample-10mb.jpg").ToListAsync();
            fileChunks.Should().NotBeNull();
            fileChunks.Should().NotBeEmpty();
        }
    }
}
