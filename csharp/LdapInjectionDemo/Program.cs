using System.DirectoryServices;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", () => "LDAP Injection Demo Server\n" +
    "Try: GET /search?username=admin&organization=MyOrg\n");

app.MapGet("/search", (string? username, string? organization) =>
{
    if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(organization))
    {
        return Results.BadRequest("Missing 'username' or 'organization' query parameter");
    }

    try
    {
        // BAD: User input used in DN (Distinguished Name) without encoding
        string ldapPath = "LDAP://myserver/OU=People,O=" + organization;

        using (DirectoryEntry root = new DirectoryEntry(ldapPath))
        {
            // BAD: User input used in search filter without encoding
            using (DirectorySearcher ds = new DirectorySearcher(root, "username=" + username))
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
