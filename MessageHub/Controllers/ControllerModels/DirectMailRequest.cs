using System;

namespace MessageHub.Controllers.ControllerModels
{
    public class DirectMailRequest
    {
        public string SendUserId { get; set; }
        public string Content { get; set; }
        public DateTimeOffset SendAt { get; set; }
    }
}