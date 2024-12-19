using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Protocol
{
    public enum PickKind
    {
        Type_1, //Normal Betburger
        Type_2, //Park Horse
        Type_3, //SoccerLive Joe
        Type_4, //bs bet
        Type_5, //openbet
        Type_6, //Tradematesports
        Type_7, //Juan Live Soccer
        Type_8, //betspan
        Type_9, //surebet
        Type_10, //giannis esports
        Type_11, //telegram eurobet pick
        Type_12, //telegram ev channel pick (gustavo)
        Type_13, //Rebelbetting
        Type_14,//seubet
        Type_15,
        Type_16,
        Type_17,
        Type_18,
    }
    [Serializable]
    public class BetburgerInfo
    {
        public long updated_sec { get; set; }
        public PickKind kind { get;set; }
        public string formula { get; set; }
        public decimal percent { get; set; }
        public double ROI { get; set; }
        public string bookmaker { get; set; }
        public string sport { get; set; }
        public string homeTeam { get; set; }
        public string awayTeam { get; set; }
        public string eventTitle { get; set; }
        public string tipster { get; set; }
        public int retryCount { get; internal set; }
        

        public string eventUrl { get; set; }
        public string outcome { get; set; }
        public double odds { get; set; }
        public double commission { get; set; }
        public string created { get; set; }
        public string started { get; set; }
        public string updated { get; set; }
        public double stake { get; set; }
        public string arbId { get; set; }
        public string league { get; set; }
        public string period { get; set; }
        public double profit { get; set; }
        public string direct_link { get; set; }
        public string siteUrl { get; set; }
        public string extra { get; set; }
        public object raw_id { get; set; }  //tr value for checking result 
        public bool isLive { get; set; } //check for fail to try to place bet (true means failed to place bet)
        public string color { get; set; }         
        public string opbookmaker { get; set; }
        public DateTime date { get; set; }
        public long eventid { get; set; }
        public double price { get; set; }

        public BetburgerInfo()
        {
            kind = PickKind.Type_14;
            percent = 0;
            ROI = 0.0;
            formula = string.Empty;
            bookmaker = string.Empty;
            sport = string.Empty;
            homeTeam = string.Empty;
            awayTeam = string.Empty;
            eventTitle = string.Empty;
            eventUrl = string.Empty;
            outcome = string.Empty;
            odds = 0.0;
            created = string.Empty;
            started = string.Empty;
            updated = string.Empty;
            arbId = string.Empty;
            league = string.Empty;
            period = string.Empty;
            profit = 0.0;
            direct_link = string.Empty;
            siteUrl = string.Empty;
            extra = string.Empty;
            raw_id = null;
            color = string.Empty;
            eventid = 0;
            price = 0.0;
        }

        public BetburgerInfo(BetburgerInfo info)
        {
            percent = info.percent;
            eventid = info.eventid;
            price = info.price;
            ROI = info.ROI;
            bookmaker = info.bookmaker;
            sport = info.sport;
            homeTeam = info.homeTeam;
            awayTeam = info.awayTeam;
            eventTitle = info.eventTitle;
            eventUrl = info.eventUrl;
            outcome = info.outcome;
            odds = info.odds;
            created = info.created;
            started = info.started;
            updated = info.updated;
            arbId = info.arbId;
            league = info.league;
            period = info.period;
            profit = info.profit;
            direct_link = info.direct_link;
            siteUrl = info.siteUrl;
            extra = info.extra;
            raw_id = info.raw_id;
            color = info.color;
            date = DateTime.Now;            
        }
    }
}
