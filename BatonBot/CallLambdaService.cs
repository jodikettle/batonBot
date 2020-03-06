using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace BatonBot
{
    public class CallLambdaService
    {
        public async Task<List<BatonModel>> CallTheLambda()
        {
            const string url = "https://16tjlp9p0e.execute-api.eu-west-1.amazonaws.com/default/GetTokens";

            var httpRequest = new HttpRequestMessage(HttpMethod.Get, $"{url}")
            {
                Content = new StringContent(
                    string.Empty,
                    Encoding.UTF8,
                    "application/json")
            };

            using (var client = new HttpClient())
            {
                 var response =  await client.SendAsync(httpRequest);

                var result =
                    JsonConvert.DeserializeObject<List<BatonModel>>(
                        response.Content.ReadAsStringAsync().Result);

                return result;
            }
        }

        public async Task<bool> TakeBaton(string batonName, string holderName)
        {
            const string url = "https://16tjlp9p0e.execute-api.eu-west-1.amazonaws.com/default/SaveToken";

            var baton = new BatonModel() {BatonName = batonName, Holder = holderName, TakenDate = DateTime.Now};

            var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"{url}")
            {
                Content = new StringContent(
                    JsonConvert.SerializeObject(baton),
                    Encoding.UTF8,
                    "application/json")
            };

            using (var client = new HttpClient())
            {
                var response = await client.SendAsync(httpRequest);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    return true;
                }

                return false;
            }
        }
    }
}
