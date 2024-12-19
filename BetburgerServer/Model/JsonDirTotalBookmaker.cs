using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BetburgerServer.Model
{
    public class JsonDirTotalBookmaker
    {
        public List<JsonDirBookmaker> arbs { get; set; } = new List<JsonDirBookmaker>();
        public List<JsonDirBookmaker> valuebets { get; set; } = new List<JsonDirBookmaker>();
    }
}
