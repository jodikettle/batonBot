using System.Collections.Generic;
using SharedBaton.Models;

namespace SharedBaton.Interfaces
{
    public interface IBatonService
    {
        List<Baton> GetBatons();
        bool Contains(string batonString);
        string List();
        Baton CheckBatonType(string batonString);
    }
}
