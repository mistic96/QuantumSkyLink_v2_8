import React, { useState } from "react";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Switch } from "@/components/ui/switch";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";
import { 
  Lock, Shield, Key, Users, Settings, 
  CheckCircle, XCircle, AlertTriangle
} from "lucide-react";

// Mock data for roles and permissions
const roles = [
  {
    id: "1",
    name: "Super Admin",
    description: "Full system access and control",
    users_count: 2,
    permissions: ["*"],
    created_date: "2024-01-15"
  },
  {
    id: "2", 
    name: "Administrator",
    description: "Administrative access with some restrictions",
    users_count: 5,
    permissions: ["user_management", "financial_operations", "compliance_read", "system_monitoring"],
    created_date: "2024-02-01"
  },
  {
    id: "3",
    name: "Compliance Officer", 
    description: "Access to compliance and regulatory functions",
    users_count: 3,
    permissions: ["compliance_full", "user_read", "reports_compliance"],
    created_date: "2024-02-15"
  },
  {
    id: "4",
    name: "Financial Manager",
    description: "Treasury and financial operations access",
    users_count: 4,
    permissions: ["treasury_full", "analytics_financial", "reports_financial"],
    created_date: "2024-03-01"
  }
];

const permissions = [
  { module: "User Management", permissions: ["user_read", "user_write", "user_delete", "role_management"] },
  { module: "Financial Operations", permissions: ["treasury_read", "treasury_write", "payment_processing", "financial_reporting"] },
  { module: "Compliance", permissions: ["compliance_read", "compliance_write", "audit_access", "regulatory_reporting"] },
  { module: "System Administration", permissions: ["system_monitoring", "service_management", "configuration", "security_settings"] },
  { module: "Analytics & Reports", permissions: ["analytics_read", "report_generation", "data_export", "dashboard_access"] }
];

const sessionData = [
  {
    user: "john@quantumskylink.com",
    role: "Super Admin", 
    location: "New York, US",
    device: "Chrome on Windows",
    last_activity: "2 minutes ago",
    status: "active"
  },
  {
    user: "sarah.johnson@example.com",
    role: "Administrator",
    location: "London, UK", 
    device: "Safari on MacOS",
    last_activity: "15 minutes ago",
    status: "active"
  },
  {
    user: "compliance@quantumskylink.com",
    role: "Compliance Officer",
    location: "Frankfurt, DE",
    device: "Chrome on Linux",
    last_activity: "1 hour ago", 
    status: "idle"
  }
];

