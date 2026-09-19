using FileTransfer.Producer.Services;
using FluentAssertions;

namespace FileTransfer.Producer.Tests
{
    public class FileTransferSenderServiceTests
    {
        [Fact]
        public async Task Should_SendFile_Successfully()
        {
            var fileInfo = new FileInfo("Data/sample-10mb.jpg");
            fileInfo.Exists.Should().BeTrue("The test file should exist for the test to run.");

            var fileTransferSenderService = new FileTransferSenderService();
            var success = await fileTransferSenderService.SendFile("Data/sample-10mb.jpg");
            success.Should().BeTrue();
        }

        [Fact]
        public async Task Should_Throw_Exception_When_File_Not_Found()
        {
            string fileName = "Data/non-existent-file.jpg";
            var fileInfo = new FileInfo(fileName);
            fileInfo.Exists.Should().BeFalse("The test file should not exist for the test to run.");

            var fileTransferSenderService = new FileTransferSenderService();
            await Assert.ThrowsAsync<FileNotFoundException>(() => fileTransferSenderService.SendFile(fileName));
        }

        [Fact]
        public async Task Should_Throw_Exception_When_Invalid_FilePath_Argument_Provided()
        {
            var fileTransferSenderService = new FileTransferSenderService();
            await Assert.ThrowsAsync<ArgumentException>(() => fileTransferSenderService.SendFile(""));
        }
    }
}
