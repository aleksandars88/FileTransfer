using System;
using System.Collections.Generic;
using System.Text;

namespace FileTransfer.Consumer.Interfaces
{
    public interface IFileTransferReceiver
    {
        Task ReceiveFileChunks(CancellationToken cancellationToken = default);
    }
}
