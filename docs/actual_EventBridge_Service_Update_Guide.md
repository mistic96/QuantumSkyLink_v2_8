# EventBridge Service Update Guide
Version: 1.0  
Date: 2025-09-03  
Purpose: Service-by-service implementation guide for EventBridge migration

---

## Executive Summary

This guide provides detailed update requirements for all 24+ QuantumSkyLink v2 services during the EventBridge migration. Each service section includes current state, required changes, event specifications, and testing requirements.

**Migration Phases**:
- **Phase 0**: Infrastructure & Shared Components
- **Phase 1**: Critical Financial Services (Weeks 2-4)
- **Phase 2**: Business Logic Services (Weeks 5-8)
- **Phase 3**: Supporting Services (Weeks 9-12)
- **Phase 4**: API Gateways & Cleanup (Weeks 13-16)

---

## Phase 0: Infrastructure Components

### 1. QuantumSkyLink.Shared

**Current State**: No event handling infrastructure

**Required Updates**:

| Component | Action | Details |
|-----------|--------|---------|
| **New Namespace** | Create | `QuantumSkyLink.Shared.Eventing` |
| **Event Models** | Add | `EventEnvelope<T>`, `TraceContext`, `EventMetadata` |
| **Publisher** | Add | `IEventPublisher`, `EventBridgePublisher` |
| **Consumer** | Add | `SqsConsumerBackgroundService`, `IEventHandler<T>` |
| **PQC Utilities** | Add | `PqcJwsCompact`, `SignatureValidator` |
| **Canonicalizer** | Add | `EventCanonicalizer` for deterministic JSON |

**Dependencies to Add**:
```xml
<PackageReference Include="AWSSDK.EventBridge" Version="3.7.303.0" />
<PackageReference Include="AWSSDK.SQS" Version="3.7.304.0" />
<PackageReference Include="AWSSDK.Core" Version="3.7.303.0" />
```

**Testing Requirements**:
- Unit tests for canonicalization
- PQC signature round-trip tests
- Publisher retry logic tests
- Consumer error handling tests

---

### 2. QuantunSkyLink_v2.AppHost

**Current State**: Aspire orchestration without EventBridge

**Required Updates**:

| Component | Action | File | Changes |
|-----------|--------|------|---------|
| **CloudFormation** | Add | `AppHost.cs` | Reference EventBridge template |
| **Environment Vars** | Add | `appsettings.json` | AWS region, bus names |
| **Service Config** | Update | `AppHost.cs` | Add SQS queue references |
| **IAM Roles** | Add | `AppHost.cs` | EventBridge permissions |

**Configuration to Add**:
```csharp
// In AppHost.cs
builder.AddAWSCloudFormationTemplate("QSLEventBridge", 
    "infra/aws/qsl-eventbridge.template.json")
    .WithParameter("Environment", environment)
    .WithParameter("BusName", "qsl-core");

// Add to each service that needs EventBridge
.WithEnvironment("AWS__EventBridge__BusName", "qsl-core")
.WithEnvironment("AWS__Region", "us-east-1")
```

---

## Phase 1: Critical Financial Services

### 3. QuantumLedger.Hub

**Current State**: Direct HTTP calls, no event publishing

**Required Updates**:

| Component | Action | Details |
|-----------|--------|---------|
| **Publisher** | Add | Inject `IEventPublisher` |
| **Events to Publish** | Add | See table below |
| **Consumer** | Add | `LedgerValidationEventHandler` |
| **Queue** | Configure | FIFO queue `qsl-ledger-transactions.fifo` |
| **Handlers** | Implement | 5 event handlers |

**Events to Publish**:
| Event | Trigger | Priority |
|-------|---------|----------|
| `Ledger.Transaction.ValidationRequested.v1` | On validation request | Critical |
| `Ledger.Transaction.Validated.v1` | After successful validation | Critical |
| `Ledger.Transaction.ValidationFailed.v1` | On validation failure | High |
| `Ledger.Entry.Created.v1` | After ledger entry | Critical |
| `Ledger.Balance.Updated.v1` | After balance change | High |

