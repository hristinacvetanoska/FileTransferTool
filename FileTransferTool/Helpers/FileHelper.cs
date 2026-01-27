namespace FileTransferTool.Helpers
{
    using FileTransferTool.Models;

    public static class FileHelper
    {
        /// <summary>
        /// Splits the file into smaller chunks based on the specified chunk size in FileData.
        /// Each chunk contains its offset, size, and will later hold buffers for reading/writing and md5 hash string.
        /// </summary>
        /// <param name="fileData">The file metadata including path, size, and chunk size.</param>
        /// <returns>A list of chunks representing portions of the file.</returns>
        public static List<Chunk> SplitFileInChunks(FileData fileData)
        {
            var numberOfChunks = (int)(fileData.FileSize / fileData.ChunkSize);
            var chunks = new List<Chunk>();
            if (fileData.FileSize % fileData.ChunkSize != 0)
            {
                numberOfChunks += 1;
            }

            for (int i = 0; i < numberOfChunks; i++)
            {
                chunks.Add(new Chunk
                {
                    ChunkId = i,
                    Offset = i * fileData.ChunkSize,
                    Size = Convert.ToInt32((i * fileData.ChunkSize + fileData.ChunkSize <= fileData.FileSize) ? fileData.ChunkSize : fileData.FileSize - i * fileData.ChunkSize),
                });
            }

            return chunks;
        }
    }
}
