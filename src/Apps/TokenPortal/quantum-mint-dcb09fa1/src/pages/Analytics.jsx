import React, { useState, useEffect } from "react";
import { Token } from "@/api/entities";
import { Asset } from "@/api/entities";
import { ComplianceCheck } from "@/api/entities";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { motion } from "framer-motion";
import { 
  BarChart3, 
  TrendingUp, 
  TrendingDown,
  DollarSign,
  Coins,
  Building2,
  Shield,
  Calendar,
  Users,
  Activity
} from "lucide-react";

import MetricsGrid from "../components/analytics/MetricsGrid";
import TokenPerformance from "../components/analytics/TokenPerformance";
import AssetDistribution from "../components/analytics/AssetDistribution";
import ComplianceAnalytics from "../components/analytics/ComplianceAnalytics";

export default function Analytics() {
  const [tokens, setTokens] = useState([]);
  const [assets, setAssets] = useState([]);
  const [complianceChecks, setComplianceChecks] = useState([]);
  const [isLoading, setIsLoading] = useState(true);
  const [timeRange, setTimeRange] = useState('30');

  useEffect(() => {
    loadAnalyticsData();
  }, []);

  const loadAnalyticsData = async () => {
    setIsLoading(true);
    try {
      const [tokensData, assetsData, complianceData] = await Promise.all([
        Token.list('-created_date'),
        Asset.list('-created_date'),
        ComplianceCheck.list('-created_date')
      ]);
      
      setTokens(tokensData);
      setAssets(assetsData);
      setComplianceChecks(complianceData);
    } catch (error) {
      console.error('Error loading analytics data:', error);
    }
    setIsLoading(false);
  };

  const calculateMetrics = () => {
    const totalPortfolioValue = tokens.reduce((sum, token) => sum + (token.market_cap || 0), 0);
    const totalAssetValue = assets.reduce((sum, asset) => sum + (asset.valuation || 0), 0);
    const avgComplianceScore = complianceChecks.length > 0 
      ? Math.round(complianceChecks.reduce((sum, check) => sum + (check.score || 0), 0) / complianceChecks.length)
      : 0;
    const totalHolders = tokens.reduce((sum, token) => sum + (token.holders_count || 0), 0);

    return {
      totalPortfolioValue,
      totalAssetValue,
      avgComplianceScore,
      totalHolders,
      totalTokens: tokens.length,
      totalAssets: assets.length,
      totalVolume: tokens.reduce((sum, token) => sum + (token.trading_volume || 0), 0)
    };
  };

  const metrics = calculateMetrics();

  return (
    <div className="p-6 bg-gradient-to-br from-gray-50 to-white min-h-screen">
      <div className="max-w-7xl mx-auto">
        {/* Header */}
        <motion.div 
          initial={{ opacity: 0, y: 20 }}
          animate={{ opacity: 1, y: 0 }}
          className="flex flex-col lg:flex-row justify-between items-start lg:items-center gap-6 mb-8"
        >
          <div>
            <h1 className="text-3xl font-bold text-gray-900 mb-2">
              Analytics <span className="quantum-text-gradient">Hub</span>
            </h1>
            <p className="text-gray-600">
              Comprehensive insights into your tokenization performance
            </p>
          </div>
          
          <Select value={timeRange} onValueChange={setTimeRange}>
            <SelectTrigger className="w-40">
              <SelectValue placeholder="Time Range" />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="7">Last 7 days</SelectItem>
              <SelectItem value="30">Last 30 days</SelectItem>
              <SelectItem value="90">Last 90 days</SelectItem>
              <SelectItem value="365">Last year</SelectItem>
            </SelectContent>
          </Select>
        </motion.div>

        {/* Metrics Grid */}
        <MetricsGrid metrics={metrics} isLoading={isLoading} />

        {/* Charts Grid */}
        <div className="grid lg:grid-cols-2 gap-8 mb-8">
          <TokenPerformance tokens={tokens} isLoading={isLoading} />
          <AssetDistribution assets={assets} isLoading={isLoading} />
        </div>

        <div className="grid lg:grid-cols-3 gap-8">
          <div className="lg:col-span-2">
            <ComplianceAnalytics complianceChecks={complianceChecks} isLoading={isLoading} />
          </div>
          
          {/* Activity Timeline */}
          <motion.div
            initial={{ opacity: 0, x: 20 }}
            animate={{ opacity: 1, x: 0 }}
            transition={{ delay: 0.5 }}
          >
            <Card className="border-0 shadow-xl">
              <CardHeader>
                <CardTitle className="flex items-center gap-2">
                  <Activity className="w-5 h-5 text-purple-600" />
                  Recent Activity
                </CardTitle>
              </CardHeader>
              <CardContent>
                {!isLoading ? (
                  <div className="space-y-4">
                    {tokens.slice(0, 5).map((token, index) => (
                      <div key={token.id} className="flex items-center gap-3 p-3 bg-gray-50 rounded-lg">
                        <div className="w-8 h-8 bg-gradient-to-r from-blue-500 to-purple-500 rounded-lg flex items-center justify-center">
                          <Coins className="w-4 h-4 text-white" />
                        </div>
                        <div className="flex-1">
                          <p className="font-medium text-gray-900">{token.name}</p>
                          <p className="text-sm text-gray-500">Token created</p>
                        </div>
                        <span className="text-xs text-gray-400">
                          {new Date(token.created_date).toLocaleDateString()}
                        </span>
                      </div>
                    ))}
                  </div>
                ) : (
                  <div className="space-y-4">
                    {[1, 2, 3, 4, 5].map((i) => (
                      <div key={i} className="flex items-center gap-3 p-3 bg-gray-50 rounded-lg animate-pulse">
                        <div className="w-8 h-8 bg-gray-200 rounded-lg"></div>
                        <div className="flex-1 space-y-1">
                          <div className="h-4 bg-gray-200 rounded w-24"></div>
                          <div className="h-3 bg-gray-200 rounded w-16"></div>
                        </div>
                        <div className="h-3 bg-gray-200 rounded w-12"></div>
                      </div>
                    ))}
                  </div>
                )}
              </CardContent>
            </Card>
          </motion.div>
        </div>
      </div>
    </div>
  );
}