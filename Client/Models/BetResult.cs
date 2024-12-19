using Protocol;

namespace Project.Models
{
    public class BetResult : BetburgerInfo
    {
        public int result { get; set; } // 0: None 1: Placed 2: Success 3: Fail

        public BetResult()
        {

        }
        public BetResult(BetburgerInfo info)
        {
            updated_sec = info.updated_sec;
            kind = info.kind;
            formula = info.formula;
            percent = info.percent;
            ROI = info.ROI;
            bookmaker = info.bookmaker;
            sport = info.sport;
            homeTeam = info.homeTeam;
            awayTeam = info.awayTeam;
            eventTitle = info.eventTitle;
            eventUrl = info.eventUrl;
            outcome = info.outcome;
            odds = info.odds;
            commission = info.commission;
            created = info.created;
            started = info.started;
            updated = info.updated;
            stake = info.stake;
            arbId = info.arbId;
            league = info.league;
            period = info.period;
            profit = info.profit;
            direct_link = info.direct_link;
            siteUrl = info.siteUrl;
            extra = info.extra;
            raw_id = info.raw_id;
            isLive = info.isLive;
            color = info.color;
            date = info.date;
            opbookmaker = info.opbookmaker;

            result = 0;
        }
    }
}
