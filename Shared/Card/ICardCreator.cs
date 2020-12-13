using Firebase.Database;
using Microsoft.Bot.Schema;
using SharedBaton.Models;
using System.Collections.Generic;

namespace SharedBaton.Card
{
    public interface ICardCreator
    {
        Attachment CreateBatonsAttachment(IList<FirebaseObject<BatonQueue>> batons);
    }
}
