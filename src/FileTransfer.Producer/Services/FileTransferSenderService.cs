using FileTransfer.Infrastructure;
using FileTransfer.Producer.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FileTransfer.Producer.Services
{
    public class FileTransferSenderService : IFileTransferSenderService
    {
        public Task<bool> SendFile(string filePath, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
