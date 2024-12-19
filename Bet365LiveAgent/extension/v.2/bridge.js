console.log("page bridge is loaded!");

var myPort = chrome.runtime.connect({name:"port-from-cs"});
myPort.postMessage({data: "content script connected"});

myPort.onMessage.addListener(function(m) {
  console.log("[content] receive message");  
  if (m.script == 'capturesock')
  {
    console.log("[content] " + m.script);  
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
                    myPort.postMessage(message);                      
                  }catch{}
                  return this.origDataCallback(body);
              };
              console.log("[content] websocket hooked");  
          }
      }
  }    
  catch{}
  }
  //let result = window.eval(m.script);
  //console.log("[content] result: " + result);  
  
});
