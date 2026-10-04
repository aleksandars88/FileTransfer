using FileTransfer.Contracts;
using System.Text.Json;

namespace FileTransfer.Infrastructure.Storage
{
    public class FailedChunkStorage : IFailedChunkStorage
    {
        private readonly string _directory;

        public FailedChunkStorage(string directory)
        {
            _directory = directory;

            Directory.CreateDirectory(_directory);
        }

        public async Task StoreAsync(
            FailedChunk chunk,
            CancellationToken cancellationToken = default)
        {
            var fileName = $"{chunk.FileId}_{chunk.ChunkIndex}.json";
            var path = Path.Combine(_directory, fileName);

            var json = JsonSerializer.Serialize(chunk);

            await File.WriteAllTextAsync(
                path,
                json,
                cancellationToken);
        }

        public IEnumerable<FailedChunk> GetFailedChunks()
        {
            foreach (var file in Directory.GetFiles(_directory, "*.json"))
            {
                var json = File.ReadAllText(file);

                var chunk = JsonSerializer.Deserialize<FailedChunk>(json);

                if (chunk is not null)
                {
                    yield return chunk;
                }
            }
        }

        public void Delete(FailedChunk chunk)
        {
            var fileName = $"{chunk.FileId}_{chunk.ChunkIndex}.json";
            var path = Path.Combine(_directory, fileName);

            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }
}
