public async Task<TranscriptionResponse> TranscribeAudioAsync(TranscriptionRequest request)
{
    try
    {
        // Download the WAV file from blob storage
        var audioFilePath = await _blobStorageService.DownloadAndSaveWavFileAsync(request.BlobName);

        // Convert audio format if needed
        var processedAudioPath = _audioProcessingService.ConvertToCompatibleFormat(audioFilePath);
        if (!File.Exists(processedAudioPath))
        {
            throw new FileNotFoundException($"The file '{processedAudioPath}' does not exist.");
        }

        // Create conversation transcription config
        var config = SpeechConfig.FromSubscription(_speechKey, _speechRegion);
        config.SetProperty(PropertyId.SpeechServiceConnection_InitialSilenceTimeoutMs, "30000");
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
            if (e.Reason == CancellationReason.Error)
            {
                throw new Exception($"Transcription canceled: {e.ErrorDetails}");
            }
            stopTranscription.TrySetResult(0);
        };

        // Handle session end
        transcriber.SessionStopped += (s, e) =>
        {
            stopTranscription.TrySetResult(0);
        };

        await transcriber.StartTranscribingAsync();

        // Wait for transcription to complete
        await stopTranscription.Task;

        // Stop transcription
        await transcriber.StopTranscribingAsync();

        // Return transcription response
        return new TranscriptionResponse
        {
            Transcription = transcription.OrderBy(t => t.Timespan.Start).ToList()
        };
    }
    catch (Exception ex)
    {
        // throw new Exception($"Error during transcription: {ex.Message}", ex);
    }
}