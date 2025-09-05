import React from "react";
import { Card, CardContent, CardFooter, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Textarea } from "@/components/ui/textarea";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { ArrowLeft, ArrowRight } from "lucide-react";

const categories = [
  { value: 'utility', label: 'Utility Token' },
  { value: 'governance', label: 'Governance Token' },
  { value: 'security', label: 'Security Token' },
  { value: 'currency', label: 'Currency Token' },
  { value: 'nft', label: 'NFT Collection' },
  { value: 'gaming', label: 'Gaming Token' }
];

const networks = [
  { value: 'ethereum', label: 'Ethereum Mainnet' },
  { value: 'polygon', label: 'Polygon' },
  { value: 'bsc', label: 'Binance Smart Chain' },
  { value: 'avalanche', label: 'Avalanche' },
  { value: 'arbitrum', label: 'Arbitrum' }
];

export default function BasicConfiguration({ tokenData, updateTokenData, onNext, onPrev }) {
  const handleChange = (field, value) => {
    updateTokenData({ [field]: value });
  };

  const canProceed = tokenData.name && tokenData.symbol && tokenData.total_supply > 0;

  return (
    <Card className="border-0 shadow-xl quantum-glow">
      <CardHeader>
        <CardTitle className="text-2xl font-bold">Basic Configuration</CardTitle>
        <p className="text-gray-600">Configure your token's fundamental properties</p>
      </CardHeader>
      <CardContent className="space-y-6">
        <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
          <div className="space-y-2">
            <Label htmlFor="name">Token Name</Label>
            <Input
              id="name"
              placeholder="e.g., QuantumCoin"
              value={tokenData.name}
              onChange={(e) => handleChange('name', e.target.value)}
            />
          </div>
          
          <div className="space-y-2">
            <Label htmlFor="symbol">Token Symbol</Label>
            <Input
              id="symbol"
              placeholder="e.g., QTC"
              value={tokenData.symbol}
              onChange={(e) => handleChange('symbol', e.target.value.toUpperCase())}
              maxLength={5}
            />
          </div>
        </div>

        <div className="space-y-2">
          <Label htmlFor="description">Description</Label>
          <Textarea
            id="description"
            placeholder="Describe your token's purpose and utility..."
            value={tokenData.description}
            onChange={(e) => handleChange('description', e.target.value)}
            rows={3}
          />
        </div>

        <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
          <div className="space-y-2">
            <Label htmlFor="total_supply">Total Supply</Label>
            <Input
              id="total_supply"
              type="number"
              placeholder="1000000"
              value={tokenData.total_supply}
              onChange={(e) => handleChange('total_supply', parseInt(e.target.value) || 0)}
            />
          </div>
          
          <div className="space-y-2">
            <Label htmlFor="decimals">Decimals</Label>
            <Select value={tokenData.decimals.toString()} onValueChange={(value) => handleChange('decimals', parseInt(value))}>
              <SelectTrigger>
                <SelectValue />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="18">18 (Standard)</SelectItem>
                <SelectItem value="8">8</SelectItem>
                <SelectItem value="6">6</SelectItem>
                <SelectItem value="0">0 (No decimals)</SelectItem>
              </SelectContent>
            </Select>
          </div>
        </div>

        <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
          <div className="space-y-2">
            <Label htmlFor="category">Category</Label>
            <Select value={tokenData.category} onValueChange={(value) => handleChange('category', value)}>
              <SelectTrigger>
                <SelectValue placeholder="Select category" />
              </SelectTrigger>
              <SelectContent>
                {categories.map(cat => (
                  <SelectItem key={cat.value} value={cat.value}>
                    {cat.label}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          </div>
          
          <div className="space-y-2">
            <Label htmlFor="blockchain_network">Blockchain Network</Label>
            <Select value={tokenData.blockchain_network} onValueChange={(value) => handleChange('blockchain_network', value)}>
              <SelectTrigger>
                <SelectValue />
              </SelectTrigger>
              <SelectContent>
                {networks.map(network => (
                  <SelectItem key={network.value} value={network.value}>
                    {network.label}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          </div>
        </div>
      </CardContent>
      <CardFooter className="flex justify-between">
        <Button variant="outline" onClick={onPrev}>
          <ArrowLeft className="w-4 h-4 mr-2" />
          Previous
        </Button>
        <Button onClick={onNext} disabled={!canProceed}>
          Next Step
          <ArrowRight className="w-4 h-4 ml-2" />
        </Button>
      </CardFooter>
    </Card>
  );
}