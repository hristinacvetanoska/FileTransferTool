namespace FileTransferTool.Services
{
    using FileTransferTool.Interfaces;
    using FileTransferTool.Models;
    using System;
    using System.IO;
    using System.Security.Cryptography;
    using System.Text;

    public class TransferFileService : ITransferFileService
    {
        public void TransferFile(FileData fileData)
        {
            Dictionary<long, string> chunkDataDetails = new Dictionary<long, string>();
            int maxChunkSize = 8192, i=0;
            byte[] buffer = new byte[maxChunkSize];
            byte[] destinationBuffer = new byte[maxChunkSize];
            int position = 0, totalBytesCopied = 0, bytesRead;
            int chunkIndex = 0, numberOfChunk = 0;

            using (FileStream sourceFileStream = new FileStream(fileData.SourceFilePath, FileMode.Open, FileAccess.Read))
            using (FileStream destinationFileStream = new FileStream(fileData.DestinationFilePath, FileMode.Create, FileAccess.ReadWrite))
            using (MD5 md5 = MD5.Create())
            {
                while ((bytesRead = sourceFileStream.Read(buffer, 0, maxChunkSize)) > 0)
                {
                    byte[] sourceHashBytes = md5.ComputeHash(buffer);
                    var sourceHexaString = ConvertToHexadecimalString(sourceHashBytes);
                    chunkDataDetails.Add(position, sourceHexaString);

                    numberOfChunk++;
                    totalBytesCopied = totalBytesCopied + bytesRead;
                    position += bytesRead;

                    var chunkHashVerification = false;
                    while (!chunkHashVerification)
                    {
                        destinationFileStream.Write(buffer, 0, bytesRead);
                        destinationFileStream.Flush();
                        destinationFileStream.Seek(position - bytesRead, SeekOrigin.Begin);

                        var destinationBytesRead = destinationFileStream.Read(destinationBuffer, 0, bytesRead);

                        if (destinationBytesRead != bytesRead)
                        {
                            throw new Exception("Could not read the expected chunk size from destination!");
                        }

                        byte[] destinationHashBytes = md5.ComputeHash(buffer);
                        var destinationHexaString = ConvertToHexadecimalString(destinationHashBytes);

                        if (sourceHexaString.Equals(destinationHexaString))
                        {
                            chunkHashVerification = true;
                        }
                    }
                }
            }

            var soureceFileHexaString = ComputeSHA1Hash(fileData.SourceFilePath);
            var destinationFileHexaString = ComputeSHA1Hash(fileData.DestinationFilePath);
            if (soureceFileHexaString.Equals(destinationFileHexaString))
            {
                Console.WriteLine($"Source SHA1:{soureceFileHexaString}");
                Console.WriteLine($"Destination SHA1: {destinationFileHexaString}");
            }
            else
            {
                throw new Exception("The files don't match");
            }

            PrintChecksumAndOffset(chunkDataDetails);

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

        private void PrintChecksumAndOffset(Dictionary<long, string> chunkDataDetails)
        {
            var i = 0;
            foreach (var chunk in chunkDataDetails)
            {
                Console.WriteLine($"{i}) position = {chunk.Key}, hash = {chunk.Value}");
                i++;
            }
            Console.WriteLine("File copy completed!");
        }

        private string ComputeSHA1Hash(string filePath)
        {
            var fileHexaString = string.Empty;
            using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            using (SHA1 sha1 = SHA1.Create())
            {
                byte[] sourceHashBytes = sha1.ComputeHash(fileStream);
                fileHexaString = ConvertToHexadecimalString(sourceHashBytes);
            }
            return fileHexaString;
        }
    }
}
