using System.Security.Cryptography;

namespace FileTransfer.Infrastructure.Helpers
{
    public static class ChecksumHelper
    {
        public static string ComputeSha256(byte[] data)
        {
            var hash = SHA256.HashData(data);

            return Convert.ToHexString(hash);
        }

        public static string ComputeMd5(byte[] data)
        {
            var hash = MD5.HashData(data);

            return Convert.ToHexString(hash);
        }

    }
}



