import React, { useState } from "react";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { 
  FileText, Download, Calendar, Filter, Search, 
  BarChart3, PieChart, TrendingUp, Eye, Plus
} from "lucide-react";
import { format } from "date-fns";

// Mock reports data
const mockReports = [
  {
    id: "1",
    title: "Monthly Financial Summary",
    type: "financial",
    category: "treasury",
    period: "December 2024",
    status: "completed",
    generated_date: new Date(Date.now() - 2 * 24 * 60 * 60 * 1000),
    file_size: "2.4 MB",
    generated_by: "System Automation"
  },
  {
    id: "2", 
    title: "User Activity Report",
    type: "analytics",
    category: "users",
    period: "Q4 2024",
    status: "completed",
    generated_date: new Date(Date.now() - 5 * 24 * 60 * 60 * 1000),
    file_size: "1.8 MB",
    generated_by: "admin@quantumskylink.com"
  },
  {
    id: "3",
    title: "Compliance Audit Report",
    type: "compliance",
    category: "regulatory",
    period: "Annual 2024",
    status: "in_progress",
    generated_date: new Date(Date.now() - 1 * 24 * 60 * 60 * 1000),
    file_size: "Generating...",
    generated_by: "compliance@quantumskylink.com"
  },
  {
    id: "4",
    title: "Service Performance Report",
    type: "system",
    category: "infrastructure",
    period: "December 2024",
    status: "completed",
    generated_date: new Date(Date.now() - 7 * 24 * 60 * 60 * 1000),
    file_size: "3.2 MB",
    generated_by: "devops@quantumskylink.com"
  }
];

const reportTemplates = [
  {
    name: "Financial Summary",
    description: "Treasury balances, transaction volumes, and revenue metrics",
    type: "financial",
    frequency: "Monthly"
  },
  {
    name: "User Analytics",
    description: "User growth, engagement, and behavioral insights", 
    type: "analytics",
    frequency: "Weekly"
  },
  {
    name: "Compliance Report",
    description: "AML/KYC compliance status and regulatory metrics",
    type: "compliance", 
    frequency: "Quarterly"
  },
  {
    name: "System Health",
    description: "Infrastructure performance and service availability",
    type: "system",
    frequency: "Daily"
  }
];

