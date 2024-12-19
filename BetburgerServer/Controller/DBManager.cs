using Org.BouncyCastle.Asn1.Ocsp;
using Protocol;
using SeastoryServer;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TL;

namespace BetburgerServer.Controller
{
    public class DBManager
    {
        public static void Insert_Balance_History(string license_id, string gameid, double balance, string info)
        {
            string query = string.Format("INSERT INTO balance_history(license_id, account_username, balance, info, timestamp) VALUES ('{0}', '{1}', {2}, '{3}', '{4}')",
                      license_id, gameid, balance, info, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            MYSqlMng.GetInstance().InsertQuery(query);
        }

        public static void Insert_Pick_History(BetburgerInfo pick)
        {

            string query = string.Format("INSERT INTO pick_history(id, kind, formula, percent, roi, bookmaker, sport, hometeam, awayteam, eventtitle, eventurl, outcome, odds, commission, created, started, updated, league, period, profit, directlink, siteurl, extra, color, islive, opbookmaker, serverlabel) VALUES ('{0}', {1}, '{2}', {3}, {4}, '{5}', '{6}', '{7}', '{8}', '{9}', '{10}', '{11}', {12}, {13}, '{14}', '{15}', '{16}', '{17}', '{18}', {19}, '{20}', '{21}', '{22}', '{23}', {24}, '{25}', '{26}')",
                       pick.arbId, (int)pick.kind, pick.formula, pick.percent, pick.ROI, pick.bookmaker, pick.sport, pick.homeTeam, pick.awayTeam, pick.eventTitle, pick.eventUrl, pick.outcome, pick.odds, pick.commission, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), pick.started, pick.updated, pick.league, pick.period, pick.profit, pick.direct_link, pick.siteUrl, pick.extra, pick.color, pick.isLive?1:0, pick.opbookmaker, cServerSettings.GetInstance().Label);
            MYSqlMng.GetInstance().InsertQuery(query);
        }
        public static void Insert_Bet_History(string license_id, string gameid, string arbID, double balance, string outcome, string sport, string homeTeam, string awayTeam, string league, double percent, double odds, double stake, string reserve, string reserve1, string pendingBets)
        {
            string query = string.Format("INSERT INTO bet_history(license_id, account_username, pick_id, balance, timestamp, outcome, sport, hometeam, awayteam, league, percent, odds, stake, reserve, reserve1, pendingbets) VALUES ('{0}', '{1}', '{2}', {3}, '{4}', '{5}', '{6}', '{7}', '{8}', '{9}', {10}, {11}, {12}, '{13}', '{14}', '{15}')",
                       license_id, gameid, arbID, balance, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), outcome, sport, homeTeam, awayTeam, league, percent, odds, stake, reserve, reserve1, pendingBets);
            MYSqlMng.GetInstance().InsertQuery(query);
        }

        public static void Update_license_account(string license_id, string gameid)
        {
            string query = string.Format("UPDATE license SET account_username='{1}', assigned_at='{2}' where id='{0}'", license_id, gameid, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            MYSqlMng.GetInstance().UpdateQuery(query);
        }

        public static void Insert_login_history(string license_id, string site_id, string gameid)
        {
            string query = string.Format("INSERT INTO login_history (license_id, site_id, account_username, timestamp) VALUES('{0}', '{1}', '{2}', '{3}')", license_id, site_id, gameid, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            MYSqlMng.GetInstance().InsertQuery(query);
        }

        public static void Increase_QRScanCount(string license)
        {
            string query = string.Format("UPDATE license set qr_scancount = qr_scancount + 1 where id='{0}'", license);
            MYSqlMng.GetInstance().UpdateQuery(query);
        }

        public static string[][] Select_license(string license)
        {
            string query = string.Format("SELECT * FROM license inner join user on user.id = license.create_user_id inner join site on site.id = license.site_id where code='{0}'", license);
            return MYSqlMng.GetInstance().SelectQuery(query);
        }
        public static bool Connect_DB()
        {
            MYSqlMng.GetInstance().DBID = cServerSettings.GetInstance().DBID;
            MYSqlMng.GetInstance().DBPwd = cServerSettings.GetInstance().DBPwd;
            MYSqlMng.GetInstance().DBServer = cServerSettings.GetInstance().DBIP.ToString();
            MYSqlMng.GetInstance().DBName = cServerSettings.GetInstance().DBName;
            MYSqlMng.GetInstance().ReconnectMode = false;

            
            if (!MYSqlMng.GetInstance().ConnectDBServer())
            {
                return false;
            }

            if (!MYSqlMng.GetInstance().AttachDatabase())
            {
                return false;
            }
            return true;
        }
    }
}
