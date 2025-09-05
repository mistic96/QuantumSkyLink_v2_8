//using Microsoft.Extensions.Logging;
//using MobileAPIGateway.Models.CardManagement;
//using MobileAPIGateway.Models.Compatibility.CardManagement;

//namespace MobileAPIGateway.Services.Compatibility;

///// <summary>
///// Implementation of the card management compatibility service
///// </summary>
//public class CardManagementCompatibilityService : ICardManagementCompatibilityService
//{
//    private readonly ICardManagementService _cardManagementService;
//    private readonly ILogger<CardManagementCompatibilityService> _logger;
    
//    /// <summary>
//    /// Initializes a new instance of the <see cref="CardManagementCompatibilityService"/> class
//    /// </summary>
//    /// <param name="cardManagementService">The card management service</param>
//    /// <param name="logger">The logger</param>
//    public CardManagementCompatibilityService(ICardManagementService cardManagementService, ILogger<CardManagementCompatibilityService> logger)
//    {
//        _cardManagementService = cardManagementService ?? throw new ArgumentNullException(nameof(cardManagementService));
//        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
//    }
    
//    /// <inheritdoc />
//    public async Task<AddCardCompatibilityResponse> AddCardAsync(AddCardCompatibilityRequest request, CancellationToken cancellationToken = default)
//    {
//        try
//        {
//            _logger.LogInformation("Adding card for customer ID: {CustomerId}", request.CustomerId);
            
//            // Map to the new model
//            var addCardRequest = new AddCardRequest
//            {
//                CustomerId = request.CustomerId,
//                CardNumber = request.CardNumber,
//                CardHolderName = request.CardHolderName,
//                ExpirationMonth = request.ExpirationMonth,
//                ExpirationYear = request.ExpirationYear,
//                CVV = request.CVV,
//                CardType = request.CardType,
//                IsDefault = request.IsDefault,
//                Metadata = request.Metadata
//            };
            
//            // Map billing address if it exists
//            if (request.BillingAddress != null)
//            {
//                addCardRequest.BillingAddress = new Address
//                {
//                    FirstName = request.BillingAddress.FirstName,
//                    LastName = request.BillingAddress.LastName,
//                    CompanyName = request.BillingAddress.CompanyName,
//                    StreetAddress1 = request.BillingAddress.StreetAddress1,
//                    StreetAddress2 = request.BillingAddress.StreetAddress2,
//                    City = request.BillingAddress.City,
//                    StateOrProvince = request.BillingAddress.StateOrProvince,
//                    PostalCode = request.BillingAddress.PostalCode,
//                    Country = request.BillingAddress.Country,
//                    PhoneNumber = request.BillingAddress.PhoneNumber,
//                    EmailAddress = request.BillingAddress.EmailAddress
//                };
//            }
            
//            // Call the new service
//            var response = await _cardManagementService.AddCardAsync(addCardRequest, cancellationToken);
            
//            // Map to the compatibility response
//            var compatibilityResponse = new AddCardCompatibilityResponse
//            {
//                IsSuccessful = response.IsSuccessful,
//                Message = response.Message,
//                CardId = response.CardId,
//                CustomerId = response.CustomerId,
//                MaskedCardNumber = response.MaskedCardNumber,
//                CardHolderName = response.CardHolderName,
//                ExpirationMonth = response.ExpirationMonth,
//                ExpirationYear = response.ExpirationYear,
//                CardType = response.CardType,
//                CardBrand = response.CardBrand,
//                IsDefault = response.IsDefault,
//                Status = response.Status,
//                Metadata = response.Metadata,
//                CreatedDate = response.CreatedDate,
//                LastUpdatedDate = response.LastUpdatedDate
//            };
            
//            // Map billing address if it exists
//            if (response.BillingAddress != null)
//            {
//                compatibilityResponse.BillingAddress = new CardAddressCompatibility
//                {
//                    FirstName = response.BillingAddress.FirstName,
//                    LastName = response.BillingAddress.LastName,
//                    CompanyName = response.BillingAddress.CompanyName,
//                    StreetAddress1 = response.BillingAddress.StreetAddress1,
//                    StreetAddress2 = response.BillingAddress.StreetAddress2,
//                    City = response.BillingAddress.City,
//                    StateOrProvince = response.BillingAddress.StateOrProvince,
//                    PostalCode = response.BillingAddress.PostalCode,
//                    Country = response.BillingAddress.Country,
//                    PhoneNumber = response.BillingAddress.PhoneNumber,
//                    EmailAddress = response.BillingAddress.EmailAddress
//                };
//            }
            
//            return compatibilityResponse;
//        }
//        catch (Exception ex)
//        {
//            _logger.LogError(ex, "Error adding card for customer ID: {CustomerId}", request.CustomerId);
            
//            return new AddCardCompatibilityResponse
//            {
//                IsSuccessful = false,
//                Message = "Failed to add card: " + ex.Message,
//                CustomerId = request.CustomerId
//            };
//        }
//    }
    
//    /// <inheritdoc />
//    public async Task<UpdateCardCompatibilityResponse> UpdateCardAsync(UpdateCardCompatibilityRequest request, CancellationToken cancellationToken = default)
//    {
//        try
//        {
//            _logger.LogInformation("Updating card ID: {CardId} for customer ID: {CustomerId}", request.CardId, request.CustomerId);
            
