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
        private readonly ClientHub _hub;
        private static ChannelService _channelService;
        private static PostService _postService;

        public MessageController(ClientHub hub, ChannelService channelService, PostService postService)
        {
            _hub = hub;
            _channelService = channelService;
            _postService = postService;
        }

        [HttpPost("{userId}")]
        public async Task<StatusCodeResult> PushDirectMail(string userId, DirectMailRequest request)
        {
            if (!Guid.TryParse(userId, out var parsedRecipientId)) return BadRequest();
            var recipientId = new UserId(parsedRecipientId);

            if (!Guid.TryParse(request.SendUserId, out var parsedSenderId)) return BadRequest();
            var senderId = new UserId(parsedSenderId);
            
            // TODO: senderId, recipientId の確認

            // TODO: Postを保存して、ユーザにリアルタイムで送信する処理をまとめる。
            var post = new Post() {
                MessageId = new MessageId(Guid.NewGuid()),
                Message = request.Content,
                RecipientId = recipientId,
                RootId = new MessageId(Guid.NewGuid()),
                SenderId = senderId,
                SendAt = request.SendAt,
            };
            _postService.StorePost(post);

            await _hub.TrySend(senderId.Id.ToString(), new ListenMessageResponse()
            {
                MessageId = post.MessageId.ToString(),
                Message = post.Message,
                SendAt = Timestamp.FromDateTimeOffset(post.SendAt),
                RootId = post.RootId?.ToString(),
                RecipientUserId = post.RecipientId.ToString(),
                SenderUserId = post.SenderId.ToString(),
            });

            return Ok();
        }

        [HttpPost("channel/{channelId}")]
        public async Task<StatusCodeResult> PushChannel(string channelId, DirectMailRequest request)
        {
            if (!Guid.TryParse(request.SendUserId, out var parsedUserId)) return BadRequest();
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

            return Ok();
        }
    }
}