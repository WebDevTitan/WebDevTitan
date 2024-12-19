using System.Collections.Generic;

namespace Project.Json.Estralebet
{
    public class EstraTicket
    {
        public string culture { get; set; } = "pt-BR";
        public int timezoneOffset { get; set; } = -480;
        public string integration { get; set; } = "estrelabet";
        public int deviceType { get; set; } = 1;
        public string numFormat { get; set; } = "en-GB";
        public string countryCode { get; set; } = "BR";
        public int betType { get; set; } = 0;
        public string requestId { get; set; } = "";
        public bool isAutoChange { get; set; } = false;
        public bool confirmedByClient { get; set; } = false;
        public int oddsChangeAction { get; set; } = 0;
        public int device { get; set; } = 0;

        public List<int> stakes { get; set; } = new List<int>();
        public List<bool> eachWays { get; set; } = new List<bool>() { false };
    }
    public class BetMarket
    {
        public long id { get; set; }
        public bool isBanker { get; set; } = false;
        public int dbId { get; set; } = 10;
        public string sportName { get; set; }
        public bool rC { get; set; } = false;
        public string eventName { get; set; }
        public string catName { get; set; }
        public string champName { get; set; }
        public string sportTypeId { get; set; }
        public List<EstraOdds> odds { get; set; } = new List<EstraOdds>();
    }
    public class EstraOdds
    {
        public long id { get; set; }
        public string sPOV { get; set; }
        public string marketId { get; set; }
        public double price { get; set; }
        public string marketName { get; set; }
        public int marketTypeId { get; set; }
        public bool mostBalanced { get; set; } = false;
        public int selectionTypeId { get; set; } = 3;
        public string selectionName { get; set; }

    }
}
