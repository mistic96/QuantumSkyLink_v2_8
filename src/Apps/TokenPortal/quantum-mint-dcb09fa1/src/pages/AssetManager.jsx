import React, { useState, useEffect } from "react";
import { Asset } from "@/api/entities";
import { UploadFile } from "@/api/integrations";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Textarea } from "@/components/ui/textarea";
import { Label } from "@/components/ui/label";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { Badge } from "@/components/ui/badge";
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogTrigger } from "@/components/ui/dialog";
import { motion } from "framer-motion";
import { 
  Building2, 
  Plus, 
  Upload, 
  FileText, 
  DollarSign,
  MapPin,
  Calendar,
  CheckCircle,
  Clock,
  XCircle,
  Eye
} from "lucide-react";
import { format } from "date-fns";

import AssetCard from "../components/asset-manager/AssetCard";
import AssetForm from "../components/asset-manager/AssetForm";
import AssetDetails from "../components/asset-manager/AssetDetails";

export default function AssetManager() {
  const [assets, setAssets] = useState([]);
  const [selectedAsset, setSelectedAsset] = useState(null);
  const [isFormOpen, setIsFormOpen] = useState(false);
  const [isDetailsOpen, setIsDetailsOpen] = useState(false);
  const [isLoading, setIsLoading] = useState(true);
  const [filter, setFilter] = useState('all');

  useEffect(() => {
    loadAssets();
  }, []);

  const loadAssets = async () => {
    setIsLoading(true);
    try {
      const data = await Asset.list('-created_date');
      setAssets(data);
    } catch (error) {
      console.error('Error loading assets:', error);
    }
    setIsLoading(false);
  };

  const handleAssetSubmit = async (assetData) => {
    try {
      await Asset.create(assetData);
      setIsFormOpen(false);
      loadAssets();
    } catch (error) {
      console.error('Error creating asset:', error);
    }
  };

  const filteredAssets = assets.filter(asset => {
    if (filter === 'all') return true;
    return asset.type === filter;
  });

  const assetTypes = ['real_estate', 'commodity', 'security', 'digital_asset', 'artwork', 'intellectual_property'];

  return (
    <div className="p-6 bg-gradient-to-br from-gray-50 to-white min-h-screen">
      <div className="max-w-7xl mx-auto">
        {/* Header */}
        <motion.div 
          initial={{ opacity: 0, y: 20 }}
          animate={{ opacity: 1, y: 0 }}
          className="flex flex-col lg:flex-row justify-between items-start lg:items-center gap-6 mb-8"
        >
          <div>
            <h1 className="text-3xl font-bold text-gray-900 mb-2">
              Asset <span className="quantum-text-gradient">Manager</span>
            </h1>
            <p className="text-gray-600">
              Register and manage your tokenizable assets
            </p>
          </div>
          
          <Dialog open={isFormOpen} onOpenChange={setIsFormOpen}>
            <DialogTrigger asChild>
              <Button className="bg-gradient-to-r from-blue-600 to-purple-600 hover:from-blue-700 hover:to-purple-700">
                <Plus className="w-4 h-4 mr-2" />
                Register Asset
              </Button>
            </DialogTrigger>
            <DialogContent className="max-w-2xl max-h-[80vh] overflow-y-auto">
              <DialogHeader>
                <DialogTitle>Register New Asset</DialogTitle>
              </DialogHeader>
              <AssetForm onSubmit={handleAssetSubmit} />
            </DialogContent>
          </Dialog>
        </motion.div>

        {/* Filters */}
        <motion.div 
          initial={{ opacity: 0, y: 20 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ delay: 0.1 }}
          className="mb-6"
        >
          <div className="flex gap-2 flex-wrap">
            <Button 
              variant={filter === 'all' ? 'default' : 'outline'}
              onClick={() => setFilter('all')}
              size="sm"
            >
              All Assets ({assets.length})
            </Button>
            {assetTypes.map(type => {
              const count = assets.filter(a => a.type === type).length;
              return (
                <Button
                  key={type}
                  variant={filter === type ? 'default' : 'outline'}
                  onClick={() => setFilter(type)}
                  size="sm"
                >
                  {type.replace(/_/g, ' ').replace(/\b\w/g, l => l.toUpperCase())} ({count})
                </Button>
              );
            })}
          </div>
        </motion.div>

        {/* Assets Grid */}
        {!isLoading ? (
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
            {filteredAssets.map((asset, index) => (
              <motion.div
                key={asset.id}
                initial={{ opacity: 0, y: 20 }}
                animate={{ opacity: 1, y: 0 }}
                transition={{ delay: 0.1 * index }}
              >
                <AssetCard 
                  asset={asset}
                  onViewDetails={() => {
                    setSelectedAsset(asset);
                    setIsDetailsOpen(true);
                  }}
                />
              </motion.div>
            ))}
          </div>
        ) : (
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
            {[1, 2, 3, 4, 5, 6].map((i) => (
              <Card key={i} className="animate-pulse">
                <CardHeader>
                  <div className="h-4 bg-gray-200 rounded w-3/4"></div>
                  <div className="h-3 bg-gray-200 rounded w-1/2"></div>
                </CardHeader>
                <CardContent className="space-y-3">
                  <div className="h-3 bg-gray-200 rounded"></div>
                  <div className="h-3 bg-gray-200 rounded w-2/3"></div>
                  <div className="h-8 bg-gray-200 rounded"></div>
                </CardContent>
              </Card>
            ))}
          </div>
        )}

        {filteredAssets.length === 0 && !isLoading && (
          <motion.div 
            initial={{ opacity: 0 }}
            animate={{ opacity: 1 }}
            className="text-center py-12"
          >
            <Building2 className="w-16 h-16 text-gray-300 mx-auto mb-4" />
            <h3 className="text-lg font-semibold text-gray-900 mb-2">No assets found</h3>
            <p className="text-gray-500 mb-6">
              {filter === 'all' 
                ? 'Get started by registering your first asset' 
                : `No ${filter.replace(/_/g, ' ')} assets registered yet`}
            </p>
            <Button 
              onClick={() => setIsFormOpen(true)}
              className="bg-gradient-to-r from-blue-600 to-purple-600"
            >
              Register Your First Asset
            </Button>
          </motion.div>
        )}

        {/* Asset Details Modal */}
        <Dialog open={isDetailsOpen} onOpenChange={setIsDetailsOpen}>
          <DialogContent className="max-w-4xl max-h-[80vh] overflow-y-auto">
            <DialogHeader>
              <DialogTitle>Asset Details</DialogTitle>
            </DialogHeader>
            {selectedAsset && <AssetDetails asset={selectedAsset} />}
          </DialogContent>
        </Dialog>
      </div>
    </div>
  );
}