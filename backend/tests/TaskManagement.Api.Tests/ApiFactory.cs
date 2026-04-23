using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace TaskManagement.Api.Tests;

public class ApiFactory : WebApplicationFactory<Program>
{
    public string DbPath { get; } = Path.Combine(Path.GetTempPath(), $"taskmgmt-api-{Guid.NewGuid()}.db");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureAppConfiguration((ctx, cfg) =>
        {
            cfg.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Default"] = $"Data Source={DbPath}",
                ["Jwt:Issuer"] = "TaskManagement",
                ["Jwt:Audience"] = "TaskManagementClient",
                ["Jwt:SigningKey"] = "this-is-a-test-signing-key-that-is-long-enough-123",
                ["Jwt:ExpiryMinutes"] = "60",
                ["Seed:Demo"] = "false"
            });
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        try { File.Delete(DbPath); } catch { }
    }
}
