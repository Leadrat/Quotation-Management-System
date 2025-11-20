# Quickstart: Spec-020 Multi-Country & Jurisdiction Tax Management

This guide shows how to configure countries, jurisdictions, tax frameworks, and tax rates for multi-country tax management. It covers setting up India (GST) and UAE (VAT) as initial examples.

## Prerequisites

- .NET 8 SDK
- PostgreSQL database
- Database with existing Users, Clients, and Quotations tables (Spec-001, Spec-002, Spec-009 complete)
- Environment variables:
  - POSTGRES_CONNECTION
  - JWT__SECRET (≥32 chars)

## Run API

```powershell
$env:POSTGRES_CONNECTION = "Host=...;Port=...;Database=...;Username=...;Password=...;SSL Mode=Require;Trust Server Certificate=true"
$env:JWT__SECRET = "change-me-32+chars-secret"
dotnet run --project src/Backend/CRM.Api/CRM.Api.csproj -c Release
```

## Step 1: Login as Admin

```bash
curl -X POST "http://localhost:5000/api/v1/auth/login" \
  -H "Content-Type: application/json" \
  -d '{
    "email": "admin@crm.com",
    "password": "Admin@123"
  }'
```

Save the `accessToken` from the response for subsequent requests.

## Step 2: Create Countries

### Create India (GST framework)

```bash
curl -X POST "http://localhost:5000/api/v1/admin/tax/countries" \
  -H "Authorization: Bearer {accessToken}" \
  -H "Content-Type: application/json" \
  -d '{
    "countryName": "India",
    "countryCode": "IN",
    "taxFrameworkType": "GST",
    "defaultCurrency": "INR",
    "isActive": true,
    "isDefault": true
  }'
```

Response:
```json
{
  "countryId": "A1B2C3D4-E5F6-47G8-H9I0-J1K2L3M4N5O6",
  "countryName": "India",
  "countryCode": "IN",
  "taxFrameworkType": "GST",
  "defaultCurrency": "INR",
  "isActive": true,
  "isDefault": true,
  "createdAt": "2025-01-27T10:00:00Z",
  "updatedAt": "2025-01-27T10:00:00Z"
}
```

### Create UAE (VAT framework)

```bash
curl -X POST "http://localhost:5000/api/v1/admin/tax/countries" \
  -H "Authorization: Bearer {accessToken}" \
  -H "Content-Type: application/json" \
  -d '{
    "countryName": "United Arab Emirates",
    "countryCode": "AE",
    "taxFrameworkType": "VAT",
    "defaultCurrency": "AED",
    "isActive": true,
    "isDefault": false
  }'
```

Save the `countryId` values for India and UAE.

## Step 3: Create Tax Frameworks

### Create GST Framework for India

```bash
curl -X POST "http://localhost:5000/api/v1/admin/tax/frameworks" \
  -H "Authorization: Bearer {accessToken}" \
  -H "Content-Type: application/json" \
  -d '{
    "countryId": "{indiaCountryId}",
    "frameworkName": "Goods and Services Tax",
    "frameworkType": "GST",
    "description": "GST framework for India with CGST, SGST, and IGST components",
    "taxComponents": [
      {
        "name": "CGST",
        "code": "CGST",
        "isCentrallyGoverned": false,
        "description": "Central Goods and Services Tax"
      },
      {
        "name": "SGST",
        "code": "SGST",
        "isCentrallyGoverned": false,
        "description": "State Goods and Services Tax"
      },
      {
        "name": "IGST",
        "code": "IGST",
        "isCentrallyGoverned": true,
        "description": "Integrated Goods and Services Tax"
      }
    ]
  }'
```

### Create VAT Framework for UAE

```bash
curl -X POST "http://localhost:5000/api/v1/admin/tax/frameworks" \
  -H "Authorization: Bearer {accessToken}" \
  -H "Content-Type: application/json" \
  -d '{
    "countryId": "{uaeCountryId}",
    "frameworkName": "Value Added Tax",
    "frameworkType": "VAT",
    "description": "VAT framework for UAE",
    "taxComponents": [
      {
        "name": "VAT",
        "code": "VAT",
        "isCentrallyGoverned": true,
        "description": "Value Added Tax"
      }
    ]
  }'
```

Save the `taxFrameworkId` values.

## Step 4: Create Jurisdictions

### Create Maharashtra State (India)

```bash
curl -X POST "http://localhost:5000/api/v1/admin/tax/jurisdictions" \
  -H "Authorization: Bearer {accessToken}" \
  -H "Content-Type: application/json" \
  -d '{
    "countryId": "{indiaCountryId}",
    "parentJurisdictionId": null,
    "jurisdictionName": "Maharashtra",
    "jurisdictionCode": "27",
    "jurisdictionType": "State",
    "isActive": true
  }'
```

### Create Dubai Emirate (UAE)

