var express = require("express");
var http = require("http");

var app = express();

app.get("/fetch", function (req, res) {
    // BAD: `target` is controlled by the attacker (SSRF)
    http.get("http://" + req.query.target + ".example.com/data/", function (response) {
        var body = "";
        response.on("data", function (chunk) { body += chunk; });
        response.on("end", function () {
            res.send("Response from upstream: " + body);
        });
    }).on("error", function (err) {
        res.status(502).send("Upstream error: " + err.message);
    });
});

app.get("/", function (req, res) {
    res.send("SSRF Demo Server.\nTry: GET /fetch?target=somehost\n");
});

app.listen(3000, function () {
    console.log("SSRF demo server listening on http://localhost:3000");
});
