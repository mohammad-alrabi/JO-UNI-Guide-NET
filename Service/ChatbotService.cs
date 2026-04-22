using System.Text;
using System.Text.Json;

namespace JO_UNI_Guide.Service
{
    public class ChatbotService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ChatbotService> _logger;

        public ChatbotService(HttpClient client, IConfiguration config, ILogger<ChatbotService> logger)
        {
            _httpClient = client;
            _logger = logger;

            var apiKey = config["OpenAI:ApiKey"]
                ?? throw new Exception("Open Ai Key is missing");
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
            _httpClient.Timeout = TimeSpan.FromSeconds(30); 
        }

        public async Task<string> AskAsync(string userMessage , string studentName)
        {
            var requestBody = new
            {
               model = "gpt-4o-mini",
               messages = new object[]
               {
                 new {
                     role = "system",
                     content = $"You are GuideBot, an academic advisor for JO-UNI Guide. You are talking to a student named {studentName}. Be friendly, concise, and helpful."
                 },
                 new {
                      role = "user",
                      content = userMessage
                 }
               },
                temperature = 0.7,
                max_tokens = 300
            };

            try
            {
                var requestJson = JsonSerializer.Serialize(requestBody);

                var response = await _httpClient.PostAsync(
                    "https://api.openai.com/v1/chat/completions",
                    new StringContent(requestJson, Encoding.UTF8, "application/json")
                );

                var responseString = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("OpenAI Error: {Status} {Body}", response.StatusCode, responseString);
                    return "AI service is currently unavailable.";
                }

                using var doc = JsonDocument.Parse(responseString);

                return doc.RootElement
                    .GetProperty("choices")[0]
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString()
                    ?.Trim() ?? "No response.";
            }
            catch (TaskCanceledException)
            {
                _logger.LogWarning("OpenAI timeout");
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