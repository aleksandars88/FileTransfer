using System;
using System.Collections.Generic;
using System.Text;

namespace FileTransfer.Contracts
{
    public class FailedChunk
    {
        public Guid FileId { get; set; }

        public string FileName { get; set; } = string.Empty;

        public int ChunkIndex { get; set; }
    }
}
