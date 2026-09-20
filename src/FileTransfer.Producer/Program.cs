using FileTransfer.Infrastructure.Transport;
using FileTransfer.Producer.Configuration;
using FileTransfer.Producer.Interfaces;
using FileTransfer.Producer.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);
        builder.Services.Configure<FileTransferOptions>(builder.Configuration.GetSection("FileTransferOptions"));
        builder.Services.AddSingleton<IChunkTransport, InMemoryChunkTransport>();
        builder.Services.AddSingleton<IFileChunkerService, FileChunkerService>();
        builder.Services.AddSingleton<IFileTransferSenderService, FileTransferSenderService>();
        using var host = builder.Build();

        var fileTransferSender = host.Services.GetRequiredService<IFileTransferSenderService>();
        fileTransferSender.SendFile("Data/sample-10mb.jpg").GetAwaiter().GetResult();  
    }
}