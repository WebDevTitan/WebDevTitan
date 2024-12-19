console.log("bridge extension is loaded!");

const bridgeTag = document.createElement('script');
bridgeTag.text =
                    'var socketUrl = "ws://localhost:6699";\n'+                    
                    'var IsConnected = false;\n' + 
                    'var ws = null;\n'+
                    'function connectWebsocket(){\n' + 
                    '    try{\n'+
                    '        try{ws.close()}catch{}\n'+
                    '        ws = new WebSocket(socketUrl);\n'+
                    '        ws.onopen = function (evt) {\n'+
                    // '            console.log("[websocket] Connected!");\n'+
                    '            IsConnected = true;\n'+                    
                    '        };\n'+
                    '        ws.onclose = function (evt) {\n'+
                    '            IsConnected = false;\n'+
                    // '            console.log("[websocket] Closed : " + evt);\n'+
                    '        };\n'+                    
                    '        ws.onerror = function (evt) {\n'+
                    '            IsConnected = false;\n'+
                    // '            console.log("[websocket] Error : " + evt);\n'+
                    '        };\n'+
                    '        ws.onmessage = function (evt) {\n'+
                    '            let payload = JSON.parse(evt.data)\n'+
                    '            if(payload.type === "jscode"){\n'+
                    '               evalJSfromApp(payload.body)\n'+
                    '            }\n'+
                    '        };\n'+
                    '    }catch(ex){\n'+
                    // '        console.log(ex);\n'+
                    '    }\n'+
                    '}\n'+
                    'setInterval(function(){\n'+
                    '    if(IsConnected === false)\n'+
                    '        connectWebsocket();\n'+
                    '}, 2000);\n'+  
                    'function evalJSfromApp(jsCode){\n' +
                    // '   console.log("eval cmd: " + jsCode);\n' +
                    '   let res = window.eval(jsCode);\n' +
                    // '   console.log("eval res: " + res);\n' +
                    '   let message = {url: "jsresult", response: res};\n'+
                    '   ws.send(JSON.stringify(message));\n'+
                    '}\n' +
                    'XMLHttpRequest.prototype.origOpen = XMLHttpRequest.prototype.open;\n'+
                    'XMLHttpRequest.prototype.open = function() {\n'+
                    '   this.addEventListener("load", function() {\n'+
                    '    if(this.responseURL.includes("addbet") ||this.responseURL.includes("placebet") ||this.responseURL.includes("referbet") || this.responseURL.includes("pollreferredbet") || this.responseURL.includes("uicountersapi/increment?desktop_count") || this.responseURL.includes("defaultapi/sports-configuration")){\n'+
                    '        let message = {url: this.responseURL, response: this.response};\n'+
                    '        ws.send(JSON.stringify(message));\n'+
                    '    }\n'+
                    '});\n'+
                    'this.origOpen.apply(this, arguments);\n'+
                '};';

document.getElementsByTagName('head')[0].appendChild(bridgeTag);
