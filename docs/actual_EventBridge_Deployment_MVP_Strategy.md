# EventBridge Deployment & MVP Strategy
Version: 1.0  
Date: 2025-09-03  
Purpose: Deployment strategy and MVP testing stages for EventBridge migration

---

## Executive Summary

This document defines the deployment strategy and MVP testing stages for the EventBridge migration, enabling incremental deployment without disrupting the existing system. The approach uses parallel run with progressive cutover, allowing real-world testing at each stage.

**Key Strategy**: Deploy infrastructure first, then migrate services in phases with feature flags and dual publishing for safety.

---

## Deployment Approach: Parallel Run with Progressive Cutover

### Core Principles
- **Zero downtime** during migration
- **Incremental deployment** with rollback capability
- **Shadow mode** testing before cutover
- **Feature flags** for service-level control
- **Dual publishing** during transition

---

## Deployment Stages & MVP Milestones

### Stage 1: Infrastructure Only (Week 1)

| Component | Action | Risk | Testing |
|-----------|--------|------|---------|
| EventBridge Buses | Deploy | None | Infrastructure validation |
| SQS Queues | Create | None | Queue connectivity |
| CloudFormation | Apply | None | Resource verification |
| IAM Roles | Configure | None | Permission testing |

**No services migrated - Zero user impact**

---

### Stage 2: MVP Core Services (Weeks 2-3) 🎯

**Services to Deploy**:

| Service | Current State | Migration Complexity | Feature Flag |
|---------|--------------|---------------------|--------------|
| OrchestrationService | Already AWS | Low | `EventBridge_Orchestration` |
| PaymentGatewayService | RabbitMQ | Medium | `EventBridge_Payment` |
| NotificationService | RabbitMQ/SignalR | Medium | `EventBridge_Notification` |
| UserService | Direct DB | Low | `EventBridge_User` |

**MVP 1 Features Available**:
- ✅ User registration and authentication
- ✅ Basic payment flows
- ✅ Email/SMS notifications
- ✅ Workflow orchestration

**Testing Strategy**: 
```yaml
Mode: Shadow Publishing
Traffic Split: 100% RabbitMQ, 10% EventBridge (validation only)
Rollback Time: 5 minutes
```

---

### Stage 3: Financial Services (Weeks 4-5) 💰

**Services to Deploy**:

| Service | Queue Type | Priority | Dependencies |
|---------|------------|----------|--------------|
| QuantumLedger.Hub | FIFO | Critical | SignatureService |
| TreasuryService | Standard | High | QuantumLedger.Hub |
| FeeService | Standard | Medium | PaymentGateway |
| AccountService | Standard | High | QuantumLedger.Hub |

**MVP 2 Features Available**:
- ✅ Complete payment processing
- ✅ Ledger validation
- ✅ Fee calculations
- ✅ Account management
- ✅ Treasury operations

**Validation Requirements**:
- Parallel validation comparing EventBridge vs RabbitMQ results
- Transaction reconciliation every hour
- Automated rollback on >1% divergence

---

### Stage 4: Blockchain Services (Week 6) ⛓️

**Services to Deploy**:

| Service | Networks | Event Types | Special Requirements |
|---------|----------|-------------|---------------------|
| InfrastructureService | 6 | Broadcast, Confirmation | EventJournaler to SurrealDB |
| TokenService | 6 | Mint, Transfer, Burn | Multi-network coordination |
| SignatureService | N/A | Validation, Rotation | PQC Dilithium2 |

**MVP 3 Features Available**:
- ✅ Multi-network blockchain transactions
- ✅ Token operations across 6 networks
- ✅ Cryptographic validation
- ✅ Event journal queries
- ✅ Blockchain confirmation tracking

---

### Stage 5: Business Services (Weeks 7-8) 📊

**Services to Deploy**:

| Service | Priority | Risk | Rollback Strategy |
|---------|----------|------|-------------------|
| MarketplaceService | High | Medium | Feature flag + cache |
| ComplianceService | Critical | Low | Dual validation |
| AIReviewService | Low | Low | Async fallback |
| GovernanceService | Medium | Low | Database fallback |

**MVP 4 Features Available**:
- ✅ Complete marketplace operations
- ✅ Compliance checks
- ✅ AI-powered reviews
- ✅ Governance features

---

### Stage 6: API Gateways (Week 9) 🌐

**Gateway Migration**:

| Gateway | SignalR Removal | SSE Addition | Client Impact |
|---------|----------------|--------------|---------------|
| MobileAPIGateway | Complete | Full SSE | App update required |
| WebAPIGateway | Complete | Full SSE | Auto-reconnect |
| AdminAPIGateway | Complete | Full SSE | Dashboard update |

**Full Platform Features**:
- ✅ Real-time updates via SSE
- ✅ Mobile app full functionality
- ✅ Admin dashboard
- ✅ Event status queries

---

## MVP Testing Milestones

