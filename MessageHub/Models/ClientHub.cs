using System.Collections.Concurrent;
using System.Threading.Tasks;
using Grpc.Core;

namespace MessageHub.Models
{
    public class ClientHub
    {
        private readonly ConcurrentDictionary<string, IServerStreamWriter<ListenMessageResponse>> _users = new();

        public bool HasJoined(string userId) => _users.ContainsKey(userId);

        public bool TryJoin(string userId, IServerStreamWriter<ListenMessageResponse> streamWriter)
            => _users.TryAdd(userId, streamWriter);

        public void Remove(string userId)
            => _users.TryRemove(userId, out _);

        public async Task<bool> TrySend(string userId, ListenMessageResponse response)
        {
            if (!_users.TryGetValue(userId, out var user))
                return false;

            await user.WriteAsync(response);
            return true;
        }
    }
}