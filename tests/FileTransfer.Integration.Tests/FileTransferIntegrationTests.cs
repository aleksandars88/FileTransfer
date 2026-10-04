using FileTransfer.Infrastructure.Storage;
using FileTransfer.Infrastructure.Transport;
using FileTransfer.Producer.Configuration;
using FileTransfer.Producer.Services;
using FileTransfer.Consumer.Services;
using FluentAssertions;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;

namespace FileTransfer.Integration.Tests;

public class FileTransferIntegrationTests
{
    private readonly string _sourceDirectory;
    private readonly string _destinationDirectory;
    private readonly string _failedChunksDirectory;

    public FileTransferIntegrationTests()
    {
        var projectDirectory = Directory.GetCurrentDirectory();

        _sourceDirectory = Path.Combine(projectDirectory,"Source");

        _destinationDirectory = Path.Combine(projectDirectory,"Destination");

        _failedChunksDirectory = Path.Combine(projectDirectory,"FailedChunks");

    }

    [Fact]
    public async Task SendFile_ShouldSendAndReceiveChunkedFile()
    {
        // Arrange

        Directory.Delete(_destinationDirectory, true);
        Directory.Delete(_failedChunksDirectory, true);

        Directory.CreateDirectory(_destinationDirectory);
        Directory.CreateDirectory(_failedChunksDirectory);

        var sourceFilePath = Path.Combine(_sourceDirectory,"sample-10mb.jpg");

        var destinationFilePath = Path.Combine(_destinationDirectory, "sample-10mb.jpg");

        if (!File.Exists(sourceFilePath))
        {
            throw new FileNotFoundException($"Test file was not found: {sourceFilePath}");
        }

        if (File.Exists(destinationFilePath))
        {
            File.Delete(destinationFilePath);
        }

        var transport = new InMemoryChunkTransport();

        var storage = new FileChunkStorage(_destinationDirectory);

        var assembler = new FileChunkAssembler(storage);

        var producerOptions = Options.Create(
            new FileTransferOptions
            {
                ChunkSize = 1024 * 1024
            });

        var fileChunker = new FileChunkerService(producerOptions);

        var failedStorage = new FailedChunkStorage(_failedChunksDirectory);

        var sender = new FileTransferSenderService(transport, fileChunker);

        var receiver = new FileTransferReceiver(transport, storage, assembler, failedStorage);

        using var cancellationTokenSource = new CancellationTokenSource();

        // Act

        var receiverTask = receiver.ReceiveFileChunks(_destinationDirectory, cancellationTokenSource.Token);

        await sender.SendFile(sourceFilePath, cancellationTokenSource.Token);

        // Give receiver time to finish processing
        await Task.Delay(1000);

        var failedChunks = failedStorage.GetFailedChunks().ToList();

        foreach (var failedChunk in failedChunks)
        {
            await sender.ResendChunk(sourceFilePath, failedChunk, cancellationTokenSource.Token);

            await Task.Delay(1000);

            failedStorage.Delete(failedChunk);
        }

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

        File.Exists(destinationFilePath).Should().BeTrue();

        var sourceInfo = new FileInfo(sourceFilePath);

        var destinationInfo = new FileInfo(destinationFilePath);

        destinationInfo.Length.Should().Be(sourceInfo.Length);

        var sourceHash = await CalculateSha256Async(sourceFilePath);

        var destinationHash = await CalculateSha256Async(destinationFilePath);

        destinationHash.Should().Be(sourceHash);
    }

    private static async Task<string> CalculateSha256Async(
        string filePath)
    {
        await using var stream = File.OpenRead(filePath);

        var hash = await SHA256.HashDataAsync(stream);

        return Convert.ToHexString(hash);
    }
}