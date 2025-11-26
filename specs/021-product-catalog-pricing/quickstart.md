# Quickstart: Spec-021 Product Catalog & Pricing Management

**Spec**: Spec-021  
**Last Updated**: 2025-01-27

## Prerequisites

- Backend solution built from `main` with Specs 001-020 applied (especially Spec-009, Spec-012, Spec-017, Spec-020).
- PostgreSQL database running and accessible.
- Frontend Next.js app set up with TailAdmin template.
- JWT authentication working (Spec-003).
- Quotation CRUD operations working (Spec-009).
- Multi-currency support working (Spec-017).
- Tax management working (Spec-020).

## Backend Setup

### 1. Database Migration

Run the migration to create `Products`, `ProductCategories`, and `ProductPriceHistory` tables, and extend `QuotationLineItems`:

```bash
cd src/Backend/CRM.Infrastructure
dotnet ef migrations add AddProductCatalogTables --startup-project ../CRM.Api
dotnet ef database update --startup-project ../CRM.Api
```

**Verify**:
```sql
SELECT table_name FROM information_schema.tables 
WHERE table_schema = 'public' 
AND table_name IN ('Products', 'ProductCategories', 'ProductPriceHistory');

-- Verify QuotationLineItems extension
SELECT column_name FROM information_schema.columns 
WHERE table_name = 'QuotationLineItems' 
AND column_name IN ('ProductId', 'BillingCycle', 'Hours', 'OriginalProductPrice', 'DiscountAmount', 'TaxCategoryId');
```

### 2. Configuration

Add product catalog settings to `appsettings.json`:

```json
{
  "ProductCatalog": {
    "DefaultBillingCycleMultipliers": {
      "quarterly": 0.95,
      "halfYearly": 0.90,
      "yearly": 0.85,
      "multiYear": 0.80
    },
    "PriceCalculationCacheDurationMinutes": 5,
    "MaxCategoryNestingDepth": 3
  }
}
```

### 3. Application Wiring

Ensure in `Program.cs`:
- MediatR registered
- AutoMapper profiles registered (including `ProductProfile`)
- FluentValidation validators registered
- Authorization policies configured (Admin role for product management)
- IMemoryCache registered (for price calculation caching)

### 4. Seed Data (Optional)

Create test products and categories via `DbSeeder` or API:

```csharp
// Example: Create product category
POST /api/v1/product-categories
{
  "categoryName": "Cloud Services",
  "categoryCode": "CLOUD_SERVICES",
  "description": "Cloud-based services and subscriptions"
}

// Example: Create subscription product
POST /api/v1/products
{
  "productName": "Cloud Storage - 1TB per user/month",
  "productType": "Subscription",
  "description": "Monthly cloud storage subscription",
  "categoryId": "...",
  "basePricePerUserPerMonth": 10.00,
  "billingCycleMultipliers": {
    "quarterly": 0.95,
    "halfYearly": 0.90,
    "yearly": 0.85,
    "multiYear": 0.80
  },
  "currency": "USD",
  "isActive": true
}

// Example: Create add-on service (one-time)
POST /api/v1/products
{
  "productName": "Migration Service",
  "productType": "AddOnOneTime",
  "description": "One-time data migration service",
  "categoryId": "...",
  "addOnPricing": {
    "pricingType": "oneTime",
    "fixedPrice": 500.00
  },
  "currency": "USD",
  "isActive": true
}

// Example: Create custom development charge (hourly)
POST /api/v1/products
{
  "productName": "Custom API Development",
  "productType": "CustomDevelopment",
  "description": "Custom API development work",
  "categoryId": "...",
  "customDevelopmentPricing": {
    "pricingModel": "hourly",
    "hourlyRate": 100.00
  },
  "currency": "USD",
  "isActive": true
}
```

## Frontend Setup

### 1. Install Dependencies

No additional dependencies required (uses existing React Query, React Hook Form, Tailwind CSS).

### 2. API Client Integration

Add product API methods to `src/Frontend/web/src/lib/api.ts`:

```typescript
export const ProductsApi = {
  list: async (params?: ProductListParams) => { /* ... */ },
  getById: async (productId: string) => { /* ... */ },
  create: async (request: CreateProductRequest) => { /* ... */ },
  update: async (productId: string, request: UpdateProductRequest) => { /* ... */ },
  delete: async (productId: string) => { /* ... */ },
  getCatalog: async (params?: ProductCatalogParams) => { /* ... */ },
  calculatePrice: async (request: CalculateProductPriceRequest) => { /* ... */ },
  getUsageStats: async (productId: string) => { /* ... */ }
};

export const ProductCategoriesApi = {
  list: async (params?: ProductCategoryListParams) => { /* ... */ },
  getById: async (categoryId: string) => { /* ... */ },
  create: async (request: CreateProductCategoryRequest) => { /* ... */ },
  update: async (categoryId: string, request: UpdateProductCategoryRequest) => { /* ... */ }
};
```