```bash
curl -X POST "http://localhost:5000/api/v1/admin/tax/jurisdictions" \
  -H "Authorization: Bearer {accessToken}" \
  -H "Content-Type: application/json" \
  -d '{
    "countryId": "{uaeCountryId}",
    "parentJurisdictionId": null,
    "jurisdictionName": "Dubai",
    "jurisdictionCode": "DXB",
    "jurisdictionType": "Emirate",
    "isActive": true
  }'
```

Save the `jurisdictionId` values.

## Step 5: Create Product/Service Categories

### Create Services Category

```bash
curl -X POST "http://localhost:5000/api/v1/admin/tax/categories" \
  -H "Authorization: Bearer {accessToken}" \
  -H "Content-Type: application/json" \
  -d '{
    "categoryName": "Services",
    "categoryCode": "SRV",
    "description": "Professional services",
    "isActive": true
  }'
```

### Create Products Category

```bash
curl -X POST "http://localhost:5000/api/v1/admin/tax/categories" \
  -H "Authorization: Bearer {accessToken}" \
  -H "Content-Type: application/json" \
  -d '{
    "categoryName": "Products",
    "categoryCode": "PROD",
    "description": "Physical products",
    "isActive": true
  }'
```

Save the `categoryId` values.

## Step 6: Create Tax Rates

### Create GST Rate for Maharashtra (Intra-State: CGST + SGST)

```bash
curl -X POST "http://localhost:5000/api/v1/admin/tax/rates" \
  -H "Authorization: Bearer {accessToken}" \
  -H "Content-Type: application/json" \
  -d '{
    "jurisdictionId": "{maharashtraJurisdictionId}",
    "taxFrameworkId": "{gstFrameworkId}",
    "productServiceCategoryId": null,
    "taxRate": 18.00,
    "effectiveFrom": "2025-01-01",
    "effectiveTo": null,
    "isExempt": false,
    "isZeroRated": false,
    "taxComponents": [
      {
        "component": "CGST",
        "rate": 9.0
      },
      {
        "component": "SGST",
        "rate": 9.0
      }
    ],
    "description": "Standard GST rate for Maharashtra (intra-state)"
  }'
```

### Create GST Rate for Inter-State (IGST)

```bash
curl -X POST "http://localhost:5000/api/v1/admin/tax/rates" \
  -H "Authorization: Bearer {accessToken}" \
  -H "Content-Type: application/json" \
  -d '{
    "jurisdictionId": null,
    "taxFrameworkId": "{gstFrameworkId}",
    "productServiceCategoryId": null,
    "taxRate": 18.00,
    "effectiveFrom": "2025-01-01",
    "effectiveTo": null,
    "isExempt": false,
    "isZeroRated": false,
    "taxComponents": [
      {
        "component": "IGST",
        "rate": 18.0
      }
    ],
    "description": "Standard GST rate for inter-state transactions"
  }'
```

### Create VAT Rate for Dubai

```bash
curl -X POST "http://localhost:5000/api/v1/admin/tax/rates" \
  -H "Authorization: Bearer {accessToken}" \
  -H "Content-Type: application/json" \
  -d '{
    "jurisdictionId": "{dubaiJurisdictionId}",
    "taxFrameworkId": "{vatFrameworkId}",
    "productServiceCategoryId": null,
    "taxRate": 5.00,
    "effectiveFrom": "2025-01-01",
    "effectiveTo": null,
    "isExempt": false,
    "isZeroRated": false,
    "taxComponents": [
      {
        "component": "VAT",
        "rate": 5.0
      }
    ],
    "description": "Standard VAT rate for Dubai"
  }'
```

## Step 7: Preview Tax Calculation

### Preview GST Calculation (Maharashtra)

```bash
curl -X POST "http://localhost:5000/api/v1/admin/tax/preview-calculation" \
  -H "Authorization: Bearer {accessToken}" \
  -H "Content-Type: application/json" \
  -d '{
    "countryId": "{indiaCountryId}",
    "jurisdictionId": "{maharashtraJurisdictionId}",
    "calculationDate": "2025-01-27",
    "lineItems": [
      {
        "productServiceCategoryId": "{servicesCategoryId}",
        "amount": 50000.00
      },
      {
        "productServiceCategoryId": "{productsCategoryId}",
        "amount": 30000.00
      }
    ]
  }'
```

Response:
```json
{
  "success": true,
  "countryId": "...",
  "jurisdictionId": "...",
  "taxFrameworkId": "...",
  "subtotal": 80000.00,
  "discountAmount": 0.00,
  "taxableAmount": 80000.00,
  "taxBreakdown": [
    {
      "component": "CGST",
      "rate": 9.0,
      "amount": 7200.00
    },
    {
      "component": "SGST",
      "rate": 9.0,
      "amount": 7200.00
    }
  ],
  "totalTax": 14400.00,
  "totalAmount": 94400.00
}
```

### Preview VAT Calculation (Dubai)

```bash
curl -X POST "http://localhost:5000/api/v1/admin/tax/preview-calculation" \
  -H "Authorization: Bearer {accessToken}" \
  -H "Content-Type: application/json" \
  -d '{
    "countryId": "{uaeCountryId}",
    "jurisdictionId": "{dubaiJurisdictionId}",
    "calculationDate": "2025-01-27",
    "lineItems": [
      {
        "productServiceCategoryId": "{servicesCategoryId}",
        "amount": 100000.00
      }
    ]
  }'
```

