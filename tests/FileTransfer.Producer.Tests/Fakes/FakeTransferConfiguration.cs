using FileTransfer.Producer.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace FileTransfer.Producer.Tests.Fakes
{
    internal class FakeTransferConfiguration : IOptions<FileTransferOptions>
    {
        public FileTransferOptions Value => new FileTransferOptions
        {
            ChunkSize = 1024 * 1024 // 1 MB
        };  
    }
}
