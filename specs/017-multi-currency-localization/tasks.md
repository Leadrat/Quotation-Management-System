# Spec-017: Task Breakdown - Multi-Currency, Multi-Language & Localization

## Phase 1: Database & Entities (15 tasks)

### 1.1 Database Migrations
- [ ] Create migration: `CreateCurrenciesTable`
- [ ] Create migration: `CreateExchangeRatesTable`
- [ ] Create migration: `CreateUserPreferencesTable`
- [ ] Create migration: `CreateCompanyPreferencesTable`
- [ ] Create migration: `CreateLocalizationResourcesTable`
- [ ] Create migration: `CreateSupportedLanguagesTable`
- [ ] Verify migrations run successfully

### 1.2 Entity Classes
- [ ] Create `Currency.cs` entity class
- [ ] Create `ExchangeRate.cs` entity class
- [ ] Create `UserPreferences.cs` entity class
- [ ] Create `CompanyPreferences.cs` entity class
- [ ] Create `LocalizationResource.cs` entity class
- [ ] Create `SupportedLanguage.cs` entity class

### 1.3 Enums
- [ ] Create `TimeFormat.cs` enum (12h/24h)

### 1.4 Entity Configurations
- [ ] Create `CurrencyEntityConfiguration.cs`
- [ ] Create `ExchangeRateEntityConfiguration.cs`
- [ ] Create `UserPreferencesEntityConfiguration.cs`
- [ ] Create `CompanyPreferencesEntityConfiguration.cs`
- [ ] Create `LocalizationResourceEntityConfiguration.cs`
- [ ] Create `SupportedLanguageEntityConfiguration.cs`
- [ ] Update `AppDbContext` and `IAppDbContext`

### 1.5 Seed Data
- [ ] Seed default currencies (INR, USD, EUR, GBP)
- [ ] Seed default languages (en, hi)
- [ ] Seed initial exchange rates
- [ ] Seed English localization resources
- [ ] Create default company preferences for existing companies
- [ ] Create default user preferences for existing users

---

## Phase 2: DTOs & Request Models (15 tasks)

### 2.1 Currency DTOs
- [ ] Create `CurrencyDto.cs`
- [ ] Create `ExchangeRateDto.cs`
- [ ] Create `CurrencyConversionRequest.cs`
- [ ] Create `CurrencyConversionResponse.cs`

### 2.2 Preference DTOs
- [ ] Create `UserPreferencesDto.cs`
- [ ] Create `CompanyPreferencesDto.cs`
- [ ] Create `UpdateUserPreferencesRequest.cs`
- [ ] Create `UpdateCompanyPreferencesRequest.cs`

### 2.3 Localization DTOs
- [ ] Create `LocalizationResourceDto.cs`
- [ ] Create `SupportedLanguageDto.cs`
- [ ] Create `CreateLocalizationResourceRequest.cs`
- [ ] Create `UpdateLocalizationResourceRequest.cs`

### 2.4 Currency Management DTOs
- [ ] Create `CreateCurrencyRequest.cs`
- [ ] Create `UpdateCurrencyRequest.cs`
- [ ] Create `UpdateExchangeRateRequest.cs`

---

## Phase 3: Services - Currency & Exchange Rates (12 tasks)

### 3.1 Currency Service
- [ ] Create `ICurrencyService` interface
- [ ] Implement `CurrencyService` class
- [ ] Implement `GetSupportedCurrenciesAsync()`
- [ ] Implement `GetCurrencyByCodeAsync()`
- [ ] Implement `ConvertCurrencyAsync()`
- [ ] Implement `GetExchangeRateAsync()`

### 3.2 Exchange Rate Service
- [ ] Create `IExchangeRateService` interface
- [ ] Implement `ExchangeRateService` class
- [ ] Implement `GetLatestRatesAsync()`
- [ ] Implement `UpdateExchangeRateAsync()`
- [ ] Implement `GetHistoricalRatesAsync()`

### 3.3 Exchange Rate Updater
- [ ] Create `IExchangeRateUpdaterService` interface
- [ ] Implement `ExchangeRateUpdaterService` class
- [ ] Implement `FetchLatestRatesFromApiAsync()` (optional third-party API)
- [ ] Create background job `ExchangeRateUpdateJob`
- [ ] Configure job scheduling

---

## Phase 4: Services - Localization (10 tasks)

### 4.1 Localization Service
- [ ] Create `ILocalizationService` interface
- [ ] Implement `LocalizationService` class
- [ ] Implement `GetLocalizedStringAsync()`
- [ ] Implement `GetLocalizedStringsAsync()`
- [ ] Implement `GetAllResourcesForLanguageAsync()`
- [ ] Implement `GetFallbackStringAsync()`
- [ ] Implement caching strategy

### 4.2 Localization Resource Manager
- [ ] Create `ILocalizationResourceManager` interface
- [ ] Implement `LocalizationResourceManager` class
- [ ] Implement `CreateResourceAsync()`
- [ ] Implement `UpdateResourceAsync()`
- [ ] Implement `DeleteResourceAsync()`
- [ ] Implement `ImportResourcesAsync()`
- [ ] Implement `ExportResourcesAsync()`

