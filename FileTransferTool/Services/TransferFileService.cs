namespace FileTransferTool.Services
{
    using FileTransferTool.Interfaces;
    using FileTransferTool.Models;
    using System;
    using System.IO;
    using System.Security.Cryptography;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// Service for transferring files from a source path to a destination path in chunks.
    /// </summary>
    public class TransferFileService : ITransferFileService
    {
        /// <summary>
        /// Ensures that only one chunk is written to the destination file at a time.
        /// </summary>
        private static readonly object lockObject = new object();

        /// <summary>
        /// Transfers file from a source path to a destination path in chunks.
        /// Each chunk is hashed using MD5 at the source and verified at the destination.
        /// After transfer, the entire file is hashed using SHA256 for final verification.
        /// Supports large files and concurrent chunk processing to improve performance.
        /// </summary>
        /// <param name="fileData">The file metadata including path, size, and chunk size.</param>
        /// <exception cref="Exception">Thrown if the SHA256 hash of the destination file does not match the source file.</exception>
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

        /// <summary>
        /// Copies a specific chunk of a file from source to destination,
        /// computes the MD5 hash at the source, writes the chunk to the destination,
        /// reads it back, and verifies that the destination hash matches the source.
        /// Retries writing until the hashes match.
        ///</summary>
        /// <param name="chunk">The chunk information, including offset, size, and buffers.</param>
        /// <param name="fileData">The file data including source and destination paths.</param>
        /// <param name="destinationFileStream">The destination file stream used for writing.</param>
        /// <exception cref="Exception">Thrown if the number of bytes read from the destination does not match the expected chunk size.</exception>
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

        /// <summary>
        /// Splits the file into smaller chunks based on the specified chunk size in FileData.
        /// Each chunk contains its offset, size, and will later hold buffers for reading/writing and md5 hash string.
        /// </summary>
        /// <param name="fileData">The file metadata including path, size, and chunk size.</param>
        /// <returns>A list of chunks representing portions of the file.</returns>
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

        /// <summary>
        /// Converts a byte array into its hexadecimal string representation.
        /// </summary>
        /// <param name="hashBytes">The byte array to convert.</param>
        /// <returns>A string containing the hexadecimal representation of the input bytes.</returns>
        private string ConvertToHexadecimalString(byte[] hashBytes)
        {
            StringBuilder sb = new StringBuilder();
            foreach (byte b in hashBytes)
            {
                sb.Append(b.ToString("x2"));
            }
            return sb.ToString();
        }

        /// <summary>
        /// Computes the SHA256 hash of the entire file at the specified path
        /// and returns it as a hexadecimal string.
        /// </summary>
        /// <param name="filePath">The path of the file to compute the hash for.</param>
        /// <returns>The SHA256 hash of the file as a hexadecimal string.</returns>
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

        /// <summary>
        /// Prints the MD5 checksum and file offset for each chunk in the list to the console.
        /// Also prints a message when the file copy is completed.
        /// </summary>
        /// <param name="chunkDataDetails">The list of chunks containing their ID, offset, and MD5 hash.</param>
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
