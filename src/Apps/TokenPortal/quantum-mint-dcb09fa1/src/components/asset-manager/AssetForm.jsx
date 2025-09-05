import React, { useState } from "react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { Textarea } from "@/components/ui/textarea";
import { Checkbox } from "@/components/ui/checkbox";
import { Loader2 } from "lucide-react";

const assetTypes = [
  { value: 'real_estate', label: 'Real Estate' },
  { value: 'commodity', label: 'Commodity' },
  { value: 'security', label: 'Security' },
  { value: 'digital_asset', label: 'Digital Asset' },
  { value: 'artwork', label: 'Artwork' },
  { value: 'intellectual_property', label: 'Intellectual Property' }
];

export default function AssetForm({ onSubmit }) {
  const [formData, setFormData] = useState({
    name: '',
    type: '',
    description: '',
    valuation: '',
    valuation_date: '',
    location: '',
    fractional_ownership: false,
    ownership_percentage: 100,
    revenue_sharing: false,
    transfer_restrictions: '',
    compliance_requirements: []
  });
  const [isSubmitting, setIsSubmitting] = useState(false);

  const handleChange = (field, value) => {
    setFormData(prev => ({ ...prev, [field]: value }));
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setIsSubmitting(true);
    try {
      await onSubmit({
        ...formData,
        valuation: parseFloat(formData.valuation) || 0,
        ownership_percentage: parseFloat(formData.ownership_percentage) || 100
      });
    } catch (error) {
      console.error('Error submitting asset:', error);
    }
    setIsSubmitting(false);
  };

  return (
    <form onSubmit={handleSubmit} className="space-y-6">
      <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
        <div className="space-y-2">
          <Label htmlFor="name">Asset Name</Label>
          <Input
            id="name"
            value={formData.name}
            onChange={(e) => handleChange('name', e.target.value)}
            placeholder="e.g., Downtown Office Building"
            required
          />
        </div>

        <div className="space-y-2">
          <Label htmlFor="type">Asset Type</Label>
          <Select value={formData.type} onValueChange={(value) => handleChange('type', value)}>
            <SelectTrigger>
              <SelectValue placeholder="Select asset type" />
            </SelectTrigger>
            <SelectContent>
              {assetTypes.map(type => (
                <SelectItem key={type.value} value={type.value}>
                  {type.label}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>
        </div>
      </div>

      <div className="space-y-2">
        <Label htmlFor="description">Description</Label>
        <Textarea
          id="description"
          value={formData.description}
          onChange={(e) => handleChange('description', e.target.value)}
          placeholder="Detailed description of the asset..."
          rows={3}
        />
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
        <div className="space-y-2">
          <Label htmlFor="valuation">Valuation (USD)</Label>
          <Input
            id="valuation"
            type="number"
            value={formData.valuation}
            onChange={(e) => handleChange('valuation', e.target.value)}
            placeholder="1000000"
            required
          />
        </div>

        <div className="space-y-2">
          <Label htmlFor="valuation_date">Valuation Date</Label>
          <Input
            id="valuation_date"
            type="date"
            value={formData.valuation_date}
            onChange={(e) => handleChange('valuation_date', e.target.value)}
          />
        </div>
      </div>

      <div className="space-y-2">
        <Label htmlFor="location">Location</Label>
        <Input
          id="location"
          value={formData.location}
          onChange={(e) => handleChange('location', e.target.value)}
          placeholder="e.g., New York, NY, USA"
        />
      </div>

      <div className="space-y-4">
        <div className="flex items-center space-x-2">
          <Checkbox
            id="fractional_ownership"
            checked={formData.fractional_ownership}
            onCheckedChange={(checked) => handleChange('fractional_ownership', checked)}
          />
          <Label htmlFor="fractional_ownership">Enable fractional ownership</Label>
        </div>

        {formData.fractional_ownership && (
          <div className="space-y-2">
            <Label htmlFor="ownership_percentage">Ownership Percentage (%)</Label>
            <Input
              id="ownership_percentage"
              type="number"
              min="1"
              max="100"
              value={formData.ownership_percentage}
              onChange={(e) => handleChange('ownership_percentage', e.target.value)}
            />
          </div>
        )}

        <div className="flex items-center space-x-2">
          <Checkbox
            id="revenue_sharing"
            checked={formData.revenue_sharing}
            onCheckedChange={(checked) => handleChange('revenue_sharing', checked)}
          />
          <Label htmlFor="revenue_sharing">Enable revenue sharing</Label>
        </div>
      </div>

      <div className="space-y-2">
        <Label htmlFor="transfer_restrictions">Transfer Restrictions</Label>
        <Textarea
          id="transfer_restrictions"
          value={formData.transfer_restrictions}
          onChange={(e) => handleChange('transfer_restrictions', e.target.value)}
          placeholder="Any restrictions on transferring this asset..."
          rows={2}
        />
      </div>

      <div className="flex justify-end gap-3">
        <Button 
          type="submit" 
          disabled={isSubmitting || !formData.name || !formData.type}
          className="bg-gradient-to-r from-blue-600 to-purple-600"
        >
          {isSubmitting ? (
            <>
              <Loader2 className="w-4 h-4 mr-2 animate-spin" />
              Registering...
            </>
          ) : (
            'Register Asset'
          )}
        </Button>
      </div>
    </form>
  );
}