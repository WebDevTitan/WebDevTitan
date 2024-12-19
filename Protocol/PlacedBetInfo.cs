using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Protocol
{
    [Serializable]
    public class PlacedBetInfo
    {
        public string arbID { get; set; }
        public string outcome { get; set; }
        public string bookmaker { get; set; }
        public string username { get; set; }
        public string sport { get; set; }
        public string homeTeam { get; set; }
        public string awayTeam { get; set; }
        public string league { get; set; }
        public decimal percent { get; set; }        
        public double odds { get; set; }
        public double stake { get; set; }        
        public double balance { get; set; }

        public string reserve { get; set; }
        public string reserve1 { get; set; }

        public string pendingBets { get; set; }

        public DateTime timeStamp { get; set; }

        public PlacedBetInfo()
        {
            outcome = "";
            bookmaker = "";
            username = "";
            sport = "";
            homeTeam = "";
            awayTeam = "";
            league = "";
            pendingBets = "";
            percent = 0;            
            odds = 0;
            stake = 0;
            balance = 0;
            timeStamp = DateTime.Now;
        }

        public PlacedBetInfo(BetburgerInfo info, double _balance)
        {
            arbID = info.arbId;
            bookmaker = info.bookmaker;
            outcome = info.outcome;
            sport = info.sport;
            homeTeam = info.homeTeam;
            awayTeam = info.awayTeam;
            league = info.league;
            percent = info.percent;
            odds = info.odds;
            stake = info.stake;
            balance = _balance;
            timeStamp = info.date;
        }
    }
}
