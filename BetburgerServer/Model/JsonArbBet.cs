using SeastoryServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BetburgerServer.Model
{
    public class JsonArbBet
    {
        public string id { get; set; }
        public string period_id { get; set; }
        public string bc_id { get; set; }
        public double koef { get; set; }
        public double commission { get; set; }
        public object koef_lay { get; set; }
        public bool is_lay { get; set; }
        public object market_depth { get; set; }
        public long diff { get; set; }
        public string bookmaker_event_direct_link { get; set; }
        public string bookmaker_league_id { get; set; }
        public long bookmaker_event_id { get; set; }
        public string direct_link { get; set; }
        public long created_at { get; set; }
        public long updated_at { get; set; }
        public long started_at { get; set; }
        public string bookmaker_id { get; set; }
        public bool swap_teams { get; set; }
        public object current_score { get; set; }
        public object raw_id { get; set; }
        public string home { get; set; }
        public string away { get; set; }
        public string league { get; set; }

        public string market_and_bet_type { get; set; }
        public string market_and_bet_type_param { get; set; }

        public string getBookmakerId()
        {
            return bookmaker_id;
        }

        public string getStartat(string seconds)
        {
            long secondcount = Convert.ToInt64(seconds);
            DateTime from = new DateTime(1970, 1, 1).Add(TimeSpan.FromSeconds(secondcount));
            DateTime time = from.AddHours(0); // 2
            return time.ToString("MM-dd-yyyy HH:mm");
        }
        public string getStartat()
        {
            if (started_at == 0)
                return string.Empty;

            DateTime from = new DateTime(1970, 1, 1).Add(TimeSpan.FromSeconds(started_at));
            DateTime time = from.AddHours(0); // 2
            return time.ToString("MM-dd-yyyy HH:mm");
        }

        public string getCreateat()
        {
            if (created_at == 0)
                return string.Empty;

            DateTime from = new DateTime(1970, 1, 1).Add(TimeSpan.FromSeconds(created_at));
            DateTime time = from.AddHours(0); // 1
            return time.ToString("MM-dd-yyyy HH:mm");
        }

        public string getUpdatedat()
        {
            if (updated_at == 0)
                return string.Empty;

            DateTime from = new DateTime(1970, 1, 1).Add(TimeSpan.FromSeconds(updated_at));
            DateTime time = from.AddHours(0); // 1
            return time.ToString("MM-dd-yyyy HH:mm");
        }

        public string getEventTitle()
        {
            return home + " - " + away;
        }

        public string getEventUrl(bool isLive)
        {
            return "https://www.betburger.com/bets/" + id + "?access_token=" + cServerSettings.GetInstance().BBToken + "&is_live=" + (isLive ? "1" : "0");
        }
    }
}
