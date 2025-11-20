"use client";
import { useState, useEffect } from "react";
import { ProductsApi } from "@/lib/api";
import type { ProductCatalogItem, ProductType, BillingCycle } from "@/types/products";
import Input from "@/components/tailadmin/form/input/InputField";
import Label from "@/components/tailadmin/form/Label";
import Button from "@/components/tailadmin/ui/button/Button";

interface ProductSelectorProps {
  onProductSelected: (product: ProductCatalogItem, quantity: number, billingCycle?: BillingCycle, hours?: number) => void;
  disabled?: boolean;
}

export default function ProductSelector({ onProductSelected, disabled = false }: ProductSelectorProps) {
  const [loading, setLoading] = useState(false);
  const [search, setSearch] = useState("");
  const [productTypeFilter, setProductTypeFilter] = useState<ProductType | "">("");
  const [products, setProducts] = useState<ProductCatalogItem[]>([]);
  const [selectedProduct, setSelectedProduct] = useState<ProductCatalogItem | null>(null);
  const [quantity, setQuantity] = useState(1);
  const [billingCycle, setBillingCycle] = useState<BillingCycle | "">("");
  const [hours, setHours] = useState<number | "">("");

  useEffect(() => {
    if (search.length >= 2 || productTypeFilter) {
      loadProducts();
    } else {
      setProducts([]);
    }
  }, [search, productTypeFilter]);

  async function loadProducts() {
    setLoading(true);
    try {
      const res = await ProductsApi.getCatalog({
        search: search || undefined,
        productType: productTypeFilter || undefined,
        pageSize: 20,
      });
      setProducts(res.data || []);
    } catch (e: any) {
      console.error("Failed to load products", e);
      setProducts([]);
    } finally {
      setLoading(false);
    }
  }

  const handleSelectProduct = (product: ProductCatalogItem) => {
    setSelectedProduct(product);
    setQuantity(1);
    setBillingCycle("");
    setHours("");
  };

  const handleAdd = () => {
    if (!selectedProduct) return;

    const billingCycleValue = billingCycle ? (billingCycle as BillingCycle) : undefined;
    const hoursValue = hours !== "" ? Number(hours) : undefined;

    onProductSelected(selectedProduct, quantity, billingCycleValue, hoursValue);
    
    // Reset form
    setSelectedProduct(null);
    setSearch("");
    setQuantity(1);
    setBillingCycle("");
    setHours("");
    setProducts([]);
  };

  return (
    <div className="space-y-4">
      <div>
        <Label htmlFor="productSearch">Search Products</Label>
        <div className="flex gap-2">
          <Input
            id="productSearch"
            type="text"
            placeholder="Type to search products..."
            value={search}
            onChange={(e) => setSearch(e.target.value)}
            disabled={disabled}
            className="flex-1"
          />
          <select
            value={productTypeFilter}
            onChange={(e) => setProductTypeFilter(e.target.value as ProductType | "")}
            disabled={disabled}
            className="rounded border border-stroke bg-white px-3 py-2 text-black focus:border-primary focus-visible:outline-none dark:border-form-strokedark dark:bg-form-input dark:text-white"
          >
            <option value="">All Types</option>
            <option value="Subscription">Subscription</option>
            <option value="AddOnSubscription">Add-On (Subscription)</option>
            <option value="AddOnOneTime">Add-On (One-Time)</option>
            <option value="CustomDevelopment">Custom Development</option>
          </select>
        </div>
      </div>

      {loading && <div className="text-sm text-gray-500">Loading products...</div>}

      {products.length > 0 && !selectedProduct && (
        <div className="max-h-60 overflow-y-auto rounded-lg border border-stroke dark:border-form-strokedark">
          {products.map((product) => (
            <button
              key={product.productId}
              type="button"
              onClick={() => handleSelectProduct(product)}
              disabled={disabled}
              className="w-full border-b border-stroke px-4 py-3 text-left hover:bg-gray-50 dark:border-form-strokedark dark:hover:bg-gray-800"
            >
              <div className="font-medium">{product.productName}</div>
              <div className="text-sm text-gray-500">
                {product.productType} • {product.pricingSummary || product.currency}
              </div>
            </button>
          ))}
        </div>
      )}

      {selectedProduct && (
        <div className="rounded-lg border border-stroke bg-gray-50 p-4 dark:border-form-strokedark dark:bg-gray-800">
          <div className="mb-4">
            <div className="font-medium">{selectedProduct.productName}</div>
            <div className="text-sm text-gray-500">{selectedProduct.productType}</div>
            {selectedProduct.description && (
              <div className="mt-1 text-sm text-gray-600">{selectedProduct.description}</div>
            )}
          </div>

          <div className="space-y-3">
            <div>
              <Label htmlFor="quantity" required>
                Quantity
              </Label>
              <Input
                id="quantity"
                type="number"
                required
                min="1"
                value={quantity}
                onChange={(e) => setQuantity(parseInt(e.target.value) || 1)}
                disabled={disabled}
              />
            </div>

            {selectedProduct.productType === "Subscription" && (
              <div>
                <Label htmlFor="billingCycle">Billing Cycle</Label>
                <select
                  id="billingCycle"
                  value={billingCycle}
                  onChange={(e) => setBillingCycle(e.target.value as BillingCycle | "")}
                  disabled={disabled}
                  className="h-11 w-full rounded-lg border border-stroke bg-white px-4 py-2.5 text-sm shadow-theme-xs focus:outline-hidden focus:ring-3 dark:bg-gray-900 dark:text-white/90"
                >
                  <option value="">Monthly</option>
                  <option value="Quarterly">Quarterly</option>
                  <option value="HalfYearly">Half-Yearly</option>
                  <option value="Yearly">Yearly</option>
                  <option value="MultiYear">Multi-Year</option>
                </select>
              </div>
            )}

            {selectedProduct.productType === "CustomDevelopment" && (
              <div>
                <Label htmlFor="hours">Hours</Label>
                <Input
                  id="hours"
                  type="number"
                  min="0"
                  step="0.5"
                  value={hours}
                  onChange={(e) => setHours(e.target.value ? parseFloat(e.target.value) : "")}
                  disabled={disabled}
                  placeholder="Enter hours"
                />
              </div>
            )}

            <div className="flex gap-2">
              <Button onClick={handleAdd} disabled={disabled || quantity <= 0}>
                Add to Quotation
              </Button>
              <Button
                variant="outline"
                onClick={() => {
                  setSelectedProduct(null);
                  setQuantity(1);
                  setBillingCycle("");
                  setHours("");
                }}
                disabled={disabled}
              >
                Cancel
              </Button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}

