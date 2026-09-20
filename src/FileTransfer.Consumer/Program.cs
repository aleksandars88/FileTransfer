using FileTransfer.Consumer.Configuration;
using FileTransfer.Consumer.Interfaces;
using FileTransfer.Consumer.Services;
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
            return new FileChunkStorage(options.StorageDirectory);
        });

        builder.Services.AddSingleton<IFileChunkAssembler, FileChunkAssembler>();

        builder.Services.AddSingleton<IFileTransferReceiver, FileTransferReceiver>();

        builder.Services.AddSingleton<IChunkTransport, InMemoryChunkTransport>();

        var host = builder.Build();

        var receiver = host.Services.GetRequiredService<IFileTransferReceiver>();

        await receiver.ReceiveFileChunks();

    }
}
