using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

namespace BetburgerServer.Controller
{
    public class TipInfo
    {
        public string name { get; set; }
        public string param1 { get; set; }
        public string param2 { get; set; }
        public string param3 { get; set; }
        public string param4 { get; set; }
    }
    public enum HttpStatusCode
    {
        // for a full list of status codes, see..
        // https://en.wikipedia.org/wiki/List_of_HTTP_status_codes

        Continue = 100,

        Ok = 200,
        Created = 201,
        Accepted = 202,
        MovedPermanently = 301,
        Found = 302,
        NotModified = 304,
        BadRequest = 400,
        Forbidden = 403,
        NotFound = 404,
        MethodNotAllowed = 405,
        InternalServerError = 500
    }

    public class HttpRequest
    {
        public string Method { get; set; }
        public string Url { get; set; }
        public string Path { get; set; } // either the Url, or the first regex group
        public string Content { get; set; }
        public Route Route { get; set; }
        public Dictionary<string, string> Headers { get; set; }

        public string appid { get; set; }
        public string signature { get; set; }

        public HttpRequest()
        {
            this.Headers = new Dictionary<string, string>();
        }

        public override string ToString()
        {
            if (!string.IsNullOrWhiteSpace(this.Content))
                if (!this.Headers.ContainsKey("Content-Length"))
                    this.Headers.Add("Content-Length", this.Content.Length.ToString());

            return string.Format("{0} {1} HTTP/1.0\r\n{2}\r\n\r\n{3}", this.Method, this.Url, string.Join("\r\n", this.Headers.Select(x => string.Format("{0}: {1}", x.Key, x.Value))), this.Content);
        }
    }

    public class HttpResponse
    {
        public string StatusCode { get; set; }
        public string ReasonPhrase { get; set; }
        public byte[] Content { get; set; }

        public Dictionary<string, string> Headers { get; set; }

        public string ContentAsUTF8
        {
            set
            {
                this.setContent(value, encoding: Encoding.UTF8);
            }
        }
        public void setContent(string content, Encoding encoding = null)
        {
            if (encoding == null)
            {
                encoding = Encoding.UTF8;
            }
            Content = encoding.GetBytes(content);
        }

        public HttpResponse()
        {
            this.Headers = new Dictionary<string, string>();
        }

        // informational only tostring...
        public override string ToString()
        {
            return string.Format("HTTP status {0} {1}", this.StatusCode, this.ReasonPhrase);
        }
    }

    public class Route
    {
        public string Name { get; set; } // descriptive name for debugging
        public string UrlRegex { get; set; }
        public string Method { get; set; }
        public Func<HttpRequest, HttpResponse> Callable { get; set; }
    }

    static class Routes
    {
        private static Dictionary<string, List<Route>> _callbackActions = new Dictionary<string, List<Route>>()
              {
                      { "GET", new List<Route>() },
                      { "POST", new List<Route>() }
              };

        public static Dictionary<string, List<Route>> GET
        {
            get
            {
                return _callbackActions;
            }
        }
    }

    public class WebAPIServer
    {
        private static WebAPIServer m_Instance = null;
        public static WebAPIServer GetInstance()
        {
            if (m_Instance == null)
            {
                m_Instance = new WebAPIServer();
            }
            return m_Instance;
        }
                
        /************************************\
         * Simple Http Web Server           *
         * Huseyin Atasoy                   *
         * www.atasoyweb.net                *
         * huseyin@atasoyweb.net            *
        \************************************/

        public bool running = false; // Is it running?
        public object mutex = new object();
        private int timeout = 8; // Time limit for data transfers.
        private Encoding charEncoder = Encoding.UTF8; // To encode string
        private Socket serverSocket; // Our server socket
        private string contentPath; // Root path of our contents

        // Content types that are supported by our server
        // You can add more...
        // To see other types: http://www.webmaster-toolkit.com/mime-types.shtml
        private Dictionary<string, string> extensions = new Dictionary<string, string>()
        { 
            //{ "extension", "content type" }
            { "htm", "text/html" },
            { "html", "text/html" },
            { "xml", "text/xml" },
            { "txt", "text/plain" },
            { "css", "text/css" },
            { "png", "image/png" },
            { "gif", "image/gif" },
            { "jpg", "image/jpg" },
            { "jpeg", "image/jpeg" },
            { "zip", "application/zip"}
        };

