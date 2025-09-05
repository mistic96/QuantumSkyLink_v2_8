
import React, { useState, useEffect } from 'react';
import { useSearchParams, Link } from 'react-router-dom';
import { Transaction, Block } from '@/api/entities';
import DetailHeader from '../components/details/DetailHeader';
import DetailItem from '../components/details/DetailItem';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { ArrowRightLeft } from 'lucide-react';
import StatusBadge from '../components/ui/StatusBadge';
import { format, formatDistanceToNow } from 'date-fns';
import { createPageUrl } from '@/utils';

export default function TransactionDetails() {
  const [searchParams] = useSearchParams();
  const [transaction, setTransaction] = useState(null);
  const [block, setBlock] = useState(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState(null);

  useEffect(() => {
    const txId = searchParams.get('id');
    const txHash = searchParams.get('hash');
    
    if (txId) {
      loadTransactionDetails({ id: txId });
    } else if (txHash) {
      loadTransactionDetails({ hash: txHash });
    } else {
      setError("No transaction identifier provided.");
      setIsLoading(false);
    }
  }, [searchParams]);

  const loadTransactionDetails = async (params) => {
    setIsLoading(true);
    setError(null);
    try {
      let txData;
      if (params.id) {
        txData = await Transaction.get(params.id);
      } else if (params.hash) {
        const results = await Transaction.filter({ transaction_id: params.hash });
        if (results.length > 0) {
            txData = results[0];
        }
      }
      
      if (txData) {
        setTransaction(txData);
        if (txData.block_number) {
          try {
            const blocks = await Block.filter({ block_number: txData.block_number });
            if (blocks.length > 0) {
              setBlock(blocks[0]);
            }
          } catch (blockErr) {
            console.error('Error loading block:', blockErr);
            // Don't fail the whole page if block loading fails
          }
        }
      } else {
        setError("Transaction not found.");
      }
    } catch (err) {
      setError("Failed to load transaction details.");
      console.error(err);
    } finally {
      setIsLoading(false);
    }
  };

  if (isLoading) return <div className="p-6">Loading transaction details...</div>;
  if (error) return <div className="p-6 text-red-500">{error}</div>;
  if (!transaction) return <div className="p-6">Transaction not found.</div>;

  return (
    <div className="p-6 space-y-8">
      <DetailHeader 
        title="Transaction Details"
        subtitle={transaction.transaction_id}
        icon={ArrowRightLeft}
        badge={<StatusBadge status={transaction.status} />}
      />

      <Card className="bg-white/60 backdrop-blur-sm border-white/80 shadow-lg">
        <CardHeader>
          <CardTitle>Transaction Information</CardTitle>
        </CardHeader>
        <CardContent>
          <dl>
            <DetailItem label="Transaction Hash" isHash copyValue={transaction.transaction_id}>
              {transaction.transaction_id}
            </DetailItem>
            <DetailItem label="Block Number" isHash={false}>
              {block ? (
                <Link to={createPageUrl(`BlockDetails?number=${transaction.block_number}`)} className="text-blue-600 hover:underline">
                  {transaction.block_number}
                </Link>
              ) : (
                transaction.block_number
              )}
            </DetailItem>
            <DetailItem label="Timestamp" isHash={false}>
              {transaction.timestamp ? `${format(new Date(transaction.timestamp), 'PPP p')} (${formatDistanceToNow(new Date(transaction.timestamp), { addSuffix: true })})` : 'N/A'}
            </DetailItem>
            <DetailItem label="From" isHash copyValue={transaction.from_address}>
              <Link to={createPageUrl(`AddressDetails?address=${transaction.from_address}`)} className="text-blue-600 hover:underline">
                {transaction.from_address}
              </Link>
            </DetailItem>
            <DetailItem label="To" isHash copyValue={transaction.to_address}>
              <Link to={createPageUrl(`AddressDetails?address=${transaction.to_address}`)} className="text-blue-600 hover:underline">
                {transaction.to_address}
              </Link>
            </DetailItem>
            <DetailItem label="Value" isHash={false}>{transaction.amount?.toFixed(8)} ETH</DetailItem>
            <DetailItem label="Transaction Fee" isHash={false}>{transaction.fee?.toFixed(8)} ETH</DetailItem>
            <DetailItem label="Gas Price" isHash={false}>{transaction.gas_price} Gwei</DetailItem>
            <DetailItem label="Gas Used" isHash={false}>{transaction.gas_used?.toLocaleString()}</DetailItem>
            <DetailItem label="Nonce" isHash={false}>{transaction.nonce}</DetailItem>
            <DetailItem label="Metadata" isHash={false}>
              <pre className="whitespace-pre-wrap font-sans text-sm max-w-full overflow-auto">
                {transaction.metadata || 'No metadata'}
              </pre>
            </DetailItem>
          </dl>
        </CardContent>
      </Card>
    </div>
  );
}
