using KIDIO.Business.DTOs.TextToSpeech;
using KIDIO.Business.Interfaces;
using KIDIO.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KIDIO.API.Controllers;

[ApiController]
[Route("api/tts")]
[Authorize]
public class TextToSpeechController : ControllerBase
{
    private readonly ITextToSpeechService _ttsService;

    public TextToSpeechController(ITextToSpeechService ttsService)
    {
        _ttsService = ttsService;
    }

    [HttpPost("synthesize")]
    public async Task<IActionResult> Synthesize(
        [FromBody] TextToSpeechSynthesizeRequest request,
        CancellationToken ct)
    {
        var result = await _ttsService.SynthesizeAsync(request, ct);
        return Ok(ApiResponse<TextToSpeechGeneratedResponse>.Ok(result));
    }

    [HttpPost("lesson/{lessonId:guid}")]
    public async Task<IActionResult> SynthesizeLesson(
        Guid lessonId,
        [FromBody] TextToSpeechOptionsRequest request,
        CancellationToken ct)
    {
        var result = await _ttsService.SynthesizeLessonAsync(lessonId, request, ct);
        return Ok(ApiResponse<TextToSpeechGeneratedResponse>.Ok(result));
    }

    [HttpGet("voices")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<List<TextToSpeechVoiceResponse>>>> GetVoices(CancellationToken ct)
    {
        var result = await _ttsService.GetVoicesAsync(ct);
        return Ok(ApiResponse<List<TextToSpeechVoiceResponse>>.Ok(result));
    }
}