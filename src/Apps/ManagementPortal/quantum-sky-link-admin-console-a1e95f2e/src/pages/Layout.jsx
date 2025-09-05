

import React, { useState } from "react";
import { Link, useLocation } from "react-router-dom";
import { createPageUrl } from "@/utils";
import { 
  LayoutDashboard, Users, Shield, TrendingUp, Settings, 
  Server, AlertTriangle, FileText, CreditCard, Database,
  Network, Lock, Zap, ChevronDown, Menu, X, Bell, Wallet
} from "lucide-react";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
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

const navigationSections = [
  {
    title: "Overview",
    items: [
      { title: "Dashboard", url: createPageUrl("Dashboard"), icon: LayoutDashboard },
      { title: "System Health", url: createPageUrl("SystemHealth"), icon: Zap },
      { title: "Alerts Center", url: createPageUrl("AlertsCenter"), icon: AlertTriangle },
    ]
  },
  {
    title: "Asset Management", 
    items: [
      { title: "Asset Overview", url: createPageUrl("AssetManagement"), icon: Wallet },
      { title: "Treasury", url: createPageUrl("Treasury"), icon: CreditCard },
      { title: "Analytics", url: createPageUrl("Analytics"), icon: TrendingUp },
    ]
  },
  {
    title: "User Management",
    items: [
      { title: "Users", url: createPageUrl("Users"), icon: Users },
      { title: "Compliance", url: createPageUrl("Compliance"), icon: Shield },
      { title: "Access Control", url: createPageUrl("AccessControl"), icon: Lock },
    ]
  },
  {
    title: "Financial Operations",
    items: [
      { title: "Reports", url: createPageUrl("Reports"), icon: FileText },
    ]
  },
  {
    title: "Infrastructure",
    items: [
      { title: "Services", url: createPageUrl("Services"), icon: Server },
      { title: "Database", url: createPageUrl("Database"), icon: Database },
      { title: "Network", url: createPageUrl("Network"), icon: Network },
      { title: "Settings", url: createPageUrl("Settings"), icon: Settings },
    ]
  }
];

export default function Layout({ children, currentPageName }) {
  const location = useLocation();
  const [collapsedSections, setCollapsedSections] = useState(new Set());

  const toggleSection = (sectionTitle) => {
    const newCollapsed = new Set(collapsedSections);
    if (newCollapsed.has(sectionTitle)) {
      newCollapsed.delete(sectionTitle);
    } else {
      newCollapsed.add(sectionTitle);
    }
    setCollapsedSections(newCollapsed);
  };

  return (
    <SidebarProvider>
      <div className="min-h-screen flex w-full bg-slate-50">
        <Sidebar className="border-r border-slate-200 bg-white shadow-xl">
          <SidebarHeader className="border-b border-slate-100 p-6">
            <div className="flex items-center gap-3">
              <div className="w-10 h-10 bg-gradient-to-br from-blue-600 to-blue-700 rounded-xl flex items-center justify-center shadow-lg">
                <Zap className="w-6 h-6 text-white" />
              </div>
              <div>
                <h2 className="font-bold text-slate-900 text-lg">QuantumSkyLink</h2>
                <p className="text-xs text-slate-500 font-medium">Administration Portal</p>
              </div>
            </div>
          </SidebarHeader>
          
          <SidebarContent className="p-4">
            {navigationSections.map((section) => (
              <SidebarGroup key={section.title} className="mb-6">
                <SidebarGroupLabel 
                  className="text-xs font-semibold text-slate-500 uppercase tracking-wider px-3 py-2 cursor-pointer hover:text-slate-700 transition-colors flex items-center justify-between"
                  onClick={() => toggleSection(section.title)}
                >
                  <span>{section.title}</span>
                  <ChevronDown 
                    className={`w-4 h-4 transition-transform duration-200 ${
                      collapsedSections.has(section.title) ? 'rotate-180' : ''
                    }`} 
                  />
                </SidebarGroupLabel>
                {!collapsedSections.has(section.title) && (
                  <SidebarGroupContent>
                    <SidebarMenu>
                      {section.items.map((item) => (
                        <SidebarMenuItem key={item.title}>
                          <SidebarMenuButton 
                            asChild 
                            className={`hover:bg-blue-50 hover:text-blue-700 transition-all duration-200 rounded-lg mb-1 relative ${
                              location.pathname === item.url 
                                ? 'bg-blue-50 text-blue-700 shadow-sm border-l-4 border-blue-600' 
                                : 'text-slate-600 hover:text-slate-900'
                            }`}
                          >
                            <Link to={item.url} className="flex items-center gap-3 px-3 py-2.5">
                              <item.icon className="w-5 h-5" />
                              <span className="font-medium">{item.title}</span>
                              {item.title === "Alerts Center" && (
                                <Badge variant="destructive" className="ml-auto text-xs px-1.5 py-0.5">
                                  3
                                </Badge>
                              )}
                            </Link>
                          </SidebarMenuButton>
                        </SidebarMenuItem>
                      ))}
                    </SidebarMenu>
                  </SidebarGroupContent>
                )}
              </SidebarGroup>
            ))}
          </SidebarContent>

          <SidebarFooter className="border-t border-slate-100 p-4">
            <div className="flex items-center gap-3 p-3 rounded-lg bg-slate-50">
              <div className="w-9 h-9 bg-gradient-to-br from-slate-600 to-slate-700 rounded-lg flex items-center justify-center">
                <span className="text-white font-semibold text-sm">A</span>
              </div>
              <div className="flex-1 min-w-0">
                <p className="font-semibold text-slate-900 text-sm truncate">Administrator</p>
                <p className="text-xs text-slate-500 truncate">Super Admin Access</p>
              </div>
            </div>
          </SidebarFooter>
        </Sidebar>

        <main className="flex-1 flex flex-col bg-slate-50">
          <header className="bg-white border-b border-slate-200 px-6 py-4 md:hidden shadow-sm">
            <div className="flex items-center justify-between">
              <SidebarTrigger className="hover:bg-slate-100 p-2 rounded-lg transition-colors duration-200" />
              <div className="flex items-center gap-3">
                <Button variant="ghost" size="icon" className="relative">
                  <Bell className="w-5 h-5" />
                  <span className="absolute -top-1 -right-1 w-2 h-2 bg-red-500 rounded-full"></span>
                </Button>
              </div>
            </div>
          </header>

          <div className="flex-1 overflow-auto">
            {children}
          </div>
        </main>
      </div>
    </SidebarProvider>
  );
}

