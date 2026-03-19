using System.DirectoryServices;
using System.Text;

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
        // GOOD: encode user input before using in DN and search filter
        string safeDn = LdapDistinguishedNameEncode(organizationName);
        string ldapPath = "LDAP://myserver/OU=People,O=" + safeDn;
        using (DirectoryEntry root = new DirectoryEntry(ldapPath))
        {
            string safeFilter = LdapFilterEncode(userName);
            using (DirectorySearcher ds = new DirectorySearcher(root, "username=" + safeFilter))
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

// Encodes a string for safe use in an LDAP search filter (RFC 4515).
static string LdapFilterEncode(string input)
{
    var sb = new StringBuilder();
    foreach (char c in input)
    {
        switch (c)
        {
            case '\\': sb.Append(@"\5c"); break;
            case '*':  sb.Append(@"\2a"); break;
            case '(':  sb.Append(@"\28"); break;
            case ')':  sb.Append(@"\29"); break;
            case '\0': sb.Append(@"\00"); break;
            default:   sb.Append(c); break;
        }
    }
    return sb.ToString();
}

// Encodes a string for safe use in an LDAP Distinguished Name (RFC 4514).
static string LdapDistinguishedNameEncode(string input)
{
    if (string.IsNullOrEmpty(input))
        return string.Empty;

    var sb = new StringBuilder();
    for (int i = 0; i < input.Length; i++)
    {
        char c = input[i];
        // Escape leading space or '#'
        if (i == 0 && (c == ' ' || c == '#'))
        {
            sb.Append('\\');
            sb.Append(c);
            continue;
        }
        // Escape trailing space
        if (i == input.Length - 1 && c == ' ')
        {
            sb.Append('\\');
            sb.Append(c);
            continue;
        }
        switch (c)
        {
            case '\\': sb.Append(@"\\"); break;
            case ',':  sb.Append(@"\,"); break;
            case '+':  sb.Append(@"\+"); break;
            case '"':  sb.Append("\\\""); break;
            case '<':  sb.Append(@"\<"); break;
            case '>':  sb.Append(@"\>"); break;
            case ';':  sb.Append(@"\;"); break;
            case '=':  sb.Append(@"\="); break;
            default:   sb.Append(c); break;
        }
    }
    return sb.ToString();
}
