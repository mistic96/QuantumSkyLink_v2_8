namespace UserService.Models.Security;

public enum SecurityEventType
{
    UserLogin,
    UserLogout,
    FailedLogin,
    PasswordChange,
    EmailChange,
    PhoneChange,
    RoleAssigned,
    RoleRemoved,
    PermissionGranted,
    PermissionDenied,
    WalletCreated,
    WalletVerified,
    WalletDeactivated,
    SuspiciousActivity,
    AccountLocked,
    AccountUnlocked,
    TwoFactorEnabled,
    TwoFactorDisabled,
    ApiKeyGenerated,
    ApiKeyRevoked,
    DataExport,
    DataDeletion,
    ComplianceViolation,
    SecurityPolicyViolation
}

public enum RoleChangeType
{
    Assigned,
    Removed,
    Expired,
    Upgraded,
    Downgraded
}

public enum WalletActionType
{
    Created,
    Verified,
    BalanceUpdated,
    TransactionSigned,
    Deactivated,
    Reactivated,
    AddressChanged,
    SecurityUpdated
}

public enum AuditLogLevel
{
    Information,
    Warning,
    Error,
    Critical
}
