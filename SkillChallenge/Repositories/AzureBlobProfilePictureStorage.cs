using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using SkillChallenge.Interfaces;

public class AzureBlobProfilePictureStorage : IProfilePictureStorage
{
    private readonly string _connectionString;
    private readonly string _containerName;

    public AzureBlobProfilePictureStorage(IConfiguration config)
    {
        _connectionString = config["AzureBlob:ConnectionString"];
        _containerName = config["AzureBlob:ContainerName"];
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

        return blobClient.Uri.ToString();
    }
}