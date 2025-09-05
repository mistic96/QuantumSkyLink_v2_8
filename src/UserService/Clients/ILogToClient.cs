using Refit;

namespace UserService.Clients;

public interface ILogToClient
{
    [Get("/api/user")]
    Task<UserInfo> GetUserInfoAsync([Header("Authorization")] string authorization);
}

public class UserInfo
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}
