import React, { useState, useEffect } from "react";
import { TreasuryTransaction } from "@/api/entities";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";
import { 
  DollarSign, TrendingUp, TrendingDown, ArrowUpRight, 
  ArrowDownLeft, RefreshCw, Plus, Wallet, CreditCard
} from "lucide-react";
import { format } from "date-fns";

export default function Treasury() {
  const [transactions, setTransactions] = useState([]);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    loadTransactions();
  }, []);

  const loadTransactions = async () => {
    setIsLoading(true);
    try {
      const data = await TreasuryTransaction.list("-created_date", 50);
      setTransactions(data);
    } catch (error) {
      console.error("Error loading transactions:", error);
    }
    setIsLoading(false);
  };

  const getStatusColor = (status) => {
    switch (status) {
      case "completed": return "bg-emerald-100 text-emerald-800 border-emerald-200";
      case "processing": return "bg-blue-100 text-blue-800 border-blue-200";
      case "pending": return "bg-amber-100 text-amber-800 border-amber-200";
      case "failed": return "bg-red-100 text-red-800 border-red-200";
      case "cancelled": return "bg-slate-100 text-slate-800 border-slate-200";
      default: return "bg-slate-100 text-slate-800 border-slate-200";
    }
  };

  const getTransactionIcon = (type) => {
    switch (type) {
      case "deposit": return <ArrowDownLeft className="w-4 h-4 text-emerald-600" />;
      case "withdrawal": return <ArrowUpRight className="w-4 h-4 text-red-600" />;
      case "transfer": return <RefreshCw className="w-4 h-4 text-blue-600" />;
      default: return <DollarSign className="w-4 h-4 text-slate-600" />;
    }
  };

  // Mock treasury balances
  const treasuryBalances = [
    { currency: "USD", balance: 2400000, change: 125000, changePercent: 5.5 },
    { currency: "EUR", balance: 1800000, change: -45000, changePercent: -2.4 },
    { currency: "BTC", balance: 45.67, change: 2.3, changePercent: 5.3 },
    { currency: "ETH", balance: 892.45, change: -12.8, changePercent: -1.4 }
  ];

  const totalUSDValue = 4850000; // Mock total value in USD

  return (
    <div className="p-6 space-y-6 bg-slate-50 min-h-screen">
      <div className="flex flex-col md:flex-row justify-between items-start md:items-center gap-4">
        <div>
          <h1 className="text-3xl font-bold text-slate-900">Treasury Management</h1>
          <p className="text-slate-600 mt-1">Monitor treasury balances and transaction flows</p>
        </div>
        <div className="flex items-center gap-3">
          <Button variant="outline" onClick={loadTransactions} disabled={isLoading}>
            <RefreshCw className={`w-4 h-4 mr-2 ${isLoading ? 'animate-spin' : ''}`} />
            Refresh
          </Button>
          <Button className="flex items-center gap-2">
            <Plus className="w-4 h-4" />
            New Transaction
          </Button>
        </div>
      </div>

      {/* Treasury Overview */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
        <Card className="md:col-span-2 lg:col-span-1">
          <CardContent className="p-6">
            <div className="flex items-center justify-between mb-4">
              <div>
                <p className="text-sm font-medium text-slate-600">Total Portfolio Value</p>
                <p className="text-3xl font-bold text-slate-900">
                  ${totalUSDValue.toLocaleString()}
                </p>
              </div>
              <Wallet className="w-8 h-8 text-blue-600" />
            </div>
            <div className="flex items-center gap-2">
              <TrendingUp className="w-4 h-4 text-emerald-600" />
              <span className="text-sm text-emerald-600 font-medium">+8.2%</span>
              <span className="text-sm text-slate-500">vs last month</span>
            </div>
          </CardContent>
        </Card>

        {treasuryBalances.slice(0, 3).map((balance) => (
          <Card key={balance.currency}>
            <CardContent className="p-6">
              <div className="flex items-center justify-between mb-2">
                <div>
                  <p className="text-sm font-medium text-slate-600">{balance.currency}</p>
                  <p className="text-xl font-bold text-slate-900">
                    {balance.currency === "USD" || balance.currency === "EUR" 
                      ? `$${balance.balance.toLocaleString()}` 
                      : `${balance.balance.toFixed(2)} ${balance.currency}`
                    }
                  </p>
                </div>
                <CreditCard className="w-6 h-6 text-slate-600" />
              </div>
              <div className="flex items-center gap-2">
                {balance.change > 0 ? (
                  <TrendingUp className="w-3 h-3 text-emerald-600" />
                ) : (
                  <TrendingDown className="w-3 h-3 text-red-600" />
                )}
                <span className={`text-xs font-medium ${
                  balance.change > 0 ? 'text-emerald-600' : 'text-red-600'
                }`}>
                  {balance.changePercent > 0 ? '+' : ''}{balance.changePercent}%
                </span>
              </div>
            </CardContent>
          </Card>
        ))}
      </div>

      {/* Detailed Balances */}
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <Wallet className="w-5 h-5" />
            Treasury Balances
          </CardTitle>
        </CardHeader>
        <CardContent>
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
            {treasuryBalances.map((balance) => (
              <div key={balance.currency} className="p-4 border rounded-lg bg-slate-50">
                <div className="flex justify-between items-center mb-2">
                  <span className="font-semibold text-slate-900">{balance.currency}</span>
                  <Badge variant="outline" className={
                    balance.change > 0 ? 'text-emerald-700 border-emerald-200' : 'text-red-700 border-red-200'
                  }>
                    {balance.change > 0 ? '+' : ''}{balance.changePercent}%
                  </Badge>
                </div>
                <p className="text-lg font-bold text-slate-900 mb-1">
                  {balance.currency === "USD" || balance.currency === "EUR" 
                    ? `$${balance.balance.toLocaleString()}` 
                    : `${balance.balance.toFixed(2)} ${balance.currency}`
                  }
                </p>
                <p className={`text-sm ${balance.change > 0 ? 'text-emerald-600' : 'text-red-600'}`}>
                  {balance.change > 0 ? '+' : ''}{balance.change.toLocaleString()} this period
                </p>
              </div>
            ))}
          </div>
        </CardContent>
      </Card>

      {/* Recent Transactions */}
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <DollarSign className="w-5 h-5" />
            Recent Transactions
          </CardTitle>
        </CardHeader>
        <CardContent>
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>Transaction</TableHead>
                <TableHead>Type</TableHead>
                <TableHead>Amount</TableHead>
                <TableHead>Status</TableHead>
                <TableHead>Gateway</TableHead>
                <TableHead>Date</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {transactions.map((transaction) => (
                <TableRow key={transaction.id}>
                  <TableCell>
                    <div className="flex items-center gap-3">
                      {getTransactionIcon(transaction.transaction_type)}
                      <div>
                        <p className="font-medium">{transaction.transaction_id}</p>
                        <p className="text-sm text-slate-500">
                          {transaction.from_account} → {transaction.to_account}
                        </p>
                      </div>
                    </div>
                  </TableCell>
                  <TableCell>
                    <Badge variant="outline">
                      {transaction.transaction_type}
                    </Badge>
                  </TableCell>
                  <TableCell>
                    <span className={`font-semibold ${
                      transaction.transaction_type === 'deposit' ? 'text-emerald-600' : 
                      transaction.transaction_type === 'withdrawal' ? 'text-red-600' : 
                      'text-slate-900'
                    }`}>
                      {transaction.transaction_type === 'deposit' ? '+' : 
                       transaction.transaction_type === 'withdrawal' ? '-' : ''}
                      ${transaction.amount.toLocaleString()} {transaction.currency}
                    </span>
                  </TableCell>
                  <TableCell>
                    <Badge className={getStatusColor(transaction.status)}>
                      {transaction.status}
                    </Badge>
                  </TableCell>
                  <TableCell>
                    <span className="text-sm">{transaction.gateway}</span>
                  </TableCell>
                  <TableCell>
                    <span className="text-sm">
                      {format(new Date(transaction.created_date), 'MMM d, yyyy HH:mm')}
                    </span>
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </CardContent>
      </Card>
    </div>
  );
}