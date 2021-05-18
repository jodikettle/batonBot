using System.Collections.Generic;

namespace BatonBot.Models
{
    public class BatonQueue
    {
        public string Name;

        public Queue<BatonRequest> Queue;

        public BatonQueue(string name)
        {
            this.Name = name;
            this.Queue = new Queue<BatonRequest>();
        }
    }
}
