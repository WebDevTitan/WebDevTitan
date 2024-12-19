console.log("page bridge is loaded!");
// const bridgeTag = document.createElement('script');
//                 bridgeTag.text += 'function add_element() {\n'+
//                     'console.log("onload");\n'+
//                     'var cursorObj = document.createElement("canvas");\n'+
//                     'cursorObj.id  = "cursor";\n'+
//                     'cursorObj.width = 10;\n'+
//                     'cursorObj.height = 10;\n'+
//                     'var context = cursorObj.getContext("2d");\n'+
//                     'context.beginPath();\n'+
//                     'context.arc(5, 5, 5, 0, 2 * Math.PI, false);\n'+
//                     'context.fillStyle = "red";\n'+
//                     'context.fill();\n'+
//                     'context.lineWidth = 1;\n'+
//                     'cursorObj.style.position = "absolute";\n'+
//                     'cursorObj.style["z-index"] = 10000;\n'+
//                     'document.body.appendChild(cursorObj);\n'+
//                 '}\n'+
//                 'setTimeout(function(){\n'+
//                 '    add_element();\n'+
//                 '}, 5000);\n'+
//                 '\n'+ 
//                 '\n'+ 
//                 'var pointerX = -1;\n'+
//                 'var pointerY = -1;\n'+
//                 '\n'+
//                 'var clickedX = -1;\n'+
//                 'var clickedY = -1;\n'+
//                 '\n'+
//                 '\n'+
//                 'document.onmousemove = function(event) {\n'+
//                 '    pointerX = event.pageX;\n'+
//                 '    pointerY = event.pageY;\n'+
//                 '}\n'+
//                 '\n'+
//                 'document.onclick = function(event) {\n'+
//                 '    clickedX = event.pageX;\n'+
//                 '    clickedY = event.pageY;\n'+
//                 '}\n'+
//                 '\n'+ 
//                 'setInterval(pointerCheck, 10);\n'+
//                 '\n'+
//                 'function pointerCheck() {\n'+
//                 '  try{\n'+
//                 '    document.getElementById("cursor").style.left = (pointerX)  + "px";\n'+
//                 '    document.getElementById("cursor").style.top = (pointerY) + "px";\n'+
//                 '                  \n'+
//                 '  }catch{\n'+
//                 '  }\n'+
//                 '}\n';

// document.getElementsByTagName('head')[0].appendChild(bridgeTag);


var myPort = browser.runtime.connect({name:"port-from-cs"});
myPort.postMessage({response: "content script connected"});

myPort.onMessage.addListener(function(m) {
  //console.log("[content] " + m.script);  
  let result = window.eval(m.script);
  //console.log("[content] result: " + result);  
  myPort.postMessage({response: result});
});
