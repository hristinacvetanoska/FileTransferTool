namespace FileTransferTool.Helpers
{
    using System.Security.Cryptography;
    using System.Text;

    public static class HashHelper
    {
        /// <summary>
        /// Converts a byte array into its hexadecimal string representation.
        /// </summary>
        /// <param name="hashBytes">The byte array to convert.</param>
        /// <returns>A string containing the hexadecimal representation of the input bytes.</returns>
        public static string ConvertToHexadecimalString(byte[] hashBytes)
        {
            StringBuilder sb = new StringBuilder();
            foreach (byte b in hashBytes)
            {
                sb.Append(b.ToString("x2"));
            }
            return sb.ToString();
        }

        /// <summary>
        /// Computes the SHA256 hash of the entire file at the specified path
        /// and returns it as a hexadecimal string.
        /// </summary>
        /// <param name="filePath">The path of the file to compute the hash for.</param>
        /// <returns>The SHA256 hash of the file as a hexadecimal string.</returns>
        public static string ComputeSHA256Hash(string filePath)
        {
            var fileHexaString = string.Empty;
            using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] sourceHashBytes = sha256.ComputeHash(fileStream);
                fileHexaString = HashHelper.ConvertToHexadecimalString(sourceHashBytes);
            }
            return fileHexaString;
        }
    }
}
