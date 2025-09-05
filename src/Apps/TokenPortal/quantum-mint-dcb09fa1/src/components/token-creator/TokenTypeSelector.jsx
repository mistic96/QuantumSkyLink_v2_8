import React from "react";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { ArrowRight, Coins, Image, Layers, Building2 } from "lucide-react";

const tokenTypes = [
  {
    id: 'ERC20',
    title: 'ERC-20 Token',
    description: 'Standard fungible token for currencies and utilities',
    icon: Coins,
    features: ['Fungible', 'Transferable', 'Divisible'],
    recommended: 'utility'
  },
  {
    id: 'ERC721',
    title: 'ERC-721 NFT',
    description: 'Non-fungible token for unique digital assets',
    icon: Image,
    features: ['Non-fungible', 'Unique', 'Collectible'],
    recommended: 'nft'
  },
  {
    id: 'ERC1155',
    title: 'ERC-1155 Multi-Token',
    description: 'Multi-token standard for both fungible and non-fungible',
    icon: Layers,
    features: ['Multi-token', 'Efficient', 'Flexible'],
    recommended: 'gaming'
  },
  {
    id: 'Asset-Backed',
    title: 'Asset-Backed Token',
    description: 'Token backed by real-world assets',
    icon: Building2,
    features: ['Asset-backed', 'Regulated', 'Stable'],
    recommended: 'security'
  }
];

export default function TokenTypeSelector({ tokenData, updateTokenData, onNext }) {
  const handleSelect = (tokenType) => {
    updateTokenData({ token_type: tokenType });
    onNext();
  };

  return (
    <Card className="border-0 shadow-xl quantum-glow">
      <CardHeader>
        <CardTitle className="text-2xl font-bold text-center">Choose Your Token Type</CardTitle>
        <p className="text-center text-gray-600">
          Select the token standard that best fits your project
        </p>
      </CardHeader>
      <CardContent>
        <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
          {tokenTypes.map((type) => {
            const IconComponent = type.icon;
            return (
              <div
                key={type.id}
                className={`border-2 rounded-xl p-6 cursor-pointer transition-all duration-300 hover:shadow-lg ${
                  tokenData.token_type === type.id
                    ? 'border-blue-500 bg-blue-50'
                    : 'border-gray-200 hover:border-blue-300'
                }`}
                onClick={() => handleSelect(type.id)}
              >
                <div className="flex items-start gap-4">
                  <div className="w-12 h-12 bg-gradient-to-r from-blue-500 to-purple-500 rounded-xl flex items-center justify-center">
                    <IconComponent className="w-6 h-6 text-white" />
                  </div>
                  <div className="flex-1">
                    <h3 className="text-lg font-semibold text-gray-900 mb-2">
                      {type.title}
                    </h3>
                    <p className="text-gray-600 text-sm mb-4">
                      {type.description}
                    </p>
                    <div className="flex flex-wrap gap-2 mb-4">
                      {type.features.map((feature) => (
                        <Badge key={feature} variant="secondary" className="text-xs">
                          {feature}
                        </Badge>
                      ))}
                    </div>
                    <Badge className="bg-green-100 text-green-800">
                      Best for {type.recommended}
                    </Badge>
                  </div>
                </div>
              </div>
            );
          })}
        </div>
        
        {tokenData.token_type && (
          <div className="mt-8 text-center">
            <Button 
              onClick={onNext}
              className="bg-gradient-to-r from-blue-600 to-purple-600 hover:from-blue-700 hover:to-purple-700"
            >
              Continue with {tokenData.token_type}
              <ArrowRight className="w-4 h-4 ml-2" />
            </Button>
          </div>
        )}
      </CardContent>
    </Card>
  );
}