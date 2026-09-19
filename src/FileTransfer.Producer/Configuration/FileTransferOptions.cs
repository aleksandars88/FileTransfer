using System;
using System.Collections.Generic;
using System.Text;

namespace FileTransfer.Producer.Configuration
{
    public class FileTransferOptions
    {
        public int ChunkSize { get; set; } = 1024;
    }
}
