using System;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using MessageHub.Models;
using Microsoft.Extensions.Logging;

namespace MessageHub.Services
{
    public class MessageService : Messenger.MessengerBase
    {
        private readonly ILogger<MessageService> _logger;
        private readonly ClientHub _hub;

        public MessageService(ILogger<MessageService> logger, ClientHub hub)
        {
            _logger = logger;
            _hub = hub;
        }

        public override async Task StartListenMessage(ListenMessageRequest request, IServerStreamWriter<ListenMessageResponse> responseStream,
            ServerCallContext context)
        {
            // TODO: Errorの場合、gRPCで返すかを確認する。
            if (_hub.HasJoined(request.ConnectUserId)) throw new Exception();
            if (!_hub.TryJoin(request.ConnectUserId, responseStream)) throw new Exception();

            _logger.LogInformation($"{context.Peer} is connected.");

            try
            {
                while (!context.CancellationToken.IsCancellationRequested)
                {
                    // Waiting for disconnect.
                    await Task.Delay(Timeout.Infinite, context.CancellationToken);
                }
            }
            catch (TaskCanceledException)
            {
                _logger.LogInformation($"{context.Peer} is disconnected.");
            }
            finally
            {
                _hub.Remove(request.ConnectUserId);
            }
        }
    }
}