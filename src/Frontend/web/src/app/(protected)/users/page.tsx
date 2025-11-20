"use client";
import { useEffect, useState } from "react";
import Link from "next/link";
import { UsersApi } from "@/lib/api";
import { getAccessToken } from "@/lib/session";
import PageBreadcrumb from "@/components/tailadmin/common/PageBreadCrumb";
import ComponentCard from "@/components/tailadmin/common/ComponentCard";
import Input from "@/components/tailadmin/form/input/InputField";
import Label from "@/components/tailadmin/form/Label";
import Button from "@/components/tailadmin/ui/button/Button";
import { Table, TableBody, TableCell, TableHeader, TableRow } from "@/components/tailadmin/ui/table";
import Pagination from "@/components/tailadmin/tables/Pagination";
import Alert from "@/components/tailadmin/ui/alert/Alert";

export default function UsersListPage() {
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [items, setItems] = useState<any[]>([]);
  const [pageNumber, setPageNumber] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [total, setTotal] = useState(0);
  const [searchTerm, setSearchTerm] = useState("");

  const totalPages = Math.ceil(total / pageSize);

  async function load(p = 1) {
    if (!getAccessToken()) {
      setLoading(false);
      return;
    }
    setLoading(true);
    setError(null);
    try {
      const res = await UsersApi.list({ 
        pageNumber: p, 
        pageSize, 
        searchTerm: searchTerm || undefined 
      });
      setItems(res.data || []);
      setPageNumber(res.pageNumber);
      setPageSize(res.pageSize);
      setTotal(res.totalCount);
    } catch (e: any) {
      if (e?.message?.includes("401")) {
        setItems([]);
        setTotal(0);
        return;
      }
      setError(e.message || "Failed to load users");
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    const token = getAccessToken();
    if (!token) return;
    load();
  }, []);

  useEffect(() => {
    const timer = setTimeout(() => {
      if (pageNumber === 1) {
        load(1);
      } else {
        setPageNumber(1);
      }
    }, 500);
    return () => clearTimeout(timer);
  }, [searchTerm]);

  useEffect(() => {
    load(pageNumber);
  }, [pageNumber]);

  return (
    <>
      <PageBreadcrumb pageName="Users" />
      
      <div className="flex flex-col gap-6">
        <ComponentCard>
          <div className="flex items-center justify-between mb-4">
            <h2 className="text-xl font-semibold text-gray-800 dark:text-white/90">Users</h2>
            <div className="flex items-center gap-2">
              <Link href="/users/new">
                <Button size="sm">New User</Button>
              </Link>
            </div>
          </div>

          <div className="mb-4">
            <Label>Search</Label>
            <Input
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              placeholder="Search by name or email..."
            />
          </div>

          {error && <Alert className="mb-4" variant="error" title="Error" message={error} />}
          
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
                      <TableCell isHeader className="px-5 py-3 font-medium text-gray-500 text-start text-theme-xs dark:text-gray-400">Name</TableCell>
                      <TableCell isHeader className="px-5 py-3 font-medium text-gray-500 text-start text-theme-xs dark:text-gray-400">Email</TableCell>
                      <TableCell isHeader className="px-5 py-3 font-medium text-gray-500 text-start text-theme-xs dark:text-gray-400">Role</TableCell>
                      <TableCell isHeader className="px-5 py-3 font-medium text-gray-500 text-start text-theme-xs dark:text-gray-400">Status</TableCell>
                      <TableCell isHeader className="px-5 py-3 font-medium text-gray-500 text-start text-theme-xs dark:text-gray-400">Actions</TableCell>
                    </TableRow>
                  </TableHeader>
                  <TableBody className="divide-y divide-gray-100 dark:divide-white/[0.05]">
                    {items.length === 0 ? (
                      <TableRow>
                        <TableCell colSpan={5} className="px-5 py-8 text-center text-gray-500 dark:text-gray-400">
                          No users found
                        </TableCell>
                      </TableRow>
                    ) : (
                      items.map((user) => (
                        <TableRow key={user.userId || user.id}>
                          <TableCell className="px-5 py-4 text-gray-800 text-theme-sm dark:text-white/90">
                            {user.firstName} {user.lastName}
                          </TableCell>
                          <TableCell className="px-5 py-4 text-gray-500 text-theme-sm dark:text-gray-400">
                            {user.email}
                          </TableCell>
                          <TableCell className="px-5 py-4 text-gray-500 text-theme-sm dark:text-gray-400">
                            {user.roleName || user.role || "N/A"}
                          </TableCell>
                          <TableCell className="px-5 py-4">
                            <span className={`px-2 py-1 text-xs rounded-full ${
                              user.isActive !== false 
                                ? "bg-green-100 text-green-800 dark:bg-green-900/30 dark:text-green-400" 
                                : "bg-red-100 text-red-800 dark:bg-red-900/30 dark:text-red-400"
                            }`}>
                              {user.isActive !== false ? "Active" : "Inactive"}
                            </span>
                          </TableCell>
                          <TableCell className="px-5 py-4">
                            <div className="flex items-center gap-2">
                              <Link href={`/users/${user.userId || user.id}/activity`} className="text-brand-500 hover:text-brand-600 text-sm">
                                View
                              </Link>
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
        </ComponentCard>
      </div>
    </>
  );
}