**Code Changes Required**:
```csharp
// Program.cs - Add services
builder.Services.AddSingleton<IEventPublisher, EventBridgePublisher>();
builder.Services.AddHostedService<LedgerEventConsumer>();

// Remove RabbitMQ if present
// Remove: builder.Services.AddMassTransit(...)
```

**Testing Requirements**:
- Ledger validation event flow test
- FIFO ordering verification
- Idempotency testing

---

### 4. SignatureService

**Current State**: No event publishing, internal validation only

**Required Updates**:

| Component | Action | Details |
|-----------|--------|---------|
| **New Endpoints** | Add | `/keys/jwks-pqc`, `/sign/dilithium2` |
| **Key Rotation** | Add | Monthly rotation with events |
| **Audit Events** | Add | All signature operations |
| **Publisher** | Add | For key rotation notifications |

**New API Endpoints**:
```yaml
/keys/jwks-pqc:
  GET: Returns Dilithium2 public keys in JWKS format
  
/sign/dilithium2:
  POST: Signs data with current Dilithium2 key
  Body: { data: string, keyId?: string }
  
/keys/rotate:
  POST: Triggers key rotation (admin only)
```

**Events to Publish**:
| Event | Trigger |
|-------|---------|
| `Security.Key.Rotated.v1` | After key rotation |
| `Security.Signature.Created.v1` | After signing operation |
| `Security.Signature.Validated.v1` | After validation |

---

### 5. InfrastructureService

**Current State**: Direct blockchain calls, no event coordination

**Required Updates**:

| Component | Action | Details |
|-----------|--------|---------|
| **MultiNetworkCoordinator** | Add | Coordinate 6 networks |
| **EventJournaler** | Add | SurrealDB event storage |
| **Publishers** | Add | Network-specific events |
| **Consumers** | Add | Blockchain broadcast requests |

**Network Event Publishers**:
| Network | Event Prefix | Queue |
|---------|--------------|-------|
| MultiChain | `qsl.blockchain.multichain` | `qsl-multichain-inbox` |
| Ethereum | `qsl.blockchain.ethereum` | `qsl-ethereum-inbox` |
| Polygon | `qsl.blockchain.polygon` | `qsl-polygon-inbox` |
| Arbitrum | `qsl.blockchain.arbitrum` | `qsl-arbitrum-inbox` |
| Bitcoin | `qsl.blockchain.bitcoin` | `qsl-bitcoin-inbox` |
| BSC | `qsl.blockchain.bsc` | `qsl-bsc-inbox` |

**EventJournaler Implementation**:
- Consume from: `qsl-journal-inbox`
- Store in: SurrealDB `event_journal` table
- Index by: correlationId, timestamp, resource
- Retention: 90 days operational, 7 years financial

---

### 6. PaymentGatewayService

**Current State**: RabbitMQ with MassTransit

**Required Updates**:

| Component | Action | Details |
|-----------|--------|---------|
| **Remove MassTransit** | Delete | All MassTransit configuration |
| **Add EventBridge** | Add | Publisher and saga orchestrator |
| **Implement Saga** | Add | `PaymentSaga` with compensation |
| **Update Handlers** | Replace | Convert consumers to handlers |

**Migration Steps**:
1. Remove from Program.cs:
   ```csharp
   // DELETE THIS
   builder.Services.AddMassTransit(x => { ... });
   ```

2. Add to Program.cs:
   ```csharp
   builder.Services.AddSingleton<IEventPublisher, EventBridgePublisher>();
   builder.Services.AddScoped<PaymentSaga>();
   builder.Services.AddHostedService<PaymentEventConsumer>();
   ```

**Events to Publish**:
| Event | When | Saga Step |
|-------|------|-----------|
| `Payment.Initiated.v1` | Payment request received | 1 |
| `Payment.Validated.v1` | After ledger validation | 2 |
| `Payment.Authorized.v1` | After authorization | 3 |
| `Payment.Settled.v1` | After settlement | 4 |
| `Payment.Failed.v1` | On any failure | Compensation |

---

### 7. OrchestrationService

**Current State**: SNS/SQS for workflow events

**Required Updates**:

| Component | Action | Details |
|-----------|--------|---------|
| **Replace SNS** | Update | Use EventBridge client |
| **Event Mapping** | Update | New naming convention |
| **Saga Support** | Add | Orchestration patterns |

