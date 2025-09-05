import React, { useState } from 'react';
import { Copy, Check } from "lucide-react";
import { Button } from "@/components/ui/button";

export default function DetailItem({ label, children, isHash = false, onCopy, isLink = false, copyValue }) {
  const [copied, setCopied] = useState(false);

  const handleCopy = () => {
    const textToCopy = copyValue || (typeof children === 'string' ? children : null);
    if (textToCopy) {
      navigator.clipboard.writeText(textToCopy);
      if (onCopy) onCopy(textToCopy);
      setCopied(true);
      setTimeout(() => setCopied(false), 2000);
    }
  };
  
  const canCopy = isHash && (copyValue || typeof children === 'string');

  return (
    <div className="py-4 border-b border-slate-200/60 flex flex-col sm:flex-row items-start sm:items-center justify-between gap-2">
      <dt className="text-sm font-medium text-slate-600 w-full sm:w-1/4">{label}</dt>
      <dd className={`mt-1 sm:mt-0 text-sm text-slate-900 w-full sm:w-3/4 flex items-center gap-2 ${isHash ? 'font-mono' : ''} ${isLink ? 'text-blue-600 hover:underline' : ''}`}>
        <span className="break-all">{children}</span>
        {canCopy && (
          <Button
            variant="ghost"
            size="sm"
            onClick={handleCopy}
            className="h-7 w-7 p-0 shrink-0"
          >
            {copied ? <Check className="w-4 h-4 text-green-500" /> : <Copy className="w-4 h-4 text-slate-500" />}
          </Button>
        )}
      </dd>
    </div>
  );
}