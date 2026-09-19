using FileTransfer.Contracts;
using FileTransfer.Producer.Configuration;
using FileTransfer.Producer.Interfaces;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;
using static System.Net.WebRequestMethods;

namespace FileTransfer.Producer.Services
{
    public class FileChunkerService : IFileChunkerService
    {
        public IAsyncEnumerable<FileChunk> GetFileChunks(string filePath, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
