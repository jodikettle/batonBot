using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Newtonsoft.Json;
using Amazon.Lambda.Core;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.APIGatewayEvents;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace BatonLambda
{
    public class Function
    {
        private const string ItemKey = "BatonName";
        private const string Holder = "Holder";
        private const string TakenDate = "TakenDate";
        private const string TableName = "Baton";

        // Lambda to get the Batons
        public APIGatewayProxyResponse Handler(APIGatewayProxyRequest request, ILambdaContext context)
        {
            var client = new AmazonDynamoDBClient();
            context.Logger.LogLine("Accessing Table");

            var att = new List<string>() { ItemKey, Holder, TakenDate };

            var result = client.ScanAsync(TableName, att).GetAwaiter().GetResult();

            context.Logger.LogLine(JsonConvert.SerializeObject(result));

            var itemsReturned = result.Items.Select(x=> new BatonModel
            {
                Holder = x.ContainsKey("Holder") ? x["Holder"].S : null,
                BatonName = x["BatonName"].S,
                TakenDate = x.ContainsKey("TakenDate") ? DateTime.Parse(x["TakenDate"].S) : ((DateTime?)null)
            });

            context.Logger.LogLine(JsonConvert.SerializeObject(itemsReturned));

            return new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.OK,
                Body = JsonConvert.SerializeObject(itemsReturned),
                Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
            };
        }
    }
}