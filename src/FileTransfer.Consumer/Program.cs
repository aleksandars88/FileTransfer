using FileTransfer.Consumer.Configuration;
using FileTransfer.Consumer.Interfaces;
using FileTransfer.Consumer.Services;
using FileTransfer.Infrastructure;
using FileTransfer.Infrastructure.Storage;
using FileTransfer.Infrastructure.Transport;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

internal class Program
{
    private async static Task Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);

        builder.Services.Configure<FileTransferConsumerOptions>(builder.Configuration.GetSection("FileTransfer"));
        builder.Services.Configure<RabbitMqOptions>(builder.Configuration.GetSection("RabbitMq"));
        builder.Services.AddSingleton(sp => sp.GetRequiredService<IOptions<RabbitMqOptions>>().Value);
        builder.Services.AddSingleton(sp => sp.GetRequiredService<IOptions<FileTransferConsumerOptions>>().Value);

        builder.Services.AddSingleton<IFileChunkStorage>(sp => {
            var options = sp.GetRequiredService<IOptions<FileTransferConsumerOptions>>().Value;
            return new FileChunkStorage(options.OutputDirectory);
        });

        builder.Services.AddSingleton<IFailedChunkStorage>(sp => {
            var options = sp.GetRequiredService<IOptions<FileTransferConsumerOptions>>().Value;
            return new FailedChunkStorage(options.FailedChunksDirectory );
        });

        builder.Services.AddSingleton<IFileChunkAssembler, FileChunkAssembler>();

        builder.Services.AddSingleton<IFileTransferReceiver, FileTransferReceiver>();

        builder.Services.AddSingleton<IChunkTransport, RabbitMqChunkTransport>();

        var host = builder.Build();

        var receiver = host.Services.GetRequiredService<IFileTransferReceiver>();

        var destinationPath = host.Services.GetRequiredService<IOptions<FileTransferConsumerOptions>>().Value.OutputDirectory;

        Console.WriteLine($"Enter destination directory absolute path [Default: {destinationPath}]:");
        var path = Console.ReadLine();

        if (!string.IsNullOrEmpty(path))
        {
            destinationPath = path;
        }

        Console.WriteLine($"Waiting files to be received on the following location {destinationPath}");    
        await receiver.ReceiveFileChunks(destinationPath);

    }
}
