import React from "react";
import { Card, CardContent, CardFooter, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Label } from "@/components/ui/label";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { Checkbox } from "@/components/ui/checkbox";
import { ArrowLeft, ArrowRight } from "lucide-react";

const supplyTypes = [
  { value: 'fixed', label: 'Fixed Supply', description: 'Total supply is fixed and cannot be changed' },
  { value: 'mintable', label: 'Mintable', description: 'New tokens can be minted after deployment' },
  { value: 'burnable', label: 'Burnable', description: 'Tokens can be burned to reduce total supply' },
  { value: 'capped', label: 'Capped', description: 'Mintable up to a maximum cap' }
];

export default function AdvancedSettings({ tokenData, updateTokenData, onNext, onPrev }) {
  const handleFeatureChange = (feature, checked) => {
    updateTokenData({
      features: {
        ...tokenData.features,
        [feature]: checked
      }
    });
  };

  const handleSupplyTypeChange = (value) => {
    updateTokenData({ supply_type: value });
  };

  return (
    <Card className="border-0 shadow-xl quantum-glow">
      <CardHeader>
        <CardTitle className="text-2xl font-bold">Advanced Settings</CardTitle>
        <p className="text-gray-600">Configure advanced features and token economics</p>
      </CardHeader>
      <CardContent className="space-y-6">
        <div className="space-y-4">
          <Label className="text-lg font-semibold">Supply Management</Label>
          <Select value={tokenData.supply_type} onValueChange={handleSupplyTypeChange}>
            <SelectTrigger>
              <SelectValue placeholder="Select supply type" />
            </SelectTrigger>
            <SelectContent>
              {supplyTypes.map(type => (
                <SelectItem key={type.value} value={type.value}>
                  <div>
                    <div className="font-medium">{type.label}</div>
                    <div className="text-sm text-gray-500">{type.description}</div>
                  </div>
                </SelectItem>
              ))}
            </SelectContent>
          </Select>
        </div>

        <div className="space-y-4">
          <Label className="text-lg font-semibold">Token Features</Label>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div className="flex items-center space-x-2">
              <Checkbox
                id="mintable"
                checked={tokenData.features.mintable}
                onCheckedChange={(checked) => handleFeatureChange('mintable', checked)}
              />
              <Label htmlFor="mintable" className="text-sm">
                <div>
                  <div className="font-medium">Mintable</div>
                  <div className="text-xs text-gray-500">Owner can mint new tokens</div>
                </div>
              </Label>
            </div>

            <div className="flex items-center space-x-2">
              <Checkbox
                id="burnable"
                checked={tokenData.features.burnable}
                onCheckedChange={(checked) => handleFeatureChange('burnable', checked)}
              />
              <Label htmlFor="burnable" className="text-sm">
                <div>
                  <div className="font-medium">Burnable</div>
                  <div className="text-xs text-gray-500">Tokens can be burned</div>
                </div>
              </Label>
            </div>

            <div className="flex items-center space-x-2">
              <Checkbox
                id="pausable"
                checked={tokenData.features.pausable}
                onCheckedChange={(checked) => handleFeatureChange('pausable', checked)}
              />
              <Label htmlFor="pausable" className="text-sm">
                <div>
                  <div className="font-medium">Pausable</div>
                  <div className="text-xs text-gray-500">Can pause all transfers</div>
                </div>
              </Label>
            </div>

            <div className="flex items-center space-x-2">
              <Checkbox
                id="upgradeable"
                checked={tokenData.features.upgradeable}
                onCheckedChange={(checked) => handleFeatureChange('upgradeable', checked)}
              />
              <Label htmlFor="upgradeable" className="text-sm">
                <div>
                  <div className="font-medium">Upgradeable</div>
                  <div className="text-xs text-gray-500">Contract can be upgraded</div>
                </div>
              </Label>
            </div>
          </div>
        </div>
      </CardContent>
      <CardFooter className="flex justify-between">
        <Button variant="outline" onClick={onPrev}>
          <ArrowLeft className="w-4 h-4 mr-2" />
          Previous
        </Button>
        <Button onClick={onNext}>
          Next Step
          <ArrowRight className="w-4 h-4 ml-2" />
        </Button>
      </CardFooter>
    </Card>
  );
}