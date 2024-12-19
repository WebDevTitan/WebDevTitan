using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PinnacleWrapper.Data
{
    public class GetSpOddsResponse
    {
        [JsonProperty(PropertyName = "sportId")]
        public int SportId { get; set; }

        [JsonProperty(PropertyName = "last")]
        public long Last { get; set; }

        [JsonProperty(PropertyName = "leagues")]
        public List<GetSpOddsLeague> Leagues { get; set; }
    }

    public class GetSpOddsLeague
    {
        [JsonProperty(PropertyName = "id")]
        public int Id { get; set; }

        [JsonProperty(PropertyName = "specials")]
        public List<GetSpOddsEvent> Events { get; set; }
    }

    public class GetSpOddsEvent
    {
        [JsonProperty(PropertyName = "id")]
        public int Id { get; set; }

        [JsonProperty(PropertyName = "contestantLines")]
        public List<GetContestantLine> contestantLines { get; set; }
    }
    public class GetContestantLine
    {
        public long? id { get; set; }
        public long? lineId { get; set; }
        public double price { get; set; }

        public double? handicap { get; set; }
    }
}
