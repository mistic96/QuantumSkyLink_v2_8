import React, { useState } from "react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { Textarea } from "@/components/ui/textarea";
import { Dialog, DialogContent, DialogHeader, DialogTitle } from "@/components/ui/dialog";
import { Loader2 } from "lucide-react";

const checkTypes = [
  { value: 'kyc', label: 'KYC (Know Your Customer)' },
  { value: 'aml', label: 'AML (Anti-Money Laundering)' },
  { value: 'securities', label: 'Securities Compliance' },
  { value: 'tax', label: 'Tax Compliance' },
  { value: 'jurisdiction', label: 'Jurisdictional Compliance' }
];

const jurisdictions = [
  'United States',
  'European Union',
  'United Kingdom',
  'Singapore',
  'Switzerland',
  'Japan',
  'Canada',
  'Australia'
];

export default function ComplianceForm({ tokens, onSubmit, onClose }) {
  const [formData, setFormData] = useState({
    token_id: '',
    check_type: '',
    jurisdiction: '',
    requirements: [],
    documentation_required: [],
    notes: ''
  });
  const [isSubmitting, setIsSubmitting] = useState(false);

  const handleChange = (field, value) => {
    setFormData(prev => ({ ...prev, [field]: value }));
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setIsSubmitting(true);
    try {
      await onSubmit(formData);
    } catch (error) {
      console.error('Error submitting compliance check:', error);
    }
    setIsSubmitting(false);
  };

  return (
    <Dialog open={true} onOpenChange={onClose}>
      <DialogContent className="max-w-2xl max-h-[80vh] overflow-y-auto">
        <DialogHeader>
          <DialogTitle>Run Compliance Check</DialogTitle>
        </DialogHeader>
        
        <form onSubmit={handleSubmit} className="space-y-6">
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div className="space-y-2">
              <Label htmlFor="token_id">Token</Label>
              <Select value={formData.token_id} onValueChange={(value) => handleChange('token_id', value)}>
                <SelectTrigger>
                  <SelectValue placeholder="Select token" />
                </SelectTrigger>
                <SelectContent>
                  {tokens.map(token => (
                    <SelectItem key={token.id} value={token.id}>
                      {token.name} ({token.symbol})
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>

            <div className="space-y-2">
              <Label htmlFor="check_type">Check Type</Label>
              <Select value={formData.check_type} onValueChange={(value) => handleChange('check_type', value)}>
                <SelectTrigger>
                  <SelectValue placeholder="Select check type" />
                </SelectTrigger>
                <SelectContent>
                  {checkTypes.map(type => (
                    <SelectItem key={type.value} value={type.value}>
                      {type.label}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>
          </div>

          <div className="space-y-2">
            <Label htmlFor="jurisdiction">Jurisdiction</Label>
            <Select value={formData.jurisdiction} onValueChange={(value) => handleChange('jurisdiction', value)}>
              <SelectTrigger>
                <SelectValue placeholder="Select jurisdiction" />
              </SelectTrigger>
              <SelectContent>
                {jurisdictions.map(jurisdiction => (
                  <SelectItem key={jurisdiction} value={jurisdiction}>
                    {jurisdiction}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          </div>

          <div className="space-y-2">
            <Label htmlFor="requirements">Requirements (comma-separated)</Label>
            <Input
              id="requirements"
              value={formData.requirements.join(', ')}
              onChange={(e) => handleChange('requirements', e.target.value.split(',').map(r => r.trim()))}
              placeholder="e.g., Identity verification, Address confirmation"
            />
          </div>

          <div className="space-y-2">
            <Label htmlFor="documentation_required">Documentation Required (comma-separated)</Label>
            <Input
              id="documentation_required"
              value={formData.documentation_required.join(', ')}
              onChange={(e) => handleChange('documentation_required', e.target.value.split(',').map(d => d.trim()))}
              placeholder="e.g., Government ID, Utility bill"
            />
          </div>

          <div className="space-y-2">
            <Label htmlFor="notes">Additional Notes</Label>
            <Textarea
              id="notes"
              value={formData.notes}
              onChange={(e) => handleChange('notes', e.target.value)}
              placeholder="Any additional information for this compliance check..."
            />
          </div>

          <div className="flex justify-end gap-3">
            <Button type="button" variant="outline" onClick={onClose}>
              Cancel
            </Button>
            <Button 
              type="submit" 
              disabled={isSubmitting || !formData.token_id || !formData.check_type}
              className="bg-gradient-to-r from-green-600 to-teal-600"
            >
              {isSubmitting ? (
                <>
                  <Loader2 className="w-4 h-4 mr-2 animate-spin" />
                  Running Check...
                </>
              ) : (
                'Run Compliance Check'
              )}
            </Button>
          </div>
        </form>
      </DialogContent>
    </Dialog>
  );
}