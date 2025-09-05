import React, { useState, useEffect } from "react";
import { ServiceMetrics } from "@/api/entities";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Progress } from "@/components/ui/progress";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { 
  Server, Activity, Zap, AlertTriangle, CheckCircle, 
  Clock, Cpu, MemoryStick, RefreshCw, Settings
} from "lucide-react";
import { format } from "date-fns";

export default function Services() {
  const [services, setServices] = useState([]);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    loadServices();
  }, []);

  const loadServices = async () => {
    setIsLoading(true);
    try {
      const data = await ServiceMetrics.list("-created_date", 50);
      setServices(data);
    } catch (error) {
      console.error("Error loading services:", error);
    }
    setIsLoading(false);
  };

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
      case "down": return <Server className="w-4 h-4 text-red-600" />;
      default: return <Server className="w-4 h-4 text-slate-600" />;
    }
  };

  const getPerformanceColor = (value, thresholds = { good: 90, warning: 70 }) => {
    if (value >= thresholds.good) return "text-emerald-600";
    if (value >= thresholds.warning) return "text-amber-600";
    return "text-red-600";
  };

  const serviceCounts = {
    total: services.length,
    healthy: services.filter(s => s.status === "healthy").length,
    degraded: services.filter(s => s.status === "degraded").length,
    down: services.filter(s => s.status === "down").length
  };

  return (
    <div className="p-6 space-y-6 bg-slate-50 min-h-screen">
      <div className="flex flex-col md:flex-row justify-between items-start md:items-center gap-4">
        <div>
          <h1 className="text-3xl font-bold text-slate-900">Service Management</h1>
          <p className="text-slate-600 mt-1">Monitor and manage microservice health and performance</p>
        </div>
        <Button onClick={loadServices} disabled={isLoading} className="flex items-center gap-2">
          <RefreshCw className={`w-4 h-4 ${isLoading ? 'animate-spin' : ''}`} />
          Refresh
        </Button>
      </div>

      {/* Service Overview */}
      <div className="grid grid-cols-1 md:grid-cols-4 gap-6">
        <Card>
          <CardContent className="p-6">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm font-medium text-slate-600">Total Services</p>
                <p className="text-2xl font-bold text-slate-900">{serviceCounts.total}</p>
              </div>
              <Server className="w-8 h-8 text-slate-600" />
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardContent className="p-6">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm font-medium text-slate-600">Healthy</p>
                <p className="text-2xl font-bold text-emerald-600">{serviceCounts.healthy}</p>
              </div>
              <CheckCircle className="w-8 h-8 text-emerald-600" />
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardContent className="p-6">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm font-medium text-slate-600">Degraded</p>
                <p className="text-2xl font-bold text-amber-600">{serviceCounts.degraded}</p>
              </div>
              <AlertTriangle className="w-8 h-8 text-amber-600" />
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardContent className="p-6">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm font-medium text-slate-600">Down</p>
                <p className="text-2xl font-bold text-red-600">{serviceCounts.down}</p>
              </div>
              <Server className="w-8 h-8 text-red-600" />
            </div>
          </CardContent>
        </Card>
      </div>

      <Tabs defaultValue="grid" className="w-full">
        <TabsList className="grid w-full grid-cols-2 bg-white border border-slate-200">
          <TabsTrigger value="grid">Grid View</TabsTrigger>
          <TabsTrigger value="detailed">Detailed View</TabsTrigger>
        </TabsList>

        <TabsContent value="grid" className="space-y-6">
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
            {services.map((service) => (
              <Card key={service.id} className="hover:shadow-lg transition-shadow duration-200">
                <CardHeader className="pb-3">
                  <div className="flex items-center justify-between">
                    <div className="flex items-center gap-2">
                      {getStatusIcon(service.status)}
                      <CardTitle className="text-lg">{service.service_name}</CardTitle>
                    </div>
                    <Badge className={getStatusColor(service.status)}>
                      {service.status}
                    </Badge>
                  </div>
                </CardHeader>
                <CardContent className="space-y-4">
                  <div className="grid grid-cols-2 gap-4 text-sm">
                    <div>
                      <p className="text-slate-500">Uptime</p>
                      <p className={`font-semibold ${getPerformanceColor(service.uptime_percentage)}`}>
                        {service.uptime_percentage?.toFixed(2)}%
                      </p>
                    </div>
                    <div>
                      <p className="text-slate-500">Response Time</p>
                      <p className={`font-semibold ${
                        service.response_time_ms < 100 ? 'text-emerald-600' :
                        service.response_time_ms < 500 ? 'text-amber-600' : 'text-red-600'
                      }`}>
                        {service.response_time_ms}ms
                      </p>
                    </div>
                  </div>

                  <div className="space-y-2">
                    <div className="flex justify-between items-center text-sm">
                      <span className="flex items-center gap-1 text-slate-500">
                        <Cpu className="w-3 h-3" />
                        CPU
                      </span>
                      <span className="font-medium">{service.cpu_usage}%</span>
                    </div>
                    <Progress value={service.cpu_usage} className="h-2" />
                  </div>

                  <div className="space-y-2">
                    <div className="flex justify-between items-center text-sm">
                      <span className="flex items-center gap-1 text-slate-500">
                        <MemoryStick className="w-3 h-3" />
                        Memory
                      </span>
                      <span className="font-medium">{service.memory_usage}%</span>
                    </div>
                    <Progress value={service.memory_usage} className="h-2" />
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

        <TabsContent value="detailed">
          <Card>
            <CardHeader>
              <CardTitle>Service Details</CardTitle>
            </CardHeader>
            <CardContent>
              <div className="space-y-4">
                {services.map((service) => (
                  <Card key={service.id} className="p-4">
                    <div className="grid grid-cols-1 md:grid-cols-6 gap-4 items-center">
                      <div className="md:col-span-2">
                        <div className="flex items-center gap-2">
                          {getStatusIcon(service.status)}
                          <div>
                            <p className="font-semibold">{service.service_name}</p>
                            <Badge className={getStatusColor(service.status)} size="sm">
                              {service.status}
                            </Badge>
                          </div>
                        </div>
                      </div>

                      <div className="text-center">
                        <p className="text-sm text-slate-500">Uptime</p>
                        <p className={`font-semibold ${getPerformanceColor(service.uptime_percentage)}`}>
                          {service.uptime_percentage?.toFixed(2)}%
                        </p>
                      </div>

                      <div className="text-center">
                        <p className="text-sm text-slate-500">Response</p>
                        <p className={`font-semibold ${
                          service.response_time_ms < 100 ? 'text-emerald-600' :
                          service.response_time_ms < 500 ? 'text-amber-600' : 'text-red-600'
                        }`}>
                          {service.response_time_ms}ms
                        </p>
                      </div>

                      <div className="space-y-1">
                        <div className="flex justify-between text-xs">
                          <span>CPU</span>
                          <span>{service.cpu_usage}%</span>
                        </div>
                        <Progress value={service.cpu_usage} className="h-1" />
                      </div>

                      <div className="space-y-1">
                        <div className="flex justify-between text-xs">
                          <span>Memory</span>
                          <span>{service.memory_usage}%</span>
                        </div>
                        <Progress value={service.memory_usage} className="h-1" />
                      </div>
                    </div>
                  </Card>
                ))}
              </div>
            </CardContent>
          </Card>
        </TabsContent>
      </Tabs>
    </div>
  );
}