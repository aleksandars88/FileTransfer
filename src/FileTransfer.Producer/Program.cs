using FileTransfer.Infrastructure;
using FileTransfer.Infrastructure.Transport;
using FileTransfer.Producer.Configuration;
using FileTransfer.Producer.Interfaces;
using FileTransfer.Producer.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

internal class Program
{
    private static async Task Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);
        builder.Services.Configure<FileTransferOptions>(builder.Configuration.GetSection("FileTransferOptions"));
        builder.Services.Configure<RabbitMqOptions>(builder.Configuration.GetSection("RabbitMq"));

        builder.Services.AddSingleton(sp => sp.GetRequiredService<IOptions<FileTransferOptions>>().Value);
        builder.Services.AddSingleton(sp => sp.GetRequiredService<IOptions<RabbitMqOptions>>().Value);

        builder.Services.AddSingleton<IChunkTransport, RabbitMqChunkTransport>();
        builder.Services.AddSingleton<IFileChunkerService, FileChunkerService>();
        builder.Services.AddSingleton<IFileTransferSenderService, FileTransferSenderService>();
        using var host = builder.Build();

        var fileTransferSender = host.Services.GetRequiredService<IFileTransferSenderService>();
        var sourcePath = host.Services.GetRequiredService<IOptions<FileTransferOptions>>().Value.SourceDirectory;

        Console.WriteLine($"Enter source directory absolute path [Default: {sourcePath}]:");
        var path = Console.ReadLine();

        if (!string.IsNullOrEmpty(path))
        {
            sourcePath = path;
        }

        if (!Directory.Exists(sourcePath))
        {
            Directory.CreateDirectory(sourcePath);
        }

        Console.WriteLine($"Watching folder: {sourcePath}");

        var watcher = new FileSystemWatcher
        {
            Path = sourcePath,
            Filter = "*.*",
            EnableRaisingEvents = true
        };

        watcher.Created += async (sender, e) =>
        {
            Console.WriteLine($"New file detected: {e.Name}");

            try
            {
                await WaitForFileReadyAsync(e.FullPath);

                Console.WriteLine($"File is ready: {e.Name}");

                await fileTransferSender.SendFile(e.FullPath);

                Console.WriteLine($"File sent successfully: {e.Name}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending file: {ex.Message}");
            }
        };

        await Task.Delay(Timeout.Infinite);        
    }

    private static async Task WaitForFileReadyAsync(
    string filePath,
    CancellationToken cancellationToken = default)
    {
        long previousSize = -1;

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var fileInfo = new FileInfo(filePath);

                if (!fileInfo.Exists)
                {
                    await Task.Delay(500, cancellationToken);
                    continue;
                }

                var currentSize = fileInfo.Length;

                if (currentSize == previousSize)
                {
                    using var stream = new FileStream(
                        filePath,
                        FileMode.Open,
                        FileAccess.Read,
                        FileShare.Read);

                    return;
                }

                previousSize = currentSize;
            }
            catch (IOException)
            {
                // File is still being copied/written.
            }

            await Task.Delay(500, cancellationToken);
        }

        cancellationToken.ThrowIfCancellationRequested();
    }
}