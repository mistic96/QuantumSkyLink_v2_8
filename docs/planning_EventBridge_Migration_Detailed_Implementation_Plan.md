# EventBridge Migration Detailed Implementation Plan
Version: 1.0 (PQC Dilithium2, MassTransit removal, SSE)

Prepared: 2025-09-03  
Prepared by: Cline

---

## 1) Objectives
- Migrate asynchronous messaging to Amazon EventBridge with SQS targets.
- Enforce Dilithium2 PQC signatures on every event; verify before processing.
- Remove MassTransit and SignalR; implement native SQS consumers and SSE for real-time status.
- Provide a queryable event journal for end-to-end tracing and client status.

## 2) Scope
In scope:
- Event buses, rules, SQS targets, DLQs (CloudFormation via Aspire AppHost).
- Shared eventing library: envelope, canonicalizer, publisher, PQC JWS utilities, SQS consumer host.
- Publishers in OrchestrationService and PaymentGatewayService.
- Native SQS consumers in TreasuryService, ComplianceService, NotificationService.
- Event journal consumer in InfrastructureService writing to SurrealDB.
- SSE endpoints in Web/Admin/Mobile API gateways; SQS-driven minimal “update available” channel.
- SignatureService JWKs-PQC endpoint and signing endpoint.

Out of scope:
- Step Functions/Lambda consumers.
- Multi-region buses and cross-region replication.
- EventBridge Archive initial activation (documented for later).
- Hangfire replacement.

## 3) Target Architecture
- EventBridge Bus: `qsl-core` (initial).
- Event Sources: `qsl.workflow`, `qsl.payment`, `qsl.order`, `qsl.user`, `qsl.token`, `qsl.security`.
- `detail-type`: `Entity.Action.vN` (e.g., `Payment.Initiated.v1`).
- Targets: SQS queues per service/stream (standard or FIFO as needed); DLQs for each.
- Journal: EventBridge rule `journal-all` → SQS `qsl-journal-inbox` → EventJournaler → SurrealDB `event_journal`.
- Client updates: EventBridge rule `client-updates` → SQS `qsl-clientupdates-inbox` → Gateway consumer → SSE.

## 4) Event Model and PQC Signature
Envelope (EventBridge fields + `detail`):
- `source`: `qsl.{domain}`
- `detail-type`: `Entity.Action.vN`
- `resources`: `[ "urn:qsl:{service}:{entity}:{id}" ]`
- `detail` object:
  - `version`: `"v1"`
  - `trace`: `{ correlationId, userId, tenantId, deviceId }`
  - `resource`: `"urn:qsl:{service}:{entity}:{id}"`
  - `detailCore`: `{ business payload; avoid PII where possible }`
  - `sig_compact`: `"<header>.<payload>.<signature>"`

QSL-JWS-PQC compact (dot-delimited, Dilithium2):
- header (JSON):
  - `alg`: `"Dilithium2"`
  - `kid`: `"<key-id>"`
  - `ts`: `"<ISO8601 UTC>"`
  - `hash`: `"SHA-256"`
  - `typ`: `"QSL-JWS-PQC"`
- payload: canonical JSON of `detailCore` (stable key ordering, UTF-8)
- signature: Dilithium2 signature over `base64url(header) + "." + base64url(payload)`

Publisher flow:
1. Canonicalize `detailCore` → bytes
2. `header := { alg=Dilithium2, kid, ts, hash=SHA-256, typ="QSL-JWS-PQC" }`
3. `sig_compact := SignDilithium2(base64url(header) + "." + base64url(payload))`
4. attach `sig_compact` into `detail` and `PutEvents(BusName=qsl-core)`

Consumer verification:
- Split `sig_compact`; parse `header`; require `alg = "Dilithium2"`
- Re-canonicalize `detailCore`; recompute compact input; fetch pubkey by `kid` from SignatureService (`/keys/jwks-pqc`)
- Verify signature; enforce time skew (`+/- 5 minutes`)
- On failure: reject → DLQ; record reason; increment metrics

JWKS-PQC format (example):
```
{
  "keys": [
    {
      "kty": "PQK",
      "alg": "Dilithium2",
      "kid": "sigsvc-2025-09",
      "crv": "Dilithium2",
      "x": "<base64url-public-key>"
    }
  ]
}
```

## 5) Infra-as-Code (CloudFormation via Aspire)
File: `infra/aws/qsl-eventbridge.template.json`
- Parameters:
  - `Environment` (e.g., `mvp`)
  - `BusName` (default `qsl-core`)