### MVP 1: Basic Operations (Week 3)

**Minimum Required Services**:
```yaml
Services:
  - OrchestrationService: eventbridge-v1
  - UserService: eventbridge-v1
  - NotificationService: eventbridge-v1
  - MobileAPIGateway: partial (no SSE)
```

**Test Scenarios**:
1. **User Journey**: Signup → Login → Profile Update → Notification
2. **Workflow Test**: Create → Execute → Complete workflow
3. **Load Test**: 100 concurrent users, 1000 requests/minute

**Success Criteria**:
- Error rate < 0.1%
- P95 latency < 200ms
- All notifications delivered

---

### MVP 2: Payment Processing (Week 5)

**Additional Services**:
```yaml
Services:
  - PaymentGatewayService: eventbridge-v1
  - QuantumLedger.Hub: eventbridge-v1
  - TreasuryService: eventbridge-v1
  - FeeService: eventbridge-v1
```

**Test Scenarios**:
1. **Payment Flow**: Initiate → Validate → Settle → Confirm
2. **Ledger Validation**: 1000 transactions with reconciliation
3. **Saga Test**: Payment with forced failure and compensation

**Success Criteria**:
- 100% ledger consistency
- Saga compensation 100% successful
- Payment processing < 300ms

---

### MVP 3: Blockchain Integration (Week 6)

**Additional Services**:
```yaml
Services:
  - InfrastructureService: eventbridge-v1
  - SignatureService: eventbridge-v1
  - TokenService: eventbridge-v1
```

**Test Scenarios**:
1. **Multi-Network Broadcast**: Transaction to all 6 networks
2. **Token Operations**: Mint → Transfer → Burn cycle
3. **Signature Validation**: 10,000 PQC validations

**Success Criteria**:
- 5/6 networks minimum success
- PQC validation < 5ms
- Event journal 100% capture

---

### MVP 4: Full Platform (Week 9)

**All Services Migrated**

**Test Scenarios**:
1. **End-to-End User Journey**: Complete user lifecycle
2. **Marketplace Operations**: List → Buy → Settle → Deliver
3. **Compliance Flow**: KYC → AML → Risk Assessment
4. **Real-time Updates**: SSE stream stability test

**Success Criteria**:
- All features operational
- SSE connections stable for 24 hours
- Cost reduction targets met

---

## Deployment Commands & Configuration

### Infrastructure Deployment
```bash
# Deploy EventBridge infrastructure
aws cloudformation deploy \
  --template-file infra/aws/qsl-eventbridge.template.json \
  --stack-name qsl-eventbridge-mvp \
  --parameter-overrides Environment=mvp BusName=qsl-core

# Verify deployment
aws events list-event-buses --name-prefix qsl
aws sqs list-queues --queue-name-prefix qsl
```

### Service Deployment (Kubernetes)
```bash
# Stage 2: Deploy core services
kubectl apply -f k8s/eventbridge/stage2-core-services.yaml
kubectl set env deployment/orchestration USE_EVENTBRIDGE=true
kubectl set env deployment/user USE_EVENTBRIDGE=shadow
kubectl rollout status deployment/orchestration

# Stage 3: Deploy financial services
kubectl apply -f k8s/eventbridge/stage3-financial-services.yaml
kubectl set env deployment/quantumledger USE_EVENTBRIDGE=true
kubectl rollout status deployment/quantumledger
```

### Feature Flag Configuration
```json
{
  "FeatureManagement": {
    "EventBridge": {
      "OrchestrationService": true,
      "PaymentGatewayService": "shadow",
      "QuantumLedger.Hub": false,
      "UserService": true,
      "NotificationService": "parallel",
      "InfrastructureService": false,
      "TokenService": false,
      "SignatureService": false,
      "MarketplaceService": false,
      "ComplianceService": false,
      "MobileAPIGateway": false,
      "WebAPIGateway": false,
      "AdminAPIGateway": false
    },
    "DualPublishing": {
      "Enabled": true,
      "ShadowPercentage": 10
    }
  }
}
```

---

## Safety Mechanisms

### Canary Deployment Configuration
```yaml
apiVersion: flagger.app/v1beta1
kind: Canary
metadata:
  name: eventbridge-migration
spec:
  targetRef:
    apiVersion: apps/v1
    kind: Deployment
    name: paymentgateway
  progressDeadlineSeconds: 600
  service:
    port: 80
  analysis:
    interval: 1m
    threshold: 5
    maxWeight: 100
    stepWeight: 10
    metrics:
    - name: error-rate
      thresholdRange:
        max: 1
    - name: latency
      thresholdRange:
        max: 500
```

### Rollback Triggers
| Metric | Threshold | Action | Recovery Time |
|--------|-----------|--------|---------------|
| Error Rate | >5% for 5min | Auto-rollback | 5 minutes |
| P95 Latency | >500ms for 10min | Alert + Manual | 15 minutes |
| DLQ Depth | >1000 messages | Pause deployment | 30 minutes |
| Delivery Failure | >1% for 5min | Auto-rollback | 5 minutes |

