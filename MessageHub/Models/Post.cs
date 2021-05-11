using System;

namespace MessageHub.Models
{
    public record MessageId(Guid Id);

    public class Post
    {
        public MessageId MessageId { get; set; }
        public UserId SenderId { get; set; } 
        public UserId RecipientId { get; set; }
        public DateTimeOffset SendAt { get; set; }
        public string Message { get; set; }
        public MessageId RootId { get; set; }
        public ChannelId ChannelId { get; set; }
    }
}