        public bool start(IPAddress ipAddress, int port, int maxNOfCon, string contentPath)
        {
            if (running) return false; // If it is already running, exit.
            Trace.WriteLine($"Http server start {ipAddress.ToString()} {port}");
            try
            {
                // A tcp/ip socket (ipv4)
                serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                serverSocket.Bind(new IPEndPoint(ipAddress, port));
                serverSocket.Listen(maxNOfCon);
                serverSocket.ReceiveTimeout = timeout;
                serverSocket.SendTimeout = timeout;
                running = true;
                this.contentPath = contentPath;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error", MessageBoxButtons.OK);
                return false; 
            }

            // Our thread that will listen connection requests and create new threads to handle them.
            Thread requestListenerT = new Thread(() =>
            {
                while (running)
                {
                    Socket clientSocket;
                    try
                    {
                        clientSocket = serverSocket.Accept();
                        // Create new thread to handle the request and continue to listen the socket.
                        Thread requestHandler = new Thread(() =>
                        {
                            clientSocket.ReceiveTimeout = timeout;
                            clientSocket.SendTimeout = timeout;
                            try { handleTheRequest(clientSocket); }
                            catch (Exception ex)
                            {
                                try { clientSocket.Close(); } catch { }
                            }
                        });
                        requestHandler.Start();
                    }
                    catch { }
                }
            });
            requestListenerT.Start();

            return true;
        }

        public void stop()
        {
            if (running)
            {
                running = false;
                try { serverSocket.Close(); }
                catch { }
                serverSocket = null;
            }
        }

