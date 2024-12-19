using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Project.Json.Bwin
{
    public class BwinPlacebet
    {
        public PlaceBetRequest placeBetRequest { get; set; } = new PlaceBetRequest();
        public List<BetContextualDetail> betContextualDetails { get; set; } = new List<BetContextualDetail>();

        public BwinPlacebet(string optionId)
        {
            betContextualDetails.Add(new BetContextualDetail(optionId));
        }
    }

    public class PlaceBetRequest
    {
        public string requestId { get; set; }
        public List<Betslipbet> betSlips { get; set; } = new List<Betslipbet>();
        public string doubleBetPreventionMode { get; set; } = "Disabled";
        public string language { get; set; } = "";

        public PlaceBetRequest()
        {
            betSlips.Add(new Betslipbet());
        }
    }

    public class BetContextualDetail
    {
        public string requestId { get; set; }
        public int betSlipKey { get; set; } = 1;
        public string betContextualData { get; set; }

        public BetContextualDetail(string optionId)
        {
            requestId = "";
            betSlipKey = 1;
            betContextualData = $"{{\\\"sourceSystem\\\":3,\\\"betIndex\\\":1,\\\"optionId\\\":{optionId},\\\"pageName\\\":\\\"EventDetails\\\"}}";
        }
        public BetContextualDetail(string _requestid, int _betslipKey, string _betContextualData)
        {
            this.requestId = _requestid;
            this.betSlipKey = _betslipKey;
            this.betContextualData = _betContextualData;
        }
    }
    public class Betslipbet
    {
        public string betSlipType { get; set; } = "Single";
        public int index { get; set; } = 1;
        public JObject stakeTaxation { get; set; } = null;
        public string oddsAcceptanceMode { get; set; } = "Any";
        public JObject overAskOfferDetails { get; set; } = null;
        public JObject systemSlipDetails { get; set; } = null;
        public Stake stake { get; set; } = new Stake();
        public List<BwinBet> bets { get; set; } = new List<BwinBet>();
        public List<string> betGroups { get; set; } = new List<string>();
        public AdditionalInformation additionalInformation { get; set; } = new AdditionalInformation(false);
        public List<string> edsPromoTokens { get; set; } = new List<string>();
        public List<string> promoTokens { get; set; } = new List<string>();

        public Betslipbet()
        {
            bets.Add(new BwinBet());
        }

    }

    public class Stake
    {
        public double amount { get; set; }
        public string currency { get; set; } = "BRL";
    }

    public class BwinBet
    {
        public int index { get; set; } = 1;
        public bool isBanker { get; set; } = false;
        public string betModel { get; set; } = "Option";
        public Bwinodds odds { get; set; } = new Bwinodds();
        public string oddsFormat { get; set; } = "European";
        public AdditionalInformation additionalInformation { get; set; } = new AdditionalInformation(true);
        public List<Pick> picks { get; set; } = new List<Pick>();
        public List<BetDetail> betDetails { get; set; } = new List<BetDetail>();

        public BwinBet()
        {
        }

    }
    public class AdditionalInformation
    {
        public List<InformationItem> informationItems { get; set; } = new List<InformationItem>();

        public AdditionalInformation(bool isInit)
        {
            if (isInit)
            {
                informationItems = new List<InformationItem>()
                {
                    new InformationItem("fixtureStartTime" , "2024-05-13T23:00:00.000Z", "DateTime"),
                    new InformationItem("fixtureType" , "PairGame", "Enum")
                };
            }
            else
                informationItems = new List<InformationItem>();
        }
    }
    public class InformationItem
    {
        public string key { get; set; }
        public string value { get; set; }
        public string valueType { get; set; }

        public InformationItem(string _key, string _value, string _valueType)
        {
            this.key = _key;
            this.value = _value;
            this.valueType = _valueType;
        }
    }
    public class Bwinodds
    {
        public string oddsFormat { get; set; } = "European";
        public double european { get; set; }
    }
    public class BetDetail
    {
        public string id { get; set; }
        public string betDetailType { get; set; }
        public JObject textValue { get; set; } = new JObject();
        public AdditionalInformation additionalInformation { get; set; } = new AdditionalInformation(false);

        public BetDetail(string _id, string _betDetailType, string value, string sign)
        {
            id = _id;
            betDetailType = _betDetailType;
            textValue["value"] = value;
            textValue["sign"] = sign;
        }
    }

    public class Pick
    {
        public string id { get; set; }
    }
}

