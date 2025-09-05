
import React, { useState } from "react";
import { Link, useLocation, useNavigate } from "react-router-dom";
import { createPageUrl } from "@/utils";
import {
  Blocks,
  Coins,
  LayoutDashboard,
  Wallet,
  ArrowRightLeft,
  AreaChart,
  BookText,
  Menu,
  X,
  Sun,
  Moon,
  Github
} from "lucide-react";
import UniversalSearch from '../components/search/UniversalSearch';
import { Button } from '@/components/ui/button';

const navLinks = [
  { href: "Dashboard", text: "Dashboard", icon: LayoutDashboard },
  { href: "Blocks", text: "Blocks", icon: Blocks },
  { href: "Transactions", text: "Transactions", icon: ArrowRightLeft },
  { href: "Addresses", text: "Addresses", icon: Wallet },
  { href: "Tokens", text: "Tokens", icon: Coins },
  { href: "Analytics", text: "Analytics", icon: AreaChart },
  { href: "ApiDocumentation", text: "API Docs", icon: BookText },
];

function NavLink({ href, icon: Icon, text, isCurrent, onClick }) {
  return (
    <Link
      to={createPageUrl(href)}
      className={`
        flex items-center gap-3 p-3 rounded-lg text-sm font-medium transition-colors duration-200
        ${isCurrent ? 'bg-blue-100 text-blue-700' : 'text-slate-600 hover:bg-slate-100 hover:text-slate-900'}
      `}
      onClick={onClick}
    >
      <Icon className="w-5 h-5" />
      <span>{text}</span>
    </Link>
  );
}

export default function Layout({ children, currentPageName }) {
  const location = useLocation();
  const navigate = useNavigate();
  const [isSidebarOpen, setIsSidebarOpen] = useState(false);
  const [theme, setTheme] = useState('light');

  const handleSearch = async (query) => {
    if (!query) return;
    const cleanedQuery = query.trim();

    // Check if it's a block number
    if (!isNaN(cleanedQuery) && !cleanedQuery.startsWith('0x')) {
      navigate(createPageUrl(`BlockDetails?number=${cleanedQuery}`));
      return;
    }

    // Check if it's a hash (address, block hash, or transaction hash)
    if (cleanedQuery.startsWith('0x')) {
      // For now, just navigate to dashboard - you can implement actual search logic here
      navigate(createPageUrl(`Dashboard`));
      return;
    }
    
    console.log(`No results found for query: ${cleanedQuery}`);
  };

  const toggleTheme = () => {
    setTheme((prevTheme) => (prevTheme === 'light' ? 'dark' : 'light'));
  };

  return (
    <div className={`min-h-screen bg-gradient-to-br from-slate-50 to-slate-200 text-slate-800 font-sans ${theme === 'dark' ? 'dark' : ''}`}>
      {/* Fixed Sidebar */}
      <div className={`fixed top-0 left-0 h-full bg-white/70 backdrop-blur-lg border-r border-slate-200/80 transition-transform duration-300 ease-in-out z-50 ${isSidebarOpen ? 'w-64 translate-x-0' : '-translate-x-full'} lg:w-64 lg:translate-x-0`}>
        {/* Sidebar Header/Logo */}
        <div className="flex items-center justify-between p-4 h-20 border-b border-slate-200/80">
          <Link to={createPageUrl('Dashboard')} className="flex items-center gap-2">
            <svg width="32" height="32" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg" className="text-blue-600">
              <path d="M12 2L2 7V17L12 22L22 17V7L12 2Z" stroke="currentColor" strokeWidth="2" strokeLinejoin="round"/>
              <path d="M2 7L12 12L22 7" stroke="currentColor" strokeWidth="2" strokeLinejoin="round"/>
              <path d="M12 12V22" stroke="currentColor" strokeWidth="2" strokeLinejoin="round"/>
            </svg>
            <h1 className="text-xl font-bold text-slate-900">Quantum Ledger</h1>
          </Link>
          <Button variant="ghost" size="icon" className="lg:hidden" onClick={() => setIsSidebarOpen(false)}>
            <X className="w-6 h-6" />
          </Button>
        </div>
        {/* Navigation links */}
        <nav className="p-4">
          <ul>
            {navLinks.map((link) => (
              <li key={link.href}>
                <NavLink
                  href={link.href}
                  icon={link.icon}
                  text={link.text}
                  isCurrent={currentPageName === link.href}
                  onClick={() => setIsSidebarOpen(false)}
                />
              </li>
            ))}
          </ul>
        </nav>
        {/* Sidebar Footer */}
        <div className="absolute bottom-4 left-4 right-4 p-4 bg-slate-100/80 rounded-lg">
          <p className="text-xs text-slate-600">© {new Date().getFullYear()} Quantum Ledger Inc. All rights reserved.</p>
        </div>
      </div>
      
      {/* Main content area */}
      <div className="lg:pl-64 transition-all duration-300 ease-in-out">
        {/* Main Header */}
        <header className="sticky top-0 z-40 bg-white/30 backdrop-blur-lg">
          <div className="flex items-center justify-between p-4 h-20 border-b border-slate-200/80">
            <div className="flex items-center gap-4">
              <Button variant="ghost" size="icon" className="lg:hidden" onClick={() => setIsSidebarOpen(true)}>
                <Menu className="w-6 h-6" />
              </Button>
              <div className="hidden md:block">
                <h2 className="text-lg font-semibold">{currentPageName}</h2>
              </div>
            </div>
            <div className="flex items-center gap-4">
              <div className="w-48 md:w-80">
                <UniversalSearch onSearch={handleSearch} />
              </div>
              <Button variant="ghost" size="icon" onClick={toggleTheme}>
                {theme === 'light' ? (
                  <Sun className="h-[1.2rem] w-[1.2rem]" />
                ) : (
                  <Moon className="h-[1.2rem] w-[1.2rem]" />
                )}
              </Button>
              <a href="https://github.com/your-repo" target="_blank" rel="noopener noreferrer">
                <Button variant="ghost" size="icon">
                  <Github className="w-5 h-5"/>
                </Button>
              </a>
            </div>
          </div>
        </header>
        {/* Main content children */}
        <main className="p-6">
          {children}
        </main>
      </div>
    </div>
  );
}
