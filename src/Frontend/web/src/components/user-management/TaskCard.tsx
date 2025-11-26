"use client";
import React from "react";
import { TaskAssignment } from "@/types/userManagement";
import Badge from "@/components/tailadmin/ui/badge/Badge";

interface TaskCardProps {
  task: TaskAssignment;
  onStatusUpdate?: (assignmentId: string, status: string) => void;
  canUpdate?: boolean;
}

export default function TaskCard({ task, onStatusUpdate, canUpdate = false }: TaskCardProps) {
  const getStatusColor = (status: string) => {
    switch (status) {
      case "Completed":
        return "success";
      case "InProgress":
        return "primary";
      case "Cancelled":
        return "danger";
      default:
        return "warning";
    }
  };

  const getStatusOptions = () => {
    if (task.status === "Completed" || task.status === "Cancelled") {
      return [];
    }
    return ["Pending", "InProgress", "Completed", "Cancelled"].filter(s => s !== task.status);
  };

  return (
    <div className={`rounded-lg border p-4 ${
      task.isOverdue ? "border-danger bg-danger/5" : "border-stroke bg-white dark:border-strokedark dark:bg-boxdark"
    }`}>
      <div className="flex items-start justify-between mb-3">
        <div className="flex-1">
          <div className="flex items-center gap-2 mb-2">
            <h4 className="font-semibold text-black dark:text-white">
              {task.entityType} Task
            </h4>
            <Badge color={getStatusColor(task.status)} className="text-xs">
              {task.status}
            </Badge>
            {task.isOverdue && (
              <Badge color="danger" className="text-xs">Overdue</Badge>
            )}
          </div>
          <p className="text-sm text-body-color dark:text-body-color-dark mb-2">
            <span className="font-medium">Assigned to:</span> {task.assignedToUserName}
          </p>
          <p className="text-sm text-body-color dark:text-body-color-dark mb-2">
            <span className="font-medium">Assigned by:</span> {task.assignedByUserName}
          </p>
          {task.dueDate && (
            <p className="text-sm text-body-color dark:text-body-color-dark">
              <span className="font-medium">Due:</span>{" "}
              {new Date(task.dueDate).toLocaleDateString()}
            </p>
          )}
        </div>
      </div>
      {canUpdate && onStatusUpdate && getStatusOptions().length > 0 && (
        <div className="flex gap-2 mt-3">
          {getStatusOptions().map(status => (
            <button
              key={status}
              onClick={() => onStatusUpdate(task.assignmentId, status)}
              className="px-3 py-1 text-xs rounded border border-stroke hover:bg-gray-50 dark:hover:bg-boxdark-2"
            >
              Mark as {status}
            </button>
          ))}
        </div>
      )}
    </div>
  );
}

