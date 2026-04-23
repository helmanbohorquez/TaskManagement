namespace TaskManagement.Infrastructure.Security;

public class JwtOptions
{
    public string Issuer { get; set; } = "TaskManagement";
    public string Audience { get; set; } = "TaskManagementClient";
    public string SigningKey { get; set; } = string.Empty;
    public int ExpiryMinutes { get; set; } = 60;
}
