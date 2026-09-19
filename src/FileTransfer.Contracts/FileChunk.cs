using System;
using System.Collections.Generic;
using System.Text;

namespace FileTransfer.Contracts
{
    public class FileChunk
    {
        public int ChunkId { get; set; }
        public byte[] Data { get; set; }
        public int Size { get; set; }
    }
}
