using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bet365LiveAgent.Data.Soccer
{
    [Serializable]
    public class PickResultData
    {
        public SoccerMatchData matchData { get; set; }

        public COMMAND command { get; set; }

        public DateTime IssuedTime { get; set; } = DateTime.MinValue;

        public string MatchLabel { get; set; } = string.Empty;
        public string MatchDescription { get; set; } = string.Empty;
        public string CommandDescription { get; set; } = string.Empty;
        public string PickDescription { get; set; } = string.Empty;

        public PickResultData()
        {
        }

        public PickResultData(SoccerMatchData _matchData, COMMAND _command, string _description)
        {
            matchData = Utils.CreateDeepCopy(_matchData);
            command = Utils.CreateDeepCopy(_command);

            MatchLabel = $"{matchData.LeagueName}" + Environment.NewLine;
            MatchLabel += $"{matchData.HomeName} - {matchData.AwayName}" + Environment.NewLine;            
            MatchLabel += $"时间      {matchData.Time}" + Environment.NewLine;
            MatchLabel += $"比分      {matchData.Score}" + Environment.NewLine;
            MatchDescription = matchData.Statistics;
            CommandDescription = _command.ToString();
            PickDescription = _description;

        }
    }
}
