import React from 'react';
import { Card, CardContent, CardHeader } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { TrendingUp, TrendingDown, Minus } from "lucide-react";

export default function MetricCard({ 
  title, 
  value, 
  subtitle, 
  icon: Icon, 
  trend, 
  trendValue, 
  status = "normal",
  className = "" 
}) {
  const statusColors = {
    success: "text-emerald-600 bg-emerald-50 border-emerald-200",
    warning: "text-amber-600 bg-amber-50 border-amber-200",
    danger: "text-red-600 bg-red-50 border-red-200",
    normal: "text-slate-600 bg-white border-slate-200"
  };

  const getTrendIcon = () => {
    if (trend === "up") return <TrendingUp className="w-4 h-4" />;
    if (trend === "down") return <TrendingDown className="w-4 h-4" />;
    return <Minus className="w-4 h-4" />;
  };

  const getTrendColor = () => {
    if (trend === "up") return "text-emerald-600";
    if (trend === "down") return "text-red-600";
    return "text-slate-500";
  };

  return (
    <Card className={`${statusColors[status]} transition-all duration-200 hover:shadow-lg ${className}`}>
      <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-3">
        <div className="flex items-center gap-2">
          {Icon && <Icon className="w-5 h-5 text-slate-600" />}
          <p className="text-sm font-medium text-slate-600">{title}</p>
        </div>
        {status !== "normal" && (
          <Badge 
            variant="outline" 
            className={`text-xs px-2 py-1 ${
              status === "success" ? "border-emerald-200 text-emerald-700" :
              status === "warning" ? "border-amber-200 text-amber-700" :
              "border-red-200 text-red-700"
            }`}
          >
            {status.toUpperCase()}
          </Badge>
        )}
      </CardHeader>
      <CardContent>
        <div className="space-y-2">
          <div className="text-2xl font-bold text-slate-900">{value}</div>
          {subtitle && (
            <p className="text-xs text-slate-500">{subtitle}</p>
          )}
          {trend && trendValue && (
            <div className={`flex items-center gap-1 text-sm ${getTrendColor()}`}>
              {getTrendIcon()}
              <span className="font-medium">{trendValue}</span>
              <span className="text-slate-500">vs last period</span>
            </div>
          )}
        </div>
      </CardContent>
    </Card>
  );
}