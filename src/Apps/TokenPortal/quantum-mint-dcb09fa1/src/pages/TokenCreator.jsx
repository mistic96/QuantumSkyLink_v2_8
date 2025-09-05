import React, { useState } from "react";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { Progress } from "@/components/ui/progress";
import { motion, AnimatePresence } from "framer-motion";
import { 
  ArrowLeft, 
  ArrowRight, 
  CheckCircle, 
  Coins,
  Sparkles,
  Zap
} from "lucide-react";

import TokenTypeSelector from "@/components/token-creator/TokenTypeSelector";
import BasicConfiguration from "@/components/token-creator/BasicConfiguration";
import AdvancedSettings from "@/components/token-creator/AdvancedSettings";
import ComplianceSetup from "@/components/token-creator/ComplianceSetup";
import ReviewAndDeploy from "@/components/token-creator/ReviewAndDeploy";

const steps = [
  { id: 1, title: "Token Type", description: "Choose your token standard" },
  { id: 2, title: "Basic Info", description: "Name, symbol & supply" },
  { id: 3, title: "Advanced", description: "Features & economics" },
  { id: 4, title: "Compliance", description: "Legal & regulatory" },
  { id: 5, title: "Deploy", description: "Review & launch" }
];

export default function TokenCreator() {
  const [currentStep, setCurrentStep] = useState(1);
  const [tokenData, setTokenData] = useState({
    token_type: '',
    name: '',
    symbol: '',
    description: '',
    total_supply: 1000000,
    decimals: 18,
    supply_type: 'fixed',
    category: 'utility',
    blockchain_network: 'ethereum',
    features: {
      mintable: false,
      burnable: false,
      pausable: false,
      upgradeable: false
    },
    compliance: {
      kyc_required: false,
      accredited_investors_only: false,
      jurisdiction: 'global'
    }
  });

  const updateTokenData = (updates) => {
    setTokenData(prev => ({ ...prev, ...updates }));
  };

  const nextStep = () => {
    if (currentStep < steps.length) {
      setCurrentStep(prev => prev + 1);
    }
  };

  const prevStep = () => {
    if (currentStep > 1) {
      setCurrentStep(prev => prev - 1);
    }
  };

  const progress = (currentStep / steps.length) * 100;

  const renderStepContent = () => {
    switch (currentStep) {
      case 1:
        return (
          <TokenTypeSelector 
            tokenData={tokenData} 
            updateTokenData={updateTokenData}
            onNext={nextStep}
          />
        );
      case 2:
        return (
          <BasicConfiguration 
            tokenData={tokenData} 
            updateTokenData={updateTokenData}
            onNext={nextStep}
            onPrev={prevStep}
          />
        );
      case 3:
        return (
          <AdvancedSettings 
            tokenData={tokenData} 
            updateTokenData={updateTokenData}
            onNext={nextStep}
            onPrev={prevStep}
          />
        );
      case 4:
        return (
          <ComplianceSetup 
            tokenData={tokenData} 
            updateTokenData={updateTokenData}
            onNext={nextStep}
            onPrev={prevStep}
          />
        );
      case 5:
        return (
          <ReviewAndDeploy 
            tokenData={tokenData} 
            onPrev={prevStep}
          />
        );
      default:
        return null;
    }
  };

  return (
    <div className="min-h-screen bg-gradient-to-br from-gray-50 via-white to-blue-50 p-6">
      <div className="max-w-4xl mx-auto">
        {/* Header */}
        <motion.div 
          initial={{ opacity: 0, y: 20 }}
          animate={{ opacity: 1, y: 0 }}
          className="text-center mb-8"
        >
          <div className="flex items-center justify-center gap-3 mb-4">
            <div className="w-12 h-12 bg-gradient-to-r from-blue-600 to-purple-600 rounded-xl flex items-center justify-center">
              <Coins className="w-6 h-6 text-white" />
            </div>
            <h1 className="text-3xl font-bold text-gray-900">
              Token <span className="quantum-text-gradient">Creator</span>
            </h1>
          </div>
          <p className="text-gray-600 max-w-2xl mx-auto">
            Create professional-grade tokens with advanced features and compliance built-in
          </p>
        </motion.div>

        {/* Progress Indicator */}
        <motion.div 
          initial={{ opacity: 0, y: 20 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ delay: 0.1 }}
          className="mb-8"
        >
          <Card className="border-0 shadow-lg quantum-glow">
            <CardContent className="p-6">
              <div className="flex items-center justify-between mb-4">
                <span className="text-sm font-medium text-gray-700">
                  Step {currentStep} of {steps.length}
                </span>
                <Badge className="bg-gradient-to-r from-blue-600 to-purple-600 text-white">
                  {Math.round(progress)}% Complete
                </Badge>
              </div>
              
              <Progress value={progress} className="h-3 mb-4" />
              
              <div className="grid grid-cols-5 gap-4">
                {steps.map((step, index) => (
                  <div key={step.id} className="text-center">
                    <div className={`w-10 h-10 rounded-full flex items-center justify-center mx-auto mb-2 transition-all duration-300 ${
                      step.id <= currentStep 
                        ? 'bg-gradient-to-r from-blue-600 to-purple-600 text-white' 
                        : 'bg-gray-200 text-gray-500'
                    }`}>
                      {step.id < currentStep ? (
                        <CheckCircle className="w-5 h-5" />
                      ) : (
                        <span className="text-sm font-bold">{step.id}</span>
                      )}
                    </div>
                    <div className={`transition-colors duration-300 ${
                      step.id <= currentStep ? 'text-gray-900' : 'text-gray-500'
                    }`}>
                      <p className="font-medium text-sm">{step.title}</p>
                      <p className="text-xs">{step.description}</p>
                    </div>
                  </div>
                ))}
              </div>
            </CardContent>
          </Card>
        </motion.div>

        {/* Step Content */}
        <AnimatePresence mode="wait">
          <motion.div
            key={currentStep}
            initial={{ opacity: 0, x: 20 }}
            animate={{ opacity: 1, x: 0 }}
            exit={{ opacity: 0, x: -20 }}
            transition={{ duration: 0.3 }}
          >
            {renderStepContent()}
          </motion.div>
        </AnimatePresence>
      </div>
    </div>
  );
}