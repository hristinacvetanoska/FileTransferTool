namespace FileTransferTool.Interfaces
{
    using FileTransferTool.Models;

    public interface ITransferFile
    {
        void TransferFile(FileData file);
    }
}
