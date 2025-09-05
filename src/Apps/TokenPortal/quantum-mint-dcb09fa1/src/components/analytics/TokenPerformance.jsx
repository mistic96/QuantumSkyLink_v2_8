import React from "react";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { motion } from "framer-motion";
import { TrendingUp } from "lucide-react";
import { BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer } from "recharts";

export default function TokenPerformance({ tokens, isLoading }) {
  const chartData = tokens.slice(0, 10).map(token => ({
    name: token.symbol,
    marketCap: token.market_cap || 0,
  }));

  return (
    <motion.div
      initial={{ opacity: 0, y: 20 }}
      animate={{ opacity: 1, y: 0 }}
      transition={{ delay: 0.3 }}
    >
      <Card className="border-0 shadow-xl">
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <TrendingUp className="w-5 h-5 text-blue-600" />
            Top Token Performance (by Market Cap)
          </CardTitle>
        </CardHeader>
        <CardContent>
          {!isLoading ? (
            <div className="h-80">
              <ResponsiveContainer width="100%" height="100%">
                <BarChart data={chartData}>
                  <CartesianGrid strokeDasharray="3 3" />
                  <XAxis dataKey="name" />
                  <YAxis />
                  <Tooltip formatter={(value) => `$${value.toLocaleString()}`} />
                  <Bar dataKey="marketCap" fill="#6366F1" />
                </BarChart>
              </ResponsiveContainer>
            </div>
          ) : (
            <div className="h-80 bg-gray-100 rounded-lg animate-pulse"></div>
          )}
        </CardContent>
      </Card>
    </motion.div>
  );
}