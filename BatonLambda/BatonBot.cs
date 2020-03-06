using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Newtonsoft.Json;

namespace BatonLambda
{
    public class BatonBot
    {
        public APIGatewayProxyResponse Handler(APIGatewayProxyRequest request, ILambdaContext context)
        {
            context.Logger.LogLine(request.Body);

            return new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.OK,
                Body = JsonConvert.SerializeObject(true),
                Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
            };

        }
    }
}
