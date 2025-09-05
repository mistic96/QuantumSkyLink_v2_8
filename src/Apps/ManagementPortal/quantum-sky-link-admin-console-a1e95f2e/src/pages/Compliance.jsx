import React, { useState, useEffect } from "react";
import { ComplianceReport } from "@/api/entities";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Progress } from "@/components/ui/progress";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";
import { 
  Shield, AlertTriangle, CheckCircle, FileText, 
  TrendingUp, Users, Eye, Download, Plus
} from "lucide-react";
import { format } from "date-fns";

export default function Compliance() {
  const [reports, setReports] = useState([]);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    loadReports();
  }, []);

  const loadReports = async () => {
    setIsLoading(true);
    try {
      const data = await ComplianceReport.list("-created_date", 50);
      setReports(data);
    } catch (error) {
      console.error("Error loading reports:", error);
    }
    setIsLoading(false);
  };

  const getStatusColor = (status) => {
    switch (status) {
      case "approved": return "bg-emerald-100 text-emerald-800 border-emerald-200";
      case "submitted": return "bg-blue-100 text-blue-800 border-blue-200";
      case "pending_review": return "bg-amber-100 text-amber-800 border-amber-200";
      case "draft": return "bg-slate-100 text-slate-800 border-slate-200";
      default: return "bg-slate-100 text-slate-800 border-slate-200";
    }
  };

  const getReportTypeColor = (type) => {
    switch (type) {
      case "aml": return "bg-red-100 text-red-800 border-red-200";
      case "kyc": return "bg-blue-100 text-blue-800 border-blue-200";
      case "sanctions": return "bg-purple-100 text-purple-800 border-purple-200";
      case "audit": return "bg-orange-100 text-orange-800 border-orange-200";
      case "risk_assessment": return "bg-yellow-100 text-yellow-800 border-yellow-200";
      default: return "bg-slate-100 text-slate-800 border-slate-200";
    }
  };

  // Mock compliance metrics
  const complianceMetrics = {
    overallScore: 96.8,
    amlScore: 98.2,
    kycScore: 95.4,
    sanctionsScore: 97.1,
    activeViolations: 3,
    pendingReviews: 12,
    monthlyTrend: 2.3
  };

  const reportCounts = {
    total: reports.length,
    approved: reports.filter(r => r.status === "approved").length,
    pending: reports.filter(r => r.status === "pending_review").length,
    draft: reports.filter(r => r.status === "draft").length
  };

  return (
    <div className="p-6 space-y-6 bg-slate-50 min-h-screen">
      <div className="flex flex-col md:flex-row justify-between items-start md:items-center gap-4">
        <div>
          <h1 className="text-3xl font-bold text-slate-900">Compliance Center</h1>
          <p className="text-slate-600 mt-1">Monitor regulatory compliance and manage reports</p>
        </div>
        <Button className="flex items-center gap-2">
          <Plus className="w-4 h-4" />
          Generate Report
        </Button>
      </div>

      {/* Compliance Overview */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
        <Card>
          <CardContent className="p-6">
            <div className="flex items-center justify-between mb-4">
              <div>
                <p className="text-sm font-medium text-slate-600">Overall Compliance</p>
                <p className="text-3xl font-bold text-emerald-600">{complianceMetrics.overallScore}%</p>
              </div>
              <Shield className="w-8 h-8 text-emerald-600" />
            </div>
            <div className="flex items-center gap-2">
              <TrendingUp className="w-4 h-4 text-emerald-600" />
              <span className="text-sm text-emerald-600 font-medium">+{complianceMetrics.monthlyTrend}%</span>
              <span className="text-sm text-slate-500">this month</span>
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardContent className="p-6">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm font-medium text-slate-600">Active Violations</p>
                <p className="text-2xl font-bold text-red-600">{complianceMetrics.activeViolations}</p>
              </div>
              <AlertTriangle className="w-8 h-8 text-red-600" />
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardContent className="p-6">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm font-medium text-slate-600">Pending Reviews</p>
                <p className="text-2xl font-bold text-amber-600">{complianceMetrics.pendingReviews}</p>
              </div>
              <Users className="w-8 h-8 text-amber-600" />
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardContent className="p-6">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm font-medium text-slate-600">Reports Generated</p>
                <p className="text-2xl font-bold text-slate-900">{reportCounts.total}</p>
              </div>
              <FileText className="w-8 h-8 text-slate-600" />
            </div>
          </CardContent>
        </Card>
      </div>

      <Tabs defaultValue="dashboard" className="w-full">
        <TabsList className="grid w-full grid-cols-3 bg-white border border-slate-200">
          <TabsTrigger value="dashboard">Dashboard</TabsTrigger>
          <TabsTrigger value="reports">Reports</TabsTrigger>
          <TabsTrigger value="monitoring">Monitoring</TabsTrigger>
        </TabsList>

        <TabsContent value="dashboard" className="space-y-6">
          {/* Compliance Scores */}
          <Card>
            <CardHeader>
              <CardTitle>Compliance Scores</CardTitle>
            </CardHeader>
            <CardContent>
              <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
                <div className="space-y-3">
                  <div className="flex justify-between items-center">
                    <span className="text-sm font-medium">AML Compliance</span>
                    <span className="text-sm font-bold text-emerald-600">{complianceMetrics.amlScore}%</span>
                  </div>
                  <Progress value={complianceMetrics.amlScore} className="h-3" />
                </div>

                <div className="space-y-3">
                  <div className="flex justify-between items-center">
                    <span className="text-sm font-medium">KYC Compliance</span>
                    <span className="text-sm font-bold text-blue-600">{complianceMetrics.kycScore}%</span>
                  </div>
                  <Progress value={complianceMetrics.kycScore} className="h-3" />
                </div>

                <div className="space-y-3">
                  <div className="flex justify-between items-center">
                    <span className="text-sm font-medium">Sanctions Screening</span>
                    <span className="text-sm font-bold text-purple-600">{complianceMetrics.sanctionsScore}%</span>
                  </div>
                  <Progress value={complianceMetrics.sanctionsScore} className="h-3" />
                </div>
              </div>
            </CardContent>
          </Card>
        </TabsContent>

        <TabsContent value="reports">
          <Card>
            <CardHeader>
              <CardTitle>Compliance Reports ({reportCounts.total})</CardTitle>
            </CardHeader>
            <CardContent>
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead>Report</TableHead>
                    <TableHead>Type</TableHead>
                    <TableHead>Period</TableHead>
                    <TableHead>Score</TableHead>
                    <TableHead>Status</TableHead>
                    <TableHead>Actions</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {reports.map((report) => (
                    <TableRow key={report.id}>
                      <TableCell>
                        <div>
                          <p className="font-medium">{report.title}</p>
                          <p className="text-sm text-slate-500">
                            Generated by {report.generated_by}
                          </p>
                        </div>
                      </TableCell>
                      <TableCell>
                        <Badge className={getReportTypeColor(report.report_type)}>
                          {report.report_type.replace('_', ' ').toUpperCase()}
                        </Badge>
                      </TableCell>
                      <TableCell>
                        <span className="text-sm">
                          {format(new Date(report.period_start), 'MMM d')} - {format(new Date(report.period_end), 'MMM d, yyyy')}
                        </span>
                      </TableCell>
                      <TableCell>
                        <span className={`font-semibold ${
                          report.compliance_score >= 95 ? 'text-emerald-600' :
                          report.compliance_score >= 85 ? 'text-amber-600' : 'text-red-600'
                        }`}>
                          {report.compliance_score}%
                        </span>
                      </TableCell>
                      <TableCell>
                        <Badge className={getStatusColor(report.status)}>
                          {report.status.replace('_', ' ')}
                        </Badge>
                      </TableCell>
                      <TableCell>
                        <div className="flex items-center gap-2">
                          <Button variant="ghost" size="sm">
                            <Eye className="w-4 h-4" />
                          </Button>
                          {report.file_url && (
                            <Button variant="ghost" size="sm">
                              <Download className="w-4 h-4" />
                            </Button>
                          )}
                        </div>
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </CardContent>
          </Card>
        </TabsContent>

        <TabsContent value="monitoring">
          <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
            <Card>
              <CardHeader>
                <CardTitle>Risk Monitoring</CardTitle>
              </CardHeader>
              <CardContent>
                <div className="space-y-4">
                  <div className="p-4 border rounded-lg">
                    <div className="flex items-center justify-between mb-2">
                      <span className="font-medium">High Risk Transactions</span>
                      <Badge className="bg-red-100 text-red-800">2 Active</Badge>
                    </div>
                    <p className="text-sm text-slate-600">Transactions flagged for manual review</p>
                  </div>

                  <div className="p-4 border rounded-lg">
                    <div className="flex items-center justify-between mb-2">
                      <span className="font-medium">Sanctions Alerts</span>
                      <Badge className="bg-amber-100 text-amber-800">1 Pending</Badge>
                    </div>
                    <p className="text-sm text-slate-600">Potential sanctions list matches</p>
                  </div>

                  <div className="p-4 border rounded-lg">
                    <div className="flex items-center justify-between mb-2">
                      <span className="font-medium">KYC Verifications</span>
                      <Badge className="bg-blue-100 text-blue-800">5 In Progress</Badge>
                    </div>
                    <p className="text-sm text-slate-600">Identity verifications pending</p>
                  </div>
                </div>
              </CardContent>
            </Card>

            <Card>
              <CardHeader>
                <CardTitle>Regulatory Updates</CardTitle>
              </CardHeader>
              <CardContent>
                <div className="space-y-4">
                  <div className="p-4 border rounded-lg">
                    <div className="flex items-center gap-2 mb-2">
                      <CheckCircle className="w-4 h-4 text-emerald-600" />
                      <span className="font-medium">AML Policy Updated</span>
                    </div>
                    <p className="text-sm text-slate-600">New anti-money laundering guidelines implemented</p>
                    <p className="text-xs text-slate-400 mt-1">2 hours ago</p>
                  </div>

                  <div className="p-4 border rounded-lg">
                    <div className="flex items-center gap-2 mb-2">
                      <AlertTriangle className="w-4 h-4 text-amber-600" />
                      <span className="font-medium">Sanctions List Update</span>
                    </div>
                    <p className="text-sm text-slate-600">New entities added to sanctions screening</p>
                    <p className="text-xs text-slate-400 mt-1">1 day ago</p>
                  </div>

                  <div className="p-4 border rounded-lg">
                    <div className="flex items-center gap-2 mb-2">
                      <FileText className="w-4 h-4 text-blue-600" />
                      <span className="font-medium">Regulatory Filing Due</span>
                    </div>
                    <p className="text-sm text-slate-600">Quarterly compliance report due next week</p>
                    <p className="text-xs text-slate-400 mt-1">5 days remaining</p>
                  </div>
                </div>
              </CardContent>
            </Card>
          </div>
        </TabsContent>
      </Tabs>
    </div>
  );
}