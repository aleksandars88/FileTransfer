using System;
using System.Collections.Generic;
using System.Text;

namespace FileTransfer.Consumer.Configuration
{
    public class FileTransferConsumerOptions
    {
        public string OutputDirectory { get; set; } = "Destination";

        public string FailedChunksDirectory { get; set; } = "FailedChunks";
    }
}
