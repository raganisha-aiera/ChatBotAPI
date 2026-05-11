using Microsoft.AspNetCore.Mvc;
using ChatbotAPI.Models;
using ChatbotAPI.Services;

namespace ChatbotAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly IOpenAIService _openAIService;
        private readonly ILogger<ChatController> _logger;

        public ChatController(IOpenAIService openAIService, ILogger<ChatController> logger)
        {
            _openAIService = openAIService;
            _logger = logger;
        }

        [HttpPost("message")]
        public async Task<IActionResult> SendMessage([FromBody] ChatRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Message))
                return BadRequest(new { error = "Message cannot be empty." });

            try
            {
                var reply = await _openAIService.GetChatResponseAsync(
                    request.Message,
                    request.ConversationHistory
                );

                return Ok(new ChatResponse
                {
                    Reply = reply,
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling OpenAI API");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("health")]
        public IActionResult Health() => Ok(new { status = "running", timestamp = DateTime.UtcNow });
    }
}