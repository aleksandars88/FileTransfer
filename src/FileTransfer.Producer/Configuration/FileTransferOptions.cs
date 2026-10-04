using System;
using System.Collections.Generic;
using System.Text;

namespace FileTransfer.Producer.Configuration
{
    public class FileTransferOptions
    {
        public string SourceDirectory { get; set; } 
        public string FailedChunksDirectory { get; set; } = "FailedChunks";
        public int ChunkSize { get; set; }
    }
}
