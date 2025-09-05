FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG TARGETARCH
WORKDIR /src
COPY ["QuantunSkyLink_v2.AppHost/QuantunSkyLink_v2.AppHost.csproj", "QuantunSkyLink_v2.AppHost/"]
COPY ["QuantunSkyLink_v2.ServiceDefaults/QuantunSkyLink_v2.ServiceDefaults.csproj", "QuantunSkyLink_v2.ServiceDefaults/"]
COPY ["src/UserService/UserService.csproj", "src/UserService/"]
COPY ["src/AccountService/AccountService.csproj", "src/AccountService/"]
COPY ["src/ComplianceService/ComplianceService.csproj", "src/ComplianceService/"]
COPY ["src/FeeService/FeeService.csproj", "src/FeeService/"]
COPY ["src/SecurityService/SecurityService.csproj", "src/SecurityService/"]
COPY ["src/TokenService/TokenService.csproj", "src/TokenService/"]
COPY ["src/GovernanceService/GovernanceService.csproj", "src/GovernanceService/"]
COPY ["src/TreasuryService/TreasuryService.csproj", "src/TreasuryService/"]
COPY ["src/PaymentGatewayService/PaymentGatewayService.csproj", "src/PaymentGatewayService/"]
COPY ["src/LiquidationService/LiquidationService.csproj", "src/LiquidationService/"]
COPY ["src/InfrastructureService/InfrastructureService.csproj", "src/InfrastructureService/"]
COPY ["src/QuantumLedger.Hub/QuantumLedger.Hub.csproj", "src/QuantumLedger.Hub/"]
COPY ["src/IdentityVerificationService/IdentityVerificationService.csproj", "src/IdentityVerificationService/"]
COPY ["src/AIReviewService/AIReviewService.csproj", "src/AIReviewService/"]
COPY ["src/NotificationService/NotificationService.csproj", "src/NotificationService/"]
COPY ["src/MarketplaceService/MarketplaceService.csproj", "src/MarketplaceService/"]
COPY ["src/WebAPIGateway/WebAPIGateway.csproj", "src/WebAPIGateway/"]
COPY ["src/AdminAPIGateway/AdminAPIGateway.csproj", "src/AdminAPIGateway/"]
COPY ["src/QuantumSkyLink.Shared/QuantumSkyLink.Shared.csproj", "src/QuantumSkyLink.Shared/"]
COPY ["src/RefitClient/RefitClient.csproj", "src/RefitClient/"]
COPY ["src/MobileAPIGateway/MobileAPIGateway.csproj", "src/MobileAPIGateway/"]
COPY ["src/SignatureService/SignatureService.csproj", "src/SignatureService/"]
COPY ["src/OrchestrationService/OrchestrationService.csproj", "src/OrchestrationService/"]
COPY ["src/QuantumLedger.Blockchain/QuantumLedger.Blockchain.csproj", "src/QuantumLedger.Blockchain/"]
COPY ["src/QuantumLedger.Cryptography/QuantumLedger.Cryptography.csproj", "src/QuantumLedger.Cryptography/"]
COPY ["src/QuantumLedger.Cryptography.PQC/QuantumLedger.Cryptography.PQC.csproj", "src/QuantumLedger.Cryptography.PQC/"]
COPY ["src/QuantumLedger.Data/QuantumLedger.Data.csproj", "src/QuantumLedger.Data/"]
COPY ["src/QuantumLedger.Models/QuantumLedger.Models.csproj", "src/QuantumLedger.Models/"]
COPY ["src/UserService.Tests/UserService.Tests.csproj", "src/UserService.Tests/"]
COPY ["src/LiquidStorageCloud.Backup.Core/LiquidStorageCloud.Backup.Core.csproj", "src/LiquidStorageCloud.Backup.Core/"]
COPY ["src/LiquidStorageCloud.Backup.Core/temp.csproj", "src/LiquidStorageCloud.Backup.Core/"]
COPY ["src/LiquidStorageCloud.Core/LiquidStorageCloud.Core.csproj", "src/LiquidStorageCloud.Core/"]
COPY ["src/LiquidStorageCloud.DataManagement.Core/LiquidStorageCloud.DataManagement.Core.csproj", "src/LiquidStorageCloud.DataManagement.Core/"]
COPY ["src/LiquidStorageCloud.DataManagement.EntityFramework/LiquidStorageCloud.DataManagement.EntityFramework.csproj", "src/LiquidStorageCloud.DataManagement.EntityFramework/"]
COPY ["src/LiquidStorageCloud.Services.EventConsumers.Core/LiquidStorageCloud.Services.EventConsumers.csproj", "src/LiquidStorageCloud.Services.EventConsumers.Core/"]
COPY ["src/LiquidStorageCloud.Services.EventPublishing/LiquidStorageCloud.Services.EventPublishing.csproj", "src/LiquidStorageCloud.Services.EventPublishing/"]
COPY ["tests/QuantumSkyLink.Integration.Tests/QuantumSkyLink.Integration.Tests.csproj", "tests/QuantumSkyLink.Integration.Tests/"]
RUN dotnet restore "src/MobileAPIGateway/MobileAPIGateway.csproj" -a $TARGETARCH
COPY . .
WORKDIR "/src/src/MobileAPIGateway"
RUN dotnet build "MobileAPIGateway.csproj" -c Release -o /app/build -a $TARGETARCH

FROM build AS publish
RUN apt-get update -yq \
    && apt-get install -yq ca-certificates curl gnupg \
    && mkdir -p /etc/apt/keyrings \
    && curl -fsSL https://deb.nodesource.com/gpgkey/nodesource-repo.gpg.key | gpg --dearmor -o /etc/apt/keyrings/nodesource.gpg \
    && echo "deb [signed-by=/etc/apt/keyrings/nodesource.gpg] https://deb.nodesource.com/node_22.x nodistro main" | tee /etc/apt/sources.list.d/nodesource.list \
    && apt-get update -yq \
    && apt-get install nodejs -yq
RUN dotnet publish "MobileAPIGateway.csproj" -c Release -o /app/publish -a $TARGETARCH

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MobileAPIGateway.dll"]
