namespace SharedBaton.Firebase
{
    using SharedBaton.Interfaces;
    using System;
    using System.Threading.Tasks;
    using global::Firebase.Auth;
    using global::Firebase.Database;
    using global::Firebase.Database.Query;
    using Microsoft.Extensions.Configuration;
    using SharedBaton.Models;

    public class FirebaseLogger : IFirebaseLogger
    {
        private string firebaseApiKey;
        private string firebaseUserId;
        private string firebaseLogUrl;
        private string firebaseLogin;
        private string firebasePassword;

        public FirebaseLogger(IConfiguration config)
        {
            this.firebaseApiKey = config["FirebaseLogApiKey"];
            this.firebaseUserId = config["FirebaseLogUserId"];
            this.firebaseLogUrl = config["FirebaseLogsUrl"];
            this.firebaseLogin = config["FirebaseLogsLogin"];
            this.firebasePassword = config["FirebaseLogsPassword"];
        }

        public async void Log(string queueId, string batonName, string name, DateTime dateRequested, DateTime? dateReceived,
            DateTime dateReleased,int moveMeCount,bool pncaked)
        {
            var auth = new FirebaseAuthProvider(new FirebaseConfig(firebaseApiKey));
                var token = await auth.SignInWithEmailAndPasswordAsync(firebaseLogin, firebasePassword);

                var firebaseClient = new FirebaseClient(firebaseLogUrl, new FirebaseOptions
                {
                    AuthTokenAsyncFactory = () => Task.FromResult(token.FirebaseToken)
                });

            /*
             * Requested DateTime
                Received DateTime
                Released DateTime
                Moveme use count
             */

            var log = new LogItem()
                {
                    QueueId = queueId,
                    BatonName = batonName,
                    User = name,
                    DateRequestedTime = dateRequested,
                    DateReceivedTime = dateReceived,
                    DateReleasedTime = dateReleased,
                    MoveMeCount = moveMeCount
                };

            await firebaseClient
                .Child("Users")
                .Child(firebaseUserId)
                .PostAsync(log);
        }
    }
}
