var socketUrl = "ws://localhost:12323";
var IsConnected = false;
var ws = null;

function connectWebsocket() {
    try {
        try {
            ws.close()
        } catch {}
        ws = new WebSocket(socketUrl);
        ws.onopen = function (evt) {
            IsConnected = true;
        };
        ws.onclose = function (evt) {
            IsConnected = false;
        };
        ws.onerror = function (evt) {
            IsConnected = false;
        };
        ws.onmessage = function (evt) {
            let request = JSON.parse(evt.data);
            if (request.type === "script") {
                evalJSfromApp(request.body)
            }
        };
    } catch (ex) {}
}

setInterval(function () {
    if (IsConnected === false)
        connectWebsocket();
    
    try{        
        if (Locator.subscriptionManager._streamDataProcessor._serverConnection._currentTransportMethod != null)
        {            
            if (Locator.subscriptionManager._streamDataProcessor._serverConnection._currentTransportMethod.origDataCallback == null)
            {                
                Locator.subscriptionManager._streamDataProcessor._serverConnection._currentTransportMethod.origDataCallback = Locator.subscriptionManager._streamDataProcessor._serverConnection._currentTransportMethod.socketDataCallback;
                Locator.subscriptionManager._streamDataProcessor._serverConnection._currentTransportMethod.socketDataCallback = function (body) {                    
                    let message = {
                        type: "websocketdata",        
                        data: body
                    };
                    try{
                        ws.send(JSON.stringify(message));
                    }catch{}
                    return this.origDataCallback(body);
                };
            }
        }
    }    
    catch{}
}, 2000);

function evalJSfromApp(jsCode) {
    
    try{
        let res = eval(jsCode);
        
        let message = {
            type: "scriptresult",
            response: res
        };
        ws.send(JSON.stringify(message));
    }catch{}
}


