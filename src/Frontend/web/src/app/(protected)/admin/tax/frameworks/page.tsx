"use client";
import { useState, useEffect } from "react";
import PageBreadcrumb from "@/components/tailadmin/common/PageBreadCrumb";
import TaxFrameworkForm from "@/components/tax/TaxFrameworkForm";
import { TaxFrameworksApi, CountriesApi } from "@/lib/api";
import Button from "@/components/tailadmin/ui/button/Button";
import { Table, TableBody, TableCell, TableHeader, TableRow } from "@/components/tailadmin/ui/table";
import Badge from "@/components/tailadmin/ui/badge/Badge";
import { useToast, ToastContainer } from "@/components/quotations/Toast";

export default function TaxFrameworksPage() {
  const [showForm, setShowForm] = useState(false);
  const [editingFramework, setEditingFramework] = useState<any>(null);
  const [frameworks, setFrameworks] = useState<any[]>([]);
  const [loading, setLoading] = useState(true);
  const toast = useToast();

  useEffect(() => {
    loadData();
  }, []);

  async function loadData() {
    try {
      setLoading(true);
      const res = await TaxFrameworksApi.list();
      setFrameworks(Array.isArray(res.data) ? res.data : []);
    } catch (e: any) {
      toast.error(e.message || "Failed to load tax frameworks");
    } finally {
      setLoading(false);
    }
  }

  const handleCreate = () => {
    setEditingFramework(null);
    setShowForm(true);
  };

  const handleEdit = (framework: any) => {
    setEditingFramework(framework);
    setShowForm(true);
  };

  const handleSave = async (data: any) => {
    try {
      if (editingFramework) {
        await TaxFrameworksApi.update(editingFramework.taxFrameworkId, data);
        toast.success("Tax framework updated successfully");
      } else {
        await TaxFrameworksApi.create(data);
        toast.success("Tax framework created successfully");
      }
      setShowForm(false);
      setEditingFramework(null);
      await loadData();
    } catch (e: any) {
      throw e; // Let form handle error display
    }
  };

  if (loading) {
    return (
      <div className="text-center py-8">
        <div className="inline-block animate-spin rounded-full h-8 w-8 border-b-2 border-gray-900"></div>
        <p className="mt-2 text-gray-600">Loading tax frameworks...</p>
      </div>
    );
  }

  return (
    <div>
      <ToastContainer toasts={toast.toasts} onRemove={toast.removeToast} />
      <PageBreadcrumb pageTitle="Tax Frameworks" />

      <div className="rounded-sm border border-stroke bg-white shadow-default dark:border-strokedark dark:bg-boxdark">
        <div className="border-b border-stroke px-6.5 py-4 dark:border-strokedark">
          <div className="flex justify-between items-center">
            <div>
              <h3 className="font-medium text-black dark:text-white">
                Tax Framework Management
              </h3>
              <p className="text-sm text-gray-600 dark:text-gray-400 mt-1">
                Configure tax frameworks (GST, VAT) for countries
              </p>
            </div>
            {!showForm && (
              <Button onClick={handleCreate}>Create Tax Framework</Button>
            )}
          </div>
        </div>

        <div className="p-6.5">
          {!showForm ? (
            <div className="overflow-x-auto">
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableCell>Country</TableCell>
                    <TableCell>Framework Name</TableCell>
                    <TableCell>Framework Type</TableCell>
                    <TableCell>Tax Components</TableCell>
                    <TableCell>Actions</TableCell>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {frameworks.length === 0 ? (
                    <TableRow>
                      <TableCell colSpan={5} className="text-center py-8 text-gray-500">
                        No tax frameworks found
                      </TableCell>
                    </TableRow>
                  ) : (
                    frameworks.map((framework) => (
                      <TableRow key={framework.taxFrameworkId}>
                        <TableCell>{framework.countryName || "N/A"}</TableCell>
                        <TableCell className="font-medium">{framework.frameworkName}</TableCell>
                        <TableCell>
                          <Badge className="bg-blue-100 text-blue-800">
                            {framework.frameworkType}
                          </Badge>
                        </TableCell>
                        <TableCell>
                          {framework.taxComponents && framework.taxComponents.length > 0 ? (
                            <div className="flex flex-wrap gap-1">
                              {framework.taxComponents.map((comp: any, idx: number) => (
                                <Badge key={idx} className="bg-gray-100 text-gray-800 text-xs">
                                  {comp.name}
                                </Badge>
                              ))}
                            </div>
                          ) : (
                            <span className="text-gray-400">-</span>
                          )}
                        </TableCell>
                        <TableCell>
                          <Button
                            variant="outline"
                            size="sm"
                            onClick={() => handleEdit(framework)}
                          >
                            Edit
                          </Button>
                        </TableCell>
                      </TableRow>
                    ))
                  )}
                </TableBody>
              </Table>
            </div>
          ) : (
            <div className="bg-white dark:bg-gray-800 rounded-lg border p-6">
              <h4 className="text-lg font-semibold mb-4">
                {editingFramework ? "Edit Tax Framework" : "Create Tax Framework"}
              </h4>
              <TaxFrameworkForm
                taxFramework={editingFramework}
                onSave={handleSave}
                onCancel={() => {
                  setShowForm(false);
                  setEditingFramework(null);
                }}
              />
            </div>
          )}
        </div>
      </div>
    </div>
  );
}
