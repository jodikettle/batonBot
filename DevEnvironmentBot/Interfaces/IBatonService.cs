using System.Collections.Generic;

namespace BatonBot.Interfaces
{
    using BatonBot.Models;

    public interface IBatonService
    {
        List<Baton> GetBatons();
        bool Contains(string batonString);
        string List();
        Baton CheckBatonType(string batonString);
    }
}
