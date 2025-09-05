import React, { useState, useEffect } from "react";
import { Token } from "@/api/entities";
import { Asset } from "@/api/entities";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { motion } from "framer-motion";
import { Link } from "react-router-dom";
import { createPageUrl } from "@/utils";
import { 
  Wallet, 
  TrendingUp, 
  TrendingDown,
  Plus,
  ExternalLink,
  Coins,
  Building2,
  DollarSign,
  BarChart3
} from "lucide-react";

import PortfolioOverview from "../components/portfolio/PortfolioOverview";
import TokenHoldings from "../components/portfolio/TokenHoldings";
import AssetHoldings from "../components/portfolio/AssetHoldings";

export default function Portfolio() {
  const [tokens, setTokens] = useState([]);
  const [assets, setAssets] = useState([]);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    loadPortfolioData();
  }, []);

  const loadPortfolioData = async () => {
    setIsLoading(true);
    try {
      const [tokensData, assetsData] = await Promise.all([
        Token.list('-created_date'),
        Asset.list('-created_date')
      ]);
      setTokens(tokensData);
      setAssets(assetsData);
    } catch (error) {
      console.error('Error loading portfolio data:', error);
    }
    setIsLoading(false);
  };

  const portfolioStats = {
    totalValue: tokens.reduce((sum, token) => sum + (token.market_cap || 0), 0) + 
                assets.reduce((sum, asset) => sum + (asset.valuation || 0), 0),
    tokenValue: tokens.reduce((sum, token) => sum + (token.market_cap || 0), 0),
    assetValue: assets.reduce((sum, asset) => sum + (asset.valuation || 0), 0),
    totalTokens: tokens.length,
    totalAssets: assets.length
  };

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
              Portfolio <span className="quantum-text-gradient">Overview</span>
            </h1>
            <p className="text-gray-600">
              Track your tokens and assets performance
            </p>
          </div>
          
          <div className="flex gap-3">
            <Link to={createPageUrl("TokenCreator")}>
              <Button className="bg-gradient-to-r from-blue-600 to-purple-600">
                <Plus className="w-4 h-4 mr-2" />
                Create Token
              </Button>
            </Link>
            <Link to={createPageUrl("AssetManager")}>
              <Button variant="outline">
                <Plus className="w-4 h-4 mr-2" />
                Add Asset
              </Button>
            </Link>
          </div>
        </motion.div>

        {/* Portfolio Overview */}
        <PortfolioOverview stats={portfolioStats} isLoading={isLoading} />

        {/* Portfolio Tabs */}
        <motion.div
          initial={{ opacity: 0, y: 20 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ delay: 0.3 }}
        >
          <Tabs defaultValue="tokens" className="space-y-6">
            <TabsList className="grid w-full grid-cols-2 lg:w-96">
              <TabsTrigger value="tokens" className="flex items-center gap-2">
                <Coins className="w-4 h-4" />
                Tokens ({tokens.length})
              </TabsTrigger>
              <TabsTrigger value="assets" className="flex items-center gap-2">
                <Building2 className="w-4 h-4" />
                Assets ({assets.length})
              </TabsTrigger>
            </TabsList>
            
            <TabsContent value="tokens" className="space-y-6">
              <TokenHoldings tokens={tokens} isLoading={isLoading} />
            </TabsContent>
            
            <TabsContent value="assets" className="space-y-6">
              <AssetHoldings assets={assets} isLoading={isLoading} />
            </TabsContent>
          </Tabs>
        </motion.div>
      </div>
    </div>
  );
}