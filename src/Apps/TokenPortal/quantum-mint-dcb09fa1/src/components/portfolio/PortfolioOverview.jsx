import React from "react";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { motion } from "framer-motion";
import { DollarSign, Coins, Building2 } from "lucide-react";

export default function PortfolioOverview({ stats, isLoading }) {
  const overviewItems = [
    { key: 'totalValue', title: 'Total Portfolio Value', icon: DollarSign, color: "text-green-600", bgColor: "bg-green-100" },
    { key: 'tokenValue', title: 'Token Value', icon: Coins, color: "text-blue-600", bgColor: "bg-blue-100" },
    { key: 'assetValue', title: 'Asset Value', icon: Building2, color: "text-purple-600", bgColor: "bg-purple-100" },
  ];

  return (
    <motion.div
      initial={{ opacity: 0, y: 20 }}
      animate={{ opacity: 1, y: 0 }}
      transition={{ delay: 0.2 }}
      className="grid grid-cols-1 md:grid-cols-3 gap-6 mb-8"
    >
      {overviewItems.map((item, index) => (
        <Card key={item.key} className="border-0 shadow-lg">
          <CardHeader className="flex flex-row items-center justify-between pb-2">
            <CardTitle className="text-base font-medium">{item.title}</CardTitle>
            <div className={`w-8 h-8 rounded-full ${item.bgColor} flex items-center justify-center`}>
              <item.icon className={`w-4 h-4 ${item.color}`} />
            </div>
          </CardHeader>
          <CardContent>
            {!isLoading ? (
              <div className="text-3xl font-bold">
                ${stats[item.key]?.toLocaleString()}
              </div>
            ) : (
              <div className="h-10 bg-gray-200 rounded animate-pulse"></div>
            )}
            <p className="text-xs text-muted-foreground mt-1">
              {item.key === 'totalValue' ? `${stats.totalTokens} tokens & ${stats.totalAssets} assets` : ' '}
            </p>
          </CardContent>
        </Card>
      ))}
    </motion.div>
  );
}