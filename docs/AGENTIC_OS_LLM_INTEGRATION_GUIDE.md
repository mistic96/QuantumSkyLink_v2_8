# Agentic-OS LLM Integration Guide

**For AI Assistants and Language Models**

## Overview

The Agentic-OS system provides a sophisticated operating system for AI agents with advanced collaboration features. This guide explains how LLMs can effectively integrate with and utilize the agentic-os system for enhanced project management, agent collaboration, and decision-making.

## System Architecture

### Core Components
- **Agent Discovery**: Find and connect with specialized agents
- **Pub/Sub Messaging**: Real-time communication between agents
- **Collaboration Sessions**: Structured multi-agent decision-making
- **Persistent Storage**: Long-term memory and context preservation
- **Built-in LLM**: Ollama integration for local AI processing
- **Live Documentation**: Dynamic system documentation
- **Real-time Events**: System event monitoring and coordination
- **System Management**: Health monitoring and backup systems

### System Status
```json
{
  "name": "Liquid Context - MCP Agentic OS",
  "version": "2.0.0",
  "authentication": "Disabled - No authentication required",
  "focus": "Multi-agent collaboration and decision-making"
}
```

### Available API Endpoints
```json
{
  "features": {
    "live_documentation": "/api/v1/docs/status",
    "agent_discovery": "/api/v1/agents/",
    "decision_collaboration": "/api/v1/collaborate/active",
    "pubsub_messaging": "/api/v1/pubsub/stats",
    "real_time_events": "/api/v1/events/status",
    "system_management": "/api/v1/system/status",
    "ollama_ai": "/api/v1/ollama/status",
    "persistent_storage": "/api/v1/system/backup/list",
    "built_in_llm": "/api/v1/ollama/models"
  }
}
```

### Documentation System
```json
{
  "documentation_system": "Live API Documentation",
  "status": "Active",
  "auto_update": true,
  "available_guides": [
    "/api/v1/docs/server/install",
    "/api/v1/docs/client/install", 
    "/api/v1/docs/api/live",
    "/api/v1/docs/collaboration",
    "/api/v1/docs/features"
  ]
}
```

## Agent Registration and Setup

### ⚠️ CRITICAL UPDATE: Registration Process Correction

**IMPORTANT**: The documented pub/sub registration method is currently non-functional. MCP tools return 404 errors for `agentic_os_pubsub_send`. 

### Working Registration Methods

#### Method 1: System Backup Registration (Recommended)
Use the backup system to establish agent presence:

```javascript
// Create a backup with agent profile information
await use_mcp_tool("agentic-os", "agentic_os_backup_create", {
  "name": "agent-registration-[agent-name]",
  "description": "Registration for [Agent Name] with capabilities: [list capabilities]",
  "include_data": true
});
```

#### Method 2: Task Tracker Integration (Fallback)
Register through the task tracker system:

```javascript
await use_mcp_tool("task-tracker", "save_session_context", {
  "project_id": "relevant_project_id",
  "current_focus": "Agent Registration - [Agent Name]",
  "notes": "AGENT PROFILE: [detailed capabilities and specializations]"
});
```

### Real API Endpoints (For Reference)
The actual agentic-os system provides these REST endpoints (not available via MCP):
- `POST /api/v1/agents/register` - Register new agent
- `GET /api/v1/agents/` - List all agents  
- `GET /api/v1/agents/me` - Get current agent info

### Active Pub/Sub Topics
Based on system statistics, these topics are currently active:
- `servicebay-coordination`: Main coordination channel
- `agent-status-updates`: Agent status notifications
- `task-assignments`: Task distribution and assignment
- `progress-reports`: Progress updates and status reports

### System Health Monitoring
```javascript
// Monitor system health
await use_mcp_tool("agentic-os", "agentic_os_events_status", {
  "limit": 10
});

// Check pub/sub system statistics
await use_mcp_tool("agentic-os", "agentic_os_pubsub_stats", {});
```

## Complete MCP Tool Reference

### 1. System Status Check
**Always start by checking system status**

```javascript
// Tool: agentic_os_system_status
{
  "arguments": {}
}
```

**Response provides:**
- System health and version
- Available features and endpoints
- Authentication status
- Core capabilities overview

### 2. Agent Discovery
**Find available specialized agents**

```javascript
// Tool: agentic_os_agent_discovery
{
  "arguments": {
    "filter": "blockchain" // Optional: filter by domain
  }
}
```

**Available Agent Types:**
- **Backend_API_Agent**: ASP.NET Core development
- **Frontend_React_Agent**: React application development
- **Database_Architecture_Agent**: Database design and optimization
- **Security_Compliance_Agent**: Security and compliance
- **QA_Testing_Agent**: Quality assurance and testing
- **DevOps_Infrastructure_Agent**: CI/CD and infrastructure
- **Documentation_Agent**: Technical documentation
- **UI_UX_Design_Agent**: User interface design

### 3. Collaboration Sessions
**Start or join collaborative decision-making**

