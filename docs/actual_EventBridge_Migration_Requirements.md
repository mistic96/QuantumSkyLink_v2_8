# EventBridge Migration Requirements Document
Version: 2.0 - Concise Requirements Focus  
Date: 2025-09-03  
Status: COMPREHENSIVE REQUIREMENTS

---

## Executive Summary

Complete migration requirements from RabbitMQ/SNS/SQS to AWS EventBridge for QuantumSkyLink v2, including QuantumLedger.Hub integration, 6-network blockchain support, and enterprise-grade operational capabilities.

**Target Metrics**: 99.99% reliability | Sub-100ms p95 latency | 38-41% cost reduction | Zero data loss

---

## 1. Core Architecture Requirements

### 1.1 Event Bus Topology

| Bus Name | Purpose | Event Sources | Priority |
|----------|---------|---------------|----------|
| `qsl-core` | Primary bus | All services | P0 |
| `qsl-financial` | QuantumLedger.Hub events | Ledger, Payment, Treasury | P0 |
| `qsl-blockchain` | Multi-network blockchain | 6 blockchain networks | P0 |
| `qsl-business` | Business operations | Order, User, Token | P1 |
| `qsl-system` | Monitoring & audit | Security, Compliance | P1 |

### 1.2 Event Naming Standards

**Format**: `{Domain}.{Entity}.{Action}.v{Version}`

**Examples**:
- `Ledger.Transaction.Validated.v1`
- `Blockchain.Ethereum.Confirmed.v1`
- `Payment.Wire.Initiated.v1`
- `Saga.Payment.CompensationRequired.v1`

### 1.3 Infrastructure Components

- **SQS Queues**: Standard (most services) + FIFO (ordered processing)
- **DLQs**: One per queue, 14-day retention
- **Event Journal**: SurrealDB-backed, indexed by correlationId
- **SSE Endpoints**: Real-time updates replacing SignalR
- **Archive**: Long-term storage with replay capability

---

## 2. QuantumLedger.Hub Integration

### 2.1 Required Event Categories

| Category | Events | SLA | Queue Type |
|----------|--------|-----|------------|
| **Validation** | ValidationRequested, Validated, Failed | <50ms | FIFO |
| **Recording** | EntryCreated, BalanceUpdated | <100ms | FIFO |
| **Audit** | ComplianceCheck, AnomalyDetected | <500ms | Standard |
| **Propagation** | BlockchainBroadcast, Confirmed | <1000ms | Standard |

### 2.2 Integration Requirements

- **Pattern**: Event Sourcing + CQRS
- **Validation**: All financial events must validate with QuantumLedger.Hub first
- **Immutability**: Ledger entries are append-only
- **Consistency**: Strong consistency for financial operations
- **Reference Implementation**: See `LedgerValidationEventHandler` pattern

---

## 3. Blockchain Network Events

### 3.1 Network Coverage Requirements

| Network | Chain ID | Event Types | Priority | Queue |
|---------|----------|-------------|----------|-------|
| **MultiChain** | private | Asset, Transaction, Block | P0 | FIFO |
| **Ethereum** | 11155111 | Transaction, Gas, Contract | P0 | Standard |
| **Polygon** | 80001 | Transaction, Checkpoint, Bridge | P0 | Standard |
| **Arbitrum** | 421614 | L2Transaction, Rollup | P1 | Standard |
| **Bitcoin** | testnet | Transaction, UTXO, Block | P1 | Standard |
| **BSC** | 97 | Transaction, Validator | P1 | Standard |

### 3.2 Multi-Network Coordination

**Requirements**:
- Parallel broadcasting to multiple networks
- Network-specific confirmation thresholds
- Automatic failover on network issues
- Gas price optimization events
- **Pattern**: Multi-Network Coordinator

---

## 4. Error Recovery & Compensation

### 4.1 Saga Pattern Requirements

| Saga Type | Steps | Compensation Strategy | Timeout |
|-----------|-------|----------------------|---------|
| **Payment** | 5 (Validate→Reserve→Execute→Broadcast→Confirm) | Reverse order | 5min |
| **Order** | 4 (Create→Validate→Process→Fulfill) | State rollback | 10min |
| **Transfer** | 3 (Lock→Transfer→Unlock) | Compensate on failure | 3min |

### 4.2 Circuit Breaker Configuration

