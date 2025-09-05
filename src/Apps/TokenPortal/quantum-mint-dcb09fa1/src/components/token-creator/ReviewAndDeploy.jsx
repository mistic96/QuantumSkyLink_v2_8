import React, { useState } from "react";
import { Card, CardContent, CardFooter, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { ArrowLeft, Zap } from "lucide-react";
import { Token } from "@/api/entities";
import { useNavigate } from "react-router-dom";
import { createPageUrl } from "@/utils";
import { Alert, AlertDescription, AlertTitle } from "@/components/ui/alert";

export default function ReviewAndDeploy({ tokenData, onPrev }) {
  const [isDeploying, setIsDeploying] = useState(false);
  const [error, setError] = useState(null);
  const navigate = useNavigate();

  const handleDeploy = async () => {
    setIsDeploying(true);
    setError(null);
    try {
      const new_token = { ...tokenData, status: 'deployed' }
      delete new_token.features
      delete new_token.compliance

      await Token.create(new_token);
      navigate(createPageUrl("Dashboard"));
    } catch (e) {
      setError("Failed to deploy token. Please try again.");
      console.error(e);
    } finally {
      setIsDeploying(false);
    }
  };

  return (
    <Card className="border-0 shadow-xl quantum-glow">
      <CardHeader>
        <CardTitle className="text-2xl font-bold">Review & Deploy</CardTitle>
      </CardHeader>
      <CardContent className="space-y-6">
        {error && (
          <Alert variant="destructive">
            <AlertTitle>Error</AlertTitle>
            <AlertDescription>{error}</AlertDescription>
          </Alert>
        )}
        <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
          <div className="space-y-4 p-4 border rounded-lg">
            <h3 className="font-semibold">Token Details</h3>
            <p><strong>Name:</strong> {tokenData.name}</p>
            <p><strong>Symbol:</strong> {tokenData.symbol}</p>
            <p><strong>Type:</strong> <Badge>{tokenData.token_type}</Badge></p>
            <p><strong>Supply:</strong> {tokenData.total_supply.toLocaleString()}</p>
            <p><strong>Network:</strong> <Badge variant="secondary">{tokenData.blockchain_network}</Badge></p>
          </div>
          <div className="space-y-4 p-4 border rounded-lg">
            <h3 className="font-semibold">Features & Compliance</h3>
            <div>
              <strong>Features:</strong>
              <div className="flex flex-wrap gap-2 mt-1">
                {Object.entries(tokenData.features).filter(([, v]) => v).map(([k]) => <Badge key={k} variant="outline">{k}</Badge>)}
              </div>
            </div>
            <div>
              <strong>Compliance:</strong>
              <div className="flex flex-wrap gap-2 mt-1">
                 {Object.entries(tokenData.compliance).filter(([, v]) => v === true).map(([k]) => <Badge key={k} variant="outline">{k.replace(/_/g, ' ')}</Badge>)}
                 <Badge variant="outline">Jurisdiction: {tokenData.compliance.jurisdiction}</Badge>
              </div>
            </div>
          </div>
        </div>
      </CardContent>
      <CardFooter className="flex justify-between">
        <Button variant="outline" onClick={onPrev} disabled={isDeploying}>
          <ArrowLeft className="w-4 h-4 mr-2" /> Previous
        </Button>
        <Button onClick={handleDeploy} disabled={isDeploying} className="bg-gradient-to-r from-green-500 to-teal-500">
          <Zap className="w-4 h-4 mr-2" /> {isDeploying ? 'Deploying...' : 'Deploy Token'}
        </Button>
      </CardFooter>
    </Card>
  );
}