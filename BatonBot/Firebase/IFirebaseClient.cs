
using System.Collections.Generic;
using System.Threading.Tasks;
using BatonBot.Models;
using Firebase.Database;

namespace BatonBot.Firebase
{
    public interface IFirebaseClient
    {
        Task UpdateQueue(FirebaseObject<BatonQueue> queue);
        void SaveQueue(BatonQueue queue);

        Task<IList<FirebaseObject<BatonQueue>>> GetQueue();
    }
}
