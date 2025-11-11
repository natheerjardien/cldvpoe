using Azure.Storage.Files.Shares.Models;
using Azure.Storage.Files.Shares;
using Azure;
using System.Text.RegularExpressions;

namespace CLDV6212_POE_ST10435542.Models.Services
{
// As demonstrated by IIEVC School of Computer Science (2025), the AzureFileShareService is responsible for managing file uploads and downloads to Azure File Share
// This model conatins the logic behind the action methods for uploading, downloading, and listing files in the Azure File Share
    public class AzureFileShareService
    {
        private readonly string _connectionString;
        private readonly string _fileShareName;

        public AzureFileShareService(string connectionString, string fileShareName)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            _fileShareName = fileShareName ?? throw new ArgumentNullException(nameof(fileShareName));
        }

        public async Task UploadFileAsync(string directoryName, string fileName, Stream fileStream)
        {
            try
            {
                // Sanitize directory and file names for Azure File Share
                directoryName = Regex.Replace(directoryName, @"[^a-zA-Z0-9_\-]", "_");
                fileName = Regex.Replace(fileName, @"[^a-zA-Z0-9_\-\.]", "_");

                // Remove trailing dots and spaces
                directoryName = directoryName.Trim().TrimEnd('.', ' ');
                fileName = fileName.Trim().TrimEnd('.', ' ');

                // Log for debugging
                Console.WriteLine($"Directory: '{directoryName}', File: '{fileName}'");

                var serviceClient = new ShareServiceClient(_connectionString); // creates a new instance of the ShareServiceClient with the connection string
                var shareClient = serviceClient.GetShareClient(_fileShareName); // gets the ShareClient for the specified file share

                var directoryClient = shareClient.GetDirectoryClient(directoryName); // gets the DirectoryClient for the specified directory
                await directoryClient.CreateIfNotExistsAsync(); // creates the directory if it does not exist

                var fileClient = directoryClient.GetFileClient(fileName); // gets the FileClient for the specified file

                await fileClient.CreateAsync(fileStream.Length); // creates the file with the specified length
                await fileClient.UploadRangeAsync(new HttpRange(0, fileStream.Length), fileStream); // uploads the file stream to the file in the Azure File Share
            }
            catch (Exception ex)
            {
                throw new Exception("Error uploading file :" + ex.Message, ex);
            }
        }

        public async Task<Stream> DownLoadFileAsync(string directoryName, string fileName)
        {
            try
            {
                var serviceClient = new ShareServiceClient(_connectionString); // creates a new instance of the ShareServiceClient with the connection string
                var shareClient = serviceClient.GetShareClient(_fileShareName); // gets the ShareClient for the specified file share
                var directoryClient = shareClient.GetDirectoryClient(directoryName); // gets the DirectoryClient for the specified directory
                var fileClient = directoryClient.GetFileClient(fileName); // gets the FileClient for the specified file
                var downloadInfo = await fileClient.DownloadAsync(); // downloads the file from the Azure File Share

                return downloadInfo.Value.Content; // returns the file stream for the downloaded file
            }
            catch (Exception ex)
            {
                throw new Exception("Error downloading file :" + ex.Message, ex);
            }
        }

        public async Task<List<FileModel>> ListFilesAsync(string directoryName)
        {
            var fileModels = new List<FileModel>();

            try
            {
                var serviceClient = new ShareServiceClient(_connectionString);
                var shareClient = serviceClient.GetShareClient(_fileShareName);

                var directoryClient = shareClient.GetDirectoryClient(directoryName);
                await foreach (ShareFileItem item in directoryClient.GetFilesAndDirectoriesAsync()) // iterates through the files and directories in the specified directory
                {
                    if (!item.IsDirectory)
                    {
                        var fileClient = directoryClient.GetFileClient(item.Name); // gets the FileClient for the specified file
                        var properties = await fileClient.GetPropertiesAsync(); // retrieves the properties of the file

                        fileModels.Add(new FileModel // creates a new FileModel object to hold the files details
                        {
                            Name = item.Name,
                            Size = properties.Value.ContentLength,
                            LastModified = properties.Value.LastModified
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error listing files :" + ex.Message, ex);
            }

            return fileModels;
        }

        public async Task WriteTextToFileAsync(string directoryName, string fileName, string content) // for filefunction
        {
            try
            {
                var serviceClient = new ShareServiceClient(_connectionString);
                var shareClient = serviceClient.GetShareClient(_fileShareName);
                var directoryClient = shareClient.GetDirectoryClient(directoryName);

                await directoryClient.CreateIfNotExistsAsync();

                var fileClient = directoryClient.GetFileClient(fileName);

                byte[] contentBytes = System.Text.Encoding.UTF8.GetBytes(content);
                using var stream = new MemoryStream(contentBytes);

                await fileClient.CreateAsync(stream.Length);
                await fileClient.UploadRangeAsync(new HttpRange(0, stream.Length), stream);
            }
            catch (Exception ex)
            {
                throw new Exception("Error writing text to file: " + ex.Message, ex);
            }
        }
    }
}

/* References:

IIEVC School of Computer Science, 2025. CLDV6212 ASP.NET MVC & Azure Series - Part 4: Mastering Azure File Share!. [video online] 
Available at: <https://youtu.be/A-mVVL88oEg?si=eIL4gyih_S6aWw2a>
[Accessed 21 August 2025].

*/
