import React from 'react';
import ReactMarkdown from 'react-markdown';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { BookText } from 'lucide-react';

const markdownContent = `
# Quantum Ledger - API Endpoint Documentation

This document outlines the required API endpoints and data structures to fully support the Quantum Ledger blockchain explorer application. This specification should be provided to the backend development team.

## Core Entity Endpoints

All list endpoints should support pagination, sorting, and filtering query parameters:
- \`?page={number}\` - Page number for pagination.
- \`?limit={number}\` - Number of items per page.
- \`?sort={field}\` - Field to sort by (e.g., \`timestamp\`).
- \`?order={asc|desc}\` - Sort order.

### Blocks
- \`GET /api/blocks\` - List blocks.
- \`GET /api/blocks/:id\` - Get a specific block by its database ID.
- \`GET /api/blocks/by-number/:blockNumber\` - Get a block by its sequential block number.
- \`GET /api/blocks/by-hash/:blockHash\` - Get a block by its unique hash.

### Transactions
- \`GET /api/transactions\` - List transactions.
- \`GET /api/transactions/:id\` - Get a specific transaction by its database ID.
- \`GET /api/transactions/by-hash/:txHash\` - Get a transaction by its unique hash.
- \`GET /api/transactions/by-block/:blockNumber\` - Get all transactions within a specific block.

### Addresses
- \`GET /api/addresses\` - List addresses.
- \`GET /api/addresses/:address\` - Get details for a specific wallet address.
- \`GET /api/addresses/:address/transactions\` - Get all transactions involving a specific address.
- \`GET /api/addresses/:address/token-holdings\` - Get all token balances for a specific address.

### Tokens
- \`GET /api/tokens\` - List all tokens.
- \`GET /api/tokens/:id\` - Get details for a specific token by its database ID.
- \`GET /api/tokens/:id/holders\` - Get the list of top token holders.
- \`GET /api/tokens/:id/transfers\` - Get the transfer history for a specific token.

## Search Endpoint

- \`GET /api/search?q={query}\` - A universal search endpoint to find an address, transaction hash, or block hash/number.

---

## Data Structures

These schemas define the structure for each core entity in the system. GET requests should return objects matching these schemas. POST/PUT requests should expect payloads matching these schemas (excluding read-only fields like \`id\`, \`created_date\`).

### Block Schema
\`\`\`json
{
  "block_number": { "type": "number" },
  "block_hash": { "type": "string" },
  "previous_hash": { "type": "string" },
  "timestamp": { "type": "string", "format": "date-time" },
  "transaction_count": { "type": "number" },
  "block_size": { "type": "number" },
  "validation_status": { "type": "string", "enum": ["validated", "pending", "invalid"] },
  "validator_address": { "type": "string" },
  "gas_used": { "type": "number" },
  "gas_limit": { "type": "number" }
}
\`\`\`

### Transaction Schema
\`\`\`json
{
  "transaction_id": { "type": "string" },
  "block_number": { "type": "number" },
  "from_address": { "type": "string" },
  "to_address": { "type": "string" },
  "amount": { "type": "number" },
  "fee": { "type": "number" },
  "status": { "type": "string", "enum": ["pending", "confirmed", "failed", "rejected"] },
  "timestamp": { "type": "string", "format": "date-time" },
  "gas_used": { "type": "number" },
  "gas_price": { "type": "number" },
  "nonce": { "type": "number" },
  "metadata": { "type": "string" }
}
\`\`\`

### Address Schema
\`\`\`json
{
  "address": { "type": "string" },
  "balance": { "type": "number" },
  "nonce": { "type": "number" },
  "transaction_count": { "type": "number" },
  "last_active": { "type": "string", "format": "date-time" },
  "address_type": { "type": "string", "enum": ["wallet", "contract", "validator"] },
  "metadata": { "type": "string" }
}
\`\`\`

### Token Schema
\`\`\`json
{
  "token_id": { "type": "string" },
  "name": { "type": "string" },
  "symbol": { "type": "string" },
  "total_supply": { "type": "number" },
  "decimals": { "type": "number" },
  "token_type": { "type": "string", "enum": ["ERC20", "ERC721", "ERC1155"] },
  "status": { "type": "string", "enum": ["active", "pending", "suspended", "burned"] },
  "approval_status": { "type": "string", "enum": ["pending", "approved", "rejected"] },
  "creator_id": { "type": "string" },
  "creation_timestamp": { "type": "string", "format": "date-time" },
  "asset_type": { "type": "string", "enum": ["real_estate", "commodity", "security", "digital"] },
  "description": { "type": "string" },
  "multichain_name": { "type": "string" },
  "cross_chain_status": { "type": "string", "enum": ["single_chain", "multi_chain", "bridge_pending"] },
  "contract_address": { "type": "string" }
}
\`\`\`
`;

export default function ApiDocumentation() {
  return (
    <div className="p-6">
      <div className="flex items-center gap-3 mb-6">
        <BookText className="w-8 h-8 text-blue-600" />
        <h1 className="text-3xl font-bold text-slate-900">API Documentation</h1>
      </div>
      <Card className="bg-white/80 backdrop-blur-sm shadow-lg">
        <CardContent className="p-6">
          <ReactMarkdown
            className="prose max-w-none"
            components={{
              h1: ({node, ...props}) => <h1 className="text-2xl font-bold mb-4" {...props} />,
              h2: ({node, ...props}) => <h2 className="text-xl font-semibold mt-6 mb-3 border-b pb-2" {...props} />,
              h3: ({node, ...props}) => <h3 className="text-lg font-semibold mt-4 mb-2" {...props} />,
              ul: ({node, ...props}) => <ul className="list-disc pl-6 space-y-1" {...props} />,
              code: ({node, inline, ...props}) => inline 
                ? <code className="bg-slate-100 text-slate-800 rounded px-1 py-0.5 text-sm font-mono" {...props} />
                : <pre className="bg-slate-800 text-white p-4 rounded-md overflow-x-auto"><code {...props} /></pre>,
            }}
          >
            {markdownContent}
          </ReactMarkdown>
        </CardContent>
      </Card>
    </div>
  );
}