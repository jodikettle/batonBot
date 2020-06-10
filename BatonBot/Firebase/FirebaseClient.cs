using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BatonBot.Firebase;
using BatonBot.Models;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Database.Query;
using Microsoft.Extensions.Configuration;

namespace BatonBot
{
    public class FirebaseServiceClient : IFirebaseClient
    {
        private string firebaseApiKey;
        private string firebaseUserId;
        private string firebaseUrl;
        private string firebaseLogin;
        private string firebasePassword;

        public FirebaseServiceClient(IConfiguration config)
        {
            this.firebaseApiKey = config["FirebaseApiKey"];
            this.firebaseUserId = config["FirebaseUserId"];
            this.firebaseUrl = config["FirebaseUrl"];
            this.firebaseLogin = config["FirebaseLogin"];
            this.firebasePassword = config["FirebasePassword"];
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
                .PostAsync(queue);
        }

        public async Task<IList<FirebaseObject<BatonQueue>>> GetQueue()
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
                .OnceAsync<BatonQueue>();

            return queues.ToList();
        }
    }
}
