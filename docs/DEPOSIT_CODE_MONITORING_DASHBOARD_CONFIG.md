# Deposit Code Monitoring Dashboard Configuration

## Overview
This document describes the comprehensive monitoring infrastructure for deposit code validation and security events within the QuantumSkyLink v2 platform.

## Architecture

### Core Components

1. **DepositCodeMonitoringService** - Central monitoring service with comprehensive telemetry
2. **DepositCodeAlertingService** - Background service for continuous monitoring and alerting
3. **MonitoringController** - API endpoints for admin dashboard consumption
4. **AdminAPIGateway Integration** - Proxy endpoints for secure admin access

### Metrics Collection

#### OpenTelemetry Meters
- `QuantumSkyLink.DepositCode` - Deposit code operations
- `QuantumSkyLink.PaymentValidation` - Validation metrics
- `QuantumSkyLink.SecurityEvents` - Security incident tracking
- `QuantumSkyLink.Performance` - Operation performance metrics
- `QuantumSkyLink.Compliance` - Audit and compliance events

#### Key Metrics

**Deposit Code Operations:**
- `deposit_code_validations_total` - Total validation attempts
- `deposit_code_validations_success_total` - Successful validations
- `deposit_code_validations_failed_total` - Failed validations
- `deposit_code_generations_total` - Code generation attempts
- `deposit_code_usage_total` - Usage events
- `active_deposit_codes` - Currently active codes (gauge)
- `expired_deposit_codes` - Expired codes (gauge)

**Performance Metrics:**
- `deposit_code_validation_duration_seconds` - Validation operation times
- `deposit_code_generation_duration_seconds` - Generation operation times
- `rejection_processing_duration_seconds` - Rejection processing times
- `validation_success_rate_percent` - Success rate gauge

**Security Metrics:**
- `security_events_total` - Total security events
- `suspicious_activity_detected_total` - Suspicious activity events
- `duplicate_attempts_total` - Duplicate attempt events
- `unauthorized_access_total` - Unauthorized access attempts

**Compliance Metrics:**
- `compliance_events_total` - Total compliance events
- `audit_trail_events_total` - Audit trail entries
- `regulatory_reports_total` - Regulatory reports generated

## Dashboard Endpoints

### Admin API Gateway Endpoints

All endpoints require authentication and are accessible via the AdminAPIGateway:

- `GET /api/admin/monitoring/deposit-codes/metrics` - Deposit code metrics
- `GET /api/admin/monitoring/deposit-codes/security` - Security metrics
- `GET /api/admin/monitoring/deposit-codes/performance` - Performance metrics
- `GET /api/admin/monitoring/deposit-codes/dashboard` - Combined dashboard data
- `GET /api/admin/monitoring/deposit-codes/health` - Monitoring health status
- `POST /api/admin/monitoring/deposit-codes/alerts/check` - Manual alert trigger (Admin only)
- `GET /api/admin/monitoring/deposit-codes/config` - Configuration view (Admin only)

### PaymentGatewayService Direct Endpoints

Direct service endpoints (internal use):

- `GET /api/monitoring/deposit-codes` - Deposit code metrics
- `GET /api/monitoring/security` - Security metrics
- `GET /api/monitoring/performance` - Performance metrics
- `GET /api/monitoring/dashboard` - Combined dashboard data
- `GET /api/monitoring/health` - Health status
- `POST /api/monitoring/alerts/check` - Manual alert check

## Alerting Configuration

### Alert Thresholds

**Critical Alerts:**
- Validation success rate < 85%
- More than 20 validation failures per hour
- More than 10 security events per hour
- Average validation time > 1000ms
- Service uptime < 99%

**Warning Alerts:**
- Validation success rate < 95%
- More than 10 validation failures per hour
- More than 5 security events per hour
- Average validation time > 500ms
- Service uptime < 99.9%

### Alert Channels

Currently configured channels:
- **Log** - Structured logging with severity levels
- **Metrics** - OpenTelemetry metrics for Aspire dashboard
- **Future**: Slack, Email, PagerDuty integration ready

