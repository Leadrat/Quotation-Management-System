"use client";
import { useEffect, useMemo, useRef, useState } from "react";
import Link from "next/link";
import { ClientsApi } from "@/lib/api";
import { getAccessToken } from "@/lib/session";
import PageBreadcrumb from "@/components/tailadmin/common/PageBreadCrumb";
import ComponentCard from "@/components/tailadmin/common/ComponentCard";
import Input from "@/components/tailadmin/form/input/InputField";
import Label from "@/components/tailadmin/form/Label";
import Button from "@/components/tailadmin/ui/button/Button";
import { Table, TableBody, TableCell, TableHeader, TableRow } from "@/components/tailadmin/ui/table";
import Pagination from "@/components/tailadmin/tables/Pagination";

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
    <>
      <PageBreadcrumb pageTitle="Clients" />
      
      <div className="flex gap-6">
        {/* Sidebar Filters */}
        <aside className="w-72 self-start">
          <ComponentCard title="Filters" className="sticky top-4">
            <div className="space-y-4">
              <div>
                <Label>Search</Label>
                <div className="relative">
                  <Input 
                    value={searchTerm} 
                    onChange={e=>setSearchTerm(e.target.value)} 
                    onFocus={()=>setShowSuggestions(true)} 
                    onBlur={()=>setTimeout(()=>setShowSuggestions(false), 150)}
                    placeholder="Company, contact or email" 
                  />
                  {showSuggestions && suggestions.length>0 && (
                    <div className="absolute z-10 bg-white border border-gray-200 w-full mt-1 rounded-lg shadow-theme-lg dark:bg-gray-900 dark:border-gray-800">
                      {suggestions.map((s,i)=> (
                        <div key={i} className="px-3 py-2 hover:bg-gray-100 dark:hover:bg-white/5 cursor-pointer text-sm text-gray-800 dark:text-white/90" onMouseDown={()=>{ setSearchTerm(s); setShowSuggestions(false); }}>
                          {s}
                        </div>
                      ))}
                    </div>
                  )}
                </div>
              </div>

              <div>
                <Label>City</Label>
                <Input value={city} onChange={e=>setCity(e.target.value)} placeholder="Enter city" />
                {options?.cities && options.cities.length > 0 && (
                  <>
                    <p className="text-xs text-gray-500 dark:text-gray-400 mt-2 mb-1">Top cities:</p>
                    <div className="flex flex-wrap gap-2">
                      {options.cities.slice(0,6).map(c => (
                        <button key={c.city} className="px-2 py-1 text-xs border border-gray-200 rounded-lg hover:bg-gray-50 dark:border-gray-800 dark:hover:bg-white/5" onClick={()=>setCity(c.city)}>{c.city} ({c.count})</button>
                      ))}
                    </div>
                  </>
                )}
              </div>

              <div>
                <Label>State</Label>
                <Input value={state} onChange={e=>setState(e.target.value)} placeholder="Enter state" />
                {options?.states && options.states.length > 0 && (
                  <>
                    <p className="text-xs text-gray-500 dark:text-gray-400 mt-2 mb-1">Popular states:</p>
                    <div className="flex flex-wrap gap-2">
                      {options.states.slice(0,6).map(s => (
                        <button key={s.state} className="px-2 py-1 text-xs border border-gray-200 rounded-lg hover:bg-gray-50 dark:border-gray-800 dark:hover:bg-white/5" onClick={()=>setState(s.state)}>{s.state} ({s.count})</button>
                      ))}
                    </div>
                  </>
                )}
              </div>

              <div>
                <Label>GSTIN</Label>
                <Input value={gstin} onChange={e=>setGstin(e.target.value)} placeholder="Enter GSTIN" />
              </div>

              <div>
                <Label>State Code</Label>
                <select 
                  value={stateCode} 
                  onChange={e=>setStateCode(e.target.value)} 
                  className="h-11 w-full appearance-none rounded-lg border border-gray-300 px-4 py-2.5 pr-11 text-sm shadow-theme-xs focus:border-brand-300 focus:outline-hidden focus:ring-3 focus:ring-brand-500/10 dark:border-gray-700 dark:bg-gray-900 dark:text-white/90"
                >
                  <option value="">All</option>
                  {options?.stateCodes?.map(code => (
                    <option key={code.code} value={code.code}>{code.code}</option>
                  ))}
                </select>
              </div>

              <div>
                <Label>Created Date</Label>
                <div className="flex gap-2">
                  <Input type="date" value={createdFrom} onChange={e=>setCreatedFrom(e.target.value)} />
                  <Input type="date" value={createdTo} onChange={e=>setCreatedTo(e.target.value)} />
                </div>
                {options?.createdDateRanges && options.createdDateRanges.length > 0 && (
                  <div className="flex flex-wrap gap-2 mt-2">
                    {options.createdDateRanges.map(r => (
                      <button key={r.label} className="px-2 py-1 text-xs border border-gray-200 rounded-lg hover:bg-gray-50 dark:border-gray-800 dark:hover:bg-white/5" onClick={()=>{ setCreatedFrom(r.from); setCreatedTo(r.to); }}>
                        {r.label}
                      </button>
                    ))}
                  </div>
                )}
              </div>

              <div>
                <Label>Sort By</Label>
                <select 
                  value={sortBy} 
                  onChange={e=>setSortBy(e.target.value)} 
                  className="h-11 w-full appearance-none rounded-lg border border-gray-300 px-4 py-2.5 pr-11 text-sm shadow-theme-xs focus:border-brand-300 focus:outline-hidden focus:ring-3 focus:ring-brand-500/10 dark:border-gray-700 dark:bg-gray-900 dark:text-white/90"
                >
                  <option value="NameAsc">Name A-Z</option>
                  <option value="NameDesc">Name Z-A</option>
                  <option value="CreatedAtDesc">Newest</option>
                  <option value="CreatedAtAsc">Oldest</option>
                  <option value="UpdatedAtDesc">Recently Updated</option>
                  <option value="EmailAsc">Email A-Z</option>
                </select>
              </div>

              <div>
                <Label>Save current filter</Label>
                <div className="flex gap-2">
                  <Input value={saveName} onChange={e=>setSaveName(e.target.value)} placeholder="Name" className="flex-1"/>
                  <Button size="sm" onClick={saveCurrent}>Save</Button>
                </div>
              </div>

              <div>
                <Label>Saved searches</Label>
                <div className="space-y-1 mt-2">
                  {saved.length === 0 && <p className="text-xs text-gray-500 dark:text-gray-400">No saved searches</p>}
                  {saved.map((s:any) => (
                    <div key={s.savedSearchId} className="flex items-center justify-between text-sm border border-gray-200 rounded-lg px-2 py-1 dark:border-gray-800">
                      <button onClick={()=>applySaved(s)} className="hover:underline text-left truncate text-gray-700 dark:text-gray-400">{s.searchName}</button>
                      <button onClick={()=>deleteSaved(s.savedSearchId)} className="text-error-500 text-xs hover:text-error-600">Delete</button>
                    </div>
                  ))}
                </div>
              </div>
            </div>
          </ComponentCard>
        </aside>

        {/* Main */}
        <div className="flex-1">
          <div className="flex items-center justify-between mb-6">
            <h2 className="text-xl font-semibold text-gray-800 dark:text-white/90">Clients</h2>
            <div className="flex items-center gap-2">
              <Link href="/clients/new">
                <Button size="sm">New Client</Button>
              </Link>
            </div>
          </div>
          
          {loading ? (
            <ComponentCard>
              <div className="text-center py-8 text-gray-500 dark:text-gray-400">Loading...</div>
            </ComponentCard>
          ) : (
            <div className="overflow-hidden rounded-xl border border-gray-200 bg-white dark:border-white/[0.05] dark:bg-white/[0.03]">
              <div className="max-w-full overflow-x-auto">
                <Table>
                  <TableHeader className="border-b border-gray-100 dark:border-white/[0.05]">
                    <TableRow>
                      <TableCell isHeader className="px-5 py-3 font-medium text-gray-500 text-start text-theme-xs dark:text-gray-400">Company</TableCell>
                      <TableCell isHeader className="px-5 py-3 font-medium text-gray-500 text-start text-theme-xs dark:text-gray-400">Email</TableCell>
                      <TableCell isHeader className="px-5 py-3 font-medium text-gray-500 text-start text-theme-xs dark:text-gray-400">Mobile</TableCell>
                      <TableCell isHeader className="px-5 py-3 font-medium text-gray-500 text-start text-theme-xs dark:text-gray-400">Actions</TableCell>
                    </TableRow>
                  </TableHeader>
                  <TableBody className="divide-y divide-gray-100 dark:divide-white/[0.05]">
                    {items.length === 0 ? (
                      <TableRow>
                        <TableCell colSpan={4} className="px-5 py-8 text-center text-gray-500 dark:text-gray-400">
                          No clients found
                        </TableCell>
                      </TableRow>
                    ) : (
                      items.map((c) => (
                        <TableRow key={c.clientId}>
                          <TableCell className="px-5 py-4 text-gray-800 text-theme-sm dark:text-white/90">{c.companyName}</TableCell>
                          <TableCell className="px-5 py-4 text-gray-500 text-theme-sm dark:text-gray-400">{c.email}</TableCell>
                          <TableCell className="px-5 py-4 text-gray-500 text-theme-sm dark:text-gray-400">{c.mobile}</TableCell>
                          <TableCell className="px-5 py-4">
                            <div className="flex items-center gap-2">
                              <Link href={`/clients/${c.clientId}`} className="text-brand-500 hover:text-brand-600 text-sm">View</Link>
                              <Link href={`/clients/${c.clientId}/edit`} className="text-gray-500 hover:text-gray-700 dark:text-gray-400 dark:hover:text-gray-300 text-sm">Edit</Link>
                            </div>
                          </TableCell>
                        </TableRow>
                      ))
                    )}
                  </TableBody>
                </Table>
              </div>
            </div>
          )}
          
          {!loading && items.length > 0 && (
            <div className="flex items-center justify-between mt-4">
              <div className="text-sm text-gray-500 dark:text-gray-400">
                Page {pageNumber} / {totalPages} ({total} total)
              </div>
              <Pagination 
                currentPage={pageNumber} 
                totalPages={totalPages} 
                onPageChange={(p) => load(p)} 
              />
            </div>
          )}
        </div>
      </div>
    </>
  );
}
