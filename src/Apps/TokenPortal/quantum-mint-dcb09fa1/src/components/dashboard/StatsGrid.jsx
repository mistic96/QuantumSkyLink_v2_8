import React from "react";
import { Card, CardContent } from "@/components/ui/card";
import { motion } from "framer-motion";
import { 
  Coins, 
  Building2, 
  DollarSign, 
  Shield,
  TrendingUp,
  TrendingDown
} from "lucide-react";

const StatCard = ({ title, value, icon: Icon, trend, trendValue, delay = 0, isLoading }) => (
  <motion.div
    initial={{ opacity: 0, y: 20 }}
    animate={{ opacity: 1, y: 0 }}
    transition={{ delay }}
  >
    <Card className="quantum-glow border-0 shadow-xl overflow-hidden relative">
      <div className="absolute top-0 right-0 w-32 h-32 transform translate-x-8 -translate-y-8 bg-gradient-to-r from-blue-500/10 to-purple-500/10 rounded-full" />
      <CardContent className="p-6 relative">
        {!isLoading ? (
          <>
            <div className="flex items-center justify-between mb-4">
              <div className="w-12 h-12 bg-gradient-to-r from-blue-500 to-purple-500 rounded-xl flex items-center justify-center">
                <Icon className="w-6 h-6 text-white" />
              </div>
              {trend && (
                <div className={`flex items-center gap-1 ${trend === 'up' ? 'text-green-600' : 'text-red-600'}`}>
                  {trend === 'up' ? <TrendingUp className="w-4 h-4" /> : <TrendingDown className="w-4 h-4" />}
                  <span className="text-sm font-medium">{trendValue}</span>
                </div>
              )}
            </div>
            <div>
              <p className="text-3xl font-bold text-gray-900 mb-1">{value}</p>
              <p className="text-sm text-gray-500">{title}</p>
            </div>
          </>
        ) : (
          <div className="animate-pulse">
            <div className="flex items-center justify-between mb-4">
              <div className="w-12 h-12 bg-gray-200 rounded-xl"></div>
              <div className="w-16 h-4 bg-gray-200 rounded"></div>
            </div>
            <div>
              <div className="h-8 bg-gray-200 rounded w-20 mb-1"></div>
              <div className="h-4 bg-gray-200 rounded w-24"></div>
            </div>
          </div>
        )}
      </CardContent>
    </Card>
  </motion.div>
);

export default function StatsGrid({ stats = {}, isLoading = false }) {
  const safeStats = {
    totalTokens: stats.totalTokens || 0,
    totalAssets: stats.totalAssets || 0,
    portfolioValue: stats.portfolioValue || 0,
    complianceScore: stats.complianceScore || 0
  };

  return (
    <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6 mb-8">
      <StatCard
        title="Active Tokens"
        value={safeStats.totalTokens}
        icon={Coins}
        trend="up"
        trendValue="+12%"
        delay={0.1}
        isLoading={isLoading}
      />
      <StatCard
        title="Total Assets"
        value={safeStats.totalAssets}
        icon={Building2}
        trend="up"
        trendValue="+8%"
        delay={0.2}
        isLoading={isLoading}
      />
      <StatCard
        title="Portfolio Value"
        value={`$${safeStats.portfolioValue.toLocaleString()}`}
        icon={DollarSign}
        trend="up"
        trendValue="+24%"
        delay={0.3}
        isLoading={isLoading}
      />
      <StatCard
        title="Compliance Score"
        value={`${safeStats.complianceScore}%`}
        icon={Shield}
        trend={safeStats.complianceScore >= 80 ? "up" : "down"}
        trendValue={safeStats.complianceScore >= 80 ? "Excellent" : "Needs Review"}
        delay={0.4}
        isLoading={isLoading}
      />
    </div>
  );
}