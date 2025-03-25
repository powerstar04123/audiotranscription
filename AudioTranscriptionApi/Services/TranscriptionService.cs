using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using Microsoft.CognitiveServices.Speech.Transcription;
using AudioTranscriptionApi.Models;
using System;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Linq;

namespace AudioTranscriptionApi.Services;

public class TranscriptionService
{
    private readonly string _speechKey;
    private readonly string _speechRegion;
    private readonly BlobStorageService _blobStorageService;
    private readonly AudioProcessingService _audioProcessingService;

    public TranscriptionService(
        IConfiguration configuration,
        BlobStorageService blobStorageService,
        AudioProcessingService audioProcessingService)
    {
        _speechKey = configuration["AzureSpeech:Key"] ?? throw new ArgumentNullException("AzureSpeech:Key");
        _speechRegion = configuration["AzureSpeech:Region"] ?? throw new ArgumentNullException("AzureSpeech:Region");
        _blobStorageService = blobStorageService;
        _audioProcessingService = audioProcessingService;
    }

    public async Task<TranscriptionResponse> TranscribeAudioAsync(TranscriptionRequest request)
    {
        try
        {
            // Download the WAV file from blob storage
            Console.WriteLine($"Downloading audio file from blob storage: {request.BlobName}");
            var audioFilePath = await _blobStorageService.DownloadAndSaveWavFileAsync(request.BlobName);

            Console.WriteLine($"Audio file downloaded to: {audioFilePath}");

            // Convert audio format if needed
            Console.WriteLine("Checking audio format compatibility...");
            var processedAudioPath = _audioProcessingService.ConvertToCompatibleFormat(audioFilePath);
            if (processedAudioPath != audioFilePath)
            {
                Console.WriteLine("Audio format converted for compatibility");
            }

            // Create conversation transcription config
            var config = SpeechConfig.FromSubscription(_speechKey, _speechRegion);
            config.SetProperty(PropertyId.SpeechServiceConnection_InitialSilenceTimeoutMs, "30000");
            // config.SetProperty(PropertyId.Speech_SegmentationSilenceTimeoutMs, "30000");
            config.SetProperty(PropertyId.SpeechServiceConnection_EndSilenceTimeoutMs, "30000");
            config.SetServiceProperty("speechcontext", "{\"diarization\": {\"speakers\": {\"minCount\": \"1\", \"maxCount\": \"10\"}}}", ServicePropertyChannel.UriQueryParameter);
            config.SpeechRecognitionLanguage = request.Language ?? "es-ES";

            using var audioConfig = AudioConfig.FromWavFileInput(processedAudioPath);
            using var transcriber = new ConversationTranscriber(config, audioConfig);

            var transcription = new List<TranscriptionEntry>();
            var stopTranscription = new TaskCompletionSource<int>();

            // Handle transcription results
            transcriber.Transcribed += (s, e) =>
            {
                if (e.Result.Text.Length > 0)
                {
                    Console.WriteLine($"TRANSCRIBED: Speaker={e.Result.SpeakerId}, Text={e.Result.Text}");
                    var entry = new TranscriptionEntry
                    {
                        Speaker = e.Result.SpeakerId ?? "Unknown Speaker",
                        Timespan = new AudioTimeSpan
                        {
                            Start = e.Result.OffsetInTicks / 10000000.0,
                            End = (e.Result.OffsetInTicks + e.Result.Duration.Ticks) / 10000000.0
                        },
                        Text = e.Result.Text
                    };
                    transcription.Add(entry);
                }
            };

            // Handle errors
            transcriber.Canceled += (s, e) =>
            {
                Console.WriteLine($"Transcription canceled. Reason: {e.Reason}, ErrorDetails: {e.ErrorDetails}");
                if (e.Reason == CancellationReason.Error)
                {
                    throw new Exception($"Transcription canceled: {e.ErrorDetails}");
                }
                stopTranscription.TrySetResult(0);
            };

            // Handle session end
            transcriber.SessionStopped += (s, e) =>
            {
                Console.WriteLine("Session stopped.");
                stopTranscription.TrySetResult(0);
            };

            Console.WriteLine("Starting conversation transcription...");
            await transcriber.StartTranscribingAsync();

            // Wait for transcription to complete
            await stopTranscription.Task;

            // Stop transcription
            await transcriber.StopTranscribingAsync();
            Console.WriteLine("Transcription completed.");

            // Clean up temporary files
            if (processedAudioPath != audioFilePath)
            {
                File.Delete(processedAudioPath);
            }
            File.Delete(audioFilePath);

            return new TranscriptionResponse
            {
                Transcription = transcription.OrderBy(t => t.Timespan.Start).ToList()
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during transcription: {ex.Message}");
            throw new Exception($"Error during transcription: {ex.Message}", ex);
        }
    }
}