---

## Phase 5: Services - Preferences (8 tasks)

### 5.1 User Preference Service
- [ ] Create `IUserPreferenceService` interface
- [ ] Implement `UserPreferenceService` class
- [ ] Implement `GetUserPreferencesAsync()`
- [ ] Implement `UpdateUserPreferencesAsync()`
- [ ] Implement `GetEffectivePreferencesAsync()` (user + company defaults)

### 5.2 Company Preference Service
- [ ] Create `ICompanyPreferenceService` interface
- [ ] Implement `CompanyPreferenceService` class
- [ ] Implement `GetCompanyPreferencesAsync()`
- [ ] Implement `UpdateCompanyPreferencesAsync()`

---

## Phase 6: Formatting Utilities (8 tasks)

### 6.1 Locale Formatter
- [ ] Create `ILocaleFormatter` interface
- [ ] Implement `LocaleFormatter` class
- [ ] Implement `FormatCurrency()`
- [ ] Implement `FormatDate()`
- [ ] Implement `FormatNumber()`
- [ ] Implement `FormatDateTime()`
- [ ] Implement `ParseDate()`
- [ ] Implement `ParseNumber()`

### 6.2 Validation Utilities
- [ ] Create `ValidateDateFormat()`
- [ ] Create `ValidateLocale()`
- [ ] Create `ValidateCurrencyCode()`

---

## Phase 7: Command Handlers (7 tasks)

- [ ] `UpdateUserPreferencesCommandHandler`
- [ ] `UpdateCompanyPreferencesCommandHandler`
- [ ] `CreateLocalizationResourceCommandHandler`
- [ ] `UpdateLocalizationResourceCommandHandler`
- [ ] `DeleteLocalizationResourceCommandHandler`
- [ ] `CreateCurrencyCommandHandler`
- [ ] `UpdateExchangeRateCommandHandler`

---

## Phase 8: Query Handlers (7 tasks)

- [ ] `GetSupportedCurrenciesQueryHandler`
- [ ] `GetSupportedLanguagesQueryHandler`
- [ ] `GetUserPreferencesQueryHandler`
- [ ] `GetCompanyPreferencesQueryHandler`
- [ ] `GetLocalizationResourcesQueryHandler`
- [ ] `GetExchangeRatesQueryHandler`
- [ ] `ConvertCurrencyQueryHandler`

---

## Phase 9: API Controllers (15 tasks)

### 9.1 LocalizationController
- [ ] GET `/api/localization/languages`
- [ ] GET `/api/localization/resources/{languageCode}`
- [ ] POST `/api/localization/resources`
- [ ] PUT `/api/localization/resources/{resourceId}`
- [ ] DELETE `/api/localization/resources/{resourceId}`

### 9.2 CurrenciesController
- [ ] GET `/api/currencies`
- [ ] GET `/api/currencies/{code}`
- [ ] POST `/api/currencies`
- [ ] PUT `/api/currencies/{code}`

### 9.3 ExchangeRatesController
- [ ] GET `/api/exchange-rates`
- [ ] GET `/api/exchange-rates/{from}/{to}`
- [ ] POST `/api/exchange-rates`
- [ ] GET `/api/exchange-rates/historical`

### 9.4 UserPreferencesController
- [ ] GET `/api/users/{userId}/preferences`
- [ ] PUT `/api/users/{userId}/preferences`

### 9.5 CompanyPreferencesController
- [ ] GET `/api/company/preferences`
- [ ] PUT `/api/company/preferences`

### 9.6 CurrencyConversionController
- [ ] POST `/api/currency/convert`

---

## Phase 10: Validators & AutoMapper (13 tasks)

### 10.1 Validators
- [ ] `UpdateUserPreferencesRequestValidator`
- [ ] `UpdateCompanyPreferencesRequestValidator`
- [ ] `CreateLocalizationResourceRequestValidator`
- [ ] `UpdateLocalizationResourceRequestValidator`
- [ ] `CreateCurrencyRequestValidator`
- [ ] `UpdateExchangeRateRequestValidator`
- [ ] `CurrencyConversionRequestValidator`

### 10.2 AutoMapper Profiles
- [ ] `CurrencyProfile`
- [ ] `ExchangeRateProfile`
- [ ] `UserPreferencesProfile`
- [ ] `CompanyPreferencesProfile`
- [ ] `LocalizationResourceProfile`
- [ ] `SupportedLanguageProfile`

---

## Phase 11: Frontend - i18n Setup & Types (10 tasks)

### 11.1 i18n Configuration
- [ ] Install i18n library (next-i18next)
- [ ] Create i18n configuration file
- [ ] Set up translation file structure
- [ ] Configure language detection
- [ ] Configure fallback language

