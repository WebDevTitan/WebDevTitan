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

var socketUrl = "ws://localhost:16699";
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
            let command = JSON.parse(evt.data)
            //console.log("[websocket] type : " + command.type + " body : " + command.body);

            if (command.type === "script"){               
                portFromCS.postMessage({script: command.body});
            }
            else if (command.type === "openpage"){
                //evalJSfromApp(command.body)       
                OpenPage(command.body);
            }
            else if (command.type === "maximize"){
                MaximizeWindow();                
            }
            else if (command.type === "domain"){
                domain = command.body;      
                let message = {type: "domainresult", response: "true"};    
                ws.send(JSON.stringify(message));             
            }
            else if (command.type === "titlebar"){
                GetTitlebarHeight().then((result) => {                
                    //console.log("[background] titlebarresult: " + result);          
                    let message = {type: "titlebarresult", response: result};    
                    ws.send(JSON.stringify(message));      
                });           
            }
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
    if (!details.url.includes("addbet") && !details.url.includes("placebet"))
        return;

    let filter = browser.webRequest.filterResponseData(details.requestId);
    let decoder = new TextDecoder("utf-8");
    let encoder = new TextEncoder();
  
    filter.ondata = event => {
      let str = decoder.decode(event.data, {stream: true});
      // Just change any instance of Example in the HTTP response
      // to WebExtension Example.
      //console.log(details.url);
      //console.log(str);
      
      //filter.write(encoder.encode(str));

      let message = {type: "webrequest", url: details.url, response: str};
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
    ["blocking"]
  );
  

function logResponse(responseDetails) {
    //console.log(responseDetails.url);
    //console.log(responseDetails.statusCode);    
  }
browser.webRequest.onCompleted.addListener(
  logResponse,
  {urls: ["*://*/*"]}
);
  
console.log("base bridge is loaded!");