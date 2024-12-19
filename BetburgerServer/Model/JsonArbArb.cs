using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BetburgerServer.Model
{
    public class JsonArbArb
    {
        public string arb_hash { get; set; }
        public double min_koef { get; set; }
        public string arb_formula_id { get; set; }
        public long f { get; set; }
        public long[] bk_ids { get; set; }
        public long created_at { get; set; }
        public string bet1_id { get; set; }
        public string arb_type { get; set; }
        public double percent { get; set; }
        public double roi { get; set; }
        public string bet2_id { get; set; }
        public string bet3_id { get; set; }
        public double max_koef { get; set; }
        public long event_id { get; set; }
        public long updated_at { get; set; }
        public long started_at { get; set; }
        public string id { get; set; }
        public double middle_value { get; set; }
        public object game_flow { get; set; }
        public string event_name { get; set; }
        public string league { get; set; }
        public long league_id { get; set; }
        public string sport_id { get; set; }
        public long country_id { get; set; }
        public bool paused { get; set; }

        public bool diffRules { get; set; }
        public long home_id { get; set; }
        public long away_id { get; set; }
        public bool is_live { get; set; }

        public string getColor()
        {
            diffRules = 2 != (2 & f);

            long t = updated_at;
            long e = started_at;
            bool n = is_live;
            bool i = paused;
            bool r = false;
            string o = "grey";
            long a = (long)DateTime.Now.ToUniversalTime().Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;

            if (a - t <= 30)
                o = "green";
            if (!n && e - a <= 600)
                o = "red";
            if (n && i)
                o = "blue";
            if (r)
                o = "yellow";
            return o;
        }
    }
}