**Event Name Mapping**:
| Old SNS Topic | New EventBridge Event |
|---------------|----------------------|
| `workflow_started` | `Workflow.Started.v1` |
| `workflow_completed` | `Workflow.Completed.v1` |
| `workflow_failed` | `Workflow.Failed.v1` |
| `workflow_status_update` | `Workflow.StatusUpdated.v1` |

---

## Phase 2: Business Logic Services

### 8. TreasuryService

**Current State**: Direct database operations

**Required Updates**:

| Component | Action | Details |
|-----------|--------|---------|
| **Consumer** | Add | `TreasuryEventConsumer` |
| **Queue** | Configure | `qsl-treasury-inbox` |
| **Handlers** | Add | Payment settlement, fund reservation |
| **Publisher** | Add | Liquidity events |

**Event Handlers to Implement**:
```csharp
public class TreasuryEventHandlers
{
    IEventHandler<Payment.Settled.v1> // Process settlement
    IEventHandler<Saga.Funds.ReserveRequested.v1> // Reserve funds
    IEventHandler<Saga.Funds.ReleaseRequested.v1> // Release funds
    IEventHandler<Treasury.Reconciliation.Required.v1> // Reconcile
}
```

---

### 9. ComplianceService

**Current State**: Synchronous compliance checks

**Required Updates**:

| Component | Action | Details |
|-----------|--------|---------|
| **Consumer** | Add | `ComplianceEventConsumer` |
| **Queue** | Configure | `qsl-compliance-inbox` |
| **Publishers** | Add | Compliance decision events |
| **Circuit Breaker** | Add | For external service calls |

**Compliance Events**:
| Event | Purpose |
|-------|---------|
| `Compliance.Check.Requested.v1` | Initiate check |
| `Compliance.Check.Passed.v1` | Check passed |
| `Compliance.Check.Failed.v1` | Check failed |
| `Compliance.Alert.Raised.v1` | Suspicious activity |

---

### 10. NotificationService

**Current State**: SignalR for real-time, RabbitMQ for async

**Required Updates**:

| Component | Action | Details |
|-----------|--------|---------|
| **Remove SignalR** | Delete | All hub classes |
| **Add SSE** | Add | Server-sent events |
| **Consumer** | Add | Multi-channel router |
| **Queue** | Configure | `qsl-notification-inbox` |

**SignalR Removal**:
```csharp
// DELETE from Program.cs
builder.Services.AddSignalR();
app.MapHub<NotificationHub>("/hubs/notification");

// DELETE NotificationHub.cs entirely
```

**SSE Implementation**:
```csharp
// Add new SSE controller
[ApiController]
[Route("api/events")]
public class EventStreamController : ControllerBase
{
    [HttpGet("stream")]
    public async Task GetEventStream() { ... }
}
```

---

### 11. MarketplaceService

**Current State**: Direct service calls

**Required Updates**:

| Component | Action | Details |
|-----------|--------|---------|
| **Publisher** | Add | Order lifecycle events |
| **Handlers** | Add | Inventory, pricing events |
| **Saga Participation** | Add | Order fulfillment saga |

**Order Events**:
| Event | Trigger |
|-------|---------|
| `Order.Created.v1` | New order |
| `Order.Validated.v1` | After validation |
| `Order.Processing.v1` | Processing started |
| `Order.Fulfilled.v1` | Order complete |
| `Order.Cancelled.v1` | Cancellation |

---

## Phase 3: Supporting Services

### 12. UserService

**Current State**: Direct database operations

**Required Updates**:

| Component | Action | Details |
|-----------|--------|---------|
| **Publisher** | Add | User lifecycle events |
| **Events** | Add | Authentication, profile updates |
| **Queue** | Configure | `qsl-user-inbox` (standard) |

**User Events**:
| Event | When |
|-------|------|
| `User.Registered.v1` | New user signup |
| `User.Authenticated.v1` | Successful login |
| `User.ProfileUpdated.v1` | Profile change |
| `User.Deactivated.v1` | Account deactivation |

---

### 13. TokenService

**Current State**: Direct blockchain calls

**Required Updates**:

