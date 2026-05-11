using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using ChatbotAPI.Models;

namespace ChatbotAPI.Services
{
    public interface IOpenAIService
    {
        Task<string> GetChatResponseAsync(string userMessage, List<ConversationMessage> history);
    }

    public class OpenAIService : IOpenAIService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;

        private const string SYSTEM_PROMPT =
            "You are a helpful, friendly AI assistant. " +
            "Answer clearly and concisely.";

        public OpenAIService(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _config = config;

            var apiKey = _config["Groq:ApiKey"]
                ?? throw new InvalidOperationException("OpenAI API key not configured.");

            _httpClient.BaseAddress = new Uri("https://api.groq.com/openai/");
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", apiKey);
        }

        public async Task<string> GetChatResponseAsync(string userMessage, List<ConversationMessage> history)
        {
            var messages = new List<object>
            {
                new { role = "system", content = SYSTEM_PROMPT }
            };

            foreach (var msg in history)
                messages.Add(new { role = msg.Role, content = msg.Content });

            messages.Add(new { role = "user", content = userMessage });

            var requestBody = new
            {
                model = "llama-3.3-70b-versatile",
                messages,
                max_tokens = 1000,
                temperature = 0.7
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("v1/chat/completions", content);

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                throw new Exception($"Groq Error: {errorBody}");
            }

            var responseJson = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(responseJson);

            var reply = doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

            return reply ?? "Sorry, I could not generate a response.";
        }
    }
}