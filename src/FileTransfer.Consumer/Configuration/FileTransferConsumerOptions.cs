using System;
using System.Collections.Generic;
using System.Text;

namespace FileTransfer.Consumer.Configuration
{
    public class FileTransferConsumerOptions
    {
        public string StorageDirectory { get; set; } = string.Empty;
        public string OutputDirectory { get; set; } = string.Empty;
    }
}
