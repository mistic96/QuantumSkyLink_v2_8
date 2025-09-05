import React from 'react';
import { Card, CardHeader, CardTitle, CardDescription } from '@/components/ui/card';

export default function DetailHeader({ title, subtitle, icon: Icon, badge }) {
  return (
    <div className="flex flex-col lg:flex-row justify-between items-start lg:items-center gap-6 mb-8">
      <div className="flex items-center gap-4">
        {Icon && (
          <div className="w-16 h-16 bg-gradient-to-r from-blue-100 to-indigo-100 rounded-lg flex items-center justify-center">
            <Icon className="w-8 h-8 text-blue-600" />
          </div>
        )}
        <div>
          <h1 className="text-4xl font-bold text-slate-900 mb-1">
            {title}
          </h1>
          <p className="text-lg text-slate-600 break-all">
            {subtitle}
          </p>
        </div>
      </div>
      {badge && <div className="flex-shrink-0">{badge}</div>}
    </div>
  );
}