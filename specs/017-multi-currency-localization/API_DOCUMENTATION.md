# Spec-017: API Documentation - Multi-Currency, Multi-Language & Localization

## Overview

This document describes all API endpoints for multi-currency support, multi-language localization, and user/company preferences management.

## Base URL

All endpoints are prefixed with `/api/v1` or `/api` depending on your API versioning strategy.

---

## Localization Endpoints

### Get Supported Languages

**GET** `/api/localization/languages`

Returns a list of all supported languages in the system.

**Response:**
```json
[
  {
    "languageCode": "en",
    "displayNameEn": "English",
    "displayNameNative": "English",
    "isRtl": false,
    "isActive": true
  },
  {
    "languageCode": "hi",
    "displayNameEn": "Hindi",
    "displayNameNative": "हिन्दी",
    "isRtl": false,
    "isActive": true
  }
]
```

### Get Localization Resources

**GET** `/api/localization/resources/{languageCode}`

Returns all localization resources for a specific language.

**Parameters:**
- `languageCode` (path, required): Language code (e.g., "en", "hi")

**Query Parameters:**
- `category` (optional): Filter by resource category

**Response:**
```json
[
  {
    "resourceId": "uuid",
    "languageCode": "en",
    "resourceKey": "common.save",
    "resourceValue": "Save",
    "category": "common"
  }
]
```

### Create Localization Resource

**POST** `/api/localization/resources`

Creates a new localization resource. Requires admin role.

**Request Body:**
```json
{
  "languageCode": "en",
  "resourceKey": "common.save",
  "resourceValue": "Save",
  "category": "common"
}
```

**Response:**
```json
{
  "resourceId": "uuid",
  "languageCode": "en",
  "resourceKey": "common.save",
  "resourceValue": "Save",
  "category": "common",
  "createdAt": "2025-01-XX"
}
```

### Update Localization Resource

**PUT** `/api/localization/resources/{resourceId}`

Updates an existing localization resource. Requires admin role.

**Request Body:**
```json
{
  "resourceValue": "Save Changes",
  "category": "common"
}
```

### Delete Localization Resource

**DELETE** `/api/localization/resources/{resourceId}`

Deletes a localization resource. Requires admin role.

---

## Currency Endpoints

### Get Supported Currencies

**GET** `/api/currencies`

Returns a list of all supported currencies.

**Response:**
```json
[
  {
    "currencyCode": "USD",
    "displayName": "US Dollar",
    "symbol": "$",
    "decimalPlaces": 2,
    "isDefault": false,
    "isActive": true
  },
  {
    "currencyCode": "INR",
    "displayName": "Indian Rupee",
    "symbol": "₹",
    "decimalPlaces": 2,
    "isDefault": true,
    "isActive": true
  }
]
```

### Get Currency by Code

**GET** `/api/currencies/{code}`

Returns details for a specific currency.

**Response:**
```json
{
  "currencyCode": "USD",
  "displayName": "US Dollar",
  "symbol": "$",
  "decimalPlaces": 2,
  "isDefault": false,
  "isActive": true
}
```

### Create Currency

**POST** `/api/currencies`

Creates a new currency. Requires admin role.

**Request Body:**
```json
{
  "currencyCode": "EUR",
  "displayName": "Euro",
  "symbol": "€",
  "decimalPlaces": 2,
  "isDefault": false
}
```

---

## Exchange Rate Endpoints

### Get Exchange Rates

**GET** `/api/exchange-rates`

Returns the latest exchange rates.

**Query Parameters:**
- `fromCurrencyCode` (optional): Filter by source currency
- `toCurrencyCode` (optional): Filter by target currency
- `date` (optional): Get rate for specific date (YYYY-MM-DD)

**Response:**
```json
[
  {
    "exchangeRateId": "uuid",
    "fromCurrencyCode": "USD",
    "toCurrencyCode": "INR",
    "rate": 83.25,
    "effectiveDate": "2025-01-XX"
  }
]
```

