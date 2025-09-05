#!/bin/bash

# QuantumSkyLink v2 - Complete LocalStack Testing Script
# Starts LocalStack, deploys infrastructure, and runs the application

set -e

echo "================================================"
echo "QuantumSkyLink v2 - LocalStack Testing"
echo "================================================"

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Configuration
SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"
PROJECT_ROOT="$(dirname "$SCRIPT_DIR")"

# Step 1: Start LocalStack
start_localstack() {
    echo -e "${BLUE}Step 1: Starting LocalStack...${NC}"
    
    cd "$PROJECT_ROOT"
    
    # Check if LocalStack is already running
    if docker ps | grep -q qsl-localstack; then
        echo -e "${YELLOW}LocalStack is already running${NC}"
    else
        echo "Starting LocalStack containers..."
        docker-compose -f docker-compose.localstack.yml up -d
        
        # Wait for LocalStack to be healthy
        echo "Waiting for LocalStack to be ready..."
        for i in {1..30}; do
            if curl -s http://localhost:4566/_localstack/health | grep -q "\"services\""; then
                echo -e "${GREEN}✓ LocalStack is ready${NC}"
                break
            fi
            echo -n "."
            sleep 2
        done
    fi
}

# Step 2: Deploy infrastructure
deploy_infrastructure() {
    echo -e "${BLUE}Step 2: Deploying EventBridge infrastructure...${NC}"
    
    cd "$PROJECT_ROOT"
    
    # Make deploy script executable
    chmod +x scripts/deploy-localstack.sh
    
    # Run deployment
    ./scripts/deploy-localstack.sh
}

# Step 3: Run smoke tests
run_smoke_tests() {
    echo -e "${BLUE}Step 3: Running smoke tests...${NC}"
    
    # Test EventBridge
    echo "Testing EventBridge..."
    aws events put-events \
        --entries '[{"Source":"qsl.test","DetailType":"Smoke.Test","Detail":"{\"test\":true}"}]' \
        --endpoint-url http://localhost:4566 \
        --region us-east-1
    
    # Test SQS
    echo "Testing SQS..."
    QUEUE_URL=$(aws sqs list-queues \
        --endpoint-url http://localhost:4566 \
        --region us-east-1 \
        --queue-name-prefix "qsl-development" \
        --query 'QueueUrls[0]' \
        --output text)
    
    if [ ! -z "$QUEUE_URL" ]; then
        aws sqs send-message \
            --queue-url "$QUEUE_URL" \
            --message-body '{"test": "LocalStack SQS test"}' \
            --endpoint-url http://localhost:4566 \
            --region us-east-1
        echo -e "${GREEN}✓ SQS test successful${NC}"
    fi
}

# Step 4: Start the application
start_application() {
    echo -e "${BLUE}Step 4: Starting QuantumSkyLink application...${NC}"
    
    cd "$PROJECT_ROOT"
    
    # Check if .NET 9 is installed
    if ! dotnet --version | grep -q "9."; then
        echo -e "${RED}✗ .NET 9 is not installed${NC}"
        echo "Please install .NET 9 SDK from https://dotnet.microsoft.com/download"
        exit 1
    fi
    
    # Set environment to use LocalStack configuration
    export ASPNETCORE_ENVIRONMENT=LocalStack
    export DOTNET_ENVIRONMENT=LocalStack
    
    echo "Building the application..."
    dotnet build QuantunSkyLink_v2.AppHost/QuantunSkyLink_v2.AppHost.csproj
    
    echo -e "${GREEN}Starting application with LocalStack configuration...${NC}"
    echo ""
    echo "The application will start with:"
    echo "- EventBridge messaging via LocalStack"
    echo "- Local PostgreSQL databases"
    echo "- Redis cache"
    echo ""
    echo -e "${YELLOW}Press Ctrl+C to stop the application${NC}"
    
    # Run the application
    dotnet run --project QuantunSkyLink_v2.AppHost/QuantunSkyLink_v2.AppHost.csproj
}

# Step 5: Cleanup (optional)
cleanup() {
    echo -e "${BLUE}Cleaning up...${NC}"
    
    cd "$PROJECT_ROOT"
    
    # Stop LocalStack
    docker-compose -f docker-compose.localstack.yml down
    
    echo -e "${GREEN}✓ Cleanup complete${NC}"
}

# Trap Ctrl+C to cleanup
trap cleanup EXIT

# Main execution
main() {
    echo "Starting LocalStack testing environment..."
    echo ""
    
    # Export AWS credentials for LocalStack
    export AWS_ACCESS_KEY_ID=test
    export AWS_SECRET_ACCESS_KEY=test
    export AWS_DEFAULT_REGION=us-east-1
    
    # Execute steps
    start_localstack
    sleep 5  # Give LocalStack time to fully initialize
    deploy_infrastructure
    run_smoke_tests
    
    echo ""
    echo -e "${GREEN}================================================${NC}"
    echo -e "${GREEN}LocalStack environment is ready!${NC}"
    echo -e "${GREEN}================================================${NC}"
    echo ""
    
    # Start the application
    start_application
}

# Run main function
main "$@"