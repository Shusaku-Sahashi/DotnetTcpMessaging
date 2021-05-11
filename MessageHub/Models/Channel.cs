using System;

namespace MessageHub.Models
{
    public record ChannelId(Guid Id);

    public class Channel
    {
        public ChannelId ChannelId { get; set; }
        public string Name { get; set; }
    }
}