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

        builder.Services.AddSingleton<IFileChunkStorage>(sp => {
            var options = sp.GetRequiredService<IOptions<FileTransferConsumerOptions>>().Value;
            return new FileChunkStorage(options.OutputDirectory);
        });

        builder.Services.AddSingleton<IFileChunkAssembler, FileChunkAssembler>();

        builder.Services.AddSingleton<IFileTransferReceiver, FileTransferReceiver>();

        builder.Services.Configure<RabbitMqOptions>(builder.Configuration.GetSection("RabbitMq"));

        builder.Services.AddSingleton(sp => sp.GetRequiredService<IOptions<RabbitMqOptions>>().Value);

        builder.Services.AddSingleton<IChunkTransport, RabbitMqChunkTransport>();


        var host = builder.Build();

        var receiver = host.Services.GetRequiredService<IFileTransferReceiver>();

        Console.WriteLine("Waiting files to be received...");    
        await receiver.ReceiveFileChunks();

    }
}
