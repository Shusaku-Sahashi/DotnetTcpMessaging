using System;

namespace MessageHub.Models
{
    public record UserId(Guid Id);
    public class User
    {
        public UserId UserId { get; set; }
        public string UserName { get; set; }
    }
}