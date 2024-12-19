using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BetburgerServer.Model
{
    public class JsonFilter
    {
        public bool active { get; set; }
        public string bks1 { get; set; }
        public string bks2 { get; set; }
        public List<string> bookmakers1 { get; set; }
        public List<string> bookmakers2 { get; set; }
        public string country_ids { get; set; }
        public string created_at { get; set; }
        public string current_period { get; set; }
        public string date_shift { get; set; }
        public string delay { get; set; }
        public string excluded_leagues { get; set; }
        public string filter { get; set; }
        public string id { get; set; }
        public string included_leagues { get; set; }
        public string max_age { get; set; }
        public string max_koef { get; set; }
        public string max_match_time { get; set; }
        public string max_percent { get; set; }
        public string max_roi { get; set; }
        public string max_value { get; set; }
        public string middle_or_arb { get; set; }
        public string min_age { get; set; }
        public string min_koef { get; set; }
        public string min_match_time { get; set; }
        public string min_percent { get; set; }
        public string min_roi { get; set; }
        public bool paused { get; set; }
        public string sport_ids { get; set; }
        public List<string> sports { get; set; }
        public string title { get; set; }
        public string type_mask { get; set; }
        public string updated_at { get; set; }
        public string user_id { get; set; }
        public string user_type { get; set; }
    }
}
