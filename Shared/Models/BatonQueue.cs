using System;
using System.Collections.Generic;

namespace SharedBaton.Models
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
