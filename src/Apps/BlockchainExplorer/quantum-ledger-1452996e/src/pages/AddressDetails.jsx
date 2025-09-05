
import React, { useState, useEffect } from 'react';
import { useSearchParams, Link } from 'react-router-dom';
import { Address, Transaction, Token } from '@/api/entities';
import DetailHeader from '../components/details/DetailHeader';
import DetailItem from '../components/details/DetailItem';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table';
import { Wallet, ArrowRightLeft, Coins } from 'lucide-react';
import StatusBadge from '../components/ui/StatusBadge';
import { formatDistanceToNow } from 'date-fns';
import { createPageUrl } from '@/utils';

export default function AddressDetails() {
  const [searchParams] = useSearchParams();
  const [address, setAddress] = useState(null);
  const [transactions, setTransactions] = useState([]);
  const [tokenHoldings, setTokenHoldings] = useState([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState(null);

  useEffect(() => {
    const addressParam = searchParams.get('address');
    if (addressParam) {
      loadAddressDetails(addressParam);
    } else {
      setError("No address provided.");
      setIsLoading(false);
    }
  }, [searchParams]);

  const loadAddressDetails = async (address) => {
    setIsLoading(true);
    try {
      const addressData = await Address.filter({ address: address });
      if (addressData.length > 0) {
        setAddress(addressData[0]);
        const fromTxs = await Transaction.filter({ from_address: address }, '-timestamp', 25);
        const toTxs = await Transaction.filter({ to_address: address }, '-timestamp', 25);
        
        // Simple merge and sort, can be improved for pagination
        const allTxs = [...fromTxs, ...toTxs]
          .sort((a, b) => new Date(b.timestamp) - new Date(a.timestamp))
          .filter((v,i,a)=>a.findIndex(t=>(t.id === v.id))===i) // unique
          .slice(0, 50);

        setTransactions(allTxs);

        // Fetch associated tokens to display holdings (simulated)
        const allTokens = await Token.list('-created_date', 5);
        setTokenHoldings(allTokens.map(token => ({
          ...token,
          balance: (Math.random() * 10000).toLocaleString(undefined, { maximumFractionDigits: 2 })
        })));

      } else {
        setError("Address not found.");
      }
    } catch (err) {
      setError("Failed to load address details.");
      console.error(err);
    } finally {
      setIsLoading(false);
    }
  };
  
  const formatHash = (hash, len = 6) => {
    if (!hash) return '';
    return `${hash.slice(0, len)}...${hash.slice(-len)}`;
  };

  if (isLoading) return <div className="p-6">Loading address details...</div>;
  if (error) return <div className="p-6 text-red-500">{error}</div>;
  if (!address) return <div className="p-6">Address not found.</div>;

  return (
    <div className="p-6 space-y-8">
      <DetailHeader 
        title="Address Details"
        subtitle={address.address}
        icon={Wallet}
      />

      <div className="grid grid-cols-1 md:grid-cols-2 gap-8">
        <Card className="bg-white/60 backdrop-blur-sm border-white/80 shadow-lg">
          <CardHeader><CardTitle>Overview</CardTitle></CardHeader>
          <CardContent>
            <dl>
              <DetailItem label="Balance">{address.balance?.toFixed(8)} ETH</DetailItem>
              <DetailItem label="Transaction Count">{address.transaction_count}</DetailItem>
              <DetailItem label="Nonce">{address.nonce}</DetailItem>
              <DetailItem label="Type">{address.address_type}</DetailItem>
              <DetailItem label="Last Active">{address.last_active ? formatDistanceToNow(new Date(address.last_active), { addSuffix: true }) : 'Never'}</DetailItem>
            </dl>
          </CardContent>
        </Card>
        <Card className="bg-white/60 backdrop-blur-sm border-white/80 shadow-lg">
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <Coins className="w-5 h-5" />
              Token Holdings (Simulated)
            </CardTitle>
          </CardHeader>
          <CardContent>
            {isLoading ? (
              <p>Loading...</p>
            ) : tokenHoldings.length > 0 ? (
              <div className="space-y-4">
                {tokenHoldings.map(token => (
                  <div key={token.id} className="flex items-center justify-between p-2 rounded-lg hover:bg-slate-50/80">
                    <div className="flex items-center gap-3">
                      <div className="w-8 h-8 bg-gradient-to-r from-blue-500 to-purple-500 rounded-full flex items-center justify-center">
                        <span className="text-white font-bold text-xs">{token.symbol?.[0]}</span>
                      </div>
                      <div>
                        <Link to={createPageUrl(`TokenDetails?id=${token.id}`)} className="font-semibold text-slate-800 hover:text-blue-600">
                          {token.name}
                        </Link>
                        <div className="text-sm text-slate-500 font-mono">{token.symbol}</div>
                      </div>
                    </div>
                    <div className="text-right font-mono text-sm">
                      {token.balance}
                    </div>
                  </div>
                ))}
              </div>
            ) : (
              <p className="text-slate-500 text-center py-8">No token holdings found.</p>
            )}
          </CardContent>
        </Card>
      </div>

      <Card className="bg-white/60 backdrop-blur-sm border-white/80 shadow-lg">
        <CardHeader><CardTitle>Transactions</CardTitle></CardHeader>
        <CardContent className="p-0">
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>Txn Hash</TableHead>
                <TableHead>Age</TableHead>
                <TableHead>From</TableHead>
                <TableHead>To</TableHead>
                <TableHead>Value</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {transactions.map(tx => (
                <TableRow key={tx.id}>
                  <TableCell className="font-mono text-blue-600">
                     <Link to={createPageUrl(`TransactionDetails?id=${tx.id}`)}>{formatHash(tx.transaction_id)}</Link>
                  </TableCell>
                  <TableCell>{formatDistanceToNow(new Date(tx.timestamp), { addSuffix: true })}</TableCell>
                  <TableCell className="font-mono text-blue-600">
                    <Link to={createPageUrl(`AddressDetails?address=${tx.from_address}`)}>{formatHash(tx.from_address)}</Link>
                  </TableCell>
                  <TableCell className="font-mono text-blue-600">
                    <Link to={createPageUrl(`AddressDetails?address=${tx.to_address}`)}>{formatHash(tx.to_address)}</Link>
                  </TableCell>
                  <TableCell>{tx.amount?.toFixed(4)} ETH</TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </CardContent>
      </Card>
    </div>
  );
}
