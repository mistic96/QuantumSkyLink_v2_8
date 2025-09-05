
import React, { useState } from "react";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Progress } from "@/components/ui/progress";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { 
  Database as DatabaseIcon, Activity, HardDrive, Zap, 
  RefreshCw, Settings, AlertTriangle, CheckCircle
} from "lucide-react";

// Mock database data
const databases = [
  {
    name: "user_database",
    type: "PostgreSQL",
    status: "healthy",
    size: "2.3 GB",
    connections: 45,
    max_connections: 100,
    uptime: 99.98,
    last_backup: "2024-01-15T10:30:00Z",
    queries_per_second: 127
  },
  {
    name: "transaction_database", 
    type: "PostgreSQL",
    status: "healthy",
    size: "15.7 GB",
    connections: 128,
    max_connections: 200,
    uptime: 99.95,
    last_backup: "2024-01-15T10:35:00Z",
    queries_per_second: 456
  },
  {
    name: "audit_database",
    type: "PostgreSQL", 
    status: "healthy",
    size: "8.9 GB",
    connections: 23,
    max_connections: 100,
    uptime: 99.99,
    last_backup: "2024-01-15T10:25:00Z",
    queries_per_second: 89
  },
  {
    name: "analytics_database",
    type: "PostgreSQL",
    status: "degraded",
    size: "45.2 GB", 
    connections: 89,
    max_connections: 150,
    uptime: 97.20,
    last_backup: "2024-01-15T09:15:00Z",
    queries_per_second: 234
  },
  {
    name: "compliance_database",
    type: "PostgreSQL",
    status: "healthy",
    size: "1.8 GB",
    connections: 34,
    max_connections: 100,
    uptime: 99.92,
    last_backup: "2024-01-15T10:40:00Z",
    queries_per_second: 67
  },
  {
    name: "document_database",
    type: "PostgreSQL",
    status: "healthy", 
    size: "12.4 GB",
    connections: 67,
    max_connections: 120,
    uptime: 99.85,
    last_backup: "2024-01-15T10:20:00Z",
    queries_per_second: 145
  }
];

const redisInstances = [
  {
    name: "session_cache",
    status: "healthy",
    memory_used: "1.2 GB",
    memory_total: "4 GB", 
    hit_rate: 94.7,
    connections: 156,
    ops_per_second: 2340
  },
  {
    name: "application_cache",
    status: "healthy",
    memory_used: "2.8 GB",
    memory_total: "8 GB",
    hit_rate: 91.3,
    connections: 234,
    ops_per_second: 4560
  },
  {
    name: "rate_limit_cache", 
    status: "healthy",
    memory_used: "512 MB",
    memory_total: "2 GB",
    hit_rate: 98.2,
    connections: 89,
    ops_per_second: 1230
  }
];

