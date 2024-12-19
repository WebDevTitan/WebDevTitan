using Bet365LiveAgent.Data.Soccer;
using Bet365LiveAgent.Logics;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Bet365LiveAgent
{
    public partial class FrmTest : Form
    {
        SoccerMatchData matchData = new SoccerMatchData();
        public FrmTest()
        {
            InitializeComponent();
        }

        public void SetParameters(SoccerMatchData _matchData)
        {
            matchData = Utils.CreateDeepCopy(_matchData);            
        }

        private void FrmTest_Load(object sender, EventArgs e)
        {
            int hOff = 0, hOn = 0, aOff = 0, aOn = 0;

            txtMatchTime.Text = matchData.Time;

            txtHomeOnTarget.Text = matchData.HomeTeam.OnTarget;
            txtHomeOffTarget.Text = matchData.HomeTeam.OffTarget;            
            int.TryParse(matchData.HomeTeam.OffTarget, out hOff);
            int.TryParse(matchData.HomeTeam.OnTarget, out hOn);            
            txtHomeTarget.Text = (hOn + hOff).ToString();
            txtHomeDangerousAttack.Text = matchData.HomeTeam.DangerAttack;
            txtHomeAttack.Text = matchData.HomeTeam.Attack;

            txtAwayOnTarget.Text = matchData.AwayTeam.OnTarget;
            txtAwayOffTarget.Text = matchData.AwayTeam.OffTarget;            
            int.TryParse(matchData.AwayTeam.OffTarget, out aOff);
            int.TryParse(matchData.AwayTeam.OnTarget, out aOn);
            txtAwayTarget.Text = (aOn + aOff).ToString();
            txtAwayDangerousAttack.Text = matchData.AwayTeam.DangerAttack;
            txtAwayAttack.Text = matchData.AwayTeam.Attack;

            if (matchData.AsianHandicap.MarketData.Count > 0)
            {
                for (int i = 0; i < matchData.AsianHandicap.MarketData.Count; i++)
                {                    
                    if (i == 0)
                    {
                        txtAHHomeHandicap1.Text = matchData.AsianHandicap.MarketData[i].HomeHdp;
                        txtAHHomeOdd1.Text = matchData.AsianHandicap.MarketData[i].HomeOdds;
                        txtAHAwayOdd1.Text = matchData.AsianHandicap.MarketData[i].AwayOdds;
                        txtAHAwayHandicap1.Text = matchData.AsianHandicap.MarketData[i].AwayHdp;
                    }
                    else if (i == 1)
                    {
                        txtAHHomeHandicap2.Text = matchData.AsianHandicap.MarketData[i].HomeHdp;
                        txtAHHomeOdd2.Text = matchData.AsianHandicap.MarketData[i].HomeOdds;
                        txtAHAwayOdd2.Text = matchData.AsianHandicap.MarketData[i].AwayOdds;
                        txtAHAwayHandicap2.Text = matchData.AsianHandicap.MarketData[i].AwayHdp;
                    }
                    else if (i == 2)
                    {
                        txtAHHomeHandicap3.Text = matchData.AsianHandicap.MarketData[i].HomeHdp;
                        txtAHHomeOdd3.Text = matchData.AsianHandicap.MarketData[i].HomeOdds;
                        txtAHAwayOdd3.Text = matchData.AsianHandicap.MarketData[i].AwayOdds;
                        txtAHAwayHandicap3.Text = matchData.AsianHandicap.MarketData[i].AwayHdp;
                    }
                    else if (i == 3)
                    {
                        txtAHHomeHandicap4.Text = matchData.AsianHandicap.MarketData[i].HomeHdp;
                        txtAHHomeOdd4.Text = matchData.AsianHandicap.MarketData[i].HomeOdds;
                        txtAHAwayOdd4.Text = matchData.AsianHandicap.MarketData[i].AwayOdds;
                        txtAHAwayHandicap4.Text = matchData.AsianHandicap.MarketData[i].AwayHdp;
                    }
                    else if (i == 4)
                    {
                        txtAHHomeHandicap5.Text = matchData.AsianHandicap.MarketData[i].HomeHdp;
                        txtAHHomeOdd5.Text = matchData.AsianHandicap.MarketData[i].HomeOdds;
                        txtAHAwayOdd5.Text = matchData.AsianHandicap.MarketData[i].AwayOdds;
                        txtAHAwayHandicap5.Text = matchData.AsianHandicap.MarketData[i].AwayHdp;
                    }
                    else
                    {
                        WriteLog("Handicap Count is larger than 5");
                        break;
                    }
                }
            }

            if (matchData.GoalLine.MarketData.Count > 0)
            {
                for (int i = 0; i < matchData.GoalLine.MarketData.Count; i++)
                {
                    if (i == 0)
                    {
                        txtGLOverOdd1.Text = matchData.GoalLine.MarketData[i].OverOdds;
                        txtGLHandicap1.Text = matchData.GoalLine.MarketData[i].UnderHdp;
                        txtGLUnderOdd1.Text = matchData.GoalLine.MarketData[i].UnderOdds;                        
                    }
                    else if (i == 1)
                    {
                        txtGLOverOdd2.Text = matchData.GoalLine.MarketData[i].OverOdds;
                        txtGLHandicap2.Text = matchData.GoalLine.MarketData[i].UnderHdp;
                        txtGLUnderOdd2.Text = matchData.GoalLine.MarketData[i].UnderOdds;
                    }
                    else if (i == 2)
                    {
                        txtGLOverOdd3.Text = matchData.GoalLine.MarketData[i].OverOdds;
                        txtGLHandicap3.Text = matchData.GoalLine.MarketData[i].UnderHdp;
                        txtGLUnderOdd3.Text = matchData.GoalLine.MarketData[i].UnderOdds;
                    }
                    else if (i == 3)
                    {
                        txtGLOverOdd4.Text = matchData.GoalLine.MarketData[i].OverOdds;
                        txtGLHandicap4.Text = matchData.GoalLine.MarketData[i].UnderHdp;
                        txtGLUnderOdd4.Text = matchData.GoalLine.MarketData[i].UnderOdds;
                    }
                    else if (i == 4)
                    {
                        txtGLOverOdd5.Text = matchData.GoalLine.MarketData[i].OverOdds;
                        txtGLHandicap5.Text = matchData.GoalLine.MarketData[i].UnderHdp;
                        txtGLUnderOdd5.Text = matchData.GoalLine.MarketData[i].UnderOdds;
                    }                
                    else
                    {
                        WriteLog("GoalLine Count is larger than 5");
                        break;
                    }
                }
            }
        }

        private string ConvertOddtoFaction(string origOdd)
        {
            double odd = Convert.ToDouble(origOdd);
            return $"{odd-1}/1";
        }

        private bool FetchData()
        {
            if (string.IsNullOrEmpty(txtMatchTime.Text))
            {
                MessageBox.Show("填写时间");
                return false;
            }
            matchData.Time = txtMatchTime.Text;

            if (string.IsNullOrEmpty(txtHomeOnTarget.Text))
            {
                MessageBox.Show("填写主队射正");
                return false;
            }
            matchData.HomeTeam.OnTarget = txtHomeOnTarget.Text;
            if (string.IsNullOrEmpty(txtHomeOffTarget.Text))
            {
                MessageBox.Show("填写主队射偏");
                return false;
            }
            matchData.HomeTeam.OffTarget = txtHomeOffTarget.Text;
            if (string.IsNullOrEmpty(txtHomeDangerousAttack.Text))
            {
                MessageBox.Show("填写主队危险攻击");
                return false;
            }
            matchData.HomeTeam.DangerAttack = txtHomeDangerousAttack.Text;
            if (string.IsNullOrEmpty(txtHomeAttack.Text))
            {
                MessageBox.Show("填写主队攻击");
                return false;
            }
            matchData.HomeTeam.Attack = txtHomeAttack.Text;

            if (string.IsNullOrEmpty(txtAwayOnTarget.Text))
            {
                MessageBox.Show("填写客队射正");
                return false;
            }
            matchData.AwayTeam.OnTarget = txtAwayOnTarget.Text;
            if (string.IsNullOrEmpty(txtAwayOffTarget.Text))
            {
                MessageBox.Show("填写客队射偏");
                return false;
            }
            matchData.AwayTeam.OffTarget = txtAwayOffTarget.Text;
            if (string.IsNullOrEmpty(txtAwayDangerousAttack.Text))
            {
                MessageBox.Show("填写客队危险攻击");
                return false;
            }
            matchData.AwayTeam.DangerAttack = txtAwayDangerousAttack.Text;
            if (string.IsNullOrEmpty(txtAwayAttack.Text))
            {
                MessageBox.Show("填写客队攻击");
                return false;
            }
            matchData.AwayTeam.Attack = txtAwayAttack.Text;

            matchData.AsianHandicap.MarketData.Clear();
            if (!string.IsNullOrEmpty(txtAHHomeHandicap1.Text) &&
                 !string.IsNullOrEmpty(txtAHHomeOdd1.Text) &&
                 !string.IsNullOrEmpty(txtAHAwayHandicap1.Text) &&
                 !string.IsNullOrEmpty(txtAHAwayOdd1.Text))
            {                
                AsianHandicapMarket market = new AsianHandicapMarket();
                market.HomeHdp = txtAHHomeHandicap1.Text;
                market.HomeOD = ConvertOddtoFaction(txtAHHomeOdd1.Text);
                market.AwayOD = ConvertOddtoFaction(txtAHAwayOdd1.Text);
                market.AwayHdp = txtAHAwayHandicap1.Text;
                matchData.AsianHandicap.MarketData.Add(market);
            }
            if (!string.IsNullOrEmpty(txtAHHomeHandicap2.Text) &&
                !string.IsNullOrEmpty(txtAHHomeOdd2.Text) &&
                !string.IsNullOrEmpty(txtAHAwayHandicap2.Text) &&
                !string.IsNullOrEmpty(txtAHAwayOdd2.Text))
            {
                
                AsianHandicapMarket market = new AsianHandicapMarket();
                market.HomeHdp = txtAHHomeHandicap2.Text;
                market.HomeOD = ConvertOddtoFaction(txtAHHomeOdd2.Text);
                market.AwayOD = ConvertOddtoFaction(txtAHAwayOdd2.Text);
                market.AwayHdp = txtAHAwayHandicap2.Text;
                matchData.AsianHandicap.MarketData.Add(market);
            }
            if (!string.IsNullOrEmpty(txtAHHomeHandicap3.Text) &&
                !string.IsNullOrEmpty(txtAHHomeOdd3.Text) &&
                !string.IsNullOrEmpty(txtAHAwayHandicap3.Text) &&
                !string.IsNullOrEmpty(txtAHAwayOdd3.Text))
            {

                AsianHandicapMarket market = new AsianHandicapMarket();
                market.HomeHdp = txtAHHomeHandicap3.Text;
                market.HomeOD = ConvertOddtoFaction(txtAHHomeOdd3.Text);
                market.AwayOD = ConvertOddtoFaction(txtAHAwayOdd3.Text);
                market.AwayHdp = txtAHAwayHandicap3.Text;
                matchData.AsianHandicap.MarketData.Add(market);
            }
            if (!string.IsNullOrEmpty(txtAHHomeHandicap4.Text) &&
                !string.IsNullOrEmpty(txtAHHomeOdd4.Text) &&
                !string.IsNullOrEmpty(txtAHAwayHandicap4.Text) &&
                !string.IsNullOrEmpty(txtAHAwayOdd4.Text))
            {

                AsianHandicapMarket market = new AsianHandicapMarket();
                market.HomeHdp = txtAHHomeHandicap4.Text;
                market.HomeOD = ConvertOddtoFaction(txtAHHomeOdd4.Text);
                market.AwayOD = ConvertOddtoFaction(txtAHAwayOdd4.Text);
                market.AwayHdp = txtAHAwayHandicap4.Text;
                matchData.AsianHandicap.MarketData.Add(market);
            }
            if (!string.IsNullOrEmpty(txtAHHomeHandicap5.Text) &&
                !string.IsNullOrEmpty(txtAHHomeOdd5.Text) &&
                !string.IsNullOrEmpty(txtAHAwayHandicap5.Text) &&
                !string.IsNullOrEmpty(txtAHAwayOdd5.Text))
            {

                AsianHandicapMarket market = new AsianHandicapMarket();
                market.HomeHdp = txtAHHomeHandicap5.Text;
                market.HomeOD = ConvertOddtoFaction(txtAHHomeOdd5.Text);
                market.AwayOD = ConvertOddtoFaction(txtAHAwayOdd5.Text);
                market.AwayHdp = txtAHAwayHandicap5.Text;
                matchData.AsianHandicap.MarketData.Add(market);
            }


            matchData.GoalLine.MarketData.Clear();
            if (!string.IsNullOrEmpty(txtGLOverOdd1.Text) &&
                 !string.IsNullOrEmpty(txtGLHandicap1.Text) &&
                 !string.IsNullOrEmpty(txtGLUnderOdd1.Text))
            {
                GoalLineMarket market = new GoalLineMarket();
                market.OverOD = ConvertOddtoFaction(txtGLOverOdd1.Text);
                market.UnderHdp = txtGLHandicap1.Text;
                market.UnderOD = ConvertOddtoFaction(txtGLUnderOdd1.Text);                
                matchData.GoalLine.MarketData.Add(market);
            }
            if (!string.IsNullOrEmpty(txtGLOverOdd2.Text) &&
                !string.IsNullOrEmpty(txtGLHandicap2.Text) &&
                !string.IsNullOrEmpty(txtGLUnderOdd2.Text))
            {
                GoalLineMarket market = new GoalLineMarket();
                market.OverOD = ConvertOddtoFaction(txtGLOverOdd2.Text);
                market.UnderHdp = txtGLHandicap2.Text;
                market.UnderOD = ConvertOddtoFaction(txtGLUnderOdd2.Text);
                matchData.GoalLine.MarketData.Add(market);
            }
            if (!string.IsNullOrEmpty(txtGLOverOdd3.Text) &&
                !string.IsNullOrEmpty(txtGLHandicap3.Text) &&
                !string.IsNullOrEmpty(txtGLUnderOdd3.Text))
            {
                GoalLineMarket market = new GoalLineMarket();
                market.OverOD = ConvertOddtoFaction(txtGLOverOdd3.Text);
                market.UnderHdp = txtGLHandicap3.Text;
                market.UnderOD = ConvertOddtoFaction(txtGLUnderOdd3.Text);
                matchData.GoalLine.MarketData.Add(market);
            }
            if (!string.IsNullOrEmpty(txtGLOverOdd4.Text) &&
                !string.IsNullOrEmpty(txtGLHandicap4.Text) &&
                !string.IsNullOrEmpty(txtGLUnderOdd4.Text))
            {
                GoalLineMarket market = new GoalLineMarket();
                market.OverOD = ConvertOddtoFaction(txtGLOverOdd4.Text);
                market.UnderHdp = txtGLHandicap4.Text;
                market.UnderOD = ConvertOddtoFaction(txtGLUnderOdd4.Text);
                matchData.GoalLine.MarketData.Add(market);
            }
            if (!string.IsNullOrEmpty(txtGLOverOdd5.Text) &&
                !string.IsNullOrEmpty(txtGLHandicap5.Text) &&
                !string.IsNullOrEmpty(txtGLUnderOdd5.Text))
            {
                GoalLineMarket market = new GoalLineMarket();
                market.OverOD = ConvertOddtoFaction(txtGLOverOdd5.Text);
                market.UnderHdp = txtGLHandicap5.Text;
                market.UnderOD = ConvertOddtoFaction(txtGLUnderOdd5.Text);
                matchData.GoalLine.MarketData.Add(market);
            }
            return true;
        }
        private void btnEliminate_Click(object sender, EventArgs e)
        {
            if (!FetchData())
                return;
            Bet365AgentManager.isMatchCondition(matchData, WriteLog);
        }

        private void WriteLog(string command)
        {
            txtLog.AppendText(command + Environment.NewLine);
        }
    }
}
