using System.Text;
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

            _apiKey = config["Gemini:ApiKey"]
                ?? throw new Exception("Gemini API Key is missing");

            _httpClient.Timeout = TimeSpan.FromSeconds(30);
        }

        public async Task<string> AskAsync(string userMessage, string studentName)
        {
            var requestBody = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new
                            {
                                text = $@"
You are GuideBot for JO-UNI Guide (Jordan University system).

Rules:
1. Respond ONLY in Arabic (Jordanian dialect preferred).
2. If user writes English, still respond in Arabic.
3. Be concise and student-friendly.
4. Give academic guidance only.

Student: {studentName}
Message: {userMessage}
"                                        
                            }
                        }
                    }
                }
            };

            try
            {
                var requestJson = JsonSerializer.Serialize(requestBody);

                var response = await _httpClient.PostAsync(
                    $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key={_apiKey}",
                    new StringContent(requestJson, Encoding.UTF8, "application/json")
                );

                var responseString = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Gemini Error: {Status} {Body}", response.StatusCode, responseString);
                    return "AI service is currently unavailable.";
                }

                using var doc = JsonDocument.Parse(responseString);

                return doc.RootElement
                    .GetProperty("candidates")[0]
                    .GetProperty("content")
                    .GetProperty("parts")[0]
                    .GetProperty("text")
                    .GetString()
                    ?.Trim() ?? "No response.";
            }
            catch (TaskCanceledException)
            {
                _logger.LogWarning("Gemini timeout");
                return "The request took too long. Try again.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error");
                return "Unexpected error occurred.";
            }
        }
    }
}