using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using MessageHub.Controllers.ControllerModels;
using MessageHub.Models;
using MessageHub.Services;
using Microsoft.AspNetCore.Mvc;

namespace MessageHub.Controllers
{
    [ApiController]
    [Route("message")]
    public class MessageController : ControllerBase
    {
        private readonly ChannelService _channelService;
        private readonly PostService _postService;
        private readonly ClientHub _hub;

        public MessageController(ClientHub hub, ChannelService channelService, PostService postService)
        {
            _hub = hub;
            _channelService = channelService;
            _postService = postService;
        }

        [HttpPost("{userId}")]
        public Task<StatusCodeResult> PushDirectMail(string userId, DirectMailRequest request)
        {
            if (!Guid.TryParse(userId, out var parsedRecipientId)) 
                return Task.FromResult<StatusCodeResult>(BadRequest());
            var recipientId = new UserId(parsedRecipientId);

            if (!Guid.TryParse(request.SendUserId, out var parsedSenderId)) 
                return Task.FromResult<StatusCodeResult>(BadRequest());
            var senderId = new UserId(parsedSenderId);

            var post = new Post
            {
                MessageId = new MessageId(Guid.NewGuid()),
                Message = request.Content,
                RecipientId = recipientId,
                RootId = new MessageId(Guid.NewGuid()),
                SenderId = senderId,
                SendAt = request.SendAt,
            };

            _postService.SendDirectMail(post);

            return Task.FromResult<StatusCodeResult>(Ok());
        }

        [HttpPost("channel/{channelId}")]
        public Task<StatusCodeResult> PushChannel(string channelId, DirectMailRequest request)
        {
            if (!Guid.TryParse(request.SendUserId, out var parsedUserId))
                return Task.FromResult<StatusCodeResult>(BadRequest());
            var senderId = new UserId(parsedUserId);

            IList<UserId> unActiveUsers = new List<UserId>();
            foreach (var recipient in _channelService.JoiningUsers(channelId).Where(x => x.UserId != senderId))
            {
                // var hasSend = await _hub.TrySend(recipient.UserId.Id.ToString(), new ListenMessageResponse()
                // {
                //     MessageId = recipient.Message = post.Message,
                //     SendAt = Timestamp.FromDateTimeOffset(post.SendAt),
                //     RootId = post.RootId.ToString(),
                //     RecipientUserId = post.RecipientId.ToString(),
                //     SenderUserId = post.SenderId.ToString(),
                // });
                //
                // if (!hasSend) unActiveUsers.Add(recipient.UserId);
            }

            // TODO: 他のサーバーに探しにいく。

            return Task.FromResult<StatusCodeResult>(Ok());
        }
    }
}