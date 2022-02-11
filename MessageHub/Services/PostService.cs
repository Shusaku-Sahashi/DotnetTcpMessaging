using System.Collections.Generic;
using MessageHub.Models;
using MessageHub.Repositories;

namespace MessageHub.Services
{
    public class PostService
    {
        private PostRepository _postRepository;
        private UserRepository _userRepository;

        public PostService(PostRepository postRepository, UserRepository userRepository)
        {
            _postRepository = postRepository;
            _userRepository = userRepository;
        }

        public IEnumerable<UserId> SendDirectMail(Post post)
        {
        }
    }
}