- Resources:
  - EventBus (BusName)
  - SQS queues per service (inbox + DLQ):
    - `qsl-treasury-inbox` (+ DLQ)
    - `qsl-compliance-inbox` (+ DLQ)
    - `qsl-notification-inbox` (+ DLQ)
    - `qsl-journal-inbox` (+ DLQ)
    - `qsl-clientupdates-inbox` (+ DLQ)
    - Additional inbox queues as needed (marketplace, fee, user, etc.)
  - Rules:
    - `workflow-all`: `source = qsl.workflow` → targets: relevant SQS inbox queues
    - `payment-all`: `source = qsl.payment` → targets: treasury/compliance/notification/fee
    - `journal-all`: match selected sources → `qsl-journal-inbox`
    - `client-updates`: match status-change events → `qsl-clientupdates-inbox`
  - EventBridge → SQS target policies
  - Redrive policies from inbox to corresponding DLQs

AppHost wiring (`QuantunSkyLink_v2.AppHost/AppHost.cs`):
- Add:
```
builder.AddAWSCloudFormationTemplate("QSLEventBridge", "infra/aws/qsl-eventbridge.template.json")
  .WithParameter("Environment", builder.Configuration["Deployment:Environment"] ?? "mvp")
  .WithReference(awsConfig);
```

## 6) Shared Library (QuantumSkyLink.Shared.Eventing)
- Models:
  - `EventEnvelope<TDetailCore>`
  - `TraceContext { correlationId, userId, tenantId, deviceId }`
- Canonicalizer:
  - Deterministic JSON serializer (sorted keys, UTF-8)
- Publisher:
  - `EventBridgePublisher` (IAmazonEventBridge) → `PutEventsAsync`
- PQC utilities:
  - `SignatureServiceClient`: `SignDilithium2(input)` and `FetchJwksPqc()`
  - `PqcJwsCompact`: Create/Verify (`sig_compact`, `detailCore`, `kid/pubkey`, `skew`)
- SQS Consumer Host:
  - `SqsConsumerBackgroundService`:
    - Long-poll Receive → parse envelope → verify sig → resolve `IEventHandler<T>` → execute → Delete
    - Config: `QueueUrl`, `MaxMessages`, `WaitTimeSeconds=20`, `VisibilityTimeout`, `MaxInFlight`, Backoff
  - Handler contracts: `IEventHandler<TDetailCore>` registered by `(source, detail-type)`

## 7) Service Changes (Phase 1)
**OrchestrationService**
- Replace SNS publisher with `EventBridgePublisher`
- Map legacy event names to:
  - `workflow_started` → `Workflow.Started.v1`
  - `workflow_completed` → `Workflow.Completed.v1`
  - `workflow_failed` → `Workflow.Failed.v1`
  - `workflow_status_update` → `Workflow.StatusUpdated.v1`
  - `workflow_error` → `Workflow.Error.v1`
- `resource`: `urn:qsl:orchestration:workflow:{workflowId}`

**PaymentGatewayService**
- Add publishers for payment lifecycle:
  - `Payment.Initiated.v1`, `Payment.Authorized.v1`, `Payment.Settled.v1`, `Payment.Failed.v1`
- `resource`: `urn:qsl:paymentgateway:payment:{paymentId}`

**TreasuryService, ComplianceService, NotificationService**
- Remove MassTransit references and configuration
- Add `SqsConsumerBackgroundService` bound to each service’s inbox queue
- Register handlers for relevant events (Payment.*, Workflow.*)
- Idempotency checks using business keys (e.g., `PaymentId`) stored in a short-lived dedup table

**InfrastructureService**
- `EventJournaler`: SQS consumer for `qsl-journal-inbox` → SurrealDB `event_journal`
  - Index: `correlationId`, `resource`, `time`, `detailType`; store full envelope + small projection (status, lastUpdated)

**Gateways (Web/Admin/Mobile)**
- Remove SignalR usage and hub mapping
- SSE endpoints:
  - `GET /events/{correlationId}`
  - `GET /events/stream?correlationId=...`
  - `GET /events/stream/user` (auth)
- `ClientUpdateConsumer`: consumes `qsl-clientupdates-inbox` to notify active SSE streams; fallback to short-interval journal polling

**SignatureService**
- Add `/keys/jwks-pqc` endpoint with Dilithium2 public keys
- Signing endpoint for publisher services
- Key rotation: publish new `kid` → accept both during overlap → retire old

