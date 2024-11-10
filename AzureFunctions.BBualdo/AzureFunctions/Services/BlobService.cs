using System.Text;
using Azure.Storage.Blobs;

namespace AzureFunctions.Services;

public class BlobService
{
    private readonly BlobServiceClient _blobServiceClient;

    public BlobService(string connectionString)
    {
        _blobServiceClient = new BlobServiceClient(connectionString);
    }

    public async Task UploadAsync(string containerName, string fileName, string reportContent)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
        await containerClient.CreateIfNotExistsAsync();

        var blobClient = containerClient.GetBlobClient(fileName);
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(reportContent));
        await blobClient.UploadAsync(stream, overwrite: true);
    }
}