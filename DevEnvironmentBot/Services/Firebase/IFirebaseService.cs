using System.Collections.Generic;
using System.Threading.Tasks;
using Firebase.Database;
using BatonBot.Models;

namespace BatonBot.Firebase
{
    public interface IFirebaseService
    {
        Task UpdateQueue(FirebaseObject<BatonQueue> queue);
        void SaveQueue(BatonQueue queue);
        Task<IList<FirebaseObject<BatonQueue>>> GetQueues();
        Task<Queue<BatonRequest>> GetQueueForBaton(string baton); 
        Task<FirebaseObject<BatonQueue>> GetQueueFireObjectForBaton(string batonName);
    }
}
