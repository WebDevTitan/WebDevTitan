using System;
using Newtonsoft.Json;
using PinnacleWrapper.Enums;

namespace PinnacleWrapper.Data
{
    public class PlaceBetRequest
    {
        [JsonProperty(PropertyName = "uniqueRequestId")]
        public Guid UniqueRequestId;

        [JsonProperty(PropertyName = "acceptBetterLine")]
        public bool AcceptBetterLine;

        [JsonProperty(PropertyName = "customerReference")]    // not required
        public string CustomerReference;

        [JsonProperty(PropertyName = "oddsFormat")]
        public OddsFormat OddsFormat;

        [JsonProperty(PropertyName = "stake")]
        public decimal Stake;

        [JsonProperty(PropertyName = "winRiskStake")]
        public WinRiskType WinRiskType;

        [JsonProperty(PropertyName = "sportId")]
        public int SportId;

        [JsonProperty(PropertyName = "eventId")]
        public int EventId;

        [JsonProperty(PropertyName = "periodNumber")]   // This represents the period of the match. For example, for soccer we have: 0 - Game, 1 - 1st Half, 2 - 2nd Half
        public int PeriodNumber;

        [JsonProperty(PropertyName = "betType")]
        public BetType BetType;

        [JsonProperty(PropertyName = "team")]           // Chosen team type. This is needed only for SPREAD, MONEYLINE and TEAM_TOTAL_POINTS bet types
        public TeamType? TeamType;

        [JsonProperty(PropertyName = "side")]           // Chosen side. This is needed only for TOTAL_POINTS and TEAM_TOTAL_POINTS bet type
        public SideType? SideType;

        [JsonProperty(PropertyName = "lineId")]
        public int LineId;
    }
}
