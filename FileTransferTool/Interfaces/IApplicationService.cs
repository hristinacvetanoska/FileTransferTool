namespace FileTransferTool.Interfaces
{
    /// <summary>
    /// Interface for ApplicationService.
    /// </summary>
    public interface IApplicationService
    {
        /// <summary>
        /// Reads source and destination paths, creates file metadata, and starts the file transfer.
        /// </summary>
        Task RunAsync();
    }
}