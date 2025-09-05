import React from "react";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { 
  Building2, 
  DollarSign, 
  MapPin, 
  Calendar,
  CheckCircle,
  Clock,
  XCircle,
  FileText,
  ExternalLink,
  Package,
  Gem,
  Image,
  Brain
} from "lucide-react";
import { format } from "date-fns";

const assetIcons = {
  real_estate: Building2,
  commodity: Package,
  security: FileText,
  digital_asset: Image,
  artwork: Gem,
  intellectual_property: Brain
};

const statusColors = {
  verified: "bg-green-100 text-green-800",
  pending: "bg-yellow-100 text-yellow-800",
  rejected: "bg-red-100 text-red-800"
};

const statusIcons = {
  verified: CheckCircle,
  pending: Clock,
  rejected: XCircle
};

export default function AssetDetails({ asset }) {
  const AssetIcon = assetIcons[asset.type] || Building2;
  const StatusIcon = statusIcons[asset.verification_status];

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-start justify-between">
        <div className="flex items-center gap-4">
          <div className="w-12 h-12 bg-gradient-to-r from-blue-500 to-purple-500 rounded-xl flex items-center justify-center">
            <AssetIcon className="w-6 h-6 text-white" />
          </div>
          <div>
            <h2 className="text-2xl font-bold text-gray-900">{asset.name}</h2>
            <p className="text-gray-500 capitalize">{asset.type.replace(/_/g, ' ')}</p>
          </div>
        </div>
        <Badge className={`${statusColors[asset.verification_status]} border-0`}>
          <StatusIcon className="w-4 h-4 mr-1" />
          {asset.verification_status}
        </Badge>
      </div>

      {/* Key Metrics */}
      <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
        <div className="bg-gradient-to-r from-blue-50 to-blue-100 p-4 rounded-lg">
          <div className="flex items-center gap-2 mb-2">
            <DollarSign className="w-5 h-5 text-blue-600" />
            <span className="font-medium text-blue-900">Valuation</span>
          </div>
          <p className="text-2xl font-bold text-blue-900">
            ${asset.valuation?.toLocaleString()}
          </p>
        </div>
        
        {asset.ownership_percentage && (
          <div className="bg-gradient-to-r from-purple-50 to-purple-100 p-4 rounded-lg">
            <div className="flex items-center gap-2 mb-2">
              <Package className="w-5 h-5 text-purple-600" />
              <span className="font-medium text-purple-900">Ownership</span>
            </div>
            <p className="text-2xl font-bold text-purple-900">
              {asset.ownership_percentage}%
            </p>
          </div>
        )}

        {asset.valuation_date && (
          <div className="bg-gradient-to-r from-green-50 to-green-100 p-4 rounded-lg">
            <div className="flex items-center gap-2 mb-2">
              <Calendar className="w-5 h-5 text-green-600" />
              <span className="font-medium text-green-900">Last Valued</span>
            </div>
            <p className="text-lg font-bold text-green-900">
              {format(new Date(asset.valuation_date), 'MMM d, yyyy')}
            </p>
          </div>
        )}
      </div>

      {/* Description */}
      <div>
        <h3 className="text-lg font-semibold text-gray-900 mb-2">Description</h3>
        <p className="text-gray-600 leading-relaxed">{asset.description}</p>
      </div>

      {/* Details Grid */}
      <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
        <div>
          <h3 className="text-lg font-semibold text-gray-900 mb-4">Asset Information</h3>
          <div className="space-y-3">
            {asset.location && (
              <div className="flex items-center gap-3">
                <MapPin className="w-5 h-5 text-gray-400" />
                <span className="text-gray-700">{asset.location}</span>
              </div>
            )}
            
            <div className="flex items-center gap-3">
              <Package className="w-5 h-5 text-gray-400" />
              <span className="text-gray-700">
                {asset.fractional_ownership ? 'Fractional Ownership Enabled' : 'Whole Asset Only'}
              </span>
            </div>
            
            <div className="flex items-center gap-3">
              <DollarSign className="w-5 h-5 text-gray-400" />
              <span className="text-gray-700">
                {asset.revenue_sharing ? 'Revenue Sharing Enabled' : 'No Revenue Sharing'}
              </span>
            </div>
          </div>
        </div>

        <div>
          <h3 className="text-lg font-semibold text-gray-900 mb-4">Compliance</h3>
          <div className="space-y-3">
            {asset.compliance_requirements && asset.compliance_requirements.length > 0 ? (
              <div>
                <p className="text-sm text-gray-500 mb-2">Required Certifications:</p>
                <div className="space-y-1">
                  {asset.compliance_requirements.map((req, index) => (
                    <Badge key={index} variant="outline" className="mr-2 mb-1">
                      {req}
                    </Badge>
                  ))}
                </div>
              </div>
            ) : (
              <p className="text-gray-500">No specific compliance requirements</p>
            )}
            
            {asset.transfer_restrictions && (
              <div>
                <p className="text-sm text-gray-500 mb-1">Transfer Restrictions:</p>
                <p className="text-gray-700 text-sm">{asset.transfer_restrictions}</p>
              </div>
            )}
          </div>
        </div>
      </div>

      {/* Documentation */}
      {asset.documentation_urls && asset.documentation_urls.length > 0 && (
        <div>
          <h3 className="text-lg font-semibold text-gray-900 mb-4">Documentation</h3>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-3">
            {asset.documentation_urls.map((url, index) => (
              <a
                key={index}
                href={url}
                target="_blank"
                rel="noopener noreferrer"
                className="flex items-center gap-3 p-3 border rounded-lg hover:bg-gray-50 transition-colors"
              >
                <FileText className="w-5 h-5 text-blue-600" />
                <span className="text-gray-700">Document {index + 1}</span>
                <ExternalLink className="w-4 h-4 text-gray-400 ml-auto" />
              </a>
            ))}
          </div>
        </div>
      )}
    </div>
  );
}