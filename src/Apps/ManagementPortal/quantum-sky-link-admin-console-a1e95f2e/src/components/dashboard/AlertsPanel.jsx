import React from 'react';
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { AlertTriangle, Clock, ExternalLink } from "lucide-react";
import { formatDistanceToNow } from "date-fns";

const mockAlerts = [
  {
    id: 1,
    type: "critical",
    title: "Blockchain Interface Down",
    service: "Blockchain Interface",
    timestamp: new Date(Date.now() - 5 * 60 * 1000),
    status: "active"
  },
  {
    id: 2,
    type: "warning",
    title: "High Response Time",
    service: "Analytics Engine",
    timestamp: new Date(Date.now() - 15 * 60 * 1000),
    status: "acknowledged"
  },
  {
    id: 3,
    type: "warning",
    title: "Memory Usage Above 85%",
    service: "Identity Verification",
    timestamp: new Date(Date.now() - 30 * 60 * 1000),
    status: "active"
  },
  {
    id: 4,
    type: "info",
    title: "Scheduled Maintenance Complete",
    service: "Payment Gateway",
    timestamp: new Date(Date.now() - 2 * 60 * 60 * 1000),
    status: "resolved"
  }
];

export default function AlertsPanel() {
  const getAlertColor = (type) => {
    switch (type) {
      case "critical": return "bg-red-100 text-red-800 border-red-200";
      case "warning": return "bg-amber-100 text-amber-800 border-amber-200";
      case "info": return "bg-blue-100 text-blue-800 border-blue-200";
      default: return "bg-slate-100 text-slate-800 border-slate-200";
    }
  };

  const getStatusColor = (status) => {
    switch (status) {
      case "active": return "bg-red-50 text-red-700 border-red-200";
      case "acknowledged": return "bg-amber-50 text-amber-700 border-amber-200";
      case "resolved": return "bg-emerald-50 text-emerald-700 border-emerald-200";
      default: return "bg-slate-50 text-slate-700 border-slate-200";
    }
  };

  return (
    <Card>
      <CardHeader className="flex flex-row items-center justify-between">
        <CardTitle className="flex items-center gap-2">
          <AlertTriangle className="w-5 h-5" />
          Recent Alerts
        </CardTitle>
        <Button variant="outline" size="sm" className="text-xs">
          View All
          <ExternalLink className="w-3 h-3 ml-1" />
        </Button>
      </CardHeader>
      <CardContent>
        <div className="space-y-4">
          {mockAlerts.map((alert) => (
            <div
              key={alert.id}
              className="flex items-start justify-between p-3 rounded-lg border bg-white hover:bg-slate-50 transition-colors duration-200"
            >
              <div className="flex-1 min-w-0">
                <div className="flex items-center gap-2 mb-1">
                  <Badge 
                    variant="outline" 
                    className={`text-xs px-2 py-0.5 ${getAlertColor(alert.type)}`}
                  >
                    {alert.type.toUpperCase()}
                  </Badge>
                  <Badge 
                    variant="outline" 
                    className={`text-xs px-2 py-0.5 ${getStatusColor(alert.status)}`}
                  >
                    {alert.status}
                  </Badge>
                </div>
                <h4 className="font-medium text-sm text-slate-900 mb-1">{alert.title}</h4>
                <p className="text-xs text-slate-500">{alert.service}</p>
                <div className="flex items-center gap-1 mt-2 text-xs text-slate-400">
                  <Clock className="w-3 h-3" />
                  <span>{formatDistanceToNow(alert.timestamp, { addSuffix: true })}</span>
                </div>
              </div>
            </div>
          ))}
        </div>
      </CardContent>
    </Card>
  );
}