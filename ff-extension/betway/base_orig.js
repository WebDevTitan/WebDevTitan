var domain = 'bet365.com';
const DEFAULT_ZOOM = 1;

function getCurrentWindow() {
    return browser.windows.getCurrent();
}

function MaximizeWindow() 
{
    getCurrentWindow().then((currentWindow) => {
      var updateInfo = {
        state: "maximized"
      };

      browser.windows.update(currentWindow.id, updateInfo).then(() => {
        let message = {type: "maximizeresult", response: "true"};    
        ws.send(JSON.stringify(message));      
      });      
    });
}

function getCurrentWindowTabs() {
    return browser.tabs.query({currentWindow: true});
}

function callOnActiveTab(callback) {
    getCurrentWindowTabs().then((tabs) => {
      for (var tab of tabs) {
        if (tab.active) {
          callback(tab, tabs);
        }
      }
    });
}

function ActiveB365tab()
{
    //active bet365 tab
    var tabexist = false;
    browser.tabs.query({
        currentWindow: true
    }).then((tabs) => {
        for (var tab of tabs) {
            if (tab.url.includes(domain)) {
                browser.tabs.update(tab.id, {
                    active: true
                });
                tabexist = true;
                break;
            }
        }
    });

    if (!tabexist)
    {
        browser.tabs.create({url: "https://www." + domain});
    }

    //zoom 100    
    callOnActiveTab((tab) => {
        var gettingZoom = browser.tabs.getZoom(tab.id);
        gettingZoom.then((zoomFactor) => {
            browser.tabs.setZoom(tab.id, DEFAULT_ZOOM);        
        });
    });
}

function OpenPage(newurl)
{
    let tabId;
    browser.tabs.create({url: newurl}).then((newtab)=>{
        tabId = newtab.id;
        browser.tabs.query({}).then((tabs) => {
            Promise.all(tabs.filter((v) => v.id !== tabId).map((itrtab) => browser.tabs.remove(itrtab.id))).then(() => {
                callOnActiveTab((tab) => {
                    var gettingZoom = browser.tabs.getZoom(tab.id);
                    gettingZoom.then((zoomFactor) => {
                        browser.tabs.setZoom(tab.id, DEFAULT_ZOOM);  

                        let message = {type: "openpageresult", response: "true"};    
                        ws.send(JSON.stringify(message));        
                    });
                });
            });
        });
      });
}

function GetTitlebarHeight()
{
    return new Promise((resolve, reject) => {
        var height = 0;
        getCurrentWindow().then((currentWindow) => {
            height = currentWindow.height;        
            //console.log("window height: " + height);
          }).then(() => {
              return callOnActiveTab((tab) => {     
                //console.log("tab height: " + tab.height);       
                height = height - tab.height;                                  
                resolve(height);
                //console.log("titlebar height: " + height);       
            });
          }).then(() => {
          }).catch((e) => {
              reject(e);
          });
    })
}

function reloadPage()
{
    callOnActiveTab((tab) => {
        browser.tabs.reload(tab.id);
      });
}

var socketUrl = "ws://localhost:12323";
var IsConnected = false;
var ws = null;
  
function connectWebsocket(){
    try{
        try{
            ws.close();
        }
        catch{}

        ws = new WebSocket(socketUrl);
        ws.onopen = function (evt) {
            IsConnected = true;
            console.log("[websocket] Connected!");
        };
        ws.onclose = function (evt) {
            IsConnected = false;
            console.log("[websocket] Closed : " + evt);
        };
        ws.onerror = function (evt) {
            IsConnected = false;
            console.log("[websocket] Error : " + evt);
        };
        ws.onmessage = function (evt) {            
            //console.log("[websocket] type : " + evt.data);
            portFromCS.postMessage({script: evt.data});           
            
        };
    }catch(ex){
        console.log(ex);
    }
}

setInterval(function(){
    if(IsConnected === false)
        connectWebsocket();
}, 2000);  

var portFromCS;

function connected(p) {
    //console.log("background-content script connected")
  portFromCS = p;
  
  portFromCS.onMessage.addListener(function(m) {
    //console.log("[background] scriptresult: " + m.response);    
    let message = {type: "scriptresult", response: m.response};    
    if (ws != null)
        ws.send(JSON.stringify(message));    
  });
}

browser.runtime.onConnect.addListener(connected);

function listener(details) {
    //console.log(JSON.stringify(details));
        
    if (!details.url.includes("api/Account/v3/LogIn") && !details.url.includes("api/Events/v2/GetEventMarkets") && !details.url.includes("api/Betting/v3/BuildBets")
    && !details.url.includes("api/Betting/v3/InitiateBets") && !details.url.includes("api/Betting/v3/LookupBets") && !details.url.includes("api/Account/v3/Info"))
        return;

    var postedString = decodeURIComponent(String.fromCharCode.apply(null, new Uint8Array(details.requestBody.raw[0].bytes)));
    //console.log('webrequest');
    //console.log(details.url);
    //console.log(postedString);

    let message = {type: "webrequest", url: details.url, response: postedString};
    if (ws != null)
      ws.send(JSON.stringify(message));


    let filter = browser.webRequest.filterResponseData(details.requestId);
    let decoder = new TextDecoder("utf-8");
    let encoder = new TextEncoder();
  
    filter.ondata = event => {
      let str = decoder.decode(event.data, {stream: true});
      // Just change any instance of Example in the HTTP response
      // to WebExtension Example.
       //console.log('webresponse');
       //console.log(details.url);
       //console.log(str);
      
      //filter.write(encoder.encode(str));

      let message = {type: "webresponse", url: details.url, response: str};
      if (ws != null)
        ws.send(JSON.stringify(message));

      filter.write(event.data);
      filter.disconnect();
    }
  
    return {};
  }
browser.webRequest.onBeforeRequest.addListener(
    listener,
    //{urls: ["*://*/*"], types: ["xmlhttprequest", "websocket"]},
    {urls: ["*://*/*"], types: ["xmlhttprequest"]},
    ["blocking", "requestBody"]
  );
  

function logResponse(responseDetails) {
    //console.log('logResponse');
    //console.log(responseDetails);    
   
  }
browser.webRequest.onCompleted.addListener(
  logResponse,
  {urls: ["*://*/*"]}
);
  
console.log("base bridge is loaded!");