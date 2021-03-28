using System;
using Microsoft.Bot.Schema;

namespace SharedBaton.Models
{
    public class BatonRequest
    {
        public string UserName;
        public string UserId;
        public DateTime DateRequested;
        public DateTime? DateReceived;
        public ConversationReference Conversation;
        public string BatonName;
        public string Comment;
    }
}
