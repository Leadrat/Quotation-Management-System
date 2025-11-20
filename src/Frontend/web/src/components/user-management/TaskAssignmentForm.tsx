"use client";
import React, { useState } from "react";
import { AssignTaskRequest } from "@/types/userManagement";
import Input from "@/components/tailadmin/form/input/InputField";
import Label from "@/components/tailadmin/form/Label";
import Button from "@/components/tailadmin/ui/button/Button";
import Alert from "@/components/tailadmin/ui/alert/Alert";

interface TaskAssignmentFormProps {
  onSubmit: (data: AssignTaskRequest) => Promise<void>;
  onCancel: () => void;
  defaultEntityType?: string;
  defaultEntityId?: string;
}

export default function TaskAssignmentForm({
  onSubmit,
  onCancel,
  defaultEntityType,
  defaultEntityId,
}: TaskAssignmentFormProps) {
  const [entityType, setEntityType] = useState(defaultEntityType || "Quotation");
  const [entityId, setEntityId] = useState(defaultEntityId || "");
  const [assignedToUserId, setAssignedToUserId] = useState("");
  const [dueDate, setDueDate] = useState("");
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);
    setLoading(true);
    try {
      await onSubmit({
        entityType,
        entityId,
        assignedToUserId,
        dueDate: dueDate || undefined,
      });
    } catch (e: any) {
      setError(e.message || "Failed to assign task");
    } finally {
      setLoading(false);
    }
  };

  return (
    <form onSubmit={handleSubmit} className="space-y-4">
      {error && (
        <Alert color="danger">{error}</Alert>
      )}
      
      <div>
        <Label htmlFor="entityType">Entity Type *</Label>
        <select
          id="entityType"
          value={entityType}
          onChange={(e) => setEntityType(e.target.value)}
          required
          disabled={loading || !!defaultEntityType}
          className="w-full rounded border border-stroke bg-transparent px-5 py-3 text-black outline-none focus:border-primary focus-visible:shadow-none dark:border-strokedark dark:bg-boxdark dark:text-white dark:focus:border-primary"
        >
          <option value="Quotation">Quotation</option>
          <option value="Approval">Approval</option>
          <option value="Client">Client</option>
        </select>
      </div>

      <div>
        <Label htmlFor="entityId">Entity ID *</Label>
        <Input
          id="entityId"
          type="text"
          value={entityId}
          onChange={(e) => setEntityId(e.target.value)}
          required
          disabled={loading || !!defaultEntityId}
        />
      </div>

      <div>
        <Label htmlFor="assignedToUserId">Assign To User ID *</Label>
        <Input
          id="assignedToUserId"
          type="text"
          value={assignedToUserId}
          onChange={(e) => setAssignedToUserId(e.target.value)}
          required
          disabled={loading}
        />
      </div>

      <div>
        <Label htmlFor="dueDate">Due Date</Label>
        <Input
          id="dueDate"
          type="datetime-local"
          value={dueDate}
          onChange={(e) => setDueDate(e.target.value)}
          disabled={loading}
        />
      </div>

      <div className="flex gap-2 justify-end">
        <Button type="button" onClick={onCancel} disabled={loading}>
          Cancel
        </Button>
        <Button type="submit" color="primary" disabled={loading}>
          {loading ? "Assigning..." : "Assign Task"}
        </Button>
      </div>
    </form>
  );
}

