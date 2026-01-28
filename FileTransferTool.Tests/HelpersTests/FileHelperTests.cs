namespace FileTransferTool.Tests.HelpersTests
{
    using FileTransferTool.Helpers;
    using FileTransferTool.Models;
    using Xunit;

    /// <summary>
    /// Test class for FileHelper
    /// </summary>
    public class FileHelperTests
    {
        [Fact]
        public void SplitFileInChunks_CreatesCorrectChunks()
        {
            // Arrange
            var fileData = new FileData
            {
                FileSize = 51225,
                ChunkSize = 1024
            };

            // Act.
            var result = FileHelper.SplitFileInChunks(fileData);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(51, result.Count);
            Assert.Equal(25, result[result.Count - 1].Size);

        }

        [Fact]
        public void SplitFileInChunks_FileSizeIsZero()
        {
            // Arrange
            var fileData = new FileData
            {
                ChunkSize = 1024
            };

            // Act.
            var result = FileHelper.SplitFileInChunks(fileData);

            // Assert
            Assert.Empty(result);

        }

        [Fact]
        public void SplitFileInChunks_FileSizeIsSmallerThanChunkSize()
        {
            // Arrange
            var fileData = new FileData
            {
                FileSize = 500,
                ChunkSize = 1024
            };

            // Act.
            var result = FileHelper.SplitFileInChunks(fileData);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal(500, result[result.Count - 1].Size);
        }
    }
}
