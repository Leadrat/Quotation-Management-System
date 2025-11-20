"use client";
import { useState, useEffect } from "react";
import { useRouter } from "next/navigation";
import { ProductsApi, ProductCategoriesApi } from "@/lib/api";
import { useToast } from "@/components/quotations/Toast";
import type { CreateProductRequest, ProductType, ProductCategory } from "@/types/products";
import Input from "@/components/tailadmin/form/input/InputField";
import Label from "@/components/tailadmin/form/Label";
import Button from "@/components/tailadmin/ui/button/Button";
import BillingCycleSelector from "./BillingCycleSelector";

interface ProductFormProps {
  productId?: string;
  initialData?: Partial<CreateProductRequest>;
}

export default function ProductForm({ productId, initialData }: ProductFormProps) {
  const router = useRouter();
  const toast = useToast();
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [categories, setCategories] = useState<ProductCategory[]>([]);

  const [formData, setFormData] = useState<CreateProductRequest>({
    productName: initialData?.productName || "",
    productType: initialData?.productType || "Subscription",
    description: initialData?.description || "",
    categoryId: initialData?.categoryId,
    basePricePerUserPerMonth: initialData?.basePricePerUserPerMonth,
    billingCycleMultipliers: initialData?.billingCycleMultipliers || {
      quarterly: 0.95,
      halfYearly: 0.90,
      yearly: 0.85,
      multiYear: 0.80,
    },
    currency: initialData?.currency || "USD",
    isActive: initialData?.isActive !== undefined ? initialData.isActive : true,
  });

  useEffect(() => {
    ProductCategoriesApi.list({ isActive: true })
      .then((res) => setCategories(res.data || []))
      .catch((err) => console.error("Failed to load categories", err));
  }, []);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading(true);
    setError(null);

    try {
      // Prepare request payload with JSONB fields serialized as strings
      const payload: any = {
        productName: formData.productName,
        productType: formData.productType,
        description: formData.description,
        categoryId: formData.categoryId,
        currency: formData.currency,
        isActive: formData.isActive,
      };

      if (formData.productType === "Subscription") {
        payload.basePricePerUserPerMonth = formData.basePricePerUserPerMonth;
        if (formData.billingCycleMultipliers) {
          payload.billingCycleMultipliers = JSON.stringify(formData.billingCycleMultipliers);
        }
      }

      if (formData.productType === "AddOnSubscription" || formData.productType === "AddOnOneTime") {
        if (formData.addOnPricing) {
          payload.addOnPricing = JSON.stringify(formData.addOnPricing);
        }
      }

      if (formData.productType === "CustomDevelopment") {
        if (formData.customDevelopmentPricing) {
          payload.customDevelopmentPricing = JSON.stringify(formData.customDevelopmentPricing);
        }
      }

      if (productId) {
        await ProductsApi.update(productId, payload);
        toast.success("Product updated successfully!");
      } else {
        await ProductsApi.create(payload);
        toast.success("Product created successfully!");
      }
      router.push("/products/catalog");
    } catch (err: any) {
      const errorMsg = err.message || "Failed to save product";
      setError(errorMsg);
      toast.error(errorMsg);
    } finally {
      setLoading(false);
    }
  };

  return (
    <form onSubmit={handleSubmit} className="space-y-6">
      {error && (
        <div className="rounded border border-red-500 bg-red-50 p-4 text-red-700 dark:bg-red-900/20 dark:text-red-400">
          {error}
        </div>
      )}

      <div>
        <Label htmlFor="productName" required>
          Product Name
        </Label>
        <Input
          id="productName"
          type="text"
          required
          value={formData.productName}
          onChange={(e) => setFormData({ ...formData, productName: e.target.value })}
          placeholder="e.g., Cloud Storage - 1TB per user/month"
        />
      </div>

      <div>
        <Label htmlFor="productType" required>
          Product Type
        </Label>
        <select
          id="productType"
          required
          value={formData.productType}
          onChange={(e) => setFormData({ ...formData, productType: e.target.value as ProductType })}
          className="h-11 w-full rounded-lg border border-stroke bg-white px-4 py-2.5 text-sm shadow-theme-xs focus:outline-hidden focus:ring-3 dark:bg-gray-900 dark:text-white/90"
        >
          <option value="Subscription">Subscription</option>
          <option value="AddOnSubscription">Add-On (Subscription)</option>
          <option value="AddOnOneTime">Add-On (One-Time)</option>
          <option value="CustomDevelopment">Custom Development</option>
        </select>
      </div>

      <div>
        <Label htmlFor="description">Description</Label>
        <textarea
          id="description"
          value={formData.description || ""}
          onChange={(e) => setFormData({ ...formData, description: e.target.value })}
          className="h-24 w-full rounded-lg border border-stroke bg-white px-4 py-2.5 text-sm shadow-theme-xs focus:outline-hidden focus:ring-3 dark:bg-gray-900 dark:text-white/90"
          placeholder="Product description"
        />
      </div>

      <div>
        <Label htmlFor="categoryId">Category</Label>
        <select
          id="categoryId"
          value={formData.categoryId || ""}
          onChange={(e) => setFormData({ ...formData, categoryId: e.target.value || undefined })}
          className="h-11 w-full rounded-lg border border-stroke bg-white px-4 py-2.5 text-sm shadow-theme-xs focus:outline-hidden focus:ring-3 dark:bg-gray-900 dark:text-white/90"
        >
          <option value="">Uncategorized</option>
          {categories.map((cat) => (
            <option key={cat.categoryId} value={cat.categoryId}>
              {cat.categoryName}
            </option>
          ))}
        </select>
      </div>

      {formData.productType === "Subscription" && (
        <>
          <div>
            <Label htmlFor="basePricePerUserPerMonth" required>
              Base Price Per User Per Month
            </Label>
            <Input
              id="basePricePerUserPerMonth"
              type="number"
              required
              min="0"
              step="0.01"
              value={formData.basePricePerUserPerMonth || ""}
              onChange={(e) =>
                setFormData({
                  ...formData,
                  basePricePerUserPerMonth: parseFloat(e.target.value) || undefined,
                })
              }
              placeholder="10.00"
            />
          </div>
          <BillingCycleSelector
            multipliers={formData.billingCycleMultipliers || {}}
            onChange={(multipliers) => setFormData({ ...formData, billingCycleMultipliers: multipliers })}
          />
        </>
      )}

      {(formData.productType === "AddOnSubscription" || formData.productType === "AddOnOneTime") && (
        <div className="space-y-4">
          <div>
            <Label htmlFor="addOnPricingType" required>
              Pricing Type
            </Label>
            <select
              id="addOnPricingType"
              required
              value={formData.addOnPricing?.pricingType || "subscription"}
              onChange={(e) =>
                setFormData({
                  ...formData,
                  addOnPricing: {
                    ...formData.addOnPricing,
                    pricingType: e.target.value as "subscription" | "oneTime",
                    monthlyPrice: e.target.value === "subscription" ? formData.addOnPricing?.monthlyPrice : undefined,
                    fixedPrice: e.target.value === "oneTime" ? formData.addOnPricing?.fixedPrice : undefined,
                  },
                })
              }
              className="h-11 w-full rounded-lg border border-stroke bg-white px-4 py-2.5 text-sm shadow-theme-xs focus:outline-hidden focus:ring-3 dark:bg-gray-900 dark:text-white/90"
            >
              <option value="subscription">Subscription (Monthly)</option>
              <option value="oneTime">One-Time</option>
            </select>
          </div>
          {formData.addOnPricing?.pricingType === "subscription" ? (
            <div>
              <Label htmlFor="monthlyPrice" required>
                Monthly Price
              </Label>
              <Input
                id="monthlyPrice"
                type="number"
                required
                min="0"
                step="0.01"
                value={formData.addOnPricing?.monthlyPrice || ""}
                onChange={(e) =>
                  setFormData({
                    ...formData,
                    addOnPricing: {
                      ...formData.addOnPricing,
                      pricingType: "subscription",
                      monthlyPrice: parseFloat(e.target.value) || undefined,
                    },
                  })
                }
                placeholder="50.00"
              />
            </div>
          ) : (
            <div>
              <Label htmlFor="fixedPrice" required>
                Fixed Price
              </Label>
              <Input
                id="fixedPrice"
                type="number"
                required
                min="0"
                step="0.01"
                value={formData.addOnPricing?.fixedPrice || ""}
                onChange={(e) =>
                  setFormData({
                    ...formData,
                    addOnPricing: {
                      ...formData.addOnPricing,
                      pricingType: "oneTime",
                      fixedPrice: parseFloat(e.target.value) || undefined,
                    },
                  })
                }
                placeholder="500.00"
              />
            </div>
          )}
        </div>
      )}

      {formData.productType === "CustomDevelopment" && (
        <div className="space-y-4">
          <div>
            <Label htmlFor="pricingModel" required>
              Pricing Model
            </Label>
            <select
              id="pricingModel"
              required
              value={formData.customDevelopmentPricing?.pricingModel || "hourly"}
              onChange={(e) =>
                setFormData({
                  ...formData,
                  customDevelopmentPricing: {
                    ...formData.customDevelopmentPricing,
                    pricingModel: e.target.value as "hourly" | "fixed" | "projectBased",
                  },
                })
              }
              className="h-11 w-full rounded-lg border border-stroke bg-white px-4 py-2.5 text-sm shadow-theme-xs focus:outline-hidden focus:ring-3 dark:bg-gray-900 dark:text-white/90"
            >
              <option value="hourly">Hourly Rate</option>
              <option value="fixed">Fixed Price</option>
              <option value="projectBased">Project-Based</option>
            </select>
          </div>
          {formData.customDevelopmentPricing?.pricingModel === "hourly" && (
            <div>
              <Label htmlFor="hourlyRate" required>
                Hourly Rate
              </Label>
              <Input
                id="hourlyRate"
                type="number"
                required
                min="0"
                step="0.01"
                value={formData.customDevelopmentPricing?.hourlyRate || ""}
                onChange={(e) =>
                  setFormData({
                    ...formData,
                    customDevelopmentPricing: {
                      ...formData.customDevelopmentPricing,
                      pricingModel: "hourly",
                      hourlyRate: parseFloat(e.target.value) || undefined,
                    },
                  })
                }
                placeholder="100.00"
              />
            </div>
          )}
          {formData.customDevelopmentPricing?.pricingModel === "fixed" && (
            <div>
              <Label htmlFor="fixedPrice" required>
                Fixed Price
              </Label>
              <Input
                id="fixedPrice"
                type="number"
                required
                min="0"
                step="0.01"
                value={formData.customDevelopmentPricing?.fixedPrice || ""}
                onChange={(e) =>
                  setFormData({
                    ...formData,
                    customDevelopmentPricing: {
                      ...formData.customDevelopmentPricing,
                      pricingModel: "fixed",
                      fixedPrice: parseFloat(e.target.value) || undefined,
                    },
                  })
                }
                placeholder="5000.00"
              />
            </div>
          )}
          {formData.customDevelopmentPricing?.pricingModel === "projectBased" && (
            <>
              <div>
                <Label htmlFor="baseProjectPrice" required>
                  Base Project Price
                </Label>
                <Input
                  id="baseProjectPrice"
                  type="number"
                  required
                  min="0"
                  step="0.01"
                  value={formData.customDevelopmentPricing?.baseProjectPrice || ""}
                  onChange={(e) =>
                    setFormData({
                      ...formData,
                      customDevelopmentPricing: {
                        ...formData.customDevelopmentPricing,
                        pricingModel: "projectBased",
                        baseProjectPrice: parseFloat(e.target.value) || undefined,
                      },
                    })
                  }
                  placeholder="20000.00"
                />
              </div>
              <div>
                <Label htmlFor="estimatedHours">Estimated Hours</Label>
                <Input
                  id="estimatedHours"
                  type="number"
                  min="0"
                  step="0.01"
                  value={formData.customDevelopmentPricing?.estimatedHours || ""}
                  onChange={(e) =>
                    setFormData({
                      ...formData,
                      customDevelopmentPricing: {
                        ...formData.customDevelopmentPricing,
                        pricingModel: "projectBased",
                        estimatedHours: parseFloat(e.target.value) || undefined,
                      },
                    })
                  }
                  placeholder="200"
                />
              </div>
            </>
          )}
        </div>
      )}

      <div>
        <Label htmlFor="currency" required>
          Currency
        </Label>
        <Input
          id="currency"
          type="text"
          required
          maxLength={3}
          value={formData.currency}
          onChange={(e) => setFormData({ ...formData, currency: e.target.value.toUpperCase() })}
          placeholder="USD"
        />
      </div>

      <div className="flex items-center gap-2">
        <input
          type="checkbox"
          id="isActive"
          checked={formData.isActive}
          onChange={(e) => setFormData({ ...formData, isActive: e.target.checked })}
          className="h-4 w-4 rounded border-gray-300 text-primary focus:ring-primary"
        />
        <Label htmlFor="isActive">Active</Label>
      </div>

      <div className="flex gap-4">
        <button
          type="submit"
          disabled={loading}
          className="inline-flex items-center justify-center gap-2 rounded-lg bg-brand-500 px-5 py-3.5 text-sm font-medium text-white shadow-theme-xs transition hover:bg-brand-600 disabled:cursor-not-allowed disabled:opacity-50"
        >
          {loading ? "Saving..." : productId ? "Update Product" : "Create Product"}
        </button>
        <Button variant="outline" onClick={() => router.back()}>
          Cancel
        </Button>
      </div>
    </form>
  );
}

