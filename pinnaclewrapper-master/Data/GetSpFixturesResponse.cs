using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PinnacleWrapper.Data
{
    public class GetSpFixturesResponse
    {
        [JsonProperty(PropertyName = "sportId")]
        public int SportId;

        [JsonProperty(PropertyName = "last")]
        public long Last;

        [JsonProperty(PropertyName = "leagues")]
        public List<SpFixturesLeague> Leagues;
    }

    public class SpFixturesLeague
    {
        [JsonProperty(PropertyName = "id")]
        public int Id;

        [JsonProperty(PropertyName = "specials")]
        public List<SpFixtureEvent> Events;
    }

    public class SpFixtureEvent
    {
        public long id { get; set; }
        public string betType { get; set; }
        public string name { get; set; }
        public string date { get; set; }
        public string category { get; set; }
        public string status { get; set; }

        [JsonProperty(PropertyName = "event")]
        public SpEvent sp_event { get; set; }

        public List<Contstant> contestants { get; set; }
        public int liveStatus { get; set; }
    }

    public class SpEvent
    {
        public long id { get; set; }
        public int periodNumber { get; set; }
        public string home { get; set; }
        public string away { get; set; }
    }
    public class Contstant
    {
        public long id { get; set; }
        public string name { get; set; }
        public int rotNum { get; set; }
    }
}
