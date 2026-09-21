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
        var sourceDirectory = host.Services.GetRequiredService<IOptions<FileTransferOptions>>().Value.SourceDirectory;

        var sourcePath = Path.Combine(Directory.GetCurrentDirectory(), sourceDirectory);

        if(!Directory.Exists(sourcePath))
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

        watcher.Created += (sender, e) =>
        {
            Console.WriteLine($"New file detected: {e.Name}");  
            fileTransferSender.SendFile(e.FullPath);
        };

        Console.WriteLine("Watching folder...");

        await Task.Delay(Timeout.Infinite);        
    }
}