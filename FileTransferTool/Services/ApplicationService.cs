namespace FileTransferTool.Services
{
    using FileTransferTool.Exceptions;
    using FileTransferTool.Interfaces;
    using FileTransferTool.Models;
    using System.Threading.Tasks;

    /// <summary>
    /// Coordinates reading file paths and transferring the file.
    /// </summary>
    public class ApplicationService : IApplicationService
    {
        /// <summary>
        /// The service used to transfer files from source to destination.
        /// </summary>
        private readonly ITransferFileService transferFileService;

        /// <summary>
        /// The constructor for ApplicationService.
        /// </summary>
        /// <param name="transferFileService"></param>
        public ApplicationService(ITransferFileService transferFileService)
        {
            this.transferFileService = transferFileService;
        }

        /// <summary>
        /// Reads source and destination paths, creates file metadata, and starts the file transfer.
        /// </summary>
        public async Task RunAsync()
        {
            try
            {
                var sourceFilePath = ReadValidSourceFilePath();
                var destinationFilePath = ReadValidDestinationFile(sourceFilePath);
                var file = new FileData
                {
                    SourceFilePath = sourceFilePath,
                    DestinationFilePath = destinationFilePath,
                    FileSize = new FileInfo(sourceFilePath).Length,
                };

                await this.transferFileService.TransferFile(file);
            }
            catch(ChunkVerificationException cvException)
            {
                Console.WriteLine(cvException.Message);
            }
            catch(IOException ioException)
            {
                Console.WriteLine(ioException.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error: {ex.Message}");
            }

        }

        /// <summary>
        /// Reads and validates the source file path from the user.
        /// </summary>
        /// <returns>The source file path.</returns>
        private string ReadValidSourceFilePath()
        {
            var sourceFilePath = String.Empty;
            while (true)
            {
                Console.Write("Enter source file path (e.g. c:\\source\\my_large_file.bin): \n");
                sourceFilePath = Console.ReadLine().Trim();
                if (File.Exists(sourceFilePath))
                {
                    break;
                }
                Console.WriteLine("Error: The File doesn't exist, please try again.");
            }
            return sourceFilePath;
        }

        /// <summary>
        /// Reads and validates the destination path from the user.
        /// </summary>
        /// <param name="sourceFilePath">Path of the source file to construct the destination path.</param>
        /// <returns>The complete destination file path.</returns>
        private string ReadValidDestinationFile(string sourceFilePath)
        {
            var destination = string.Empty;
            var destinationFilePath = string.Empty;
            while (true)
            {

                Console.Write("Enter destination path (e.g. d:\\destination\\): \n");
                destination = Console.ReadLine().Trim();
                if (!Directory.Exists(destination))
                {
                    Console.WriteLine($"Please try again, provided destination folder path {destination} does not exist.");
                    continue;
                }

                destinationFilePath = Path.Combine(destination, Path.GetFileName(sourceFilePath));

                if (string.Equals(destinationFilePath, sourceFilePath, StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine($"Please try again, the destination file path cannot be the same as the source file path.");
                    continue;
                }
                return destinationFilePath;
            }
        }
    }
}
