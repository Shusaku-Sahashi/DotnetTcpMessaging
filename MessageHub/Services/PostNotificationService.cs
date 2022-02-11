using System.Collections.Generic;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using MessageHub.Models;

namespace MessageHub.Services
{
    public class PostNotificationService
    {
        private ClientHub _hub;

        public PostNotificationService(ClientHub hub)
        {
            _hub = hub;
        }

        public async Task SendNotification(Post post)
        {
            await _hub.TrySend(post.RecipientId.Id.ToString(), new ListenMessageResponse()
            {
                MessageId = post.MessageId.ToString(),
                Message = post.Message,
                SendAt = Timestamp.FromDateTimeOffset(post.SendAt),
                RootId = post.RootId?.ToString(),
                RecipientUserId = post.RecipientId.ToString(),
                SenderUserId = post.SenderId.ToString(),
            });
        }

        public async Task SendNotifications(IEnumerable<Post> posts)
        {
            foreach (var post in posts) await SendNotification(post);
        }
    }
}