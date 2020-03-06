using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Newtonsoft.Json;

namespace BatonLambda
{
    public class SaveBatonLambda
    {
        private const string ItemKey = "BatonName";
        private const string Holder = "Holder";
        private const string TakenDate = "TakenDate";
        private const string TableName = "Baton";

        public APIGatewayProxyResponse Handler(APIGatewayProxyRequest request, ILambdaContext context)
        {
            context.Logger.LogLine(request.Body);

            var baton = JsonConvert.DeserializeObject<BatonModel>(request?.Body);

            var client = new AmazonDynamoDBClient();

            var key =
                new Dictionary<string, AttributeValue>
                {
                    {
                        ItemKey,
                        new AttributeValue { S = baton.BatonName }
                    }
                };

            var itemRequest = new GetItemRequest
            {
                TableName = TableName,
                Key = key
            };

            var result = client.GetItemAsync(itemRequest).GetAwaiter().GetResult();

            context.Logger.LogLine(JsonConvert.SerializeObject(result));

            var dict = baton.Holder != null
                ? new Dictionary<string, AttributeValue>
                {
                    {ItemKey, new AttributeValue(baton.BatonName)},
                    {Holder, baton.Holder != null ? new AttributeValue(baton.Holder) : null},
                    {TakenDate, baton.TakenDate != null ? new AttributeValue(baton.TakenDate.ToString()) : null},
                }
                : new Dictionary<string, AttributeValue>
                {
                    {ItemKey, new AttributeValue(baton.BatonName)}
                };

            context.Logger.LogLine("Dictionary");
            context.Logger.LogLine(JsonConvert.SerializeObject(dict));

            client.PutItemAsync(
                    TableName,
                    dict)
                .GetAwaiter()
                .GetResult();

            return new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.OK,
                Body = JsonConvert.SerializeObject(true),
                Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
            };
        }
    }
}
