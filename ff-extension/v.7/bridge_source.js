var socketUrl = "ws://localhost:12323";
var IsConnected = false;
var ws = null;

function isNumber(n) {
	return /^(-?|\\+?)[\\d.]+(?:e-?\\d+)?$/.test(n);
}
function getDomElement(tagName, classLabel, outerText = "") {
    var domArray = [];
    domArray.push(ns_gen5_ui.Application.currentApplication.getElementChildren());
    var counter = 0;
    while (domArray.length > 0) {
        try {
            var curIterator = domArray.shift();
            for (var i = 0; i < curIterator.length; i++) {
                try {
                    if (curIterator[i].tagName.includes(tagName) &&
                        curIterator[i].className.includes(classLabel)) {
                        var checkouterText = true;
                        if (outerText != "" && !curIterator[i].outerText.includes(outerText)) {
                            checkouterText = false;
                        }
                        if (checkouterText) {
                            return curIterator[i];
                        }
                    }
                    domArray.push(curIterator[i].childNodes);
                } catch (ex1) {}
            }
        } catch (ex) {}
    }
    return null;
};

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
    
}, 2000);

function evalJSfromApp(jsCode) {
    
    try{
        console.log('eval code: ' + jsCode);
        let res = eval(jsCode);
        console.log('eval result: ' + res);
        let message = {
            type: "scriptresult",
            response: res
        };
        ws.send(JSON.stringify(message));
    }catch{}
}

XMLHttpRequest.prototype.origOpen = XMLHttpRequest.prototype.open;
XMLHttpRequest.prototype.open = function () {
    this.recordedMethod = arguments[0];
    this.recordedUrl = arguments[1];
    this.addEventListener("load", function () {
        if (this.responseURL.includes("addbet") || this.responseURL.includes("placebet") || this.responseURL.includes("referbet") || this.responseURL.includes("pollreferredbet") || this.responseURL.includes("uicountersapi/increment?desktop_count") || this.responseURL.includes("defaultapi/sports-configuration")) {
            let message = {
                type: "webresponse",
                resurl: this.responseURL,
                response: this.response
            };
            try{
                ws.send(JSON.stringify(message));
            }catch{}
        }
    });
    this.origOpen.apply(this, arguments);
};
XMLHttpRequest.prototype.origSend = XMLHttpRequest.prototype.send;
XMLHttpRequest.prototype.send = function (body) {
    const method = this.recordedMethod;
    const url = this.recordedUrl;
    if (url.includes("addbet") === true || url.includes("placebet") === true) {
        
        
        let message = {
            type: "webrequest",
            requrl: url,
            request: body
        };
        try{
        ws.send(JSON.stringify(message));
        }catch{}
    }
    return this.origSend(body);
};

async function getXcft() {
    return await new Promise((resolve) => {
        const counter = ns_gen5_net.Loader.Counter++;
        const listener = (callback) => {
            window.removeEventListener(`xcft${counter}`, listener);
            resolve(callback.detail);
        };
        window.addEventListener(`xcft${counter}`, listener);
        window.dispatchEvent(new CustomEvent('xcftr', { detail: counter }));
    });
}
  
async function getCurrentLocation() {
    return await new Promise((resolve, reject) => {
      navigator.geolocation.getCurrentPosition(
        function(position) {
          var latitude = position.coords.latitude;
          var longitude = position.coords.longitude;
          resolve({latitude, longitude});
        },
        function(error) {
          reject(error);
        }
      );
    });
  }

const observerCallback = (mutationsList, observer) => {
    for (const mutation of mutationsList) {
      if (mutation.type === 'childList' && mutation.addedNodes.length > 0) {
        // Loop through the added nodes to check for the target element
        mutation.addedNodes.forEach(async node => {
          if (node.nodeType === Node.ELEMENT_NODE) {
            const targetElement = node.querySelector('div[class*="atm-AuthenticatorModule"]');
            if (targetElement) {
                let xtoken = await getXcft();        
                let {latitude, longitude} = await getCurrentLocation();        
                let message = {
                    type: "qrPost",            
                    xcft: xtoken,
                    lat: latitude,
                    lon: longitude,
                };
                console.log(message);
                try{
                ws.send(JSON.stringify(message));
                }catch{} 
  
            }
          }
        });
      }
    }
  };
    
  const observer = new MutationObserver(observerCallback);  
  observer.observe(document.body, { childList: true, subtree: true });
  