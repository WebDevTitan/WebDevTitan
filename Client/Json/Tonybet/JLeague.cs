using System.Collections.Generic;

namespace Project.Json.Tonybet
{
    public class JLeague
    {
        public JData data { get; set; } = new JData();
    }

    public class JData
    {
        public List<TonyEvent> items { get; set; } = new List<TonyEvent>();
        public JRelation relations { get; set; }
    }
    public class TonyEvent
    {
        public long id { get; set; }
        public int sportId { get; set; }
        public long competitor1Id { get; set; }
        public long competitor2Id { get; set; }
    }

    public class JRelation
    {
        public List<TonyCompetition> competitors { get; set; } = new List<TonyCompetition>();
    }
    public class TonyCompetition
    {
        public long id { get; set; }
        public string name { get; set; }
    }
}
