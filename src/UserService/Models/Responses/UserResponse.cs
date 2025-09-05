namespace UserService.Models.Responses;

public class UserResponse
{
    public Guid Id { get; set; }
    public string LogtoUserId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public bool IsEmailVerified { get; set; }
    public bool IsPhoneVerified { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public UserProfileResponse? Profile { get; set; }
    public UserWalletResponse? Wallet { get; set; }
    public IEnumerable<UserRoleResponse> Roles { get; set; } = new List<UserRoleResponse>();
}

public class UserProfileResponse
{
    public string? Bio { get; set; }
    public string? ProfileImageUrl { get; set; }
    public string? Country { get; set; }
    public string? City { get; set; }
    public string? PostalCode { get; set; }
    public string? Address { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public string? Occupation { get; set; }
    public string? Company { get; set; }
    public string? Website { get; set; }
    public string? TimeZone { get; set; }
    public string? PreferredLanguage { get; set; }
    public string? PreferredCurrency { get; set; }
    public bool IsProfileComplete { get; set; }
}

public class UserWalletResponse
{
    public string WalletAddress { get; set; } = string.Empty;
    public string PublicKey { get; set; } = string.Empty;
    public int RequiredSignatures { get; set; }
    public int TotalSigners { get; set; }
    public string WalletType { get; set; } = string.Empty;
    public string Network { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public bool IsVerified { get; set; }
    public decimal Balance { get; set; }
    public string BalanceCurrency { get; set; } = string.Empty;
    public DateTime? LastTransactionAt { get; set; }
}

public class UserRoleResponse
{
    public Guid RoleId { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public string? RoleDescription { get; set; }
    public DateTime AssignedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public bool IsActive { get; set; }
    public IEnumerable<PermissionResponse> Permissions { get; set; } = new List<PermissionResponse>();
}

public class PermissionResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Resource { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
}
