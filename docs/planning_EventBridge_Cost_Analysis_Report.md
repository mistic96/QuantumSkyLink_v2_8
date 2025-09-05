# 📊 EventBridge Cost Analysis Report

> **Date**: 2025-09-03  
> **Prepared for**: QuantumSkyLink v2 Architecture Review  
> **Document Type**: Financial Impact Analysis  
> **Classification**: Strategic Planning

---

## 🎯 Executive Summary

This comprehensive report analyzes the financial impact of migrating QuantumSkyLink v2's messaging infrastructure from the current RabbitMQ/SNS/SQS hybrid architecture to AWS EventBridge. The analysis includes detailed cost projections, operational savings, and ROI calculations based on current MVP deployment patterns.

### 📈 Key Financial Findings

| **Metric** | **Current State** | **Post-Migration** | **Improvement** |
|------------|-------------------|-------------------|-----------------|
| **Monthly Messaging Costs** | $50-80 | $3-6 | **94-96% reduction** |
| **Total Monthly Infrastructure** | $90-160 | $55-95 | **38-41% reduction** |
| **Annual Savings (MVP)** | - | $420-780 | **Net positive ROI** |
| **Scalability Factor** | Limited | 10x+ capacity | **Unlimited growth** |

### 💡 Strategic Benefits Summary

- ✅ **Massive Cost Reduction**: 94-96% savings on messaging infrastructure
- ✅ **Operational Simplification**: Eliminate broker management overhead
- ✅ **Enhanced Scalability**: Pay-per-event model with unlimited capacity
- ✅ **Improved Reliability**: Built-in high availability and disaster recovery
- ✅ **Developer Productivity**: Reduced complexity and faster feature development

---

## Assumptions and Baseline

Source: `AWS_DEPLOYMENT_IMPLEMENTATION_PLAN.md` and repository service configuration.

Baseline (MVP):
- Target monthly cost (current plan): USD 90–160 (includes ECS Fargate, ALB, Neon DB, Amazon MQ, CloudWatch, NAT egress estimate, minimal infra).
- Messaging today: Amazon MQ (RabbitMQ) as primary broker + SNS/SQS hybrid for serverless patterns.
- Neon PostgreSQL remains external (no change).
- Aspire-managed deployment model (ECS Fargate + CloudFormation templates).
- Minimal HA for MVP (single broker optionally multi-AZ for production).

EventBridge Pricing (public AWS list approximations used for modeling):
- Custom events: $1.00 per 1,000,000 events
- Schema Registry: $0.10 per schema version per month (small contributor)
- Replays/archives: additional fees only when used; not assumed in steady-state

Operational assumptions:
- MVP expected event volume scenarios considered below
- EventBridge will replace core messaging where appropriate (not necessarily all synchronous HTTP calls)
- Retain RabbitMQ/Amazon MQ where necessary for legacy patterns (hybrid approach)

---

## Cost Comparison — Scenario Modelling

We model three event-volume scenarios: Low, Medium, High. We include a conservative Amazon MQ baseline.

1) Amazon MQ baseline (monthly)
- Minimal single-broker: $25–40
- Multi-AZ / HA: additional $25–40
- Typical configuration for MVP conservatively estimated: $50–80 / month

2) EventBridge projection (monthly)
- Low: 100,000 events → 0.1 * $1.00 = $0.10
- Medium: 1,000,000 events → $1.00
- High: 10,000,000 events → $10.00
- Schema registry overhead: $2–5 total
- Total EventBridge (Medium): ~$3–6 / month (includes small extra items)

Table: Messaging monthly cost comparison (approximate)

- Amazon MQ (current): $50–80
- EventBridge (Low): ~$1–3
- EventBridge (Medium): ~$3–6
- EventBridge (High): ~$12–20

Estimated monthly savings (messaging only, Medium case):
- Messaging saving = $50–80 − $3–6 = $47–75 (≈ 94–96% reduction in messaging cost)

---

## Impact on Total MVP Cost

Baseline total MVP monthly cost: $90–160 (includes messaging, ECS, ALB, NAT, CloudWatch, Neon DB, minimal other infra).