### Alert Check Frequency

- **Default**: Every 5 minutes
- **Configurable**: Via `DepositCodeMonitoring:AlertCheckIntervalMinutes`

## Configuration

### appsettings.json Structure

```json
{
  "DepositCodeMonitoring": {
    "AlertCheckIntervalMinutes": 5,
    "Thresholds": {
      "ValidationSuccessRate": {
        "Warning": 95.0,
        "Critical": 85.0
      },
      "ValidationFailuresPerHour": {
        "Warning": 10,
        "Critical": 20
      },
      "SecurityEventsPerHour": {
        "Warning": 5,
        "Critical": 10
      },
      "AverageValidationTimeMs": {
        "Warning": 500,
        "Critical": 1000
      },
      "ServiceUptimePercent": {
        "Warning": 99.9,
        "Critical": 99.0
      }
    },
    "Alerting": {
      "Enabled": true,
      "Channels": ["log", "metrics"],
      "SlackWebhookUrl": "",
      "EmailRecipients": [],
      "PagerDutyIntegrationKey": ""
    },
    "MetricsRetention": {
      "DetailedMetricsDays": 7,
      "SummaryMetricsDays": 90,
      "AuditTrailDays": 365
    }
  }
}
```

## Security Features

### Security Event Detection

The monitoring system automatically detects and records:
- Invalid deposit code attempts
- Unauthorized access attempts
- Suspicious validation failure patterns
- Duplicate code usage attempts
- Rapid successive validation attempts

### Audit Trail

All monitoring events include:
- Correlation IDs for tracing
- User identification (when applicable)
- Timestamp and duration information
- Detailed context and metadata
- Security classifications

## Integration Points

### Aspire Dashboard

- All metrics are exported to OpenTelemetry
- Available in Aspire dashboard at `http://localhost:17140`
- Real-time visualization of all deposit code operations
- Health check integration for service status

### Service Integration

**PaymentValidationService:**
- Records validation success/failure with reasons
- Tracks performance metrics for all operations
- Detects and reports suspicious activity patterns

**PaymentProcessingService:**
- Records rejection processing metrics
- Tracks fee calculation and return processing
- Reports compliance events for audit trails

## Troubleshooting

### Common Issues

1. **Missing Metrics**
   - Verify OpenTelemetry configuration in ServiceDefaults
   - Check meter registration in service startup
   - Ensure monitoring service is registered as singleton

2. **Alert Not Triggering**
   - Verify DepositCodeAlertingService is running
   - Check threshold configuration
   - Review service logs for errors

3. **Dashboard Endpoint Issues**
   - Verify AdminAPIGateway HTTP client configuration
   - Check PaymentGatewayService health status
   - Ensure proper authentication/authorization

### Monitoring Health Check

Use the health endpoint to verify monitoring system status:
```bash
curl http://localhost:5000/api/admin/monitoring/deposit-codes/health
```

## Future Enhancements

### Planned Features

1. **Advanced Analytics**
   - Machine learning-based anomaly detection
   - Predictive failure analysis
   - Usage pattern analysis

2. **Enhanced Alerting**
   - Smart alert throttling
   - Escalation policies
   - Integration with incident management systems

3. **Extended Metrics**
   - Geographic distribution analysis
   - Payment method correlation
   - Gateway performance comparison

4. **Compliance Reporting**
   - Automated regulatory reports
   - Audit trail export capabilities
   - Data retention policy enforcement

## Maintenance

### Regular Tasks

1. **Weekly**: Review alert thresholds and adjust based on patterns
2. **Monthly**: Analyze metrics trends and optimize performance
3. **Quarterly**: Review security event patterns and update detection rules
4. **Annually**: Update compliance requirements and audit procedures

### Backup and Recovery

- Metrics data retention policy configured
- OpenTelemetry data exported to persistent storage
- Configuration backup recommended for disaster recovery