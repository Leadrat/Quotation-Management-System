"use client";
import React, { useState, useEffect } from "react";
import { CreateTeamRequest, UpdateTeamRequest, Team } from "@/types/userManagement";
import Input from "@/components/tailadmin/form/input/InputField";
import Label from "@/components/tailadmin/form/Label";
import Button from "@/components/tailadmin/ui/button/Button";
import Alert from "@/components/tailadmin/ui/alert/Alert";

interface TeamFormProps {
  team?: Team;
  onSubmit: (data: CreateTeamRequest | UpdateTeamRequest) => Promise<void>;
  onCancel: () => void;
}

export default function TeamForm({ team, onSubmit, onCancel }: TeamFormProps) {
  const [name, setName] = useState(team?.name || "");
  const [description, setDescription] = useState(team?.description || "");
  const [teamLeadUserId, setTeamLeadUserId] = useState(team?.teamLeadUserId || "");
  const [parentTeamId, setParentTeamId] = useState(team?.parentTeamId || "");
  const [companyId, setCompanyId] = useState(team?.companyId || "");
  const [isActive, setIsActive] = useState(team?.isActive ?? true);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);
    setLoading(true);
    try {
      if (team) {
        await onSubmit({
          name: name || undefined,
          description: description || undefined,
          teamLeadUserId: teamLeadUserId || undefined,
          parentTeamId: parentTeamId || undefined,
          isActive,
        } as UpdateTeamRequest);
      } else {
        await onSubmit({
          name,
          description: description || undefined,
          teamLeadUserId,
          parentTeamId: parentTeamId || undefined,
          companyId,
        } as CreateTeamRequest);
      }
    } catch (e: any) {
      setError(e.message || "Failed to save team");
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
        <Label htmlFor="name">Team Name *</Label>
        <Input
          id="name"
          type="text"
          value={name}
          onChange={(e) => setName(e.target.value)}
          required
          disabled={loading}
        />
      </div>

      <div>
        <Label htmlFor="description">Description</Label>
        <textarea
          id="description"
          value={description}
          onChange={(e) => setDescription(e.target.value)}
          rows={3}
          className="w-full rounded border border-stroke bg-transparent px-5 py-3 text-black outline-none focus:border-primary focus-visible:shadow-none dark:border-strokedark dark:bg-boxdark dark:text-white dark:focus:border-primary"
          disabled={loading}
        />
      </div>

      <div>
        <Label htmlFor="teamLeadUserId">Team Lead User ID *</Label>
        <Input
          id="teamLeadUserId"
          type="text"
          value={teamLeadUserId}
          onChange={(e) => setTeamLeadUserId(e.target.value)}
          required
          disabled={loading}
        />
      </div>

      {!team && (
        <div>
          <Label htmlFor="companyId">Company ID *</Label>
          <Input
            id="companyId"
            type="text"
            value={companyId}
            onChange={(e) => setCompanyId(e.target.value)}
            required
            disabled={loading}
          />
        </div>
      )}

      <div>
        <Label htmlFor="parentTeamId">Parent Team ID</Label>
        <Input
          id="parentTeamId"
          type="text"
          value={parentTeamId}
          onChange={(e) => setParentTeamId(e.target.value)}
          disabled={loading}
        />
      </div>

      {team && (
        <div className="flex items-center gap-2">
          <input
            type="checkbox"
            id="isActive"
            checked={isActive}
            onChange={(e) => setIsActive(e.target.checked)}
            disabled={loading}
            className="rounded border-stroke"
          />
          <Label htmlFor="isActive" className="mb-0">Active</Label>
        </div>
      )}

      <div className="flex gap-2 justify-end">
        <Button type="button" onClick={onCancel} disabled={loading}>
          Cancel
        </Button>
        <Button type="submit" color="primary" disabled={loading}>
          {loading ? "Saving..." : team ? "Update Team" : "Create Team"}
        </Button>
      </div>
    </form>
  );
}

