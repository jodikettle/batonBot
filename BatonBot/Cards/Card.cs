using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Firebase.Database;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using SharedBaton.Card;
using SharedBaton.Models;

namespace BatonBot.Cards
{
    public class Card : ICardCreator
    {
        public Attachment CreateBatonsAttachment(IList<FirebaseObject<BatonQueue>> batons)
        {
            // combine path for cross platform support
            string[] paths = { ".", "Resources", "batonQueue.json" };
            var adaptiveCardJson = File.ReadAllText(Path.Combine(paths));

            var manifestBaton = batons?.FirstOrDefault(x => x.Object.Name == "man");
            var frontEndBaton = batons?.FirstOrDefault(x => x.Object.Name == "fe");
            var backendBaton = batons?.FirstOrDefault(x => x.Object.Name == "be");


            adaptiveCardJson = adaptiveCardJson.Replace("\"{manifestQueue}\"", FormatQueue(manifestBaton?.Object));
            adaptiveCardJson = adaptiveCardJson.Replace("\"{backendQueue}\"", FormatQueue(backendBaton?.Object));
            adaptiveCardJson = adaptiveCardJson.Replace("\"{frontQueue}\"", FormatQueue(frontEndBaton?.Object));

            var adaptiveCardAttachment = new Attachment()
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = JsonConvert.DeserializeObject(adaptiveCardJson),
            };
            return adaptiveCardAttachment;
        }

        private string FormatQueue(BatonQueue queue)
        {
            if (queue == null)
                return $"[]";

            var sb = new StringBuilder();
            sb.Append("[");


            var queueArray = queue.Queue.ToArray();

            for (var i = 0; i < queue.Queue.Count; i++)
            {
                var personInQueue = queueArray[i];
                if (personInQueue != null)
                {
                    var comma = i != 0 ? "," : "";
                    sb.AppendLine(
                        $"{comma}{{\"type\" : \"RichTextBlock\", \"horizontalAlignment\": \"Center\", \"separator\": true, \"inlines\" : [{{ \"type\" : \"TextRun\", \"text\" : \"{personInQueue.UserName} Since:{personInQueue.DateRequested}\"}}]}}");
                }
            }

            sb.Append("]");

            return sb.ToString();
        }
    }
}
