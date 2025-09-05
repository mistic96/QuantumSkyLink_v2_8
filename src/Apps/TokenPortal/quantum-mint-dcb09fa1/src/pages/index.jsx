import Layout from "./Layout.jsx";

import Dashboard from "./Dashboard";

import TokenCreator from "./TokenCreator";

import AssetManager from "./AssetManager";

import Compliance from "./Compliance";

import Analytics from "./Analytics";

import Portfolio from "./Portfolio";

import { BrowserRouter as Router, Route, Routes, useLocation } from 'react-router-dom';

const PAGES = {
    
    Dashboard: Dashboard,
    
    TokenCreator: TokenCreator,
    
    AssetManager: AssetManager,
    
    Compliance: Compliance,
    
    Analytics: Analytics,
    
    Portfolio: Portfolio,
    
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
                
                <Route path="/TokenCreator" element={<TokenCreator />} />
                
                <Route path="/AssetManager" element={<AssetManager />} />
                
                <Route path="/Compliance" element={<Compliance />} />
                
                <Route path="/Analytics" element={<Analytics />} />
                
                <Route path="/Portfolio" element={<Portfolio />} />
                
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