using System;
using System.Collections.Generic;
using System.Text;

namespace FileTransfer.Consumer.Interfaces
{
    public interface IFileChunkAssembler
    {
        Task ReassembleAsync(Guid fileId, int totalChunks, string outputPath, CancellationToken cancellationToken = default);
    }
}
