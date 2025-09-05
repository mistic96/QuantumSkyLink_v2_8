import React, { useState } from 'react';
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogFooter, DialogDescription } from "@/components/ui/dialog";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Badge } from "@/components/ui/badge";
import { Separator } from "@/components/ui/separator";
import { Wallet, CreditCard, ArrowRight, AlertTriangle, CheckCircle, XCircle, Mail } from "lucide-react";
import StatusBadge from "../ui/StatusBadge";

export default function QuickBuyModal({ token, isOpen, onClose }) {
  const [amount, setAmount] = useState('');
  const [email, setEmail] = useState('');
  const [paymentMethod, setPaymentMethod] = useState('wallet');
  const [isProcessing, setIsProcessing] = useState(false);
  const [showConfirmation, setShowConfirmation] = useState(false);
  const [showFailure, setShowFailure] = useState(false);
  const [errorMessage, setErrorMessage] = useState('');
  const [cardDetails, setCardDetails] = useState({ 
    number: '', 
    expiry: '', 
    cvc: '', 
    name: '' 
  });
  const [walletDetails, setWalletDetails] = useState({
    address: '',
    privateKey: ''
  });

  if (!token) return null;

  const tokenPrice = 0.85; // Mock price in ETH
  const totalCost = amount ? (parseFloat(amount) * tokenPrice).toFixed(6) : '0';
  const gasFee = 0.002; // Mock gas fee
  const totalWithFees = amount ? (parseFloat(totalCost) + gasFee).toFixed(6) : '0';

  const resetState = () => {
    setShowConfirmation(false);
    setShowFailure(false);
    setIsProcessing(false);
    setAmount('');
    setEmail('');
    setCardDetails({ number: '', expiry: '', cvc: '', name: '' });
    setWalletDetails({ address: '', privateKey: '' });
    setErrorMessage('');
    onClose();
  };

  const validateEmail = (email) => {
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return emailRegex.test(email);
  };
  
  const validateCardDetails = () => {
    if (!cardDetails.name.trim()) return "Cardholder name is required.";
    if (cardDetails.number.replace(/\s/g, '').length !== 16) return "Invalid card number.";
    if (!/^(0[1-9]|1[0-2])\/?([0-9]{2})$/.test(cardDetails.expiry)) return "Invalid expiry date.";
    if (cardDetails.cvc.length !== 3) return "Invalid CVC.";
    return null;
  };

  const validateWalletDetails = () => {
    if (!walletDetails.address.trim()) return "Wallet address is required.";
    if (walletDetails.address.length !== 42 || !walletDetails.address.startsWith('0x')) {
      return "Invalid wallet address format.";
    }
    if (!walletDetails.privateKey.trim()) return "Private key is required for transaction signing.";
    if (walletDetails.privateKey.length !== 64) return "Invalid private key format.";
    return null;
  };

  const handleBuy = async () => {
    if (!amount || parseFloat(amount) <= 0) {
      setErrorMessage("Please enter a valid amount.");
      setShowFailure(true);
      return;
    }

    if (!validateEmail(email)) {
      setErrorMessage("Please enter a valid email address.");
      setShowFailure(true);
      return;
    }
    
    let validationError = null;
    if (paymentMethod === 'card') {
      validationError = validateCardDetails();
    } else if (paymentMethod === 'wallet') {
      validationError = validateWalletDetails();
    }

    if (validationError) {
      setErrorMessage(validationError);
      setShowFailure(true);
      return;
    }
    
    setIsProcessing(true);
    setShowFailure(false);
    
    // Simulate API call with random success/failure
    setTimeout(() => {
      setIsProcessing(false);
      const isSuccess = Math.random() > 0.25; // 75% chance of success

      if (isSuccess) {
        setShowConfirmation(true);
        setTimeout(resetState, 4000);
      } else {
        const errors = [
          "Insufficient funds in your account.",
          "Transaction was rejected by the network.",
          "Invalid payment method or expired card.",
          "Network congestion - please try again later.",
          "Security check failed - please verify your details."
        ];
        setErrorMessage(errors[Math.floor(Math.random() * errors.length)]);
        setShowFailure(true);
      }
    }, 2500);
  };
  
  const handleCardInputChange = (e) => {
    const { name, value } = e.target;
    let formattedValue = value;
    if (name === 'number') {
      formattedValue = value.replace(/[^\d]/g, '').replace(/(.{4})/g, '$1 ').trim();
    }
    if (name === 'expiry') {
      formattedValue = value.replace(/[^\d]/g, '').replace(/(\d{2})(\d{1,2})/, '$1/$2').slice(0, 5);
    }
    if (name === 'cvc') {
      formattedValue = value.replace(/[^\d]/g, '').slice(0, 3);
    }
    setCardDetails(prev => ({ ...prev, [name]: formattedValue }));
  };

  const handleWalletInputChange = (e) => {
    const { name, value } = e.target;
    setWalletDetails(prev => ({ ...prev, [name]: value }));
  };

  const getTokenTypeColor = (type) => {
    const colors = {
      'ERC20': 'bg-blue-100 text-blue-800 border-blue-200',
      'ERC721': 'bg-purple-100 text-purple-800 border-purple-200',
      'ERC1155': 'bg-indigo-100 text-indigo-800 border-indigo-200'
    };
    return colors[type] || 'bg-gray-100 text-gray-800 border-gray-200';
  };

  const renderContent = () => {
    if (showConfirmation) {
      return (
        <div className="flex flex-col items-center text-center space-y-4 py-6">
          <div className="w-16 h-16 bg-green-100 rounded-full flex items-center justify-center">
            <CheckCircle className="w-8 h-8 text-green-600" />
          </div>
          <div>
            <h3 className="text-lg font-semibold text-slate-900">Purchase Successful!</h3>
            <p className="text-slate-600 mt-1">
              You've successfully purchased {amount} {token.symbol} tokens.
            </p>
          </div>
          <div className="bg-slate-50 rounded-lg p-4 w-full space-y-2">
            <div>
              <div className="text-sm text-slate-600">Transaction Hash</div>
              <div className="font-mono text-sm text-blue-600">
                0x{Math.random().toString(36).substring(2, 15)}...{Math.random().toString(36).substring(2, 15)}
              </div>
            </div>
            <div>
              <div className="text-sm text-slate-600">Confirmation Email Sent To</div>
              <div className="text-sm text-slate-900">{email}</div>
            </div>
          </div>
        </div>
      );
    }

    if (showFailure) {
      return (
        <div className="flex flex-col items-center text-center space-y-4 py-6">
          <div className="w-16 h-16 bg-red-100 rounded-full flex items-center justify-center">
            <XCircle className="w-8 h-8 text-red-600" />
          </div>
          <div>
            <h3 className="text-lg font-semibold text-slate-900">Purchase Failed</h3>
            <p className="text-slate-600 mt-1 max-w-sm">
              {errorMessage}
            </p>
          </div>
          <Button onClick={() => setShowFailure(false)}>Try Again</Button>
        </div>
      );
    }

    return (
      <>
        <DialogHeader>
          <DialogTitle className="flex items-center gap-3">
            <div className="w-10 h-10 bg-gradient-to-r from-blue-500 to-purple-500 rounded-full flex items-center justify-center">
              <span className="text-white font-bold text-sm">{token.symbol?.[0]}</span>
            </div>
            Quick Buy {token.symbol}
          </DialogTitle>
        </DialogHeader>

        <div className="space-y-6">
          {/* Email Address - Required for all transactions */}
          <div className="space-y-2">
            <Label htmlFor="email" className="flex items-center gap-2">
              <Mail className="w-4 h-4" />
              Email Address *
            </Label>
            <Input
              id="email"
              type="email"
              placeholder="your.email@example.com"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              className="w-full"
              required
            />
            <p className="text-xs text-slate-500">Required for transaction confirmation and receipt</p>
          </div>

          {/* Purchase Amount */}
          <div className="space-y-4">
            <div className="space-y-2">
              <Label htmlFor="amount">Amount to Purchase *</Label>
              <div className="relative">
                <Input 
                  id="amount" 
                  type="number" 
                  placeholder="0" 
                  value={amount} 
                  onChange={(e) => setAmount(e.target.value)} 
                  className="pr-16 text-lg h-12" 
                />
                <div className="absolute right-4 top-1/2 transform -translate-y-1/2 text-lg text-slate-500 font-mono">
                  {token.symbol}
                </div>
              </div>
            </div>
            <div className="flex gap-2">
              {[100, 500, 1000, 5000].map(q => (
                <Button key={q} variant="outline" size="sm" onClick={() => setAmount(String(q))}>
                  ${q}
                </Button>
              ))}
            </div>
          </div>
          
          {/* Payment Method Selection */}
          <div className="space-y-4">
            <Label>Payment Method *</Label>
            <div className="flex gap-3">
              <Button 
                variant={paymentMethod === 'wallet' ? 'default' : 'outline'} 
                onClick={() => setPaymentMethod('wallet')} 
                className="flex-1"
              >
                <Wallet className="w-4 h-4 mr-2" />
                Crypto Wallet
              </Button>
              <Button 
                variant={paymentMethod === 'card' ? 'default' : 'outline'} 
                onClick={() => setPaymentMethod('card')} 
                className="flex-1"
              >
                <CreditCard className="w-4 h-4 mr-2" />
                Credit Card
              </Button>
            </div>

            {/* Payment Details */}
            {paymentMethod === 'card' && (
              <div className="grid grid-cols-1 gap-4 mt-4 p-4 border rounded-lg bg-slate-50">
                <div className="space-y-2">
                  <Label htmlFor="card-name">Cardholder Name *</Label>
                  <Input 
                    id="card-name" 
                    name="name" 
                    placeholder="John Doe" 
                    value={cardDetails.name} 
                    onChange={handleCardInputChange} 
                  />
                </div>
                <div className="space-y-2">
                  <Label htmlFor="card-number">Card Number *</Label>
                  <Input 
                    id="card-number" 
                    name="number" 
                    placeholder="0000 0000 0000 0000" 
                    value={cardDetails.number} 
                    onChange={handleCardInputChange} 
                    maxLength="19" 
                  />
                </div>
                <div className="grid grid-cols-2 gap-4">
                  <div className="space-y-2">
                    <Label htmlFor="expiry">Expiry Date *</Label>
                    <Input 
                      id="expiry" 
                      name="expiry" 
                      placeholder="MM/YY" 
                      value={cardDetails.expiry} 
                      onChange={handleCardInputChange} 
                    />
                  </div>
                  <div className="space-y-2">
                    <Label htmlFor="cvc">CVC *</Label>
                    <Input 
                      id="cvc" 
                      name="cvc" 
                      placeholder="123" 
                      value={cardDetails.cvc} 
                      onChange={handleCardInputChange} 
                      maxLength="3" 
                    />
                  </div>
                </div>
              </div>
            )}

            {paymentMethod === 'wallet' && (
              <div className="grid grid-cols-1 gap-4 mt-4 p-4 border rounded-lg bg-slate-50">
                <div className="space-y-2">
                  <Label htmlFor="wallet-address">Wallet Address *</Label>
                  <Input 
                    id="wallet-address" 
                    name="address" 
                    placeholder="0x742d35cc6ca4c4e6dd3c8c65a3d3b8c9d4e5f6a7" 
                    value={walletDetails.address} 
                    onChange={handleWalletInputChange}
                    className="font-mono text-sm"
                  />
                </div>
                <div className="space-y-2">
                  <Label htmlFor="private-key">Private Key (for signing) *</Label>
                  <Input 
                    id="private-key" 
                    name="privateKey" 
                    type="password"
                    placeholder="Enter your private key for transaction signing" 
                    value={walletDetails.privateKey} 
                    onChange={handleWalletInputChange}
                    className="font-mono text-sm"
                  />
                  <p className="text-xs text-amber-600 flex items-center gap-1">
                    <AlertTriangle className="w-3 h-3" />
                    Never share your private key. It's only used to sign this transaction.
                  </p>
                </div>
              </div>
            )}
          </div>
          
          {/* Order Summary */}
          {amount && parseFloat(amount) > 0 && (
            <div className="bg-blue-50 rounded-lg p-4 border border-blue-200 space-y-2 text-sm">
              <div className="flex justify-between">
                <span className="text-slate-600">{amount} {token.symbol}</span>
                <span className="font-mono">{totalCost} ETH</span>
              </div>
              <div className="flex justify-between">
                <span className="text-slate-600">Network Fee</span>
                <span className="font-mono">{gasFee} ETH</span>
              </div>
              <Separator />
              <div className="flex justify-between font-semibold">
                <span>Total</span>
                <span className="font-mono">{totalWithFees} ETH</span>
              </div>
            </div>
          )}

          {/* Risk Warning */}
          {token.approval_status !== 'approved' && (
            <div className="bg-amber-50 border border-amber-200 rounded-lg p-3 text-sm flex items-start gap-3">
              <AlertTriangle className="w-5 h-5 text-amber-600 flex-shrink-0 mt-0.5" />
              <div>
                <span className="font-medium text-amber-900">Risk Warning:</span> This token is not fully approved. Please verify details before purchasing.
              </div>
            </div>
          )}
        </div>

        <DialogFooter className="flex gap-3">
          <Button variant="outline" onClick={resetState} disabled={isProcessing}>
            Cancel
          </Button>
          <Button 
            onClick={handleBuy} 
            disabled={!amount || parseFloat(amount) <= 0 || !email || isProcessing} 
            className="bg-gradient-to-r from-blue-600 to-purple-600 hover:from-blue-700 hover:to-purple-700"
          >
            {isProcessing ? (
              <>
                <div className="animate-spin rounded-full h-4 w-4 border-b-2 border-white mr-2"></div>
                Processing...
              </>
            ) : (
              <>
                Buy Now <ArrowRight className="w-4 h-4 ml-2" />
              </>
            )}
          </Button>
        </DialogFooter>
      </>
    );
  };

  return (
    <Dialog open={isOpen} onOpenChange={!isProcessing ? resetState : undefined}>
      <DialogContent className="sm:max-w-md">
        {renderContent()}
      </DialogContent>
    </Dialog>
  );
}