using Firebase.Database;
using SharedBaton.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Firebase.Auth;
using Firebase.Database.Query;
using Microsoft.Extensions.Configuration;

namespace SharedBaton.Firebase
{
    public class FirebaseService : IFirebaseService
    {
        private string firebaseApiKey;
        private string firebaseUserId;
        private string firebaseUrl;
        private string firebaseLogin;
        private string firebasePassword;

        private string queueId;

        public FirebaseService(IConfiguration config)
        {
            this.firebaseApiKey = config["FirebaseApiKey"];
            this.firebaseUserId = config["FirebaseUserId"];
            this.firebaseUrl = config["FirebaseUrl"];
            this.firebaseLogin = config["FirebaseLogin"];
            this.firebasePassword = config["FirebasePassword"];
            this.queueId = config["QueueId"];
        }

        public async Task UpdateQueue(FirebaseObject<BatonQueue> queue)
        {
            var auth = new FirebaseAuthProvider(new FirebaseConfig(firebaseApiKey));
            var token = await auth.SignInWithEmailAndPasswordAsync(firebaseLogin, firebasePassword);

            var firebaseClient = new FirebaseClient(firebaseUrl, new FirebaseOptions
            {
                AuthTokenAsyncFactory = () => Task.FromResult(token.FirebaseToken)
            });

            await firebaseClient
                .Child("Users")
                .Child(firebaseUserId)
                .Child(this.queueId)
                .Child(queue.Key)
                .PutAsync(queue.Object);
        }

        public async void SaveQueue(BatonQueue queue)
        {
            var auth = new FirebaseAuthProvider(new FirebaseConfig(firebaseApiKey));
            var token = await auth.SignInWithEmailAndPasswordAsync(firebaseLogin, firebasePassword);

            var firebaseClient = new FirebaseClient(firebaseUrl, new FirebaseOptions
            {
                AuthTokenAsyncFactory = () => Task.FromResult(token.FirebaseToken)
            });

            await firebaseClient
                .Child("Users")
                .Child(firebaseUserId)
                .Child(this.queueId)
                .PostAsync(queue);
        }

        public async Task<Queue<BatonRequest>> GetQueueForBaton(string batonName)
        {
            var batons = await this.GetQueues();
            var baton = batons.FirstOrDefault(x => x.Object.Name == batonName);
            return baton?.Object.Queue;
        }

        public async Task<FirebaseObject<BatonQueue>> GetQueueFireObjectForBaton(string batonName)
        {
            var batons = await this.GetQueues();
            return batons?.FirstOrDefault(x => x.Object.Name.Equals(batonName));
        }

        public async Task<IList<FirebaseObject<BatonQueue>>> GetQueues()
        {
            var auth = new FirebaseAuthProvider(new FirebaseConfig(firebaseApiKey));
            var token = await auth.SignInWithEmailAndPasswordAsync(firebaseLogin, firebasePassword);

            var firebaseClient = new FirebaseClient(firebaseUrl, new FirebaseOptions
            {
                AuthTokenAsyncFactory = () => Task.FromResult(token.FirebaseToken)
            });

            var queues = await firebaseClient
                .Child("Users")
                .Child(firebaseUserId)
                .Child(this.queueId)
                .OnceAsync<BatonQueue>();

            return queues.ToList();
        }
    }
}
