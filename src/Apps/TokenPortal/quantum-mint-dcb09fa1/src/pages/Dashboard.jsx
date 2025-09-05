import React, { useState, useEffect } from "react";
import { Token } from "@/api/entities";
import { Asset } from "@/api/entities";
import { ComplianceCheck } from "@/api/entities";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Link } from "react-router-dom";
import { createPageUrl } from "@/utils";
import { 
  TrendingUp, 
  Coins, 
  Building2, 
  Shield, 
  Plus,
  ArrowUpRight,
  AlertTriangle,
  CheckCircle,
  Clock,
  DollarSign
} from "lucide-react";
import { Badge } from "@/components/ui/badge";
import { Progress } from "@/components/ui/progress";
import { motion } from "framer-motion";

import StatsGrid from "../components/dashboard/StatsGrid";
import TokensOverview from "../components/dashboard/TokensOverview";
import ComplianceStatus from "../components/dashboard/ComplianceStatus";
import RecentActivity from "../components/dashboard/RecentActivity";
import QuickActions from "../components/dashboard/QuickActions";

export default function Dashboard() {
  const [tokens, setTokens] = useState([]);
  const [assets, setAssets] = useState([]);
  const [complianceChecks, setComplianceChecks] = useState([]);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    loadDashboardData();
  }, []);

  const loadDashboardData = async () => {
    setIsLoading(true);
    try {
      const [tokensData, assetsData, complianceData] = await Promise.all([
        Token.list('-created_date', 10),
        Asset.list('-created_date', 5),
        ComplianceCheck.list('-created_date', 10)
      ]);
      
      setTokens(tokensData || []);
      setAssets(assetsData || []);
      setComplianceChecks(complianceData || []);
    } catch (error) {
      console.error('Error loading dashboard data:', error);
      setTokens([]);
      setAssets([]);
      setComplianceChecks([]);
    }
    setIsLoading(false);
  };

  const stats = {
    totalTokens: tokens?.length || 0,
    totalAssets: assets?.length || 0,
    portfolioValue: tokens?.reduce((sum, token) => sum + (token?.market_cap || 0), 0) || 0,
    complianceScore: complianceChecks?.length > 0 
      ? Math.round(complianceChecks.reduce((sum, check) => sum + (check?.score || 0), 0) / complianceChecks.length)
      : 0
  };

  return (
    <div className="p-6 space-y-8 bg-gradient-to-br from-gray-50 to-white min-h-screen">
      <div className="max-w-7xl mx-auto">
        {/* Header */}
        <motion.div 
          initial={{ opacity: 0, y: 20 }}
          animate={{ opacity: 1, y: 0 }}
          className="flex flex-col lg:flex-row justify-between items-start lg:items-center gap-6 mb-8"
        >
          <div>
            <h1 className="text-4xl font-bold text-gray-900 mb-2">
              Welcome to <span className="quantum-text-gradient">QuantumMint</span>
            </h1>
            <p className="text-lg text-gray-600">
              Premium tokenization platform for digital and physical assets
            </p>
          </div>
          
          <QuickActions />
        </motion.div>

        {/* Stats Grid */}
        <StatsGrid stats={stats} isLoading={isLoading} />

        {/* Main Dashboard Grid */}
        <div className="grid lg:grid-cols-3 gap-8">
          {/* Left Column - 2/3 width */}
          <div className="lg:col-span-2 space-y-8">
            <TokensOverview tokens={tokens || []} isLoading={isLoading} />
            <RecentActivity tokens={tokens || []} assets={assets || []} isLoading={isLoading} />
          </div>

          {/* Right Column - 1/3 width */}
          <div className="space-y-8">
            <ComplianceStatus complianceChecks={complianceChecks || []} isLoading={isLoading} />
            
            {/* Assets Overview */}
            <motion.div
              initial={{ opacity: 0, x: 20 }}
              animate={{ opacity: 1, x: 0 }}
              transition={{ delay: 0.4 }}
            >
              <Card className="quantum-glow border-0 shadow-xl">
                <CardHeader className="pb-4">
                  <div className="flex items-center justify-between">
                    <CardTitle className="flex items-center gap-2 text-xl">
                      <Building2 className="w-5 h-5 text-blue-600" />
                      Asset Portfolio
                    </CardTitle>
                    <Link to={createPageUrl("AssetManager")}>
                      <Button variant="ghost" size="sm" className="text-blue-600 hover:text-blue-700">
                        View All <ArrowUpRight className="w-4 h-4 ml-1" />
                      </Button>
                    </Link>
                  </div>
                </CardHeader>
                <CardContent>
                  {!isLoading ? (
                    (assets && assets.length > 0) ? (
                      <div className="space-y-4">
                        {assets.slice(0, 3).map((asset) => (
                          <div key={asset.id} className="flex items-center justify-between p-4 bg-gray-50 rounded-xl">
                            <div>
                              <p className="font-medium text-gray-900">{asset.name}</p>
                              <p className="text-sm text-gray-500 capitalize">
                                {asset.type?.replace(/_/g, ' ') || 'Unknown Type'}
                              </p>
                            </div>
                            <div className="text-right">
                              <p className="font-semibold text-gray-900">
                                ${asset.valuation?.toLocaleString() || '0'}
                              </p>
                              <Badge 
                                variant={asset.verification_status === 'verified' ? 'default' : 'secondary'}
                                className="mt-1"
                              >
                                {asset.verification_status || 'pending'}
                              </Badge>
                            </div>
                          </div>
                        ))}
                      </div>
                    ) : (
                      <div className="text-center py-8">
                        <Building2 className="w-12 h-12 text-gray-300 mx-auto mb-4" />
                        <p className="text-gray-500 mb-4">No assets registered yet</p>
                        <Link to={createPageUrl("AssetManager")}>
                          <Button size="sm" className="bg-blue-600 hover:bg-blue-700">
                            Add Asset
                          </Button>
                        </Link>
                      </div>
                    )
                  ) : (
                    <div className="space-y-4">
                      {[1, 2, 3].map((i) => (
                        <div key={i} className="flex items-center justify-between p-4 bg-gray-50 rounded-xl animate-pulse">
                          <div className="space-y-2">
                            <div className="h-4 bg-gray-200 rounded w-24"></div>
                            <div className="h-3 bg-gray-200 rounded w-16"></div>
                          </div>
                          <div className="space-y-2">
                            <div className="h-4 bg-gray-200 rounded w-20"></div>
                            <div className="h-6 bg-gray-200 rounded w-16"></div>
                          </div>
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
    </div>
  );
}