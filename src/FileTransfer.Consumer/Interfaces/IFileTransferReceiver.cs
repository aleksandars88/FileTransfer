using System;
using System.Collections.Generic;
using System.Text;

namespace FileTransfer.Consumer.Interfaces
{
    public interface IFileTransferReceiver
    {
        Task ReceiveFileChunks(string destinationPath, CancellationToken cancellationToken = default);
    }
}