### Emergency Rollback Script
```bash
#!/bin/bash
# emergency-rollback.sh

STAGE=$1
case $STAGE in
  1)
    echo "Rolling back Stage 1 services..."
    kubectl set env deployment/orchestration USE_EVENTBRIDGE=false
    kubectl set env deployment/user USE_EVENTBRIDGE=false
    ;;
  2)
    echo "Rolling back Stage 2 services..."
    kubectl set env deployment/paymentgateway USE_EVENTBRIDGE=false
    kubectl set env deployment/quantumledger USE_EVENTBRIDGE=false
    ;;
  3)
    echo "Rolling back Stage 3 services..."
    kubectl set env deployment/infrastructure USE_EVENTBRIDGE=false
    kubectl set env deployment/token USE_EVENTBRIDGE=false
    ;;
  *)
    echo "Rolling back all services..."
    kubectl set env deployment --all USE_EVENTBRIDGE=false
    ;;
esac

kubectl rollout restart deployment --all
echo "Rollback complete. Services reverting to RabbitMQ."
```

---

## Monitoring & Validation

### Key Metrics Dashboard
```yaml
Dashboards:
  - EventBridge Health:
      - PutEvents success rate
      - Rule invocation rate
      - Failed event count
  
  - SQS Performance:
      - Queue depth by service
      - Message age
      - DLQ message count
  
  - Service Metrics:
      - Event processing latency
      - Signature validation time
      - Saga completion rate
  
  - Cost Tracking:
      - EventBridge usage cost
      - SQS message cost
      - Total vs projected savings
```

### Validation Checkpoints

**Daily Validation**:
- [ ] Error rates within threshold
- [ ] No messages in DLQ
- [ ] Event journal capturing all events
- [ ] Cost tracking on target

**Weekly Validation**:
- [ ] End-to-end test suite passing
- [ ] Performance benchmarks met
- [ ] Security scan clean
- [ ] Rollback procedures tested

---

## Deployment Decision Matrix

| Stage | Services | Risk | Rollback Time | Decision Criteria |
|-------|----------|------|---------------|-------------------|
| MVP 1 | 4 | Low | 5 min | Error rate <1%, Latency <200ms |
| MVP 2 | 8 | Medium | 15 min | Ledger consistency 100%, Saga success 100% |
| MVP 3 | 11 | Medium | 30 min | Blockchain success >80%, PQC <5ms |
| MVP 4 | 24 | High | 1 hour | All features working, Cost targets met |

---

## Go/No-Go Criteria

### MVP 1 Launch (Week 3)
**GO if**:
- ✅ All 4 services deployed successfully
- ✅ Shadow mode validation shows <0.1% divergence
- ✅ Load test passes with target metrics
- ✅ Rollback tested successfully

**NO-GO if**:
- ❌ Any service fails to start
- ❌ Error rate >1%
- ❌ Rollback fails in testing

### MVP 2 Launch (Week 5)
**GO if**:
- ✅ Financial services validated
- ✅ Ledger consistency verified
- ✅ Saga compensation working
- ✅ Parallel validation shows match

**NO-GO if**:
- ❌ Any financial discrepancy
- ❌ Saga compensation fails
- ❌ Performance degradation >20%

---

## Support & Escalation

### Support Matrix
| Issue Type | First Response | Escalation | Resolution Target |
|------------|---------------|------------|-------------------|
| Service Down | DevOps | Platform Team | 15 minutes |
| Performance | Platform Team | Architecture | 1 hour |
| Data Loss | Platform Team | CTO | Immediate |
| Security | Security Team | CISO | Immediate |

### Key Contacts
- **Platform Lead**: For architecture decisions
- **DevOps Lead**: For deployment issues
- **Security Lead**: For PQC and compliance
- **Product Owner**: For feature decisions

---

## Related Documentation

### Planning Documents
- `planning_EventBridge_Cost_Analysis_Report.md` - Cost analysis and projections
- `planning_EventBridge_Implementation_Plan.md` - Original implementation plan
- `planning_EventBridge_Migration_Detailed_Implementation_Plan.md` - Detailed technical plan
- `planning_EventBridge_Mobile_Integration_Strategy.md` - Mobile-specific strategy

### Implementation Documents
- `actual_EventBridge_Migration_Requirements.md` - Concrete requirements
- `actual_EventBridge_Service_Update_Guide.md` - Service-by-service guide

### Infrastructure
- `infra/aws/qsl-eventbridge.template.json` - CloudFormation template
- `k8s/eventbridge/` - Kubernetes manifests

---

**Document Status**: Ready for deployment execution  
**Last Updated**: 2025-09-03  
**Version**: 1.0  
**Next Review**: After MVP 1 deployment
