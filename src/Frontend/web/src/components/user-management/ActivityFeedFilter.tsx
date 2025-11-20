"use client";
import React from "react";
import Input from "@/components/tailadmin/form/input/InputField";
import Label from "@/components/tailadmin/form/Label";
import Button from "@/components/tailadmin/ui/button/Button";

interface ActivityFeedFilterProps {
  filters: {
    userId?: string;
    actionType?: string;
    entityType?: string;
    fromDate?: string;
    toDate?: string;
  };
  onFilterChange: (filters: ActivityFeedFilterProps["filters"]) => void;
  onReset: () => void;
}

export default function ActivityFeedFilter({
  filters,
  onFilterChange,
  onReset,
}: ActivityFeedFilterProps) {
  return (
    <div className="rounded-lg border border-stroke bg-white p-4 dark:border-strokedark dark:bg-boxdark mb-4">
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-5 gap-4">
        <div>
          <Label htmlFor="userId">User ID</Label>
          <Input
            id="userId"
            type="text"
            value={filters.userId || ""}
            onChange={(e) => onFilterChange({ ...filters, userId: e.target.value || undefined })}
          />
        </div>
        <div>
          <Label htmlFor="actionType">Action Type</Label>
          <Input
            id="actionType"
            type="text"
            value={filters.actionType || ""}
            onChange={(e) => onFilterChange({ ...filters, actionType: e.target.value || undefined })}
          />
        </div>
        <div>
          <Label htmlFor="entityType">Entity Type</Label>
          <Input
            id="entityType"
            type="text"
            value={filters.entityType || ""}
            onChange={(e) => onFilterChange({ ...filters, entityType: e.target.value || undefined })}
          />
        </div>
        <div>
          <Label htmlFor="fromDate">From Date</Label>
          <Input
            id="fromDate"
            type="date"
            value={filters.fromDate || ""}
            onChange={(e) => onFilterChange({ ...filters, fromDate: e.target.value || undefined })}
          />
        </div>
        <div>
          <Label htmlFor="toDate">To Date</Label>
          <Input
            id="toDate"
            type="date"
            value={filters.toDate || ""}
            onChange={(e) => onFilterChange({ ...filters, toDate: e.target.value || undefined })}
          />
        </div>
      </div>
      <div className="mt-4 flex justify-end">
        <Button type="button" onClick={onReset}>
          Reset Filters
        </Button>
      </div>
    </div>
  );
}

