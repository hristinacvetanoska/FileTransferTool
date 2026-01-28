namespace FileTransferTool.Services
{
    using FileTransferTool.Helpers;
    using FileTransferTool.Interfaces;
    using FileTransferTool.Models;
    using System;
    using System.IO;
    using System.Security.Cryptography;
    using System.Threading.Tasks;

    /// <summary>
    /// Service for transferring files from a source path to a destination path in chunks.
    /// </summary>
    public class TransferFileService : ITransferFileService
    {
        /// <summary>
        /// Limits up to 8 parallel tasks to read/write chunks at the same time.
        /// </summary>
        private static readonly SemaphoreSlim semaphore = new SemaphoreSlim(8);

        /// <summary>
        /// Transfers file from a source path to a destination path in chunks.
        /// Each chunk is hashed using MD5 at the source and verified at the destination.
        /// After transfer, the entire file is hashed using SHA256 for final verification.
        /// Supports large files and concurrent chunk processing to improve performance.
        /// </summary>
        /// <param name="fileData">The file metadata including path, size, and chunk size.</param>
        /// <exception cref="Exception">Thrown if the SHA256 hash of the destination file does not match the source file.</exception>
        public async Task TransferFile(FileData fileData)
        {
            var chunks = FileHelper.SplitFileInChunks(fileData);
            var tasks = new List<Task>();

            foreach (var chunk in chunks)
            {
                tasks.Add(Task.Run(async () =>
                {
                    await semaphore.WaitAsync();
                    try
                    {
                        using var destStream = new FileStream(fileData.DestinationFilePath, FileMode.OpenOrCreate,
                                                   FileAccess.ReadWrite, FileShare.ReadWrite, 8192, true);

                        chunk.Buffer = new byte[chunk.Size];
                        chunk.DestinationBuffer = new byte[chunk.Size];
                        await CopyAndVerifyChunk(chunk, fileData, destStream);
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                }));
            }

            await Task.WhenAll(tasks);

            //comment: I decided to use SHA256 because it is better and more secure compared with SHA1
            var sourceFileHexaString = HashHelper.ComputeSHA256Hash(fileData.SourceFilePath);
            var destinationFileHexaString = HashHelper.ComputeSHA256Hash(fileData.DestinationFilePath);
            Console.WriteLine($"Source SHA256:{sourceFileHexaString}");
            Console.WriteLine($"Destination SHA256: {destinationFileHexaString}");

            if (sourceFileHexaString.Equals(destinationFileHexaString))
            {
                Console.WriteLine("The files are identical.");
            }
            else
            {
                throw new Exception("The files do not match.");
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
        /// <exception cref="Exception">Thrown if the number of bytes read from the destination does not match the expected chunk size.</exception>
        private async Task CopyAndVerifyChunk(Chunk chunk, FileData fileData, FileStream destinationFileStream)
        {
            var bytesRead = 0;
            var sourceHexaString = string.Empty;
            using (FileStream sourceFileStream = new FileStream(fileData.SourceFilePath, FileMode.Open, FileAccess.Read)) 
            using(MD5 md5 = MD5.Create())
            {
                sourceFileStream.Seek(chunk.Offset, SeekOrigin.Begin);
                bytesRead = await sourceFileStream.ReadAsync(chunk.Buffer, 0, chunk.Size);
                byte[] sourceHashBytes = md5.ComputeHash(chunk.Buffer);
                sourceHexaString = HashHelper.ConvertToHexadecimalString(sourceHashBytes);
            }

            var chunkHashVerification = false;
            var retryCount = 3;
            //comment: I decided to add retryCount to prevent infinite loop if something goes wrong.
            while (!chunkHashVerification && retryCount>0)
            {
                destinationFileStream.Seek(chunk.Offset, SeekOrigin.Begin);
                await destinationFileStream.WriteAsync(chunk.Buffer, 0, bytesRead);
                await destinationFileStream.FlushAsync();
                destinationFileStream.Seek(chunk.Offset, SeekOrigin.Begin);
                var destinationBytesRead = await destinationFileStream.ReadAsync(chunk.DestinationBuffer, 0, bytesRead);

                if (destinationBytesRead != bytesRead)
                {
                    throw new Exception("Could not read the expected chunk size from destination!");
                }
                using (MD5 md5 = MD5.Create())
                {
                    byte[] destinationHashBytes = md5.ComputeHash(chunk.DestinationBuffer);
                    var destinationHexaString = HashHelper.ConvertToHexadecimalString(destinationHashBytes);
                    if (sourceHexaString.Equals(destinationHexaString))
                    {
                        chunk.MD5 = sourceHexaString;
                        chunkHashVerification = true;
                    }
                    else
                    {
                        retryCount--;
                        Console.WriteLine($"The source md5 hash does not match the destination md5 hash for chunk with id = {chunk.ChunkId}!");
                    }
                }
            }
        }

        /// <summary>
        /// Prints the MD5 checksum and file offset for each chunk in the list to the console.
        /// </summary>
        /// <param name="chunkDataDetails">The list of chunks containing their ID, offset, and MD5 hash.</param>
        private void PrintChecksumAndOffset(List<Chunk> chunkDataDetails)
        {
            foreach (var chunk in chunkDataDetails)
            {
                Console.WriteLine($"{chunk.ChunkId}) position = {chunk.Offset}, hash = {chunk.MD5}");
            }
        }
    }
}
