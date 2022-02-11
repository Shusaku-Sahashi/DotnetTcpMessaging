using System;
using System.Collections.Concurrent;
using MessageHub.Models;

namespace MessageHub.Repositories
{
    public class PostRepository
    {
        private ConcurrentDictionary<Guid, Post> _datas = new ConcurrentDictionary<Guid, Post>();

        public PostList BelongingPostList(MessageId rootId)
        {
            
        }

        public 
    }
}