Projected total after partial EventBridge migration:
- Messaging reduced (e.g., −$47–75)
- Some operational overhead shift to serverless (CloudWatch, increased Lambda invocations) — small incremental cost
- Estimated new total: $55–95 / month

Relative reduction: ~38–41% of total monthly spend (MVP-level).

Annual savings (MVP):
- Lower bound: ($90 − $55) * 12 = $420
- Upper bound: ($160 − $95) * 12 = $780

---

## Operational and Hidden Cost Savings

1. Reduced Ops & Maintenance
- No broker patching or broker instance lifecycle management.
- Lower time required for scaling/incident handling.
- Reduced risk of broker misconfiguration and throughput shortages.

2. Observability and Incident Triage
- EventBridge integrates with CloudWatch Logs, Metrics, and X-Ray more directly than self-managed brokers.
- Event replay capabilities reduce need for elaborate backup/recovery tooling.

3. Developer Productivity
- Less boilerplate for queue management and retry logic.
- Faster onboarding of new consumers via EventBridge rules (no queue provisioning UX required).
- Decreased testing surface for broker edge-cases.

4. Cost Predictability
- EventBridge is pay-per-event; cost growth is linear with usage and easier to forecast.
- Eliminates small-but-recurring instance-based charges for low-traffic periods.

---

## Performance & Scale Considerations that Affect Cost

- EventBridge scales transparently; for high spikes (e.g., Black Friday) you won't pay for idle capacity (unlike pre-provisioned broker sizing).
- At very large volumes (100s of millions events/month), EventBridge costs scale linearly; cost planning required. For MVP and typical expected growth, EventBridge remains the lower-cost option.
- If some services require low-latency intra-cluster communication (sub-ms), consider retaining direct synchronous paths or a hybrid broker for those specific patterns — this adds minimal cost but may be justified for very latency-sensitive paths.

---

## Sensitivity Analysis

- If the system produces 10M+ events/month and heavy replay/archive usage is frequent, EventBridge costs grow but still often remain competitive vs. multi-broker HA, maintenance, and additional standby capacity.
- Conversely, if event volume is extremely low (<100k/month) the savings are dramatic because EventBridge cost approaches zero.

---

## Risks and Cost Mitigations

1. Vendor Lock-in
- Migration to EventBridge increases platform dependence on AWS.
- Mitigation: keep high-level event schemas and translation layer; avoid packing business logic into EventBridge rules where possible.

2. Unexpected Event Volumes
- Sudden high-volume bursts will increase EventBridge charges.
- Mitigation: budget alerts, rate-limiting producers (throttle spikes), apply batching at producers.

3. Hidden Lambda & Downstream Costs
- EventBridge often triggers Lambda or Step Functions; compute costs must be accounted for.
- Mitigation: cost modeling of expected Lambda execution; use efficient handlers, provisioned concurrency only where necessary.

---

## Recommendations & Next Steps

1. Adopt a hybrid migration approach:
   - Migrate orchestration and high-fanout patterns first (OrchestrationService, PaymentGatewayService, NotificationService).
   - Keep synchronous, low-latency APIs as direct HTTP calls.

2. Implement monitoring and budget alerts:
   - CloudWatch metrics for EventBridge PutEvents, aggregate monthly cost alarms.
   - Alert on unexpected growth in event volume.

3. Define and version event schemas:
   - Use EventBridge Schema Registry for type safety and to estimate schema costs.

4. Run a 30-day pilot:
   - Route a subset of events to EventBridge for a single high-impact workflow (e.g., order lifecycle).
   - Measure real costs and latency under production-like load.

5. Update the financial model with real telemetry after pilot:
   - Recalculate ROI and adjust migration cadence accordingly.

---

## Appendix — Quick Cost Examples

- Example: 1M events/month
  - EventBridge: $1.00
  - Schema registry: $2.00
  - Total messaging cost: ~$3.00
  - Compared to Amazon MQ (~$60): Savings ≈ $57/month

- Example: 5M events/month
  - EventBridge: $5.00
  - Schema registry: $2.00
  - Total: ~$7.00
  - Compared to Amazon MQ (~$60): Savings ≈ $53/month

---

Prepared by: Cline (Architecture & Cost Analysis)  
Location: QuantumSkyLink v2 repository — docs folder
