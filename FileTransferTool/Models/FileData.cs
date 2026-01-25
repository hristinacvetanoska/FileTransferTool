namespace FileTransferTool.Models
{
    public class FileData
    {
        public string SourceFilePath {  get; set; }
        public string DestinationFilePath { get; set; }
        public long FileSize { get; set; }

        public int ChunkSize = 8;
        public int ChunkCount { get; set; }
    }
}
