import React, { useState } from 'react';
import { Input } from "@/components/ui/input";
import { Button } from "@/components/ui/button";
import { Search, X } from "lucide-react";
import { 
  Command,
  CommandDialog,
  CommandEmpty,
  CommandGroup,
  CommandInput,
  CommandItem,
  CommandList,
} from "@/components/ui/command";

export default function UniversalSearch({ onSearch, placeholder = "Search blocks, transactions, addresses, tokens..." }) {
  const [isOpen, setIsOpen] = useState(false);
  const [searchQuery, setSearchQuery] = useState("");

  const handleSearch = (query) => {
    onSearch(query);
    setIsOpen(false);
    setSearchQuery("");
  };

  const handleKeyPress = (e) => {
    if (e.key === 'Enter' && searchQuery.trim()) {
      handleSearch(searchQuery.trim());
    }
  };

  return (
    <>
      <div className="relative">
        <Button
          variant="outline"
          className="w-full justify-start text-slate-500 bg-white/60 backdrop-blur-sm border-white/80 hover:bg-white/80"
          onClick={() => setIsOpen(true)}
        >
          <Search className="w-4 h-4 mr-2" />
          {placeholder}
        </Button>
      </div>

      <CommandDialog open={isOpen} onOpenChange={setIsOpen}>
        <CommandInput 
          placeholder={placeholder}
          value={searchQuery}
          onValueChange={setSearchQuery}
          onKeyPress={handleKeyPress}
        />
        <CommandList>
          <CommandEmpty>
            {searchQuery ? "No results found." : "Start typing to search..."}
          </CommandEmpty>
          {searchQuery && (
            <CommandGroup heading="Search Results">
              <CommandItem
                onSelect={() => handleSearch(searchQuery)}
                className="cursor-pointer"
              >
                <Search className="w-4 h-4 mr-2" />
                Search for "{searchQuery}"
              </CommandItem>
            </CommandGroup>
          )}
          <CommandGroup heading="Quick Actions">
            <CommandItem onSelect={() => handleSearch("2847392")}>
              Latest Block (#2847392)
            </CommandItem>
            <CommandItem onSelect={() => handleSearch("0xabc123def456789abc123def456789abc123def456789abc123def456789abc123")}>
              Sample Transaction
            </CommandItem>
            <CommandItem onSelect={() => handleSearch("0x742d35cc66a4c4e6dd3c8c65a3d3b8c9d4e5f6a7")}>
              Sample Address
            </CommandItem>
          </CommandGroup>
        </CommandList>
      </CommandDialog>
    </>
  );
}