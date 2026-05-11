namespace ChatbotAPI.Models
{
    public class ChatRequest
    {
        public string Message { get; set; } = string.Empty;
        public List<ConversationMessage> ConversationHistory { get; set; } = new();
    }

    public class ConversationMessage
    {
        public string Role { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
    }

    public class ChatResponse
    {
        public string Reply { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }
}