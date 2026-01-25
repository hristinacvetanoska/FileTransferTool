namespace FileTransferTool.Services
{
    using FileTransferTool.Interfaces;
    using FileTransferTool.Models;
    using System;
    using System.IO;

    public class TransferFileService : ITransferFile
    {
        public void TransferFile(FileData file)
        {
            int maxChunkSize = file.ChunkSize * 1024 * 1024;
            byte[] buffer = new byte[maxChunkSize];
            int position = 0, totalBytesCopied = 0, bytesRead;
            int chunkIndex = 0, numberOfChunk = 0;
            using (FileStream sourceFileStream = new FileStream(file.SourceFilePath, FileMode.Open, FileAccess.Read))
            using (FileStream destinationFileStream = new FileStream(file.DestinationFilePath, FileMode.Create, FileAccess.Write))
            {
                while ((bytesRead = sourceFileStream.Read(buffer, 0, maxChunkSize)) > 0) //reading 8MB chunks at a time
                {
                    destinationFileStream.Write(buffer, 0, bytesRead);
                    numberOfChunk++;
                    totalBytesCopied = totalBytesCopied + bytesRead;
                }
            }
            Console.WriteLine("File copy completed!");

        }
    }
}
