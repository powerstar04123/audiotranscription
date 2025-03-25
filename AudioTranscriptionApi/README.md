# Audio Transcription API

This is a .NET 9 Web API that provides an endpoint for transcribing WAV audio files using Azure Speech Services and Azure OpenAI.

## Prerequisites

- .NET 9 SDK
- Azure Speech Service subscription
- Azure Blob Stroage subscription

## Setup

1. Clone the repository
2. Navigate to the project directory
3. Update the `appsettings.json` file with your Azure credentials:
   - Azure Speech Service Key and Region
   - Azure Blob Storage Account Key and Endpoint
4. Run the following commands:
   ```bash
   dotnet restore
   dotnet build
   dotnet run
   ```

## API Usage

### Transcribe Audio

**Endpoint:** `POST /api/transcription/`

**Request Body:**
```json
{
    "blobName": "wav-filename",
    "language": "es-ES"
}
```

**Response:**
```json
{
    "transcription": [
    {
        "speaker": "Guest-1",
        "timespan": {
            "start": 17.03,
            "end": 30.67
        },
        "text": "¿Saludo, es que tengo una duda para ver desde de porque si tuve un accidente, verdad, EH? ¿De desde si hice la querella, cuánto tiempo tengo para presentárselo a ustedes y poder reclamar?"
    },
    ...
    ]
}
```

## Notes

- The API accepts WAV files
- The default language is "es-ES" but can be changed in the request
- The API includes CORS support for cross-origin requests