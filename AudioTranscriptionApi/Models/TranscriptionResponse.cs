namespace AudioTranscriptionApi.Models;

public class TranscriptionResponse
{
    public List<TranscriptionEntry> Transcription { get; set; } = new();
}

public class TranscriptionEntry
{
    public string Speaker { get; set; } = string.Empty;
    public AudioTimeSpan Timespan { get; set; } = new();
    public string Text { get; set; } = string.Empty;
}

public class AudioTimeSpan
{
    public double Start { get; set; }
    public double End { get; set; }
} 