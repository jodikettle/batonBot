using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace BatonBot.Cards
{
    public class Card
    {
        public static Attachment CreateAttachment(List<BatonModel> batons)
        {
            // combine path for cross platform support
            string[] paths = { ".", "Resources", "table.json" };
            var adaptiveCardJson = File.ReadAllText(Path.Combine(paths));

            var feBatonHolder = batons.FirstOrDefault(x => x.BatonName == "fe")?.Holder;
            var beBatonHolder = batons.FirstOrDefault(x => x.BatonName == "be")?.Holder;
            var manBatonHolder = batons.FirstOrDefault(x => x.BatonName == "man")?.Holder;

            var feBatonTakenDate = batons.FirstOrDefault(x => x.BatonName == "fe")?.TakenDate;
            var beBatonTakenDate = batons.FirstOrDefault(x => x.BatonName == "be")?.TakenDate;
            var manBatonTakenDate = batons.FirstOrDefault(x => x.BatonName == "man")?.TakenDate;

            adaptiveCardJson = adaptiveCardJson.Replace("{frontend.name}", feBatonHolder ?? "");
            adaptiveCardJson = adaptiveCardJson.Replace("{backend.name}", beBatonHolder ?? "");
            adaptiveCardJson = adaptiveCardJson.Replace("{manifest.name}", manBatonHolder ?? "");

            adaptiveCardJson = adaptiveCardJson.Replace("{frontend.takenDate}", feBatonTakenDate.ToString() ?? "");
            adaptiveCardJson = adaptiveCardJson.Replace("{backend.takenDate}", beBatonTakenDate.ToString() ?? "");
            adaptiveCardJson = adaptiveCardJson.Replace("{manifest.takenDate}", manBatonTakenDate.ToString() ?? "");

            var adaptiveCardAttachment = new Attachment()
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = JsonConvert.DeserializeObject(adaptiveCardJson),
            };
            return adaptiveCardAttachment;
        }

        public static Attachment CreateBatonAttachment(string baton)
        {
            // combine path for cross platform support
            string[] paths = { ".", "Resources", "baton.json" };
            var adaptiveCardJson = File.ReadAllText(Path.Combine(paths));

            adaptiveCardJson = adaptiveCardJson.Replace("{batonType}", baton);

            var adaptiveCardAttachment = new Attachment()
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = JsonConvert.DeserializeObject(adaptiveCardJson),
            };
            return adaptiveCardAttachment;
        }
    }
}
