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
        // GOOD: Organization name is encoded before being used in DN (RFC 4514)
        string safeLdapPath = "LDAP://myserver/OU=People,O=" + LdapEncoder.EncodeDnValue(organizationName);
        using (DirectoryEntry root = new DirectoryEntry(safeLdapPath))
        {
            // GOOD: User input is encoded before being used in search filter (RFC 4515)
            using (DirectorySearcher ds = new DirectorySearcher(root, "username=" + LdapEncoder.EncodeFilterValue(userName)))
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

static class LdapEncoder
{
    // Encodes a value for use in an LDAP search filter per RFC 4515.
    // Escapes: NUL, '(', ')', '*', '\', and non-ASCII bytes.
    public static string EncodeFilterValue(string value)
    {
        var sb = new StringBuilder();
        foreach (char c in value)
        {
            switch (c)
            {
                case '\0': sb.Append("\\00"); break;
                case '(':  sb.Append("\\28"); break;
                case ')':  sb.Append("\\29"); break;
                case '*':  sb.Append("\\2a"); break;
                case '\\': sb.Append("\\5c"); break;
                default:
                    if (c > 0x7f)
                    {
                        foreach (byte b in Encoding.UTF8.GetBytes(c.ToString()))
                            sb.Append($"\\{b:x2}");
                    }
                    else
                    {
                        sb.Append(c);
                    }
                    break;
            }
        }
        return sb.ToString();
    }

    // Encodes a value for use in an LDAP DN attribute value per RFC 4514.
    // Escapes: ',', '\', '#', '+', '<', '>', ';', '"', '=', '/', NUL, and non-ASCII bytes.
    // Also escapes leading/trailing spaces.
    public static string EncodeDnValue(string value)
    {
        if (string.IsNullOrEmpty(value))
            return value;

        var sb = new StringBuilder();
        for (int i = 0; i < value.Length; i++)
        {
            char c = value[i];
            switch (c)
            {
                case '\0': sb.Append("\\00"); break;
                case ',':  sb.Append("\\,");  break;
                case '\\': sb.Append("\\\\"); break;
                case '#':  sb.Append("\\#");  break;
                case '+':  sb.Append("\\+");  break;
                case '<':  sb.Append("\\<");  break;
                case '>':  sb.Append("\\>");  break;
                case ';':  sb.Append("\\;");  break;
                case '"':  sb.Append("\\\""); break;
                case '=':  sb.Append("\\=");  break;
                case '/':  sb.Append("\\/");  break;
                case ' ' when (i == 0 || i == value.Length - 1):
                    sb.Append("\\ ");
                    break;
                default:
                    if (c > 0x7f)
                    {
                        foreach (byte b in Encoding.UTF8.GetBytes(c.ToString()))
                            sb.Append($"\\{b:x2}");
                    }
                    else
                    {
                        sb.Append(c);
                    }
                    break;
            }
        }
        return sb.ToString();
    }
}
