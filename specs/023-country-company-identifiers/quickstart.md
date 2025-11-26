# Quickstart: Country-Specific Company Identifiers & Bank Details (Spec-023)

**Date**: 2025-01-27  
**Spec**: [spec.md](./spec.md)  
**Plan**: [plan.md](./plan.md)  
**Data Model**: [data-model.md](./data-model.md)  
**API Contract**: [contracts/country-company-identifiers.openapi.yaml](./contracts/country-company-identifiers.openapi.yaml)

## Overview

This quickstart guide provides step-by-step instructions for setting up and using the country-specific company identifiers and bank details management feature.

## Prerequisites

- Admin user account with proper JWT token
- Access to the Company Details admin page
- Countries configured in the system (from Spec-020: Country Tax Management)

## Quick Setup

### 1. Master Configuration: Identifier Types

First, configure the identifier types that can be used across countries.

#### Create Identifier Types

**API Endpoint**: `POST /api/v1/identifier-types`

**Example Request**:
```json
{
  "name": "PAN",
  "displayName": "PAN Number",
  "description": "Permanent Account Number for India"
}
```

**Example Response**:
```json
{
  "success": true,
  "message": "Identifier type created successfully",
  "data": {
    "identifierTypeId": "a1b2c3d4-e5f6-4789-a0b1-c2d3e4f5a6b7",
    "name": "PAN",
    "displayName": "PAN Number",
    "description": "Permanent Account Number for India",
    "isActive": true,
    "createdAt": "2025-01-27T10:00:00Z",
    "updatedAt": "2025-01-27T10:00:00Z"
  }
}
```

**Common Identifier Types to Create**:
- `PAN` - Permanent Account Number (India)
- `TAN` - Tax Deduction and Collection Account Number (India)
- `GST` - Goods and Services Tax Identification Number (India)
- `VAT` - Value Added Tax Number (EU)
- `BUSINESS_LICENSE` - Trade License Number (Dubai/UAE)

### 2. Master Configuration: Country Identifier Configurations

Configure which identifier types are required/optional for each country.

#### Configure Identifier Type for Country

**API Endpoint**: `POST /api/v1/countries/{countryId}/identifier-configurations`

**Example Request** (India - PAN):
```json
{
  "identifierTypeId": "a1b2c3d4-e5f6-4789-a0b1-c2d3e4f5a6b7",
  "isRequired": true,
  "validationRegex": "^[A-Z]{5}[0-9]{4}[A-Z]{1}$",
  "minLength": 10,
  "maxLength": 10,
  "displayName": "PAN Number",
  "helpText": "10-character alphanumeric Permanent Account Number",
  "displayOrder": 1,
  "isActive": true
}
```

**Example Request** (EU - VAT):
```json
{
  "identifierTypeId": "b2c3d4e5-f6a7-4890-b1c2-d3e4f5a6b7c8",
  "isRequired": true,
  "validationRegex": "^[A-Z]{2}[0-9A-Z]{2,12}$",
  "minLength": 4,
  "maxLength": 14,
  "displayName": "VAT Number",
  "helpText": "Country code followed by alphanumeric VAT number",
  "displayOrder": 1,
  "isActive": true
}
```

### 3. Master Configuration: Bank Field Types

Configure the bank field types that can be used across countries.

#### Create Bank Field Types

**API Endpoint**: `POST /api/v1/bank-field-types`

**Example Request**:
```json
{
  "name": "IFSC",
  "displayName": "IFSC Code",
  "description": "Indian Financial System Code"
}
```

**Common Bank Field Types to Create**:
- `IFSC` - Indian Financial System Code (India)
- `IBAN` - International Bank Account Number (Dubai/UAE)
- `SWIFT` - SWIFT/BIC Code (Dubai/UAE)
- `ROUTING_NUMBER` - ABA Routing Number (US)
- `ACCOUNT_NUMBER` - Universal account number

### 4. Master Configuration: Country Bank Field Configurations

Configure which bank field types are required/optional for each country.

#### Configure Bank Field Type for Country

**API Endpoint**: `POST /api/v1/countries/{countryId}/bank-field-configurations`

**Example Request** (India - IFSC):
```json
{
  "bankFieldTypeId": "d4e5f6a7-b8c9-4901-d2e3-f4a5b6c7d8e9",
  "isRequired": true,
  "validationRegex": "^[A-Z]{4}0[0-9A-Z]{6}$",
  "minLength": 11,
  "maxLength": 11,
  "displayName": "IFSC Code",
  "helpText": "11-character alphanumeric Indian Financial System Code",
  "displayOrder": 1,
  "isActive": true
}
```

**Example Request** (Dubai - IBAN):
```json
{
  "bankFieldTypeId": "e5f6a7b8-c9d0-4901-e2f3-a4b5c6d7e8f9",
  "isRequired": true,
  "validationRegex": "^[A-Z]{2}[0-9]{2}[A-Z0-9]{4,30}$",
  "minLength": 16,
  "maxLength": 34,
  "displayName": "IBAN",
  "helpText": "International Bank Account Number",
  "displayOrder": 1,
  "isActive": true
}
```

### 5. Company Details: Enter Identifier Values

Enter company identifier values for a specific country.

#### Get Identifier Configurations for Country

**API Endpoint**: `GET /api/v1/countries/{countryId}/identifier-configurations`