export default function Database() {
  const [selectedDb, setSelectedDb] = useState(databases[0]);

  const getStatusColor = (status) => {
    switch (status) {
      case "healthy": return "bg-emerald-100 text-emerald-800 border-emerald-200";
      case "degraded": return "bg-amber-100 text-amber-800 border-amber-200";
      case "down": return "bg-red-100 text-red-800 border-red-200";
      default: return "bg-slate-100 text-slate-800 border-slate-200";
    }
  };

  const getStatusIcon = (status) => {
    switch (status) {
      case "healthy": return <CheckCircle className="w-4 h-4 text-emerald-600" />;
      case "degraded": return <AlertTriangle className="w-4 h-4 text-amber-600" />;
      case "down": return <DatabaseIcon className="w-4 h-4 text-red-600" />;
      default: return <DatabaseIcon className="w-4 h-4 text-slate-600" />;
    }
  };

  const dbCounts = {
    total: databases.length,
    healthy: databases.filter(db => db.status === "healthy").length,
    degraded: databases.filter(db => db.status === "degraded").length,
    down: databases.filter(db => db.status === "down").length
  };

  const totalConnections = databases.reduce((sum, db) => sum + db.connections, 0);
  const totalQPS = databases.reduce((sum, db) => sum + db.queries_per_second, 0);

  return (
    <div className="p-6 space-y-6 bg-slate-50 min-h-screen">
      <div className="flex flex-col md:flex-row justify-between items-start md:items-center gap-4">
        <div>
          <h1 className="text-3xl font-bold text-slate-900">Database Management</h1>
          <p className="text-slate-600 mt-1">Monitor and manage database performance and health</p>
        </div>
        <Button className="flex items-center gap-2">
          <RefreshCw className="w-4 h-4" />
          Refresh All
        </Button>
      </div>

      {/* Database Overview */}
      <div className="grid grid-cols-1 md:grid-cols-4 gap-6">
        <Card>
          <CardContent className="p-6">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm font-medium text-slate-600">Total Databases</p>
                <p className="text-2xl font-bold text-slate-900">{dbCounts.total}</p>
              </div>
              <DatabaseIcon className="w-8 h-8 text-slate-600" />
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardContent className="p-6">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm font-medium text-slate-600">Healthy</p>
                <p className="text-2xl font-bold text-emerald-600">{dbCounts.healthy}</p>
              </div>
              <CheckCircle className="w-8 h-8 text-emerald-600" />
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardContent className="p-6">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm font-medium text-slate-600">Total Connections</p>
                <p className="text-2xl font-bold text-slate-900">{totalConnections}</p>
              </div>
              <Zap className="w-8 h-8 text-blue-600" />
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardContent className="p-6">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm font-medium text-slate-600">Queries/sec</p>
                <p className="text-2xl font-bold text-slate-900">{totalQPS.toLocaleString()}</p>
              </div>
              <Activity className="w-8 h-8 text-purple-600" />
            </div>
          </CardContent>
        </Card>
      </div>

      <Tabs defaultValue="postgresql" className="w-full">
        <TabsList className="grid w-full grid-cols-3 bg-white border border-slate-200">
          <TabsTrigger value="postgresql">PostgreSQL</TabsTrigger>
          <TabsTrigger value="redis">Redis Cache</TabsTrigger>
          <TabsTrigger value="management">Management</TabsTrigger>
        </TabsList>

        <TabsContent value="postgresql" className="space-y-6">
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
            {databases.map((db) => (
              <Card key={db.name} className="hover:shadow-lg transition-shadow duration-200">
                <CardHeader className="pb-3">
                  <div className="flex items-center justify-between">
                    <CardTitle className="text-lg flex items-center gap-2">
                      {getStatusIcon(db.status)}
                      {db.name.replace('_', ' ')}
                    </CardTitle>
                    <Badge className={getStatusColor(db.status)}>
                      {db.status}
                    </Badge>
                  </div>
                </CardHeader>
                <CardContent className="space-y-4">
                  <div className="grid grid-cols-2 gap-4 text-sm">
                    <div>
                      <p className="text-slate-500">Size</p>
                      <p className="font-semibold">{db.size}</p>
                    </div>
                    <div>
                      <p className="text-slate-500">Uptime</p>
                      <p className="font-semibold text-emerald-600">{db.uptime}%</p>
                    </div>
                    <div>
                      <p className="text-slate-500">QPS</p>
                      <p className="font-semibold">{db.queries_per_second}</p>
                    </div>
                    <div>
                      <p className="text-slate-500">Type</p>
                      <p className="font-semibold">{db.type}</p>
                    </div>
                  </div>

                  <div className="space-y-2">
                    <div className="flex justify-between items-center text-sm">
                      <span className="text-slate-500">Connections</span>
                      <span className="font-medium">{db.connections}/{db.max_connections}</span>
                    </div>
                    <Progress 
                      value={(db.connections / db.max_connections) * 100} 
                      className="h-2" 
                    />
                  </div>

                  <div className="pt-2 border-t">
                    <Button variant="ghost" size="sm" className="w-full flex items-center gap-2">
                      <Settings className="w-3 h-3" />
                      Manage
                    </Button>
                  </div>
                </CardContent>
              </Card>
            ))}
          </div>
        </TabsContent>

        <TabsContent value="redis" className="space-y-6">
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
            {redisInstances.map((redis) => (
              <Card key={redis.name} className="hover:shadow-lg transition-shadow duration-200">
                <CardHeader className="pb-3">
                  <div className="flex items-center justify-between">
                    <CardTitle className="text-lg flex items-center gap-2">
                      <Zap className="w-5 h-5 text-red-500" />
                      {redis.name.replace('_', ' ')}
                    </CardTitle>
                    <Badge className={getStatusColor(redis.status)}>
                      {redis.status}
                    </Badge>
                  </div>
                </CardHeader>
                <CardContent className="space-y-4">
                  <div className="grid grid-cols-2 gap-4 text-sm">
                    <div>
                      <p className="text-slate-500">Hit Rate</p>
                      <p className="font-semibold text-emerald-600">{redis.hit_rate}%</p>
                    </div>
                    <div>
                      <p className="text-slate-500">OPS/sec</p>
                      <p className="font-semibold">{redis.ops_per_second.toLocaleString()}</p>
                    </div>
                    <div>
                      <p className="text-slate-500">Connections</p>
                      <p className="font-semibold">{redis.connections}</p>
                    </div>
                    <div>
                      <p className="text-slate-500">Memory</p>
                      <p className="font-semibold">{redis.memory_used}</p>
                    </div>
                  </div>

                  <div className="space-y-2">
                    <div className="flex justify-between items-center text-sm">
                      <span className="text-slate-500">Memory Usage</span>
                      <span className="font-medium">{redis.memory_used} / {redis.memory_total}</span>
                    </div>
                    <Progress 
                      value={(parseFloat(redis.memory_used) / parseFloat(redis.memory_total)) * 100} 
                      className="h-2" 
                    />
                  </div>

                  <div className="pt-2 border-t">
                    <Button variant="ghost" size="sm" className="w-full flex items-center gap-2">
                      <Settings className="w-3 h-3" />
                      Configure
                    </Button>
                  </div>
                </CardContent>
              </Card>
            ))}
          </div>
        </TabsContent>

        <TabsContent value="management">
          <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
            <Card>
              <CardHeader>
                <CardTitle>Database Operations</CardTitle>
              </CardHeader>
              <CardContent className="space-y-4">
                <Button className="w-full justify-start" variant="outline">
                  <HardDrive className="w-4 h-4 mr-2" />
                  Schedule Backup
                </Button>
                <Button className="w-full justify-start" variant="outline">
                  <Activity className="w-4 h-4 mr-2" />
                  Performance Analysis
                </Button>
                <Button className="w-full justify-start" variant="outline">
                  <Settings className="w-4 h-4 mr-2" />
                  Connection Pool Settings
                </Button>
                <Button className="w-full justify-start" variant="outline">
                  <DatabaseIcon className="w-4 h-4 mr-2" />
                  Query Optimization
                </Button>
              </CardContent>
            </Card>

            <Card>
              <CardHeader>
                <CardTitle>Backup Status</CardTitle>
              </CardHeader>
              <CardContent>
                <div className="space-y-4">
                  {databases.slice(0, 4).map((db) => (
                    <div key={db.name} className="flex items-center justify-between p-3 border rounded-lg">
                      <div>
                        <p className="font-medium">{db.name.replace('_', ' ')}</p>
                        <p className="text-sm text-slate-500">
                          Last backup: {new Date(db.last_backup).toLocaleDateString()}
                        </p>
                      </div>
                      <CheckCircle className="w-5 h-5 text-emerald-600" />
                    </div>
                  ))}
                </div>
              </CardContent>
            </Card>
          </div>
        </TabsContent>
      </Tabs>
    </div>
  );
}
