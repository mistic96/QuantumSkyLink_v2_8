
import React, { useState, useEffect } from "react";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import {
  Activity, Users, DollarSign, Shield, Server, Database,
  TrendingUp, AlertTriangle, CheckCircle, RefreshCw
} from "lucide-react";

import MetricCard from "../components/dashboard/MetricCard";
import ServiceGrid from "../components/dashboard/ServiceGrid";
import AlertsPanel from "../components/dashboard/AlertsPanel";

export default function Dashboard() {
  const [lastUpdated, setLastUpdated] = useState(new Date());
  const [isRefreshing, setIsRefreshing] = useState(false);

  const handleRefresh = async () => {
    setIsRefreshing(true);
    // Simulate API call
    await new Promise(resolve => setTimeout(resolve, 1500));
    setLastUpdated(new Date());
    setIsRefreshing(false);
  };

  return (
    <div className="p-6 space-y-6 bg-slate-50 min-h-screen">
      <div className="flex flex-col md:flex-row justify-between items-start md:items-center gap-4">
        <div>
          <h1 className="text-3xl font-bold text-slate-900">System Dashboard</h1>
          <p className="text-slate-600 mt-1">
            Real-time monitoring and administration for QuantumSkyLink v2
          </p>
        </div>
        <div className="flex items-center gap-3">
          <p className="text-sm text-slate-500">
            Last updated: {lastUpdated.toLocaleTimeString()}
          </p>
          <Button
            variant="outline"
            size="sm"
            onClick={handleRefresh}
            disabled={isRefreshing}
            className="flex items-center gap-2"
          >
            <RefreshCw className={`w-4 h-4 ${isRefreshing ? 'animate-spin' : ''}`} />
            Refresh
          </Button>
        </div>
      </div>

      {/* Quick Actions */}
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <CheckCircle className="w-5 h-5" />
            Quick Actions
          </CardTitle>
        </CardHeader>
        <CardContent>
          <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
            <Button variant="outline" className="h-auto py-4 flex flex-col gap-2">
              <Users className="w-5 h-5" />
              <span className="text-sm">Manage Users</span>
            </Button>
            <Button variant="outline" className="h-auto py-4 flex flex-col gap-2">
              <DollarSign className="w-5 h-5" />
              <span className="text-sm">Treasury</span>
            </Button>
            <Button variant="outline" className="h-auto py-4 flex flex-col gap-2">
              <Shield className="w-5 h-5" />
              <span className="text-sm">Compliance</span>
            </Button>
            <Button variant="outline" className="h-auto py-4 flex flex-col gap-2">
              <Server className="w-5 h-5" />
              <span className="text-sm">Services</span>
            </Button>
          </div>
        </CardContent>
      </Card>

      {/* Key Metrics Grid */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
        <MetricCard
          title="System Health"
          value="98.7%"
          subtitle="24 services monitored"
          icon={Activity}
          trend="up"
          trendValue="+0.3%"
          status="success"
        />

        <MetricCard
          title="Active Users"
          value="12,847"
          subtitle="Across all platforms"
          icon={Users}
          trend="up"
          trendValue="+8.2%"
          status="normal"
        />

        <MetricCard
          title="Treasury Balance"
          value="$2.4M"
          subtitle="Multi-currency portfolio"
          icon={DollarSign}
          trend="up"
          trendValue="+12.5%"
          status="normal"
        />

        <MetricCard
          title="Security Alerts"
          value="3"
          subtitle="1 critical, 2 warnings"
          icon={AlertTriangle}
          trend="down"
          trendValue="-2"
          status="warning"
        />
      </div>

      {/* Infrastructure Status */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
        <MetricCard
          title="Database Status"
          value="18/18"
          subtitle="All databases online"
          icon={Database}
          status="success"
        />

        <MetricCard
          title="Microservices"
          value="22/24"
          subtitle="2 services degraded"
          icon={Server}
          status="warning"
        />

        <MetricCard
          title="Compliance Score"
          value="96.8%"
          subtitle="AML/KYC monitoring"
          icon={Shield}
          status="success"
        />

        <MetricCard
          title="API Performance"
          value="145ms"
          subtitle="Average response time"
          icon={TrendingUp}
          status="normal"
        />
      </div>

      {/* Main Content Grid */}
      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        <ServiceGrid />
        <AlertsPanel />
      </div>
    </div>
  );
}
