using System;
using System.Collections.Generic;
using System.Text;

namespace FileTransfer.Contracts
{
    public class FileChunk
    {
        public Guid FileId { get; set; }
        public string FileName { get; set; }
        public int ChunkIndex { get; set; }
        public int TotalChunks { get; set; }
        public long FileSize { get; set; }
        public byte[] Data { get; set; }
        public string Checksum { get; set; }
    }
}
