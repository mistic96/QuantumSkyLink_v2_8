
import React, { useState } from "react";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Progress } from "@/components/ui/progress";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { 
  Network as NetworkIcon, Wifi, Globe, Shield, 
  Activity, Zap, AlertTriangle, CheckCircle, Settings
} from "lucide-react";

// Mock network data
const networkMetrics = {
  totalBandwidth: "10 Gbps",
  usedBandwidth: "2.3 Gbps", 
  utilizationPercent: 23,
  latency: 145,
  uptime: 99.98,
  packetsPerSecond: 145000,
  connectionsActive: 2847
};

const apiGateways = [
  {
    name: "Web API Gateway",
    status: "healthy",
    endpoint: "api.quantumskylink.com",
    requests_per_minute: 12500,
    response_time: 145,
    error_rate: 0.03,
    uptime: 99.97
  },
  {
    name: "Mobile API Gateway", 
    status: "healthy",
    endpoint: "mobile-api.quantumskylink.com",
    requests_per_minute: 8900,
    response_time: 122,
    error_rate: 0.01,
    uptime: 99.99
  },
  {
    name: "Admin API Gateway",
    status: "degraded",
    endpoint: "admin-api.quantumskylink.com", 
    requests_per_minute: 1200,
    response_time: 890,
    error_rate: 2.1,
    uptime: 98.50
  }
];

const blockchainNodes = [
  {
    name: "Primary Node",
    status: "healthy",
    block_height: 2847593,
    peers: 24,
    sync_status: "synced",
    network: "MultiChain"
  },
  {
    name: "Backup Node",
    status: "healthy", 
    block_height: 2847593,
    peers: 18,
    sync_status: "synced",
    network: "MultiChain"
  },
  {
    name: "Archive Node",
    status: "healthy",
    block_height: 2847593, 
    peers: 12,
    sync_status: "synced",
    network: "MultiChain"
  }
];

const securityRules = [
  {
    name: "DDoS Protection",
    status: "active",
    description: "Rate limiting and traffic filtering",
    blocked_requests: 1247
  },
  {
    name: "Firewall Rules",
    status: "active", 
    description: "IP filtering and port restrictions",
    blocked_ips: 89
  },
  {
    name: "SSL/TLS Enforcement",
    status: "active",
    description: "Encrypted connections only",
    certificates: 15
  }
];

