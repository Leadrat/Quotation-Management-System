export type ProductType = 'Subscription' | 'AddOnSubscription' | 'AddOnOneTime' | 'CustomDevelopment';
export type BillingCycle = 'Monthly' | 'Quarterly' | 'HalfYearly' | 'Yearly' | 'MultiYear';

export interface BillingCycleMultipliers {
  quarterly?: number;
  halfYearly?: number;
  yearly?: number;
  multiYear?: number;
}

export interface AddOnPricing {
  pricingType: 'subscription' | 'oneTime';
  monthlyPrice?: number;
  fixedPrice?: number;
}

export interface CustomDevelopmentPricing {
  pricingModel: 'hourly' | 'fixed' | 'projectBased';
  hourlyRate?: number;
  fixedPrice?: number;
  baseProjectPrice?: number;
  estimatedHours?: number;
}

export interface Product {
  productId: string;
  productName: string;
  productType: ProductType;
  description?: string;
  categoryId?: string;
  categoryName?: string;
  basePricePerUserPerMonth?: number;
  billingCycleMultipliers?: BillingCycleMultipliers;
  addOnPricing?: AddOnPricing;
  customDevelopmentPricing?: CustomDevelopmentPricing;
  currency: string;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface CreateProductRequest {
  productName: string;
  productType: ProductType;
  description?: string;
  categoryId?: string;
  basePricePerUserPerMonth?: number;
  billingCycleMultipliers?: BillingCycleMultipliers;
  addOnPricing?: AddOnPricing;
  customDevelopmentPricing?: CustomDevelopmentPricing;
  currency?: string;
  isActive?: boolean;
}

export interface ProductCategory {
  categoryId: string;
  categoryName: string;
  categoryCode: string;
  description?: string;
  parentCategoryId?: string;
  parentCategoryName?: string;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface CreateProductCategoryRequest {
  categoryName: string;
  categoryCode: string;
  description?: string;
  parentCategoryId?: string;
  isActive?: boolean;
}

export interface ProductCatalogItem {
  productId: string;
  productName: string;
  productType: ProductType;
  description?: string;
  categoryId?: string;
  categoryName?: string;
  basePricePerUserPerMonth?: number;
  currency: string;
  pricingSummary?: string;
}

export interface ProductPriceCalculationRequest {
  productId: string;
  quantity: number;
  billingCycle?: BillingCycle;
  hours?: number;
  currency?: string;
}

export interface ProductPriceCalculationResponse {
  productId: string;
  productName: string;
  unitRate: number;
  quantity: number;
  billingCycle?: BillingCycle;
  hours?: number;
  subtotal: number;
  currency: string;
  calculationBreakdown?: string;
}

export interface ProductUsageStats {
  productId: string;
  productName: string;
  totalQuotationsUsedIn: number;
  totalRevenueGenerated: number;
  currency: string;
}

export interface PagedProductResult {
  success: boolean;
  data: Product[];
  pageNumber: number;
  pageSize: number;
  totalCount: number;
}

export interface PagedProductCatalogResult {
  success: boolean;
  data: ProductCatalogItem[];
  pageNumber: number;
  pageSize: number;
  totalCount: number;
}

