namespace FileTransferTool.Tests.Serivces
{
    using FileTransferTool.Models;
    using FileTransferTool.Services;

    public class TransferFileServiceTests
    {
        [Fact]
        public void TransferFile_CopiesFileAndVerifiesHashes()
        {
            // Arrange
            var fileName = Path.GetRandomFileName();
            var sourceTempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var destinationTempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(sourceTempDirectory);
            Directory.CreateDirectory(destinationTempDirectory);
            var sourceFilePath = Path.Combine(sourceTempDirectory, fileName);
            var destinationFilePath = Path.Combine(destinationTempDirectory, fileName);
            File.WriteAllText(sourceFilePath, "Some test content");

            var fileData = new FileData
            {
                SourceFilePath = sourceFilePath,
                DestinationFilePath = destinationFilePath,
                FileSize = new FileInfo(sourceFilePath).Length,
                ChunkSize = 1024
            };

            var service = new TransferFileService();

            // Act
            service.TransferFile(fileData);

            // Assert
            Assert.True(File.Exists(sourceFilePath));
            Assert.True(File.Exists(destinationFilePath));
            Assert.Equal(new FileInfo(sourceFilePath).Length, new FileInfo(destinationFilePath).Length);
            Assert.Equal(File.ReadAllBytes(sourceFilePath), File.ReadAllBytes(destinationFilePath));
            File.Delete(sourceFilePath);
            File.Delete(fileData.DestinationFilePath);
        }
    }
}
