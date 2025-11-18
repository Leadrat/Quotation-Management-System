import { useEffect, useState } from "react";
import { ClientsApi } from "@/lib/api";

interface ClientSelectorProps {
  value: string;
  onChange: (clientId: string) => void;
  disabled?: boolean;
  required?: boolean;
}

export default function ClientSelector({ value, onChange, disabled = false, required = false }: ClientSelectorProps) {
  const [clients, setClients] = useState<any[]>([]);
  const [loading, setLoading] = useState(true);
  const [selectedClient, setSelectedClient] = useState<any>(null);

  useEffect(() => {
    loadClients();
  }, []);

  useEffect(() => {
    if (value && clients.length > 0) {
      const client = clients.find((c) => c.clientId === value);
      setSelectedClient(client);
    } else {
      setSelectedClient(null);
    }
  }, [value, clients]);

  const loadClients = async () => {
    try {
      setLoading(true);
      const result = await ClientsApi.list(1, 100);
      setClients(result.data || []);
    } catch (err) {
      console.error("Failed to load clients", err);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div>
      <label className="mb-2.5 block text-black dark:text-white">
        Client {required && "*"}
      </label>
      <select
        value={value}
        onChange={(e) => onChange(e.target.value)}
        required={required}
        disabled={disabled || loading}
        className="w-full rounded border-[1.5px] border-stroke bg-transparent px-5 py-3 font-medium outline-none transition focus:border-primary active:border-primary dark:border-form-strokedark dark:bg-form-input dark:text-white disabled:opacity-50"
      >
        <option value="">{loading ? "Loading clients..." : "Select a client"}</option>
        {clients.map((client) => (
          <option key={client.clientId} value={client.clientId}>
            {client.companyName} - {client.email}
          </option>
        ))}
      </select>
      {selectedClient && (
        <div className="mt-2 text-sm text-gray-600 dark:text-gray-400">
          <p>State: {selectedClient.state || "N/A"} ({selectedClient.stateCode || "N/A"})</p>
          {selectedClient.gstin && <p>GSTIN: {selectedClient.gstin}</p>}
        </div>
      )}
    </div>
  );
}

