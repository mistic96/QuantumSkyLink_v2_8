

import React from "react";
import { Link, useLocation } from "react-router-dom";
import { createPageUrl } from "@/utils";
import {
  LayoutDashboard,
  Coins,
  Building2,
  Shield,
  BarChart3,
  Wallet,
  Settings,
  Bell,
  Search,
  User
} from "lucide-react";
import {
  Sidebar,
  SidebarContent,
  SidebarGroup,
  SidebarGroupContent,
  SidebarGroupLabel,
  SidebarMenu,
  SidebarMenuButton,
  SidebarMenuItem,
  SidebarHeader,
  SidebarFooter,
  SidebarProvider,
  SidebarTrigger,
} from "@/components/ui/sidebar";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Avatar, AvatarFallback } from "@/components/ui/avatar";

const navigationItems = [
  {
    title: "Dashboard",
    url: createPageUrl("Dashboard"),
    icon: LayoutDashboard,
    description: "Overview & metrics"
  },
  {
    title: "Token Creator",
    url: createPageUrl("TokenCreator"),
    icon: Coins,
    description: "Mint new tokens"
  },
  {
    title: "Asset Manager",
    url: createPageUrl("AssetManager"),
    icon: Building2,
    description: "Manage assets"
  },
  {
    title: "Compliance",
    url: createPageUrl("Compliance"),
    icon: Shield,
    description: "Regulatory tracking"
  },
  {
    title: "Analytics",
    url: createPageUrl("Analytics"),
    icon: BarChart3,
    description: "Performance insights"
  },
  {
    title: "Portfolio",
    url: createPageUrl("Portfolio"),
    icon: Wallet,
    description: "Your tokens"
  }
];

