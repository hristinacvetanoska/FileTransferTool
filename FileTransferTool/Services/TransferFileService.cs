namespace FileTransferTool.Services
{
    using FileTransferTool.Interfaces;
    using FileTransferTool.Models;
    using System;
    using System.IO;
    using System.Security.Cryptography;
    using System.Text;
    using System.Threading.Tasks;

    public class TransferFileService : ITransferFileService
    {
        private static readonly object lockObject = new object();
        public void TransferFile(FileData fileData)
        {
            var chunks = SplitFileInChunks(fileData);

            using (var destStream = new FileStream(fileData.DestinationFilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite))
            {
                Parallel.ForEach(chunks, new ParallelOptions { MaxDegreeOfParallelism = 4 }, chunk =>
                {
                    chunk.Buffer = new byte[chunk.Size];
                    chunk.DestinationBuffer = new byte[chunk.Size];
                    CopyAndVerifyChunk(chunk, fileData, destStream);
                });
            }


            //comment: I decided to use SHA256 because it is better and more secure compared with SHA1
            var sourceFileHexaString = ComputeSHA256Hash(fileData.SourceFilePath);
            var destinationFileHexaString = ComputeSHA256Hash(fileData.DestinationFilePath);
            if (sourceFileHexaString.Equals(destinationFileHexaString))
            {
                Console.WriteLine($"Source SHA256:{sourceFileHexaString}");
                Console.WriteLine($"Destination SHA256: {destinationFileHexaString}");
            }
            else
            {
                throw new Exception("The files don't match");
            }

            PrintChecksumAndOffset(chunks);

        }
        private void CopyAndVerifyChunk(Chunk chunk, FileData fileData, FileStream destinationFileStream)
        {
            var bytesRead = 0;
            var sourceHexaString = string.Empty;
            using (FileStream sourceFileStream = new FileStream(fileData.SourceFilePath, FileMode.Open, FileAccess.Read)) 
            using(MD5 md5 = MD5.Create())
            {
                sourceFileStream.Seek(chunk.Offset, SeekOrigin.Begin);
                bytesRead = sourceFileStream.Read(chunk.Buffer, 0, chunk.Size);
                byte[] sourceHashBytes = md5.ComputeHash(chunk.Buffer);
                sourceHexaString = ConvertToHexadecimalString(sourceHashBytes);
            }

            var chunkHashVerification = false;
            while (!chunkHashVerification)
            {
                lock (lockObject)
                {
                    destinationFileStream.Seek(chunk.Offset, SeekOrigin.Begin);
                    destinationFileStream.Write(chunk.Buffer, 0, bytesRead);
                    destinationFileStream.Flush();
                    destinationFileStream.Seek(chunk.Offset, SeekOrigin.Begin);
                    var destinationBytesRead = destinationFileStream.Read(chunk.DestinationBuffer, 0, bytesRead);

                    if (destinationBytesRead != bytesRead)
                    {
                        throw new Exception("Could not read the expected chunk size from destination!");
                    }

                    using (MD5 md5 = MD5.Create())
                    {
                        byte[] destinationHashBytes = md5.ComputeHash(chunk.DestinationBuffer);
                        var destinationHexaString = ConvertToHexadecimalString(destinationHashBytes);

                        if (sourceHexaString.Equals(destinationHexaString))
                        {
                            chunk.MD5 = sourceHexaString;
                            chunkHashVerification = true;
                        }
                    }
                }
            }
        }
        private List<Chunk> SplitFileInChunks(FileData fileData)
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
        private string ConvertToHexadecimalString(byte[] hashBytes)
        {
            StringBuilder sb = new StringBuilder();
            foreach (byte b in hashBytes)
            {
                sb.Append(b.ToString("x2"));
            }
            return sb.ToString();
        }
        private string ComputeSHA256Hash(string filePath)
        {
            var fileHexaString = string.Empty;
            using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] sourceHashBytes = sha256.ComputeHash(fileStream);
                fileHexaString = ConvertToHexadecimalString(sourceHashBytes);
            }
            return fileHexaString;
        }
        private void PrintChecksumAndOffset(List<Chunk> chunkDataDetails)
        {
            foreach (var chunk in chunkDataDetails)
            {
                Console.WriteLine($"{chunk.ChunkId}) position = {chunk.Offset}, hash = {chunk.MD5}");
            }
            Console.WriteLine("File copy completed!");
        }
    }
}