| Component | Action | Details |
|-----------|--------|---------|
| **Publisher** | Add | Token lifecycle events |
| **Handlers** | Add | Blockchain confirmations |
| **Multi-network** | Add | Network coordination |

---

### 14. AccountService

**Current State**: Mixed (some events, mostly direct)

**Required Updates**:

| Component | Action | Details |
|-----------|--------|---------|
| **Enhance Publisher** | Update | Standardize event format |
| **Add Handlers** | Add | Balance updates, deposits |
| **Ledger Integration** | Add | Validate with QuantumLedger.Hub |

---

### 15. FeeService

**Current State**: Synchronous fee calculation

**Required Updates**:

| Component | Action | Details |
|-----------|--------|---------|
| **Handlers** | Add | Fee calculation requests |
| **Publisher** | Add | Fee assessment events |
| **Queue** | Configure | `qsl-fee-inbox` |

---

### 16. SecurityService

**Current State**: Internal security monitoring

**Required Updates**:

| Component | Action | Details |
|-----------|--------|---------|
| **Publisher** | Add | Security events |
| **Handlers** | Add | Threat detection |
| **Audit Stream** | Add | Compliance events |

---

### 17. GovernanceService

**Current State**: Database-driven governance

**Required Updates**:

| Component | Action | Details |
|-----------|--------|---------|
| **Publisher** | Add | Governance decisions |
| **Handlers** | Add | Voting events |

---

### 18. AIReviewService

**Current State**: Synchronous AI analysis

**Required Updates**:

| Component | Action | Details |
|-----------|--------|---------|
| **Async Processing** | Add | Event-driven analysis |
| **Publisher** | Add | Analysis results |
| **Queue** | Configure | `qsl-aireview-inbox` |

---

### 19. IdentityVerificationService

**Current State**: Direct API calls

**Required Updates**:

| Component | Action | Details |
|-----------|--------|---------|
| **Publisher** | Add | KYC/AML events |
| **Handlers** | Add | Verification requests |
| **Circuit Breaker** | Add | External service protection |

---

### 20. LiquidationService

**Current State**: Scheduled jobs only

**Required Updates**:

| Component | Action | Details |
|-----------|--------|---------|
| **Event Triggers** | Add | Real-time liquidation |
| **Publisher** | Add | Liquidation events |
| **Handlers** | Add | Margin calls |

---

## Phase 4: API Gateways

### 21-23. Web/Admin/Mobile API Gateways

**Current State**: SignalR for real-time updates

**Common Updates for All Gateways**:

| Component | Action | Details |
|-----------|--------|---------|
| **Remove SignalR** | Delete | All SignalR hubs and configuration |
| **Add SSE** | Add | Server-sent events endpoints |
| **Add Consumer** | Add | `ClientUpdateConsumer` |
| **Query Endpoints** | Add | Event status queries |

**SSE Endpoints to Add**:
```csharp
[Route("api/events")]
public class EventController : ControllerBase
{
    [HttpGet("{correlationId}")]
    public async Task<IActionResult> GetEventStatus(string correlationId);
    
    [HttpGet("stream")]
    public async Task GetEventStream(CancellationToken ct);
    
    [HttpGet("stream/user")]
    [Authorize]
    public async Task GetUserEventStream(CancellationToken ct);
}
```

**SignalR Removal Checklist**:
- [ ] Remove `Microsoft.AspNetCore.SignalR` package
- [ ] Delete all Hub classes
- [ ] Remove `builder.Services.AddSignalR()`
- [ ] Remove `app.MapHub<>()` calls
- [ ] Update client applications to use SSE

---

## Common Updates for ALL Services

### Dependencies to Add

```xml
<!-- Add to all service .csproj files -->
<ItemGroup>
  <PackageReference Include="AWSSDK.EventBridge" Version="3.7.303.0" />
  <PackageReference Include="AWSSDK.SQS" Version="3.7.304.0" />
  <PackageReference Include="QuantumSkyLink.Shared" Version="1.0.0" />
</ItemGroup>
```

### Dependencies to Remove

```xml
<!-- Remove from all service .csproj files -->
<!-- DELETE THESE -->
<PackageReference Include="MassTransit" />
<PackageReference Include="MassTransit.RabbitMQ" />
<PackageReference Include="MassTransit.AspNetCore" />
<PackageReference Include="Microsoft.AspNetCore.SignalR" />
<PackageReference Include="Microsoft.AspNetCore.SignalR.Client" />
```

