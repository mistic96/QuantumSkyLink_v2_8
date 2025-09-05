import React from "react";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { 
  Building2, 
  DollarSign, 
  MapPin, 
  Calendar,
  CheckCircle,
  Clock,
  XCircle,
  Eye,
  Package,
  Gem,
  FileText,
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

export default function AssetCard({ asset, onViewDetails }) {
  const AssetIcon = assetIcons[asset.type] || Building2;
  const StatusIcon = statusIcons[asset.verification_status];

  return (
    <Card className="hover:shadow-lg transition-all duration-300 border-0 shadow-md">
      <CardHeader className="pb-3">
        <div className="flex items-start justify-between">
          <div className="flex items-center gap-3">
            <div className="w-10 h-10 bg-gradient-to-r from-blue-500 to-purple-500 rounded-lg flex items-center justify-center">
              <AssetIcon className="w-5 h-5 text-white" />
            </div>
            <div>
              <CardTitle className="text-lg">{asset.name}</CardTitle>
              <p className="text-sm text-gray-500 capitalize">
                {asset.type.replace(/_/g, ' ')}
              </p>
            </div>
          </div>
          <Badge className={`${statusColors[asset.verification_status]} border-0`}>
            <StatusIcon className="w-3 h-3 mr-1" />
            {asset.verification_status}
          </Badge>
        </div>
      </CardHeader>
      
      <CardContent className="space-y-4">
        <div className="space-y-2">
          <div className="flex items-center justify-between">
            <span className="text-sm text-gray-600">Valuation</span>
            <span className="font-semibold text-lg">
              ${asset.valuation?.toLocaleString()}
            </span>
          </div>
          
          {asset.location && (
            <div className="flex items-center gap-2 text-sm text-gray-600">
              <MapPin className="w-4 h-4" />
              {asset.location}
            </div>
          )}
          
          {asset.valuation_date && (
            <div className="flex items-center gap-2 text-sm text-gray-600">
              <Calendar className="w-4 h-4" />
              Valued on {format(new Date(asset.valuation_date), 'MMM d, yyyy')}
            </div>
          )}
        </div>

        <div className="pt-2 border-t">
          <div className="flex items-center justify-between">
            <div className="flex items-center gap-4 text-sm">
              {asset.fractional_ownership && (
                <Badge variant="outline" className="text-xs">
                  Fractional: {asset.ownership_percentage}%
                </Badge>
              )}
              {asset.revenue_sharing && (
                <Badge variant="outline" className="text-xs">
                  Revenue Share
                </Badge>
              )}
            </div>
            <Button 
              variant="outline" 
              size="sm"
              onClick={onViewDetails}
              className="hover:bg-blue-50 hover:text-blue-600"
            >
              <Eye className="w-4 h-4 mr-1" />
              Details
            </Button>
          </div>
        </div>
      </CardContent>
    </Card>
  );
}