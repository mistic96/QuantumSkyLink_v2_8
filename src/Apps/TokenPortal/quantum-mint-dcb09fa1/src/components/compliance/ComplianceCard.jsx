import React from "react";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { Progress } from "@/components/ui/progress";
import { 
  Shield, 
  CheckCircle, 
  AlertTriangle, 
  Clock, 
  XCircle,
  Eye,
  Users,
  Building,
  Globe,
  FileText,
  DollarSign
} from "lucide-react";
import { format } from "date-fns";

const checkTypeIcons = {
  kyc: Users,
  aml: Shield,
  securities: Building,
  tax: DollarSign,
  jurisdiction: Globe
};

const statusColors = {
  passed: "bg-green-100 text-green-800",
  failed: "bg-red-100 text-red-800",
  pending: "bg-yellow-100 text-yellow-800",
  requires_review: "bg-orange-100 text-orange-800"
};

const statusIcons = {
  passed: CheckCircle,
  failed: XCircle,
  pending: Clock,
  requires_review: AlertTriangle
};

const riskColors = {
  low: "bg-green-50 border-green-200",
  medium: "bg-yellow-50 border-yellow-200",
  high: "bg-red-50 border-red-200",
  critical: "bg-red-100 border-red-300"
};

export default function ComplianceCard({ check, onViewDetails, tokens }) {
  const CheckIcon = checkTypeIcons[check.check_type] || Shield;
  const StatusIcon = statusIcons[check.status];
  const token = tokens.find(t => t.id === check.token_id);

  return (
    <Card className={`hover:shadow-lg transition-all duration-300 border ${riskColors[check.risk_level] || 'border-gray-200'}`}>
      <CardHeader className="pb-3">
        <div className="flex items-start justify-between">
          <div className="flex items-center gap-3">
            <div className="w-10 h-10 bg-gradient-to-r from-green-500 to-teal-500 rounded-lg flex items-center justify-center">
              <CheckIcon className="w-5 h-5 text-white" />
            </div>
            <div>
              <CardTitle className="text-lg capitalize">
                {check.check_type.replace(/_/g, ' ')} Check
              </CardTitle>
              <p className="text-sm text-gray-500">
                {token?.name || 'Unknown Token'}
              </p>
            </div>
          </div>
          <Badge className={`${statusColors[check.status]} border-0`}>
            <StatusIcon className="w-3 h-3 mr-1" />
            {check.status.replace(/_/g, ' ')}
          </Badge>
        </div>
      </CardHeader>
      
      <CardContent className="space-y-4">
        <div className="space-y-2">
          <div className="flex items-center justify-between">
            <span className="text-sm font-medium text-gray-600">Compliance Score</span>
            <span className="font-bold text-lg">
              {check.score || 0}%
            </span>
          </div>
          <Progress value={check.score || 0} className="h-2" />
        </div>

        <div className="space-y-2">
          <div className="flex items-center justify-between text-sm">
            <span className="text-gray-600">Risk Level</span>
            <Badge 
              variant="outline"
              className={check.risk_level === 'low' ? 'text-green-600 border-green-300' :
                         check.risk_level === 'medium' ? 'text-yellow-600 border-yellow-300' :
                         'text-red-600 border-red-300'}
            >
              {check.risk_level}
            </Badge>
          </div>
          
          <div className="flex items-center justify-between text-sm">
            <span className="text-gray-600">Jurisdiction</span>
            <span className="font-medium">{check.jurisdiction || 'Global'}</span>
          </div>

          {check.review_date && (
            <div className="flex items-center justify-between text-sm">
              <span className="text-gray-600">Reviewed</span>
              <span className="font-medium">
                {format(new Date(check.review_date), 'MMM d, yyyy')}
              </span>
            </div>
          )}
        </div>

        <div className="pt-2 border-t">
          <Button 
            variant="outline" 
            size="sm"
            onClick={onViewDetails}
            className="w-full hover:bg-green-50 hover:text-green-600"
          >
            <Eye className="w-4 h-4 mr-2" />
            View Details
          </Button>
        </div>
      </CardContent>
    </Card>
  );
}