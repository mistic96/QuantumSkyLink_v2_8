namespace UserService.Models.Logto;

public class LogtoUserRequest
{
    public string Username { get; set; } = string.Empty;
    public string? Password { get; set; }
    public string? PrimaryEmail { get; set; }
    public string? PrimaryPhone { get; set; }
    public string? Name { get; set; }
    public string? Avatar { get; set; }
    public LogtoCustomData? CustomData { get; set; }
    public LogtoProfile? Profile { get; set; }
}

public class LogtoCustomData
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Country { get; set; }
    public string? City { get; set; }
    public string? PreferredLanguage { get; set; }
    public string? PreferredCurrency { get; set; }
}

public class LogtoProfile
{
    public string? FamilyName { get; set; }
    public string? GivenName { get; set; }
    public string? MiddleName { get; set; }
    public string? Nickname { get; set; }
    public string? PreferredUsername { get; set; }
    public string? Profile { get; set; }
    public string? Picture { get; set; }
    public string? Website { get; set; }
    public string? Gender { get; set; }
    public string? Birthdate { get; set; }
    public string? Zoneinfo { get; set; }
    public string? Locale { get; set; }
    public LogtoAddress? Address { get; set; }
}

public class LogtoAddress
{
    public string? Formatted { get; set; }
    public string? StreetAddress { get; set; }
    public string? Locality { get; set; }
    public string? Region { get; set; }
    public string? PostalCode { get; set; }
    public string? Country { get; set; }
}