| Service | Failure Threshold | Open Duration | Half-Open Tests |
|---------|------------------|---------------|-----------------|
| External APIs | 50% over 10 calls | 30s | 3 |
| Blockchain Networks | 30% over 5 calls | 60s | 1 |
| Internal Services | 70% over 20 calls | 10s | 5 |

---

## 5. Security Requirements

### 5.1 PQC Signature Enforcement

**Mandatory for all events**:
- Algorithm: Dilithium2
- Format: QSL-JWS-PQC compact
- Verification: Before processing any event
- Key Rotation: Monthly with 7-day overlap
- **Implementation**: SignatureService JWKS-PQC endpoint

### 5.2 Encryption & Privacy

| Data Type | Encryption | Method | Key Management |
|-----------|------------|--------|----------------|
| PII Fields | Field-level | AES-256-GCM | AWS KMS |
| Financial Data | Envelope | AES-256-GCM | Multi-cloud KMS |
| Audit Logs | At-rest | S3 SSE | AWS managed |

### 5.3 Audit Event Requirements

**Categories**: Authentication, Authorization, DataAccess, Compliance, Privacy  
**Retention**: 7 years (financial), 90 days (operational)  
**Pattern**: Structured audit events with correlation tracking

---

## 6. Performance Optimization

### 6.1 Latency Targets

| Event Priority | P50 | P95 | P99 | Max |
|---------------|-----|-----|-----|-----|
| **Critical** | 20ms | 50ms | 100ms | 500ms |
| **High** | 50ms | 100ms | 200ms | 1s |
| **Standard** | 100ms | 500ms | 1s | 5s |
| **Low** | 500ms | 2s | 5s | 30s |

### 6.2 Optimization Strategies

- **Batching**: Events with cost < $0.000005
- **Filtering**: Rule-based routing to reduce processing
- **Connection Pooling**: 10 connections per service
- **Caching**: 5-minute TTL for frequently accessed data
- **Pattern**: BatchEventProcessor, CostOptimizedPublisher

---

## 7. Data Consistency

### 7.1 Event Ordering Guarantees

| Scenario | Ordering Requirement | Implementation |
|----------|---------------------|----------------|
| Financial Transactions | Strict | FIFO queue with MessageGroupId |
| User Updates | Eventual | Standard queue with idempotency |
| Analytics | Best effort | Batch processing |

### 7.2 CQRS Requirements

- **Write Model**: Event-sourced aggregates
- **Read Model**: Projected from events
- **Consistency**: Eventual (except financial)
- **Pattern**: PaymentAggregate, PaymentProjection

---

## 8. Migration Strategy

### 8.1 Phase Timeline

| Phase | Duration | Services | Risk | Rollback Time |
|-------|----------|----------|------|---------------|
| **Phase 0** | Week 1 | Infrastructure setup | Low | N/A |
| **Phase 1** | Weeks 2-4 | Orchestration, Payment, Notification | Medium | 1 hour |
| **Phase 2** | Weeks 5-8 | Marketplace, Compliance, Treasury | Medium | 2 hours |
| **Phase 3** | Weeks 9-12 | User, Token, Security | Low | 4 hours |
| **Phase 4** | Weeks 13-16 | Optimization & cleanup | Low | N/A |

### 8.2 Rollback Triggers

| Metric | Threshold | Action |
|--------|-----------|--------|
| Error Rate | >5% for 5min | Auto-rollback |
| P95 Latency | >500ms for 10min | Alert + Manual decision |
| DLQ Depth | >1000 messages | Pause traffic |
| Delivery Failure | >1% for 5min | Auto-rollback |

---

## 9. Testing Requirements

### 9.1 Test Coverage Targets

| Test Type | Coverage | Tools | Environment |
|-----------|----------|-------|-------------|
| Unit | 90% | Jest, xUnit | Local |
| Integration | 80% | Playwright, Postman | LocalStack |
| Contract | 100% critical paths | Pact | Staging |
| Load | 10x current volume | K6, JMeter | Pre-prod |
| Chaos | 5 scenarios | Chaos Monkey | Staging |

### 9.2 Validation Criteria

- **E2E Payment Flow**: <300ms with all validations
- **Multi-network broadcast**: Success on 5/6 networks minimum
- **Saga compensation**: 100% rollback success
- **PQC validation**: <5ms per signature

---

## 10. Operational Requirements

### 10.1 Monitoring & Alerting

