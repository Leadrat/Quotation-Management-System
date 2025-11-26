export type ImportSession = {
  importSessionId: string;
  sourceType?: string;
  status: string;
  suggestedMappingsJson?: string | null;
  confirmedMappingsJson?: string | null;
};

const API_BASE = process.env.NEXT_PUBLIC_API_BASE ?? "";

export async function createImport(formData: FormData) {
  const res = await fetch(`${API_BASE}/api/imports`, {
    method: "POST",
    body: formData,
  });
  if (!res.ok) throw new Error("Failed to create import session");
  return (await res.json()) as ImportSession;
}

export async function getImport(id: string) {
  const res = await fetch(`${API_BASE}/api/imports/${id}`);
  if (!res.ok) throw new Error("Failed to fetch import session");
  return (await res.json()) as ImportSession;
}

export async function postChat(id: string, message: string) {
  const res = await fetch(`${API_BASE}/api/imports/${id}/chat`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ message }),
  });
  if (!res.ok) throw new Error("Failed to send chat message");
  return await res.json();
}

export async function saveMappings(id: string, mappings: Record<string, unknown>) {
  const res = await fetch(`${API_BASE}/api/imports/${id}/mappings`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ mappings }),
  });
  if (!res.ok) throw new Error("Failed to save mappings");
  return await res.json();
}

export async function generateTemplate(id: string) {
  const res = await fetch(`${API_BASE}/api/imports/${id}/generate`, { method: "POST" });
  if (!res.ok) throw new Error("Failed to generate preview");
  return await res.json();
}

export function getPreviewUrl(id: string) {
  return `${API_BASE}/api/imports/${id}/preview`;
}

export async function saveTemplateApi(id: string, name: string, type: string) {
  const res = await fetch(`${API_BASE}/api/imports/${id}/save-template`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ name, type }),
  });
  if (!res.ok) throw new Error("Failed to save template");
  return await res.json();
}
