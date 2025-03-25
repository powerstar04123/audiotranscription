# Audio Transcription API

This is a .NET 9 Web API that provides an endpoint for transcribing WAV audio files using Azure Speech Services and Azure OpenAI.

## Prerequisites

- .NET 9 SDK
- Azure Speech Service subscription
- Azure OpenAI subscription

## Setup

1. Clone the repository
2. Navigate to the project directory
3. Update the `appsettings.json` file with your Azure credentials:
   - Azure Speech Service Key and Region
   - Azure OpenAI Key and Endpoint
4. Run the following commands:
   ```bash
   dotnet restore
   dotnet build
   dotnet run
   ```

## API Usage

### Transcribe Audio

**Endpoint:** `POST /api/transcription/transcribe`

**Request Body:**
```json
{
    "audioFileBase64": "base64-encoded-wav-file",
    "language": "en-US"
}
```

**Response:**
```json
{
    "text": "Transcribed text",
    "language": "en-US",
    "duration": 1.234,
    "segments": [
        {
            "text": "Segment text",
            "startTime": 0.0,
            "endTime": 1.234,
            "confidence": 0.95
        }
    ]
}
```

## Notes

- The API accepts WAV files encoded in base64 format
- The default language is "en-US" but can be changed in the request
- The API includes CORS support for cross-origin requests
- Swagger UI is available at `/swagger` when running in development mode 