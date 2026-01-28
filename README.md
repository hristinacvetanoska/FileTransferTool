# FileTransferTool

FileTransferTool is a C# console application designed to efficiently transfer large files from a source path to a destination path. 
The application ensures data integrity by splitting files into chunks, verifying each chunk using MD5 hashes, and performing a final SHA256 hash check on the complete file. 
This makes it reliable for handling large files or scenarios where accuracy is critical.

### Features
- Transfers large files by splitting them into chunks
- Verifies each chunk using MD5 hashing, with up to 3 retries if verification fails
- Performs final file integrity validation using SHA256
- Supports parallel chunk processing for better performance (up to 8 concurrent chunks)
- Prevents copying a file onto itself

## Technologies
- C# 
- .NET 10 
- Async/Await
- MD5/SHA256 hashing
- SemaphoreSlim for concurrency

## Project Structure

- Services – application logic and file transfer logic
- Interfaces – service abstractions
- Models – FileData and Chunk models
- Helpers – hashing and file-related helper logic

## Testing

The project includes unit tests for helper classes as well as tests covering the core file transfer logic.
The tests validate hashing, chunk handling, and ensure that files are transferred correctly from source to destination.

## How to Run

1. Clone the repository:
`git clone https://github.com/hristinacvetanoska/FileTransferTool.git`
3. Open the solution in Visual Studio.
4. Run the project as a Console Application.
5. Enter the source file path and destination directory when prompted.
