using Microsoft.AspNetCore.Mvc;
using AudioTranscriptionApi.Models;
using AudioTranscriptionApi.Services;

namespace AudioTranscriptionApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TranscriptionController : ControllerBase
{
    private readonly TranscriptionService _transcriptionService;

    public TranscriptionController(TranscriptionService transcriptionService)
    {
        _transcriptionService = transcriptionService;
    }

    [HttpPost("transcribe")]
    public async Task<ActionResult<TranscriptionResponse>> TranscribeAudio([FromBody] TranscriptionRequest request)
    {
        try
        {
            var response = await _transcriptionService.TranscribeAudioAsync(request);
            return Ok(response);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
} 