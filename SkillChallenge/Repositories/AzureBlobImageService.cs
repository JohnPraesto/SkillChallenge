using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace SkillChallenge.Services
{
    public class AzureBlobImageService : IImageService
    {
        private readonly string _connectionString;
        private readonly IConfiguration _config;

        public AzureBlobImageService(IConfiguration config)
        {
            _config = config;
            _connectionString = config["AzureBlob:ConnectionString"] ?? throw new InvalidOperationException("Missing AzureBlob:ConnectionString in configuration.");
        }

        public async Task<string> SaveImageAsync(IFormFile image, string folder)
        {
            string containerName = folder switch
            {
                "category-images" => _config["AzureBlob:CategoryImageContainer"],
                "subcategory-images" => _config["AzureBlob:SubCategoryImageContainer"],
                "profile-pictures" => _config["AzureBlob:ProfilePictureContainer"],
                _ => throw new ArgumentException("Invalid folder/container type.")
            };

            var blobServiceClient = new BlobServiceClient(_connectionString);
            var containerClient = blobServiceClient.GetBlobContainerClient(containerName);
            await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(image.FileName)}";
            var blobClient = containerClient.GetBlobClient(fileName);

            using (var stream = image.OpenReadStream())
            {
                await blobClient.UploadAsync(stream, overwrite: true);
            }
            return blobClient.Uri.ToString();
        }

        public string GetImageUrl(string? imagePath)
        {
            if (string.IsNullOrEmpty(imagePath))
                return "";
            return imagePath.StartsWith("/") ? imagePath : "/" + imagePath;
        }

        public async Task DeleteImageAsync(string pictureUrl)
        {
            if (string.IsNullOrWhiteSpace(pictureUrl))
                return;

            try
            {
                var uri = new Uri(pictureUrl);
                // The first segment is always "/", so container is at index 1
                var segments = uri.Segments;
                if (segments.Length < 3)
                    return; // Not a valid blob URL

                var containerName = segments[1].TrimEnd('/');
                var blobName = string.Join("", segments.Skip(2));

                var blobServiceClient = new BlobServiceClient(_connectionString);
                var containerClient = blobServiceClient.GetBlobContainerClient(containerName);
                var blobClient = containerClient.GetBlobClient(blobName);

                await blobClient.DeleteIfExistsAsync();
            }
            catch
            {
                // Optionally log the error
            }
        }
    }
}