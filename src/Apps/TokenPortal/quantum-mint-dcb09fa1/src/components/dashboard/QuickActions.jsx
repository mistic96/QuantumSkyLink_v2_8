import React from "react";
import { Button } from "@/components/ui/button";
import { Link } from "react-router-dom";
import { createPageUrl } from "@/utils";
import { Plus, BarChart3, Shield } from "lucide-react";

export default function QuickActions() {
  return (
    <div className="flex gap-3">
      <Link to={createPageUrl("TokenCreator")}>
        <Button className="bg-gradient-to-r from-blue-600 to-purple-600 hover:from-blue-700 hover:to-purple-700">
          <Plus className="w-4 h-4 mr-2" />
          Create Token
        </Button>
      </Link>
      <Link to={createPageUrl("Analytics")}>
        <Button variant="outline">
          <BarChart3 className="w-4 h-4 mr-2" />
          Analytics
        </Button>
      </Link>
      <Link to={createPageUrl("Compliance")}>
        <Button variant="outline">
          <Shield className="w-4 h-4 mr-2" />
          Compliance
        </Button>
      </Link>
    </div>
  );
}