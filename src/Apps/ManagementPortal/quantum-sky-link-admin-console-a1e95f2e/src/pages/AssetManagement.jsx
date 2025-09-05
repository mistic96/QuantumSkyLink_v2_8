import React, { useState, useEffect } from "react";
import { CustomerAsset } from "@/api/entities";
import { CompanyAsset } from "@/api/entities";
import { AssetTransfer } from "@/api/entities";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";
import { 
  Wallet, TrendingUp, Search, Filter, ArrowUpRight, 
  ArrowDownLeft, DollarSign, Bitcoin, Banknote, 
  Building2, RefreshCw, Eye, Send, Plus
} from "lucide-react";
import { format } from "date-fns";

export default function AssetManagement() {
  const [customerAssets, setCustomerAssets] = useState([]);
  const [companyAssets, setCompanyAssets] = useState([]);
  const [transfers, setTransfers] = useState([]);
  const [searchTerm, setSearchTerm] = useState("");
  const [assetTypeFilter, setAssetTypeFilter] = useState("all");
  const [isLoading, setIsLoading] = useState(true);
  const [activeTab, setActiveTab] = useState("overview");

  useEffect(() => {
    loadAssets();
  }, []);

  const loadAssets = async () => {
    setIsLoading(true);
    try {
      const [customerData, companyData, transferData] = await Promise.all([
        CustomerAsset.list("-created_date", 100),
        CompanyAsset.list("-created_date", 50),
        AssetTransfer.list("-created_date", 50)
      ]);
      
      setCustomerAssets(customerData);
      setCompanyAssets(companyData);
      setTransfers(transferData);
    } catch (error) {
      console.error("Error loading assets:", error);
    }
    setIsLoading(false);
  };

  const getAssetIcon = (assetType) => {
    switch (assetType) {
      case "crypto": return <Bitcoin className="w-4 h-4 text-orange-600" />;
      case "fiat": return <DollarSign className="w-4 h-4 text-green-600" />;
      case "token": return <Wallet className="w-4 h-4 text-blue-600" />;
      case "nft": return <TrendingUp className="w-4 h-4 text-purple-600" />;
      default: return <Banknote className="w-4 h-4 text-slate-600" />;
    }
  };

  const getStatusColor = (status) => {
    switch (status) {
      case "confirmed": return "bg-emerald-100 text-emerald-800 border-emerald-200";
      case "pending": return "bg-amber-100 text-amber-800 border-amber-200";
      case "failed": return "bg-red-100 text-red-800 border-red-200";
      default: return "bg-slate-100 text-slate-800 border-slate-200";
    }
  };

  const filteredCustomerAssets = customerAssets.filter(asset => {
    const matchesSearch = asset.user_email.toLowerCase().includes(searchTerm.toLowerCase()) ||
                         asset.asset_symbol.toLowerCase().includes(searchTerm.toLowerCase()) ||
                         asset.wallet_address?.toLowerCase().includes(searchTerm.toLowerCase());
    const matchesType = assetTypeFilter === "all" || asset.asset_type === assetTypeFilter;
    return matchesSearch && matchesType;
  });

  const filteredCompanyAssets = companyAssets.filter(asset => {
    const matchesSearch = asset.asset_symbol.toLowerCase().includes(searchTerm.toLowerCase()) ||
                         asset.gl_account_code?.toLowerCase().includes(searchTerm.toLowerCase()) ||
                         asset.wallet_address?.toLowerCase().includes(searchTerm.toLowerCase());
    const matchesType = assetTypeFilter === "all" || asset.asset_type === assetTypeFilter;
    return matchesSearch && matchesType;
  });

  // Calculate totals
  const totalCustomerValue = customerAssets.reduce((sum, asset) => sum + (asset.usd_value || 0), 0);
  const totalCompanyValue = companyAssets.reduce((sum, asset) => sum + (asset.usd_value || 0), 0);
  const totalAssets = customerAssets.length + companyAssets.length;
  const activeTransfers = transfers.filter(t => t.status === "pending").length;

  return (
    <div className="p-6 space-y-6 bg-slate-50 min-h-screen">
      <div className="flex flex-col md:flex-row justify-between items-start md:items-center gap-4">
        <div>
          <h1 className="text-3xl font-bold text-slate-900">Asset Management</h1>
          <p className="text-slate-600 mt-1">Comprehensive asset oversight and management system</p>
        </div>
        <div className="flex items-center gap-3">
          <Button onClick={loadAssets} disabled={isLoading} variant="outline">
            <RefreshCw className={`w-4 h-4 mr-2 ${isLoading ? 'animate-spin' : ''}`} />
            Refresh
          </Button>
          <Button className="flex items-center gap-2">
            <Plus className="w-4 h-4" />
            New Transfer
          </Button>
        </div>
      </div>

      {/* Asset Overview */}
      <div className="grid grid-cols-1 md:grid-cols-4 gap-6">
        <Card>
          <CardContent className="p-6">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm font-medium text-slate-600">Total Assets</p>
                <p className="text-2xl font-bold text-slate-900">{totalAssets.toLocaleString()}</p>
              </div>
              <Wallet className="w-8 h-8 text-blue-600" />
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardContent className="p-6">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm font-medium text-slate-600">Customer Assets</p>
                <p className="text-2xl font-bold text-blue-600">${totalCustomerValue.toLocaleString()}</p>
              </div>
              <Building2 className="w-8 h-8 text-blue-600" />
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardContent className="p-6">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm font-medium text-slate-600">Company Assets</p>
                <p className="text-2xl font-bold text-emerald-600">${totalCompanyValue.toLocaleString()}</p>
              </div>
              <DollarSign className="w-8 h-8 text-emerald-600" />
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardContent className="p-6">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm font-medium text-slate-600">Active Transfers</p>
                <p className="text-2xl font-bold text-amber-600">{activeTransfers}</p>
              </div>
              <ArrowUpRight className="w-8 h-8 text-amber-600" />
            </div>
          </CardContent>
        </Card>
      </div>

      {/* Filters */}
      <Card>
        <CardContent className="p-6">
          <div className="flex flex-col md:flex-row gap-4">
            <div className="flex-1">
              <div className="relative">
                <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 text-slate-400 w-4 h-4" />
                <Input
                  placeholder="Search by address, symbol, or email..."
                  value={searchTerm}
                  onChange={(e) => setSearchTerm(e.target.value)}
                  className="pl-10"
                />
              </div>
            </div>
            <Select value={assetTypeFilter} onValueChange={setAssetTypeFilter}>
              <SelectTrigger className="w-48">
                <SelectValue placeholder="Filter by asset type" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="all">All Asset Types</SelectItem>
                <SelectItem value="crypto">Cryptocurrency</SelectItem>
                <SelectItem value="fiat">Fiat Currency</SelectItem>
                <SelectItem value="token">Tokens</SelectItem>
                <SelectItem value="nft">NFTs</SelectItem>
              </SelectContent>
            </Select>
          </div>
        </CardContent>
      </Card>

      <Tabs value={activeTab} onValueChange={setActiveTab} className="w-full">
        <TabsList className="grid w-full grid-cols-4 bg-white border border-slate-200">
          <TabsTrigger value="overview">Overview</TabsTrigger>
          <TabsTrigger value="customer">Customer Assets</TabsTrigger>
          <TabsTrigger value="company">Company Assets</TabsTrigger>
          <TabsTrigger value="transfers">Transfers</TabsTrigger>
        </TabsList>

        <TabsContent value="overview" className="space-y-6">
          <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
            <Card>
              <CardHeader>
                <CardTitle>Asset Distribution</CardTitle>
              </CardHeader>
              <CardContent>
                <div className="space-y-4">
                  <div className="flex items-center justify-between p-3 border rounded-lg">
                    <div className="flex items-center gap-3">
                      <Bitcoin className="w-5 h-5 text-orange-600" />
                      <span className="font-medium">Cryptocurrency</span>
                    </div>
                    <span className="text-sm font-semibold">
                      {customerAssets.filter(a => a.asset_type === 'crypto').length + 
                       companyAssets.filter(a => a.asset_type === 'crypto').length}
                    </span>
                  </div>
                  <div className="flex items-center justify-between p-3 border rounded-lg">
                    <div className="flex items-center gap-3">
                      <DollarSign className="w-5 h-5 text-green-600" />
                      <span className="font-medium">Fiat Currency</span>
                    </div>
                    <span className="text-sm font-semibold">
                      {customerAssets.filter(a => a.asset_type === 'fiat').length +
                       companyAssets.filter(a => a.asset_type === 'fiat').length}
                    </span>
                  </div>
                  <div className="flex items-center justify-between p-3 border rounded-lg">
                    <div className="flex items-center gap-3">
                      <Wallet className="w-5 h-5 text-blue-600" />
                      <span className="font-medium">Tokens</span>
                    </div>
                    <span className="text-sm font-semibold">
                      {customerAssets.filter(a => a.asset_type === 'token').length +
                       companyAssets.filter(a => a.asset_type === 'token').length}
                    </span>
                  </div>
                </div>
              </CardContent>
            </Card>

            <Card>
              <CardHeader>
                <CardTitle>Recent Transfers</CardTitle>
              </CardHeader>
              <CardContent>
                <div className="space-y-3">
                  {transfers.slice(0, 5).map((transfer) => (
                    <div key={transfer.id} className="flex items-center justify-between p-3 border rounded-lg">
                      <div className="flex items-center gap-3">
                        {transfer.transfer_type === 'external' ? 
                          <ArrowUpRight className="w-4 h-4 text-red-600" /> :
                          <ArrowDownLeft className="w-4 h-4 text-green-600" />
                        }
                        <div>
                          <p className="font-medium text-sm">{transfer.asset_symbol}</p>
                          <p className="text-xs text-slate-500">{transfer.amount}</p>
                        </div>
                      </div>
                      <Badge className={getStatusColor(transfer.status)}>
                        {transfer.status}
                      </Badge>
                    </div>
                  ))}
                </div>
              </CardContent>
            </Card>
          </div>
        </TabsContent>

        <TabsContent value="customer">
          <Card>
            <CardHeader>
              <CardTitle>Customer Assets ({filteredCustomerAssets.length})</CardTitle>
            </CardHeader>
            <CardContent>
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead>User</TableHead>
                    <TableHead>Asset</TableHead>
                    <TableHead>Balance</TableHead>
                    <TableHead>USD Value</TableHead>
                    <TableHead>Address</TableHead>
                    <TableHead>Status</TableHead>
                    <TableHead>Actions</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {filteredCustomerAssets.map((asset) => (
                    <TableRow key={asset.id}>
                      <TableCell>
                        <span className="font-medium">{asset.user_email}</span>
                      </TableCell>
                      <TableCell>
                        <div className="flex items-center gap-2">
                          {getAssetIcon(asset.asset_type)}
                          <div>
                            <p className="font-medium">{asset.asset_symbol}</p>
                            <p className="text-xs text-slate-500">{asset.asset_name}</p>
                          </div>
                        </div>
                      </TableCell>
                      <TableCell>
                        <span className="font-semibold">{asset.balance.toLocaleString()}</span>
                      </TableCell>
                      <TableCell>
                        <span className="text-emerald-600 font-semibold">
                          ${asset.usd_value?.toLocaleString() || '0'}
                        </span>
                      </TableCell>
                      <TableCell>
                        <span className="font-mono text-xs">
                          {asset.wallet_address ? 
                            `${asset.wallet_address.substring(0, 8)}...${asset.wallet_address.substring(asset.wallet_address.length - 6)}` : 
                            'N/A'
                          }
                        </span>
                      </TableCell>
                      <TableCell>
                        <Badge className={asset.frozen ? "bg-red-100 text-red-800" : "bg-emerald-100 text-emerald-800"}>
                          {asset.frozen ? 'Frozen' : 'Active'}
                        </Badge>
                      </TableCell>
                      <TableCell>
                        <Button variant="ghost" size="sm">
                          <Eye className="w-4 h-4" />
                        </Button>
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </CardContent>
          </Card>
        </TabsContent>

        <TabsContent value="company">
          <Card>
            <CardHeader>
              <CardTitle>Company Assets ({filteredCompanyAssets.length})</CardTitle>
            </CardHeader>
            <CardContent>
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead>Asset</TableHead>
                    <TableHead>GL Account</TableHead>
                    <TableHead>Category</TableHead>
                    <TableHead>Balance</TableHead>
                    <TableHead>USD Value</TableHead>
                    <TableHead>P&L</TableHead>
                    <TableHead>Address/Account</TableHead>
                    <TableHead>Actions</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {filteredCompanyAssets.map((asset) => (
                    <TableRow key={asset.id}>
                      <TableCell>
                        <div className="flex items-center gap-2">
                          {getAssetIcon(asset.asset_type)}
                          <div>
                            <p className="font-medium">{asset.asset_symbol}</p>
                            <p className="text-xs text-slate-500">{asset.asset_name}</p>
                          </div>
                        </div>
                      </TableCell>
                      <TableCell>
                        <span className="font-mono text-sm">{asset.gl_account_code}</span>
                      </TableCell>
                      <TableCell>
                        <Badge variant="outline">
                          {asset.account_category}
                        </Badge>
                      </TableCell>
                      <TableCell>
                        <span className="font-semibold">{asset.balance.toLocaleString()}</span>
                      </TableCell>
                      <TableCell>
                        <span className="text-emerald-600 font-semibold">
                          ${asset.usd_value?.toLocaleString() || '0'}
                        </span>
                      </TableCell>
                      <TableCell>
                        <span className={`font-semibold ${
                          (asset.unrealized_pnl || 0) >= 0 ? 'text-emerald-600' : 'text-red-600'
                        }`}>
                          {(asset.unrealized_pnl || 0) >= 0 ? '+' : ''}
                          ${asset.unrealized_pnl?.toLocaleString() || '0'}
                        </span>
                      </TableCell>
                      <TableCell>
                        <span className="font-mono text-xs">
                          {asset.wallet_address ? 
                            `${asset.wallet_address.substring(0, 8)}...${asset.wallet_address.substring(asset.wallet_address.length - 6)}` : 
                            asset.bank_account_number || 'N/A'
                          }
                        </span>
                      </TableCell>
                      <TableCell>
                        <div className="flex items-center gap-1">
                          <Button variant="ghost" size="sm">
                            <Eye className="w-4 h-4" />
                          </Button>
                          <Button variant="ghost" size="sm">
                            <Send className="w-4 h-4" />
                          </Button>
                        </div>
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </CardContent>
          </Card>
        </TabsContent>

        <TabsContent value="transfers">
          <Card>
            <CardHeader>
              <CardTitle>Asset Transfers ({transfers.length})</CardTitle>
            </CardHeader>
            <CardContent>
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead>Type</TableHead>
                    <TableHead>Asset</TableHead>
                    <TableHead>Amount</TableHead>
                    <TableHead>From</TableHead>
                    <TableHead>To</TableHead>
                    <TableHead>Status</TableHead>
                    <TableHead>Date</TableHead>
                    <TableHead>Actions</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {transfers.map((transfer) => (
                    <TableRow key={transfer.id}>
                      <TableCell>
                        <Badge variant="outline">
                          {transfer.transfer_type.replace('_', ' ')}
                        </Badge>
                      </TableCell>
                      <TableCell>
                        <span className="font-medium">{transfer.asset_symbol}</span>
                      </TableCell>
                      <TableCell>
                        <div>
                          <span className="font-semibold">{transfer.amount.toLocaleString()}</span>
                          <p className="text-xs text-slate-500">
                            ${transfer.usd_value?.toLocaleString() || '0'}
                          </p>
                        </div>
                      </TableCell>
                      <TableCell>
                        <span className="font-mono text-xs">
                          {transfer.from_address.substring(0, 8)}...{transfer.from_address.substring(transfer.from_address.length - 6)}
                        </span>
                      </TableCell>
                      <TableCell>
                        <span className="font-mono text-xs">
                          {transfer.to_address.substring(0, 8)}...{transfer.to_address.substring(transfer.to_address.length - 6)}
                        </span>
                      </TableCell>
                      <TableCell>
                        <Badge className={getStatusColor(transfer.status)}>
                          {transfer.status}
                        </Badge>
                      </TableCell>
                      <TableCell>
                        <span className="text-sm">
                          {format(new Date(transfer.created_date), 'MMM d, yyyy HH:mm')}
                        </span>
                      </TableCell>
                      <TableCell>
                        <Button variant="ghost" size="sm">
                          <Eye className="w-4 h-4" />
                        </Button>
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </CardContent>
          </Card>
        </TabsContent>
      </Tabs>
    </div>
  );
}