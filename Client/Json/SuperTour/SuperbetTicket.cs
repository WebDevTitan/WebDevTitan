using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Project.Helphers;

namespace Project.Json.SuperTour
{
    public class SuperbetTicket
    {
        public string ticketOnline { get; set; } = "online";
        public double total { get; set; }
        public string betType { get; set; }
        public string combs { get; set; }
        public List<TicketItem> items { get; set; } = new List<TicketItem>();
        public string clientSourceType { get; set; } = "Desktop_new";
        public int paymentBonusType { get; set; } = 1;
        public string locale { get; set; } = "en-BR";
        public RequestDetail requestDetails { get; set; } = new RequestDetail();
        public string autoAcceptChanges { get; set; } = "1";
        public string ticketUuid { get; set; } = Utils.generateGuid();
    }
    public class TicketItem
    {
        public long oddTypeId { get; set; }
        public string sbValue { get; set; }
        public long matchId { get; set; }
        public string value { get; set; }
        public string matchName { get; set; }
        public string oddFullName { get; set; }
        public string matchDate { get; set; }
        public string matchDateUtc { get; set; }
        public long betGroupId { get; set; }
        public bool selected { get; set; } = true;
        public string type { get; set; } = "sport";
        public JArray rules { get; set; } = new JArray();
        public bool fix { get; set; } = false;
        public string teamnameone { get; set; }
        public string teamnametwo { get; set; }
        public string tournamentName { get; set; }
        public string sportName { get; set; }
        public string teamId1 { get; set; }
        public string teamId2 { get; set; }
        public string betRadarId { get; set; }
        public string tournamentId { get; set; }
        public string oddDescription { get; set; }
        public int sportId { get; set; }
        public bool live { get; set; }
        public long eventId { get; set; }
        public string eventUuid { get; set; }
        public string eventName { get; set; }
        public long eventCode { get; set; }
        public long marketId { get; set; }
        public string marketName { get; set; }
        public string marketUuid { get; set; }
        public long oddId { get; set; }
        public string oddName { get; set; }
        public string uuid { get; set; }
        public string oddUuid { get; set; }
        public int sourceType { get; set; } = 101;
        public long sourceScreen { get; set; } = 100;
    }
    public class RequestDetail
    {
        public string ldAnonymousUserKey { get; set; }
        public string deviceId { get; set; }
    }
}