### 11.2 TypeScript Types
- [ ] Create `Currency.ts` type
- [ ] Create `ExchangeRate.ts` type
- [ ] Create `UserPreferences.ts` type
- [ ] Create `CompanyPreferences.ts` type
- [ ] Create `LocalizationResource.ts` type
- [ ] Create `SupportedLanguage.ts` type

### 11.3 API Client
- [ ] Create `LocalizationApi` methods
- [ ] Create `CurrenciesApi` methods
- [ ] Create `ExchangeRatesApi` methods
- [ ] Create `UserPreferencesApi` methods
- [ ] Create `CompanyPreferencesApi` methods

### 11.4 Translation Files
- [ ] Create English translation file (`en.json`)
- [ ] Create Hindi translation file (`hi.json`)
- [ ] Seed common translations

---

## Phase 12: Frontend - Language & Currency Selectors (8 tasks)

### 12.1 Language Selector
- [ ] Create `LanguageSelector` component
- [ ] Add language options with flags
- [ ] Implement language switching
- [ ] Save preference on change
- [ ] Add RTL indicator

### 12.2 Currency Selector
- [ ] Create `CurrencySelector` component
- [ ] Add currency options with symbols
- [ ] Implement currency switching
- [ ] Save preference on change

### 12.3 Locale Context
- [ ] Create `LocaleProvider` context
- [ ] Implement locale state management
- [ ] Create `useLocale` hook
- [ ] Add formatting functions to hook

### 12.4 Integration
- [ ] Integrate selectors into header
- [ ] Integrate selectors into footer (optional)
- [ ] Add responsive design
- [ ] Add accessibility features

---

## Phase 13: Frontend - User Preferences Page (6 tasks)

- [ ] Create `/profile/preferences` page
- [ ] Create `PreferencesForm` component
- [ ] Create `FormatPreview` component
- [ ] Integrate with API
- [ ] Add form validation
- [ ] Add toast notifications

---

## Phase 14: Frontend - Localized Components (12 tasks)

### 14.1 Component Localization
- [ ] Update quotation components
- [ ] Update payment components
- [ ] Update notification components
- [ ] Update form components
- [ ] Update dashboard components

### 14.2 Formatting Components
- [ ] Create `LocalizedText` component
- [ ] Create `FormattedCurrency` component
- [ ] Create `FormattedDate` component
- [ ] Create `FormattedNumber` component
- [ ] Create `FormattedDateTime` component

### 14.3 Integration
- [ ] Apply formatting to all monetary values
- [ ] Apply formatting to all dates
- [ ] Apply formatting to all numbers

---

## Phase 15: Frontend - Admin Dashboards (10 tasks)

### 15.1 Localization Dashboard
- [ ] Create `/admin/localization` page
- [ ] Create `LocalizationResourceTable` component
- [ ] Add filter by language
- [ ] Add filter by category
- [ ] Add search functionality
- [ ] Add import/export CSV

### 15.2 Currency Dashboard
- [ ] Create `/admin/currencies` page
- [ ] Create `CurrencyManagementTable` component
- [ ] Create `ExchangeRateHistoryChart` component
- [ ] Add exchange rate management
- [ ] Add rate history view

---

## Phase 16: Integration - Email & PDF Localization (8 tasks)

### 16.1 Email Localization
- [ ] Update email service to use user language
- [ ] Create localized email templates
- [ ] Format currency in emails
- [ ] Format dates in emails
- [ ] Test email localization

### 16.2 PDF Localization
- [ ] Update PDF generation service
- [ ] Add locale support to PDFs
- [ ] Add RTL support for PDFs
- [ ] Test PDF localization

---

## Phase 17: Integration - Reports & Analytics (6 tasks)

- [ ] Update report generation with locale formatting
- [ ] Update dashboard queries with currency conversion
- [ ] Update export services with localization
- [ ] Format currency in reports
- [ ] Format dates in reports
- [ ] Test report localization

---

## Phase 18: Testing & Quality Assurance (15 tasks)

### 18.1 Backend Tests
- [ ] Unit tests for currency conversion
- [ ] Unit tests for exchange rate management
- [ ] Unit tests for localization service
- [ ] Unit tests for preference management
- [ ] Integration tests for API endpoints

### 18.2 Frontend Tests
- [ ] Unit tests for locale switching
- [ ] Unit tests for formatting functions
- [ ] Unit tests for component localization
- [ ] Integration tests for preference updates
- [ ] E2E tests for language switching
- [ ] E2E tests for currency switching

### 18.3 Quality Assurance
- [ ] Accessibility testing
- [ ] RTL layout testing
- [ ] Browser compatibility testing
- [ ] Performance testing

---

## Phase 19: Documentation & Deployment (5 tasks)

- [ ] API documentation
- [ ] User guides
- [ ] Developer guides
- [ ] Migration guides
- [ ] Update implementation summary

---

## Total Tasks: 180+

**Estimated Timeline**: 7 weeks  
**Team Size**: 2-3 developers (1 backend, 1-2 frontend)

---

**End of Task Breakdown**

