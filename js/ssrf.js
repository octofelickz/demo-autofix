import http from 'http';

const server = http.createServer(function(req, res) {
    const url = new URL(req.url, "http://localhost:3000");

    if (url.pathname === "/fetch") {
        const target = url.searchParams.get("target");

        if (!target) {
            res.writeHead(400, { "Content-Type": "text/plain" });
            res.end("Missing 'target' query parameter");
            return;
        }

        // BAD: `target` is controlled by the attacker
        const fetchUrl = 'http://' + target + ".example.com/data/";
        console.log("Fetching:", fetchUrl);

        http.get(fetchUrl, (upstream) => {
            let body = '';
            upstream.on('data', chunk => body += chunk);
            upstream.on('end', () => {
                res.writeHead(200, { "Content-Type": "text/plain" });
                res.end("Response from upstream: " + body);
            });
        }).on('error', (err) => {
            res.writeHead(502, { "Content-Type": "text/plain" });
            res.end("Upstream error: " + err.message);
        });
    } else {
        res.writeHead(200, { "Content-Type": "text/plain" });
        res.end("SSRF Demo Server. Try GET /fetch?target=somehost");
    }
});

const PORT = 3000;
server.listen(PORT, () => {
    console.log(`SSRF demo server listening on http://localhost:${PORT}`);
    console.log(`Try: http://localhost:${PORT}/fetch?target=somehost`);
});
