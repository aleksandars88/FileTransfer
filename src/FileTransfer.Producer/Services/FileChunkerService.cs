using FileTransfer.Contracts;
using FileTransfer.Infrastructure.Helpers;
using FileTransfer.Producer.Configuration;
using FileTransfer.Producer.Interfaces;
using Microsoft.Extensions.Options;

namespace FileTransfer.Producer.Services
{
    public class FileChunkerService : IFileChunkerService
    {
        private IOptions<FileTransferOptions> _options;

        public FileChunkerService(IOptions<FileTransferOptions> options)
        {
            _options = options;
        }
        public async IAsyncEnumerable<FileChunk> GetFileChunks(string filePath, [System.Runtime.CompilerServices.EnumeratorCancellation]
        CancellationToken cancellationToken = default)
        {
            var fileId = Guid.NewGuid();
            var fileInfo = new FileInfo(filePath);

            if(_options.Value.ChunkSize <= 0)
            {
                throw new ArgumentException("Chunk size must be greater than zero");
            }

            var chunkSize = _options.Value.ChunkSize;
            var totalChunks = (int)Math.Ceiling((double)fileInfo.Length / chunkSize);

            await using var stream = new FileStream(filePath,
                                                     FileMode.Open,
                                                     FileAccess.Read,
                                                     FileShare.Read,
                                                     bufferSize: _options.Value.ChunkSize,
                                                     useAsync: true);
            var buffer = new byte[chunkSize];

            int chunkBytes;

            int chunkIndex = 0;

            while ((chunkBytes = await stream.ReadAsync(
                      buffer.AsMemory(0, buffer.Length),
                      cancellationToken)) > 0)
            {
                var data = buffer[..chunkBytes];

                var checksum = ChecksumHelper.ComputeSha256(data);

                yield return new FileChunk()
                {
                    FileId = fileId,
                    FileName = fileInfo.Name,
                    ChunkIndex = chunkIndex,
                    TotalChunks = totalChunks,
                    FileSize = fileInfo.Length,
                    Data = data,
                    Checksum = checksum
                };

                chunkIndex++;
            }
        }
    }
}
