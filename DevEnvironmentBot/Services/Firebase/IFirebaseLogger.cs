
namespace BatonBot.Interfaces
{
    using System;

    public interface IFirebaseLogger
    {
        void Log(
            string queueId, string batonName, string name, DateTime dateRequested, DateTime? dateReceived,
            DateTime dateReleased, int moveMeCount, bool pncaked);
    }
}
