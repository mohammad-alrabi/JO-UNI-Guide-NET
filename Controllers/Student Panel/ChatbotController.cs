using JO_UNI_Guide.Models;
using JO_UNI_Guide.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace JO_UNI_Guide.Controllers.Student_Panel
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Student")]
    public class ChatbotController : ControllerBase
    {
        private readonly ChatbotService _chatbotService;
        private readonly ILogger<ChatbotController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;

        public ChatbotController(
            ChatbotService chatbotService,
            ILogger<ChatbotController> logger,
            UserManager<ApplicationUser> userManager)
        {
            _chatbotService = chatbotService;
            _logger = logger;
            _userManager = userManager;
        }

        public class ChatRequestDto
        {
            public string Message { get; set; } = string.Empty;
        }

        [HttpPost]
        public async Task<IActionResult> Ask([FromBody] ChatRequestDto request)
        {
            // 🔴 Validation
            if (request == null || string.IsNullOrWhiteSpace(request.Message))
            {
                _logger.LogWarning("Empty chat message received");
                return BadRequest(new { success = false, reply = "Message cannot be empty." });
            }

            if (request.Message.Length > 500)
            {
                _logger.LogWarning("Too long message received: {Length}", request.Message.Length);
                return BadRequest(new { success = false, reply = "Message is too long (max 500 characters)." });
            }

            try
            {
                var user = await _userManager.GetUserAsync(User);
                var studentName = user?.Name ?? "Student";

                var reply = await _chatbotService.AskAsync(request.Message, studentName);

                return Ok(new
                {
                    success = true,
                    reply
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Chatbot unexpected error");
                return StatusCode(500, new
                {
                    success = false,
                    reply = "Server error occurred. Please try again later."
                });
            }
        }
    }
}