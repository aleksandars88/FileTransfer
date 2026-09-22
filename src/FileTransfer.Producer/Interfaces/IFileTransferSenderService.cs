using FileTransfer.Contracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace FileTransfer.Producer.Interfaces
{
    public interface IFileTransferSenderService
    {
        Task<bool> SendFile(string filePath, CancellationToken cancellationToken = default);
        Task ResendChunk(string filePath, FailedChunk failedChunk, CancellationToken cancellationToken = default);
    }
}
