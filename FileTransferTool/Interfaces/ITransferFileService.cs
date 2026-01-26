namespace FileTransferTool.Interfaces
{
    using FileTransferTool.Models;

    /// <summary>
    /// Interface for transferring files from a source path to a destination path in chunks.
    /// </summary>
    public interface ITransferFileService
    {
        /// <summary>
        /// Transfers file from a source path to a destination path in chunks.
        /// Each chunk is hashed using MD5 at the source and verified at the destination.
        /// After transfer, the entire file is hashed using SHA256 for final verification.
        /// Supports large files and concurrent chunk processing to improve performance.
        /// </summary>
        /// <param name="fileData">The file metadata including path, size, and chunk size.</param>
        /// <exception cref="Exception">Thrown if the SHA256 hash of the destination file does not match the source file.</exception>

        void TransferFile(FileData file);
    }
}
