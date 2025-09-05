import React, { useState, useEffect } from "react";
import { Block, Transaction, Token, Address } from "@/api/entities";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { 
  BarChart, 
  Bar, 
  XAxis, 
  YAxis, 
  CartesianGrid, 
  Tooltip, 
  ResponsiveContainer,
  LineChart,
  Line,
  PieChart,
  Pie,
  Cell,
  Area,
  AreaChart
} from "recharts";
import { 
  BarChart3, 
  TrendingUp, 
  Activity, 
  Zap, 
  Users, 
  Coins,
  ArrowRightLeft,
  Clock
} from "lucide-react";
import DataCard from "../components/ui/DataCard";
import { format, subDays, startOfDay } from "date-fns";

const COLORS = ['#3b82f6', '#10b981', '#f59e0b', '#ef4444', '#8b5cf6', '#06b6d4'];

export default function AnalyticsPage() {
  const [timeRange, setTimeRange] = useState("7d");
  const [isLoading, setIsLoading] = useState(true);
  const [analyticsData, setAnalyticsData] = useState({
    networkStats: {
      totalBlocks: 0,
      totalTransactions: 0,
      totalAddresses: 0,
      totalTokens: 0,
      avgBlockTime: 0,
      networkHashrate: 0,
      tps: 0,
      activeValidators: 0
    },
    transactionTrends: [],
    blockTrends: [],
    tokenDistribution: [],
    addressTypeDistribution: [],
    gasUsageData: [],
    networkActivity: []
  });

  useEffect(() => {
    loadAnalyticsData();
  }, [timeRange]);

  const loadAnalyticsData = async () => {
    setIsLoading(true);
    try {
      const [blocks, transactions, tokens, addresses] = await Promise.all([
        Block.list('-created_date'),
        Transaction.list('-timestamp'),
        Token.list(),
        Address.list()
      ]);

      // Calculate network stats
      const networkStats = {
        totalBlocks: blocks.length,
        totalTransactions: transactions.length,
        totalAddresses: addresses.length,
        totalTokens: tokens.length,
        avgBlockTime: calculateAvgBlockTime(blocks),
        networkHashrate: 156.7, // Mock data
        tps: 45.2, // Mock data
        activeValidators: addresses.filter(a => a.address_type === 'validator').length
      };

      // Generate time-based data
      const transactionTrends = generateTimeSeriesData(transactions, timeRange, 'transactions');
      const blockTrends = generateTimeSeriesData(blocks, timeRange, 'blocks');
      const gasUsageData = generateGasUsageData(blocks, timeRange);
      const networkActivity = generateNetworkActivityData(transactions, timeRange);

      // Generate distribution data
      const tokenDistribution = generateTokenDistribution(tokens);
      const addressTypeDistribution = generateAddressTypeDistribution(addresses);

      setAnalyticsData({
        networkStats,
        transactionTrends,
        blockTrends,
        tokenDistribution,
        addressTypeDistribution,
        gasUsageData,
        networkActivity
      });
    } catch (error) {
      console.error('Error loading analytics data:', error);
    } finally {
      setIsLoading(false);
    }
  };

  const calculateAvgBlockTime = (blocks) => {
    if (blocks.length < 2) return 0;
    let totalTimeDiff = 0;
    for (let i = 0; i < blocks.length - 1; i++) {
      const timeDiff = new Date(blocks[i].timestamp) - new Date(blocks[i+1].timestamp);
      totalTimeDiff += Math.abs(timeDiff);
    }
    return (totalTimeDiff / (blocks.length - 1) / 1000).toFixed(2);
  };

  const generateTimeSeriesData = (data, range, type) => {
    const days = range === '7d' ? 7 : range === '30d' ? 30 : 90;
    const result = [];
    
    for (let i = days - 1; i >= 0; i--) {
      const date = startOfDay(subDays(new Date(), i));
      const dayData = data.filter(item => {
        const itemDate = startOfDay(new Date(item.timestamp || item.created_date));
        return itemDate.getTime() === date.getTime();
      });
      
      result.push({
        date: format(date, 'MMM dd'),
        value: dayData.length,
        fullDate: date
      });
    }
    
    return result;
  };

  const generateGasUsageData = (blocks, range) => {
    const days = range === '7d' ? 7 : range === '30d' ? 30 : 90;
    const result = [];
    
    for (let i = days - 1; i >= 0; i--) {
      const date = startOfDay(subDays(new Date(), i));
      const dayBlocks = blocks.filter(block => {
        const blockDate = startOfDay(new Date(block.timestamp));
        return blockDate.getTime() === date.getTime();
      });
      
      const avgGasUsed = dayBlocks.length > 0 
        ? dayBlocks.reduce((sum, block) => sum + (block.gas_used || 0), 0) / dayBlocks.length
        : 0;
      
      result.push({
        date: format(date, 'MMM dd'),
        gasUsed: Math.round(avgGasUsed),
        gasLimit: 15000000 // Mock gas limit
      });
    }
    
    return result;
  };

  const generateNetworkActivityData = (transactions, range) => {
    const days = range === '7d' ? 7 : range === '30d' ? 30 : 90;
    const result = [];
    
    for (let i = days - 1; i >= 0; i--) {
      const date = startOfDay(subDays(new Date(), i));
      const dayTxs = transactions.filter(tx => {
        const txDate = startOfDay(new Date(tx.timestamp));
        return txDate.getTime() === date.getTime();
      });
      
      const confirmed = dayTxs.filter(tx => tx.status === 'confirmed').length;
      const pending = dayTxs.filter(tx => tx.status === 'pending').length;
      const failed = dayTxs.filter(tx => tx.status === 'failed').length;
      
      result.push({
        date: format(date, 'MMM dd'),
        confirmed,
        pending,
        failed
      });
    }
    
    return result;
  };

  const generateTokenDistribution = (tokens) => {
    const distribution = {};
    tokens.forEach(token => {
      distribution[token.token_type] = (distribution[token.token_type] || 0) + 1;
    });
    
    return Object.entries(distribution).map(([type, count]) => ({
      name: type,
      value: count,
      percentage: ((count / tokens.length) * 100).toFixed(1)
    }));
  };

  const generateAddressTypeDistribution = (addresses) => {
    const distribution = {};
    addresses.forEach(address => {
      distribution[address.address_type] = (distribution[address.address_type] || 0) + 1;
    });
    
    return Object.entries(distribution).map(([type, count]) => ({
      name: type,
      value: count,
      percentage: ((count / addresses.length) * 100).toFixed(1)
    }));
  };

  const formatNumber = (num) => {
    if (!num) return '0';
    if (num >= 1e9) return (num / 1e9).toFixed(2) + 'B';
    if (num >= 1e6) return (num / 1e6).toFixed(2) + 'M';
    if (num >= 1e3) return (num / 1e3).toFixed(2) + 'K';
    return num.toLocaleString();
  };

  const CustomTooltip = ({ active, payload, label }) => {
    if (active && payload && payload.length) {
      return (
        <div className="bg-white/95 backdrop-blur-sm border border-gray-200 rounded-lg p-3 shadow-lg">
          <p className="font-medium text-gray-900">{`${label}`}</p>
          {payload.map((entry, index) => (
            <p key={index} className="text-sm" style={{ color: entry.color }}>
              {`${entry.dataKey}: ${formatNumber(entry.value)}`}
            </p>
          ))}
        </div>
      );
    }
    return null;
  };

  return (
    <div className="p-6 space-y-8">
      {/* Header */}
      <div className="flex flex-col lg:flex-row justify-between items-start lg:items-center gap-6">
        <div>
          <h1 className="text-4xl font-bold text-slate-900 mb-2">
            Network Analytics
          </h1>
          <p className="text-lg text-slate-600">
            Comprehensive insights and metrics for the Quantum Network
          </p>
        </div>
        <Select value={timeRange} onValueChange={setTimeRange}>
          <SelectTrigger className="w-48 bg-white/80">
            <SelectValue placeholder="Time Range" />
          </SelectTrigger>
          <SelectContent>
            <SelectItem value="7d">Last 7 Days</SelectItem>
            <SelectItem value="30d">Last 30 Days</SelectItem>
            <SelectItem value="90d">Last 90 Days</SelectItem>
          </SelectContent>
        </Select>
      </div>

      {/* Network Overview Stats */}
      <div className="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-4 gap-6">
        <DataCard
          title="Network TPS"
          value={analyticsData.networkStats.tps}
          subtitle="Transactions per second"
          icon={Zap}
          trend="+5.2% from yesterday"
          trendDirection="up"
        />
        <DataCard
          title="Block Time"
          value={`${analyticsData.networkStats.avgBlockTime}s`}
          subtitle="Average block time"
          icon={Clock}
        />
        <DataCard
          title="Active Validators"
          value={analyticsData.networkStats.activeValidators}
          subtitle="Securing the network"
          icon={Users}
        />
        <DataCard
          title="Network Hashrate"
          value={`${analyticsData.networkStats.networkHashrate} TH/s`}
          subtitle="Total network power"
          icon={BarChart3}
          trend="+12.3% this week"
          trendDirection="up"
        />
      </div>

      {/* Transaction Trends */}
      <div className="grid lg:grid-cols-2 gap-8">
        <Card className="bg-white/60 backdrop-blur-sm border-white/80 shadow-lg">
          <CardHeader>
            <CardTitle className="flex items-center gap-3">
              <ArrowRightLeft className="w-6 h-6 text-blue-600" />
              Transaction Volume
            </CardTitle>
          </CardHeader>
          <CardContent>
            <ResponsiveContainer width="100%" height={300}>
              <AreaChart data={analyticsData.transactionTrends}>
                <CartesianGrid strokeDasharray="3 3" stroke="#e2e8f0" />
                <XAxis dataKey="date" stroke="#64748b" fontSize={12} />
                <YAxis stroke="#64748b" fontSize={12} />
                <Tooltip content={<CustomTooltip />} />
                <Area 
                  type="monotone" 
                  dataKey="value" 
                  stroke="#3b82f6" 
                  fill="#3b82f6" 
                  fillOpacity={0.2}
                  strokeWidth={2}
                />
              </AreaChart>
            </ResponsiveContainer>
          </CardContent>
        </Card>

        <Card className="bg-white/60 backdrop-blur-sm border-white/80 shadow-lg">
          <CardHeader>
            <CardTitle className="flex items-center gap-3">
              <Activity className="w-6 h-6 text-blue-600" />
              Block Production
            </CardTitle>
          </CardHeader>
          <CardContent>
            <ResponsiveContainer width="100%" height={300}>
              <LineChart data={analyticsData.blockTrends}>
                <CartesianGrid strokeDasharray="3 3" stroke="#e2e8f0" />
                <XAxis dataKey="date" stroke="#64748b" fontSize={12} />
                <YAxis stroke="#64748b" fontSize={12} />
                <Tooltip content={<CustomTooltip />} />
                <Line 
                  type="monotone" 
                  dataKey="value" 
                  stroke="#10b981" 
                  strokeWidth={3}
                  dot={{ fill: '#10b981', strokeWidth: 2, r: 4 }}
                />
              </LineChart>
            </ResponsiveContainer>
          </CardContent>
        </Card>
      </div>

      {/* Gas Usage and Network Activity */}
      <div className="grid lg:grid-cols-2 gap-8">
        <Card className="bg-white/60 backdrop-blur-sm border-white/80 shadow-lg">
          <CardHeader>
            <CardTitle className="flex items-center gap-3">
              <Zap className="w-6 h-6 text-blue-600" />
              Gas Usage Trends
            </CardTitle>
          </CardHeader>
          <CardContent>
            <ResponsiveContainer width="100%" height={300}>
              <BarChart data={analyticsData.gasUsageData}>
                <CartesianGrid strokeDasharray="3 3" stroke="#e2e8f0" />
                <XAxis dataKey="date" stroke="#64748b" fontSize={12} />
                <YAxis stroke="#64748b" fontSize={12} />
                <Tooltip content={<CustomTooltip />} />
                <Bar dataKey="gasUsed" fill="#f59e0b" radius={[4, 4, 0, 0]} />
              </BarChart>
            </ResponsiveContainer>
          </CardContent>
        </Card>

        <Card className="bg-white/60 backdrop-blur-sm border-white/80 shadow-lg">
          <CardHeader>
            <CardTitle className="flex items-center gap-3">
              <TrendingUp className="w-6 h-6 text-blue-600" />
              Transaction Status Distribution
            </CardTitle>
          </CardHeader>
          <CardContent>
            <ResponsiveContainer width="100%" height={300}>
              <AreaChart data={analyticsData.networkActivity}>
                <CartesianGrid strokeDasharray="3 3" stroke="#e2e8f0" />
                <XAxis dataKey="date" stroke="#64748b" fontSize={12} />
                <YAxis stroke="#64748b" fontSize={12} />
                <Tooltip content={<CustomTooltip />} />
                <Area type="monotone" dataKey="confirmed" stackId="1" stroke="#10b981" fill="#10b981" fillOpacity={0.8} />
                <Area type="monotone" dataKey="pending" stackId="1" stroke="#f59e0b" fill="#f59e0b" fillOpacity={0.8} />
                <Area type="monotone" dataKey="failed" stackId="1" stroke="#ef4444" fill="#ef4444" fillOpacity={0.8} />
              </AreaChart>
            </ResponsiveContainer>
          </CardContent>
        </Card>
      </div>

      {/* Distribution Charts */}
      <div className="grid lg:grid-cols-2 gap-8">
        <Card className="bg-white/60 backdrop-blur-sm border-white/80 shadow-lg">
          <CardHeader>
            <CardTitle className="flex items-center gap-3">
              <Coins className="w-6 h-6 text-blue-600" />
              Token Type Distribution
            </CardTitle>
          </CardHeader>
          <CardContent>
            <ResponsiveContainer width="100%" height={300}>
              <PieChart>
                <Pie
                  data={analyticsData.tokenDistribution}
                  cx="50%"
                  cy="50%"
                  outerRadius={100}
                  fill="#8884d8"
                  dataKey="value"
                  label={({ name, percentage }) => `${name}: ${percentage}%`}
                  labelLine={false}
                >
                  {analyticsData.tokenDistribution.map((entry, index) => (
                    <Cell key={`cell-${index}`} fill={COLORS[index % COLORS.length]} />
                  ))}
                </Pie>
                <Tooltip />
              </PieChart>
            </ResponsiveContainer>
          </CardContent>
        </Card>

        <Card className="bg-white/60 backdrop-blur-sm border-white/80 shadow-lg">
          <CardHeader>
            <CardTitle className="flex items-center gap-3">
              <Users className="w-6 h-6 text-blue-600" />
              Address Type Distribution
            </CardTitle>
          </CardHeader>
          <CardContent>
            <ResponsiveContainer width="100%" height={300}>
              <PieChart>
                <Pie
                  data={analyticsData.addressTypeDistribution}
                  cx="50%"
                  cy="50%"
                  outerRadius={100}
                  fill="#8884d8"
                  dataKey="value"
                  label={({ name, percentage }) => `${name}: ${percentage}%`}
                  labelLine={false}
                >
                  {analyticsData.addressTypeDistribution.map((entry, index) => (
                    <Cell key={`cell-${index}`} fill={COLORS[index % COLORS.length]} />
                  ))}
                </Pie>
                <Tooltip />
              </PieChart>
            </ResponsiveContainer>
          </CardContent>
        </Card>
      </div>
    </div>
  );
}