export default function Network() {
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
      case "down": return <NetworkIcon className="w-4 h-4 text-red-600" />;
      default: return <NetworkIcon className="w-4 h-4 text-slate-600" />;
    }
  };

  return (
    <div className="p-6 space-y-6 bg-slate-50 min-h-screen">
      <div className="flex flex-col md:flex-row justify-between items-start md:items-center gap-4">
        <div>
          <h1 className="text-3xl font-bold text-slate-900">Network Management</h1>
          <p className="text-slate-600 mt-1">Monitor network performance and infrastructure</p>
        </div>
        <Button className="flex items-center gap-2">
          <Settings className="w-4 h-4" />
          Network Config
        </Button>
      </div>

      {/* Network Overview */}
      <div className="grid grid-cols-1 md:grid-cols-4 gap-6">
        <Card>
          <CardContent className="p-6">
            <div className="flex items-center justify-between mb-4">
              <div>
                <p className="text-sm font-medium text-slate-600">Total Bandwidth</p>
                <p className="text-2xl font-bold text-slate-900">{networkMetrics.totalBandwidth}</p>
              </div>
              <Wifi className="w-8 h-8 text-blue-600" />
            </div>
            <div className="space-y-2">
              <div className="flex justify-between text-sm">
                <span>Used: {networkMetrics.usedBandwidth}</span>
                <span>{networkMetrics.utilizationPercent}%</span>
              </div>
              <Progress value={networkMetrics.utilizationPercent} className="h-2" />
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardContent className="p-6">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm font-medium text-slate-600">Network Uptime</p>
                <p className="text-2xl font-bold text-emerald-600">{networkMetrics.uptime}%</p>
              </div>
              <Activity className="w-8 h-8 text-emerald-600" />
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardContent className="p-6">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm font-medium text-slate-600">Latency</p>
                <p className="text-2xl font-bold text-slate-900">{networkMetrics.latency}ms</p>
              </div>
              <Zap className="w-8 h-8 text-purple-600" />
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardContent className="p-6">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm font-medium text-slate-600">Active Connections</p>
                <p className="text-2xl font-bold text-slate-900">{networkMetrics.connectionsActive.toLocaleString()}</p>
              </div>
              <Globe className="w-8 h-8 text-blue-600" />
            </div>
          </CardContent>
        </Card>
      </div>

      <Tabs defaultValue="gateways" className="w-full">
        <TabsList className="grid w-full grid-cols-4 bg-white border border-slate-200">
          <TabsTrigger value="gateways">API Gateways</TabsTrigger>
          <TabsTrigger value="blockchain">Blockchain</TabsTrigger>
          <TabsTrigger value="security">Security</TabsTrigger>
          <TabsTrigger value="performance">Performance</TabsTrigger>
        </TabsList>

        <TabsContent value="gateways" className="space-y-6">
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
            {apiGateways.map((gateway) => (
              <Card key={gateway.name} className="hover:shadow-lg transition-shadow duration-200">
                <CardHeader className="pb-3">
                  <div className="flex items-center justify-between">
                    <CardTitle className="text-lg flex items-center gap-2">
                      {getStatusIcon(gateway.status)}
                      {gateway.name}
                    </CardTitle>
                    <Badge className={getStatusColor(gateway.status)}>
                      {gateway.status}
                    </Badge>
                  </div>
                </CardHeader>
                <CardContent className="space-y-4">
                  <div>
                    <p className="text-sm text-slate-500 mb-1">Endpoint</p>
                    <p className="text-sm font-mono bg-slate-50 p-2 rounded">{gateway.endpoint}</p>
                  </div>

                  <div className="grid grid-cols-2 gap-4 text-sm">
                    <div>
                      <p className="text-slate-500">Requests/min</p>
                      <p className="font-semibold">{gateway.requests_per_minute.toLocaleString()}</p>
                    </div>
                    <div>
                      <p className="text-slate-500">Response Time</p>
                      <p className={`font-semibold ${
                        gateway.response_time < 200 ? 'text-emerald-600' :
                        gateway.response_time < 500 ? 'text-amber-600' : 'text-red-600'
                      }`}>
                        {gateway.response_time}ms
                      </p>
                    </div>
                    <div>
                      <p className="text-slate-500">Error Rate</p>
                      <p className={`font-semibold ${
                        gateway.error_rate < 1 ? 'text-emerald-600' :
                        gateway.error_rate < 5 ? 'text-amber-600' : 'text-red-600'
                      }`}>
                        {gateway.error_rate}%
                      </p>
                    </div>
                    <div>
                      <p className="text-slate-500">Uptime</p>
                      <p className="font-semibold text-emerald-600">{gateway.uptime}%</p>
                    </div>
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

        <TabsContent value="blockchain" className="space-y-6">
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
            {blockchainNodes.map((node) => (
              <Card key={node.name} className="hover:shadow-lg transition-shadow duration-200">
                <CardHeader className="pb-3">
                  <div className="flex items-center justify-between">
                    <CardTitle className="text-lg flex items-center gap-2">
                      {getStatusIcon(node.status)}
                      {node.name}
                    </CardTitle>
                    <Badge className={getStatusColor(node.status)}>
                      {node.status}
                    </Badge>
                  </div>
                </CardHeader>
                <CardContent className="space-y-4">
                  <div className="grid grid-cols-1 gap-3 text-sm">
                    <div className="flex justify-between">
                      <span className="text-slate-500">Network</span>
                      <span className="font-semibold">{node.network}</span>
                    </div>
                    <div className="flex justify-between">
                      <span className="text-slate-500">Block Height</span>
                      <span className="font-semibold">{node.block_height.toLocaleString()}</span>
                    </div>
                    <div className="flex justify-between">
                      <span className="text-slate-500">Peers</span>
                      <span className="font-semibold">{node.peers}</span>
                    </div>
                    <div className="flex justify-between">
                      <span className="text-slate-500">Sync Status</span>
                      <Badge className="bg-emerald-100 text-emerald-800 text-xs">
                        {node.sync_status}
                      </Badge>
                    </div>
                  </div>

                  <div className="pt-2 border-t">
                    <Button variant="ghost" size="sm" className="w-full flex items-center gap-2">
                      <Settings className="w-3 h-3" />
                      Manage Node
                    </Button>
                  </div>
                </CardContent>
              </Card>
            ))}
          </div>
        </TabsContent>

        <TabsContent value="security" className="space-y-6">
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
            {securityRules.map((rule) => (
              <Card key={rule.name} className="hover:shadow-lg transition-shadow duration-200">
                <CardHeader className="pb-3">
                  <div className="flex items-center justify-between">
                    <CardTitle className="text-lg flex items-center gap-2">
                      <Shield className="w-5 h-5 text-blue-600" />
                      {rule.name}
                    </CardTitle>
                    <Badge className={getStatusColor(rule.status === "active" ? "healthy" : "down")}>
                      {rule.status}
                    </Badge>
                  </div>
                </CardHeader>
                <CardContent className="space-y-4">
                  <p className="text-sm text-slate-600">{rule.description}</p>
                  
                  <div className="grid grid-cols-1 gap-2 text-sm">
                    {rule.blocked_requests && (
                      <div className="flex justify-between">
                        <span className="text-slate-500">Blocked Requests</span>
                        <span className="font-semibold text-red-600">{rule.blocked_requests.toLocaleString()}</span>
                      </div>
                    )}
                    {rule.blocked_ips && (
                      <div className="flex justify-between">
                        <span className="text-slate-500">Blocked IPs</span>
                        <span className="font-semibold text-red-600">{rule.blocked_ips}</span>
                      </div>
                    )}
                    {rule.certificates && (
                      <div className="flex justify-between">
                        <span className="text-slate-500">Certificates</span>
                        <span className="font-semibold text-emerald-600">{rule.certificates}</span>
                      </div>
                    )}
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

        <TabsContent value="performance">
          <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
            <Card>
              <CardHeader>
                <CardTitle>Network Performance Metrics</CardTitle>
              </CardHeader>
              <CardContent>
                <div className="space-y-6">
                  <div>
                    <div className="flex justify-between items-center mb-2">
                      <span className="text-sm font-medium">Throughput</span>
                      <span className="text-sm font-bold">2.3 Gbps / 10 Gbps</span>
                    </div>
                    <Progress value={23} className="h-3" />
                  </div>

                  <div>
                    <div className="flex justify-between items-center mb-2">
                      <span className="text-sm font-medium">Packet Loss</span>
                      <span className="text-sm font-bold text-emerald-600">0.01%</span>
                    </div>
                    <Progress value={1} className="h-3" />
                  </div>

                  <div>
                    <div className="flex justify-between items-center mb-2">
                      <span className="text-sm font-medium">CPU Usage</span>
                      <span className="text-sm font-bold">34%</span>
                    </div>
                    <Progress value={34} className="h-3" />
                  </div>

                  <div>
                    <div className="flex justify-between items-center mb-2">
                      <span className="text-sm font-medium">Memory Usage</span>
                      <span className="text-sm font-bold">67%</span>
                    </div>
                    <Progress value={67} className="h-3" />
                  </div>
                </div>
              </CardContent>
            </Card>

            <Card>
              <CardHeader>
                <CardTitle>Traffic Analysis</CardTitle>
              </CardHeader>
              <CardContent>
                <div className="space-y-4">
                  <div className="p-4 border rounded-lg">
                    <div className="flex justify-between items-center mb-2">
                      <span className="font-medium">HTTP Requests</span>
                      <span className="text-sm font-bold">22,400/min</span>
                    </div>
                    <p className="text-sm text-slate-600">API gateway traffic</p>
                  </div>

                  <div className="p-4 border rounded-lg">
                    <div className="flex justify-between items-center mb-2">
                      <span className="font-medium">WebSocket Connections</span>
                      <span className="text-sm font-bold">1,847</span>
                    </div>
                    <p className="text-sm text-slate-600">Real-time connections</p>
                  </div>

                  <div className="p-4 border rounded-lg">
                    <div className="flex justify-between items-center mb-2">
                      <span className="font-medium">Database Queries</span>
                      <span className="text-sm font-bold">1,348/sec</span>
                    </div>
                    <p className="text-sm text-slate-600">Total DB operations</p>
                  </div>

                  <div className="p-4 border rounded-lg">
                    <div className="flex justify-between items-center mb-2">
                      <span className="font-medium">Cache Operations</span>
                      <span className="text-sm font-bold">8,130/sec</span>
                    </div>
                    <p className="text-sm text-slate-600">Redis cache operations</p>
                  </div>
                </div>
              </CardContent>
            </Card>
          </div>
        </TabsContent>
      </Tabs>
    </div>
  );
}
