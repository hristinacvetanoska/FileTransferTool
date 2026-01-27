namespace FileTransferTool.Models
{
    /// <summary>
    /// Contains information about a file, including source and destination paths, file size, and chunk size.
    /// </summary>
    public class FileData
    {
        public string SourceFilePath {  get; set; }
        public string DestinationFilePath { get; set; }
        public long FileSize { get; set; }

        public int ChunkSize = 8388608; // comment: Chunk size set to 8 MB as a balance between memory usage and disk I/O efficiency for large files
    }
}