```javascript
// Tool: agentic_os_collaborate
{
  "arguments": {
    "action": "start", // or "join", "list", "status"
    "topic": "QuantumSkyLink_v2_Blockchain_Infrastructure_Assessment",
    "session_id": "optional-for-join-actions"
  }
}
```

**Collaboration Actions:**
- `start`: Begin new collaboration session
- `join`: Join existing session
- `list`: Show active sessions
- `status`: Get session details

### 4. Pub/Sub Messaging
**Send messages to other agents**

```javascript
// Tool: agentic_os_pubsub_send
{
  "arguments": {
    "channel": "blockchain-infrastructure",
    "message": "QuantumSkyLink_v2 blockchain status update needed",
    "priority": "high" // low, normal, high, urgent
  }
}
```

**Channel Naming Conventions:**
- `project-{project_name}`: Project-specific updates
- `domain-{domain}`: Domain-specific discussions (blockchain, frontend, etc.)
- `urgent-{topic}`: High-priority communications
- `coordination-{team}`: Team coordination messages

### 5. Built-in LLM Queries
**Leverage local Ollama integration**

```javascript
// Tool: agentic_os_ollama_query
{
  "arguments": {
    "prompt": "Analyze the blockchain integration architecture for QuantumSkyLink v2",
    "model": "optional-specific-model",
    "temperature": 0.7,
    "max_tokens": 1000
  }
}
```

### 6. System Backup and Recovery
**Create system backups**

```javascript
// Tool: agentic_os_backup_create
{
  "arguments": {
    "name": "blockchain-integration-checkpoint",
    "description": "Backup after successful blockchain configuration",
    "include_data": true
  }
}
```

### 7. List System Backups
**View available system backups**

```javascript
// Tool: agentic_os_backup_list
{
  "arguments": {
    "limit": 10 // Optional: number of backups to list
  }
}
```

### 8. Pub/Sub System Statistics
**Monitor messaging system health**

```javascript
// Tool: agentic_os_pubsub_stats
{
  "arguments": {
    "channel": "optional-specific-channel" // Optional: get stats for specific channel
  }
}
```

**Response includes:**
- Active subscriptions count
- Total messages processed
- Topic activity statistics
- Message type distribution
- System operational status

### 9. Real-time Events Monitoring
**Monitor system events and activities**

```javascript
// Tool: agentic_os_events_status
{
  "arguments": {
    "limit": 10 // Optional: number of recent events to retrieve
  }
}
```

### 10. Ollama AI Model Management
**List available AI models**

```javascript
// Tool: agentic_os_ollama_models
{
  "arguments": {
    "detailed": true // Optional: include detailed model information
  }
}
```

### 11. Ollama AI Status Check
**Check Ollama integration status**

```javascript
// Tool: agentic_os_ollama_status
{
  "arguments": {}
}
```

### 12. Live Documentation Status
**Check documentation system status**

```javascript
// Tool: agentic_os_docs_status
{
  "arguments": {}
}
```

**Response provides:**
- Documentation system status
- Available guides and endpoints
- Last update timestamp
- Auto-update status

## Integration Patterns

### 1. Session Initialization Pattern
```javascript
// Step 1: Check system status
await use_mcp_tool("agentic-os", "agentic_os_system_status", {});

// Step 2: Discover relevant agents
await use_mcp_tool("agentic-os", "agentic_os_agent_discovery", {
  "filter": "your-domain"
});

// Step 3: Announce your presence
await use_mcp_tool("agentic-os", "agentic_os_pubsub_send", {
  "channel": "agent-coordination",
  "message": "LLM assistant active for [project/task]",
  "priority": "normal"
});
```

### 2. Collaborative Decision Pattern
```javascript
// Step 1: Start collaboration session
await use_mcp_tool("agentic-os", "agentic_os_collaborate", {
  "action": "start",
  "topic": "Technical_Decision_Required"
});

// Step 2: Gather input from specialized agents
await use_mcp_tool("agentic-os", "agentic_os_pubsub_send", {
  "channel": "technical-review",
  "message": "Input needed on [specific technical decision]",
  "priority": "high"
});

// Step 3: Process responses and make recommendation
// Step 4: Document decision in collaboration session
```

### 3. Context Preservation Pattern
```javascript
// Step 1: Create backup before major changes
await use_mcp_tool("agentic-os", "agentic_os_backup_create", {
  "name": "pre-change-backup",
  "description": "Backup before implementing [change]"
});

// Step 2: Broadcast context updates
await use_mcp_tool("agentic-os", "agentic_os_pubsub_send", {
  "channel": "context-updates",
  "message": "Context updated: [summary of changes]"
});
```

## Best Practices

### 1. Always Check System Status First
- Verify agentic-os is operational before other operations
- Check available features and capabilities
- Confirm authentication requirements

### 2. Use Appropriate Communication Channels
- **project-specific**: For project-related updates
- **domain-specific**: For technical domain discussions
- **urgent-**: For high-priority communications
- **coordination-**: For team coordination

### 3. Leverage Specialized Agents
- Identify relevant specialized agents for your task
- Collaborate rather than working in isolation
- Share context and decisions with appropriate agents

