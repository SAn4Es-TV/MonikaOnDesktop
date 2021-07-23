/*chrome.tabs.onUpdated.addListener(function (tabId, changeInfo, tab) {
    if (changeInfo.url) {
        var http = new XMLHttpRequest();
        http.open("GET", "http:/localhost:2005/?myurl=" + changeInfo.url, true);
        http.send();
    }
});

var firstHref = $("a[href^='http']").eq(0).attr("href");

var http = new XMLHttpRequest();
http.open("GET", "http:/localhost:2005/?myurl=" + firstHref, true);
http.send();
console.log(firstHref);*/