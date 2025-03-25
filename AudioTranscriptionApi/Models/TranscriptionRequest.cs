namespace AudioTranscriptionApi.Models;

public class TranscriptionRequest
{
    public string? Language { get; set; }
    public string BlobName { get; set; } = string.Empty;
} 