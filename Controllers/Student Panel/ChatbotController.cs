using JO_UNI_Guide.Models;
using JO_UNI_Guide.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Claims;
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
        private readonly UserManager<ApplicationUser> _userManager;

        public ChatbotController (
                ChatbotService chatbotService,
                ILogger<ChatbotController>logger,
                UserManager<ApplicationUser> userManager) 
        {
             _logger = logger;
            _chatbotService = chatbotService;
            _userManager = userManager; 
            
        }
        public class ChatRequestDto
        {
            public string? Message { get; set; } = string.Empty;
        }
        [HttpPost]
        public async Task<IActionResult> Ask([FromBody] ChatRequestDto request)
        {
            if (string.IsNullOrWhiteSpace(request.Message))
                return BadRequest(new { success = false, reply = "The Message empty" });

            if (request.Message.Length > 500)
                return BadRequest(new { success = false, reply = "The message is too long" });

            try
            {
                var user = await _userManager.GetUserAsync(User);
                var studentName = user?.Name ?? "Student";

                var reply = await _chatbotService.AskAsync(request.Message, studentName);
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
