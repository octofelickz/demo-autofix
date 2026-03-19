using System.DirectoryServices;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", () => "LDAP Injection Demo Server\nTry: GET /search?username=admin&organization=MyOrg\n");

app.MapGet("/search", (HttpContext ctx) =>
{
    string userName = ctx.Request.Query["username"];
    string organizationName = ctx.Request.Query["organization"];

    if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(organizationName))
    {
        return Results.BadRequest("Missing 'username' or 'organization' query parameter");
    }

    try
    {
        // BAD: User input used in DN (Distinguished Name) without encoding
        string ldapPath = "LDAP://myserver/OU=People,O=" + organizationName;
        using (DirectoryEntry root = new DirectoryEntry(ldapPath))
        {
            // BAD: User input used in search filter without encoding
            using (DirectorySearcher ds = new DirectorySearcher(root, "username=" + userName))
            {
                SearchResult? result = ds.FindOne();
                if (result != null)
                {
                    return Results.Ok("Found user in directory");
                }
            }
        }
    }
    catch (Exception ex)
    {
        return Results.Problem("LDAP query error: " + ex.Message);
    }

    return Results.NotFound("User not found");
});

app.Run();
