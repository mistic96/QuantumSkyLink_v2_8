# QuantumSkyLink v2 - LocalStack Deployment Script (Windows)
# Deploys EventBridge infrastructure to LocalStack for local testing

$ErrorActionPreference = "Stop"

Write-Host "================================================" -ForegroundColor Cyan
Write-Host "QuantumSkyLink v2 - LocalStack Deployment" -ForegroundColor Cyan
Write-Host "================================================" -ForegroundColor Cyan

# Configuration
$LOCALSTACK_URL = if ($env:LOCALSTACK_URL) { $env:LOCALSTACK_URL } else { "http://localhost:4566" }
$AWS_REGION = if ($env:AWS_REGION) { $env:AWS_REGION } else { "us-east-1" }
$PROJECT_NAME = if ($env:PROJECT_NAME) { $env:PROJECT_NAME } else { "qsl" }
$ENVIRONMENT = if ($env:ENVIRONMENT) { $env:ENVIRONMENT } else { "development" }

# Check if LocalStack is running
function Test-LocalStack {
    Write-Host "Checking LocalStack status..." -ForegroundColor Yellow
    try {
        $response = Invoke-RestMethod -Uri "$LOCALSTACK_URL/_localstack/health" -Method Get
        if ($response.services) {
            Write-Host "✓ LocalStack is running" -ForegroundColor Green
            return $true
        }
    }
    catch {
        Write-Host "✗ LocalStack is not running" -ForegroundColor Red
        Write-Host "Please start LocalStack with: docker-compose -f docker-compose.localstack.yml up -d"
        exit 1
    }
}

# Deploy CloudFormation stack
function Deploy-Stack {
    param(
        [string]$StackName,
        [string]$TemplateFile,
        [string]$Parameters
    )
    
    Write-Host "Deploying stack: $StackName" -ForegroundColor Yellow
    
    # Set AWS credentials for LocalStack
    $env:AWS_ACCESS_KEY_ID = "test"
    $env:AWS_SECRET_ACCESS_KEY = "test"
    $env:AWS_DEFAULT_REGION = $AWS_REGION
    
    # Try to create or update the stack
    $templateBody = Get-Content -Path $TemplateFile -Raw
    
    try {
        # Try to create stack
        aws cloudformation create-stack `
            --stack-name $StackName `
            --template-body file://$TemplateFile `
            --parameters $Parameters `
            --endpoint-url $LOCALSTACK_URL `
            --region $AWS_REGION `
            --output json 2>$null
            
        Write-Host "Waiting for stack creation..."
        aws cloudformation wait stack-create-complete `
            --stack-name $StackName `
            --endpoint-url $LOCALSTACK_URL `
            --region $AWS_REGION 2>$null
    }
    catch {
        # If create fails, try update
        try {
            aws cloudformation update-stack `
                --stack-name $StackName `
                --template-body file://$TemplateFile `
                --parameters $Parameters `
                --endpoint-url $LOCALSTACK_URL `
                --region $AWS_REGION `
                --output json 2>$null
                
            Write-Host "Waiting for stack update..."
            aws cloudformation wait stack-update-complete `
                --stack-name $StackName `
                --endpoint-url $LOCALSTACK_URL `
                --region $AWS_REGION 2>$null
        }
        catch {
            # Stack might already be up to date
            Write-Host "Stack is already up to date or an error occurred" -ForegroundColor Yellow
        }
    }
    
    Write-Host "✓ Stack $StackName deployed successfully" -ForegroundColor Green
}

# Verify EventBridge resources
function Test-Resources {
    Write-Host "Verifying EventBridge resources..." -ForegroundColor Yellow
    
    # Check event buses
    Write-Host "`nEvent Buses:"
    aws events list-event-buses `
        --endpoint-url $LOCALSTACK_URL `
        --region $AWS_REGION `
        --query "EventBuses[?Name!='default'].Name" `
        --output table
    
    # Check SQS queues
    Write-Host "`nSQS Queues:"
    aws sqs list-queues `
        --endpoint-url $LOCALSTACK_URL `
        --region $AWS_REGION `
        --queue-name-prefix "$PROJECT_NAME-$ENVIRONMENT" `
        --query "QueueUrls" `
        --output table
    
    # Check SNS topics
    Write-Host "`nSNS Topics:"
    aws sns list-topics `
        --endpoint-url $LOCALSTACK_URL `
        --region $AWS_REGION `
        --query "Topics[].TopicArn" `
        --output table
}

# Test event publishing
function Test-EventPublishing {
    Write-Host "Testing event publishing..." -ForegroundColor Yellow
    
    $eventBus = "$PROJECT_NAME-$ENVIRONMENT-core"
    $timestamp = Get-Date -Format "yyyy-MM-ddTHH:mm:ssZ"
    
    $testEvent = @{
        Source = "qsl.test"
        DetailType = "Test.Event"
        Detail = '{"message": "LocalStack test event", "timestamp": "' + $timestamp + '"}'
    } | ConvertTo-Json -Compress
    
    $entries = "[" + $testEvent + "]"
    
    aws events put-events `
        --entries $entries `
        --endpoint-url $LOCALSTACK_URL `
        --region $AWS_REGION `
        --output json
    
    Write-Host "✓ Test event published successfully" -ForegroundColor Green
}

# Main execution
function Main {
    Write-Host "Starting LocalStack deployment..."
    Write-Host "Environment: $ENVIRONMENT"
    Write-Host "Project: $PROJECT_NAME"
    Write-Host "Region: $AWS_REGION"
    Write-Host ""
    
    # Check LocalStack
    Test-LocalStack
    
    # Deploy EventBridge stack
    $parameters = "ParameterKey=ProjectName,ParameterValue=$PROJECT_NAME ParameterKey=Environment,ParameterValue=$ENVIRONMENT"
    Deploy-Stack `
        -StackName "$PROJECT_NAME-eventbridge-$ENVIRONMENT" `
        -TemplateFile "infra/aws/aws-eventbridge-messaging.template.json" `
        -Parameters $parameters
    
    # Verify resources
    Test-Resources
    
    # Test event publishing
    Test-EventPublishing
    
    Write-Host ""
    Write-Host "================================================" -ForegroundColor Green
    Write-Host "LocalStack deployment completed successfully!" -ForegroundColor Green
    Write-Host "================================================" -ForegroundColor Green
    Write-Host ""
    Write-Host "Next steps:"
    Write-Host "1. Update appsettings.Development.json with LocalStack endpoints"
    Write-Host "2. Run: dotnet run --project QuantunSkyLink_v2.AppHost"
    Write-Host "3. Monitor LocalStack logs: docker-compose -f docker-compose.localstack.yml logs -f"
}

# Run main function
Main