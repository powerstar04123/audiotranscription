using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace AudioTranscriptionApi.Services;

public class BlobStorageService
{
    private readonly string _connectionString;
    private readonly string _containerName;

    public BlobStorageService(IConfiguration configuration)
    {
        _connectionString = configuration["AzureStorage:ConnectionString"] ?? throw new ArgumentNullException("AzureStorage:ConnectionString");
        _containerName = configuration["AzureStorage:ContainerName"] ?? throw new ArgumentNullException("AzureStorage:ContainerName");
    }

    public async Task<string> DownloadAndSaveWavFileAsync(string blobName)
    {
        var blobServiceClient = new BlobServiceClient(_connectionString);
        var containerClient = blobServiceClient.GetBlobContainerClient(_containerName);
        var blobClient = containerClient.GetBlobClient(blobName);
        var blobDownloadInfo = await blobClient.DownloadAsync();
        // Create wav directory if it doesn't exist
        var wavDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..\\..\\..\\wav");
        Directory.CreateDirectory(wavDirectory);
        // Save the file
        var localFilePath = Path.Combine(wavDirectory, "output.wav");
        using (var fileStream = File.Create(localFilePath))
        {
            await blobDownloadInfo.Value.Content.CopyToAsync(fileStream);
        }
        return localFilePath;
    }
} 