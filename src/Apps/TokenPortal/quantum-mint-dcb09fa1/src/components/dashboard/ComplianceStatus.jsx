import React from "react";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Progress } from "@/components/ui/progress";
import { motion } from "framer-motion";
import { Link } from "react-router-dom";
import { createPageUrl } from "@/utils";
import { 
  Shield, 
  ArrowUpRight, 
  CheckCircle, 
  AlertTriangle, 
  Clock,
  XCircle
} from "lucide-react";
import { Button } from "@/components/ui/button";

const statusIcons = {
  passed: CheckCircle,
  failed: XCircle,
  pending: Clock,
  requires_review: AlertTriangle
};

const statusColors = {
  passed: "text-green-600 bg-green-100",
  failed: "text-red-600 bg-red-100",
  pending: "text-yellow-600 bg-yellow-100",
  requires_review: "text-orange-600 bg-orange-100"
};

export default function ComplianceStatus({ complianceChecks = [], isLoading = false }) {
  const safeChecks = Array.isArray(complianceChecks) ? complianceChecks : [];
  
  const complianceScore = safeChecks.length > 0 
    ? Math.round(safeChecks.reduce((sum, check) => sum + (check?.score || 0), 0) / safeChecks.length)
    : 0;

  const recentChecks = safeChecks.slice(0, 4);

  return (
    <motion.div
      initial={{ opacity: 0, x: 20 }}
      animate={{ opacity: 1, x: 0 }}
      transition={{ delay: 0.3 }}
    >
      <Card className="quantum-glow border-0 shadow-xl">
        <CardHeader className="pb-4">
          <div className="flex items-center justify-between">
            <CardTitle className="flex items-center gap-2 text-xl">
              <Shield className="w-5 h-5 text-green-600" />
              Compliance
            </CardTitle>
            <Link to={createPageUrl("Compliance")}>
              <Button variant="ghost" size="sm" className="text-blue-600 hover:text-blue-700">
                View All <ArrowUpRight className="w-4 h-4 ml-1" />
              </Button>
            </Link>
          </div>
        </CardHeader>
        <CardContent>
          {!isLoading ? (
            <>
              {/* Compliance Score */}
              <div className="mb-6">
                <div className="flex items-center justify-between mb-2">
                  <span className="text-sm font-medium text-gray-700">Overall Score</span>
                  <span className="text-2xl font-bold text-gray-900">{complianceScore}%</span>
                </div>
                <Progress 
                  value={complianceScore} 
                  className="h-3"
                />
                <p className="text-xs text-gray-500 mt-2">
                  {complianceScore >= 80 ? 'Excellent compliance status' : 
                   complianceScore >= 60 ? 'Good compliance status' : 
                   'Needs improvement'}
                </p>
              </div>

              {/* Recent Checks */}
              {recentChecks.length > 0 ? (
                <div className="space-y-3">
                  <h4 className="font-medium text-gray-900">Recent Checks</h4>
                  {recentChecks.map((check) => {
                    const StatusIcon = statusIcons[check?.status] || Clock;
                    return (
                      <div key={check?.id} className="flex items-center justify-between p-3 bg-gray-50 rounded-lg">
                        <div className="flex items-center gap-3">
                          <div className={`w-8 h-8 rounded-full flex items-center justify-center ${statusColors[check?.status] || 'text-gray-600 bg-gray-100'}`}>
                            <StatusIcon className="w-4 h-4" />
                          </div>
                          <div>
                            <p className="font-medium text-gray-900 capitalize">
                              {check?.check_type?.replace(/_/g, ' ') || 'Unknown Check'}
                            </p>
                            <p className="text-xs text-gray-500">
                              {check?.jurisdiction || 'Global'}
                            </p>
                          </div>
                        </div>
                        <Badge 
                          className={`${statusColors[check?.status] || 'text-gray-600 bg-gray-100'} border-0`}
                          variant="secondary"
                        >
                          {check?.status?.replace(/_/g, ' ') || 'pending'}
                        </Badge>
                      </div>
                    );
                  })}
                </div>
              ) : (
                <div className="text-center py-6">
                  <Shield className="w-12 h-12 text-gray-300 mx-auto mb-4" />
                  <p className="text-gray-500 mb-4">No compliance checks yet</p>
                  <Link to={createPageUrl("Compliance")}>
                    <Button size="sm" className="bg-green-600 hover:bg-green-700">
                      Start Compliance Check
                    </Button>
                  </Link>
                </div>
              )}
            </>
          ) : (
            <div className="space-y-4 animate-pulse">
              <div>
                <div className="h-4 bg-gray-200 rounded w-24 mb-2"></div>
                <div className="h-3 bg-gray-200 rounded w-full"></div>
              </div>
              {[1, 2, 3].map((i) => (
                <div key={i} className="flex items-center justify-between p-3 bg-gray-50 rounded-lg">
                  <div className="flex items-center gap-3">
                    <div className="w-8 h-8 bg-gray-200 rounded-full"></div>
                    <div className="space-y-1">
                      <div className="h-4 bg-gray-200 rounded w-20"></div>
                      <div className="h-3 bg-gray-200 rounded w-16"></div>
                    </div>
                  </div>
                  <div className="h-6 bg-gray-200 rounded w-16"></div>
                </div>
              ))}
            </div>
          )}
        </CardContent>
      </Card>
    </motion.div>
  );
}