### Configuration Updates

**Add to appsettings.json**:
```json
{
  "AWS": {
    "Region": "us-east-1",
    "EventBridge": {
      "BusName": "qsl-core"
    },
    "SQS": {
      "QueueUrl": "https://sqs.us-east-1.amazonaws.com/123456789012/qsl-{service}-inbox"
    }
  }
}
```

**Remove from appsettings.json**:
```json
// DELETE THESE
"RabbitMQ": { ... },
"MassTransit": { ... },
"SignalR": { ... }
```

### Code Pattern Updates

**Replace IPublishEndpoint with IEventPublisher**:
```csharp
// OLD (MassTransit)
public MyService(IPublishEndpoint publisher) { }

// NEW (EventBridge)
public MyService(IEventPublisher publisher) { }
```

**Replace IConsumer with IEventHandler**:
```csharp
// OLD (MassTransit)
public class MyConsumer : IConsumer<MyMessage> { }

// NEW (EventBridge)
public class MyHandler : IEventHandler<MyEvent> { }
```

---

## Testing Requirements by Service Type

### Financial Services Testing
- [ ] Ledger validation flow
- [ ] FIFO queue ordering
- [ ] Saga compensation
- [ ] Idempotency
- [ ] PQC signature validation

### Business Services Testing
- [ ] Event publishing
- [ ] Handler processing
- [ ] Error handling
- [ ] DLQ processing
- [ ] Performance benchmarks

### Gateway Testing
- [ ] SSE connection stability
- [ ] Event streaming
- [ ] Authentication flow
- [ ] Mobile optimization
- [ ] Reconnection logic

---

## Rollback Procedures

### Service-Level Rollback

**Feature Flag Configuration**:
```json
{
  "FeatureManagement": {
    "EventBridge_PaymentGatewayService": true,
    "EventBridge_TreasuryService": false,
    "EventBridge_NotificationService": false
  }
}
```

**Rollback Steps**:
1. Set feature flag to `false`
2. Restart service
3. Monitor RabbitMQ traffic resumption
4. Verify no event loss
5. Document rollback reason

### Emergency Rollback Script
```bash
#!/bin/bash
# emergency-rollback.sh
SERVICE=$1
aws ssm put-parameter --name "/qsl/$SERVICE/EventBridge" --value "false" --overwrite
kubectl rollout restart deployment/$SERVICE
echo "Rolled back $SERVICE to RabbitMQ"
```

---

## Migration Validation Checklist

### Per-Service Validation
- [ ] All events have PQC signatures
- [ ] No MassTransit references remain
- [ ] No SignalR references remain
- [ ] SQS consumers operational
- [ ] Event publishers working
- [ ] DLQ near zero
- [ ] Performance metrics met
- [ ] Integration tests passing

### System-Wide Validation
- [ ] End-to-end payment flow
- [ ] Multi-network blockchain broadcast
- [ ] Saga compensation working
- [ ] SSE real-time updates
- [ ] Event journal capturing all events
- [ ] Cost targets achieved

---

## Support Resources

### Documentation
- Requirements: `actual_EventBridge_Migration_Requirements.md`
- Deployment Strategy: `actual_EventBridge_Deployment_MVP_Strategy.md`
- CloudFormation Templates: `infra/aws/eventbridge/`

### Planning Documents
- Cost Analysis: `planning_EventBridge_Cost_Analysis_Report.md`
- Original Plan: `planning_EventBridge_Implementation_Plan.md`
- Detailed Technical Plan: `planning_EventBridge_Migration_Detailed_Implementation_Plan.md`
- Mobile Strategy: `planning_EventBridge_Mobile_Integration_Strategy.md`

### Monitoring Dashboards
- EventBridge Metrics: CloudWatch Dashboard
- SQS Queue Depths: Custom Dashboard
- Service Health: Aspire Dashboard

### Contact Points
- Architecture Team: For design questions
- DevOps Team: For infrastructure support
- Security Team: For PQC implementation

---

**Document Status**: Ready for implementation  
**Last Updated**: 2025-09-03  
**Version**: 1.0
