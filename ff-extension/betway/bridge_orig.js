console.log("page bridge is loaded!");


var myPort = browser.runtime.connect({name:"port-from-cs"});
//myPort.postMessage({response: "content script connected"});

myPort.onMessage.addListener(function(m) {
  //console.log("[content] " + m.script);  
  let result = window.eval(m.script);
  //console.log("[content] result: " + result);  
  myPort.postMessage({response: result});
});
