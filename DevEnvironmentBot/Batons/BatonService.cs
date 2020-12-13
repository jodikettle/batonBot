using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DevEnvironmentBot.Batons
{
    public class BatonService
    {
        public bool Contains(string batonString)
        {
            return Batons.BatonList.Any(x => x.Shortname == batonString);
        }

        public string List()
        {
            return string.Join(',', Batons.BatonList.Select(x => x.Shortname));
        }

        public Baton checkBatonType(string batonString)
        {
            return Batons.BatonList.FirstOrDefault(x => x.Shortname == batonString);
        }
    }
}