| Metric | Warning | Critical | Dashboard |
|--------|---------|----------|-----------|
| Event Processing Rate | <80% baseline | <50% baseline | Real-time |
| DLQ Messages | >100 | >1000 | 5-min refresh |
| Signature Failures | >1% | >5% | Real-time |
| Cost per Event | >$0.00002 | >$0.00005 | Daily |

### 10.2 Runbook Requirements

**Required Runbooks**:
- Event replay from archive
- DLQ message reprocessing
- Circuit breaker manual reset
- Saga compensation trigger
- Emergency rollback procedure

---

## 11. Cost Management

### 11.1 Cost Targets

| Component | Current | Target | Savings |
|-----------|---------|--------|---------|
| Messaging Infrastructure | $1,100/mo | $650/mo | 41% |
| Event Processing | $0.00003/event | $0.00001/event | 67% |
| Storage (Archive) | $200/mo | $50/mo | 75% |
| **Total** | $1,300/mo | $700/mo | 46% |

### 11.2 Optimization Requirements

- Event batching for low-priority events
- Archive tiering (hot/warm/cold)
- Reserved capacity for predictable workloads
- Cost allocation tags for chargeback

---

## 12. Compliance & Governance

### 12.1 Regulatory Requirements

| Regulation | Requirement | Implementation |
|------------|-------------|----------------|
| **GDPR** | Right to deletion | Event anonymization |
| **PCI DSS** | Secure transmission | TLS 1.3 + encryption |
| **SOC 2** | Audit trails | Immutable event log |
| **Financial** | 7-year retention | Archive with legal hold |

### 12.2 Data Residency

- **US Data**: us-east-1 primary, us-west-2 backup
- **EU Data**: eu-central-1 with GDPR compliance
- **Cross-border**: Explicit consent required

---

## 13. Service-Specific Requirements

### 13.1 Critical Service Matrix

| Service | Event Sources | Consumers | Special Requirements |
|---------|--------------|-----------|---------------------|
| **QuantumLedger.Hub** | All financial | Blockchain, Audit | FIFO, Immutable |
| **InfrastructureService** | 6 networks | All services | Multi-network coordination |
| **SignatureService** | N/A | All (validation) | Sub-5ms validation |
| **PaymentGatewayService** | Payment events | 8 services | Saga orchestration |
| **OrchestrationService** | Workflow events | All services | State machine |

---

## 14. Implementation Checklist

### Phase 1 Deliverables

- [ ] CloudFormation template for EventBridge infrastructure
- [ ] Shared.Eventing library with PQC support
- [ ] SignatureService JWKS-PQC endpoint
- [ ] SQS consumer framework
- [ ] Event journal implementation
- [ ] SSE endpoints in gateways
- [ ] Monitoring dashboards
- [ ] Runbook documentation
- [ ] Integration tests
- [ ] Load test scenarios

### Success Criteria

- [ ] 100% PQC signature coverage
- [ ] Zero data loss during migration
- [ ] Meeting all latency targets
- [ ] Cost reduction achieved
- [ ] All compliance requirements met

---

## 15. References

### Pattern Library
- **Saga Orchestration**: Distributed transaction management
- **Circuit Breaker**: Resilience pattern for external services
- **Event Sourcing**: Append-only event store
- **CQRS**: Command Query Responsibility Segregation
- **Multi-Network Coordinator**: Blockchain broadcasting pattern

### Implementation Guides
- Service update guide: `actual_EventBridge_Service_Update_Guide.md`
- Deployment strategy: `actual_EventBridge_Deployment_MVP_Strategy.md`
- CloudFormation templates: `infra/aws/eventbridge/`
- Shared library: `src/QuantumSkyLink.Shared.Eventing/`

### Planning Documents
- Cost analysis: `planning_EventBridge_Cost_Analysis_Report.md`
- Original plan: `planning_EventBridge_Implementation_Plan.md`
- Detailed technical plan: `planning_EventBridge_Migration_Detailed_Implementation_Plan.md`
- Mobile strategy: `planning_EventBridge_Mobile_Integration_Strategy.md`

### External Documentation
- [AWS EventBridge Best Practices](https://docs.aws.amazon.com/eventbridge/)
- [Dilithium2 Specification](https://pq-crystals.org/dilithium/)
- [Event-Driven Architecture Patterns](https://martinfowler.com/articles/201701-event-driven.html)

---

**Document Status**: Ready for implementation  
**Review Required By**: Architecture Team, Security Team, DevOps Team  
**Implementation Start**: Upon approval
