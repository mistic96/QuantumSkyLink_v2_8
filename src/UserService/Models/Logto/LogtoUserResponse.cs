namespace UserService.Models.Logto;

public class LogtoUserResponse
{
    public string Id { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string? PrimaryEmail { get; set; }
    public string? PrimaryPhone { get; set; }
    public string? Name { get; set; }
    public string? Avatar { get; set; }
    public LogtoCustomData? CustomData { get; set; }
    public LogtoProfile? Profile { get; set; }
    public bool IsEmailVerified { get; set; }
    public bool IsPhoneVerified { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? LastSignInAt { get; set; }
}

public class LogtoTokenResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public string TokenType { get; set; } = "Bearer";
    public int ExpiresIn { get; set; }
    public string? Scope { get; set; }
}

public class LogtoErrorResponse
{
    public string Code { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public object? Data { get; set; }
}

public class LogtoApiResponse<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public LogtoErrorResponse? Error { get; set; }
}
