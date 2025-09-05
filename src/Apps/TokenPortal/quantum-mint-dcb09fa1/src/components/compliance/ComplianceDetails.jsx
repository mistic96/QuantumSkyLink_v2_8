import React from "react";
import { Badge } from "@/components/ui/badge";
import { Progress } from "@/components/ui/progress";
import { Dialog, DialogContent, DialogHeader, DialogTitle } from "@/components/ui/dialog";
import { 
  Shield, 
  CheckCircle, 
  AlertTriangle, 
  Clock, 
  XCircle,
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

export default function ComplianceDetails({ check, tokens, onClose }) {
  const CheckIcon = checkTypeIcons[check.check_type] || Shield;
  const StatusIcon = statusIcons[check.status];
  const token = tokens.find(t => t.id === check.token_id);

  return (
    <Dialog open={true} onOpenChange={onClose}>
      <DialogContent className="max-w-4xl max-h-[80vh] overflow-y-auto">
        <DialogHeader>
          <DialogTitle>Compliance Check Details</DialogTitle>
        </DialogHeader>
        
        <div className="space-y-6">
          {/* Header */}
          <div className="flex items-start justify-between">
            <div className="flex items-center gap-4">
              <div className="w-12 h-12 bg-gradient-to-r from-green-500 to-teal-500 rounded-xl flex items-center justify-center">
                <CheckIcon className="w-6 h-6 text-white" />
              </div>
              <div>
                <h2 className="text-2xl font-bold text-gray-900 capitalize">
                  {check.check_type.replace(/_/g, ' ')} Compliance Check
                </h2>
                <p className="text-gray-500">{token?.name || 'Unknown Token'}</p>
              </div>
            </div>
            <Badge className={`${statusColors[check.status]} border-0`}>
              <StatusIcon className="w-4 h-4 mr-1" />
              {check.status.replace(/_/g, ' ')}
            </Badge>
          </div>

          {/* Score */}
          <div className="bg-gradient-to-r from-green-50 to-teal-50 p-6 rounded-xl">
            <div className="flex items-center justify-between mb-4">
              <h3 className="text-lg font-semibold text-gray-900">Compliance Score</h3>
              <span className="text-3xl font-bold text-green-600">{check.score || 0}%</span>
            </div>
            <Progress value={check.score || 0} className="h-3" />
            <p className="text-sm text-gray-600 mt-2">
              {check.score >= 80 ? 'Excellent compliance rating' :
               check.score >= 60 ? 'Good compliance rating' :
               'Needs improvement'}
            </p>
          </div>

          {/* Details Grid */}
          <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
            <div>
              <h3 className="text-lg font-semibold text-gray-900 mb-4">Check Information</h3>
              <div className="space-y-3">
                <div className="flex justify-between">
                  <span className="text-gray-600">Check Type:</span>
                  <span className="font-medium capitalize">{check.check_type.replace(/_/g, ' ')}</span>
                </div>
                <div className="flex justify-between">
                  <span className="text-gray-600">Jurisdiction:</span>
                  <span className="font-medium">{check.jurisdiction || 'Global'}</span>
                </div>
                <div className="flex justify-between">
                  <span className="text-gray-600">Risk Level:</span>
                  <Badge 
                    variant="outline"
                    className={check.risk_level === 'low' ? 'text-green-600 border-green-300' :
                               check.risk_level === 'medium' ? 'text-yellow-600 border-yellow-300' :
                               'text-red-600 border-red-300'}
                  >
                    {check.risk_level}
                  </Badge>
                </div>
                {check.reviewed_by && (
                  <div className="flex justify-between">
                    <span className="text-gray-600">Reviewed By:</span>
                    <span className="font-medium">{check.reviewed_by}</span>
                  </div>
                )}
                {check.review_date && (
                  <div className="flex justify-between">
                    <span className="text-gray-600">Review Date:</span>
                    <span className="font-medium">
                      {format(new Date(check.review_date), 'MMM d, yyyy')}
                    </span>
                  </div>
                )}
              </div>
            </div>

            <div>
              <h3 className="text-lg font-semibold text-gray-900 mb-4">Token Information</h3>
              {token && (
                <div className="space-y-3">
                  <div className="flex justify-between">
                    <span className="text-gray-600">Token Name:</span>
                    <span className="font-medium">{token.name}</span>
                  </div>
                  <div className="flex justify-between">
                    <span className="text-gray-600">Symbol:</span>
                    <span className="font-medium">{token.symbol}</span>
                  </div>
                  <div className="flex justify-between">
                    <span className="text-gray-600">Type:</span>
                    <span className="font-medium">{token.token_type}</span>
                  </div>
                  <div className="flex justify-between">
                    <span className="text-gray-600">Network:</span>
                    <span className="font-medium capitalize">{token.blockchain_network}</span>
                  </div>
                  <div className="flex justify-between">
                    <span className="text-gray-600">Status:</span>
                    <Badge variant="outline">{token.status.replace(/_/g, ' ')}</Badge>
                  </div>
                </div>
              )}
            </div>
          </div>

          {/* Requirements */}
          {check.requirements && check.requirements.length > 0 && (
            <div>
              <h3 className="text-lg font-semibold text-gray-900 mb-4">Requirements</h3>
              <div className="space-y-2">
                {check.requirements.map((requirement, index) => (
                  <div key={index} className="flex items-center gap-2 p-3 bg-gray-50 rounded-lg">
                    <CheckCircle className="w-4 h-4 text-green-600" />
                    <span className="text-gray-700">{requirement}</span>
                  </div>
                ))}
              </div>
            </div>
          )}

          {/* Documentation */}
          {check.documentation_required && check.documentation_required.length > 0 && (
            <div>
              <h3 className="text-lg font-semibold text-gray-900 mb-4">Required Documentation</h3>
              <div className="grid grid-cols-1 md:grid-cols-2 gap-3">
                {check.documentation_required.map((doc, index) => (
                  <div key={index} className="flex items-center gap-3 p-3 border rounded-lg">
                    <FileText className="w-5 h-5 text-blue-600" />
                    <span className="text-gray-700">{doc}</span>
                  </div>
                ))}
              </div>
            </div>
          )}

          {/* Notes */}
          {check.notes && (
            <div>
              <h3 className="text-lg font-semibold text-gray-900 mb-4">Notes</h3>
              <div className="p-4 bg-gray-50 rounded-lg">
                <p className="text-gray-700">{check.notes}</p>
              </div>
            </div>
          )}
        </div>
      </DialogContent>
    </Dialog>
  );
}