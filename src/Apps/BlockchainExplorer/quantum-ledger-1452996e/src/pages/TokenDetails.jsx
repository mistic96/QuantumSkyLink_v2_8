
import React, { useState, useEffect } from 'react';
import { useSearchParams, Link } from 'react-router-dom';
import { Token, Transaction, Address } from '@/api/entities';
import DetailHeader from '../components/details/DetailHeader';
import DetailItem from '../components/details/DetailItem';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table';
import { Coins, Info, ArrowRightLeft, Users } from 'lucide-react';
import StatusBadge from '../components/ui/StatusBadge';
import { format } from 'date-fns';
import { createPageUrl } from '@/utils';

export default function TokenDetails() {
  const [searchParams] = useSearchParams();
  const [token, setToken] = useState(null);
  const [transfers, setTransfers] = useState([]);
  const [holders, setHolders] = useState([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState(null);

  useEffect(() => {
    const tokenId = searchParams.get('id');
    if (tokenId) {
      loadTokenDetails(tokenId);
    } else {
      setError("No token ID provided.");
      setIsLoading(false);
    }
  }, [searchParams]);

  const loadTokenDetails = async (id) => {
    setIsLoading(true);
    try {
      const tokenData = await Token.get(id);
      setToken(tokenData);

      if (tokenData && tokenData.contract_address) {
        // This is an assumption: transfers are transactions to the contract address
        const transferData = await Transaction.filter({ to_address: tokenData.contract_address }, '-timestamp', 50);
        setTransfers(transferData);
      }
      
      // Generate dynamic mock holder data
      if (tokenData && tokenData.total_supply) {
        const addresses = await Address.list('-transaction_count', 10);
        const topHolders = addresses.map(addr => {
          const amount = Math.random() * (tokenData.total_supply / 20); // Assign up to 5% of total supply
          const percentage = ((amount / tokenData.total_supply) * 100).toFixed(2);
          return {
            address: addr.address,
            amount: amount,
            percentage: percentage,
          };
        }).sort((a, b) => b.amount - a.amount);
        setHolders(topHolders);
      }

    } catch (err) {
      setError("Failed to load token details.");
      console.error(err);
    } finally {
      setIsLoading(false);
    }
  };
  
  const formatHash = (hash, len = 6) => {
    if (!hash) return '';
    return `${hash.slice(0, len)}...${hash.slice(-len)}`;
  };

  if (isLoading) return <div className="p-6">Loading token details...</div>;
  if (error) return <div className="p-6 text-red-500">{error}</div>;
  if (!token) return <div className="p-6">Token not found.</div>;

  return (
    <div className="p-6 space-y-8">
      <DetailHeader 
        title={token.name}
        subtitle={token.symbol}
        icon={Coins}
        badge={<StatusBadge status={token.status} />}
      />

      <Tabs defaultValue="info">
        <TabsList className="grid w-full grid-cols-3">
          <TabsTrigger value="info"><Info className="w-4 h-4 mr-2" />Information</TabsTrigger>
          <TabsTrigger value="transfers"><ArrowRightLeft className="w-4 h-4 mr-2" />Transfers</TabsTrigger>
          <TabsTrigger value="holders"><Users className="w-4 h-4 mr-2" />Holders</TabsTrigger>
        </TabsList>
        <TabsContent value="info">
          <Card className="bg-white/60 backdrop-blur-sm border-white/80 shadow-lg mt-4">
            <CardHeader><CardTitle>Token Information</CardTitle></CardHeader>
            <CardContent>
              <dl>
                <DetailItem label="Description">{token.description || 'No description provided.'}</DetailItem>
                <DetailItem label="Contract Address" isHash copyValue={token.contract_address}>
                  <Link to={createPageUrl(`AddressDetails?address=${token.contract_address}`)} className="text-blue-600 hover:underline">{token.contract_address}</Link>
                </DetailItem>
                <DetailItem label="Total Supply">{token.total_supply?.toLocaleString()}</DetailItem>
                <DetailItem label="Decimals">{token.decimals}</DetailItem>
                <DetailItem label="Token Type">{token.token_type}</DetailItem>
                <DetailItem label="Asset Type">{token.asset_type}</DetailItem>
                <DetailItem label="Creator" isHash copyValue={token.creator_id}>
                  <Link to={createPageUrl(`AddressDetails?address=${token.creator_id}`)} className="text-blue-600 hover:underline">{token.creator_id}</Link>
                </DetailItem>
                <DetailItem label="Created">{format(new Date(token.creation_timestamp), 'PPP')}</DetailItem>
                <DetailItem label="Approval Status"><StatusBadge status={token.approval_status} size="sm" /></DetailItem>
              </dl>
            </CardContent>
          </Card>
        </TabsContent>
        <TabsContent value="transfers">
          <Card className="bg-white/60 backdrop-blur-sm border-white/80 shadow-lg mt-4">
            <CardHeader><CardTitle>Recent Transfers</CardTitle></CardHeader>
            <CardContent className="p-0">
              <Table>
                <TableHeader>
                  <TableRow><TableHead>Txn Hash</TableHead><TableHead>From</TableHead><TableHead>To</TableHead><TableHead>Value</TableHead></TableRow>
                </TableHeader>
                <TableBody>
                  {transfers.map(tx => (
                    <TableRow key={tx.id}>
                      <TableCell className="font-mono text-blue-600">
                        <Link to={createPageUrl(`TransactionDetails?id=${tx.id}`)}>{formatHash(tx.transaction_id)}</Link>
                      </TableCell>
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
        </TabsContent>
        <TabsContent value="holders">
          <Card className="bg-white/60 backdrop-blur-sm border-white/80 shadow-lg mt-4">
            <CardHeader><CardTitle>Top Token Holders (Simulated)</CardTitle></CardHeader>
            <CardContent className="p-0">
              <Table>
                <TableHeader>
                  <TableRow><TableHead>Address</TableHead><TableHead>Amount</TableHead><TableHead>Percentage</TableHead></TableRow>
                </TableHeader>
                <TableBody>
                  {holders.map(holder => (
                    <TableRow key={holder.address}>
                      <TableCell className="font-mono text-blue-600">
                        <Link to={createPageUrl(`AddressDetails?address=${holder.address}`)}>{formatHash(holder.address, 12)}</Link>
                      </TableCell>
                      <TableCell>{Number(holder.amount).toLocaleString(undefined, { maximumFractionDigits: 2 })}</TableCell>
                      <TableCell>{holder.percentage}%</TableCell>
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
