import React, { useState, useEffect, useMemo } from "react";
import { Address } from "@/api/entities";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";
import { Input } from "@/components/ui/input";
import { Button } from "@/components/ui/button";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { Badge } from "@/components/ui/badge";
import { Search, Wallet, Users, Building, Shield, ChevronLeft, ChevronRight, Copy } from "lucide-react";
import DataCard from "../components/ui/DataCard";
import { formatDistanceToNow } from "date-fns";
import { Link } from "react-router-dom";
import { createPageUrl } from "@/utils";

const ROWS_PER_PAGE = 15;

export default function AddressesPage() {
  const [allAddresses, setAllAddresses] = useState([]);
  const [filteredAddresses, setFilteredAddresses] = useState([]);
  const [searchQuery, setSearchQuery] = useState("");
  const [typeFilter, setTypeFilter] = useState("all");
  const [currentPage, setCurrentPage] = useState(1);
  const [isLoading, setIsLoading] = useState(true);
  const [stats, setStats] = useState({
    totalAddresses: 0,
    walletAddresses: 0,
    contractAddresses: 0,
    validatorAddresses: 0,
    totalBalance: 0
  });

  useEffect(() => {
    loadAddresses();
  }, []);

  useEffect(() => {
    let filtered = allAddresses;
    if (searchQuery) {
      const lowercasedQuery = searchQuery.toLowerCase();
      filtered = allAddresses.filter(addr =>
        addr.address?.toLowerCase().includes(lowercasedQuery) ||
        addr.metadata?.toLowerCase().includes(lowercasedQuery)
      );
    }
    if (typeFilter !== 'all') {
      filtered = filtered.filter(addr => addr.address_type === typeFilter);
    }
    setFilteredAddresses(filtered);
    setCurrentPage(1);
  }, [searchQuery, typeFilter, allAddresses]);

  const loadAddresses = async () => {
    try {
      const addressData = await Address.list('-last_active');
      setAllAddresses(addressData);
      setFilteredAddresses(addressData);
      calculateStats(addressData);
    } catch (error) {
      console.error('Error loading addresses:', error);
    } finally {
      setIsLoading(false);
    }
  };

  const calculateStats = (addresses) => {
    if (addresses.length === 0) return;

    const walletAddresses = addresses.filter(addr => addr.address_type === 'wallet').length;
    const contractAddresses = addresses.filter(addr => addr.address_type === 'contract').length;
    const validatorAddresses = addresses.filter(addr => addr.address_type === 'validator').length;
    const totalBalance = addresses.reduce((sum, addr) => sum + (addr.balance || 0), 0);

    setStats({
      totalAddresses: addresses.length,
      walletAddresses,
      contractAddresses,
      validatorAddresses,
      totalBalance
    });
  };

  const paginatedAddresses = useMemo(() => {
    const startIndex = (currentPage - 1) * ROWS_PER_PAGE;
    return filteredAddresses.slice(startIndex, startIndex + ROWS_PER_PAGE);
  }, [currentPage, filteredAddresses]);

  const totalPages = Math.ceil(filteredAddresses.length / ROWS_PER_PAGE);

  const formatHash = (hash, len = 6) => {
    if (!hash) return '';
    return `${hash.slice(0, len)}...${hash.slice(-len)}`;
  };

  const formatNumber = (num) => {
    if (!num) return '0';
    if (num >= 1e9) return (num / 1e9).toFixed(2) + 'B';
    if (num >= 1e6) return (num / 1e6).toFixed(2) + 'M';
    if (num >= 1e3) return (num / 1e3).toFixed(2) + 'K';
    return num.toLocaleString();
  };

  const getAddressTypeColor = (type) => {
    const colors = {
      'wallet': 'bg-blue-100 text-blue-800 border-blue-200',
      'contract': 'bg-purple-100 text-purple-800 border-purple-200',
      'validator': 'bg-green-100 text-green-800 border-green-200'
    };
    return colors[type] || 'bg-gray-100 text-gray-800 border-gray-200';
  };

  const getAddressTypeIcon = (type) => {
    const icons = {
      'wallet': Wallet,
      'contract': Building,
      'validator': Shield
    };
    return icons[type] || Wallet;
  };

  const copyToClipboard = (text) => {
    navigator.clipboard.writeText(text);
    // Could add toast notification here
  };

  const handlePageChange = (page) => {
    if (page > 0 && page <= totalPages) {
      setCurrentPage(page);
    }
  };

  const renderPagination = () => {
    if (totalPages <= 1) return null;
    const pages = [];
    const maxVisiblePages = 5;
    let startPage = Math.max(1, currentPage - Math.floor(maxVisiblePages / 2));
    let endPage = Math.min(totalPages, startPage + maxVisiblePages - 1);
    if (endPage - startPage + 1 < maxVisiblePages) {
      startPage = Math.max(1, endPage - maxVisiblePages + 1);
    }
    for (let i = startPage; i <= endPage; i++) {
      pages.push(i);
    }

    return (
      <div className="flex items-center justify-center gap-2 p-4 border-t border-slate-200/60">
        <Button variant="outline" size="sm" onClick={() => handlePageChange(currentPage - 1)} disabled={currentPage === 1} className="flex items-center gap-1">
          <ChevronLeft className="w-4 h-4" /> Previous
        </Button>
        {startPage > 1 && (<><Button variant="outline" size="sm" onClick={() => handlePageChange(1)}>1</Button>{startPage > 2 && <span className="px-2">...</span>}</>)}
        {pages.map(page => (<Button key={page} variant="outline" size="sm" onClick={() => handlePageChange(page)} className={currentPage === page ? "bg-blue-100 text-blue-700 border-blue-300" : ""}>{page}</Button>))}
        {endPage < totalPages && (<>{endPage < totalPages - 1 && <span className="px-2">...</span>}<Button variant="outline" size="sm" onClick={() => handlePageChange(totalPages)}>{totalPages}</Button></>)}
        <Button variant="outline" size="sm" onClick={() => handlePageChange(currentPage + 1)} disabled={currentPage === totalPages} className="flex items-center gap-1">
          Next <ChevronRight className="w-4 h-4" />
        </Button>
      </div>
    );
  };

  return (
    <div className="p-6 space-y-8">
      {/* Header */}
      <div className="space-y-6">
        <div>
          <h1 className="text-4xl font-bold text-slate-900 mb-2">
            Address Explorer
          </h1>
          <p className="text-lg text-slate-600">
            Browse wallet addresses, smart contracts, and validators on the network
          </p>
        </div>

        {/* Stats Cards */}
        <div className="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-4 gap-6">
          <DataCard
            title="Total Addresses"
            value={formatNumber(stats.totalAddresses)}
            subtitle="Unique addresses"
            icon={Users}
            trend="+2.3% this week"
            trendDirection="up"
          />
          <DataCard
            title="Wallet Addresses"
            value={formatNumber(stats.walletAddresses)}
            subtitle="User wallets"
            icon={Wallet}
          />
          <DataCard
            title="Smart Contracts"
            value={formatNumber(stats.contractAddresses)}
            subtitle="Deployed contracts"
            icon={Building}
          />
          <DataCard
            title="Total Balance"
            value={`${stats.totalBalance.toFixed(2)} ETH`}
            subtitle="Across all addresses"
            icon={Shield}
          />
        </div>
      </div>

      {/* Address Table */}
      <Card className="bg-white/60 backdrop-blur-sm border-white/80 shadow-lg">
        <CardHeader className="flex flex-col md:flex-row items-center justify-between gap-4">
          <CardTitle className="flex items-center gap-3">
            <Wallet className="w-6 h-6 text-blue-600" />
            Network Addresses
          </CardTitle>
          <div className="flex flex-col md:flex-row gap-4 w-full md:w-auto">
            <div className="relative flex-grow md:w-80">
              <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 text-slate-400 w-4 h-4" />
              <Input
                placeholder="Search by address or metadata..."
                value={searchQuery}
                onChange={(e) => setSearchQuery(e.target.value)}
                className="pl-10 bg-white/80"
              />
            </div>
            <Select value={typeFilter} onValueChange={setTypeFilter}>
              <SelectTrigger className="bg-white/80 md:w-48">
                <SelectValue placeholder="Address Type" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="all">All Types</SelectItem>
                <SelectItem value="wallet">Wallets</SelectItem>
                <SelectItem value="contract">Contracts</SelectItem>
                <SelectItem value="validator">Validators</SelectItem>
              </SelectContent>
            </Select>
          </div>
        </CardHeader>
        <CardContent className="p-0">
          <div className="overflow-x-auto">
            <Table>
              <TableHeader>
                <TableRow className="bg-slate-50/80">
                  <TableHead className="font-semibold">Address</TableHead>
                  <TableHead className="font-semibold">Type</TableHead>
                  <TableHead className="font-semibold">Balance</TableHead>
                  <TableHead className="font-semibold">Transactions</TableHead>
                  <TableHead className="font-semibold">Nonce</TableHead>
                  <TableHead className="font-semibold">Last Active</TableHead>
                  <TableHead className="font-semibold">Description</TableHead>
                  <TableHead className="font-semibold w-16">Actions</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {isLoading ? (
                  Array(ROWS_PER_PAGE).fill(0).map((_, i) => (
                    <TableRow key={i}>
                      {[...Array(8)].map((_, j) => (
                        <TableCell key={j}><div className="h-4 bg-slate-200 rounded animate-pulse"></div></TableCell>
                      ))}
                    </TableRow>
                  ))
                ) : paginatedAddresses.length === 0 ? (
                  <TableRow>
                    <TableCell colSpan={8} className="text-center py-8 text-slate-500">
                      No addresses found matching your criteria.
                    </TableCell>
                  </TableRow>
                ) : (
                  paginatedAddresses.map((address) => {
                    const TypeIcon = getAddressTypeIcon(address.address_type);
                    return (
                      <TableRow key={address.id} className="hover:bg-slate-50/60 transition-colors">
                        <TableCell className="font-mono text-sm text-blue-600">
                          <Link to={createPageUrl(`AddressDetails?address=${address.address}`)}>{formatHash(address.address, 8)}</Link>
                        </TableCell>
                        <TableCell>
                          <Badge 
                            variant="outline" 
                            className={`${getAddressTypeColor(address.address_type)} flex items-center gap-1 w-fit`}
                          >
                            <TypeIcon className="w-3 h-3" />
                            {address.address_type}
                          </Badge>
                        </TableCell>
                        <TableCell className="font-medium">
                          {address.balance?.toFixed(4)} ETH
                        </TableCell>
                        <TableCell className="font-mono text-sm">
                          {formatNumber(address.transaction_count)}
                        </TableCell>
                        <TableCell className="font-mono text-sm">
                          {address.nonce}
                        </TableCell>
                        <TableCell className="text-sm text-slate-600">
                          {address.last_active ? 
                            formatDistanceToNow(new Date(address.last_active), { addSuffix: true }) 
                            : 'Never'
                          }
                        </TableCell>
                        <TableCell className="text-sm text-slate-600 max-w-48 truncate">
                          {address.metadata || '-'}
                        </TableCell>
                        <TableCell>
                          <Button
                            variant="ghost"
                            size="sm"
                            onClick={() => copyToClipboard(address.address)}
                            className="h-8 w-8 p-0 hover:bg-slate-100"
                          >
                            <Copy className="w-4 h-4" />
                          </Button>
                        </TableCell>
                      </TableRow>
                    );
                  })
                )}
              </TableBody>
            </Table>
          </div>
          {renderPagination()}
        </CardContent>
      </Card>
    </div>
  );
}