//            // Map to the new model
//            var updateCardRequest = new UpdateCardRequest
//            {
//                CardId = request.CardId,
//                CustomerId = request.CustomerId,
//                CardHolderName = request.CardHolderName,
//                ExpirationMonth = request.ExpirationMonth,
//                ExpirationYear = request.ExpirationYear,
//                IsDefault = request.IsDefault,
//                Metadata = request.Metadata
//            };
            
//            // Map billing address if it exists
//            if (request.BillingAddress != null)
//            {
//                updateCardRequest.BillingAddress = new Address
//                {
//                    FirstName = request.BillingAddress.FirstName,
//                    LastName = request.BillingAddress.LastName,
//                    CompanyName = request.BillingAddress.CompanyName,
//                    StreetAddress1 = request.BillingAddress.StreetAddress1,
//                    StreetAddress2 = request.BillingAddress.StreetAddress2,
//                    City = request.BillingAddress.City,
//                    StateOrProvince = request.BillingAddress.StateOrProvince,
//                    PostalCode = request.BillingAddress.PostalCode,
//                    Country = request.BillingAddress.Country,
//                    PhoneNumber = request.BillingAddress.PhoneNumber,
//                    EmailAddress = request.BillingAddress.EmailAddress
//                };
//            }
            
//            // Call the new service
//            var response = await _cardManagementService.UpdateCardAsync(updateCardRequest, cancellationToken);
            
//            // Map to the compatibility response
//            var compatibilityResponse = new UpdateCardCompatibilityResponse
//            {
//                IsSuccessful = response.IsSuccessful,
//                Message = response.Message,
//                CardId = response.CardId,
//                CustomerId = response.CustomerId,
//                MaskedCardNumber = response.MaskedCardNumber,
//                CardHolderName = response.CardHolderName,
//                ExpirationMonth = response.ExpirationMonth,
//                ExpirationYear = response.ExpirationYear,
//                CardType = response.CardType,
//                CardBrand = response.CardBrand,
//                IsDefault = response.IsDefault,
//                Status = response.Status,
//                Metadata = response.Metadata,
//                CreatedDate = response.CreatedDate,
//                LastUpdatedDate = response.LastUpdatedDate
//            };
            
//            // Map billing address if it exists
//            if (response.BillingAddress != null)
//            {
//                compatibilityResponse.BillingAddress = new CardAddressCompatibility
//                {
//                    FirstName = response.BillingAddress.FirstName,
//                    LastName = response.BillingAddress.LastName,
//                    CompanyName = response.BillingAddress.CompanyName,
//                    StreetAddress1 = response.BillingAddress.StreetAddress1,
//                    StreetAddress2 = response.BillingAddress.StreetAddress2,
//                    City = response.BillingAddress.City,
//                    StateOrProvince = response.BillingAddress.StateOrProvince,
//                    PostalCode = response.BillingAddress.PostalCode,
//                    Country = response.BillingAddress.Country,
//                    PhoneNumber = response.BillingAddress.PhoneNumber,
//                    EmailAddress = response.BillingAddress.EmailAddress
//                };
//            }
            
//            return compatibilityResponse;
//        }
//        catch (Exception ex)
//        {
//            _logger.LogError(ex, "Error updating card ID: {CardId} for customer ID: {CustomerId}", request.CardId, request.CustomerId);
            
//            return new UpdateCardCompatibilityResponse
//            {
//                IsSuccessful = false,
//                Message = "Failed to update card: " + ex.Message,
//                CardId = request.CardId,
//                CustomerId = request.CustomerId
//            };
//        }
//    }
    
//    /// <inheritdoc />
//    public async Task<CardVerificationCompatibilityResponse> VerifyCardAsync(CardVerificationCompatibilityRequest request, CancellationToken cancellationToken = default)
//    {
//        try
//        {
//            _logger.LogInformation("Verifying card ID: {CardId} for customer ID: {CustomerId}", request.CardId, request.CustomerId);
            
//            // Map to the new model
//            var cardVerificationRequest = new CardVerificationRequest
//            {
//                CardId = request.CardId,
//                CustomerId = request.CustomerId,
//                VerificationCode = request.VerificationCode,
//                VerificationType = request.VerificationType,
//                Metadata = request.Metadata
//            };
            
//            // Call the new service
//            var response = await _cardManagementService.VerifyCardAsync(cardVerificationRequest, cancellationToken);
            
//            // Map to the compatibility response
//            var compatibilityResponse = new CardVerificationCompatibilityResponse
//            {
//                IsSuccessful = response.IsSuccessful,
//                Message = response.Message,
//                CardId = response.CardId,
//                CustomerId = response.CustomerId,
//                VerificationStatus = response.VerificationStatus,
//                VerificationType = response.VerificationType,
//                VerificationDate = response.VerificationDate,
//                Metadata = response.Metadata
//            };
            
//            return compatibilityResponse;
//        }
//        catch (Exception ex)
//        {
//            _logger.LogError(ex, "Error verifying card ID: {CardId} for customer ID: {CustomerId}", request.CardId, request.CustomerId);
            
//            return new CardVerificationCompatibilityResponse
//            {
//                IsSuccessful = false,
//                Message = "Failed to verify card: " + ex.Message,
//                CardId = request.CardId,
//                CustomerId = request.CustomerId,
//                VerificationStatus = "Failed"
//            };
//        }
//    }
//}
