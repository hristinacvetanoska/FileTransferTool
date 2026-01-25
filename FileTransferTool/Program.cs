class Program
{
    static void Main()
    {
        var sourceFilePath = String.Empty;
        var destination = String.Empty;
        while (true) {
            Console.Write("Enter source file path (e.g. c:\\source\\my_large_file.bin)");
            sourceFilePath = Console.ReadLine().Trim();
            if (File.Exists(sourceFilePath))
            {
                break;
            }
            Console.WriteLine("Error: The File doesn't exist, please try again.");
        }

        while (true) {

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
        Console.ReadLine();
    }
}