export default function Reports() {
  const [reports, setReports] = useState(mockReports);
  const [searchTerm, setSearchTerm] = useState("");
  const [typeFilter, setTypeFilter] = useState("all");
  const [statusFilter, setStatusFilter] = useState("all");

  const getStatusColor = (status) => {
    switch (status) {
      case "completed": return "bg-emerald-100 text-emerald-800 border-emerald-200";
      case "in_progress": return "bg-blue-100 text-blue-800 border-blue-200";
      case "failed": return "bg-red-100 text-red-800 border-red-200";
      case "scheduled": return "bg-amber-100 text-amber-800 border-amber-200";
      default: return "bg-slate-100 text-slate-800 border-slate-200";
    }
  };

  const getTypeColor = (type) => {
    switch (type) {
      case "financial": return "bg-emerald-100 text-emerald-800 border-emerald-200";
      case "analytics": return "bg-blue-100 text-blue-800 border-blue-200";
      case "compliance": return "bg-purple-100 text-purple-800 border-purple-200";
      case "system": return "bg-orange-100 text-orange-800 border-orange-200";
      default: return "bg-slate-100 text-slate-800 border-slate-200";
    }
  };

  const filteredReports = reports.filter(report => {
    const matchesSearch = report.title.toLowerCase().includes(searchTerm.toLowerCase()) ||
                         report.category.toLowerCase().includes(searchTerm.toLowerCase());
    const matchesType = typeFilter === "all" || report.type === typeFilter;
    const matchesStatus = statusFilter === "all" || report.status === statusFilter;
    
    return matchesSearch && matchesType && matchesStatus;
  });

  const reportCounts = {
    total: reports.length,
    completed: reports.filter(r => r.status === "completed").length,
    in_progress: reports.filter(r => r.status === "in_progress").length,
    scheduled: reports.filter(r => r.status === "scheduled").length
  };

  return (
    <div className="p-6 space-y-6 bg-slate-50 min-h-screen">
      <div className="flex flex-col md:flex-row justify-between items-start md:items-center gap-4">
        <div>
          <h1 className="text-3xl font-bold text-slate-900">Reports Center</h1>
          <p className="text-slate-600 mt-1">Generate and manage business reports and analytics</p>
        </div>
        <Button className="flex items-center gap-2">
          <Plus className="w-4 h-4" />
          Generate Report
        </Button>
      </div>

      {/* Report Statistics */}
      <div className="grid grid-cols-1 md:grid-cols-4 gap-6">
        <Card>
          <CardContent className="p-6">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm font-medium text-slate-600">Total Reports</p>
                <p className="text-2xl font-bold text-slate-900">{reportCounts.total}</p>
              </div>
              <FileText className="w-8 h-8 text-slate-600" />
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardContent className="p-6">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm font-medium text-slate-600">Completed</p>
                <p className="text-2xl font-bold text-emerald-600">{reportCounts.completed}</p>
              </div>
              <BarChart3 className="w-8 h-8 text-emerald-600" />
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardContent className="p-6">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm font-medium text-slate-600">In Progress</p>
                <p className="text-2xl font-bold text-blue-600">{reportCounts.in_progress}</p>
              </div>
              <TrendingUp className="w-8 h-8 text-blue-600" />
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardContent className="p-6">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm font-medium text-slate-600">Scheduled</p>
                <p className="text-2xl font-bold text-amber-600">{reportCounts.scheduled}</p>
              </div>
              <Calendar className="w-8 h-8 text-amber-600" />
            </div>
          </CardContent>
        </Card>
      </div>

      <Tabs defaultValue="reports" className="w-full">
        <TabsList className="grid w-full grid-cols-3 bg-white border border-slate-200">
          <TabsTrigger value="reports">All Reports</TabsTrigger>
          <TabsTrigger value="templates">Templates</TabsTrigger>
          <TabsTrigger value="scheduled">Scheduled</TabsTrigger>
        </TabsList>

        <TabsContent value="reports" className="space-y-6">
          {/* Filters */}
          <Card>
            <CardContent className="p-6">
              <div className="flex flex-col md:flex-row gap-4">
                <div className="flex-1">
                  <div className="relative">
                    <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 text-slate-400 w-4 h-4" />
                    <Input
                      placeholder="Search reports..."
                      value={searchTerm}
                      onChange={(e) => setSearchTerm(e.target.value)}
                      className="pl-10"
                    />
                  </div>
                </div>
                <Select value={typeFilter} onValueChange={setTypeFilter}>
                  <SelectTrigger className="w-48">
                    <SelectValue placeholder="Filter by type" />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="all">All Types</SelectItem>
                    <SelectItem value="financial">Financial</SelectItem>
                    <SelectItem value="analytics">Analytics</SelectItem>
                    <SelectItem value="compliance">Compliance</SelectItem>
                    <SelectItem value="system">System</SelectItem>
                  </SelectContent>
                </Select>
                <Select value={statusFilter} onValueChange={setStatusFilter}>
                  <SelectTrigger className="w-48">
                    <SelectValue placeholder="Filter by status" />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="all">All Status</SelectItem>
                    <SelectItem value="completed">Completed</SelectItem>
                    <SelectItem value="in_progress">In Progress</SelectItem>
                    <SelectItem value="failed">Failed</SelectItem>
                    <SelectItem value="scheduled">Scheduled</SelectItem>
                  </SelectContent>
                </Select>
              </div>
            </CardContent>
          </Card>

          {/* Reports List */}
          <Card>
            <CardHeader>
              <CardTitle>Reports ({filteredReports.length})</CardTitle>
            </CardHeader>
            <CardContent>
              <div className="space-y-4">
                {filteredReports.map((report) => (
                  <Card key={report.id} className="border border-slate-200">
                    <CardContent className="p-4">
                      <div className="flex items-center justify-between">
                        <div className="flex items-center gap-4">
                          <div className="w-12 h-12 bg-blue-50 rounded-lg flex items-center justify-center">
                            <FileText className="w-6 h-6 text-blue-600" />
                          </div>
                          <div>
                            <h4 className="font-semibold text-slate-900 mb-1">{report.title}</h4>
                            <div className="flex items-center gap-2 mb-2">
                              <Badge className={getTypeColor(report.type)}>
                                {report.type}
                              </Badge>
                              <Badge className={getStatusColor(report.status)}>
                                {report.status.replace('_', ' ')}
                              </Badge>
                            </div>
                            <div className="flex items-center gap-4 text-sm text-slate-500">
                              <span>Period: {report.period}</span>
                              <span>Generated: {format(report.generated_date, 'MMM d, yyyy')}</span>
                              <span>Size: {report.file_size}</span>
                              <span>By: {report.generated_by}</span>
                            </div>
                          </div>
                        </div>
                        <div className="flex items-center gap-2">
                          <Button variant="ghost" size="sm">
                            <Eye className="w-4 h-4" />
                          </Button>
                          {report.status === "completed" && (
                            <Button variant="ghost" size="sm">
                              <Download className="w-4 h-4" />
                            </Button>
                          )}
                        </div>
                      </div>
                    </CardContent>
                  </Card>
                ))}
              </div>
            </CardContent>
          </Card>
        </TabsContent>

        <TabsContent value="templates">
          <Card>
            <CardHeader>
              <CardTitle>Report Templates</CardTitle>
            </CardHeader>
            <CardContent>
              <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                {reportTemplates.map((template, index) => (
                  <Card key={index} className="border border-slate-200 hover:shadow-md transition-shadow">
                    <CardContent className="p-6">
                      <div className="flex items-start justify-between mb-4">
                        <div className="flex items-center gap-3">
                          <div className="w-10 h-10 bg-blue-50 rounded-lg flex items-center justify-center">
                            {template.type === "financial" && <BarChart3 className="w-5 h-5 text-emerald-600" />}
                            {template.type === "analytics" && <TrendingUp className="w-5 h-5 text-blue-600" />}
                            {template.type === "compliance" && <FileText className="w-5 h-5 text-purple-600" />}
                            {template.type === "system" && <PieChart className="w-5 h-5 text-orange-600" />}
                          </div>
                          <div>
                            <h4 className="font-semibold text-slate-900">{template.name}</h4>
                            <Badge className={getTypeColor(template.type)} size="sm">
                              {template.type}
                            </Badge>
                          </div>
                        </div>
                      </div>
                      <p className="text-sm text-slate-600 mb-4">{template.description}</p>
                      <div className="flex items-center justify-between">
                        <span className="text-sm text-slate-500">Frequency: {template.frequency}</span>
                        <Button size="sm">Generate</Button>
                      </div>
                    </CardContent>
                  </Card>
                ))}
              </div>
            </CardContent>
          </Card>
        </TabsContent>

        <TabsContent value="scheduled">
          <Card>
            <CardHeader>
              <CardTitle>Scheduled Reports</CardTitle>
            </CardHeader>
            <CardContent>
              <div className="text-center py-8">
                <Calendar className="w-12 h-12 text-slate-400 mx-auto mb-4" />
                <p className="text-slate-500 mb-4">No scheduled reports configured</p>
                <Button variant="outline">Schedule Report</Button>
              </div>
            </CardContent>
          </Card>
        </TabsContent>
      </Tabs>
    </div>
  );
}