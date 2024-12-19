using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BetburgerServer.Constant
{
    public class PickSource
    {
        public string name;
        public string description;
        public int ndirectlink; //0 : Direct link, 1 : SiteUrl, 2 : copybet(bs), 3 : Openbet
        public bool isbroadcast;

        public PickSource(string Name, string Desc, string IsDirectLink, string IsBroadCast)
        {
            name = Name;
            description = Desc;
            ndirectlink = Utils.ParseToInt(IsDirectLink);
            isbroadcast = false;
            if (IsBroadCast == "1")
                isbroadcast = true;
        }
    }
}
