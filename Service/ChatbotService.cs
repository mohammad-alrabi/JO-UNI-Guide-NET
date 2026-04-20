using System.Text.Json;

namespace JO_UNI_Guide.Service
{
    public class ChatbotService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ChatbotService> _logger;
        private readonly string _apiKey;

        public ChatbotService(HttpClient client, IConfiguration config, ILogger<ChatbotService> logger)
        {
            _httpClient = client;
            _logger = logger;
            _apiKey = config["GeminiApi:Key"] ?? throw new Exception("Gemini API Key missing");
            _httpClient.Timeout = TimeSpan.FromSeconds(30); 
        }

        public async Task<string> AskAsync(string message)
        {
            var requestBody = new
            {
                system_instruction = new
                {
                    parts = new[] { new { text = "You are GuideBot, a professional academic advisor for JO-UNI Guide platform in Jordan. Keep answers concise." } }
                },
                contents = new[]
                {
                    new {
                        parts = new[] { new { text = message } }
                    }
                }
            };

            try
            {
                var response = await _httpClient.PostAsJsonAsync(
                    $"https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent?key={_apiKey}",
                    requestBody
                );

                var responseString = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Gemini API failed: {Status} {Response}", response.StatusCode, responseString);
                    throw new Exception("AI service temporarily unavailable.");
                }

                using var doc = JsonDocument.Parse(responseString);

                if (!doc.RootElement.TryGetProperty("candidates", out var candidates) || candidates.GetArrayLength() == 0)
                {
                    return "I couldn't get a reply right now, please try again.";
                }

                return candidates[0]
                    .GetProperty("content")
                    .GetProperty("parts")[0]
                    .GetProperty("text")
                    .GetString() ?? "There is no response at the moment.";
            }
            catch (TaskCanceledException)
            {
                _logger.LogWarning("Gemini API request timed out.");
                return "I'm taking too long to think! Please try asking again.";
            }
        }
    }
}