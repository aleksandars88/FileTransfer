using System.Security.Cryptography;

namespace FileTransfer.Infrastructure.Helpers
{
    public static class ChecksumHelper
    {
        public static async Task<string> ComputeSha256(string filePath, CancellationToken cancellationToken = default)
        {
            await using var stream = File.OpenRead(filePath);

            using var sha256 = SHA256.Create();

            var hash = await sha256.ComputeHashAsync(stream, cancellationToken);

            return Convert.ToHexString(hash);
        }

        public static string ComputeMd5(byte[] data)
        {
            var hash = MD5.HashData(data);

            return Convert.ToHexString(hash);
        }

    }
}



