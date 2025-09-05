import React, { useState, useEffect, useMemo } from "react";
import { Transaction } from "@/api/entities";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";
import { Input } from "@/components/ui/input";
import { Button } from "@/components/ui/button";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { Search, ArrowRightLeft, Clock, DollarSign, Zap, ChevronLeft, ChevronRight } from "lucide-react";
import StatusBadge from "../components/ui/StatusBadge";
import DataCard from "../components/ui/DataCard";
import { formatDistanceToNow } from "date-fns";
import { Link } from "react-router-dom";
import { createPageUrl } from "@/utils";

const ROWS_PER_PAGE = 15;

export default function TransactionsPage() {
  const [allTransactions, setAllTransactions] = useState([]);
  const [filteredTransactions, setFilteredTransactions] = useState([]);
  const [searchQuery, setSearchQuery] = useState("");
  const [statusFilter, setStatusFilter] = useState("all");
  const [currentPage, setCurrentPage] = useState(1);
  const [isLoading, setIsLoading] = useState(true);
  const [stats, setStats] = useState({
    totalTransactions: 0,
    pendingTransactions: 0,
    avgFee: 0,
    avgGasPrice: 0,
  });

  useEffect(() => {
    loadTransactions();
  }, []);

  useEffect(() => {
    let filtered = allTransactions;
    if (searchQuery) {
      const lowercasedQuery = searchQuery.toLowerCase();
      filtered = allTransactions.filter(tx =>
        tx.transaction_id?.toLowerCase().includes(lowercasedQuery) ||
        tx.from_address?.toLowerCase().includes(lowercasedQuery) ||
        tx.to_address?.toLowerCase().includes(lowercasedQuery)
      );
    }
    if (statusFilter !== 'all') {
      filtered = filtered.filter(tx => tx.status === statusFilter);
    }
    setFilteredTransactions(filtered);
    setCurrentPage(1); // Reset page on filter change
  }, [searchQuery, statusFilter, allTransactions]);

  const loadTransactions = async () => {
    try {
      const txData = await Transaction.list('-timestamp');
      setAllTransactions(txData);
      setFilteredTransactions(txData);
      calculateStats(txData);
    } catch (error) {
      console.error('Error loading transactions:', error);
    } finally {
      setIsLoading(false);
    }
  };

  const calculateStats = (transactions) => {
    if (transactions.length === 0) return;

    const pendingTransactions = transactions.filter(tx => tx.status === 'pending').length;
    const avgFee = transactions.reduce((sum, tx) => sum + (tx.fee || 0), 0) / transactions.length;
    const avgGasPrice = transactions.reduce((sum, tx) => sum + (tx.gas_price || 0), 0) / transactions.length;

    setStats({
      totalTransactions: transactions.length,
      pendingTransactions,
      avgFee,
      avgGasPrice,
    });
  };

  const paginatedTransactions = useMemo(() => {
    const startIndex = (currentPage - 1) * ROWS_PER_PAGE;
    return filteredTransactions.slice(startIndex, startIndex + ROWS_PER_PAGE);
  }, [currentPage, filteredTransactions]);

  const totalPages = Math.ceil(filteredTransactions.length / ROWS_PER_PAGE);

  const formatHash = (hash, len = 6) => {
    if (!hash) return '';
    return `${hash.slice(0, len)}...${hash.slice(-len)}`;
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
          <h1 className="text-4xl font-bold text-slate-900 mb-2">Transactions Explorer</h1>
          <p className="text-lg text-slate-600">Search and explore all transactions on the network.</p>
        </div>

        {/* Stats */}
        <div className="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-4 gap-6">
          <DataCard title="Total Transactions" value={stats.totalTransactions.toLocaleString()} subtitle="In latest blocks" icon={ArrowRightLeft} />
          <DataCard title="Pending Transactions" value={stats.pendingTransactions.toLocaleString()} subtitle="Awaiting confirmation" icon={Clock} />
          <DataCard title="Avg. Transaction Fee" value={`${stats.avgFee.toFixed(6)} ETH`} subtitle="Average network fee" icon={DollarSign} />
          <DataCard title="Avg. Gas Price" value={`${stats.avgGasPrice.toFixed(2)} Gwei`} subtitle="Average gas cost" icon={Zap} />
        </div>
      </div>
      
      {/* Transaction Table */}
      <Card className="bg-white/60 backdrop-blur-sm border-white/80 shadow-lg">
        <CardHeader className="flex flex-col md:flex-row items-center justify-between gap-4">
          <CardTitle className="flex items-center gap-3">
            <ArrowRightLeft className="w-6 h-6 text-blue-600" />
            Blockchain Transactions
          </CardTitle>
          <div className="flex flex-col md:flex-row gap-4 w-full md:w-auto">
            <div className="relative flex-grow md:w-80">
              <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 text-slate-400 w-4 h-4" />
              <Input
                placeholder="Search by Txn Hash / Address..."
                value={searchQuery}
                onChange={(e) => setSearchQuery(e.target.value)}
                className="pl-10 bg-white/80"
              />
            </div>
            <Select value={statusFilter} onValueChange={setStatusFilter}>
              <SelectTrigger className="bg-white/80 md:w-48">
                <SelectValue placeholder="Status" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="all">All Status</SelectItem>
                <SelectItem value="confirmed">Confirmed</SelectItem>
                <SelectItem value="pending">Pending</SelectItem>
                <SelectItem value="failed">Failed</SelectItem>
                <SelectItem value="rejected">Rejected</SelectItem>
              </SelectContent>
            </Select>
          </div>
        </CardHeader>
        <CardContent className="p-0">
          <div className="overflow-x-auto">
            <Table>
              <TableHeader>
                <TableRow className="bg-slate-50/80">
                  <TableHead>Txn Hash</TableHead>
                  <TableHead>Block</TableHead>
                  <TableHead>Age</TableHead>
                  <TableHead>From</TableHead>
                  <TableHead>To</TableHead>
                  <TableHead>Value</TableHead>
                  <TableHead>Fee</TableHead>
                  <TableHead>Status</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {isLoading ? (
                  Array(ROWS_PER_PAGE).fill(0).map((_, i) => (
                    <TableRow key={i}>{[...Array(8)].map((_, j) => (<TableCell key={j}><div className="h-4 bg-slate-200 rounded animate-pulse w-full"></div></TableCell>))}</TableRow>
                  ))
                ) : paginatedTransactions.length === 0 ? (
                  <TableRow><TableCell colSpan={8} className="text-center py-8 text-slate-500">No transactions found.</TableCell></TableRow>
                ) : (
                  paginatedTransactions.map((tx) => (
                    <TableRow key={tx.id} className="hover:bg-slate-50/60 transition-colors">
                      <TableCell className="font-mono text-sm text-blue-600">
                        <Link to={createPageUrl(`TransactionDetails?id=${tx.id}`)}>{formatHash(tx.transaction_id)}</Link>
                      </TableCell>
                      <TableCell className="font-mono text-sm text-blue-600">
                        <Link to={createPageUrl(`BlockDetails?number=${tx.block_number}`)}>{tx.block_number}</Link>
                      </TableCell>
                      <TableCell className="text-sm text-slate-600">{tx.timestamp ? formatDistanceToNow(new Date(tx.timestamp), { addSuffix: true }) : 'N/A'}</TableCell>
                      <TableCell className="font-mono text-sm text-blue-600">
                        <Link to={createPageUrl(`AddressDetails?address=${tx.from_address}`)}>{formatHash(tx.from_address)}</Link>
                      </TableCell>
                      <TableCell className="font-mono text-sm text-blue-600">
                        <Link to={createPageUrl(`AddressDetails?address=${tx.to_address}`)}>{formatHash(tx.to_address)}</Link>
                      </TableCell>
                      <TableCell className="font-medium">{tx.amount?.toFixed(4)} ETH</TableCell>
                      <TableCell className="text-sm text-slate-500">{tx.fee?.toFixed(6)}</TableCell>
                      <TableCell><StatusBadge status={tx.status} size="sm" /></TableCell>
                    </TableRow>
                  ))
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