export default function Layout({ children, currentPageName }) {
  const location = useLocation();

  return (
    <SidebarProvider>
      <style>{`
        :root {
          --quantum-blue: #00D4FF;
          --quantum-purple: #6366F1;
        }
        .quantum-text-gradient {
          background: linear-gradient(135deg, var(--quantum-blue) 0%, var(--quantum-purple) 100%);
          -webkit-background-clip: text;
          -webkit-text-fill-color: transparent;
          background-clip: text;
        }
      `}</style>
      
      <div className="min-h-screen flex w-full">
        <Sidebar className="bg-white border-r border-gray-200 shadow-sm">
          <SidebarHeader className="border-b border-gray-100 p-6">
            <div className="flex items-center gap-3">
              <div className="w-10 h-10 rounded-xl bg-gradient-to-r from-blue-600 to-purple-600 flex items-center justify-center shadow-lg">
                <Coins className="w-6 h-6 text-white" />
              </div>
              <div>
                <h2 className="font-bold text-gray-900 text-lg">QuantumMint</h2>
                <p className="text-sm text-gray-500">Premium Tokenization</p>
              </div>
            </div>
          </SidebarHeader>
          
          <SidebarContent className="p-4">
            <SidebarGroup>
              <SidebarGroupLabel className="text-xs font-semibold text-gray-400 uppercase tracking-wider px-3 py-3 mb-2">
                Platform
              </SidebarGroupLabel>
              <SidebarGroupContent>
                <SidebarMenu className="space-y-1">
                  {navigationItems.map((item) => (
                    <SidebarMenuItem key={item.title}>
                      <SidebarMenuButton 
                        asChild 
                        className={`group hover:bg-gray-50 transition-all duration-200 rounded-lg p-0 ${
                          location.pathname === item.url ? 'bg-blue-50 border border-blue-200' : 'border border-transparent'
                        }`}
                      >
                        <Link to={item.url} className="flex items-start gap-4 px-4 py-3 w-full">
                          <item.icon className={`w-5 h-5 mt-1 ${
                            location.pathname === item.url ? 'text-blue-600' : 'text-gray-500 group-hover:text-gray-700'
                          }`} />
                          <div className="flex-1">
                            <span className={`font-semibold text-base ${
                              location.pathname === item.url ? 'text-blue-900' : 'text-gray-900 group-hover:text-gray-900'
                            }`}>
                              {item.title}
                            </span>
                            <p className={`text-sm mt-0.5 ${
                              location.pathname === item.url ? 'text-blue-600' : 'text-gray-500 group-hover:text-gray-600'
                            }`}>
                              {item.description}
                            </p>
                          </div>
                        </Link>
                      </SidebarMenuButton>
                    </SidebarMenuItem>
                  ))}
                </SidebarMenu>
              </SidebarGroupContent>
            </SidebarGroup>

            <SidebarGroup className="mt-8">
              <SidebarGroupContent>
                <div className="bg-gray-50 border border-gray-200 rounded-xl p-4 mx-3">
                  <div className="flex items-center gap-2 mb-3">
                    <div className="w-2 h-2 bg-green-500 rounded-full animate-pulse"></div>
                    <span className="text-base font-semibold text-gray-900">Network Status</span>
                  </div>
                  <div className="space-y-2 text-sm">
                    <div className="flex justify-between">
                      <span className="text-gray-700 font-medium">Ethereum</span>
                      <span className="text-green-600 font-semibold">Active</span>
                    </div>
                    <div className="flex justify-between">
                      <span className="text-gray-700 font-medium">Polygon</span>
                      <span className="text-green-600 font-semibold">Active</span>
                    </div>
                    <div className="flex justify-between">
                      <span className="text-gray-700 font-medium">Gas Price</span>
                      <span className="text-blue-600 font-semibold">23 gwei</span>
                    </div>
                  </div>
                </div>
              </SidebarGroupContent>
            </SidebarGroup>
          </SidebarContent>

          <SidebarFooter className="border-t border-gray-100 p-4">
            <div className="flex items-center gap-3 p-3 bg-gray-50 border border-gray-200 rounded-xl hover:bg-gray-100 transition-colors duration-200">
              <Avatar className="w-10 h-10">
                <AvatarFallback className="bg-gradient-to-r from-blue-600 to-purple-600 text-white text-sm font-bold">
                  TC
                </AvatarFallback>
              </Avatar>
              <div className="flex-1 min-w-0">
                <p className="font-semibold text-gray-900 text-sm truncate">Token Creator</p>
                <p className="text-xs text-gray-500 truncate">Premium Account</p>
              </div>
              <Button variant="ghost" size="icon" className="text-gray-400 hover:text-gray-600 hover:bg-gray-200">
                <Settings className="w-4 h-4" />
              </Button>
            </div>
          </SidebarFooter>
        </Sidebar>

        <main className="flex-1 flex flex-col bg-gray-50">
          {/* Header */}
          <header className="bg-white border-b border-gray-200 px-6 py-4 flex items-center justify-between shadow-sm">
            <div className="flex items-center gap-4">
              <SidebarTrigger className="lg:hidden hover:bg-gray-100 p-2 rounded-lg transition-colors duration-200" />
              <div className="relative">
                <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400 w-4 h-4" />
                <Input
                  placeholder="Search tokens, assets..."
                  className="pl-10 w-80 bg-gray-50 border-gray-200 focus:bg-white focus:border-blue-300 transition-all duration-200"
                />
              </div>
            </div>

            <div className="flex items-center gap-4">
              <Button variant="outline" size="icon" className="relative hover:bg-gray-50">
                <Bell className="w-4 h-4" />
                <div className="absolute -top-1 -right-1 w-3 h-3 bg-red-500 rounded-full"></div>
              </Button>
              <div className="h-8 w-px bg-gray-200"></div>
              <div className="flex items-center gap-3">
                <div className="text-right">
                  <p className="text-sm font-medium text-gray-900">Portfolio Value</p>
                  <p className="text-lg font-bold quantum-text-gradient">$2,847,392</p>
                </div>
                <Avatar className="w-10 h-10">
                  <AvatarFallback className="bg-gradient-to-r from-blue-600 to-purple-600 text-white font-bold">
                    TC
                  </AvatarFallback>
                </Avatar>
              </div>
            </div>
          </header>

          {/* Main content area */}
          <div className="flex-1 overflow-auto">
            {children}
          </div>
        </main>
      </div>
    </SidebarProvider>
  );
}