### Update Exchange Rate

**POST** `/api/exchange-rates`

Creates or updates an exchange rate. Requires admin role.

**Request Body:**
```json
{
  "fromCurrencyCode": "USD",
  "toCurrencyCode": "INR",
  "rate": 83.25,
  "effectiveDate": "2025-01-XX"
}
```

### Convert Currency

**POST** `/api/currency/convert`

Converts an amount from one currency to another.

**Request Body:**
```json
{
  "amount": 100,
  "fromCurrencyCode": "USD",
  "toCurrencyCode": "INR",
  "date": "2025-01-XX"
}
```

**Response:**
```json
{
  "amount": 100,
  "fromCurrencyCode": "USD",
  "toCurrencyCode": "INR",
  "rate": 83.25,
  "convertedAmount": 8325.00,
  "date": "2025-01-XX"
}
```

---

## User Preferences Endpoints

### Get User Preferences

**GET** `/api/users/{userId}/preferences`

Returns user preferences. If `includeDefaults` query parameter is `true`, merges with company defaults.

**Query Parameters:**
- `includeDefaults` (optional, boolean): Include company default preferences

**Response:**
```json
{
  "userId": "uuid",
  "languageCode": "en",
  "currencyCode": "INR",
  "dateFormat": "dd/MM/yyyy",
  "timeFormat": "24h",
  "numberFormat": "en-IN",
  "timezone": "Asia/Kolkata",
  "firstDayOfWeek": 1
}
```

### Update User Preferences

**PUT** `/api/users/{userId}/preferences`

Updates user preferences.

**Request Body:**
```json
{
  "languageCode": "hi",
  "currencyCode": "USD",
  "dateFormat": "MM/dd/yyyy",
  "timeFormat": "12h",
  "numberFormat": "en-US",
  "timezone": "America/New_York",
  "firstDayOfWeek": 0
}
```

---

## Company Preferences Endpoints

### Get Company Preferences

**GET** `/api/company/preferences`

Returns company default preferences. Requires admin role.

**Response:**
```json
{
  "companyId": "uuid",
  "defaultLanguageCode": "en",
  "defaultCurrencyCode": "INR",
  "dateFormat": "dd/MM/yyyy",
  "timeFormat": "24h",
  "numberFormat": "en-IN",
  "timezone": "Asia/Kolkata",
  "firstDayOfWeek": 1
}
```

### Update Company Preferences

**PUT** `/api/company/preferences`

Updates company default preferences. Requires admin role.

**Request Body:**
```json
{
  "defaultLanguageCode": "en",
  "defaultCurrencyCode": "USD",
  "dateFormat": "MM/dd/yyyy",
  "timeFormat": "12h",
  "numberFormat": "en-US",
  "timezone": "America/New_York",
  "firstDayOfWeek": 0
}
```

---

## Error Responses

All endpoints may return the following error responses:

**400 Bad Request:**
```json
{
  "error": "Validation failed",
  "details": {
    "field": "error message"
  }
}
```

**401 Unauthorized:**
```json
{
  "error": "Unauthorized",
  "message": "Authentication required"
}
```

**403 Forbidden:**
```json
{
  "error": "Forbidden",
  "message": "Insufficient permissions"
}
```

**404 Not Found:**
```json
{
  "error": "Not Found",
  "message": "Resource not found"
}
```

**500 Internal Server Error:**
```json
{
  "error": "Internal Server Error",
  "message": "An unexpected error occurred"
}
```

---

## Authentication

All endpoints require authentication via JWT token in the Authorization header:

```
Authorization: Bearer <token>
```

Admin-only endpoints require the user to have the "Admin" role.

---

## Rate Limiting

- Currency conversion: 100 requests per minute per user
- Exchange rate updates: 10 requests per minute per admin user
- Other endpoints: 1000 requests per minute per user

---

## Versioning

API versioning is handled via URL path. Current version: `v1`.

---

**Last Updated**: 2025-01-XX

