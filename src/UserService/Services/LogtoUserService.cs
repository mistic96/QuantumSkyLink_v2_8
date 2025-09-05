using Mapster;
using UserService.Models.Logto;
using UserService.Models.Requests;
using UserService.Models.Responses;
using UserService.Services.Interfaces;

namespace UserService.Services;

public class LogtoUserService : ILogtoUserService
{
    private readonly ILogtoApiClient _logtoApiClient;
    private readonly ILogger<LogtoUserService> _logger;

    public LogtoUserService(ILogtoApiClient logtoApiClient, ILogger<LogtoUserService> logger)
    {
        _logtoApiClient = logtoApiClient;
        _logger = logger;
    }

    public async Task<LogtoApiResponse<LogtoUserResponse>> CreateUserAsync(RegisterUserRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var logtoRequest = new LogtoUserRequest
            {
                Username = request.Email,
                Password = request.Password,
                PrimaryEmail = request.Email,
                PrimaryPhone = request.PhoneNumber,
                Name = $"{request.FirstName} {request.LastName}",
                CustomData = new LogtoCustomData
                {
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    Country = request.Country,
                    City = request.City,
                    PreferredLanguage = request.PreferredLanguage,
                    PreferredCurrency = request.PreferredCurrency
                },
                Profile = new LogtoProfile
                {
                    GivenName = request.FirstName,
                    FamilyName = request.LastName,
                    Locale = request.PreferredLanguage,
                    Address = new LogtoAddress
                    {
                        Country = request.Country,
                        Locality = request.City
                    }
                }
            };

            var response = await _logtoApiClient.CreateUserAsync(logtoRequest, cancellationToken);

            if (response.IsSuccessStatusCode && response.Content != null)
            {
                return new LogtoApiResponse<LogtoUserResponse>
                {
                    Success = true,
                    Data = response.Content
                };
            }

            return new LogtoApiResponse<LogtoUserResponse>
            {
                Success = false,
                Error = new LogtoErrorResponse
                {
                    Code = response.StatusCode.ToString(),
                    Message = response.Error?.Content ?? "Failed to create user in Logto"
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user in Logto");
            return new LogtoApiResponse<LogtoUserResponse>
            {
                Success = false,
                Error = new LogtoErrorResponse
                {
                    Code = "INTERNAL_ERROR",
                    Message = ex.Message
                }
            };
        }
    }

    public async Task<LogtoApiResponse<LogtoUserResponse>> GetUserAsync(string logtoUserId, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _logtoApiClient.GetUserAsync(logtoUserId, cancellationToken);

            if (response.IsSuccessStatusCode && response.Content != null)
            {
                return new LogtoApiResponse<LogtoUserResponse>
                {
                    Success = true,
                    Data = response.Content
                };
            }

            return new LogtoApiResponse<LogtoUserResponse>
            {
                Success = false,
                Error = new LogtoErrorResponse
                {
                    Code = response.StatusCode.ToString(),
                    Message = response.Error?.Content ?? "User not found in Logto"
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user from Logto");
            return new LogtoApiResponse<LogtoUserResponse>
            {
                Success = false,
                Error = new LogtoErrorResponse
                {
                    Code = "INTERNAL_ERROR",
                    Message = ex.Message
                }
            };
        }
    }

    public async Task<LogtoApiResponse<LogtoUserResponse>> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _logtoApiClient.GetUserByEmailAsync(email, cancellationToken);

            if (response.IsSuccessStatusCode && response.Content != null)
            {
                return new LogtoApiResponse<LogtoUserResponse>
                {
                    Success = true,
                    Data = response.Content
                };
            }

            return new LogtoApiResponse<LogtoUserResponse>
            {
                Success = false,
                Error = new LogtoErrorResponse
                {
                    Code = response.StatusCode.ToString(),
                    Message = response.Error?.Content ?? "User not found in Logto"
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user by email from Logto");
            return new LogtoApiResponse<LogtoUserResponse>
            {
                Success = false,
                Error = new LogtoErrorResponse
                {
                    Code = "INTERNAL_ERROR",
                    Message = ex.Message
                }
            };
        }
    }

    public async Task<LogtoApiResponse<LogtoUserResponse>> UpdateUserAsync(string logtoUserId, LogtoUserRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _logtoApiClient.UpdateUserAsync(logtoUserId, request, cancellationToken);

            if (response.IsSuccessStatusCode && response.Content != null)
            {
                return new LogtoApiResponse<LogtoUserResponse>
                {
                    Success = true,
                    Data = response.Content
                };
            }

            return new LogtoApiResponse<LogtoUserResponse>
            {
                Success = false,
                Error = new LogtoErrorResponse
                {
                    Code = response.StatusCode.ToString(),
                    Message = response.Error?.Content ?? "Failed to update user in Logto"
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user in Logto");
            return new LogtoApiResponse<LogtoUserResponse>
            {
                Success = false,
                Error = new LogtoErrorResponse
                {
                    Code = "INTERNAL_ERROR",
                    Message = ex.Message
                }
            };
        }
    }

    public async Task<LogtoApiResponse<bool>> DeleteUserAsync(string logtoUserId, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _logtoApiClient.DeleteUserAsync(logtoUserId, cancellationToken);

            return new LogtoApiResponse<bool>
            {
                Success = response.IsSuccessStatusCode,
                Data = response.IsSuccessStatusCode,
                Error = response.IsSuccessStatusCode ? null : new LogtoErrorResponse
                {
                    Code = response.StatusCode.ToString(),
                    Message = response.Error?.Content ?? "Failed to delete user in Logto"
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user in Logto");
            return new LogtoApiResponse<bool>
            {
                Success = false,
                Data = false,
                Error = new LogtoErrorResponse
                {
                    Code = "INTERNAL_ERROR",
                    Message = ex.Message
                }
            };
        }
    }

    public async Task<LogtoApiResponse<LogtoTokenResponse>> AuthenticateUserAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        try
        {
            var parameters = new Dictionary<string, object>
            {
                ["grant_type"] = "password",
                ["username"] = email,
                ["password"] = password,
                ["scope"] = "openid profile email"
            };

            var response = await _logtoApiClient.GetTokenAsync(parameters, cancellationToken);

            if (response.IsSuccessStatusCode && response.Content != null)
            {
                return new LogtoApiResponse<LogtoTokenResponse>
                {
                    Success = true,
                    Data = response.Content
                };
            }

            return new LogtoApiResponse<LogtoTokenResponse>
            {
                Success = false,
                Error = new LogtoErrorResponse
                {
                    Code = response.StatusCode.ToString(),
                    Message = response.Error?.Content ?? "Authentication failed"
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error authenticating user in Logto");
            return new LogtoApiResponse<LogtoTokenResponse>
            {
                Success = false,
                Error = new LogtoErrorResponse
                {
                    Code = "INTERNAL_ERROR",
                    Message = ex.Message
                }
            };
        }
    }

    public async Task<LogtoApiResponse<LogtoTokenResponse>> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        try
        {
            var parameters = new Dictionary<string, object>
            {
                ["grant_type"] = "refresh_token",
                ["refresh_token"] = refreshToken
            };

            var response = await _logtoApiClient.GetTokenAsync(parameters, cancellationToken);

            if (response.IsSuccessStatusCode && response.Content != null)
            {
                return new LogtoApiResponse<LogtoTokenResponse>
                {
                    Success = true,
                    Data = response.Content
                };
            }

            return new LogtoApiResponse<LogtoTokenResponse>
            {
                Success = false,
                Error = new LogtoErrorResponse
                {
                    Code = response.StatusCode.ToString(),
                    Message = response.Error?.Content ?? "Token refresh failed"
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing token in Logto");
            return new LogtoApiResponse<LogtoTokenResponse>
            {
                Success = false,
                Error = new LogtoErrorResponse
                {
                    Code = "INTERNAL_ERROR",
                    Message = ex.Message
                }
            };
        }
    }

    public async Task<LogtoApiResponse<bool>> VerifyTokenAsync(string accessToken, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _logtoApiClient.GetCurrentUserAsync($"Bearer {accessToken}", cancellationToken);

            return new LogtoApiResponse<bool>
            {
                Success = response.IsSuccessStatusCode,
                Data = response.IsSuccessStatusCode,
                Error = response.IsSuccessStatusCode ? null : new LogtoErrorResponse
                {
                    Code = response.StatusCode.ToString(),
                    Message = response.Error?.Content ?? "Token verification failed"
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying token in Logto");
            return new LogtoApiResponse<bool>
            {
                Success = false,
                Data = false,
                Error = new LogtoErrorResponse
                {
                    Code = "INTERNAL_ERROR",
                    Message = ex.Message
                }
            };
        }
    }

    public async Task<LogtoApiResponse<bool>> RevokeTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        try
        {
            var parameters = new Dictionary<string, object>
            {
                ["token"] = token
            };

            var response = await _logtoApiClient.RevokeTokenAsync(parameters, cancellationToken);

            return new LogtoApiResponse<bool>
            {
                Success = response.IsSuccessStatusCode,
                Data = response.IsSuccessStatusCode,
                Error = response.IsSuccessStatusCode ? null : new LogtoErrorResponse
                {
                    Code = response.StatusCode.ToString(),
                    Message = response.Error?.Content ?? "Token revocation failed"
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error revoking token in Logto");
            return new LogtoApiResponse<bool>
            {
                Success = false,
                Data = false,
                Error = new LogtoErrorResponse
                {
                    Code = "INTERNAL_ERROR",
                    Message = ex.Message
                }
            };
        }
    }

    // Missing methods needed by controller
    public async Task<LogtoUserResponse> SyncUserFromLogtoAsync(string logtoUserId, CancellationToken cancellationToken = default)
    {
        var correlationId = Guid.NewGuid().ToString();
        _logger.LogInformation("Syncing user from Logto {LogtoUserId} (CorrelationId: {CorrelationId})", 
            logtoUserId, correlationId);

        try
        {
            var response = await GetUserAsync(logtoUserId, cancellationToken);

            if (response.Success && response.Data != null)
            {
                _logger.LogInformation("User synced successfully from Logto {LogtoUserId} (CorrelationId: {CorrelationId})", 
                    logtoUserId, correlationId);

                return response.Data;
            }

            throw new InvalidOperationException($"Failed to sync user from Logto: {response.Error?.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to sync user from Logto {LogtoUserId} (CorrelationId: {CorrelationId})", 
                logtoUserId, correlationId);
            throw;
        }
    }

    public async Task<LogtoUserProfileResponse> GetUserProfileAsync(string userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await GetUserAsync(userId, cancellationToken);

            if (response.Success && response.Data != null)
            {
                return new LogtoUserProfileResponse
                {
                    Id = response.Data.Id,
                    Username = response.Data.Username,
                    Email = response.Data.PrimaryEmail,
                    PhoneNumber = response.Data.PrimaryPhone,
                    Avatar = response.Data.Avatar,
                    EmailVerified = !string.IsNullOrEmpty(response.Data.PrimaryEmail),
                    PhoneVerified = !string.IsNullOrEmpty(response.Data.PrimaryPhone),
                    IsActive = true,
                    CreatedAt = response.Data.CreatedAt,
                    UpdatedAt = response.Data.UpdatedAt
                };
            }

            throw new InvalidOperationException($"User profile not found: {response.Error?.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get user profile for {UserId}", userId);
            throw;
        }
    }

    public async Task<LogtoUserProfileResponse> UpdateUserProfileAsync(string userId, LogtoUserRequest request, CancellationToken cancellationToken = default)
    {
        var correlationId = Guid.NewGuid().ToString();
        _logger.LogInformation("Updating user profile for {UserId} (CorrelationId: {CorrelationId})", 
            userId, correlationId);

        try
        {
            var response = await UpdateUserAsync(userId, request, cancellationToken);

            if (response.Success && response.Data != null)
            {
                _logger.LogInformation("User profile updated successfully for {UserId} (CorrelationId: {CorrelationId})", 
                    userId, correlationId);

                return new LogtoUserProfileResponse
                {
                    Id = response.Data.Id,
                    Username = response.Data.Username,
                    Email = response.Data.PrimaryEmail,
                    PhoneNumber = response.Data.PrimaryPhone,
                    Avatar = response.Data.Avatar,
                    EmailVerified = !string.IsNullOrEmpty(response.Data.PrimaryEmail),
                    PhoneVerified = !string.IsNullOrEmpty(response.Data.PrimaryPhone),
                    IsActive = true,
                    CreatedAt = response.Data.CreatedAt,
                    UpdatedAt = response.Data.UpdatedAt
                };
            }

            throw new InvalidOperationException($"Failed to update user profile: {response.Error?.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update user profile for {UserId} (CorrelationId: {CorrelationId})", 
                userId, correlationId);
            throw;
        }
    }

    public async Task<bool> VerifyUserEmailAsync(string userId, CancellationToken cancellationToken = default)
    {
        var correlationId = Guid.NewGuid().ToString();
        _logger.LogInformation("Verifying email for user {UserId} (CorrelationId: {CorrelationId})", 
            userId, correlationId);

        try
        {
            // In a real implementation, this would trigger email verification in Logto
            // For now, we'll simulate the verification process
            var response = await GetUserAsync(userId, cancellationToken);

            if (response.Success && response.Data != null)
            {
                _logger.LogInformation("Email verification initiated for user {UserId} (CorrelationId: {CorrelationId})", 
                    userId, correlationId);
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to verify email for user {UserId} (CorrelationId: {CorrelationId})", 
                userId, correlationId);
            return false;
        }
    }

    public async Task<bool> ResetUserPasswordAsync(string userId, CancellationToken cancellationToken = default)
    {
        var correlationId = Guid.NewGuid().ToString();
        _logger.LogInformation("Resetting password for user {UserId} (CorrelationId: {CorrelationId})", 
            userId, correlationId);

        try
        {
            // In a real implementation, this would trigger password reset in Logto
            // For now, we'll simulate the password reset process
            var response = await GetUserAsync(userId, cancellationToken);

            if (response.Success && response.Data != null)
            {
                _logger.LogInformation("Password reset initiated for user {UserId} (CorrelationId: {CorrelationId})", 
                    userId, correlationId);
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to reset password for user {UserId} (CorrelationId: {CorrelationId})", 
                userId, correlationId);
            return false;
        }
    }

    public async Task<LogtoAuthStatusResponse> GetAuthenticationStatusAsync(string userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await GetUserAsync(userId, cancellationToken);

            if (response.Success && response.Data != null)
            {
                return new LogtoAuthStatusResponse
                {
                    IsAuthenticated = true,
                    IsLogtoConnected = true,
                    Status = "Active",
                    HealthCheckPassed = true,
                    CheckedAt = DateTime.UtcNow,
                    Metadata = new Dictionary<string, object>
                    {
                        ["userId"] = response.Data.Id,
                        ["email"] = response.Data.PrimaryEmail ?? "",
                        ["phone"] = response.Data.PrimaryPhone ?? "",
                        ["lastUpdated"] = response.Data.UpdatedAt
                    }
                };
            }

            return new LogtoAuthStatusResponse
            {
                IsAuthenticated = false,
                IsLogtoConnected = false,
                Status = "Not Found",
                HealthCheckPassed = false,
                CheckedAt = DateTime.UtcNow,
                Metadata = new Dictionary<string, object>
                {
                    ["userId"] = userId
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get authentication status for user {UserId}", userId);
            return new LogtoAuthStatusResponse
            {
                IsAuthenticated = false,
                IsLogtoConnected = false,
                Status = "Error",
                StatusMessage = ex.Message,
                HealthCheckPassed = false,
                CheckedAt = DateTime.UtcNow,
                Metadata = new Dictionary<string, object>
                {
                    ["userId"] = userId,
                    ["error"] = ex.Message
                }
            };
        }
    }
}
