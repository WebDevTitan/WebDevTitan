using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace BetburgerServer
{
    public class CustomEndpoint
    {
        public static void sendNewTips(string payload)
        {
            try
            {
                HttpClient client = getHttpClient();
                var payloadPost = new StringContent(payload, Encoding.UTF8, "application/json");

                HttpResponseMessage ipResponse = client.PostAsync($"http://176.223.142.38:9002/interface/esport_bets", payloadPost).Result;
                ipResponse.EnsureSuccessStatusCode();
                string content = ipResponse.Content.ReadAsStringAsync().Result;
            }
            catch (Exception ex)
            {
                int a = 1;
            }
        }

        public static void sendPinnacleNewFeeds(string payload)
        {
            try
            {
                HttpClient client = getHttpClient();
                var payloadPost = new StringContent(payload, Encoding.UTF8, "application/json");
                HttpResponseMessage ipResponse = client.PostAsync($"http://211.37.175.6:9002/interface/pin_tip", payloadPost).Result;
                ipResponse.EnsureSuccessStatusCode();
                string content = ipResponse.Content.ReadAsStringAsync().Result;
            }
            catch (Exception ex)
            {
                int a = 1;
            }
        }
        public static HttpClient getHttpClient(string proxy = "")
        {
            HttpClientHandler handler;

            if (!string.IsNullOrEmpty(proxy) && proxy != null)
            {
                var proxyURI = new Uri(string.Format("{0}{1}", "http://", proxy));
                WebProxy proxyItem;
                var useAuth = false;
                /*
                var credentials = new NetworkCredential(Setting.instance.proxyUsername, Setting.instance.proxyPassword);
                proxyItem = new WebProxy(proxyURI, true, null, credentials);
                useAuth = true;
                */
                proxyItem = new WebProxy(proxyURI, false);
                handler = new HttpClientHandler()
                {
                    Proxy = proxyItem,
                    UseProxy = true,
                    PreAuthenticate = useAuth,
                    UseDefaultCredentials = !useAuth,
                };
            }
            else
            {
                handler = new HttpClientHandler()
                {
                    AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
                };
            }
            HttpClient httpClientEx = new HttpClient(handler);

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
            /*
            ServicePointManager.DefaultConnectionLimit = 200;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            ServicePointManager.Expect100Continue = false;
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            */
            httpClientEx.Timeout = new TimeSpan(0, 0, 5);
            httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "text/html, application/xhtml+xml, application/xml; q=0.9, image/webp, */*; q=0.8");
            httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 6.3; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/47.0.2526.111 Safari/537.36");
            httpClientEx.DefaultRequestHeaders.ExpectContinue = false;
            return httpClientEx;
        }
    }
}
