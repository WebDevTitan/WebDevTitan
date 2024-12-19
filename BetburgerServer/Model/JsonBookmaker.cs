using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BetburgerServer.Model
{
    public class JsonBookmaker
    {
        public bool active { get; set; }
        public string amount { get; set; }
        public string bookmaker_id { get; set; }
        public string clone_id { get; set; }
        public string commission { get; set; }
        public string created_at { get; set; }
        public string currency_id { get; set; }
        public string domain { get; set; }
        public string id { get; set; }
        public string max_odds { get; set; }
        public string min_odds { get; set; }
        public string round { get; set; }
        public string updated_at { get; set; }
        public string user_id { get; set; }
        public string user_type { get; set; }
    }
}
