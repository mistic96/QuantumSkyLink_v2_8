import React from 'react';
import { Badge } from "@/components/ui/badge";
import { CheckCircle, Clock, XCircle, AlertCircle } from "lucide-react";

const statusConfig = {
  confirmed: { 
    color: "bg-emerald-100 text-emerald-800 border-emerald-200", 
    icon: CheckCircle 
  },
  pending: { 
    color: "bg-amber-100 text-amber-800 border-amber-200", 
    icon: Clock 
  },
  failed: { 
    color: "bg-red-100 text-red-800 border-red-200", 
    icon: XCircle 
  },
  rejected: { 
    color: "bg-red-100 text-red-800 border-red-200", 
    icon: XCircle 
  },
  active: { 
    color: "bg-green-100 text-green-800 border-green-200", 
    icon: CheckCircle 
  },
  suspended: { 
    color: "bg-orange-100 text-orange-800 border-orange-200", 
    icon: AlertCircle 
  },
  burned: { 
    color: "bg-gray-100 text-gray-800 border-gray-200", 
    icon: XCircle 
  },
  approved: { 
    color: "bg-blue-100 text-blue-800 border-blue-200", 
    icon: CheckCircle 
  },
  validated: { 
    color: "bg-green-100 text-green-800 border-green-200", 
    icon: CheckCircle 
  },
  invalid: { 
    color: "bg-red-100 text-red-800 border-red-200", 
    icon: XCircle 
  }
};

export default function StatusBadge({ status, size = "default" }) {
  const config = statusConfig[status] || statusConfig.pending;
  const Icon = config.icon;

  return (
    <Badge 
      variant="outline" 
      className={`${config.color} border font-medium ${
        size === "sm" ? "text-xs px-2 py-0.5" : "text-sm px-3 py-1"
      } flex items-center gap-1.5 w-fit`}
    >
      <Icon className={size === "sm" ? "w-3 h-3" : "w-4 h-4"} />
      {status.charAt(0).toUpperCase() + status.slice(1)}
    </Badge>
  );
}