**Example Response**:
```json
{
  "success": true,
  "data": [
    {
      "configurationId": "b2c3d4e5-f6a7-4890-b1c2-d3e4f5a6b7c8",
      "countryId": "c3d4e5f6-a7b8-4901-c2d3-e4f5a6b7c8d9",
      "identifierTypeId": "a1b2c3d4-e5f6-4789-a0b1-c2d3e4f5a6b7",
      "identifierTypeName": "PAN",
      "identifierTypeDisplayName": "PAN Number",
      "isRequired": true,
      "validationRegex": "^[A-Z]{5}[0-9]{4}[A-Z]{1}$",
      "minLength": 10,
      "maxLength": 10,
      "displayName": "PAN Number",
      "helpText": "10-character alphanumeric Permanent Account Number",
      "displayOrder": 1,
      "isActive": true
    }
  ]
}
```

#### Save Company Identifier Values

**API Endpoint**: `PUT /api/v1/company-details/{countryId}/identifiers`

**Example Request** (India):
```json
{
  "values": {
    "PAN": "ABCDE1234F",
    "TAN": "ABCD12345E",
    "GST": "27ABCDE1234F1Z5"
  }
}
```

**Example Request** (EU - Germany):
```json
{
  "values": {
    "VAT": "DE123456789"
  }
}
```

### 6. Company Details: Enter Bank Details

Enter company bank details for a specific country.

#### Get Bank Field Configurations for Country

**API Endpoint**: `GET /api/v1/countries/{countryId}/bank-field-configurations`

**Example Response**:
```json
{
  "success": true,
  "data": [
    {
      "configurationId": "e5f6a7b8-c9d0-4901-e2f3-a4b5c6d7e8f9",
      "countryId": "c3d4e5f6-a7b8-4901-c2d3-e4f5a6b7c8d9",
      "bankFieldTypeId": "d4e5f6a7-b8c9-4901-d2e3-f4a5b6c7d8e9",
      "bankFieldTypeName": "IFSC",
      "bankFieldTypeDisplayName": "IFSC Code",
      "isRequired": true,
      "validationRegex": "^[A-Z]{4}0[0-9A-Z]{6}$",
      "minLength": 11,
      "maxLength": 11,
      "displayName": "IFSC Code",
      "helpText": "11-character alphanumeric Indian Financial System Code",
      "displayOrder": 1,
      "isActive": true
    }
  ]
}
```

#### Save Company Bank Details

**API Endpoint**: `PUT /api/v1/company-details/{countryId}/bank-details`

**Example Request** (India):
```json
{
  "values": {
    "IFSC": "HDFC0001234",
    "AccountNumber": "1234567890",
    "BankName": "HDFC Bank",
    "BranchName": "Mumbai"
  }
}
```

**Example Request** (Dubai):
```json
{
  "values": {
    "IBAN": "AE070331234567890123456",
    "SWIFT": "HSBCAEAD",
    "AccountNumber": "1234567890",
    "BankName": "HSBC UAE",
    "BranchName": "Dubai"
  }
}
```

## Frontend Usage

### 1. Master Configuration Pages

Navigate to:
- `/admin/company-identifiers` - Manage identifier types
- `/admin/company-identifiers/{countryId}` - Configure identifier types for country
- `/admin/company-bank-fields` - Manage bank field types
- `/admin/company-bank-fields/{countryId}` - Configure bank field types for country

### 2. Company Details Page

Navigate to `/admin/company-details`:

1. **Select Country** - Choose the country for which you want to enter details
2. **Enter Identifiers** - Form dynamically displays only identifier fields configured for the selected country
3. **Enter Bank Details** - Form dynamically displays only bank fields configured for the selected country
4. **Save** - Values are validated against country-specific rules before saving

### 3. Quotation Integration

When creating a quotation:
- Company identifiers and bank details are automatically filtered by client country
- Only relevant fields for the client's country are displayed in the quotation
- PDF and email generation includes only country-relevant company details

## Validation Rules

### Identifier Validation

- **Format Validation**: Uses regex patterns from country configuration
- **Length Validation**: Enforces min/max length from country configuration
- **Required Fields**: Ensures all required identifiers for the country are provided

### Bank Details Validation

- **Format Validation**: Uses regex patterns from country configuration
- **Length Validation**: Enforces min/max length from country configuration
- **Required Fields**: Ensures all required bank fields for the country are provided

## Common Patterns

### Adding a New Country

1. Ensure the country exists in the Countries table (from Spec-020)
2. Create identifier type configurations for the country:
   - Determine which identifier types are needed
   - Configure validation rules and display properties
3. Create bank field type configurations for the country:
   - Determine which bank fields are needed
   - Configure validation rules and display properties
4. Enter company identifier values for the country
5. Enter company bank details for the country

### Adding a New Identifier Type

1. Create the identifier type in master configuration
2. Configure it for relevant countries with validation rules
3. Company details form will automatically include the new identifier type for configured countries

### Adding a New Bank Field Type

1. Create the bank field type in master configuration
2. Configure it for relevant countries with validation rules
3. Company details form will automatically include the new bank field type for configured countries

## Troubleshooting

### Issue: Field Not Showing in Form

**Solution**: Check that:
1. The identifier/bank field type exists and is active
2. Country configuration exists for the selected country and is active
3. The country is properly selected in the form

### Issue: Validation Error

**Solution**: Check that:
1. The value matches the validation regex pattern for the country
2. The value length is within min/max constraints
3. Required fields are provided

### Issue: Country Configuration Not Found

**Solution**: Ensure:
1. The country exists in the Countries table (from Spec-020)
2. Identifier/bank field configurations are created for the country
3. Configurations are active (IsActive = true)

## Next Steps

- Review the [Data Model](./data-model.md) for detailed database schema
- Review the [API Contract](./contracts/country-company-identifiers.openapi.yaml) for complete API documentation
- Review the [Implementation Plan](./plan.md) for technical implementation details
- Review the [Research](./research.md) for design decisions and rationale

