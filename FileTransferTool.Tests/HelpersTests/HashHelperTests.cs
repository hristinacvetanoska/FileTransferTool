using FileTransferTool.Helpers;
namespace FileTransferTool.Tests.HelpersTests
{
    /// <summary>
    /// Test class for HashHelper.
    /// </summary>
    public class HashHelperTests
    {
        [Fact]
        public void ConvertToHexadecimalString_ReturnsCorrectString()
        {
            // Arrange
            byte[] data = { 0x0F, 0xA0 };
            var expectedString = "0fa0";

            // Act
            var result = HashHelper.ConvertToHexadecimalString(data);

            // Assert
            Assert.Equal(expectedString, result);
            Assert.IsType<string>(result);
        }

        [Fact]
        public void ComputeSHA256Hash_ReturnsCorrectHash()
        {
            string path = "testfile.txt";
            File.WriteAllText(path, "Hello World");
            var hash = HashHelper.ComputeSHA256Hash(path);
            File.Delete(path);

            Assert.Equal("a591a6d40bf420404a011733cfb7b190d62c65bf0bcda32b57b277d9ad9f146e", hash);
        }
    }
}
