import React, { useState, useEffect } from 'react';
import { useSearchParams, Link } from 'react-router-dom';
import { Block, Transaction } from '@/api/entities';
import DetailHeader from '../components/details/DetailHeader';
import DetailItem from '../components/details/DetailItem';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table';
import { Blocks, ArrowRightLeft } from 'lucide-react';
import StatusBadge from '../components/ui/StatusBadge';
import { format, formatDistanceToNow } from 'date-fns';
import { createPageUrl } from '@/utils';

export default function BlockDetails() {
  const [searchParams] = useSearchParams();
  const [block, setBlock] = useState(null);
  const [transactions, setTransactions] = useState([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState(null);

  useEffect(() => {
    const blockId = searchParams.get('id');
    const blockNumber = searchParams.get('number');
    const blockHash = searchParams.get('hash');

    if (blockId) {
      loadBlockById(blockId);
    } else if (blockNumber) {
      loadBlockByNumber(parseInt(blockNumber));
    } else if (blockHash) {
      loadBlockByHash(blockHash);
    } else {
      setError("No block identifier provided.");
      setIsLoading(false);
    }
  }, [searchParams]);

  const loadBlockById = async (id) => {
    setIsLoading(true);
    setError(null);
    try {
      const blockData = await Block.get(id);
      setBlock(blockData);
      await loadTransactionsForBlock(blockData);
    } catch (err) {
      setError("Block not found.");
      console.error(err);
    } finally {
      setIsLoading(false);
    }
  };

  const loadBlockByNumber = async (blockNumber) => {
    setIsLoading(true);
    setError(null);
    try {
      const blocks = await Block.filter({ block_number: blockNumber });
      if (blocks.length > 0) {
        setBlock(blocks[0]);
        await loadTransactionsForBlock(blocks[0]);
      } else {
        setError("Block not found.");
      }
    } catch (err) {
      setError("Failed to load block details.");
      console.error(err);
    } finally {
      setIsLoading(false);
    }
  };

  const loadBlockByHash = async (blockHash) => {
    setIsLoading(true);
    setError(null);
    try {
      const blocks = await Block.filter({ block_hash: blockHash });
      if (blocks.length > 0) {
        setBlock(blocks[0]);
        await loadTransactionsForBlock(blocks[0]);
      } else {
        setError("Block not found.");
      }
    } catch (err) {
      setError("Failed to load block details.");
      console.error(err);
    } finally {
      setIsLoading(false);
    }
  };

  const loadTransactionsForBlock = async (blockData) => {
    try {
      const transactionData = await Transaction.filter({ block_number: blockData.block_number }, '-timestamp');
      setTransactions(transactionData);
    } catch (err) {
      console.error('Error loading transactions for block:', err);
      // Don't set error state here, just log it
    }
  };

  const formatHash = (hash, len = 6) => {
    if (!hash) return '';
    return `${hash.slice(0, len)}...${hash.slice(-len)}`;
  };

  if (isLoading) return <div className="p-6">Loading block details...</div>;
  if (error) return <div className="p-6 text-red-500">{error}</div>;
  if (!block) return <div className="p-6">Block not found.</div>;

  return (
    <div className="p-6 space-y-8">
      <DetailHeader 
        title={`Block #${block.block_number}`}
        subtitle="Details for this blockchain block"
        icon={Blocks}
        badge={<StatusBadge status={block.validation_status} />}
      />

      <Card className="bg-white/60 backdrop-blur-sm border-white/80 shadow-lg">
        <CardHeader>
          <CardTitle>Block Details</CardTitle>
        </CardHeader>
        <CardContent>
          <dl>
            <DetailItem label="Block Height" isHash={false}>{block.block_number}</DetailItem>
            <DetailItem label="Timestamp" isHash={false}>
              {format(new Date(block.timestamp), 'PPP p')} ({formatDistanceToNow(new Date(block.timestamp), { addSuffix: true })})
            </DetailItem>
            <DetailItem label="Block Hash" isHash copyValue={block.block_hash}>
              <Link to={createPageUrl(`BlockDetails?hash=${block.block_hash}`)} className="text-blue-600 hover:underline">{block.block_hash}</Link>
            </DetailItem>
            <DetailItem label="Previous Hash" isHash copyValue={block.previous_hash}>
              <Link to={createPageUrl(`BlockDetails?hash=${block.previous_hash}`)} className="text-blue-600 hover:underline">{block.previous_hash}</Link>
            </DetailItem>
            <DetailItem label="Validator" isHash copyValue={block.validator_address}>
              <Link to={createPageUrl(`AddressDetails?address=${block.validator_address}`)} className="text-blue-600 hover:underline">{block.validator_address}</Link>
            </DetailItem>
            <DetailItem label="Transaction Count" isHash={false}>{block.transaction_count}</DetailItem>
            <DetailItem label="Block Size" isHash={false}>{block.block_size?.toLocaleString()} bytes</DetailItem>
            <DetailItem label="Gas Used" isHash={false}>{block.gas_used?.toLocaleString()}</DetailItem>
            <DetailItem label="Gas Limit" isHash={false}>{block.gas_limit?.toLocaleString()}</DetailItem>
          </dl>
        </CardContent>
      </Card>

      <Card className="bg-white/60 backdrop-blur-sm border-white/80 shadow-lg">
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <ArrowRightLeft className="w-5 h-5" />
            Transactions in this Block ({transactions.length})
          </CardTitle>
        </CardHeader>
        <CardContent className="p-0">
          {transactions.length === 0 ? (
            <div className="p-8 text-center text-slate-500">
              No transactions found in this block.
            </div>
          ) : (
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Txn Hash</TableHead>
                  <TableHead>From</TableHead>
                  <TableHead>To</TableHead>
                  <TableHead>Value</TableHead>
                  <TableHead>Status</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {transactions.map(tx => (
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
                    <TableCell><StatusBadge status={tx.status} size="sm" /></TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          )}
        </CardContent>
      </Card>
    </div>
  );
}