Response:
```json
{
  "success": true,
  "countryId": "...",
  "jurisdictionId": "...",
  "taxFrameworkId": "...",
  "subtotal": 100000.00,
  "discountAmount": 0.00,
  "taxableAmount": 100000.00,
  "taxBreakdown": [
    {
      "component": "VAT",
      "rate": 5.0,
      "amount": 5000.00
    }
  ],
  "totalTax": 5000.00,
  "totalAmount": 105000.00
}
```

## Step 8: View Tax Audit Log

```bash
curl -X GET "http://localhost:5000/api/v1/admin/tax/audit-log?pageNumber=1&pageSize=10" \
  -H "Authorization: Bearer {accessToken}"
```

## Step 9: Integration with Quotations

When creating or updating a quotation, the system will automatically:

1. Determine the client's country and jurisdiction from the `Client` entity
2. Look up applicable tax rates based on:
   - Client's jurisdiction
   - Line item categories
   - Effective dates
3. Calculate taxes using the tax framework
4. Store tax breakdown in the quotation
5. Log the calculation in the audit log

Example quotation creation (automatic tax calculation):

```bash
curl -X POST "http://localhost:5000/api/v1/quotations" \
  -H "Authorization: Bearer {accessToken}" \
  -H "Content-Type: application/json" \
  -d '{
    "clientId": "{clientId}",
    "quotationDate": "2025-01-27",
    "validUntil": "2025-02-27",
    "discountPercentage": 10.0,
    "lineItems": [
      {
        "sequenceNumber": 1,
        "itemName": "Cloud Storage Service",
        "description": "Monthly cloud storage subscription",
        "quantity": 10.00,
        "unitRate": 5000.00,
        "productServiceCategoryId": "{servicesCategoryId}"
      },
      {
        "sequenceNumber": 2,
        "itemName": "Server Hardware",
        "description": "Physical server equipment",
        "quantity": 2.00,
        "unitRate": 15000.00,
        "productServiceCategoryId": "{productsCategoryId}"
      }
    ]
  }'
```

The response will include automatically calculated tax fields:
- `taxAmount`: Total tax
- `cgstAmount`, `sgstAmount`, `igstAmount`: Component-wise taxes (for GST)
- `taxCountryId`, `taxJurisdictionId`, `taxFrameworkId`: Tax context
- `taxBreakdown`: JSONB with detailed breakdown

## Common Tasks

### List All Countries

```bash
curl -X GET "http://localhost:5000/api/v1/admin/tax/countries?isActive=true" \
  -H "Authorization: Bearer {accessToken}"
```

### List Jurisdictions for a Country

```bash
curl -X GET "http://localhost:5000/api/v1/admin/tax/countries/{countryId}/jurisdictions" \
  -H "Authorization: Bearer {accessToken}"
```

### List Tax Rates with Filters

```bash
curl -X GET "http://localhost:5000/api/v1/admin/tax/rates?jurisdictionId={jurisdictionId}&effectiveDate=2025-01-27" \
  -H "Authorization: Bearer {accessToken}"
```

### Update Tax Rate

```bash
curl -X PUT "http://localhost:5000/api/v1/admin/tax/rates/{rateId}" \
  -H "Authorization: Bearer {accessToken}" \
  -H "Content-Type: application/json" \
  -d '{
    "taxRate": 20.00,
    "effectiveFrom": "2025-02-01",
    "taxComponents": [
      {
        "component": "CGST",
        "rate": 10.0
      },
      {
        "component": "SGST",
        "rate": 10.0
      }
    ]
  }'
```

## Notes

- All tax management endpoints require Admin role (403 Forbidden for non-admins)
- Tax rates use effective dates - rates with `effectiveFrom <= current_date` and `effectiveTo IS NULL` or `>= current_date` are active
- Jurisdiction hierarchy: Country → Jurisdiction → Sub-Jurisdiction (up to 3 levels)
- Tax calculation priority: Category+Jurisdiction → Jurisdiction → Country default
- Historical quotations retain original tax calculations (no recalculation)
- Tax calculations are logged in the audit log for compliance

## Troubleshooting

### 403 Forbidden
- Ensure you're logged in as an Admin user
- Check that the access token is valid and included in the Authorization header

### 400 Bad Request
- Verify country code is exactly 2 uppercase letters (ISO 3166-1 alpha-2)
- Verify currency code is exactly 3 uppercase letters (ISO 4217)
- Check tax rate is between 0 and 100
- Ensure effective dates are valid (effectiveFrom <= effectiveTo)

### 404 Not Found
- Verify country/jurisdiction/framework IDs exist
- Check that entities are not soft-deleted (`isActive = true`)

### Tax Calculation Returns Zero
- Verify tax rates are configured for the jurisdiction
- Check effective dates - rates must be active on the calculation date
- Ensure client has `CountryId` and `JurisdictionId` set

