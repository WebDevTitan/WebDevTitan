
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
            let command = JSON.parse(evt.data)
            //console.log("[websocket] type : " + command.type + " body : " + command.body);     
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
    console.log("background-content script connected")
  portFromCS = p;
  
  portFromCS.onMessage.addListener(function(m) {
    console.log("[background] websocketdata: " + m.data);    
    let message = {type: "websocketdata", data: m.data};    
    if (ws != null)
        ws.send(JSON.stringify(message));    
  });
}

chrome.runtime.onConnect.addListener(connected);
 

function logResponse(responseDetails) {
    console.log("logResponse!");
    portFromCS.postMessage({script: 'capturesock'});
  }
chrome.webRequest.onCompleted.addListener(
  logResponse,
  {urls: ["*://*/*"]}
);
  
console.log("base bridge is loaded!");