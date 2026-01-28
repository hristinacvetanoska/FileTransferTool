namespace FileTransferTool.Exceptions
{
    /// <summary>
    /// This class is for custom exception when chunk verification fails.
    /// </summary>
    public class ChunkVerificationException : Exception
    {
        public ChunkVerificationException(string message) : base(message)
        {
        }
    }
}