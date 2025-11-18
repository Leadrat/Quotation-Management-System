"use client";
import { useEffect, useMemo, useRef, useState } from "react";
import Link from "next/link";
import { ClientsApi } from "@/lib/api";
import { getAccessToken } from "@/lib/session";

type FilterOptions = {
  states: { state: string; count: number }[];
  cities: { city: string; count: number }[];
  createdDateRanges: { label: string; from: string; to: string }[];
  stateCodes: { code: string; name: string }[];
};

export default function ClientsListPage() {
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [items, setItems] = useState<any[]>([]);
  const [pageNumber, setPageNumber] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [total, setTotal] = useState(0);

  // Filters & search
  const [searchTerm, setSearchTerm] = useState("");
  const [city, setCity] = useState("");
  const [state, setState] = useState("");
  const [stateCode, setStateCode] = useState("");
  const [gstin, setGstin] = useState("");
  const [createdFrom, setCreatedFrom] = useState("");
  const [createdTo, setCreatedTo] = useState("");
  const [sortBy, setSortBy] = useState("CreatedAtDesc");

  // Suggestions
  const [suggestions, setSuggestions] = useState<string[]>([]);
  const [showSuggestions, setShowSuggestions] = useState(false);
  const suggestTimer = useRef<any>(null);

  // Filter options
  const [options, setOptions] = useState<FilterOptions | null>(null);

  // Saved searches
  const [saved, setSaved] = useState<any[]>([]);
  const [saveName, setSaveName] = useState("");

  const params = useMemo(() => ({
    searchTerm: searchTerm || undefined,
    city: city || undefined,
    state: state || undefined,
    stateCode: stateCode || undefined,
    gstin: gstin || undefined,
    createdDateFrom: createdFrom || undefined,
    createdDateTo: createdTo || undefined,
    sortBy,
    pageNumber,
    pageSize,
  }), [searchTerm, city, state, stateCode, gstin, createdFrom, createdTo, sortBy, pageNumber, pageSize]);

  async function load(p = 1) {
    // Only load if authenticated
    if (!getAccessToken()) {
      setLoading(false);
      return;
    }
    setLoading(true); setError(null);
    try {
      const res = await ClientsApi.search({ ...params, pageNumber: p });
      setItems(res.data || []);
      setPageNumber(res.pageNumber);
      setPageSize(res.pageSize);
      setTotal(res.totalCount);
    } catch (e: any) {
      // Silently ignore 401 errors
      if (e?.message?.includes("401")) {
        setItems([]);
        setTotal(0);
        return;
      }
      setError(e.message || "Failed to load");
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    // Only load if authenticated
    const token = getAccessToken();
    if (!token) return;
    
    // Load filter options and saved searches on mount
    ClientsApi.filterOptions().then(r => setOptions(r.data)).catch((e: any) => {
      if (!e?.message?.includes("401")) console.error("Failed to load filter options", e);
    });
    ClientsApi.getSaved().then(r => setSaved(r.data || [])).catch((e: any) => {
      if (!e?.message?.includes("401")) console.error("Failed to load saved searches", e);
    });
    load(1);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  // Debounce search
  useEffect(() => {
    const t = setTimeout(() => load(1), 400);
    return () => clearTimeout(t);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [searchTerm, city, state, stateCode, gstin, createdFrom, createdTo, sortBy, pageSize]);

  // Suggestions debounce
  useEffect(() => {
    if (!searchTerm || searchTerm.length < 2) { setSuggestions([]); return; }
    if (suggestTimer.current) clearTimeout(suggestTimer.current);
    suggestTimer.current = setTimeout(async () => {
      try {
        const s = await ClientsApi.suggestions(searchTerm, "CompanyName", 8);
        setSuggestions(s.data || []); setShowSuggestions(true);
      } catch { setSuggestions([]); }
    }, 300);
  }, [searchTerm]);

  const totalPages = Math.max(1, Math.ceil(total / pageSize));

  function applySaved(s: any) {
    const f = s.filterCriteria || {};
    setSearchTerm(f.searchTerm || "");
    setCity(f.city || "");
    setState(f.state || "");
    setStateCode(f.stateCode || "");
    setGstin(f.gstin || "");
    setCreatedFrom(f.createdDateFrom || "");
    setCreatedTo(f.createdDateTo || "");
    if (s.sortBy) setSortBy(s.sortBy);
    load(1);
  }

  async function saveCurrent() {
    try {
      const filterCriteria: any = { searchTerm, city, state, stateCode, gstin, createdDateFrom: createdFrom, createdDateTo: createdTo };
      await ClientsApi.saveSearch({ searchName: saveName || "Quick Filter", filterCriteria, sortBy });
      const r = await ClientsApi.getSaved();
      setSaved(r.data || []);
      setSaveName("");
    } catch (e: any) { alert(e.message || "Failed to save"); }
  }

  async function deleteSaved(id: string) {
    await ClientsApi.deleteSaved(id);
    const r = await ClientsApi.getSaved();
    setSaved(r.data || []);
  }

  return (
    <div className="flex gap-6">
      {/* Sidebar Filters */}
      <aside className="w-72 border rounded p-3 self-start">
        <div className="mb-3">
          <label className="block text-sm mb-1">Search</label>
          <div className="relative">
            <input value={searchTerm} onChange={e=>setSearchTerm(e.target.value)} onFocus={()=>setShowSuggestions(true)} onBlur={()=>setTimeout(()=>setShowSuggestions(false), 150)}
                   placeholder="Company, contact or email" className="w-full border rounded px-3 py-2" />
            {showSuggestions && suggestions.length>0 && (
              <div className="absolute z-10 bg-white border w-full mt-1 rounded shadow">
                {suggestions.map((s,i)=> (
                  <div key={i} className="px-3 py-1 hover:bg-gray-100 cursor-pointer" onMouseDown={()=>{ setSearchTerm(s); setShowSuggestions(false); }}>
                    {s}
                  </div>
                ))}
              </div>
            )}
          </div>
        </div>

        <div className="mb-3">
          <label className="block text-sm mb-1">City</label>
          <input value={city} onChange={e=>setCity(e.target.value)} className="w-full border rounded px-3 py-2" />
          <div className="text-xs mt-1">Top cities:</div>
          <div className="flex flex-wrap gap-2 mt-1">
            {options?.cities?.slice(0,6).map(c => (
              <button key={c.city} className="px-2 py-1 text-xs border rounded" onClick={()=>setCity(c.city)}>{c.city} ({c.count})</button>
            ))}
          </div>
        </div>

        <div className="mb-3">
          <label className="block text-sm mb-1">State</label>
          <input value={state} onChange={e=>setState(e.target.value)} className="w-full border rounded px-3 py-2" />
          <div className="text-xs mt-1">Popular states:</div>
          <div className="flex flex-wrap gap-2 mt-1">
            {options?.states?.slice(0,6).map(s => (
              <button key={s.state} className="px-2 py-1 text-xs border rounded" onClick={()=>setState(s.state)}>{s.state} ({s.count})</button>
            ))}
          </div>
        </div>

        <div className="mb-3">
          <label className="block text-sm mb-1">GSTIN</label>
          <input value={gstin} onChange={e=>setGstin(e.target.value)} className="w-full border rounded px-3 py-2" />
        </div>

        <div className="mb-3">
          <label className="block text-sm mb-1">State Code</label>
          <select value={stateCode} onChange={e=>setStateCode(e.target.value)} className="w-full border rounded px-3 py-2">
            <option value="">All</option>
            {options?.stateCodes?.map(code => (
              <option key={code.code} value={code.code}>{code.code}</option>
            ))}
          </select>
        </div>

        <div className="mb-3">
          <label className="block text-sm mb-1">Created Date</label>
          <div className="flex gap-2">
            <input type="date" value={createdFrom} onChange={e=>setCreatedFrom(e.target.value)} className="border rounded px-2 py-1 w-full" />
            <input type="date" value={createdTo} onChange={e=>setCreatedTo(e.target.value)} className="border rounded px-2 py-1 w-full" />
          </div>
          <div className="flex flex-wrap gap-2 mt-2">
            {options?.createdDateRanges?.map(r => (
              <button key={r.label} className="px-2 py-1 text-xs border rounded" onClick={()=>{ setCreatedFrom(r.from); setCreatedTo(r.to); }}>
                {r.label}
              </button>
            ))}
          </div>
        </div>

        <div className="mb-3">
          <label className="block text-sm mb-1">Sort By</label>
          <select value={sortBy} onChange={e=>setSortBy(e.target.value)} className="w-full border rounded px-3 py-2">
            <option value="NameAsc">Name A-Z</option>
            <option value="NameDesc">Name Z-A</option>
            <option value="CreatedAtDesc">Newest</option>
            <option value="CreatedAtAsc">Oldest</option>
            <option value="UpdatedAtDesc">Recently Updated</option>
            <option value="EmailAsc">Email A-Z</option>
          </select>
        </div>

        <div className="mb-3">
          <label className="block text-sm mb-1">Save current filter</label>
          <div className="flex gap-2">
            <input value={saveName} onChange={e=>setSaveName(e.target.value)} placeholder="Name" className="border rounded px-3 py-2 w-full"/>
            <button onClick={saveCurrent} className="px-3 py-2 border rounded">Save</button>
          </div>
        </div>

        <div>
          <div className="font-medium mb-1">Saved searches</div>
          <div className="space-y-1">
            {saved.length === 0 && <div className="text-xs">No saved searches</div>}
            {saved.map((s:any) => (
              <div key={s.savedSearchId} className="flex items-center justify-between text-sm border rounded px-2 py-1">
                <button onClick={()=>applySaved(s)} className="hover:underline text-left truncate">{s.searchName}</button>
                <button onClick={()=>deleteSaved(s.savedSearchId)} className="text-red-600 text-xs">Delete</button>
              </div>
            ))}
          </div>
        </div>
      </aside>

      {/* Main */}
      <div className="flex-1">
        <div className="flex items-center justify-between mb-4">
          <h1 className="text-2xl font-semibold">Clients</h1>
          <div className="flex items-center gap-2">
            <button onClick={()=>ClientsApi.exportCsv(params)} className="rounded border px-3 py-2 text-sm">Export CSV</button>
            <Link href="/clients/new" className="rounded bg-blue-600 text-white px-3 py-2 text-sm hover:bg-blue-700">New Client</Link>
          </div>
        </div>
        {error && <div className="text-red-600 mb-3 text-sm">{error}</div>}
        {loading ? (
          <div>Loading...</div>
        ) : (
          <div className="overflow-x-auto border rounded">
            <table className="min-w-full text-sm">
              <thead className="bg-gray-50">
                <tr>
                  <th className="text-left px-3 py-2">Company</th>
                  <th className="text-left px-3 py-2">Email</th>
                  <th className="text-left px-3 py-2">Mobile</th>
                  <th className="text-left px-3 py-2">Actions</th>
                </tr>
              </thead>
              <tbody>
                {items.map((c) => (
                  <tr key={c.clientId} className="border-t">
                    <td className="px-3 py-2">{c.companyName}</td>
                    <td className="px-3 py-2">{c.email}</td>
                    <td className="px-3 py-2">{c.mobile}</td>
                    <td className="px-3 py-2 space-x-2">
                      <Link href={`/clients/${c.clientId}`} className="text-blue-600 hover:underline">View</Link>
                      <Link href={`/clients/${c.clientId}/edit`} className="text-gray-700 hover:underline">Edit</Link>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
        <div className="flex items-center justify-between mt-4 text-sm">
          <div>Page {pageNumber} / {totalPages} ({total} total)</div>
          <div className="space-x-2">
            <button onClick={() => load(Math.max(1, pageNumber - 1))} disabled={pageNumber<=1} className="px-3 py-1 rounded border disabled:opacity-50">Prev</button>
            <button onClick={() => load(Math.min(totalPages, pageNumber + 1))} disabled={pageNumber>=totalPages} className="px-3 py-1 rounded border disabled:opacity-50">Next</button>
          </div>
        </div>
      </div>
    </div>
  );
}