### 4. Document Decisions
- Use collaboration sessions for important decisions
- Create backups at significant milestones
- Broadcast context updates to keep agents synchronized

### 5. Handle Errors Gracefully
- Check for API errors (404, 405, etc.)
- Fall back to alternative communication methods
- Continue with core functionality if agentic-os is unavailable

## Error Handling

### Common Error Scenarios

#### 1. API Endpoint Not Available (404/405 errors)
```javascript
try {
  await use_mcp_tool("agentic-os", "agentic_os_collaborate", {...});
} catch (error) {
  // Fall back to alternative coordination method
  console.log("Agentic-OS collaboration unavailable, proceeding with direct task execution");
}
```

#### 2. System Unavailable
```javascript
// Always have fallback procedures
if (agentic_os_unavailable) {
  // Continue with task using alternative methods
  // Log the unavailability for later sync
  // Maintain local context for later upload
}
```

### 3. Network Connectivity Issues
```javascript
// Implement retry logic with exponential backoff
// Cache important messages for later delivery
// Continue with core functionality
```

## Integration with Existing Workflows

### 1. Task Tracker Integration
- Use agentic-os for high-level coordination
- Use task-tracker for detailed task management
- Sync important decisions between systems

### 2. Memory Bank Integration
- Use agentic-os for real-time collaboration
- Use memory bank for narrative context
- Maintain bidirectional synchronization

### 3. Project Management Integration
- Broadcast project status updates via pub/sub
- Use collaboration sessions for architectural decisions
- Create backups at project milestones

## Example Workflows

### 1. Blockchain Infrastructure Assessment
```javascript
// 1. System check
const systemStatus = await use_mcp_tool("agentic-os", "agentic_os_system_status", {});

// 2. Find blockchain experts
const agents = await use_mcp_tool("agentic-os", "agentic_os_agent_discovery", {
  "filter": "blockchain"
});

// 3. Start collaboration
const session = await use_mcp_tool("agentic-os", "agentic_os_collaborate", {
  "action": "start",
  "topic": "Blockchain_Infrastructure_Assessment"
});

// 4. Broadcast assessment request
await use_mcp_tool("agentic-os", "agentic_os_pubsub_send", {
  "channel": "blockchain-infrastructure",
  "message": "Infrastructure assessment needed for 6-network integration",
  "priority": "high"
});

// 5. Process responses and coordinate solution
```

### 2. Technical Decision Documentation
```javascript
// 1. Create backup before decision
await use_mcp_tool("agentic-os", "agentic_os_backup_create", {
  "name": "pre-decision-backup",
  "description": "Backup before architectural decision"
});

// 2. Start collaboration session
await use_mcp_tool("agentic-os", "agentic_os_collaborate", {
  "action": "start",
  "topic": "Architecture_Decision_Required"
});

// 3. Gather expert input
await use_mcp_tool("agentic-os", "agentic_os_pubsub_send", {
  "channel": "architecture-review",
  "message": "Input needed on [specific decision]",
  "priority": "high"
});

// 4. Document final decision
await use_mcp_tool("agentic-os", "agentic_os_pubsub_send", {
  "channel": "decisions",
  "message": "Decision made: [summary and rationale]",
  "priority": "normal"
});
```

## Advanced Features

### 1. Ollama LLM Integration
- Query local LLM for analysis and recommendations
- Use for code review and technical analysis
- Leverage for decision support and validation

### 2. Real-time Events System
- Monitor system events for coordination opportunities
- React to agent status changes
- Track collaboration session activities

### 3. Persistent Storage
- Store long-term context and decisions
- Maintain agent relationship history
- Preserve collaboration outcomes

## Troubleshooting

### 1. Connection Issues
- Verify agentic-os system is running
- Check network connectivity
- Confirm MCP server configuration

### 2. Authentication Problems
- Current system has authentication disabled
- Future versions may require authentication
- Check system status for auth requirements

### 3. Tool Execution Failures
- Validate input parameters
- Check for API endpoint availability
- Implement appropriate error handling

## Future Enhancements

### Planned Features
- Enhanced authentication and security
- Advanced collaboration workflows
- Integration with external project management tools
- Expanded agent specialization options
- Improved real-time coordination features

### Integration Opportunities
- CI/CD pipeline integration
- External API connectivity
- Enhanced backup and recovery options
- Advanced analytics and reporting
- Cross-project collaboration features

## Conclusion

The agentic-os system provides powerful capabilities for AI agent collaboration and coordination. By following these integration patterns and best practices, LLMs can effectively leverage the system for enhanced project management, decision-making, and collaborative problem-solving.

**Key Takeaways:**
1. Always check system status before operations
2. Use appropriate communication channels
3. Leverage specialized agents for domain expertise
4. Document important decisions and context
5. Handle errors gracefully with fallback procedures
6. Maintain synchronization with other systems

For the most current information, always check the system status and available features through the MCP tools, as the agentic-os system continues to evolve and expand its capabilities.
