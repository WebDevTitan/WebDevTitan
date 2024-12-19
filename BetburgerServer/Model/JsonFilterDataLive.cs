using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BetburgerServer.Model
{
    public class JsonFilterDataLive
    {
        public string name { get; set; }
        public string auto_update { get; set; }
        public string grouped { get; set; }
        public string page { get; set; }
        public string per_page { get; set; }
        public string min_profit { get; set; }
        public string max_profit { get; set; }
        public string min_koef { get; set; }
        public string max_koef { get; set; }
        public string min_roi { get; set; }
        public string max_roi { get; set; }
        public string min_middle_value { get; set; }
        public string max_middle_value { get; set; }
        public string min_age { get; set; }
        public string max_age { get; set; }
        public string sort_by { get; set; }
        public string sort_order { get; set; }
        public string[] outcomes { get; set; }
        public string date_shift { get; set; }
        public bool is_live { get; set; }
        public string[] country_ids { get; set; }
        public long countries { get; set; }
        public string access_token { get; set; }
        public string id { get; set; }
        public string primary { get; set; }
        public string isLive { get; set; }
        public string filter_id { get; set; }
        public string event_id { get; set; }
        public string straight { get; set; }
        public string paused { get; set; }
        public string notification_sound { get; set; }
        public string notification_popup { get; set; }
        public string only_rules_group { get; set; }
        public string only_diff_book { get; set; }
        public string hide_decimal_values { get; set; }
        public string show_middles { get; set; }
        public string show_arbs { get; set; }
        public string period_select { get; set; }
        public string min_match_time { get; set; }
        public string max_match_time { get; set; }
        public string fractional { get; set; }
        public string[] bookmakers1 { get; set; }
        public string[] bookmakers2 { get; set; }
        public string[] sports { get; set; }
        public string check_all_countries { get; set; }
        public string[] splat { get; set; }
        public string[] captures { get; set; }
        public long user_id { get; set; }
        public string till_days { get; set; }
        public string till_hours { get; set; }
        public string till_minutes { get; set; }
        public string included_leagues { get; set; }
        public string excluded_leagues { get; set; }
    }
}
