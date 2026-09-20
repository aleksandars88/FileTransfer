using FileTransfer.Consumer.Configuration;
using FileTransfer.Consumer.Services;
using FileTransfer.Contracts;
using FileTransfer.Infrastructure.Storage;
using FileTransfer.Infrastructure.Transport;
using FileTransfer.Consumer;
using FileTransfer.Producer;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using FileTransfer.Producer.Services;
using FileTransfer.Producer.Configuration;
using FluentAssertions;

namespace FileTransfer.Integration.Tests;

public class FileTransferIntegrationTests
{
    private readonly string _sourceDirectory;
    private readonly string _destinationDirectory;

    public FileTransferIntegrationTests()
    {
        var projectDirectory = Directory.GetCurrentDirectory();

        _sourceDirectory = Path.Combine(
            projectDirectory,
            "Source");

        _destinationDirectory = Path.Combine(
            projectDirectory,
            "Destination");

        Directory.CreateDirectory(_sourceDirectory);
        Directory.CreateDirectory(_destinationDirectory);
    }

    [Fact]
    public async Task SendFile_ShouldSendAndReceiveChunkedFile()
    {
        // Arrange

        var sourceFilePath = Path.Combine(_sourceDirectory, "sample-10mb.jpg");

        var destinationFilePath = Path.Combine(_destinationDirectory,"sample-10mb.jpg");

        if (!File.Exists(sourceFilePath))
        {
            throw new FileNotFoundException($"Test file was not found: {sourceFilePath}");
        }

        if (File.Exists(destinationFilePath))
        {
            File.Delete(destinationFilePath);
        }

        var transport = new InMemoryChunkTransport();

        var storage = new FileChunkStorage(
            _destinationDirectory);

        var assembler = new FileChunkAssembler(storage);

        var consumerOptions = Options.Create(
            new FileTransferConsumerOptions
            {
                OutputDirectory = _destinationDirectory
            });

        var producerOptions = Options.Create(new FileTransferOptions{
           ChunkSize = 1024 * 1024 
        });

        var fileChunker = new FileChunkerService(producerOptions);

        var sender = new FileTransferSenderService(transport, fileChunker);

        var receiver = new FileTransferReceiver(transport, storage, assembler, consumerOptions);

        using var cancellationTokenSource = new CancellationTokenSource();

        // Act

        var receiverTask = receiver.ReceiveFileChunks(cancellationTokenSource.Token);

        await sender.SendFile(sourceFilePath, cancellationTokenSource.Token);

        // Give receiver time to finish processing
        await Task.Delay(500);

        cancellationTokenSource.Cancel();

        try
        {
            await receiverTask;
        }
        catch (OperationCanceledException)
        {
            // Expected when receiver is cancelled.
        }

        // Assert
        var fileExist = File.Exists(destinationFilePath);
        fileExist.Should().BeTrue(); 

        var sourceInfo = new FileInfo(sourceFilePath);
        var destinationInfo = new FileInfo(destinationFilePath);

        sourceInfo.Length.Should().Be(destinationInfo.Length);

        var sourceHash = await CalculateSha256Async(sourceFilePath);

        var destinationHash = await CalculateSha256Async(destinationFilePath);

        sourceHash.Should().Be(destinationHash);
    }

    private static async Task<string> CalculateSha256Async(
        string filePath)
    {
        await using var stream = File.OpenRead(filePath);

        var hash = await SHA256.HashDataAsync(stream);

        return Convert.ToHexString(hash);
    }
}