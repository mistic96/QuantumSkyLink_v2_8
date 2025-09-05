import React from "react";
import { Card, CardContent, CardFooter, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Label } from "@/components/ui/label";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { Checkbox } from "@/components/ui/checkbox";
import { ArrowLeft, ArrowRight, Shield } from "lucide-react";

const jurisdictions = [
  { value: 'global', label: 'Global' },
  { value: 'united_states', label: 'United States' },
  { value: 'european_union', label: 'European Union' },
  { value: 'united_kingdom', label: 'United Kingdom' },
  { value: 'singapore', label: 'Singapore' },
  { value: 'switzerland', label: 'Switzerland' },
  { value: 'japan', label: 'Japan' },
  { value: 'canada', label: 'Canada' }
];

export default function ComplianceSetup({ tokenData, updateTokenData, onNext, onPrev }) {
  const handleComplianceChange = (field, value) => {
    updateTokenData({
      compliance: {
        ...tokenData.compliance,
        [field]: value
      }
    });
  };

  return (
    <Card className="border-0 shadow-xl quantum-glow">
      <CardHeader>
        <CardTitle className="text-2xl font-bold flex items-center gap-2">
          <Shield className="w-6 h-6 text-green-600" />
          Compliance & Legal
        </CardTitle>
        <p className="text-gray-600">Set up regulatory compliance for your token</p>
      </CardHeader>
      <CardContent className="space-y-6">
        <div className="space-y-4">
          <Label className="text-lg font-semibold">Primary Jurisdiction</Label>
          <Select 
            value={tokenData.compliance.jurisdiction} 
            onValueChange={(value) => handleComplianceChange('jurisdiction', value)}
          >
            <SelectTrigger>
              <SelectValue placeholder="Select jurisdiction" />
            </SelectTrigger>
            <SelectContent>
              {jurisdictions.map(jurisdiction => (
                <SelectItem key={jurisdiction.value} value={jurisdiction.value}>
                  {jurisdiction.label}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>
        </div>

        <div className="space-y-4">
          <Label className="text-lg font-semibold">Compliance Requirements</Label>
          
          <div className="space-y-4">
            <div className="flex items-start space-x-3 p-4 border rounded-lg">
              <Checkbox
                id="kyc_required"
                checked={tokenData.compliance.kyc_required}
                onCheckedChange={(checked) => handleComplianceChange('kyc_required', checked)}
              />
              <Label htmlFor="kyc_required" className="flex-1">
                <div>
                  <div className="font-medium">KYC Required</div>
                  <div className="text-sm text-gray-500">
                    Require Know Your Customer verification for token holders
                  </div>
                </div>
              </Label>
            </div>

            <div className="flex items-start space-x-3 p-4 border rounded-lg">
              <Checkbox
                id="accredited_investors_only"
                checked={tokenData.compliance.accredited_investors_only}
                onCheckedChange={(checked) => handleComplianceChange('accredited_investors_only', checked)}
              />
              <Label htmlFor="accredited_investors_only" className="flex-1">
                <div>
                  <div className="font-medium">Accredited Investors Only</div>
                  <div className="text-sm text-gray-500">
                    Restrict token sales to accredited investors only
                  </div>
                </div>
              </Label>
            </div>
          </div>
        </div>

        <div className="bg-blue-50 border border-blue-200 rounded-lg p-4">
          <div className="flex items-start gap-3">
            <Shield className="w-5 h-5 text-blue-600 mt-0.5" />
            <div>
              <h4 className="font-medium text-blue-900">Compliance Note</h4>
              <p className="text-sm text-blue-700 mt-1">
                These settings help ensure your token complies with relevant regulations. 
                Consider consulting with legal professionals for complex compliance requirements.
              </p>
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
          Review & Deploy
          <ArrowRight className="w-4 h-4 ml-2" />
        </Button>
      </CardFooter>
    </Card>
  );
}