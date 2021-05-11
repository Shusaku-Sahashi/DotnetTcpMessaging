using System;
using System.Collections.Generic;
using MessageHub.Models;

namespace MessageHub.Services
{
    public class ChannelService
    {
        private static Models.User[] users = {
            new Models.User(){UserId = new UserId(Guid.Parse("16bfc58a-10ed-419e-c123-08e255198b8a")), UserName = "User1"},
            new Models.User(){UserId = new UserId(Guid.Parse("137858a7-c1ca-657a-5fc7-8222abed4f9d")), UserName = "User2"},
            new Models.User(){UserId = new UserId(Guid.Parse("6e9150a9-9568-0c36-f0ad-2b53d5345671")), UserName = "User3"},
            new Models.User(){UserId = new UserId(Guid.Parse("b7ae6e51-ed55-9b46-a183-785e8f7c297a")), UserName = "User4"},
            new Models.User(){UserId = new UserId(Guid.Parse("ba7c9467-a8b9-86a5-34ce-b995dce42b41")), UserName = "User5"},
        };
        public IEnumerable<Models.User> JoiningUsers(string channelName)
        {
            return users;
        }
    }
}