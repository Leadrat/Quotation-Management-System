"use client";
import { useEffect, useState, useCallback } from "react";
import { HubConnection, HubConnectionBuilder } from "@microsoft/signalr";
import { getAccessToken } from "@/lib/session";
import { PresenceStatus } from "@/types/userManagement";
import { UserManagementApi } from "@/lib/api";

const API_BASE = process.env.NEXT_PUBLIC_API_BASE_URL || "";

export function usePresence(userId: string) {
  const [connection, setConnection] = useState<HubConnection | null>(null);
  const [presenceStatus, setPresenceStatus] = useState<PresenceStatus>("Offline");
  const [connected, setConnected] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (!userId) return;

    const token = getAccessToken();
    if (!token) return;

    const newConnection = new HubConnectionBuilder()
      .withUrl(`${API_BASE}/hubs/presence`, {
        accessTokenFactory: () => token,
      })
      .withAutomaticReconnect()
      .build();

    newConnection.start()
      .then(() => {
        setConnected(true);
        setError(null);
        // Update presence to Online when connected
        newConnection.invoke("UpdatePresence", "Online");
      })
      .catch((err) => {
        setError(err.message);
        console.error("SignalR connection error:", err);
      });

    newConnection.on("UserPresenceChanged", (changedUserId: string, status: string) => {
      if (changedUserId === userId) {
        setPresenceStatus(status as PresenceStatus);
      }
    });

    newConnection.onclose(() => {
      setConnected(false);
    });

    setConnection(newConnection);

    return () => {
      newConnection.stop();
    };
  }, [userId]);

  const updatePresence = useCallback(async (status: PresenceStatus) => {
    if (connection && connected) {
      try {
        await connection.invoke("UpdatePresence", status);
        setPresenceStatus(status);
      } catch (err: any) {
        console.error("Failed to update presence via SignalR:", err);
        // Fallback to REST API
        try {
          await UserManagementApi.profiles.updatePresence(userId, status);
          setPresenceStatus(status);
        } catch (apiErr: any) {
          setError(apiErr.message || "Failed to update presence");
        }
      }
    } else {
      // Fallback to REST API if SignalR not connected
      try {
        await UserManagementApi.profiles.updatePresence(userId, status);
        setPresenceStatus(status);
      } catch (err: any) {
        setError(err.message || "Failed to update presence");
      }
    }
  }, [connection, connected, userId]);

  return {
    presenceStatus,
    connected,
    error,
    updatePresence,
  };
}