## 8) Configuration and IAM
Config (`appsettings`):
- `AWS:Region`
- `AWS:EventBridge:BusName = qsl-core`
- `Messaging:UseEventBridge = true`
- `Messaging:Consumers:SQS:{ServiceName}:{QueueUrl}`, `MaxInFlight`, `VisibilityTimeout`, `Backoff`
Remove static `AWS:AccessKey`/`AWS:SecretKey`; rely on IAM roles.

IAM:
- Publishers: `PutEvents` on `qsl-core` bus
- EventBridge → SQS: Allow `SendMessage` to target queues
- Consumers: SQS `ReceiveMessage`/`DeleteMessage` on their inbox and DLQ

## 9) Monitoring, Alarms, and Cost
CloudWatch dashboards & alarms:
- EventBridge: `PutEventsFailed`, `RuleInvocationsFailed`
- SQS: `ApproximateAgeOfOldestMessage`, `ApproximateNumberOfMessagesNotVisible`, DLQ message count
- Budget alarms on EventBridge + SQS

DLQ/Replay:
- Standardized failure reason fields
- Runbook for redrive from DLQ to inbox after fix
- Optional: EventBridge Archive (future)

## 10) Testing Strategy
Unit:
- Publisher: detail → canonicalize → sign (Dilithium2) → PutEvents request formation
- PQC utilities: Create/Verify roundtrips; skew validation; error paths
- SQS consumer host: happy path, signature invalid, handler failure → DLQ

Integration:
- End-to-end: Publish `Payment.Initiated.v1` → Treasury/Compliance/Notification process → Journal updated → SSE emits update → `GET /events/{correlationId}` shows timeline

Performance:
- p95 signature verify < 5 ms
- p95 end-to-end (PutEvents → handler) ≤ 300 ms typical
- SSE push ≤ 500 ms p95 after status change

## 11) Rollout Plan
**Phase 0 (prep)**
- Add `Shared.Eventing`, PQC utils, `EventBridgePublisher`, `SqsConsumerBackgroundService`
- Add `QSLEventBridge` infra; AppHost reference; deploy infra only

**Phase 1**
- OrchestrationService + PaymentGatewayService → EventBridge publishers (Dilithium2)
- Treasury, Compliance, Notification → native SQS consumers
- EventJournaler online; `journal-all` rule active
- SSE endpoints online in Gateways; `ClientUpdateConsumer` wired
- Remove MassTransit and SignalR from Phase 1 projects

**Validation**
- E2E payment flow; DLQs near zero; alarms verified; load at MVP scale

**Phase 2**
- Expand to Marketplace, User, Token, Security domains; introduce FIFO queues where strict ordering required

**Phase 3**
- Remove SNS remnants; ensure no RabbitMQ path is used for async flows
- Optional: enable EventBridge Archive

## 12) Acceptance Criteria
- 100% of events have `sig_compact` (Dilithium2) and pass verification before processing
- OrchestrationService/PaymentGatewayService publish via EventBridge only
- Treasury/Compliance/Notification consume via native SQS consumers
- Journal captures ≥ 99.9% events; `GET /events/{correlationId}` returns ordered timeline < 200 ms typical
- SSE real-time status operates with p95 < 500 ms
- No hardcoded AWS credentials; IAM least-privilege enforced
- CloudWatch alarms and runbooks in place; rollback switches validated

## 13) Developer Task Checklist (Phase 1)
- Shared.Eventing: models, canonicalizer, publisher, PQC utils, SQS host
- SignatureService: JWKS-PQC endpoint + signing
- Infra: `qsl-eventbridge.template.json`; AppHost reference
- OrchestrationService: swap SNS → `EventBridgePublisher`; map event types
- PaymentGatewayService: add lifecycle publishers
- Treasury/Compliance/Notification: add SQS consumers + handlers; idempotency
- InfrastructureService: `EventJournaler`
- Gateways: SSE endpoints + `ClientUpdateConsumer`; remove SignalR
- Config & IAM: bus name, queues, roles; remove static creds
- Tests: unit + integration; load test; alarms and DLQ validation

---

If you want naming or retention changes for queues, or want to include a separate `qsl-mobile` bus now, specify them and I will update the plan before saving.
    "Team": "${team_name}",
    "CostCenter": "${cost_center}",
    "Project": "QuantumSkyLink",
    "EventSource": "${event_source}",
    "Priority": "${priority_level}"
  }
}
```

