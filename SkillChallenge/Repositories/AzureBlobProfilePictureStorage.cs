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
        Console.WriteLine("CUSTOM DEBUG MESSAGE: AzureBlobProfilePictureStorage constructor called");

        _connectionString = config["AzureBlob:ConnectionString"] ?? throw new InvalidOperationException("Missing AzureBlob:ConnectionString in configuration.");

        _containerName = config["AzureBlob:ContainerName"] ?? throw new InvalidOperationException("Missing AzureBlob:ContainerName in configuration.");


        Console.WriteLine("CUSTOM DEBUG MESSAGE: Connection string is: " + (_connectionString?.Substring(0, 10) ?? "null"));
        Console.WriteLine("CUSTOM DEBUG MESSAGE: Container name is: " + _containerName);

        _logger.LogInformation("CUSTOM DEBUG MESSAGE: AzureBlobProfilePictureStorage initialized.");
    }

    public async Task<string> SaveAsync(IFormFile file)
    {
        _logger.LogInformation($"CUSTOM DEBUG MESSAGE: Saving file: {file.FileName}");
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
        _logger.LogInformation($"CUSTOM DEBUG MESSAGE: File uploaded: {uri}");
        return uri;
    }
}