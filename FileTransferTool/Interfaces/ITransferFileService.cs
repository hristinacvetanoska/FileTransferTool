namespace FileTransferTool.Interfaces
{
    using FileTransferTool.Models;

    public interface ITransferFileService
    {
        void TransferFile(FileData file);
    }
}
