using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using WebSocketSharp;
using WebSocketSharp.Server;
using Bet365LiveAgent.Data.Soccer;

namespace Bet365LiveAgent.Logics
{
    class WebSocketClient : WebSocketBehavior
    {
        private static int _number = 0;

        private string _prefix;
        private string _name;

        private string _activeEventID;

        public WebSocketClient()
        {

        }

        public WebSocketClient(string prefix)
        {
            _prefix = !prefix.IsNullOrEmpty() ? prefix : "anon#";
        }

        private static int GetNumber()
        {
            return Interlocked.Increment(ref _number);
        }

        private string GetName()
        {
            var name = Context.QueryString["name"];
            return !name.IsNullOrEmpty() ? name : _prefix + GetNumber();
        }

        protected override void OnOpen()
        {
            base.OnOpen();

            Log.Level = LogLevel.Error;
            Log.File = $"{Global.LogFilePath}WebSocketClient-{Global.LogFileName}";

            _name = GetName();

            string message = string.Empty;
            List<Object> matches = new List<object>();
            if (Bet365AgentManager.Instance.SoccerMatches != null)
            {
                matches.AddRange(Bet365AgentManager.Instance.SoccerMatches);
            }            
            message = JsonConvert.SerializeObject(matches);
            base.Send($"F{message}");
            Global.WriteLog(LOGLEVEL.FULL, LOGTYPE.OUTDATA, $"WebSocketClient [{_name}] connected.");
        }

        protected override void OnClose(CloseEventArgs e)
        {
            base.OnClose(e);
            Global.WriteLog(LOGLEVEL.FULL, LOGTYPE.OUTDATA, $"WebSocketClient [{_name}] disconnected. Reason: {e.Reason}");
        }

        protected override void OnError(ErrorEventArgs e)
        {
            base.OnError(e);
            Global.WriteLog(LOGLEVEL.FULL, LOGTYPE.OUTDATA, $"WebSocketClient [{_name}] error: {e.Message}");
        }

        protected override void OnMessage(MessageEventArgs e)
        {
            base.OnMessage(e);
            Global.WriteLog(LOGLEVEL.FULL, LOGTYPE.OUTDATA, $"Received from WebSocketClient [{_name}] : {e.Data}");

            if (string.IsNullOrWhiteSpace(e.Data))
                return;

            string type = e.Data.Substring(0, 1);
            if ("R".Equals(type))
            {
                _activeEventID = e.Data.Substring(1);
                SoccerMatchData soccerMatchData = Bet365AgentManager.Instance.SoccerMatches.Find(m => m.EventID == _activeEventID);
                if (soccerMatchData != null)
                {
                    string message = JsonConvert.SerializeObject(soccerMatchData);
                    base.Send($"R{message}");
                }
                else
                {
                    base.Send("R");
                }
            }
            else if ("M".Equals(type))
            {
                SoccerMatchData soccerMatchData = Bet365AgentManager.Instance.SoccerMatches.Find(m => m.EventID == _activeEventID);
                if (soccerMatchData != null)
                {
                    string message = string.Empty;
                    base.Send($"M{message}");
                }
                else
                {
                    base.Send("M");
                }
            }
        }
    }
}
