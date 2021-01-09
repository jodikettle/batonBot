using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Firebase.Database;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using SharedBaton.Models;
using SharedBaton.Card;
using SharedBaton.Interfaces;

namespace DevEnvironmentBot.Cards
{
    public class Card : ICardCreator
    {
        private readonly IBatonService batonList;

        public Card(IBatonService batonList)
        {
            this.batonList = batonList;
        }

        public Attachment CreateBatonsAttachment(IList<FirebaseObject<BatonQueue>> batons)
        {
            // combine path for cross platform support
            string[] paths = { ".", "Resources", "batonQueue.json" };
            var adaptiveCardJson = File.ReadAllText(Path.Combine(paths));

            var listBuilder = new StringBuilder();
            var headerBuilder = new StringBuilder();

            foreach (var baton in batonList.GetBatons())
            {
                //Get the batonqueue
                var batonQueue = batons?.FirstOrDefault(x => x.Object.Name == baton.Shortname);
                listBuilder.Append(FormatQueue(batonQueue?.Object));
                headerBuilder.Append(FormatHeader(batonQueue?.Object));
            }

            adaptiveCardJson = adaptiveCardJson.Replace("\"{QueueData}\"", listBuilder.ToString());
            adaptiveCardJson = adaptiveCardJson.Replace("\"{HeaderData}\"", headerBuilder.ToString());

            var adaptiveCardAttachment = new Attachment()
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = JsonConvert.DeserializeObject(adaptiveCardJson),
            };
            return adaptiveCardAttachment;
        }

        private static string FormatQueue(BatonQueue queue)
        {            
            if (queue == null || queue.Queue.Count() == 0)
                return $"";

            var sb = new StringBuilder();
            sb.Append($"{{\"type\" : \"Column\", \"width\" : \"stretch\", \"items\":");
            sb.Append("[");

            var queueArray = queue.Queue.ToArray();

            for (var i = 0; i < queue.Queue.Count; i++)
            {
                var personInQueue = queueArray[i];
                if (personInQueue != null)
                {
                    var comma = i != 0 ? "," : "";
                    sb.AppendLine(
                        $"{comma}{{\"type\" : \"RichTextBlock\", \"horizontalAlignment\": \"Center\", \"separator\": true, \"inlines\" : [{{ \"type\" : \"TextRun\", \"text\" : \"{personInQueue.UserName} Since:{personInQueue.DateRequested:dd MMM HH:mm}\"}}]}}");
                }
            }

            sb.Append("]");
            sb.Append("},");

            return sb.ToString();
        }

        private static string FormatHeader(BatonQueue queue)
        {
            if (queue == null || queue.Queue.Count() == 0)
                return $"";

            var sb = new StringBuilder();
            sb.Append($"{{\"type\" : \"Column\", \"width\" : \"stretch\", \"items\":");
            sb.Append("[");

            sb.AppendLine($"{{\"type\" : \"Image\", \"altText\": \"\", \"url\": \"https://infomational-bucket.s3.eu-west-2.amazonaws.com/olympic-flame.png\", \"size\" : \"small\", \"horizontalAlignment\": \"Center\" }}," +
            $"{{\"type\" : \"RichTextBlock\", \"horizontalAlignment\": \"Center\", \"inlines\": [{{ \"type\" : \"TextRun\", \"text\" : \"{queue.Name}\"}}]}},");

            sb.Append("]");
            sb.Append("},");

            return sb.ToString();
        }
    }
}
