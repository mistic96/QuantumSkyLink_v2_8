import Layout from "./Layout.jsx";

import Dashboard from "./Dashboard";

import SystemHealth from "./SystemHealth";

import AlertsCenter from "./AlertsCenter";

import Users from "./Users";

import Treasury from "./Treasury";

import Services from "./Services";

import Compliance from "./Compliance";

import Analytics from "./Analytics";

import Reports from "./Reports";

import AccessControl from "./AccessControl";

import Database from "./Database";

import Network from "./Network";

import Settings from "./Settings";

import AssetManagement from "./AssetManagement";

import { BrowserRouter as Router, Route, Routes, useLocation } from 'react-router-dom';

const PAGES = {
    
    Dashboard: Dashboard,
    
    SystemHealth: SystemHealth,
    
    AlertsCenter: AlertsCenter,
    
    Users: Users,
    
    Treasury: Treasury,
    
    Services: Services,
    
    Compliance: Compliance,
    
    Analytics: Analytics,
    
    Reports: Reports,
    
    AccessControl: AccessControl,
    
    Database: Database,
    
    Network: Network,
    
    Settings: Settings,
    
    AssetManagement: AssetManagement,
    
}

function _getCurrentPage(url) {
    if (url.endsWith('/')) {
        url = url.slice(0, -1);
    }
    let urlLastPart = url.split('/').pop();
    if (urlLastPart.includes('?')) {
        urlLastPart = urlLastPart.split('?')[0];
    }

    const pageName = Object.keys(PAGES).find(page => page.toLowerCase() === urlLastPart.toLowerCase());
    return pageName || Object.keys(PAGES)[0];
}

// Create a wrapper component that uses useLocation inside the Router context
function PagesContent() {
    const location = useLocation();
    const currentPage = _getCurrentPage(location.pathname);
    
    return (
        <Layout currentPageName={currentPage}>
            <Routes>            
                
                    <Route path="/" element={<Dashboard />} />
                
                
                <Route path="/Dashboard" element={<Dashboard />} />
                
                <Route path="/SystemHealth" element={<SystemHealth />} />
                
                <Route path="/AlertsCenter" element={<AlertsCenter />} />
                
                <Route path="/Users" element={<Users />} />
                
                <Route path="/Treasury" element={<Treasury />} />
                
                <Route path="/Services" element={<Services />} />
                
                <Route path="/Compliance" element={<Compliance />} />
                
                <Route path="/Analytics" element={<Analytics />} />
                
                <Route path="/Reports" element={<Reports />} />
                
                <Route path="/AccessControl" element={<AccessControl />} />
                
                <Route path="/Database" element={<Database />} />
                
                <Route path="/Network" element={<Network />} />
                
                <Route path="/Settings" element={<Settings />} />
                
                <Route path="/AssetManagement" element={<AssetManagement />} />
                
            </Routes>
        </Layout>
    );
}

export default function Pages() {
    return (
        <Router>
            <PagesContent />
        </Router>
    );
}