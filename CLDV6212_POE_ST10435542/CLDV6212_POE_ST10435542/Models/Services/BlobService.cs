using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using System.ComponentModel;

namespace CLDV6212_POE_ST10435542.Models.Services
{
// As demonstrated by IIEVC School of Computer Science (2025), the BlobService is responsible for managing image uploads to Azure Blob Storage
// Ive implemented methods for uploading images, deleting blobs, and retrieving the BlobContainerClient
    public class BlobService
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly string _containerName = "Products";

        public BlobService(string connectionString)
        {
            _blobServiceClient = new BlobServiceClient(connectionString);
        }

        public async Task<string> UploadAsync(IFormFile file, string containerName, string fileName) // upoads image to blob storage
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

            var blobClient = containerClient.GetBlobClient(fileName);

            using var stream = file.OpenReadStream();
            await blobClient.UploadAsync(stream, overwrite: true);

            return blobClient.Uri.ToString();
        }

        public async Task DeleteBlobAsync(string blobUri)
        {
            if (string.IsNullOrEmpty(blobUri)) return;

            Uri uri = new Uri(blobUri);
            var segments = uri.Segments;
            var blobName = segments.Last();

            var containerClient = _blobServiceClient.GetBlobContainerClient("productimages");
            var blobClient = containerClient.GetBlobClient(blobName);
            await blobClient.DeleteIfExistsAsync(DeleteSnapshotsOption.IncludeSnapshots);
        }

        public BlobContainerClient GetBlobContainer(string containerName)
        {
            return _blobServiceClient.GetBlobContainerClient(_containerName);
        }
    }
}

/* References:

IIEVC School of Computer Science, 2025. CLDV6212 ASP.NET MVC & Azure Series - Part 2: Adding Image Uploads with Blob Storage!. [video online] 
Available at: <https://youtu.be/CuszKqZvRuM?si=RZaHcDniR_ZWB-59>
[Accessed 16 August 2025].

*/
