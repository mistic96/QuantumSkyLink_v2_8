import React from 'react';
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Server, Zap, AlertTriangle, Clock } from "lucide-react";
import { motion } from "framer-motion";

const mockServices = [
  { name: "Authentication Service", status: "healthy", uptime: 99.98, responseTime: 45 },
  { name: "Payment Gateway", status: "healthy", uptime: 99.95, responseTime: 120 },
  { name: "Identity Verification", status: "degraded", uptime: 98.50, responseTime: 890 },
  { name: "Treasury Service", status: "healthy", uptime: 99.99, responseTime: 32 },
  { name: "Compliance Engine", status: "healthy", uptime: 99.92, responseTime: 78 },
  { name: "Blockchain Interface", status: "down", uptime: 0.00, responseTime: 0 },
  { name: "Risk Assessment", status: "healthy", uptime: 99.88, responseTime: 156 },
  { name: "Notification Service", status: "healthy", uptime: 99.95, responseTime: 23 },
  { name: "Analytics Engine", status: "degraded", uptime: 97.20, responseTime: 2100 },
  { name: "User Management", status: "healthy", uptime: 99.97, responseTime: 67 },
  { name: "Document Storage", status: "healthy", uptime: 99.90, responseTime: 89 },
  { name: "Audit Logger", status: "healthy", uptime: 99.85, responseTime: 45 }
];

export default function ServiceGrid() {
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
      case "healthy": return <Zap className="w-4 h-4" />;
      case "degraded": return <AlertTriangle className="w-4 h-4" />;
      case "down": return <Server className="w-4 h-4" />;
      default: return <Server className="w-4 h-4" />;
    }
  };

  return (
    <Card className="col-span-2">
      <CardHeader>
        <CardTitle className="flex items-center gap-2">
          <Server className="w-5 h-5" />
          Service Status Overview
        </CardTitle>
      </CardHeader>
      <CardContent>
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
          {mockServices.map((service, index) => (
            <motion.div
              key={service.name}
              initial={{ opacity: 0, y: 20 }}
              animate={{ opacity: 1, y: 0 }}
              transition={{ delay: index * 0.05 }}
            >
              <Card className="hover:shadow-md transition-shadow duration-200">
                <CardContent className="p-4">
                  <div className="flex items-start justify-between mb-3">
                    <div className="flex items-center gap-2">
                      {getStatusIcon(service.status)}
                      <h4 className="font-medium text-sm text-slate-900 leading-tight">
                        {service.name}
                      </h4>
                    </div>
                    <Badge 
                      variant="outline" 
                      className={`text-xs px-2 py-1 ${getStatusColor(service.status)}`}
                    >
                      {service.status}
                    </Badge>
                  </div>
                  <div className="space-y-2">
                    <div className="flex justify-between items-center text-xs">
                      <span className="text-slate-500">Uptime</span>
                      <span className="font-semibold text-slate-900">{service.uptime}%</span>
                    </div>
                    <div className="flex justify-between items-center text-xs">
                      <span className="text-slate-500 flex items-center gap-1">
                        <Clock className="w-3 h-3" />
                        Response
                      </span>
                      <span className="font-semibold text-slate-900">{service.responseTime}ms</span>
                    </div>
                  </div>
                </CardContent>
              </Card>
            </motion.div>
          ))}
        </div>
      </CardContent>
    </Card>
  );
}