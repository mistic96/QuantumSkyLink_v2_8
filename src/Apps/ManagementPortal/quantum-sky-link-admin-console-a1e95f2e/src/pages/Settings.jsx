
import React, { useState } from "react";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Switch } from "@/components/ui/switch";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { Textarea } from "@/components/ui/textarea";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { 
  Settings as SettingsIcon, Bell, Shield, Globe, 
  Database, Mail, Zap, Save, 
  AlertTriangle, CheckCircle, Clock
} from "lucide-react";

export default function Settings() {
  const [notifications, setNotifications] = useState({
    system_alerts: true,
    security_events: true,
    performance_warnings: false,
    maintenance_updates: true,
    user_activity: false
  });

  const [systemConfig, setSystemConfig] = useState({
    max_login_attempts: 5,
    session_timeout: 30,
    backup_retention: 90,
    log_level: "INFO",
    maintenance_mode: false
  });

  return (
    <div className="p-6 space-y-6 bg-slate-50 min-h-screen">
      <div className="flex flex-col md:flex-row justify-between items-start md:items-center gap-4">
        <div>
          <h1 className="text-3xl font-bold text-slate-900">System Settings</h1>
          <p className="text-slate-600 mt-1">Configure system-wide settings and preferences</p>
        </div>
        <Button className="flex items-center gap-2">
          <Save className="w-4 h-4" />
          Save Changes
        </Button>
      </div>

      <Tabs defaultValue="general" className="w-full">
        <TabsList className="grid w-full grid-cols-5 bg-white border border-slate-200">
          <TabsTrigger value="general">General</TabsTrigger>
          <TabsTrigger value="security">Security</TabsTrigger>
          <TabsTrigger value="notifications">Notifications</TabsTrigger>
          <TabsTrigger value="integrations">Integrations</TabsTrigger>
          <TabsTrigger value="maintenance">Maintenance</TabsTrigger>
        </TabsList>

        <TabsContent value="general" className="space-y-6">
          <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
            <Card>
              <CardHeader>
                <CardTitle className="flex items-center gap-2">
                  <SettingsIcon className="w-5 h-5" />
                  System Configuration
                </CardTitle>
              </CardHeader>
              <CardContent className="space-y-6">
                <div className="space-y-2">
                  <label className="text-sm font-medium">System Name</label>
                  <Input defaultValue="QuantumSkyLink v2 Administration" />
                </div>

                <div className="space-y-2">
                  <label className="text-sm font-medium">Environment</label>
                  <Select defaultValue="production">
                    <SelectTrigger>
                      <SelectValue />
                    </SelectTrigger>
                    <SelectContent>
                      <SelectItem value="development">Development</SelectItem>
                      <SelectItem value="staging">Staging</SelectItem>
                      <SelectItem value="production">Production</SelectItem>
                    </SelectContent>
                  </Select>
                </div>

                <div className="space-y-2">
                  <label className="text-sm font-medium">Timezone</label>
                  <Select defaultValue="UTC">
                    <SelectTrigger>
                      <SelectValue />
                    </SelectTrigger>
                    <SelectContent>
                      <SelectItem value="UTC">UTC</SelectItem>
                      <SelectItem value="America/New_York">Eastern Time</SelectItem>
                      <SelectItem value="America/Los_Angeles">Pacific Time</SelectItem>
                      <SelectItem value="Europe/London">GMT</SelectItem>
                    </SelectContent>
                  </Select>
                </div>

                <div className="space-y-2">
                  <label className="text-sm font-medium">Log Level</label>
                  <Select value={systemConfig.log_level} onValueChange={(value) => 
                    setSystemConfig({...systemConfig, log_level: value})
                  }>
                    <SelectTrigger>
                      <SelectValue />
                    </SelectTrigger>
                    <SelectContent>
                      <SelectItem value="DEBUG">Debug</SelectItem>
                      <SelectItem value="INFO">Info</SelectItem>
                      <SelectItem value="WARN">Warning</SelectItem>
                      <SelectItem value="ERROR">Error</SelectItem>
                    </SelectContent>
                  </Select>
                </div>
              </CardContent>
            </Card>

            <Card>
              <CardHeader>
                <CardTitle className="flex items-center gap-2">
                  <Globe className="w-5 h-5" />
                  Regional Settings
                </CardTitle>
              </CardHeader>
              <CardContent className="space-y-6">
                <div className="space-y-2">
                  <label className="text-sm font-medium">Default Language</label>
                  <Select defaultValue="en">
                    <SelectTrigger>
                      <SelectValue />
                    </SelectTrigger>
                    <SelectContent>
                      <SelectItem value="en">English</SelectItem>
                      <SelectItem value="es">Spanish</SelectItem>
                      <SelectItem value="fr">French</SelectItem>
                      <SelectItem value="de">German</SelectItem>
                    </SelectContent>
                  </Select>
                </div>

                <div className="space-y-2">
                  <label className="text-sm font-medium">Default Currency</label>
                  <Select defaultValue="USD">
                    <SelectTrigger>
                      <SelectValue />
                    </SelectTrigger>
                    <SelectContent>
                      <SelectItem value="USD">USD - US Dollar</SelectItem>
                      <SelectItem value="EUR">EUR - Euro</SelectItem>
                      <SelectItem value="GBP">GBP - British Pound</SelectItem>
                      <SelectItem value="JPY">JPY - Japanese Yen</SelectItem>
                    </SelectContent>
                  </Select>
                </div>

                <div className="space-y-2">
                  <label className="text-sm font-medium">Date Format</label>
                  <Select defaultValue="MM/DD/YYYY">
                    <SelectTrigger>
                      <SelectValue />
                    </SelectTrigger>
                    <SelectContent>
                      <SelectItem value="MM/DD/YYYY">MM/DD/YYYY</SelectItem>
                      <SelectItem value="DD/MM/YYYY">DD/MM/YYYY</SelectItem>
                      <SelectItem value="YYYY-MM-DD">YYYY-MM-DD</SelectItem>
                    </SelectContent>
                  </Select>
                </div>

                <div className="space-y-2">
                  <label className="text-sm font-medium">Number Format</label>
                  <Select defaultValue="1,234.56">
                    <SelectTrigger>
                      <SelectValue />
                    </SelectTrigger>
                    <SelectContent>
                      <SelectItem value="1,234.56">1,234.56 (US)</SelectItem>
                      <SelectItem value="1.234,56">1.234,56 (EU)</SelectItem>
                      <SelectItem value="1 234,56">1 234,56 (FR)</SelectItem>
                    </SelectContent>
                  </Select>
                </div>
              </CardContent>
            </Card>
          </div>
        </TabsContent>

        <TabsContent value="security" className="space-y-6">
          <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
            <Card>
              <CardHeader>
                <CardTitle className="flex items-center gap-2">
                  <Shield className="w-5 h-5" />
                  Authentication Settings
                </CardTitle>
              </CardHeader>
              <CardContent className="space-y-6">
                <div className="flex items-center justify-between">
                  <div>
                    <p className="font-medium">Require Multi-Factor Authentication</p>
                    <p className="text-sm text-slate-600">Force MFA for all admin users</p>
                  </div>
                  <Switch defaultChecked />
                </div>

                <div className="space-y-2">
                  <label className="text-sm font-medium">Max Login Attempts</label>
                  <Input 
                    type="number" 
                    value={systemConfig.max_login_attempts}
                    onChange={(e) => setSystemConfig({
                      ...systemConfig, 
                      max_login_attempts: parseInt(e.target.value)
                    })}
                  />
                </div>

                <div className="space-y-2">
                  <label className="text-sm font-medium">Session Timeout (minutes)</label>
                  <Input 
                    type="number" 
                    value={systemConfig.session_timeout}
                    onChange={(e) => setSystemConfig({
                      ...systemConfig, 
                      session_timeout: parseInt(e.target.value)
                    })}
                  />
                </div>

                <div className="flex items-center justify-between">
                  <div>
                    <p className="font-medium">Password Complexity Requirements</p>
                    <p className="text-sm text-slate-600">Enforce strong password policies</p>
                  </div>
                  <Switch defaultChecked />
                </div>
              </CardContent>
            </Card>

            <Card>
              <CardHeader>
                <CardTitle>Security Monitoring</CardTitle>
              </CardHeader>
              <CardContent className="space-y-6">
                <div className="flex items-center justify-between">
                  <div>
                    <p className="font-medium">Failed Login Monitoring</p>
                    <p className="text-sm text-slate-600">Alert on suspicious login attempts</p>
                  </div>
                  <Switch defaultChecked />
                </div>

                <div className="flex items-center justify-between">
                  <div>
                    <p className="font-medium">IP Whitelist Enforcement</p>
                    <p className="text-sm text-slate-600">Restrict admin access to approved IPs</p>
                  </div>
                  <Switch />
                </div>

                <div className="flex items-center justify-between">
                  <div>
                    <p className="font-medium">Audit Logging</p>
                    <p className="text-sm text-slate-600">Log all administrative actions</p>
                  </div>
                  <Switch defaultChecked />
                </div>

                <div className="space-y-2">
                  <label className="text-sm font-medium">Audit Log Retention (days)</label>
                  <Input type="number" defaultValue="365" />
                </div>
              </CardContent>
            </Card>
          </div>
        </TabsContent>

        <TabsContent value="notifications" className="space-y-6">
          <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
            <Card>
              <CardHeader>
                <CardTitle className="flex items-center gap-2">
                  <Bell className="w-5 h-5" />
                  Alert Preferences
                </CardTitle>
              </CardHeader>
              <CardContent className="space-y-6">
                {Object.entries(notifications).map(([key, value]) => (
                  <div key={key} className="flex items-center justify-between">
                    <div>
                      <p className="font-medium">
                        {key.replace(/_/g, ' ').replace(/\b\w/g, l => l.toUpperCase())}
                      </p>
                      <p className="text-sm text-slate-600">
                        {key === 'system_alerts' && 'Critical system alerts and failures'}
                        {key === 'security_events' && 'Security incidents and breaches'}
                        {key === 'performance_warnings' && 'Performance degradation warnings'}
                        {key === 'maintenance_updates' && 'Scheduled maintenance notifications'}
                        {key === 'user_activity' && 'User registration and activity alerts'}
                      </p>
                    </div>
                    <Switch 
                      checked={value}
                      onCheckedChange={(checked) => 
                        setNotifications({...notifications, [key]: checked})
                      }
                    />
                  </div>
                ))}
              </CardContent>
            </Card>

            <Card>
              <CardHeader>
                <CardTitle>Notification Channels</CardTitle>
              </CardHeader>
              <CardContent className="space-y-6">
                <div className="space-y-2">
                  <label className="text-sm font-medium">Email Recipients</label>
                  <Textarea 
                    placeholder="admin@quantumskylink.com&#10;alerts@quantumskylink.com"
                    rows={3}
                    defaultValue="admin@quantumskylink.com&#10;devops@quantumskylink.com"
                  />
                </div>

                <div className="space-y-2">
                  <label className="text-sm font-medium">Slack Webhook URL</label>
                  <Input placeholder="https://hooks.slack.com/..." />
                </div>

                <div className="space-y-2">
                  <label className="text-sm font-medium">SMS Recipients</label>
                  <Textarea 
                    placeholder="+1234567890&#10;+0987654321"
                    rows={2}
                  />
                </div>

                <div className="flex items-center justify-between">
                  <div>
                    <p className="font-medium">Critical Alerts Only</p>
                    <p className="text-sm text-slate-600">Send only critical severity alerts via SMS</p>
                  </div>
                  <Switch defaultChecked />
                </div>
              </CardContent>
            </Card>
          </div>
        </TabsContent>

        <TabsContent value="integrations" className="space-y-6">
          <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
            <Card>
              <CardHeader>
                <CardTitle className="flex items-center gap-2">
                  <Zap className="w-5 h-5" />
                  External Services
                </CardTitle>
              </CardHeader>
              <CardContent className="space-y-6">
                <div className="space-y-4">
                  <div className="flex items-center justify-between p-3 border rounded-lg">
                    <div className="flex items-center gap-3">
                      <Mail className="w-5 h-5 text-blue-600" />
                      <div>
                        <p className="font-medium">Email Service</p>
                        <p className="text-sm text-slate-600">SMTP configuration</p>
                      </div>
                    </div>
                    <Badge className="bg-emerald-100 text-emerald-800">Connected</Badge>
                  </div>

                  <div className="flex items-center justify-between p-3 border rounded-lg">
                    <div className="flex items-center gap-3">
                      <Database className="w-5 h-5 text-purple-600" />
                      <div>
                        <p className="font-medium">Backup Service</p>
                        <p className="text-sm text-slate-600">Automated backups</p>
                      </div>
                    </div>
                    <Badge className="bg-emerald-100 text-emerald-800">Connected</Badge>
                  </div>

                  <div className="flex items-center justify-between p-3 border rounded-lg">
                    <div className="flex items-center gap-3">
                      <Shield className="w-5 h-5 text-red-600" />
                      <div>
                        <p className="font-medium">Security Scanner</p>
                        <p className="text-sm text-slate-600">Vulnerability scanning</p>
                      </div>
                    </div>
                    <Badge className="bg-amber-100 text-amber-800">Pending</Badge>
                  </div>
                </div>
              </CardContent>
            </Card>

            <Card>
              <CardHeader>
                <CardTitle>API Configuration</CardTitle>
              </CardHeader>
              <CardContent className="space-y-6">
                <div className="space-y-2">
                  <label className="text-sm font-medium">Rate Limit (requests/minute)</label>
                  <Input type="number" defaultValue="1000" />
                </div>

                <div className="space-y-2">
                  <label className="text-sm font-medium">API Key Expiration (days)</label>
                  <Input type="number" defaultValue="90" />
                </div>

                <div className="flex items-center justify-between">
                  <div>
                    <p className="font-medium">Auto-rotate API Keys</p>
                    <p className="text-sm text-slate-600">Automatically rotate keys before expiration</p>
                  </div>
                  <Switch defaultChecked />
                </div>

                <div className="flex items-center justify-between">
                  <div>
                    <p className="font-medium">CORS Enforcement</p>
                    <p className="text-sm text-slate-600">Enable Cross-Origin Resource Sharing controls</p>
                  </div>
                  <Switch defaultChecked />
                </div>
              </CardContent>
            </Card>
          </div>
        </TabsContent>

        <TabsContent value="maintenance" className="space-y-6">
          <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
            <Card>
              <CardHeader>
                <CardTitle className="flex items-center gap-2">
                  <Clock className="w-5 h-5" />
                  Maintenance Mode
                </CardTitle>
              </CardHeader>
              <CardContent className="space-y-6">
                <div className="flex items-center justify-between">
                  <div>
                    <p className="font-medium">Enable Maintenance Mode</p>
                    <p className="text-sm text-slate-600">Put system in maintenance mode</p>
                  </div>
                  <Switch 
                    checked={systemConfig.maintenance_mode}
                    onCheckedChange={(checked) => 
                      setSystemConfig({...systemConfig, maintenance_mode: checked})
                    }
                  />
                </div>

                <div className="space-y-2">
                  <label className="text-sm font-medium">Maintenance Message</label>
                  <Textarea 
                    placeholder="System is currently under maintenance. Please try again later."
                    rows={3}
                  />
                </div>

                <div className="space-y-2">
                  <label className="text-sm font-medium">Allowed IP Addresses</label>
                  <Textarea 
                    placeholder="192.168.1.100&#10;10.0.0.5"
                    rows={2}
                  />
                  <p className="text-xs text-slate-500">IPs that can access the system during maintenance</p>
                </div>
              </CardContent>
            </Card>

            <Card>
              <CardHeader>
                <CardTitle>Backup & Recovery</CardTitle>
              </CardHeader>
              <CardContent className="space-y-6">
                <div className="space-y-2">
                  <label className="text-sm font-medium">Backup Retention Period (days)</label>
                  <Input 
                    type="number" 
                    value={systemConfig.backup_retention}
                    onChange={(e) => setSystemConfig({
                      ...systemConfig, 
                      backup_retention: parseInt(e.target.value)
                    })}
                  />
                </div>

                <div className="flex items-center justify-between">
                  <div>
                    <p className="font-medium">Automated Daily Backups</p>
                    <p className="text-sm text-slate-600">Schedule automatic daily system backups</p>
                  </div>
                  <Switch defaultChecked />
                </div>

                <div className="space-y-2">
                  <label className="text-sm font-medium">Backup Schedule</label>
                  <Input type="time" defaultValue="02:00" />
                </div>

                <div className="pt-4 space-y-3">
                  <Button className="w-full" variant="outline">
                    <Database className="w-4 h-4 mr-2" />
                    Backup Now
                  </Button>
                  <Button className="w-full" variant="outline">
                    <CheckCircle className="w-4 h-4 mr-2" />
                    Test Recovery
                  </Button>
                </div>
              </CardContent>
            </Card>
          </div>
        </TabsContent>
      </Tabs>
    </div>
  );
}
