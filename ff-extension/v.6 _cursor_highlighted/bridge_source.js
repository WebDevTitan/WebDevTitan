var socketUrl = "ws://localhost:12323";
var IsConnected = false;
var ws = null;

var last_wclPageContainerScrollTop = -1000;
var last_ovmOverviewScrollerScrollTop = -1000;
var last_ipeEventViewDetailScrollerScrollTop = -1000;

function removeLineCharacters(param) {
    return param.replace(/(\\r\\n|\\n|\\r)/gm, "");
}

function isNumber(n) {
	return /^(-?|\\+?)[\\d.]+(?:e-?\\d+)?$/.test(n);
}

function getDomElement(tagName, classLabel, outerText = "") {
    var domArray = [];
    domArray.push(ns_gen5_ui.Application.currentApplication.getElementChildren()[0].childNodes);
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

function getXPath(element) {
    var paths = [];
    for (; element && element.nodeType == Node.ELEMENT_NODE; element = element.parentNode) {
        var index = 0;
        var hasFollowingSiblings = false;
        for (var sibling = element.previousSibling; sibling; sibling = sibling.previousSibling) {
            if (sibling.nodeType == Node.DOCUMENT_TYPE_NODE)
                continue;
            if (sibling.nodeName == element.nodeName)
                ++index;
        }
        for (var sibling = element.nextSibling; sibling && !hasFollowingSiblings; sibling = sibling.nextSibling) {
            if (sibling.nodeName == element.nodeName)
                hasFollowingSiblings = true;
        }
        var tagName = (element.prefix ? element.prefix + ":" : "") + element.localName;
        var pathIndex = (index || hasFollowingSiblings ? "[" + (index + 1) + "]" : "");
        paths.splice(0, 0, tagName + pathIndex);
    }
    return paths.length ? "/" + paths.join("/") : null;
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

    try {
        if (BetSlipLocator.betSlipManager.betslip.activeModule.slip.footer.stakeBox.stakeValueInputElement.onkeydown === null) {
            
            BetSlipLocator.betSlipManager.betslip.activeModule.slip.footer.stakeBox.stakeValueInputElement.onkeydown = function (event) {
                
                
                let message = {
                    type: "onStakeInput",
                    value: event.key,
                    xpath: getXPath(document.activeElement),
                    tagName: document.activeElement.tagName,
                    className: document.activeElement.className,
                    outerText: removeLineCharacters(document.activeElement.outerText)
                };
                try{
                    ws.send(JSON.stringify(message));
                }catch{}
            };
        }
    } catch {}

    try {
        if (BetSlipLocator.betSlipManager.betslip.activeModule.slip.bet.stakeBox.onkeydown === null) {
            
            BetSlipLocator.betSlipManager.betslip.activeModule.slip.bet.stakeBox.onkeydown = function (event) {
                
                
                let message = {
                    type: "onStakeInput",
                    value: event.key,
                    xpath: getXPath(document.activeElement),
                    tagName: document.activeElement.tagName,
                    className: document.activeElement.className,
                    outerText: removeLineCharacters(document.activeElement.outerText)
                };
                try{
                    ws.send(JSON.stringify(message));
                }catch{}
            };
        };
    } catch {}

    var wclPageContainerDom = getDomElement("DIV", "wcl-PageContainer ");
    if (wclPageContainerDom !== null && wclPageContainerDom.onscroll === null) {
        
        wclPageContainerDom.onscroll = function () {
            if (Math.abs(this.scrollTop - last_wclPageContainerScrollTop) < 100)
                return;
            last_wclPageContainerScrollTop = this.scrollTop;
            last_ovmOverviewScrollerScrollTop = -1000;
            last_ipeEventViewDetailScrollerScrollTop = -1000;
            let message = {
                type: "scroll",
                target: "wcl-PageContainer ",
                scrollTop: this.scrollTop,
                scrollHeight: this.scrollHeight,
                clientHeight: this.clientHeight
            };
            try{
                ws.send(JSON.stringify(message));
            }catch{}
        };
    }
    var ovmOverviewScroller = getDomElement("DIV", "ovm-OverviewScroller ");
    if (ovmOverviewScroller !== null && ovmOverviewScroller.onscroll === null) {
        
        ovmOverviewScroller.onscroll = function () {
            if (Math.abs(this.scrollTop - last_ovmOverviewScrollerScrollTop) < 100)
                return;
            last_wclPageContainerScrollTop = -1000;
            last_ovmOverviewScrollerScrollTop = this.scrollTop;
            last_ipeEventViewDetailScrollerScrollTop = -1000;
            let message = {
                type: "scroll",
                target: "ovm-OverviewScroller ",
                scrollTop: this.scrollTop,
                scrollHeight: this.scrollHeight,
                clientHeight: this.clientHeight
            };
            try{
                ws.send(JSON.stringify(message));
            }catch{}
        };
    }
    var ipeEventViewDetailScroller = getDomElement("DIV", "ipe-EventViewDetailScroller ");
    if (ipeEventViewDetailScroller !== null && ipeEventViewDetailScroller.onscroll === null) {
        
        ipeEventViewDetailScroller.onscroll = function () {
            if (Math.abs(this.scrollTop - last_ipeEventViewDetailScrollerScrollTop) < 100)
                return;
            last_wclPageContainerScrollTop = -1000;
            last_ovmOverviewScrollerScrollTop = -1000;
            last_ipeEventViewDetailScrollerScrollTop = this.scrollTop;
            let message = {
                type: "scroll",
                target: "ipe-EventViewDetailScroller ",
                scrollTop: this.scrollTop,
                scrollHeight: this.scrollHeight,
                clientHeight: this.clientHeight
            };
            try{
                ws.send(JSON.stringify(message));
            }catch{}
        };
    }
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

window.onkeydown = function (event) {
    
    
    let message = {
        type: "onkeydown",
        value: event.key,
        xpath: getXPath(document.activeElement),
        tagName: document.activeElement.tagName,
        className: document.activeElement.className,
        outerText: removeLineCharacters(document.activeElement.outerText)
    };
    try{
        ws.send(JSON.stringify(message));
    }catch{}
};
window.onclick = e => {
    
    var twin = '';
    
    for (let i = 0; i < Locator.user._eRegister.oddsChanged.length; i++)
    {
        try
        {       
            let item = Locator.user._eRegister.oddsChanged[i];
            if (item == null || item.scope == null || item.scope.twinEmphasizedHandlerType == null)
                continue;               
            if (item.scope._active_element === e.target || item.scope._active_element === e.path[1])
            {
                
                twin = item.scope.twinEmphasizedHandlerType;
                break;
            }                       
        }   
        catch (err) {}
    }

    
    
    setTimeout(function () {
        
        let message = {
            type: "curUrl",
            url: ns_navlib_util.WebsiteNavigationManager.CurrentPageData
        };
        try{
            ws.send(JSON.stringify(message));
        }catch{}
    }, 1000);
    let message = {
        type: "onclick",
        twin: twin,
        xpath: getXPath(e.target),
        tagName: e.target.tagName,
        className: e.target.className,
        outerText: removeLineCharacters(e.target.outerText)
    };
    if (e.path.length > 1) {
        message = {
            type: "onclick",
            twin: twin,
            xpath: getXPath(e.target),
            tagName: e.target.tagName,
            className: e.target.className,
            outerText: removeLineCharacters(e.target.outerText),
            ptagName: e.path[1].tagName,
            pclassName: e.path[1].className,
            pouterText: removeLineCharacters(e.path[1].outerText)
        };
    }
    try{
        ws.send(JSON.stringify(message));
    }catch{}
};
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