using NAudio.Wave;
using System;

namespace AudioTranscriptionApi.Services;

public class AudioProcessingService
{
    public string ConvertToCompatibleFormat(string inputFilePath)
    {
        var outputFilePath = Path.Combine(
            Path.GetDirectoryName(inputFilePath)!,
            "converted_" + Path.GetFileName(inputFilePath)
        );

        using (var reader = new WaveFileReader(inputFilePath))
        {
            // Check if the audio format needs conversion
            if (NeedsConversion(reader.WaveFormat))
            {
                // Create a new format that's compatible with Azure Speech Service
                var newFormat = new WaveFormat(16000, 16, 1); // 16kHz, 16-bit, mono

                using (var resampler = new MediaFoundationResampler(reader, newFormat))
                {
                    WaveFileWriter.CreateWaveFile(outputFilePath, resampler);
                }
                return outputFilePath;
            }
        }

        return inputFilePath;
    }

    private bool NeedsConversion(WaveFormat format)
    {
        // Azure Speech Service prefers:
        // - 16kHz sample rate
        // - 16-bit depth
        // - Mono channel
        return format.SampleRate != 16000 ||
               format.BitsPerSample != 16 ||
               format.Channels != 1;
    }
}