import React from "react";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { motion } from "framer-motion";
import { Building2, MapPin, DollarSign } from "lucide-react";

const assetIcons = {
  real_estate: Building2,
  commodity: Building2,
  security: Building2,
  digital_asset: Building2,
  artwork: Building2,
  intellectual_property: Building2
};

export default function AssetHoldings({ assets, isLoading }) {
  return (
    <motion.div
      initial={{ opacity: 0, y: 20 }}
      animate={{ opacity: 1, y: 0 }}
      transition={{ delay: 0.1 }}
    >
      <Card className="border-0 shadow-xl">
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <Building2 className="w-5 h-5 text-purple-600" />
            Asset Holdings
          </CardTitle>
        </CardHeader>
        <CardContent>
          {!isLoading ? (
            assets.length > 0 ? (
              <div className="space-y-4">
                {assets.map((asset) => {
                  const AssetIcon = assetIcons[asset.type] || Building2;
                  return (
                    <div key={asset.id} className="flex items-center justify-between p-4 bg-gray-50 rounded-xl">
                      <div className="flex items-center gap-4">
                        <div className="w-12 h-12 bg-gradient-to-r from-purple-500 to-pink-500 rounded-xl flex items-center justify-center">
                          <AssetIcon className="w-6 h-6 text-white" />
                        </div>
                        <div>
                          <h3 className="font-semibold text-gray-900">{asset.name}</h3>
                          <div className="flex items-center gap-2 mt-1">
                            <p className="text-sm text-gray-500 capitalize">
                              {asset.type?.replace(/_/g, ' ')}
                            </p>
                            {asset.location && (
                              <>
                                <span className="text-gray-300">•</span>
                                <div className="flex items-center gap-1 text-sm text-gray-500">
                                  <MapPin className="w-3 h-3" />
                                  {asset.location}
                                </div>
                              </>
                            )}
                          </div>
                        </div>
                      </div>
                      <div className="text-right">
                        <p className="font-semibold text-gray-900">
                          ${(asset.valuation || 0).toLocaleString()}
                        </p>
                        <Badge 
                          variant={asset.verification_status === 'verified' ? 'default' : 'secondary'}
                          className="mt-1"
                        >
                          {asset.verification_status || 'pending'}
                        </Badge>
                      </div>
                    </div>
                  );
                })}
              </div>
            ) : (
              <div className="text-center py-12">
                <Building2 className="w-16 h-16 text-gray-300 mx-auto mb-4" />
                <h3 className="text-lg font-semibold text-gray-900 mb-2">No assets yet</h3>
                <p className="text-gray-500">Register your first asset to get started</p>
              </div>
            )
          ) : (
            <div className="space-y-4">
              {[1, 2, 3].map((i) => (
                <div key={i} className="flex items-center justify-between p-4 bg-gray-50 rounded-xl animate-pulse">
                  <div className="flex items-center gap-4">
                    <div className="w-12 h-12 bg-gray-200 rounded-xl"></div>
                    <div className="space-y-2">
                      <div className="h-4 bg-gray-200 rounded w-24"></div>
                      <div className="h-3 bg-gray-200 rounded w-32"></div>
                    </div>
                  </div>
                  <div className="space-y-2">
                    <div className="h-4 bg-gray-200 rounded w-20"></div>
                    <div className="h-6 bg-gray-200 rounded w-16"></div>
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