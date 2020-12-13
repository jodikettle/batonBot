using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DevEnvironmentBot.Batons
{
    public class Batons
    {
        public static List<Baton> BatonList = new List<Baton>()
        {
            new Baton { Shortname = "sjp" },
            new Baton { Shortname = "redington" },
            new Baton { Shortname = "demo" },
            new Baton { Shortname = "trial" },
            new Baton { Shortname = "ford" }
        };
    }

    public class Baton
    {
        public string Shortname;
    }
}
