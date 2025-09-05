import React from "react";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { motion } from "framer-motion";
import { 
  Activity, 
  Coins, 
  Building2, 
  Shield, 
  Plus,
  CheckCircle,
  Clock
} from "lucide-react";
import { format } from "date-fns";

export default function RecentActivity({ tokens = [], assets = [], isLoading = false }) {
  const safeTokens = Array.isArray(tokens) ? tokens : [];
  const safeAssets = Array.isArray(assets) ? assets : [];

  // Combine and sort all activities by date
  const activities = [
    ...safeTokens.map(token => ({
      id: token?.id || Math.random(),
      type: 'token',
      title: `Token ${token?.name || 'Unknown'} created`,
      description: `${token?.token_type || 'Unknown'} token with symbol ${token?.symbol || 'N/A'}`,
      status: token?.status || 'draft',
      date: token?.created_date || new Date().toISOString(),
      icon: Coins
    })),
    ...safeAssets.map(asset => ({
      id: asset?.id || Math.random(),
      type: 'asset',
      title: `Asset ${asset?.name || 'Unknown'} registered`,
      description: `${asset?.type?.replace(/_/g, ' ') || 'Unknown type'} valued at $${asset?.valuation?.toLocaleString() || '0'}`,
      status: asset?.verification_status || 'pending',
      date: asset?.created_date || new Date().toISOString(),
      icon: Building2
    }))
  ].sort((a, b) => new Date(b.date) - new Date(a.date)).slice(0, 6);

  const getStatusColor = (status) => {
    switch (status) {
      case 'deployed':
      case 'verified':
        return 'bg-green-100 text-green-800';
      case 'pending_review':
      case 'pending':
        return 'bg-yellow-100 text-yellow-800';
      case 'approved':
        return 'bg-blue-100 text-blue-800';
      case 'rejected':
        return 'bg-red-100 text-red-800';
      default:
        return 'bg-gray-100 text-gray-800';
    }
  };

  const getStatusIcon = (status) => {
    switch (status) {
      case 'deployed':
      case 'verified':
      case 'approved':
        return CheckCircle;
      default:
        return Clock;
    }
  };

  return (
    <motion.div
      initial={{ opacity: 0, y: 20 }}
      animate={{ opacity: 1, y: 0 }}
      transition={{ delay: 0.3 }}
    >
      <Card className="quantum-glow border-0 shadow-xl">
        <CardHeader className="pb-4">
          <CardTitle className="flex items-center gap-2 text-xl">
            <Activity className="w-5 h-5 text-purple-600" />
            Recent Activity
          </CardTitle>
        </CardHeader>
        <CardContent>
          {!isLoading ? (
            activities.length > 0 ? (
              <div className="space-y-4">
                {activities.map((activity, index) => {
                  const ActivityIcon = activity?.icon || Activity;
                  const StatusIcon = getStatusIcon(activity?.status);
                  
                  return (
                    <motion.div
                      key={activity?.id || index}
                      initial={{ opacity: 0, x: -20 }}
                      animate={{ opacity: 1, x: 0 }}
                      transition={{ delay: 0.1 * index }}
                      className="flex items-start gap-4 p-4 bg-gradient-to-r from-gray-50 to-white rounded-xl border border-gray-100"
                    >
                      <div className="w-10 h-10 bg-gradient-to-r from-purple-500 to-pink-500 rounded-lg flex items-center justify-center flex-shrink-0">
                        <ActivityIcon className="w-5 h-5 text-white" />
                      </div>
                      <div className="flex-1 min-w-0">
                        <div className="flex items-center gap-2 mb-1">
                          <h4 className="font-medium text-gray-900">{activity?.title || 'Unknown Activity'}</h4>
                          <StatusIcon className="w-4 h-4 text-gray-400" />
                        </div>
                        <p className="text-sm text-gray-600 mb-2">{activity?.description || 'No description'}</p>
                        <div className="flex items-center gap-2">
                          <Badge className={getStatusColor(activity?.status)} variant="secondary">
                            {activity?.status?.replace(/_/g, ' ') || 'unknown'}
                          </Badge>
                          <span className="text-xs text-gray-500">
                            {activity?.date ? format(new Date(activity.date), 'MMM d, yyyy') : 'Unknown date'}
                          </span>
                        </div>
                      </div>
                    </motion.div>
                  );
                })}
              </div>
            ) : (
              <div className="text-center py-8">
                <Activity className="w-12 h-12 text-gray-300 mx-auto mb-4" />
                <p className="text-gray-500">No recent activity</p>
              </div>
            )
          ) : (
            <div className="space-y-4">
              {[1, 2, 3, 4].map((i) => (
                <div key={i} className="flex items-start gap-4 p-4 bg-gray-50 rounded-xl animate-pulse">
                  <div className="w-10 h-10 bg-gray-200 rounded-lg"></div>
                  <div className="flex-1 space-y-2">
                    <div className="h-4 bg-gray-200 rounded w-48"></div>
                    <div className="h-3 bg-gray-200 rounded w-64"></div>
                    <div className="flex gap-2">
                      <div className="h-6 bg-gray-200 rounded w-16"></div>
                      <div className="h-6 bg-gray-200 rounded w-20"></div>
                    </div>
                  </div>
                </div>
              ))}
            </div>
          )}
        </CardContent>
      </Card>
    </motion.div>
  );
}