        private void handleTheRequest(Socket clientSocket)
        {
            try
            {
                Trace.WriteLine($"Http Request started");
                string strReceived = "";
                byte[] buffer = new byte[10240]; // 10 kb, just in case
                while (true)
                {
                    int receivedBCount = 0;
                    try
                    {
                        receivedBCount = clientSocket.Receive(buffer); // Receive the request
                    }
                    catch (Exception ex)
                    {
                        Trace.WriteLine($"Break 1 : {ex}");
                        break;
                    }
                    if (receivedBCount <= 0)
                    {
                        Trace.WriteLine($"Break 2 : {receivedBCount}");
                        break;
                    }
                    string strSegReceived = charEncoder.GetString(buffer, 0, receivedBCount);

                    Trace.WriteLine($"Http Request seg received length: {strSegReceived.Length}");
                    Trace.WriteLine(strSegReceived);

                    strReceived += strSegReceived;

                    if (strReceived.Contains("{"))
                        break;
                }
                Trace.WriteLine($"Http Request received length: {strReceived.Length}");
                Trace.WriteLine(strReceived);

                // Parse the method of the request
                string httpMethod = strReceived.Substring(0, strReceived.IndexOf(" "));

                int start = strReceived.IndexOf(httpMethod) + httpMethod.Length + 1;
                int length = strReceived.LastIndexOf("HTTP") - start - 1;
                string requestedUrl = strReceived.Substring(start, length);

                int lenHeader = strReceived.LastIndexOf("{");
                string strHeaders = strReceived.Substring(0, lenHeader > 0 ? lenHeader : strReceived.Length);
                string[] headers = strHeaders.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

                Dictionary<string, string> dicHeaders = new Dictionary<string, string>();
                foreach (string header in headers)
                {
                    if (!header.Contains(": ")) continue;

                    string key = header.Split(new string[] { ": " }, StringSplitOptions.RemoveEmptyEntries)[0];
                    string val = header.Split(new string[] { ": " }, StringSplitOptions.RemoveEmptyEntries)[1];
                    dicHeaders.Add(key, val);
                }
                Trace.WriteLine($"Http Request received header counts {dicHeaders.Count}");

                HttpRequest request = null;
                IDictionary<string, object> requestedParams = null;

                if (requestedUrl.Contains("?"))
                {
                    Trace.WriteLine($"Http Request received contains ?");
                    string[] parameters = requestedUrl.Split(new char[] { '?' }, StringSplitOptions.RemoveEmptyEntries)[1].Split(new char[] { '&' }, StringSplitOptions.RemoveEmptyEntries);

                    dynamic requestedParameters = new System.Dynamic.ExpandoObject();

                    foreach (string param in parameters)
                    {
                        string[] strKeyValuePair = param.Split(new char[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
                        string key = strKeyValuePair.Length > 0 ? strKeyValuePair[0] : null;
                        string val = strKeyValuePair.Length > 1 ? strKeyValuePair[1] : null;

                        if (string.IsNullOrEmpty(key)) continue;

                        if (!((IDictionary<string, object>)requestedParameters).ContainsKey(System.Web.HttpUtility.UrlDecode(key)))
                        {
                            ((IDictionary<string, object>)requestedParameters).Add(System.Web.HttpUtility.UrlDecode(key), System.Web.HttpUtility.UrlDecode(val));
                        }
                        else
                        {
                            ((IDictionary<string, object>)requestedParameters)[System.Web.HttpUtility.UrlDecode(key)] = System.Web.HttpUtility.UrlDecode(val);
                        }
                    }

                    //request = new HttpRequest
                    //{
                    //    Headers = dicHeaders,
                    //    Content = JsonConvert.SerializeObject(requestedParameters)
                    //};

                    requestedParams = (IDictionary<string, object>)requestedParameters;
                }

                Dictionary<string, object> requestBodies = new Dictionary<string, object>();

                Trace.WriteLine($"Http Request received httpMethod {httpMethod}");
                if (httpMethod.Equals("POST"))
                {
                    Trace.WriteLine($"Http Request strReceived {strReceived}");

                    string requestContent = strReceived.Substring(strReceived.LastIndexOf("{"));
                    requestBodies = JsonConvert.DeserializeObject<Dictionary<string, object>>(requestContent);

                    Trace.WriteLine($"Http Request received requestBodies count {requestBodies.Count}");

                }

                //string strContent = JsonConvert.SerializeObject(requestedParams.Concat(requestBodies).GroupBy(d => d.Key).ToDictionary(d => d.Key, d => d.First().Value));
                string strContent = JsonConvert.SerializeObject(requestBodies.GroupBy(d => d.Key).ToDictionary(d => d.Key, d => d.First().Value));

                Trace.WriteLine($"Http Request received strContent {strContent}");

                request = new HttpRequest
                {
                    //Method
                    //Url
                    //Path
                    //Route
                    //signature = requestedParams.ContainsKey("signature") ? (string)requestedParams["signature"] : "",
                    Headers = dicHeaders,
                    Content = strContent
                };

                if (httpMethod.Equals("POST"))
                {
                    foreach (Route route in Routes.GET["POST"])
                    {
                        Match match = Regex.Match(requestedUrl, route.UrlRegex);
                        if (!match.Success)
                        {
                            continue;
                        }
                        HttpResponse response = route.Callable(request);
                        sendResponse(clientSocket, response.Content, response.StatusCode + " " + response.ReasonPhrase, "text/html");
                        return;
                    }

                    return;
                }

                foreach (Route route in Routes.GET["GET"])
                {
                    var match = Regex.Match(requestedUrl, route.UrlRegex);
                    if (!match.Success)
                    {
                        continue;
                    }

                    HttpResponse response = route.Callable(request);
                    sendResponse(clientSocket, response.Content, response.StatusCode + " " + response.ReasonPhrase, "text/html");
                    return;
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine("handlRequset Exception: " + ex);
            }
            notFound(clientSocket);
        }

        private void notImplemented(Socket clientSocket)
        {
            sendResponse(clientSocket, "<html><head><meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\"></head><body><h2>NinjaSlot Web API Server</h2><div>501 - Method Not Implemented</div></body></html>", "501 Not Implemented", "text/html");
        }

        private void notFound(Socket clientSocket)
        {
            sendResponse(clientSocket, "<html><head><meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\"></head><body><h2>NinjaSlot Web API Server</h2><div>404 - Not Found</div></body></html>", "404 Not Found", "text/html");
        }

        private void sendOkResponse(Socket clientSocket, byte[] bContent, string contentType)
        {
            sendResponse(clientSocket, bContent, "200 OK", contentType);
        }

        // For strings
        private void sendResponse(Socket clientSocket, string strContent, string responseCode, string contentType)
        {
            byte[] bContent = charEncoder.GetBytes(strContent);
            sendResponse(clientSocket, bContent, responseCode, contentType);
        }

        // For byte arrays
        private void sendResponse(Socket clientSocket, byte[] bContent, string responseCode, string contentType)
        {
            //GameClientService.Instance.SendOnlyFileLog(charEncoder.GetString(bContent, 0, bContent.Length));
            try
            {
                byte[] bHeader = charEncoder.GetBytes(
                                    "HTTP/1.1 " + responseCode + "\r\n"
                                  + "Server: NinjaSlot Web Server\r\n"
                                  + "Content-Length: " + bContent.Length.ToString() + "\r\n"
                                  + "Connection: close\r\n"
                                  + "Content-Type: " + contentType + "\r\n\r\n");
                clientSocket.Send(bHeader);
                clientSocket.Send(bContent);
                clientSocket.Close();
            }
            catch { }
        }
    }
}
