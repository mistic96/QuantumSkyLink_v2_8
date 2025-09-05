import React from "react";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { motion } from "framer-motion";
import { 
  DollarSign,
  Coins,
  Building2,
  Shield,
  Users,
  Activity
} from "lucide-react";

const metricItems = [
  { key: "totalPortfolioValue", title: "Portfolio Value", icon: DollarSign, prefix: "$", color: "text-blue-600" },
  { key: "totalAssetValue", title: "Asset Value", icon: Building2, prefix: "$", color: "text-purple-600" },
  { key: "totalTokens", title: "Total Tokens", icon: Coins, color: "text-pink-600" },
  { key: "totalVolume", title: "24h Volume", icon: Activity, prefix: "$", color: "text-green-600" },
  { key: "totalHolders", title: "Total Holders", icon: Users, color: "text-orange-600" },
  { key: "avgComplianceScore", title: "Compliance Score", icon: Shield, suffix: "%", color: "text-teal-600" },
];

export default function MetricsGrid({ metrics, isLoading }) {
  return (
    <motion.div 
      initial={{ opacity: 0, y: 20 }}
      animate={{ opacity: 1, y: 0 }}
      transition={{ delay: 0.2 }}
      className="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-6 gap-6 mb-8"
    >
      {metricItems.map((item, index) => (
        <motion.div
          key={item.key}
          initial={{ opacity: 0, scale: 0.9 }}
          animate={{ opacity: 1, scale: 1 }}
          transition={{ delay: 0.1 * index }}
        >
          <Card className="border-0 shadow-xl">
            <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
              <CardTitle className="text-sm font-medium">{item.title}</CardTitle>
              <item.icon className={`w-4 h-4 text-muted-foreground ${item.color}`} />
            </CardHeader>
            <CardContent>
              {!isLoading ? (
                <div className="text-2xl font-bold">
                  {item.prefix}{metrics[item.key]?.toLocaleString()}{item.suffix}
                </div>
              ) : (
                <div className="h-8 bg-gray-200 rounded animate-pulse"></div>
              )}
            </CardContent>
          </Card>
        </motion.div>
      ))}
    </motion.div>
  );
}