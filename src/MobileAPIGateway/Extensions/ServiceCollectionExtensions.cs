using Refit;
using MobileAPIGateway.Clients;
using MobileAPIGateway.Services;
using MobileAPIGateway.Validators.CardManagement;
using MobileAPIGateway.Validators.Dashboard;
using MobileAPIGateway.Validators.SecondaryMarkets;
using MobileAPIGateway.Validators.TokenizedCart;
using MobileAPIGateway.Authentication;

namespace MobileAPIGateway.Extensions;

/// <summary>
/// Service collection extensions
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds the service clients
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="configuration">Configuration</param>
    /// <returns>Service collection</returns>
    public static IServiceCollection AddServiceClients(this IServiceCollection services, IConfiguration configuration)
    {
        // Add Auth client
        services.AddRefitClient<IAuthClient>()
            .ConfigureHttpClient(c => c.BaseAddress = new Uri(configuration["ServiceUrls:AuthService"] ?? "http://localhost:5001"));

        // Add User client
        services.AddRefitClient<IUserClient>()
            .ConfigureHttpClient(c => c.BaseAddress = new Uri(configuration["ServiceUrls:AccountService"] ?? "http://localhost:5002"));

        // Add Wallet client
        services.AddRefitClient<IWalletClient>()
            .ConfigureHttpClient(c => c.BaseAddress = new Uri(configuration["ServiceUrls:WalletService"] ?? "http://localhost:5003"));

        // Add Markets client
        services.AddRefitClient<IMarketsClient>()
            .ConfigureHttpClient(c => c.BaseAddress = new Uri(configuration["ServiceUrls:MarketplaceService"] ?? "http://localhost:5004"));

        // Add Customer Markets client
        services.AddRefitClient<ICustomerMarketsClient>()
            .ConfigureHttpClient(c => c.BaseAddress = new Uri(configuration["ServiceUrls:MarketplaceService"] ?? "http://localhost:5004"));

        // Add Dashboard client
        services.AddRefitClient<IDashboardClient>()
            .ConfigureHttpClient(c => c.BaseAddress = new Uri(configuration["Services:Dashboard:BaseUrl"] ?? "https://dashboard-api"));

        // Add Secondary Markets client
        services.AddRefitClient<ISecondaryMarketsClient>()
            .ConfigureHttpClient(c => c.BaseAddress = new Uri(configuration["Services:SecondaryMarkets:BaseUrl"] ?? "https://secondary-markets-api"));

        // Add Unified Cart client (connects to UnifiedCartService backend)
        services.AddRefitClient<ITokenizedCartClient>()
            .ConfigureHttpClient(c => c.BaseAddress = new Uri(configuration["ServiceUrls:UnifiedCartService"] ?? "https+http://unifiedcartservice"));

        // Add Card Management client
        services.AddRefitClient<ICardManagementClient>()
            .ConfigureHttpClient(c => c.BaseAddress = new Uri(configuration["ServiceUrls:PaymentGatewayService"] ?? "http://localhost:5006"));

        // Add Carts client
        services.AddRefitClient<ICartsClient>()
            .ConfigureHttpClient(c => c.BaseAddress = new Uri(configuration["ServiceUrls:CartService"] ?? "http://localhost:5007"));

        // Add Search client
        services.AddRefitClient<ISearchClient>()
            .ConfigureHttpClient(c => c.BaseAddress = new Uri(configuration["ServiceUrls:SearchService"] ?? "http://localhost:5008"));

        // Add Global client
        services.AddRefitClient<IGlobalClient>()
            .ConfigureHttpClient(c => c.BaseAddress = new Uri(configuration["ServiceUrls:InfrastructureService"] ?? "http://localhost:5009"));

        // Add Compliance client
        services.AddRefitClient<IComplianceServiceClient>()
            .ConfigureHttpClient(c => c.BaseAddress = new Uri("https+http://complianceservice"));

        // Add Infrastructure client
        services.AddRefitClient<IInfrastructureServiceClient>()
            .ConfigureHttpClient(c => c.BaseAddress = new Uri("https+http://infrastructureservice"));

        // Add Payment Gateway client for deposit code operations
        services.AddHttpClient<IPaymentGatewayClient, PaymentGatewayClient>(client =>
        {
            client.BaseAddress = new Uri("https+http://paymentgatewayservice");
        });

        return services;
    }

    /// <summary>
    /// Adds the services
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <returns>Service collection</returns>
    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        // Add Auth service
        services.AddScoped<IAuthService, AuthService>();
        
        // Add LogTo service
        services.AddScoped<ILogToService, LogToService>();

        // Add Compatibility services
        //services.AddScoped<IAuthCompatibilityService, AuthCompatibilityService>();
        //services.AddScoped<IUserCompatibilityService, UserCompatibilityService>();
        ////services.AddScoped<IWalletCompatibilityService, WalletCompatibilityService>();
        ////services.AddScoped<IMarketsCompatibilityService, MarketsCompatibilityService>();
        //services.AddScoped<ICustomerMarketsCompatibilityService, CustomerMarketsCompatibilityService>();
        //services.AddScoped<ITokenizedCartCompatibilityService, TokenizedCartCompatibilityService>();
        ////services.AddScoped<ICardManagementCompatibilityService, CardManagementCompatibilityService>();
        //services.AddScoped<ICartsCompatibilityService, CartsCompatibilityService>();
        //services.AddScoped<ISearchCompatibilityService, SearchCompatibilityService>();
        //services.AddScoped<IGlobalCompatibilityService, GlobalCompatibilityService>();

        // Add User service
        services.AddScoped<IUserService, UserService>();

        // Add Wallet service
        services.AddScoped<IWalletService, WalletService>();

        // Add Unified Markets service (replaces old Markets and SecondaryMarkets services)
        services.AddScoped<IUnifiedMarketsService, UnifiedMarketsService>();

        // Add Customer Markets service
        services.AddScoped<ICustomerMarketsService, CustomerMarketsService>();

        // Add Dashboard service
        services.AddScoped<IDashboardService, DashboardService>();

        // Add Unified Cart service (handles both Primary and Secondary markets)
        services.AddScoped<ITokenizedCartService, TokenizedCartService>();

        // Add Card Management service
        //services.AddScoped<ICardManagementService, CardManagementService>();

        // Add Carts service
        //services.AddScoped<ICartsService, CartsService>();

        // Add Search service
        services.AddScoped<ISearchService, SearchService>();

        // Add Global service
        services.AddScoped<IGlobalService, GlobalService>();

        // Add Compliance service
        services.AddScoped<IComplianceService, ComplianceService>();

        // Add Infrastructure service
        services.AddScoped<IInfrastructureService, InfrastructureService>();


        return services;
    }

    /// <summary>
    /// Adds the validators
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <returns>Service collection</returns>
    public static IServiceCollection AddValidators(this IServiceCollection services)
    {
        // Add Auth validators
        services.AddScoped<Validators.Auth.LoginRequestValidator>();
        services.AddScoped<Validators.Auth.RefreshTokenRequestValidator>();

        // Add User validators
        services.AddScoped<Validators.User.UpdateUserRequestValidator>();
        services.AddScoped<Validators.User.ChangePasswordRequestValidator>();

        // Add Users validators
        services.AddScoped<Validators.Users.UserValidator>();
        services.AddScoped<Validators.Users.CountryValidator>();
        services.AddScoped<Validators.Users.CustomerAddressesValidator>();
        services.AddScoped<Validators.Users.UserTimeZoneValidator>();
        services.AddScoped<Validators.Users.WalletAddressValidator>();

        // Add Wallet validators
        services.AddScoped<Validators.Wallet.WalletBalanceValidator>();
        services.AddScoped<Validators.Wallet.WalletTransactionValidator>();
        services.AddScoped<Validators.Wallet.TransferRequestValidator>();
        services.AddScoped<Validators.Wallet.WithdrawRequestValidator>();
        services.AddScoped<Validators.Wallet.DepositRequestValidator>();

        // Add Markets validators
        services.AddScoped<Validators.Markets.MarketValidator>();
        services.AddScoped<Validators.Markets.TradingPairValidator>();
        services.AddScoped<Validators.Markets.TradeValidator>();
        services.AddScoped<Validators.Markets.PriceTierValidator>();

        // Add Customer Markets validators
        services.AddScoped<Validators.CustomerMarkets.CustomerMarketValidator>();
        services.AddScoped<Validators.CustomerMarkets.CustomerTradingPairValidator>();
        services.AddScoped<Validators.CustomerMarkets.CustomerTradingPairAlertValidator>();
        services.AddScoped<Validators.CustomerMarkets.CustomerMarketSubscriptionValidator>();
        services.AddScoped<Validators.CustomerMarkets.CustomerMarketSubscriptionPlanValidator>();

        // Add Dashboard validators
        services.AddScoped<PercentageFluxValidator>();
        services.AddScoped<FxRateValidator>();
        services.AddScoped<AssetBackedCurrencyValidator>();
        services.AddScoped<DynamicUserCoinValidator>();
        services.AddScoped<PublicTeamValidator>();
        services.AddScoped<TeamPriceValidator>();
        services.AddScoped<PublicTeamExtendedPriceValidator>();
        services.AddScoped<WalletAssetsValidator>();
        services.AddScoped<WalletExtendedValidator>();
        services.AddScoped<NewsValidator>();
        services.AddScoped<BalanceMetricValidator>();
        services.AddScoped<UserCoinMetricValidator>();

        // Add Secondary Markets validators
        services.AddScoped<ImageValidator>();
        services.AddScoped<AcceptedPaymentMethodValidator>();
        services.AddScoped<LedgerMarketExchangeRateValidator>();
        services.AddScoped<OrderPaymentAddressValidator>();
        services.AddScoped<PaymentInfoValidator>();
        services.AddScoped<PaymentAddressValidator>();
        services.AddScoped<MicroServiceChargeRequestFeeValidator>();
        services.AddScoped<EstimatedProfitProjectionValidator>();
        services.AddScoped<CloudFeeValidator>();
        services.AddScoped<CustomerValidator>();
        services.AddScoped<MicroDepositCryptoResponseValidator>();
        services.AddScoped<ListingSaleValidator>();
        services.AddScoped<MarketListingValidator>();

        // Add Unified Cart validators (supports both market types)
        services.AddScoped<TokenizedCartValidator>();
        services.AddScoped<TokenizedCartItemValidator>();
        services.AddScoped<CartCreationRequestValidator>();
        services.AddScoped<CartUpdateRequestValidator>();
        services.AddScoped<CartCheckoutRequestValidator>();
        services.AddScoped<CartResponseValidator>();
        services.AddScoped<Validators.TokenizedCart.CartItemRequestValidator>();

        // Add Card Management validators
        services.AddScoped<PaymentCardValidator>();
        services.AddScoped<AddCardRequestValidator>();
        services.AddScoped<UpdateCardRequestValidator>();
        services.AddScoped<CardVerificationRequestValidator>();
        services.AddScoped<CardResponseValidator>();

        // Add Carts validators
        services.AddScoped<Validators.Carts.ShoppingCartValidator>();
        services.AddScoped<Validators.Carts.CartItemValidator>();
        services.AddScoped<Validators.Carts.CreateCartRequestValidator>();
        services.AddScoped<Validators.Carts.UpdateCartRequestValidator>();
        services.AddScoped<Validators.Carts.UpdateCartItemRequestValidator>();
        services.AddScoped<Validators.Carts.CheckoutCartRequestValidator>();
        services.AddScoped<Validators.Carts.CartResponseValidator>();

        // Add Search validators
        services.AddScoped<Validators.Search.SearchRequestValidator>();
        services.AddScoped<Validators.Search.SearchResultValidator>();
        services.AddScoped<Validators.Search.SearchResponseValidator>();

        // Add Global validators
        services.AddScoped<Validators.Global.SystemStatusValidator>();
        services.AddScoped<Validators.Global.AppConfigValidator>();
        services.AddScoped<Validators.Global.LimitResponseValidator>();

        // Add Deposit Code validators
        services.AddScoped<Validators.Wallet.DepositCodeGenerationRequestValidator>();
        services.AddScoped<Validators.Wallet.DepositCodeValidationRequestValidator>();
        services.AddScoped<Validators.Wallet.EnhancedDepositRequestValidator>();
        services.AddScoped<Validators.Wallet.DepositMetadataValidator>();
        services.AddScoped<Validators.Wallet.DepositPreValidationRequestValidator>();
        services.AddScoped<Validators.Wallet.MobileFeaturesValidator>();
        services.AddScoped<Validators.Wallet.PushNotificationSettingsValidator>();

        return services;
    }

}
