# Tracker MCP Server - Quick Reference

**Status**: ✅ Operational  
**Database**: `/home/codespace/Documents/Cline/MCP/tracker/tracker.db`

## Before You Start - ID_TRACKER.md Integration

**MANDATORY**: Always check and update ID_TRACKER.md to maintain canonical project binding:

```bash
# Windows - Check for existing project
findstr /C:"proj_1754850263745" ID_TRACKER.md

# Unix/Linux - Check for existing project  
grep "proj_1754850263745" ID_TRACKER.md

# After creating project - Add canonical entry
echo "P: proj_1754850263745 — ProjectName — Short description" >> ID_TRACKER.md

# After creating task - Add task entry under project
echo "  T: task_abc123 — Task title" >> ID_TRACKER.md
```

## Quick Commands

### Project Management
```javascript
// Create project
use_mcp_tool("tracker", "create_project", {
  "name": "ProjectName",
  "description": "Project description",
  "status": "active"
})

// List all projects
use_mcp_tool("tracker", "get_projects", {})
```

### Task Management
```javascript
// Create task
use_mcp_tool("tracker", "create_task", {
  "project_id": 1,
  "title": "Task title",
  "description": "Task description",
  "priority": "high",
  "tags": ["tag1", "tag2"],
  "progress": 0
})

// Update task
use_mcp_tool("tracker", "update_task", {
  "id": 1,
  "status": "in_progress",
  "progress": 50
})

// Complete task
use_mcp_tool("tracker", "complete_task", {
  "id": 1,
  "notes": "Completion notes"
})

// Get task details
use_mcp_tool("tracker", "get_task_details", {"id": 1})

// Filter tasks
use_mcp_tool("tracker", "get_tasks", {
  "project_id": 1,
  "status": "in_progress",
  "priority": "high"
})
```

### Progress Tracking
```javascript
// Log progress
use_mcp_tool("tracker", "log_progress", {
  "task_id": 1,
  "message": "Completed feature implementation",
  "progress_after": 75,
  "context": {
    "files_modified": ["file1.cs", "file2.cs"],
    "tests_added": 5
  }
})

// Add technical context
use_mcp_tool("tracker", "add_context", {
  "task_id": 1,
  "context_type": "technical_detail",
  "content": "Implemented using Strategy Pattern",
  "metadata": {"pattern": "Strategy", "component": "Service"}
})
```

### Search & Analytics
```javascript
// Search tasks
use_mcp_tool("tracker", "search_tasks", {
  "query": "pricing engine",
  "limit": 10
})

// Generate project summary
use_mcp_tool("tracker", "generate_summary", {
  "type": "project",
  "target_id": 1,
  "include_context": true
})

// Generate insights
use_mcp_tool("tracker", "generate_summary", {
  "type": "insights"
})

// Daily summary
use_mcp_tool("tracker", "generate_summary", {
  "type": "daily"
})
```

## Status Values
- `pending` - Not started
- `in_progress` - Currently working
- `completed` - Finished
- `blocked` - Blocked by dependencies

## Priority Levels
- `low` - Nice to have
- `medium` - Standard (default)
- `high` - Important
- `critical` - Urgent

## Context Types
- `file_path` - File system paths
- `code_snippet` - Code examples
- `note` - General notes
- `link` - URLs and references
- `technical_detail` - Technical decisions

## Summary Types
- `project` - Project status and progress
- `daily` - Daily progress summary
- `weekly` - Weekly analysis
- `task` - Individual task details
- `progress` - Overall metrics
- `insights` - Pattern analysis

## Common Workflows

### Starting New Feature (with ID_TRACKER.md)
1. **Check ID_TRACKER.md**: Search for existing project entries
2. Create task with `create_task`
3. **Update ID_TRACKER.md**: Add T: entry under project's P: entry
4. Add technical context with `add_context`
5. Log progress regularly with `log_progress`
6. Complete with `complete_task`

### Creating New Project (with ID_TRACKER.md)
1. **Search ID_TRACKER.md**: Check for duplicate project entries
2. Create project with `create_project`
3. **Update ID_TRACKER.md**: Add canonical P: entry immediately
4. Create initial tasks and add T: entries to ID_TRACKER.md
5. Validate no duplicate P: entries exist

### Project Status Check
1. Generate project summary: `generate_summary` (type: "project")
2. Check insights: `generate_summary` (type: "insights")
3. Search for specific work: `search_tasks`
4. **Validate ID_TRACKER.md**: Ensure consistency with tracker state

### Daily Standup Prep
1. Generate daily summary: `generate_summary` (type: "daily")
2. Get in-progress tasks: `get_tasks` (status: "in_progress")
3. Check blocked items: `get_tasks` (status: "blocked")

## Example: QuantumSkyLink_v2 Setup
```javascript
// 1. Create main project
use_mcp_tool("tracker", "create_project", {
  "name": "QuantumSkyLink_v2",
  "description": "Complete microservices financial platform with 24 services",
  "status": "active"
})

// 2. Create service implementation task
use_mcp_tool("tracker", "create_task", {
  "project_id": 1,
  "title": "Complete MarketplaceService implementation",
  "description": "Implement all pricing strategies and market data integration",
  "priority": "high",
  "tags": ["marketplace", "pricing", "backend"]
})

// 3. Log technical progress
use_mcp_tool("tracker", "log_progress", {
  "task_id": 1,
  "message": "Implemented 6 pricing strategies using Strategy Pattern",
  "progress_after": 85,
  "context": {
    "strategies": ["Fixed", "Bulk", "Margin", "Tiered", "Dynamic", "Unit"],
    "pattern": "Strategy Pattern",
    "integration": "Hangfire background processing"
  }
})

// 4. Generate project status
use_mcp_tool("tracker", "generate_summary", {
  "type": "project",
  "target_id": 1,
  "include_context": true
})
```

## Troubleshooting
- **Connection issues**: Restart MCP server
- **Database errors**: Check `/home/codespace/Documents/Cline/MCP/tracker/tracker.db` permissions
- **Build errors**: Run `cd /home/codespace/Documents/Cline/MCP/tracker && npm run build`

**Last Updated**: January 1, 2025
