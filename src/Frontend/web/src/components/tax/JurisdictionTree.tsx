"use client";
import { useState } from "react";
import Badge from "@/components/tailadmin/ui/badge/Badge";
import Button from "@/components/tailadmin/ui/button/Button";

interface Jurisdiction {
  jurisdictionId: string;
  jurisdictionName: string;
  jurisdictionCode?: string;
  jurisdictionType?: string;
  isActive: boolean;
  parentJurisdictionId?: string;
  childJurisdictions?: Jurisdiction[];
}

interface JurisdictionTreeProps {
  jurisdictions: Jurisdiction[];
  onEdit?: (jurisdiction: Jurisdiction) => void;
  onDelete?: (jurisdictionId: string) => void;
  onAddChild?: (parentId: string) => void;
}

export default function JurisdictionTree({
  jurisdictions,
  onEdit,
  onDelete,
  onAddChild,
}: JurisdictionTreeProps) {
  const [expanded, setExpanded] = useState<Set<string>>(new Set());

  const toggleExpand = (jurisdictionId: string) => {
    const newExpanded = new Set(expanded);
    if (newExpanded.has(jurisdictionId)) {
      newExpanded.delete(jurisdictionId);
    } else {
      newExpanded.add(jurisdictionId);
    }
    setExpanded(newExpanded);
  };

  const getRootJurisdictions = () => {
    return jurisdictions.filter(j => !j.parentJurisdictionId);
  };

  const getChildJurisdictions = (parentId: string) => {
    return jurisdictions.filter(j => j.parentJurisdictionId === parentId);
  };

  const renderJurisdiction = (jurisdiction: Jurisdiction, level: number = 0) => {
    const children = getChildJurisdictions(jurisdiction.jurisdictionId);
    const hasChildren = children.length > 0;
    const isExpanded = expanded.has(jurisdiction.jurisdictionId);

    return (
      <div key={jurisdiction.jurisdictionId} className="mb-2">
        <div
          className={`flex items-center gap-2 p-2 rounded hover:bg-gray-50 dark:hover:bg-boxdark-2`}
          style={{ paddingLeft: `${level * 24 + 8}px` }}
        >
          {hasChildren && (
            <button
              onClick={() => toggleExpand(jurisdiction.jurisdictionId)}
              className="text-body-color dark:text-body-color-dark hover:text-primary"
              aria-label={isExpanded ? "Collapse" : "Expand"}
            >
              <svg
                className={`w-4 h-4 transition-transform ${isExpanded ? "rotate-90" : ""}`}
                fill="none"
                stroke="currentColor"
                viewBox="0 0 24 24"
              >
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 5l7 7-7 7" />
              </svg>
            </button>
          )}
          {!hasChildren && <div className="w-4" />}
          <div className="flex-1">
            <div className="font-medium text-black dark:text-white">
              {jurisdiction.jurisdictionName}
            </div>
            {jurisdiction.jurisdictionCode && (
              <div className="text-xs text-gray-500">Code: {jurisdiction.jurisdictionCode}</div>
            )}
            {jurisdiction.jurisdictionType && (
              <div className="text-xs text-gray-500">Type: {jurisdiction.jurisdictionType}</div>
            )}
          </div>
          <Badge className={jurisdiction.isActive ? "bg-green-100 text-green-800" : "bg-gray-100 text-gray-800"}>
            {jurisdiction.isActive ? "Active" : "Inactive"}
          </Badge>
          <div className="flex gap-2">
            {onAddChild && (
              <Button
                variant="outline"
                size="sm"
                onClick={() => onAddChild(jurisdiction.jurisdictionId)}
              >
                Add Child
              </Button>
            )}
            {onEdit && (
              <Button
                variant="outline"
                size="sm"
                onClick={() => onEdit(jurisdiction)}
              >
                Edit
              </Button>
            )}
            {onDelete && (
              <Button
                variant="outline"
                size="sm"
                onClick={() => {
                  if (confirm("Are you sure you want to delete this jurisdiction?")) {
                    onDelete(jurisdiction.jurisdictionId);
                  }
                }}
              >
                Delete
              </Button>
            )}
          </div>
        </div>
        {hasChildren && isExpanded && (
          <div>
            {children.map(child => renderJurisdiction(child, level + 1))}
          </div>
        )}
      </div>
    );
  };

  const rootJurisdictions = getRootJurisdictions();

  if (rootJurisdictions.length === 0) {
    return (
      <div className="text-center py-8 text-gray-500">
        No jurisdictions found
      </div>
    );
  }

  return (
    <div className="space-y-2">
      {rootJurisdictions.map(jurisdiction => renderJurisdiction(jurisdiction, 0))}
    </div>
  );
}

