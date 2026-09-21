using System;
using System.Collections.Generic;
using System.Text;

namespace FileTransfer.Producer.Configuration
{
    public class FileTransferOptions
    {
        public string SourceDirectory { get; set; } = "Source";
        public int ChunkSize { get; set; } = 1024*1024;
    }
}
