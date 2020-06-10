﻿using System;
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
    }
}
