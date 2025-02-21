using Newtonsoft.Json;

namespace BotBaoKhach.Models
{
    public class FromWebhook
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("is_bot")]
        public bool IsBot { get; set; }

        [JsonProperty("first_name")]
        public string FirstName { get; set; }

        [JsonProperty("username")]
        public string Username { get; set; }

        [JsonProperty("last_name")]
        public string LastName { get; set; }

    }

    public class SenderChatWebhook
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("username")]
        public string Username { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }
    }

    public class ChatWebhook
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }
    }

    public class ForwardOriginWebhook
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("chat")]
        public ChatWebhook Chat { get; set; }

        [JsonProperty("message_id")]
        public int MessageId { get; set; }

        [JsonProperty("date")]
        public int Date { get; set; }
    }

    public class MessageWebhook
    {
        [JsonProperty("message_id")]
        public int MessageId { get; set; }

        [JsonProperty("from")]
        public FromWebhook From { get; set; }

        [JsonProperty("sender_chat")]
        public SenderChatWebhook SenderChat { get; set; }

        [JsonProperty("chat")]
        public ChatWebhook Chat { get; set; }

        [JsonProperty("date")]
        public int Date { get; set; }

        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("forward_origin")]
        public ForwardOriginWebhook ForwardOrigin { get; set; }

        [JsonProperty("is_automatic_forward")]
        public bool IsAutomaticForward { get; set; }

        [JsonProperty("forward_from_chat")]
        public SenderChatWebhook ForwardFromChat { get; set; }

        [JsonProperty("forward_from_message_id")]
        public int ForwardFromMessageId { get; set; }

        [JsonProperty("forward_date")]
        public int ForwardDate { get; set; }

        [JsonProperty("message_thread_id")]
        public int MessageThreadId { get; set; }

        [JsonProperty("reply_to_message")]
        public MessageWebhook ReplyToMessage { get; set; }
    }

    public class ChannelPostWebhook
    {
        [JsonProperty("message_id")]
        public int MessageId { get; set; }

        [JsonProperty("chat")]
        public ChatWebhook Chat { get; set; }

        [JsonProperty("date")]
        public int Date { get; set; }

        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("sender_chat")]
        public SenderChatWebhook SenderChat { get; set; }
    }

    public class WebhookDataDto
    {
        [JsonProperty("update_id")]
        public int UpdateId { get; set; }

        [JsonProperty("message")]
        public MessageWebhook Message { get; set; }

        [JsonProperty("channel_post")]
        public ChannelPostWebhook ChannelPost { get; set; }
    }
}
