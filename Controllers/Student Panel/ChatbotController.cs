using JO_UNI_Guide.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace JO_UNI_Guide.Controllers.Student_Panel
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles ="Student")]
    public class ChatbotController : ControllerBase
    {
        private readonly ChatbotService _chatbotService;
        private readonly ILogger<ChatbotController> _logger;
        public ChatbotController (
                ChatbotService chatbotService,
                ILogger<ChatbotController>logger) 
        {
             _logger = logger;
            _chatbotService = chatbotService;
            
        }
        public class ChatRequestDto
        {
            public string? Message { get; set; }
        }
       
        [HttpPost]
        public async Task<IActionResult> Ask([FromBody] ChatRequestDto request)
        {
            if (string.IsNullOrWhiteSpace(request.Message))
            {
                return BadRequest(new { success = false, reply = "The Message empty" });
            }
            if (request.Message.Length > 500)
            {
                return BadRequest(new { reply = "The message is too long" });
            }

            try
            {
                var reply = await _chatbotService.AskAsync(request.Message);
                return Ok(new { success = true, reply });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Chatbot error");
                return StatusCode(500, new { success = false, reply = "There was a server error." });
            }
        }
    }
}
