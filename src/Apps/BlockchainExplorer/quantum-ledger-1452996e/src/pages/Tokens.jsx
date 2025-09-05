import React, { useState, useEffect } from "react";
import { Token } from "@/api/entities";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";
import { Input } from "@/components/ui/input";
import { Button } from "@/components/ui/button";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { Badge } from "@/components/ui/badge";
import { Search, Filter, Coins, TrendingUp, Users, Clock, ShoppingCart } from "lucide-react";
import StatusBadge from "../components/ui/StatusBadge";
import DataCard from "../components/ui/DataCard";
import QuickBuyModal from "../components/tokens/QuickBuyModal";
import { format } from "date-fns";
import { Link } from "react-router-dom";
import { createPageUrl } from "@/utils";

export default function Tokens() {
  const [tokens, setTokens] = useState([]);
  const [filteredTokens, setFilteredTokens] = useState([]);
  const [searchQuery, setSearchQuery] = useState("");
  const [statusFilter, setStatusFilter] = useState("all");
  const [typeFilter, setTypeFilter] = useState("all");
  const [assetFilter, setAssetFilter] = useState("all");
  const [isLoading, setIsLoading] = useState(true);
  const [selectedToken, setSelectedToken] = useState(null);
  const [showBuyModal, setShowBuyModal] = useState(false);
  const [stats, setStats] = useState({
    total: 0,
    active: 0,
    pending: 0,
    approved: 0
  });

  useEffect(() => {
    loadTokens();
  }, []);

  useEffect(() => {
    filterTokens();
  }, [tokens, searchQuery, statusFilter, typeFilter, assetFilter]);

  const loadTokens = async () => {
    try {
      const tokenData = await Token.list('-created_date');
      setTokens(tokenData);
      
      // Calculate stats
      const stats = {
        total: tokenData.length,
        active: tokenData.filter(t => t.status === 'active').length,
        pending: tokenData.filter(t => t.status === 'pending').length,
        approved: tokenData.filter(t => t.approval_status === 'approved').length
      };
      setStats(stats);
    } catch (error) {
      console.error('Error loading tokens:', error);
    } finally {
      setIsLoading(false);
    }
  };

  const filterTokens = () => {
    let filtered = [...tokens];

    // Search filter
    if (searchQuery.trim()) {
      filtered = filtered.filter(token => 
        token.name?.toLowerCase().includes(searchQuery.toLowerCase()) ||
        token.symbol?.toLowerCase().includes(searchQuery.toLowerCase()) ||
        token.creator_id?.toLowerCase().includes(searchQuery.toLowerCase())
      );
    }

    // Status filter
    if (statusFilter !== "all") {
      filtered = filtered.filter(token => token.status === statusFilter);
    }

    // Type filter
    if (typeFilter !== "all") {
      filtered = filtered.filter(token => token.token_type === typeFilter);
    }

    // Asset type filter
    if (assetFilter !== "all") {
      filtered = filtered.filter(token => token.asset_type === assetFilter);
    }

    setFilteredTokens(filtered);
  };

  const formatNumber = (num) => {
    if (!num) return '0';
    if (num >= 1e9) return (num / 1e9).toFixed(2) + 'B';
    if (num >= 1e6) return (num / 1e6).toFixed(2) + 'M';
    if (num >= 1e3) return (num / 1e3).toFixed(2) + 'K';
    return num.toLocaleString();
  };

  const formatAddress = (address) => {
    if (!address) return '';
    return `${address.slice(0, 6)}...${address.slice(-4)}`;
  };

  const getTokenTypeColor = (type) => {
    const colors = {
      'ERC20': 'bg-blue-100 text-blue-800 border-blue-200',
      'ERC721': 'bg-purple-100 text-purple-800 border-purple-200',
      'ERC1155': 'bg-indigo-100 text-indigo-800 border-indigo-200'
    };
    return colors[type] || 'bg-gray-100 text-gray-800 border-gray-200';
  };

  const getAssetTypeColor = (type) => {
    const colors = {
      'real_estate': 'bg-green-100 text-green-800 border-green-200',
      'commodity': 'bg-yellow-100 text-yellow-800 border-yellow-200',
      'security': 'bg-red-100 text-red-800 border-red-200',
      'digital': 'bg-cyan-100 text-cyan-800 border-cyan-200'
    };
    return colors[type] || 'bg-gray-100 text-gray-800 border-gray-200';
  };

  const handleQuickBuy = (token) => {
    setSelectedToken(token);
    setShowBuyModal(true);
  };

  const canBuyToken = (token) => {
    return token.status === 'active' && token.token_type === 'ERC20';
  };

  return (
    <div className="p-6 space-y-8">
      {/* Header Section */}
      <div className="space-y-6">
        <div>
          <h1 className="text-4xl font-bold text-slate-900 mb-2">
            Token Directory
          </h1>
          <p className="text-lg text-slate-600">
            Comprehensive registry of all tokens on the Quantum Network
          </p>
        </div>

        {/* Stats Cards */}
        <div className="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-4 gap-6">
          <DataCard
            title="Total Tokens"
            value={stats.total.toLocaleString()}
            subtitle="All registered tokens"
            icon={Coins}
            trend="+12% this month"
            trendDirection="up"
          />
          <DataCard
            title="Active Tokens"
            value={stats.active.toLocaleString()}
            subtitle="Currently active"
            icon={TrendingUp}
            trend="+8% this week"
            trendDirection="up"
          />
          <DataCard
            title="Pending Approval"
            value={stats.pending.toLocaleString()}
            subtitle="Awaiting review"
            icon={Clock}
          />
          <DataCard
            title="Approved Tokens"
            value={stats.approved.toLocaleString()}
            subtitle="Compliance approved"
            icon={Users}
            trend="+5% this week"
            trendDirection="up"
          />
        </div>

        {/* Search and Filters */}
        <Card className="bg-white/60 backdrop-blur-sm border-white/80 shadow-lg">
          <CardHeader>
            <CardTitle className="flex items-center gap-3">
              <Filter className="w-5 h-5 text-blue-600" />
              Search & Filter Tokens
            </CardTitle>
          </CardHeader>
          <CardContent>
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-5 gap-4">
              <div className="relative lg:col-span-2">
                <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 text-slate-400 w-4 h-4" />
                <Input
                  placeholder="Search by name, symbol, or creator..."
                  value={searchQuery}
                  onChange={(e) => setSearchQuery(e.target.value)}
                  className="pl-10 bg-white/80"
                />
              </div>
              
              <Select value={statusFilter} onValueChange={setStatusFilter}>
                <SelectTrigger className="bg-white/80">
                  <SelectValue placeholder="Status" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="all">All Status</SelectItem>
                  <SelectItem value="active">Active</SelectItem>
                  <SelectItem value="pending">Pending</SelectItem>
                  <SelectItem value="suspended">Suspended</SelectItem>
                  <SelectItem value="burned">Burned</SelectItem>
                </SelectContent>
              </Select>

              <Select value={typeFilter} onValueChange={setTypeFilter}>
                <SelectTrigger className="bg-white/80">
                  <SelectValue placeholder="Type" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="all">All Types</SelectItem>
                  <SelectItem value="ERC20">ERC20</SelectItem>
                  <SelectItem value="ERC721">ERC721</SelectItem>
                  <SelectItem value="ERC1155">ERC1155</SelectItem>
                </SelectContent>
              </Select>

              <Select value={assetFilter} onValueChange={setAssetFilter}>
                <SelectTrigger className="bg-white/80">
                  <SelectValue placeholder="Asset Type" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="all">All Assets</SelectItem>
                  <SelectItem value="real_estate">Real Estate</SelectItem>
                  <SelectItem value="commodity">Commodity</SelectItem>
                  <SelectItem value="security">Security</SelectItem>
                  <SelectItem value="digital">Digital</SelectItem>
                </SelectContent>
              </Select>
            </div>
          </CardContent>
        </Card>

        {/* Token Table */}
        <Card className="bg-white/60 backdrop-blur-sm border-white/80 shadow-lg">
          <CardHeader>
            <CardTitle className="flex items-center justify-between">
              <div className="flex items-center gap-3">
                <Coins className="w-6 h-6 text-blue-600" />
                Token Registry
              </div>
              <Badge variant="outline" className="text-slate-600">
                {filteredTokens.length} of {tokens.length} tokens
              </Badge>
            </CardTitle>
          </CardHeader>
          <CardContent className="p-0">
            <div className="overflow-x-auto">
              <Table>
                <TableHeader>
                  <TableRow className="bg-slate-50/80">
                    <TableHead className="font-semibold">Token</TableHead>
                    <TableHead className="font-semibold">Type</TableHead>
                    <TableHead className="font-semibold">Supply</TableHead>
                    <TableHead className="font-semibold">Status</TableHead>
                    <TableHead className="font-semibold">Asset Type</TableHead>
                    <TableHead className="font-semibold">Creator</TableHead>
                    <TableHead className="font-semibold">Created</TableHead>
                    <TableHead className="font-semibold">Actions</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {isLoading ? (
                    Array(10).fill(0).map((_, i) => (
                      <TableRow key={i}>
                        <TableCell><div className="h-4 bg-slate-200 rounded animate-pulse"></div></TableCell>
                        <TableCell><div className="h-4 bg-slate-200 rounded animate-pulse"></div></TableCell>
                        <TableCell><div className="h-4 bg-slate-200 rounded animate-pulse"></div></TableCell>
                        <TableCell><div className="h-4 bg-slate-200 rounded animate-pulse"></div></TableCell>
                        <TableCell><div className="h-4 bg-slate-200 rounded animate-pulse"></div></TableCell>
                        <TableCell><div className="h-4 bg-slate-200 rounded animate-pulse"></div></TableCell>
                        <TableCell><div className="h-4 bg-slate-200 rounded animate-pulse"></div></TableCell>
                        <TableCell><div className="h-4 bg-slate-200 rounded animate-pulse"></div></TableCell>
                      </TableRow>
                    ))
                  ) : filteredTokens.length === 0 ? (
                    <TableRow>
                      <TableCell colSpan={8} className="text-center py-8 text-slate-500">
                        No tokens found matching your criteria
                      </TableCell>
                    </TableRow>
                  ) : (
                    filteredTokens.map((token) => (
                      <TableRow key={token.id} className="hover:bg-slate-50/60 transition-colors">
                        <TableCell>
                          <div>
                            <Link to={createPageUrl(`TokenDetails?id=${token.id}`)} className="font-semibold text-slate-900 hover:text-blue-600">{token.name}</Link>
                            <div className="text-sm text-slate-500 font-mono">{token.symbol}</div>
                          </div>
                        </TableCell>
                        <TableCell>
                          <Badge variant="outline" className={getTokenTypeColor(token.token_type)}>
                            {token.token_type}
                          </Badge>
                        </TableCell>
                        <TableCell>
                          <div className="font-mono text-sm">
                            {formatNumber(token.total_supply)}
                          </div>
                        </TableCell>
                        <TableCell>
                          <div className="space-y-1">
                            <StatusBadge status={token.status} size="sm" />
                            <StatusBadge status={token.approval_status} size="sm" />
                          </div>
                        </TableCell>
                        <TableCell>
                          {token.asset_type && (
                            <Badge variant="outline" className={getAssetTypeColor(token.asset_type)}>
                              {token.asset_type.replace('_', ' ')}
                            </Badge>
                          )}
                        </TableCell>
                        <TableCell>
                          <div className="font-mono text-sm text-slate-600">
                            {formatAddress(token.creator_id)}
                          </div>
                        </TableCell>
                        <TableCell>
                          <div className="text-sm text-slate-600">
                            {token.creation_timestamp ? 
                              format(new Date(token.creation_timestamp), 'MMM dd, yyyy') 
                              : 'N/A'
                            }
                          </div>
                        </TableCell>
                        <TableCell>
                          {canBuyToken(token) ? (
                            <Button
                              size="sm"
                              onClick={() => handleQuickBuy(token)}
                              className="bg-gradient-to-r from-blue-600 to-purple-600 hover:from-blue-700 hover:to-purple-700 text-white"
                            >
                              <ShoppingCart className="w-4 h-4 mr-1" />
                              Buy
                            </Button>
                          ) : (
                            <Button size="sm" variant="outline" disabled>
                              Unavailable
                            </Button>
                          )}
                        </TableCell>
                      </TableRow>
                    ))
                  )}
                </TableBody>
              </Table>
            </div>
          </CardContent>
        </Card>
      </div>

      {/* Quick Buy Modal */}
      <QuickBuyModal
        token={selectedToken}
        isOpen={showBuyModal}
        onClose={() => {
          setShowBuyModal(false);
          setSelectedToken(null);
        }}
      />
    </div>
  );
}