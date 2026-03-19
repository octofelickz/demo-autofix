var express = require("express");
var http = require("http");

var app = express();

var ALLOWED_TARGETS = {
    "eu": "europe",
    "us": "america",
    "ap": "asia"
};

app.get("/fetch", function (req, res) {
    var subdomain = ALLOWED_TARGETS[req.query.target];
    if (!subdomain) {
        return res.status(400).send("Invalid target. Allowed values: " + Object.keys(ALLOWED_TARGETS).join(", "));
    }
    // GOOD: `subdomain` is controlled by the server via allowlist
    http.get("http://" + subdomain + ".example.com/data/", function (response) {
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
