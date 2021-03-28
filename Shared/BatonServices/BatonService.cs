using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
using SharedBaton.Interfaces;
using SharedBaton.Models;

namespace SharedBaton.BatonServices
{
    public class BatonService: IBatonService
    {
        private readonly List<Baton> batonList;

        public BatonService(IConfiguration config)
        { 
            var batonsString = config["Batons"];
            this.batonList = batonsString.Split(',').Select(x => new Baton { Shortname = x }).ToList();
        }

        public List<Baton> GetBatons()
        {
            return batonList;
        }

        public bool Contains(string batonString)
        {
            return this.batonList.Any(x => x.Shortname == batonString);
        }

        public string List()
        {
            return string.Join(",", this.batonList.Select(x => x.Shortname));
        }

        public Baton CheckBatonType(string batonString)
        {
            return this.batonList.FirstOrDefault(x => x.Shortname.ToLower() == batonString.ToLower());
        }
    }
}
