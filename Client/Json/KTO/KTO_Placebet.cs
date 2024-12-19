using System.Collections.Generic;

namespace Project.Json.KTO
{
    public class KTO_Placebet
    {
        public string culture { get; set; } = "en-GB";
        public long timezoneOffset { get; set; } = -480;
        public string integration { get; set; } = "kto.com";
        public int deviceType { get; set; } = 1;
        public string numFormat { get; set; } = "en-GB";
        public string countryCode { get; set; } = "BR";
        public int betType { get; set; } = 0;
        public string requestId { get; set; }
        public bool isAutoCharge { get; set; } = false;
        public bool confirmedByClient { get; set; } = false;
        public int oddsChangeAction { get; set; } = 3;
        public int device { get; set; } = 0;
        public List<double> stakes { get; set; } = new List<double>();
        public List<bool> eachWays { get; set; } = new List<bool>() { false };
        public List<BetMarket> betMarkets { get; set; } = new List<BetMarket>();
    }

    public class BetMarket
    {
        public long id { get; set; }
        public bool isBanker { get; set; } = false;
        public int dbId { get; set; } = 10;
        public string sportName { get; set; }
        public bool rC { get; set; }
        public string eventName { get; set; }
        public string catName { get; set; }
        public string champName { get; set; }
        public int sportTypeId { get; set; }
        public List<KTO_Odd> odds { get; set; } = new List<KTO_Odd>();

    }

    public class KTO_Odd
    {
        public long id { get; set; }
        public string sPOV { get; set; }
        public long marketId { get; set; }
        public double price { get; set; }
        public string marketName { get; set; }
        public int marketTypeId { get; set; }
        public bool mostBalanced { get; set; }
        public int selectionTypeId { get; set; }
        public string selectionName { get; set; }


    }
}
