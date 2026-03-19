var http = require("http");
var url = require("url");

var server = http.createServer(function (req, res) {
    var queryData = url.parse(req.url, true).query;

    if (queryData.target) {
        // BAD: `target` is controlled by the attacker
        var fetchUrl = "http://" + queryData.target + ".example.com/data/";

        http.get(fetchUrl, function (upstream) {
            var body = "";
            upstream.on("data", function (chunk) { body += chunk; });
            upstream.on("end", function () {
                res.writeHead(200, { "Content-Type": "text/plain" });
                res.end("Response from upstream: " + body);
            });
        }).on("error", function (err) {
            res.writeHead(502, { "Content-Type": "text/plain" });
            res.end("Upstream error: " + err.message);
        });
    } else {
        res.writeHead(200, { "Content-Type": "text/plain" });
        res.end("SSRF Demo Server.\nTry: GET /?target=somehost\n");
    }
});

server.listen(3000, function () {
    console.log("SSRF demo server listening on http://localhost:3000");
});
