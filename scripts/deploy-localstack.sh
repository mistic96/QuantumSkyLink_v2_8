#!/bin/bash

# QuantumSkyLink v2 - LocalStack Deployment Script
# Deploys EventBridge infrastructure to LocalStack for local testing

set -e

echo "================================================"
echo "QuantumSkyLink v2 - LocalStack Deployment"
echo "================================================"

# Configuration
LOCALSTACK_URL=${LOCALSTACK_URL:-http://localhost:4566}
AWS_REGION=${AWS_REGION:-us-east-1}
PROJECT_NAME=${PROJECT_NAME:-qsl}
ENVIRONMENT=${ENVIRONMENT:-development}

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Check if LocalStack is running
check_localstack() {
    echo -e "${YELLOW}Checking LocalStack status...${NC}"
    if curl -s "${LOCALSTACK_URL}/_localstack/health" | grep -q "\"services\""; then
        echo -e "${GREEN}✓ LocalStack is running${NC}"
        return 0
    else
        echo -e "${RED}✗ LocalStack is not running${NC}"
        echo "Please start LocalStack with: docker-compose -f docker-compose.localstack.yml up -d"
        exit 1
    fi
}

# Deploy CloudFormation stack
deploy_stack() {
    local stack_name=$1
    local template_file=$2
    local parameters=$3
    
    echo -e "${YELLOW}Deploying stack: ${stack_name}${NC}"
    
    aws cloudformation create-stack \
        --stack-name "${stack_name}" \
        --template-body "file://${template_file}" \
        --parameters ${parameters} \
        --endpoint-url "${LOCALSTACK_URL}" \
        --region "${AWS_REGION}" \
        2>/dev/null || \
    aws cloudformation update-stack \
        --stack-name "${stack_name}" \
        --template-body "file://${template_file}" \
        --parameters ${parameters} \
        --endpoint-url "${LOCALSTACK_URL}" \
        --region "${AWS_REGION}" \
        2>/dev/null || true
    
    # Wait for stack to complete
    echo "Waiting for stack deployment..."
    aws cloudformation wait stack-create-complete \
        --stack-name "${stack_name}" \
        --endpoint-url "${LOCALSTACK_URL}" \
        --region "${AWS_REGION}" \
        2>/dev/null || \
    aws cloudformation wait stack-update-complete \
        --stack-name "${stack_name}" \
        --endpoint-url "${LOCALSTACK_URL}" \
        --region "${AWS_REGION}" \
        2>/dev/null || true
    
    echo -e "${GREEN}✓ Stack ${stack_name} deployed successfully${NC}"
}

# Verify EventBridge resources
verify_resources() {
    echo -e "${YELLOW}Verifying EventBridge resources...${NC}"
    
    # Check event buses
    echo "Event Buses:"
    aws events list-event-buses \
        --endpoint-url "${LOCALSTACK_URL}" \
        --region "${AWS_REGION}" \
        --query 'EventBuses[?Name!=`default`].Name' \
        --output table
    
    # Check SQS queues
    echo -e "\nSQS Queues:"
    aws sqs list-queues \
        --endpoint-url "${LOCALSTACK_URL}" \
        --region "${AWS_REGION}" \
        --queue-name-prefix "${PROJECT_NAME}-${ENVIRONMENT}" \
        --query 'QueueUrls' \
        --output table
    
    # Check SNS topics
    echo -e "\nSNS Topics:"
    aws sns list-topics \
        --endpoint-url "${LOCALSTACK_URL}" \
        --region "${AWS_REGION}" \
        --query 'Topics[].TopicArn' \
        --output table
}

# Test event publishing
test_event() {
    echo -e "${YELLOW}Testing event publishing...${NC}"
    
    local event_bus="${PROJECT_NAME}-${ENVIRONMENT}-core"
    local test_event='{
        "Source": "qsl.test",
        "DetailType": "Test.Event",
        "Detail": "{\"message\": \"LocalStack test event\", \"timestamp\": \"'$(date -u +"%Y-%m-%dT%H:%M:%SZ")'\"}"
    }'
    
    aws events put-events \
        --entries "[${test_event//\"/\\\"}]" \
        --endpoint-url "${LOCALSTACK_URL}" \
        --region "${AWS_REGION}" \
        --output json
    
    echo -e "${GREEN}✓ Test event published successfully${NC}"
}

# Main execution
main() {
    echo "Starting LocalStack deployment..."
    echo "Environment: ${ENVIRONMENT}"
    echo "Project: ${PROJECT_NAME}"
    echo "Region: ${AWS_REGION}"
    echo ""
    
    # Check LocalStack
    check_localstack
    
    # Deploy EventBridge stack
    deploy_stack \
        "${PROJECT_NAME}-eventbridge-${ENVIRONMENT}" \
        "infra/aws/aws-eventbridge-messaging.template.json" \
        "ParameterKey=ProjectName,ParameterValue=${PROJECT_NAME} ParameterKey=Environment,ParameterValue=${ENVIRONMENT}"
    
    # Verify resources
    verify_resources
    
    # Test event publishing
    test_event
    
    echo ""
    echo -e "${GREEN}================================================${NC}"
    echo -e "${GREEN}LocalStack deployment completed successfully!${NC}"
    echo -e "${GREEN}================================================${NC}"
    echo ""
    echo "Next steps:"
    echo "1. Update appsettings.Development.json with LocalStack endpoints"
    echo "2. Run: dotnet run --project QuantunSkyLink_v2.AppHost"
    echo "3. Monitor LocalStack logs: docker-compose -f docker-compose.localstack.yml logs -f"
}

# Export AWS credentials for LocalStack
export AWS_ACCESS_KEY_ID=test
export AWS_SECRET_ACCESS_KEY=test
export AWS_DEFAULT_REGION=${AWS_REGION}

# Run main function
main "$@"