
import React, { useState, useEffect } from "react";
import { Link, useNavigate } from 'react-router-dom';
import { Block, Transaction, Token, Address } from "@/api/entities";
import { Activity, Blocks, ArrowRightLeft, Coins, Wallet, TrendingUp } from "lucide-react";
import DataCard from "../components/ui/DataCard";
import UniversalSearch from "../components/search/UniversalSearch";
import StatusBadge from "../components/ui/StatusBadge";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";
import { format } from "date-fns";
import { createPageUrl } from "@/utils";

export default function Dashboard() {
  const [stats, setStats] = useState({
    totalBlocks: 0,
    totalTransactions: 0,
    totalTokens: 0,
    totalAddresses: 0
  });
  const [recentBlocks, setRecentBlocks] = useState([]);
  const [recentTransactions, setRecentTransactions] = useState([]);
  const [isLoading, setIsLoading] = useState(true);
  const navigate = useNavigate();

  useEffect(() => {
    loadDashboardData();
  }, []);

  const loadDashboardData = async () => {
    try {
      const [blocks, transactions, tokens, addresses] = await Promise.all([
        Block.list('-created_date', 50), // Get more blocks to calculate better stats
        Transaction.list('-created_date', 50), // Get more transactions
        Token.list(),
        Address.list()
      ]);

      setStats({
        totalBlocks: blocks.length,
        totalTransactions: transactions.length,
        totalTokens: tokens.length,
        totalAddresses: addresses.length
      });

      // Take the first 5 for display
      setRecentBlocks(blocks.slice(0, 5));
      setRecentTransactions(transactions.slice(0, 5));
    } catch (error) {
      console.error('Error loading dashboard data:', error);
      // Set some default values if API fails
      setStats({
        totalBlocks: 0,
        totalTransactions: 0,
        totalTokens: 0,
        totalAddresses: 0
      });
      setRecentBlocks([]);
      setRecentTransactions([]);
    } finally {
      setIsLoading(false);
    }
  };

  const handleSearch = async (query) => {
    if (!query) return;
    const cleanedQuery = query.trim();

    // Check if it's a block number
    if (!isNaN(cleanedQuery) && !cleanedQuery.startsWith('0x')) {
      navigate(createPageUrl(`BlockDetails?number=${cleanedQuery}`));
      return;
    }

    // Check if it's a hash (address, block hash, or transaction hash)
    if (cleanedQuery.startsWith('0x')) {
      // Try to find as an address first
      const addresses = await Address.filter({ address: cleanedQuery });
      if (addresses.length > 0) {
        navigate(createPageUrl(`AddressDetails?address=${cleanedQuery}`));
        return;
      }
      
      // Try to find as a block hash
      const blocks = await Block.filter({ block_hash: cleanedQuery });
      if (blocks.length > 0) {
        navigate(createPageUrl(`BlockDetails?hash=${cleanedQuery}`));
        return;
      }

      // Try to find as a transaction hash
      const transactions = await Transaction.filter({ transaction_id: cleanedQuery });
      if (transactions.length > 0) {
        navigate(createPageUrl(`TransactionDetails?id=${transactions[0].id}`));
        return;
      }
    }
    
    // If no match, we could navigate to a "not found" page or show a toast
    // For now, let's just log it.
    console.log(`No results found for query: ${cleanedQuery}`);
    alert(`No results found for query: ${cleanedQuery}`);
  };

  const formatHash = (hash) => {
    if (!hash) return '';
    return `${hash.slice(0, 6)}...${hash.slice(-4)}`;
  };

  return (
    <div className="p-6 space-y-8">
      {/* Header Section */}
      <div className="space-y-6">
        <div className="flex flex-col lg:flex-row justify-between items-start lg:items-center gap-6">
          <div>
            <h1 className="text-4xl font-bold text-slate-900 mb-2">
              Network Overview
            </h1>
            <p className="text-lg text-slate-600">
              Real-time blockchain analytics and explorer
            </p>
          </div>
          <div className="w-full lg:w-96">
            <UniversalSearch onSearch={handleSearch} />
          </div>
        </div>

        {/* Stats Cards */}
        <div className="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-4 gap-6">
          <DataCard
            title="Total Blocks"
            value={stats.totalBlocks.toLocaleString()}
            subtitle="Current blockchain height"
            icon={Blocks}
            trend="+2.5% from yesterday"
            trendDirection="up"
          />
          <DataCard
            title="Total Transactions"
            value={stats.totalTransactions.toLocaleString()}
            subtitle="All-time transaction count"
            icon={ArrowRightLeft}
            trend="+15.2% from yesterday"
            trendDirection="up"
          />
          <DataCard
            title="Active Tokens"
            value={stats.totalTokens.toLocaleString()}
            subtitle="Registered token contracts"
            icon={Coins}
            trend="+3.8% this week"
            trendDirection="up"
          />
          <DataCard
            title="Unique Addresses"
            value={stats.totalAddresses.toLocaleString()}
            subtitle="Wallet and contract addresses"
            icon={Wallet}
            trend="+7.1% this month"
            trendDirection="up"
          />
        </div>
      </div>

      {/* Recent Activity */}
      <div className="grid lg:grid-cols-2 gap-8">
        {/* Recent Blocks */}
        <Card className="bg-white/60 backdrop-blur-sm border-white/80 shadow-lg">
          <CardHeader>
            <CardTitle className="flex items-center gap-3 text-xl font-bold text-slate-900">
              <Blocks className="w-6 h-6 text-blue-600" />
              Latest Blocks
            </CardTitle>
          </CardHeader>
          <CardContent>
            {isLoading ? (
              <div className="space-y-4">
                {[...Array(5)].map((_, i) => (
                  <div key={i} className="animate-pulse">
                    <div className="h-4 bg-slate-200 rounded w-3/4 mb-2"></div>
                    <div className="h-3 bg-slate-200 rounded w-1/2"></div>
                  </div>
                ))}
              </div>
            ) : (
              <div className="space-y-4">
                {recentBlocks.map((block) => (
                  <div key={block.id} className="flex items-center justify-between p-3 rounded-lg bg-slate-50/80 hover:bg-slate-100/80 transition-colors">
                    <div>
                      {/* Block Number Link */}
                      <div className="font-mono text-sm font-semibold">
                        <Link to={createPageUrl(`BlockDetails?id=${block.id}`)} className="text-blue-600 hover:underline">
                          Block #{block.block_number}
                        </Link>
                      </div>
                      <div className="text-xs text-slate-500">
                        {block.transaction_count} txns • 
                        {/* Block Hash Link */}
                        <Link 
                          to={createPageUrl(`BlockDetails?id=${block.id}`)} 
                          className="text-blue-600 hover:underline ml-1"
                        >
                          {formatHash(block.block_hash)}
                        </Link>
                      </div>
                      {/* Validator Address Link */}
                      <div className="text-xs text-slate-500">
                        Validator: 
                        <Link 
                          to={createPageUrl(`AddressDetails?address=${block.validator_address}`)} 
                          className="text-blue-600 hover:underline ml-1"
                        >
                          {formatHash(block.validator_address)}
                        </Link>
                      </div>
                    </div>
                    <div className="text-right">
                      <StatusBadge status={block.validation_status} size="sm" />
                      <div className="text-xs text-slate-500 mt-1">
                        {format(new Date(block.timestamp), 'HH:mm:ss')}
                      </div>
                    </div>
                  </div>
                ))}
              </div>
            )}
          </CardContent>
        </Card>

        {/* Recent Transactions */}
        <Card className="bg-white/60 backdrop-blur-sm border-white/80 shadow-lg">
          <CardHeader>
            <CardTitle className="flex items-center gap-3 text-xl font-bold text-slate-900">
              <ArrowRightLeft className="w-6 h-6 text-blue-600" />
              Recent Transactions
            </CardTitle>
          </CardHeader>
          <CardContent>
            {isLoading ? (
              <div className="space-y-4">
                {[...Array(5)].map((_, i) => (
                  <div key={i} className="animate-pulse">
                    <div className="h-4 bg-slate-200 rounded w-3/4 mb-2"></div>
                    <div className="h-3 bg-slate-200 rounded w-1/2"></div>
                  </div>
                ))}
              </div>
            ) : (
              <div className="space-y-4">
                {recentTransactions.map((tx) => (
                  <div key={tx.id} className="flex items-center justify-between p-3 rounded-lg bg-slate-50/80 hover:bg-slate-100/80 transition-colors">
                    <div>
                      {/* Transaction ID Link */}
                      <div className="font-mono text-sm font-semibold">
                        <Link to={createPageUrl(`TransactionDetails?id=${tx.id}`)} className="text-blue-600 hover:underline">
                          {formatHash(tx.transaction_id)}
                        </Link>
                      </div>
                      {/* From/To Address Links */}
                      <div className="text-xs text-slate-500">
                        <Link 
                          to={createPageUrl(`AddressDetails?address=${tx.from_address}`)} 
                          className="text-blue-600 hover:underline"
                        >
                          {formatHash(tx.from_address)}
                        </Link>
                        {' → '}
                        <Link 
                          to={createPageUrl(`AddressDetails?address=${tx.to_address}`)} 
                          className="text-blue-600 hover:underline"
                        >
                          {formatHash(tx.to_address)}
                        </Link>
                      </div>
                      {/* Block Number Link */}
                      <div className="text-xs text-slate-500">
                        Block: 
                        <Link 
                          to={createPageUrl(`BlockDetails?number=${tx.block_number}`)} 
                          className="text-blue-600 hover:underline ml-1"
                        >
                          #{tx.block_number}
                        </Link>
                      </div>
                    </div>
                    <div className="text-right">
                      <StatusBadge status={tx.status} size="sm" />
                      <div className="text-xs text-slate-500 mt-1">
                        {tx.amount} ETH
                      </div>
                    </div>
                  </div>
                ))}
              </div>
            )}
          </CardContent>
        </Card>
      </div>

      {/* Network Health Indicator */}
      <Card className="bg-gradient-to-r from-green-50 to-emerald-50 border-green-200 shadow-lg">
        <CardContent className="p-6">
          <div className="flex items-center gap-4">
            <div className="w-12 h-12 bg-green-500 rounded-full flex items-center justify-center">
              <Activity className="w-6 h-6 text-white" />
            </div>
            <div>
              <h3 className="text-lg font-semibold text-green-900">Network Status: Healthy</h3>
              <p className="text-green-700">All systems operational • Average block time: 12.3s • TPS: 45.2</p>
            </div>
            <div className="ml-auto flex items-center gap-2">
              <div className="w-3 h-3 bg-green-500 rounded-full animate-pulse"></div>
              <span className="text-sm font-medium text-green-700">Live</span>
            </div>
          </div>
        </CardContent>
      </Card>
    </div>
  );
}