export default function AccessControl() {
  const [selectedRole, setSelectedRole] = useState(roles[0]);

  const getStatusColor = (status) => {
    switch (status) {
      case "active": return "bg-emerald-100 text-emerald-800 border-emerald-200";
      case "idle": return "bg-amber-100 text-amber-800 border-amber-200";
      case "offline": return "bg-slate-100 text-slate-800 border-slate-200";
      default: return "bg-slate-100 text-slate-800 border-slate-200";
    }
  };

  return (
    <div className="p-6 space-y-6 bg-slate-50 min-h-screen">
      <div className="flex flex-col md:flex-row justify-between items-start md:items-center gap-4">
        <div>
          <h1 className="text-3xl font-bold text-slate-900">Access Control</h1>
          <p className="text-slate-600 mt-1">Manage roles, permissions, and user access</p>
        </div>
        <Button className="flex items-center gap-2">
          <Shield className="w-4 h-4" />
          Create Role
        </Button>
      </div>

      {/* Access Overview */}
      <div className="grid grid-cols-1 md:grid-cols-4 gap-6">
        <Card>
          <CardContent className="p-6">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm font-medium text-slate-600">Total Roles</p>
                <p className="text-2xl font-bold text-slate-900">{roles.length}</p>
              </div>
              <Shield className="w-8 h-8 text-purple-600" />
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardContent className="p-6">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm font-medium text-slate-600">Active Sessions</p>
                <p className="text-2xl font-bold text-emerald-600">{sessionData.filter(s => s.status === "active").length}</p>
              </div>
              <Users className="w-8 h-8 text-emerald-600" />
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardContent className="p-6">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm font-medium text-slate-600">Permission Modules</p>
                <p className="text-2xl font-bold text-slate-900">{permissions.length}</p>
              </div>
              <Key className="w-8 h-8 text-blue-600" />
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardContent className="p-6">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm font-medium text-slate-600">Security Status</p>
                <p className="text-xl font-bold text-emerald-600">Secure</p>
              </div>
              <Lock className="w-8 h-8 text-emerald-600" />
            </div>
          </CardContent>
        </Card>
      </div>

      <Tabs defaultValue="roles" className="w-full">
        <TabsList className="grid w-full grid-cols-4 bg-white border border-slate-200">
          <TabsTrigger value="roles">Roles & Permissions</TabsTrigger>
          <TabsTrigger value="sessions">Active Sessions</TabsTrigger>
          <TabsTrigger value="permissions">Permission Matrix</TabsTrigger>
          <TabsTrigger value="security">Security Settings</TabsTrigger>
        </TabsList>

        <TabsContent value="roles" className="space-y-6">
          <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
            <Card className="lg:col-span-1">
              <CardHeader>
                <CardTitle>Roles</CardTitle>
              </CardHeader>
              <CardContent>
                <div className="space-y-2">
                  {roles.map((role) => (
                    <div 
                      key={role.id}
                      className={`p-3 rounded-lg border cursor-pointer transition-colors ${
                        selectedRole.id === role.id ? 'border-blue-500 bg-blue-50' : 'border-slate-200 hover:bg-slate-50'
                      }`}
                      onClick={() => setSelectedRole(role)}
                    >
                      <div className="flex items-center justify-between mb-1">
                        <h4 className="font-medium">{role.name}</h4>
                        <Badge variant="outline">{role.users_count} users</Badge>
                      </div>
                      <p className="text-sm text-slate-600">{role.description}</p>
                    </div>
                  ))}
                </div>
              </CardContent>
            </Card>

            <Card className="lg:col-span-2">
              <CardHeader>
                <CardTitle className="flex items-center justify-between">
                  <span>{selectedRole.name} Permissions</span>
                  <Button variant="outline" size="sm">
                    <Settings className="w-4 h-4 mr-2" />
                    Edit Role
                  </Button>
                </CardTitle>
              </CardHeader>
              <CardContent>
                <div className="space-y-4">
                  {permissions.map((module, index) => (
                    <div key={index} className="border rounded-lg p-4">
                      <h4 className="font-semibold mb-3">{module.module}</h4>
                      <div className="grid grid-cols-1 md:grid-cols-2 gap-3">
                        {module.permissions.map((permission) => (
                          <div key={permission} className="flex items-center justify-between">
                            <span className="text-sm">{permission.replace('_', ' ')}</span>
                            <div className="flex items-center gap-2">
                              {selectedRole.permissions.includes(permission) || selectedRole.permissions.includes("*") ? (
                                <CheckCircle className="w-4 h-4 text-emerald-600" />
                              ) : (
                                <XCircle className="w-4 h-4 text-slate-400" />
                              )}
                            </div>
                          </div>
                        ))}
                      </div>
                    </div>
                  ))}
                </div>
              </CardContent>
            </Card>
          </div>
        </TabsContent>

        <TabsContent value="sessions">
          <Card>
            <CardHeader>
              <CardTitle>Active User Sessions</CardTitle>
            </CardHeader>
            <CardContent>
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead>User</TableHead>
                    <TableHead>Role</TableHead>
                    <TableHead>Location</TableHead>
                    <TableHead>Device</TableHead>
                    <TableHead>Last Activity</TableHead>
                    <TableHead>Status</TableHead>
                    <TableHead>Actions</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {sessionData.map((session, index) => (
                    <TableRow key={index}>
                      <TableCell>
                        <span className="font-medium">{session.user}</span>
                      </TableCell>
                      <TableCell>
                        <Badge variant="outline">{session.role}</Badge>
                      </TableCell>
                      <TableCell>{session.location}</TableCell>
                      <TableCell>{session.device}</TableCell>
                      <TableCell>{session.last_activity}</TableCell>
                      <TableCell>
                        <Badge className={getStatusColor(session.status)}>
                          {session.status}
                        </Badge>
                      </TableCell>
                      <TableCell>
                        <Button variant="ghost" size="sm" className="text-red-600 hover:text-red-700">
                          Terminate
                        </Button>
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </CardContent>
          </Card>
        </TabsContent>

        <TabsContent value="permissions">
          <Card>
            <CardHeader>
              <CardTitle>Permission Matrix</CardTitle>
            </CardHeader>
            <CardContent>
              <div className="overflow-x-auto">
                <table className="w-full">
                  <thead>
                    <tr className="border-b">
                      <th className="text-left p-2 font-medium">Module / Permission</th>
                      {roles.map((role) => (
                        <th key={role.id} className="text-center p-2 font-medium min-w-32">
                          {role.name}
                        </th>
                      ))}
                    </tr>
                  </thead>
                  <tbody>
                    {permissions.map((module, moduleIndex) => (
                      <React.Fragment key={moduleIndex}>
                        <tr className="bg-slate-50">
                          <td className="p-2 font-semibold" colSpan={roles.length + 1}>
                            {module.module}
                          </td>
                        </tr>
                        {module.permissions.map((permission) => (
                          <tr key={permission} className="border-b">
                            <td className="p-2 pl-6 text-sm">{permission.replace('_', ' ')}</td>
                            {roles.map((role) => (
                              <td key={role.id} className="text-center p-2">
                                {role.permissions.includes(permission) || role.permissions.includes("*") ? (
                                  <CheckCircle className="w-4 h-4 text-emerald-600 mx-auto" />
                                ) : (
                                  <XCircle className="w-4 h-4 text-slate-400 mx-auto" />
                                )}
                              </td>
                            ))}
                          </tr>
                        ))}
                      </React.Fragment>
                    ))}
                  </tbody>
                </table>
              </div>
            </CardContent>
          </Card>
        </TabsContent>

        <TabsContent value="security">
          <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
            <Card>
              <CardHeader>
                <CardTitle>Security Policies</CardTitle>
              </CardHeader>
              <CardContent className="space-y-6">
                <div className="flex items-center justify-between">
                  <div>
                    <p className="font-medium">Multi-Factor Authentication</p>
                    <p className="text-sm text-slate-600">Require MFA for all admin users</p>
                  </div>
                  <Switch defaultChecked />
                </div>

                <div className="flex items-center justify-between">
                  <div>
                    <p className="font-medium">Session Timeout</p>
                    <p className="text-sm text-slate-600">Auto logout after 30 minutes of inactivity</p>
                  </div>
                  <Switch defaultChecked />
                </div>

                <div className="flex items-center justify-between">
                  <div>
                    <p className="font-medium">Password Complexity</p>
                    <p className="text-sm text-slate-600">Enforce strong password requirements</p>
                  </div>
                  <Switch defaultChecked />
                </div>

                <div className="flex items-center justify-between">
                  <div>
                    <p className="font-medium">Login Monitoring</p>
                    <p className="text-sm text-slate-600">Monitor and alert on suspicious login attempts</p>
                  </div>
                  <Switch defaultChecked />
                </div>
              </CardContent>
            </Card>

            <Card>
              <CardHeader>
                <CardTitle>Security Alerts</CardTitle>
              </CardHeader>
              <CardContent>
                <div className="space-y-4">
                  <div className="p-3 border rounded-lg">
                    <div className="flex items-center gap-2 mb-2">
                      <AlertTriangle className="w-4 h-4 text-amber-600" />
                      <span className="font-medium">Failed Login Attempts</span>
                    </div>
                    <p className="text-sm text-slate-600">3 failed attempts detected from IP 192.168.1.100</p>
                    <p className="text-xs text-slate-400 mt-1">2 hours ago</p>
                  </div>

                  <div className="p-3 border rounded-lg">
                    <div className="flex items-center gap-2 mb-2">
                      <CheckCircle className="w-4 h-4 text-emerald-600" />
                      <span className="font-medium">MFA Setup Complete</span>
                    </div>
                    <p className="text-sm text-slate-600">User sarah.johnson@example.com enabled MFA</p>
                    <p className="text-xs text-slate-400 mt-1">1 day ago</p>
                  </div>

                  <div className="p-3 border rounded-lg">
                    <div className="flex items-center gap-2 mb-2">
                      <Lock className="w-4 h-4 text-blue-600" />
                      <span className="font-medium">Password Changed</span>
                    </div>
                    <p className="text-sm text-slate-600">Admin user updated password successfully</p>
                    <p className="text-xs text-slate-400 mt-1">3 days ago</p>
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