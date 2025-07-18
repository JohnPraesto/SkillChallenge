using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using SkillChallenge.Interfaces;

public class AzureBlobProfilePictureStorage : IProfilePictureStorage
{
    private readonly string _connectionString;
    private readonly string _containerName;
    private readonly ILogger<AzureBlobProfilePictureStorage> _logger;

    public AzureBlobProfilePictureStorage(IConfiguration config, ILogger<AzureBlobProfilePictureStorage> logger)
    {
        _logger = logger;
        _connectionString = config["AzureBlob:ConnectionString"] ?? throw new InvalidOperationException("Missing AzureBlob:ConnectionString in configuration.");

        _containerName = config["AzureBlob:ContainerName"] ?? throw new InvalidOperationException("Missing AzureBlob:ContainerName in configuration.");
    }

    public async Task<string> SaveAsync(IFormFile file)
    {
        var blobServiceClient = new BlobServiceClient(_connectionString);
        var containerClient = blobServiceClient.GetBlobContainerClient(_containerName);
        await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

        var fileName = $"{Guid.NewGuid()}_{file.FileName}";
        var blobClient = containerClient.GetBlobClient(fileName);

        using (var stream = file.OpenReadStream())
        {
            await blobClient.UploadAsync(stream, overwrite: true);
        }
        var uri = blobClient.Uri.ToString();
        return uri;
    }

    public async Task DeleteAsync(string pictureUrl)
    {
        if (string.IsNullOrWhiteSpace(pictureUrl))
            return;

        try
        {
            // Extract blob name from the URL
            var uri = new Uri(pictureUrl);
            var blobName = uri.Segments.Last(); // If no folders, this works
            if (uri.Segments.Length > 2)
                blobName = string.Join("", uri.Segments.Skip(2)); // Handles folders

            var blobServiceClient = new BlobServiceClient(_connectionString);
            var containerClient = blobServiceClient.GetBlobContainerClient(_containerName);
            var blobClient = containerClient.GetBlobClient(blobName);

            await blobClient.DeleteIfExistsAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete blob for URL: {Url}", pictureUrl);
        }
    }
}