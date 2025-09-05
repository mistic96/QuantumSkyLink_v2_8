using MobileAPIGateway.Models.Enums;

namespace MobileAPIGateway.Models.Users;

/// <summary>
/// User
/// </summary>
public sealed class User
{
    /// <summary>
    /// Gets or sets the ID
    /// </summary>
    public string Id { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the wallet address
    /// </summary>
    public string? WalletAddress { get; set; }
    
    /// <summary>
    /// Gets or sets the escrow wallet address
    /// </summary>
    public string? EscrowWalletAddress { get; set; }
    
    /// <summary>
    /// Gets or sets the multi-factor authentication amount
    /// </summary>
    public decimal? MultiFactorAuthenticationAmount { get; set; }
    
    /// <summary>
    /// Gets or sets the status
    /// </summary>
    public UserStatus? Status { get; set; }
    
    /// <summary>
    /// Gets or sets the address
    /// </summary>
    public CustomerAddresses? Address { get; set; }
    
    /// <summary>
    /// Gets or sets the email
    /// </summary>
    public string? Email { get; set; }
    
    /// <summary>
    /// Gets or sets the first name
    /// </summary>
    public string? FirstName { get; set; }
    
    /// <summary>
    /// Gets or sets the last name
    /// </summary>
    public string? LastName { get; set; }
    
    /// <summary>
    /// Gets or sets the mobile phone
    /// </summary>
    public string? MobilePhone { get; set; }
    
    /// <summary>
    /// Gets or sets the language
    /// </summary>
    public PreferedLanguage? Language { get; set; }
    
    /// <summary>
    /// Gets or sets the created date
    /// </summary>
    public DateTime? CreatedDate { get; set; }
    
    /// <summary>
    /// Gets or sets the payout addresses
    /// </summary>
    public ICollection<WalletAddress>? PayoutAddresses { get; set; }
    
    /// <summary>
    /// Gets or sets a value indicating whether the account is frozen
    /// </summary>
    public bool? FreezeAccount { get; set; }
    
    /// <summary>
    /// Gets or sets the account frozen reason
    /// </summary>
    public string? AccountFrozenReason { get; set; }
    
    /// <summary>
    /// Gets or sets a value indicating whether KYC is valid
    /// </summary>
    public bool? IsKycValid { get; set; }
    
    /// <summary>
    /// Gets or sets the last KYC validation date
    /// </summary>
    public DateTime? LastKycValidationDate { get; set; }
    
    /// <summary>
    /// Gets or sets the full name
    /// </summary>
    public string? GetFullName { get; set; }
    
    /// <summary>
    /// Gets or sets the named email address
    /// </summary>
    public string? GetNamedEmailAddress { get; set; }
    
    /// <summary>
    /// Gets or sets the last signed in date
    /// </summary>
    public DateTime? LastSignedInDate { get; set; }
}
