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
        Console.WriteLine($"Downloading and saving WAV file from blob storage: {blobName}");
        var blobServiceClient = new BlobServiceClient(_connectionString);
        Console.WriteLine($"Blob service client created");
        var containerClient = blobServiceClient.GetBlobContainerClient(_containerName);
        Console.WriteLine($"Container client created");
        var blobClient = containerClient.GetBlobClient(blobName);
        Console.WriteLine($"Blob client created");
        var blobDownloadInfo = await blobClient.DownloadAsync();
        Console.WriteLine($"Blob download info created");
        // Create wav directory if it doesn't exist
        var wavDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..\\..\\..\\wav");
        Directory.CreateDirectory(wavDirectory);
        Console.WriteLine($"Wav directory created");
        // Save the file
        var localFilePath = Path.Combine(wavDirectory, "output.wav");
        Console.WriteLine($"Local file path created");
        using (var fileStream = File.Create(localFilePath))
        {
            Console.WriteLine($"File stream created");
            await blobDownloadInfo.Value.Content.CopyToAsync(fileStream);
            Console.WriteLine($"File copied to local path");
        }
        Console.WriteLine($"File copied to local path");
        return localFilePath;
    }
} 