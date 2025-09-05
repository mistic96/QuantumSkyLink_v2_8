import React, { useState } from "react";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { Progress } from "@/components/ui/progress";
import { 
  Activity, Server, Database, Network, Cpu, HardDrive, 
  MemoryStick, Zap, AlertTriangle, CheckCircle, Clock
} from "lucide-react";

const systemMetrics = {
  cpu: { usage: 67, status: "normal", cores: 16 },
  memory: { usage: 78, status: "warning", total: "64GB" },
  disk: { usage: 45, status: "normal", total: "2TB" },
  network: { usage: 23, status: "normal", bandwidth: "10Gbps" }
};

const databaseMetrics = [
  { name: "User Database", status: "healthy", connections: 45, size: "2.3GB" },
  { name: "Transaction Database", status: "healthy", connections: 128, size: "15.7GB" },
  { name: "Audit Database", status: "healthy", connections: 23, size: "8.9GB" },
  { name: "Analytics Database", status: "degraded", connections: 89, size: "45.2GB" },
  { name: "Compliance Database", status: "healthy", connections: 34, size: "1.8GB" },
  { name: "Document Database", status: "healthy", connections: 67, size: "12.4GB" }
];

export default function SystemHealth() {
  const [activeTab, setActiveTab] = useState("overview");

  const getStatusColor = (status) => {
    switch (status) {
      case "healthy": return "text-emerald-600 bg-emerald-50 border-emerald-200";
      case "warning": return "text-amber-600 bg-amber-50 border-amber-200";
      case "degraded": return "text-orange-600 bg-orange-50 border-orange-200";
      case "critical": return "text-red-600 bg-red-50 border-red-200";
      default: return "text-slate-600 bg-slate-50 border-slate-200";
    }
  };

  const getUsageColor = (usage) => {
    if (usage >= 90) return "bg-red-500";
    if (usage >= 75) return "bg-amber-500";
    return "bg-emerald-500";
  };

  return (
    <div className="p-6 space-y-6 bg-slate-50 min-h-screen">
      <div className="flex justify-between items-center">
        <div>
          <h1 className="text-3xl font-bold text-slate-900">System Health Monitor</h1>
          <p className="text-slate-600 mt-1">Real-time infrastructure monitoring and diagnostics</p>
        </div>
        <div className="flex items-center gap-2">
          <Badge className="bg-emerald-50 text-emerald-700 border-emerald-200">
            <CheckCircle className="w-3 h-3 mr-1" />
            System Operational
          </Badge>
        </div>
      </div>

      <Tabs value={activeTab} onValueChange={setActiveTab} className="w-full">
        <TabsList className="grid w-full grid-cols-4 bg-white border border-slate-200">
          <TabsTrigger value="overview" className="flex items-center gap-2">
            <Activity className="w-4 h-4" />
            Overview
          </TabsTrigger>
          <TabsTrigger value="infrastructure" className="flex items-center gap-2">
            <Server className="w-4 h-4" />
            Infrastructure
          </TabsTrigger>
          <TabsTrigger value="databases" className="flex items-center gap-2">
            <Database className="w-4 h-4" />
            Databases
          </TabsTrigger>
          <TabsTrigger value="network" className="flex items-center gap-2">
            <Network className="w-4 h-4" />
            Network
          </TabsTrigger>
        </TabsList>

        <TabsContent value="overview" className="space-y-6">
          {/* System Resource Overview */}
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
            <Card>
              <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
                <CardTitle className="text-sm font-medium flex items-center gap-2">
                  <Cpu className="w-4 h-4" />
                  CPU Usage
                </CardTitle>
                <Badge className={getStatusColor(systemMetrics.cpu.status)}>
                  {systemMetrics.cpu.status}
                </Badge>
              </CardHeader>
              <CardContent>
                <div className="text-2xl font-bold mb-2">{systemMetrics.cpu.usage}%</div>
                <Progress 
                  value={systemMetrics.cpu.usage} 
                  className="h-2 mb-2"
                  // @ts-ignore
                  style={{ "--progress-background": getUsageColor(systemMetrics.cpu.usage) }}
                />
                <p className="text-xs text-slate-500">{systemMetrics.cpu.cores} cores</p>
              </CardContent>
            </Card>

            <Card>
              <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
                <CardTitle className="text-sm font-medium flex items-center gap-2">
                  <MemoryStick className="w-4 h-4" />
                  Memory Usage
                </CardTitle>
                <Badge className={getStatusColor(systemMetrics.memory.status)}>
                  {systemMetrics.memory.status}
                </Badge>
              </CardHeader>
              <CardContent>
                <div className="text-2xl font-bold mb-2">{systemMetrics.memory.usage}%</div>
                <Progress 
                  value={systemMetrics.memory.usage} 
                  className="h-2 mb-2"
                />
                <p className="text-xs text-slate-500">{systemMetrics.memory.total} total</p>
              </CardContent>
            </Card>

            <Card>
              <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
                <CardTitle className="text-sm font-medium flex items-center gap-2">
                  <HardDrive className="w-4 h-4" />
                  Disk Usage
                </CardTitle>
                <Badge className={getStatusColor(systemMetrics.disk.status)}>
                  {systemMetrics.disk.status}
                </Badge>
              </CardHeader>
              <CardContent>
                <div className="text-2xl font-bold mb-2">{systemMetrics.disk.usage}%</div>
                <Progress 
                  value={systemMetrics.disk.usage} 
                  className="h-2 mb-2"
                />
                <p className="text-xs text-slate-500">{systemMetrics.disk.total} total</p>
              </CardContent>
            </Card>

            <Card>
              <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
                <CardTitle className="text-sm font-medium flex items-center gap-2">
                  <Network className="w-4 h-4" />
                  Network Usage
                </CardTitle>
                <Badge className={getStatusColor(systemMetrics.network.status)}>
                  {systemMetrics.network.status}
                </Badge>
              </CardHeader>
              <CardContent>
                <div className="text-2xl font-bold mb-2">{systemMetrics.network.usage}%</div>
                <Progress 
                  value={systemMetrics.network.usage} 
                  className="h-2 mb-2"
                />
                <p className="text-xs text-slate-500">{systemMetrics.network.bandwidth} capacity</p>
              </CardContent>
            </Card>
          </div>
        </TabsContent>

        <TabsContent value="databases" className="space-y-6">
          <Card>
            <CardHeader>
              <CardTitle className="flex items-center gap-2">
                <Database className="w-5 h-5" />
                Database Health Status
              </CardTitle>
            </CardHeader>
            <CardContent>
              <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                {databaseMetrics.map((db, index) => (
                  <Card key={index} className="bg-slate-50">
                    <CardContent className="p-4">
                      <div className="flex items-center justify-between mb-3">
                        <h4 className="font-medium text-slate-900">{db.name}</h4>
                        <Badge className={getStatusColor(db.status)}>
                          {db.status}
                        </Badge>
                      </div>
                      <div className="space-y-2 text-sm">
                        <div className="flex justify-between">
                          <span className="text-slate-500">Connections:</span>
                          <span className="font-medium">{db.connections}</span>
                        </div>
                        <div className="flex justify-between">
                          <span className="text-slate-500">Size:</span>
                          <span className="font-medium">{db.size}</span>
                        </div>
                      </div>
                    </CardContent>
                  </Card>
                ))}
              </div>
            </CardContent>
          </Card>
        </TabsContent>

        <TabsContent value="infrastructure" className="space-y-6">
          <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
            <Card>
              <CardHeader>
                <CardTitle className="flex items-center gap-2">
                  <Server className="w-5 h-5" />
                  Server Infrastructure
                </CardTitle>
              </CardHeader>
              <CardContent>
                <div className="space-y-4">
                  <div className="flex items-center justify-between p-3 bg-slate-50 rounded-lg">
                    <div className="flex items-center gap-3">
                      <Zap className="w-5 h-5 text-blue-600" />
                      <div>
                        <p className="font-medium">Primary Cluster</p>
                        <p className="text-sm text-slate-500">8 nodes active</p>
                      </div>
                    </div>
                    <Badge className="bg-emerald-50 text-emerald-700">Online</Badge>
                  </div>
                  
                  <div className="flex items-center justify-between p-3 bg-slate-50 rounded-lg">
                    <div className="flex items-center gap-3">
                      <Zap className="w-5 h-5 text-amber-600" />
                      <div>
                        <p className="font-medium">Backup Cluster</p>
                        <p className="text-sm text-slate-500">4 nodes standby</p>
                      </div>
                    </div>
                    <Badge className="bg-amber-50 text-amber-700">Standby</Badge>
                  </div>
                </div>
              </CardContent>
            </Card>

            <Card>
              <CardHeader>
                <CardTitle className="flex items-center gap-2">
                  <Activity className="w-5 h-5" />
                  Performance Metrics
                </CardTitle>
              </CardHeader>
              <CardContent>
                <div className="space-y-4">
                  <div className="space-y-2">
                    <div className="flex justify-between text-sm">
                      <span>Request Throughput</span>
                      <span className="font-medium">2,847 req/sec</span>
                    </div>
                    <Progress value={75} className="h-2" />
                  </div>
                  
                  <div className="space-y-2">
                    <div className="flex justify-between text-sm">
                      <span>Error Rate</span>
                      <span className="font-medium">0.03%</span>
                    </div>
                    <Progress value={3} className="h-2" />
                  </div>

                  <div className="space-y-2">
                    <div className="flex justify-between text-sm">
                      <span>Cache Hit Rate</span>
                      <span className="font-medium">94.7%</span>
                    </div>
                    <Progress value={95} className="h-2" />
                  </div>
                </div>
              </CardContent>
            </Card>
          </div>
        </TabsContent>

        <TabsContent value="network" className="space-y-6">
          <Card>
            <CardHeader>
              <CardTitle className="flex items-center gap-2">
                <Network className="w-5 h-5" />
                Network Performance
              </CardTitle>
            </CardHeader>
            <CardContent>
              <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
                <div className="text-center">
                  <div className="text-2xl font-bold text-slate-900 mb-1">145ms</div>
                  <p className="text-sm text-slate-500">Average Latency</p>
                </div>
                <div className="text-center">
                  <div className="text-2xl font-bold text-slate-900 mb-1">99.98%</div>
                  <p className="text-sm text-slate-500">Uptime</p>
                </div>
                <div className="text-center">
                  <div className="text-2xl font-bold text-slate-900 mb-1">2.3GB/s</div>
                  <p className="text-sm text-slate-500">Throughput</p>
                </div>
              </div>
            </CardContent>
          </Card>
        </TabsContent>
      </Tabs>
    </div>
  );
}