"use client";
import React, { useState } from "react";
import { SetOutOfOfficeRequest } from "@/types/userManagement";
import Input from "@/components/tailadmin/form/input/InputField";
import Label from "@/components/tailadmin/form/Label";
import Button from "@/components/tailadmin/ui/button/Button";
import Alert from "@/components/tailadmin/ui/alert/Alert";

interface OutOfOfficeToggleProps {
  isOutOfOffice: boolean;
  message?: string;
  delegateUserId?: string;
  onSubmit: (data: SetOutOfOfficeRequest) => Promise<void>;
}

export default function OutOfOfficeToggle({
  isOutOfOffice,
  message,
  delegateUserId,
  onSubmit,
}: OutOfOfficeToggleProps) {
  const [isOOO, setIsOOO] = useState(isOutOfOffice);
  const [oooMessage, setOooMessage] = useState(message || "");
  const [delegateId, setDelegateId] = useState(delegateUserId || "");
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);
    setLoading(true);
    try {
      await onSubmit({
        isOutOfOffice: isOOO,
        message: oooMessage || undefined,
        delegateUserId: delegateId || undefined,
      });
    } catch (e: any) {
      setError(e.message || "Failed to update out-of-office status");
    } finally {
      setLoading(false);
    }
  };

  return (
    <form onSubmit={handleSubmit} className="space-y-4">
      {error && (
        <Alert color="danger">{error}</Alert>
      )}

      <div className="flex items-center gap-2">
        <input
          type="checkbox"
          id="isOutOfOffice"
          checked={isOOO}
          onChange={(e) => setIsOOO(e.target.checked)}
          disabled={loading}
          className="rounded border-stroke"
        />
        <Label htmlFor="isOutOfOffice" className="mb-0">Out of Office</Label>
      </div>

      {isOOO && (
        <>
          <div>
            <Label htmlFor="message">Out-of-Office Message</Label>
            <textarea
              id="message"
              value={oooMessage}
              onChange={(e) => setOooMessage(e.target.value)}
              rows={3}
              placeholder="I'm currently out of office..."
              className="w-full rounded border border-stroke bg-transparent px-5 py-3 text-black outline-none focus:border-primary focus-visible:shadow-none dark:border-strokedark dark:bg-boxdark dark:text-white dark:focus:border-primary"
              disabled={loading}
            />
          </div>

          <div>
            <Label htmlFor="delegateUserId">Delegate To User ID</Label>
            <Input
              id="delegateUserId"
              type="text"
              value={delegateId}
              onChange={(e) => setDelegateId(e.target.value)}
              placeholder="Optional: User ID to delegate to"
              disabled={loading}
            />
          </div>
        </>
      )}

      <Button type="submit" color="primary" disabled={loading}>
        {loading ? "Saving..." : "Update Status"}
      </Button>
    </form>
  );
}

