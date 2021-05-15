using System.Threading.Tasks;

namespace SharedBaton.RepositoryMapper
{
    public class RepositoryMapper : IRepositoryMapper
    {
        public string GetRepositoryNameFromBatonName(string batonName)
        {
            if (batonName == "be")
            {
                return "maraschino";
            }

            if (batonName == "fe")
            {
                return "ADA-Research-UI";
            }

            if (batonName == "man")
            {
                return "ADA-Research-Configuration";
            }

            return null;
        }
    }
}
