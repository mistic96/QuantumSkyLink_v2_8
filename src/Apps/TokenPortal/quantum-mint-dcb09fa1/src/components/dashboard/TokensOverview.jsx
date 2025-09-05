import React from "react";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { motion } from "framer-motion";
import { Link } from "react-router-dom";
import { createPageUrl } from "@/utils";
import { 
  Coins, 
  ArrowUpRight, 
  TrendingUp, 
  TrendingDown,
  Circle
} from "lucide-react";

const statusColors = {
  draft: "bg-gray-100 text-gray-800",
  pending_review: "bg-yellow-100 text-yellow-800",
  approved: "bg-green-100 text-green-800",
  deployed: "bg-blue-100 text-blue-800",
  rejected: "bg-red-100 text-red-800"
};

const typeColors = {
  ERC20: "bg-purple-100 text-purple-800",
  ERC721: "bg-pink-100 text-pink-800",
  ERC1155: "bg-indigo-100 text-indigo-800",
  "Asset-Backed": "bg-emerald-100 text-emerald-800"
};

export default function TokensOverview({ tokens = [], isLoading = false }) {
  const safeTokens = Array.isArray(tokens) ? tokens : [];

  return (
    <motion.div
      initial={{ opacity: 0, y: 20 }}
      animate={{ opacity: 1, y: 0 }}
      transition={{ delay: 0.2 }}
    >
      <Card className="quantum-glow border-0 shadow-xl">
        <CardHeader className="pb-4">
          <div className="flex items-center justify-between">
            <CardTitle className="flex items-center gap-2 text-xl">
              <Coins className="w-5 h-5 text-blue-600" />
              Token Portfolio
            </CardTitle>
            <Link to={createPageUrl("TokenCreator")}>
              <Button className="bg-gradient-to-r from-blue-600 to-purple-600 hover:from-blue-700 hover:to-purple-700">
                Create Token <ArrowUpRight className="w-4 h-4 ml-1" />
              </Button>
            </Link>
          </div>
        </CardHeader>
        <CardContent>
          {!isLoading ? (
            safeTokens.length > 0 ? (
              <div className="space-y-4">
                {safeTokens.slice(0, 5).map((token, index) => (
                  <motion.div
                    key={token?.id || index}
                    initial={{ opacity: 0, x: -20 }}
                    animate={{ opacity: 1, x: 0 }}
                    transition={{ delay: 0.1 * index }}
                    className="flex items-center justify-between p-4 bg-gradient-to-r from-gray-50 to-white rounded-xl border border-gray-100 hover:shadow-md transition-all duration-300"
                  >
                    <div className="flex items-center gap-4">
                      <div className="w-12 h-12 bg-gradient-to-r from-blue-500 to-purple-500 rounded-xl flex items-center justify-center">
                        <span className="text-white font-bold text-sm">
                          {token?.symbol?.substring(0, 2) || 'TK'}
                        </span>
                      </div>
                      <div>
                        <h3 className="font-semibold text-gray-900">{token?.name || 'Unknown Token'}</h3>
                        <div className="flex items-center gap-2 mt-1">
                          <Badge className={typeColors[token?.token_type] || 'bg-gray-100 text-gray-800'} variant="secondary">
                            {token?.token_type || 'Unknown'}
                          </Badge>
                          <Badge className={statusColors[token?.status] || 'bg-gray-100 text-gray-800'} variant="secondary">
                            <Circle className="w-2 h-2 mr-1" />
                            {token?.status?.replace(/_/g, ' ') || 'draft'}
                          </Badge>
                        </div>
                      </div>
                    </div>
                    <div className="text-right">
                      <p className="font-semibold text-gray-900">
                        {(token?.total_supply || 0).toLocaleString()} {token?.symbol || 'TKN'}
                      </p>
                      {(token?.market_cap || 0) > 0 && (
                        <div className="flex items-center gap-1 mt-1">
                          <TrendingUp className="w-3 h-3 text-green-500" />
                          <span className="text-sm text-green-600 font-medium">
                            ${(token.market_cap || 0).toLocaleString()}
                          </span>
                        </div>
                      )}
                    </div>
                  </motion.div>
                ))}
              </div>
            ) : (
              <div className="text-center py-12">
                <div className="w-16 h-16 bg-gradient-to-r from-blue-500 to-purple-500 rounded-full flex items-center justify-center mx-auto mb-4">
                  <Coins className="w-8 h-8 text-white" />
                </div>
                <h3 className="text-lg font-semibold text-gray-900 mb-2">No tokens created yet</h3>
                <p className="text-gray-500 mb-6">
                  Start your tokenization journey by creating your first token
                </p>
                <Link to={createPageUrl("TokenCreator")}>
                  <Button className="bg-gradient-to-r from-blue-600 to-purple-600 hover:from-blue-700 hover:to-purple-700">
                    Create Your First Token
                  </Button>
                </Link>
              </div>
            )
          ) : (
            <div className="space-y-4">
              {[1, 2, 3, 4, 5].map((i) => (
                <div key={i} className="flex items-center justify-between p-4 bg-gray-50 rounded-xl animate-pulse">
                  <div className="flex items-center gap-4">
                    <div className="w-12 h-12 bg-gray-200 rounded-xl"></div>
                    <div className="space-y-2">
                      <div className="h-4 bg-gray-200 rounded w-32"></div>
                      <div className="flex gap-2">
                        <div className="h-6 bg-gray-200 rounded w-16"></div>
                        <div className="h-6 bg-gray-200 rounded w-20"></div>
                      </div>
                    </div>
                  </div>
                  <div className="space-y-2">
                    <div className="h-4 bg-gray-200 rounded w-24"></div>
                    <div className="h-4 bg-gray-200 rounded w-16"></div>
                  </div>
                </div>
              ))}
            </div>
          )}
        </CardContent>
      </Card>
    </motion.div>
  );
}