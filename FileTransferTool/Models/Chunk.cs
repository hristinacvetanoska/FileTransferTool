namespace FileTransferTool.Models
{
    /// <summary>
    /// This class defines the chunk.
    /// </summary>
    public class Chunk
    {
        public int ChunkId { get; set; }
        public long Offset { get; set; }
        public long Size { get; set; }
        public byte[] Data { get; set; }
        public string MD5 { get; set; }
    }
}
