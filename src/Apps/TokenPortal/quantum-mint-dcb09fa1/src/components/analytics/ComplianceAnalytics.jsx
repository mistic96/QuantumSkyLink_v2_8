import React from "react";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { motion } from "framer-motion";
import { Shield } from "lucide-react";
import { AreaChart, Area, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer } from "recharts";
import { format } from "date-fns";

export default function ComplianceAnalytics({ complianceChecks, isLoading }) {
  const chartData = complianceChecks
    .sort((a, b) => new Date(a.created_date) - new Date(b.created_date))
    .slice(-10)
    .map(check => ({
      date: format(new Date(check.created_date), 'MMM d'),
      score: check.score
    }));

  return (
    <motion.div
      initial={{ opacity: 0, y: 20 }}
      animate={{ opacity: 1, y: 0 }}
      transition={{ delay: 0.5 }}
    >
      <Card className="border-0 shadow-xl">
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <Shield className="w-5 h-5 text-teal-600" />
            Compliance Score Over Time
          </CardTitle>
        </CardHeader>
        <CardContent>
          {!isLoading ? (
            <div className="h-80">
              <ResponsiveContainer width="100%" height="100%">
                <AreaChart data={chartData}>
                  <CartesianGrid strokeDasharray="3 3" />
                  <XAxis dataKey="date" />
                  <YAxis domain={[0, 100]} />
                  <Tooltip />
                  <Area type="monotone" dataKey="score" stroke="#14B8A6" fill="#14B8A6" fillOpacity={0.2} />
                </AreaChart>
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