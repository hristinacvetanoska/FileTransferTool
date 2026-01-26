namespace FileTransferTool.Services
{
    using FileTransferTool.Interfaces;
    using FileTransferTool.Models;
    using System.Threading.Tasks;
    public class ApplicationService : IApplicationService
    {
        private readonly ITransferFileService transferFileService;
        public ApplicationService(ITransferFileService transferFileService)
        {
            this.transferFileService = transferFileService;
        }
        public async Task RunAsync()
        {
            var sourceFilePath = ReadValidSourceFile();
            var destinationFilePath = ReadValidDestinationFile(sourceFilePath);
            var file = new FileData
            {
                SourceFilePath = sourceFilePath,
                DestinationFilePath = destinationFilePath,
            };

            this.transferFileService.TransferFile(file);

        }
        private string ReadValidSourceFile()
        {
            var sourceFilePath = String.Empty;
            while (true)
            {
                Console.Write("Enter source file path (e.g. c:\\source\\my_large_file.bin)");
                sourceFilePath = Console.ReadLine().Trim();
                if (File.Exists(sourceFilePath))
                {
                    break;
                }
                Console.WriteLine("Error: The File doesn't exist, please try again.");
            }
            return sourceFilePath;
        }

        private string ReadValidDestinationFile(string sourceFilePath)
        {
            var destination = string.Empty;
            while (true)
            {

                Console.Write("Enter destination path (e.g. d:\\destination\\)");
                destination = Console.ReadLine().Trim();
                if (Directory.Exists(destination))
                {
                    break;
                }
                Console.WriteLine($"Directory {destination} does not exist!");
            }
            var destionationFilePath = Path.Combine(destination, Path.GetFileName(sourceFilePath));
            Console.WriteLine($"Full destination file path is: {destionationFilePath}");
            return destionationFilePath;
        }
    }
}