### 3. TypeScript Types

Add product types to `src/Frontend/web/src/types/products.ts`:

```typescript
export type ProductType = 'Subscription' | 'AddOnSubscription' | 'AddOnOneTime' | 'CustomDevelopment';
export type BillingCycle = 'Monthly' | 'Quarterly' | 'HalfYearly' | 'Yearly' | 'MultiYear';

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

// ... additional types
```

### 4. Create Pages

Create product management pages:

```bash
# Product catalog list
src/Frontend/web/src/app/(protected)/products/catalog/page.tsx

# Create product
src/Frontend/web/src/app/(protected)/products/catalog/new/page.tsx

# Product details/edit
src/Frontend/web/src/app/(protected)/products/catalog/[productId]/page.tsx

# Product categories
src/Frontend/web/src/app/(protected)/products/categories/page.tsx
```

### 5. Create Components

Create product management components:

```bash
src/Frontend/web/src/components/products/
├── ProductCatalogTable.tsx
├── ProductForm.tsx
├── ProductSelector.tsx
├── ProductPricingCalculator.tsx
├── BillingCycleSelector.tsx
├── ProductCategoryTree.tsx
└── ProductUsageStats.tsx
```

### 6. Integrate with Quotations

Update quotation creation/edit forms to include product selector:

```typescript
// In QuotationCreateForm or QuotationEditForm
import { ProductSelector } from '@/components/products/ProductSelector';

// Add product selector component
<ProductSelector
  onProductSelected={(product, quantity, billingCycle, hours) => {
    // Add product to quotation line items
  }}
/>
```

## Testing

### Backend API Testing

Test product CRUD operations:

```bash
# List products
GET /api/v1/products?productType=Subscription&isActive=true

# Create product
POST /api/v1/products
# (with request body as shown in seed data)

# Get product details
GET /api/v1/products/{productId}

# Calculate product price
POST /api/v1/products/calculate-price
{
  "productId": "...",
  "quantity": 10,
  "billingCycle": "Yearly"
}

# Add product to quotation
PUT /api/v1/quotations/{quotationId}/line-items/product
{
  "productId": "...",
  "quantity": 10,
  "billingCycle": "Yearly"
}
```

### Frontend Testing

Test product management UI:

1. **Admin Login**: Log in as admin user
2. **Navigate to Product Catalog**: Go to `/products/catalog`
3. **Create Product**: Click "Add Product", fill form, submit
4. **View Product**: Click on product to view details
5. **Edit Product**: Click "Edit", modify, save
6. **Create Quotation with Product**: 
   - Go to quotation creation
   - Click "Add Product from Catalog"
   - Select product, set quantity/billing cycle
   - Verify price calculation
   - Add to quotation
   - Verify line item appears with correct pricing

### Integration Testing

Test complete workflow:

1. **Admin creates product** → Product appears in catalog
2. **Sales rep creates quotation** → Can select product from catalog
3. **Add product to quotation** → Line item created with correct pricing
4. **Change quantity/billing cycle** → Price recalculates automatically
5. **Apply discount** → Discount applied correctly
6. **Calculate tax** → Tax calculated based on product category and client location
7. **View quotation totals** → All totals (subtotal, discount, tax, total) correct

## Common Issues & Solutions

### Issue: Product price calculation incorrect

**Solution**: 
- Verify billing cycle multipliers are configured correctly
- Check product type matches pricing configuration (Subscription vs AddOn vs CustomDevelopment)
- Verify quantity and billing cycle are set correctly

### Issue: Product not appearing in catalog selector

**Solution**:
- Check product `IsActive` status (must be true)
- Verify product type filter matches product type
- Check currency filter matches product currency

### Issue: Cannot delete product

**Solution**:
- Product is in use by quotations (check `QuotationLineItems` table)
- Use soft delete (set `IsActive = false`) instead
- Or remove product from all quotations first

### Issue: Tax calculation not working for products

**Solution**:
- Verify product has `CategoryId` set
- Check product category maps to tax category (Spec-020)
- Verify client location is set correctly
- Check tax rules are configured for product category and client location

## Next Steps

1. **Create Product Categories**: Set up product categories aligned with tax categories (Spec-020)
2. **Create Products**: Add subscription products, add-ons, and custom development charges
3. **Test Quotation Integration**: Create quotations using products from catalog
4. **Verify Calculations**: Test price calculations, discounts, and taxes
5. **Train Users**: Train admins on product management and sales reps on product selection

## Related Documentation

- [Spec-021 Specification](./spec.md)
- [Data Model](./data-model.md)
- [API Contracts](./contracts/products.openapi.yaml)
- [Implementation Plan](./plan.md)
- [Research & Technical Decisions](./research.md)

---

**Status**: ✅ Quickstart guide complete. Ready for implementation.

