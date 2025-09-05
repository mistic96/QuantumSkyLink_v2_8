import Layout from "./Layout.jsx";

import Dashboard from "./Dashboard";

import Tokens from "./Tokens";

import Blocks from "./Blocks";

import Transactions from "./Transactions";

import Addresses from "./Addresses";

import Analytics from "./Analytics";

import BlockDetails from "./BlockDetails";

import TransactionDetails from "./TransactionDetails";

import AddressDetails from "./AddressDetails";

import TokenDetails from "./TokenDetails";

import ApiDocumentation from "./ApiDocumentation";

import { BrowserRouter as Router, Route, Routes, useLocation } from 'react-router-dom';

const PAGES = {
    
    Dashboard: Dashboard,
    
    Tokens: Tokens,
    
    Blocks: Blocks,
    
    Transactions: Transactions,
    
    Addresses: Addresses,
    
    Analytics: Analytics,
    
    BlockDetails: BlockDetails,
    
    TransactionDetails: TransactionDetails,
    
    AddressDetails: AddressDetails,
    
    TokenDetails: TokenDetails,
    
    ApiDocumentation: ApiDocumentation,
    
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
                
                <Route path="/Tokens" element={<Tokens />} />
                
                <Route path="/Blocks" element={<Blocks />} />
                
                <Route path="/Transactions" element={<Transactions />} />
                
                <Route path="/Addresses" element={<Addresses />} />
                
                <Route path="/Analytics" element={<Analytics />} />
                
                <Route path="/BlockDetails" element={<BlockDetails />} />
                
                <Route path="/TransactionDetails" element={<TransactionDetails />} />
                
                <Route path="/AddressDetails" element={<AddressDetails />} />
                
                <Route path="/TokenDetails" element={<TokenDetails />} />
                
                <Route path="/ApiDocumentation" element={<ApiDocumentation />} />
                
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