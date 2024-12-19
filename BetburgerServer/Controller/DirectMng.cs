using BetburgerServer.Constant;
using BetburgerServer.Model;
using Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BetburgerServer.Controller
{
    public class DirectMng
    {
        private static DirectMng _instance = null;

        public static DirectMng Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new DirectMng();
                }

                return _instance;
            }
        }

        private HttpClient getHttpClientBetburger()
        {
            HttpClientHandler handler = new HttpClientHandler()
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            };

            handler.CookieContainer = GameConstants.container;

            HttpClient httpClientEx = new HttpClient(handler);
            httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "text/html, application/xhtml+xml, application/xml; q=0.9, image/webp, */*; q=0.8");
            httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 6.3; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/47.0.2526.111 Safari/537.36");
            httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("Upgrade-Insecure-Requests", "1");
            httpClientEx.DefaultRequestHeaders.ExpectContinue = false;

            return httpClientEx;
        }

        public bool getDirectLink(ref BetburgerInfo info)
        {
            try
            {
                string siteUrl = string.Empty;

                switch(info.bookmaker)
                {
                    case "Bwin":
                        {
                            siteUrl = getSiteUrlBwin(info);
                            if (string.IsNullOrEmpty(siteUrl))
                                siteUrl = getSiteUrlBwin(info);
                        }
                        break;
                    default:
                        {
                            siteUrl = getSiteUrl(info);
                            if (string.IsNullOrEmpty(siteUrl))
                                siteUrl = getSiteUrl(info);
                        }
                        break;
                }

                if (siteUrl.Contains("https://www.10bet.com"))
                    siteUrl = siteUrl.Replace("https://www.10bet.com", "https://www.10bet.co.uk");

                if (siteUrl.Contains("https://www.unibet.com"))
                    siteUrl = siteUrl.Replace("https://www.unibet.com", "https://www.unibet.co.uk");

                if (siteUrl.Contains("https://www.marathonbet.com/"))
                    siteUrl = siteUrl.Replace("https://www.marathonbet.com/", "https://www.marathonbet.co.uk/");

                if (siteUrl.Contains("https://bet.pokerstars.eu/"))
                    siteUrl = siteUrl.Replace("https://bet.pokerstars.eu/", "https://www.betstars.uk/");

                if (info.bookmaker != "Pinnacle" && string.IsNullOrEmpty(siteUrl))
                    return false;

                info.siteUrl = siteUrl;

                return true;
            }
            catch(Exception e)
            {
                return false;
            }
        }

        private string getSiteUrlBwin(BetburgerInfo info)
        {
            try
            {
                HttpClient httpClientBetburger = getHttpClientBetburger();

                httpClientBetburger.DefaultRequestHeaders.Referrer = new Uri("https://www.betburger.com/arbs");
                HttpResponseMessage responseMessageMain = httpClientBetburger.GetAsync(info.eventUrl).Result;
                responseMessageMain.EnsureSuccessStatusCode();

                string mainReferer = responseMessageMain.RequestMessage.RequestUri.AbsoluteUri;
                if (string.IsNullOrEmpty(mainReferer) || mainReferer.Equals(info.eventUrl))
                    return string.Empty;

                string responseMessageMainString = responseMessageMain.Content.ReadAsStringAsync().Result;
                if (string.IsNullOrEmpty(responseMessageMainString))
                    return string.Empty;

                GroupCollection groups = Regex.Match(responseMessageMainString, "'<form action=\"(?<siteUrl>.*)\" method=\"POST\"").Groups;
                if (groups == null || groups["siteUrl"] == null)
                    return string.Empty;

                string siteUrl = groups["siteUrl"].Value;
                if (string.IsNullOrEmpty(siteUrl))
                    return string.Empty;

                siteUrl = WebUtility.HtmlDecode(siteUrl);

                groups = Regex.Match(responseMessageMainString, "<input id=\"ff3fix\" name=\"ff3fix\" type=\"hidden\" value=\"(?<ff3fix>\\d*)\" />").Groups;
                if (groups == null || groups["ff3fix"] == null)
                    return string.Empty;

                string ff3fix = groups["ff3fix"].Value;
                if (ff3fix == null)
                    return string.Empty;

                return siteUrl;
            }
            catch (Exception e)
            {
                return string.Empty;
            }
        }

        private string getSiteUrl(BetburgerInfo info)
        {
            try
            {
                if (info.bookmaker == "Betsson")
                    info.eventUrl += "&domain=www.betsafe.com";

                HttpClient httpClientBetburger = getHttpClientBetburger();

                httpClientBetburger.DefaultRequestHeaders.Referrer = new Uri("https://www.betburger.com/arbs");
                HttpResponseMessage responsemessageMain = httpClientBetburger.GetAsync(info.eventUrl).Result;
                responsemessageMain.EnsureSuccessStatusCode();

                string mainReferer = responsemessageMain.RequestMessage.RequestUri.AbsoluteUri;
                if (string.IsNullOrEmpty(mainReferer))
                    return string.Empty;

                string responseMessageMainString = responsemessageMain.Content.ReadAsStringAsync().Result;
                if (string.IsNullOrEmpty(responseMessageMainString))
                    return string.Empty;

                GroupCollection groups = Regex.Match(responseMessageMainString, "direct_link = '(?<siteUrl>.*)';").Groups;
                if (groups == null || groups["siteUrl"] == null)
                    return string.Empty;

                string siteUrl = groups["siteUrl"].Value;
                if (string.IsNullOrEmpty(siteUrl))
                    return string.Empty;

                return siteUrl;
            }
            catch (Exception e)
            {
                return string.Empty;
            }
        }
    }
}
