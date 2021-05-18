using System;
using Microsoft.Bot.Schema;

namespace BatonBot.Models
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
        public int MoveMeCount;
        public int PullRequestNumber;
    }
}
