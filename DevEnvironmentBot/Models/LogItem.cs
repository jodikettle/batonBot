namespace BatonBot.Models
{
    using System;

    public class LogItem
    {
        public string QueueId { get; set; }
        public string BatonName { get; set; }
        public string User { get; set; }
        public DateTime DateRequestedTime { get; set; }
        public DateTime? DateReceivedTime { get; set; }
        public DateTime DateReleasedTime { get; set; }
        public int MoveMeCount { get; set; }
        public bool Pancaked { get; set; }
    }
}
