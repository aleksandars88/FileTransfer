using FileTransfer.Consumer.Interfaces;
using FileTransfer.Infrastructure.Storage;
using System;
using System.Collections.Generic;
using System.Text;

namespace FileTransfer.Consumer.Services
{
    public class FileChunkAssembler : IFileChunkAssembler
    {
        private readonly IFileChunkStorage _storage;

        public FileChunkAssembler(IFileChunkStorage storage)
        {
            _storage = storage;
        }

        public async Task ReassembleAsync(Guid fileId, int totalChunks, string outputPath, CancellationToken cancellationToken = default)
        {
            await using var output = new FileStream(
                outputPath,
                FileMode.Create,
                FileAccess.Write,
                FileShare.None,
                bufferSize: 81920,
                useAsync: true);

            for (var i = 0; i < totalChunks; i++)
            {
                var chunk = await _storage.GetAsync(
                    fileId,
                    i,
                    cancellationToken);

                await output.WriteAsync(
                    chunk.Data,
                    cancellationToken);
            }
        }
    }
}
