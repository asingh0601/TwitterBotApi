using Newtonsoft.Json;

namespace TwitterBotApi.Models
{
    public class Chat
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("first_name")]
        public string? FirstName { get; set; }

        [JsonProperty("last_name")]
        public string? LastName { get; set; }

        [JsonProperty("username")]
        public string? Username { get; set; }

        [JsonProperty("type")]
        public string? Type { get; set; }
    }

    public class From
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("is_bot")]
        public bool IsBot { get; set; }

        [JsonProperty("first_name")]
        public string? FirstName { get; set; }

        [JsonProperty("last_name")]
        public string? LastName { get; set; }

        [JsonProperty("username")]
        public string? Username { get; set; }

        [JsonProperty("language_code")]
        public string? LanguageCode { get; set; }
    }

    public class Message
    {
        [JsonProperty("message_id")]
        public long MessageId { get; set; }

        [JsonProperty("from")]
        public From? From { get; set; }

        [JsonProperty("chat")]
        public Chat? Chat { get; set; }

        [JsonProperty("date")]
        public long Date { get; set; }

        [JsonProperty("text")]
        public string? Text { get; set; }
    }

    public class WebHookUpdate
    {
        [JsonProperty("update_id")]
        public long UpdateId { get; set; }

        [JsonProperty("message")]
        public Message? Message { get; set; }
    }
}

