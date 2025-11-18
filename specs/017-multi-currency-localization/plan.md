# Spec-017: Implementation Plan - Multi-Currency, Multi-Language & Localization

## Overview

This plan outlines the phased implementation of multi-currency support, multi-language localization, and user/company preferences. The implementation will be done in parallel for backend and frontend to ensure seamless integration.

## Implementation Phases

### Phase 1: Database & Entities (Week 1)
**Goal**: Set up database schema and domain entities

**Tasks**:
1. Create database migrations for:
   - `Currencies` table
   - `ExchangeRates` table
   - `UserPreferences` table
   - `CompanyPreferences` table
   - `LocalizationResources` table
   - `SupportedLanguages` table
2. Create C# entity classes:
   - `Currency.cs`
   - `ExchangeRate.cs`
   - `UserPreferences.cs`
   - `CompanyPreferences.cs`
   - `LocalizationResource.cs`
   - `SupportedLanguage.cs`
3. Create enums:
   - `DateFormat` enum (optional, or use string)
   - `TimeFormat` enum
4. Configure Entity Framework mappings
5. Update `AppDbContext` and `IAppDbContext`
6. Seed initial data (currencies, languages, default resources)
7. Run migrations and verify schema

**Deliverables**:
- 6 database tables created
- 6 entity classes
- EF Core configurations
- Initial seed data

---

### Phase 2: DTOs & Request Models (Week 1)
**Goal**: Define all data transfer objects and request models

**Tasks**:
1. Create DTOs:
   - `CurrencyDto.cs`
   - `ExchangeRateDto.cs`
   - `UserPreferencesDto.cs`
   - `CompanyPreferencesDto.cs`
   - `LocalizationResourceDto.cs`
   - `SupportedLanguageDto.cs`
   - `CurrencyConversionRequest.cs`
   - `CurrencyConversionResponse.cs`
2. Create request DTOs:
   - `UpdateUserPreferencesRequest.cs`
   - `UpdateCompanyPreferencesRequest.cs`
   - `CreateLocalizationResourceRequest.cs`
   - `UpdateLocalizationResourceRequest.cs`
   - `CreateCurrencyRequest.cs`
   - `UpdateExchangeRateRequest.cs`
3. Create response DTOs for all endpoints

**Deliverables**:
- 15+ DTO classes
- Request/Response models for all endpoints

---

### Phase 3: Services - Currency & Exchange Rates (Week 2)
**Goal**: Implement currency conversion and exchange rate management

**Tasks**:
1. Create `ICurrencyService` interface:
   - `GetSupportedCurrenciesAsync()`
   - `GetCurrencyByCodeAsync(string code)`
   - `ConvertCurrencyAsync(decimal amount, string from, string to, DateTime? date)`
   - `GetExchangeRateAsync(string from, string to, DateTime? date)`
2. Create `IExchangeRateService` interface:
   - `GetLatestRatesAsync()`
   - `UpdateExchangeRateAsync(UpdateExchangeRateRequest)`
   - `GetHistoricalRatesAsync(string from, string to, DateTime fromDate, DateTime toDate)`
3. Implement `CurrencyService`:
   - Currency lookup and validation
   - Currency conversion logic
   - Exchange rate retrieval
4. Implement `ExchangeRateService`:
   - Rate management
   - Historical rate tracking
   - Rate validation
5. Create `IExchangeRateUpdaterService` interface:
   - `FetchLatestRatesFromApiAsync()`
   - `UpdateRatesAsync()`
6. Implement exchange rate updater (with optional third-party API integration)
7. Create background job for scheduled rate updates

**Deliverables**:
- Currency service implementation
- Exchange rate service implementation
- Exchange rate updater service
- Background job for rate updates

---

### Phase 4: Services - Localization (Week 2)
**Goal**: Implement localization and translation services

**Tasks**:
1. Create `ILocalizationService` interface:
   - `GetLocalizedStringAsync(string key, string languageCode)`
   - `GetLocalizedStringsAsync(string languageCode, string category)`
   - `GetAllResourcesForLanguageAsync(string languageCode)`
   - `GetFallbackStringAsync(string key)`
2. Implement `LocalizationService`:
   - Resource lookup with fallback
   - Caching strategy
   - Resource loading optimization
3. Create `ILocalizationResourceManager` interface:
   - `CreateResourceAsync(CreateLocalizationResourceRequest)`
   - `UpdateResourceAsync(Guid resourceId, UpdateLocalizationResourceRequest)`
   - `DeleteResourceAsync(Guid resourceId)`
   - `ImportResourcesAsync(string languageCode, Dictionary<string, string>)`
   - `ExportResourcesAsync(string languageCode)`
4. Implement `LocalizationResourceManager`:
   - CRUD operations for resources
   - Import/Export functionality
   - Validation and error handling

**Deliverables**:
- Localization service implementation
- Localization resource manager
- Caching and optimization

---

### Phase 5: Services - Preferences (Week 2)
**Goal**: Implement user and company preference management

**Tasks**:
1. Create `IUserPreferenceService` interface:
   - `GetUserPreferencesAsync(Guid userId)`
   - `UpdateUserPreferencesAsync(Guid userId, UpdateUserPreferencesRequest)`
   - `GetEffectivePreferencesAsync(Guid userId)` (user + company defaults)
2. Implement `UserPreferenceService`:
   - Preference retrieval
   - Preference updates
   - Default value resolution
3. Create `ICompanyPreferenceService` interface:
   - `GetCompanyPreferencesAsync(Guid companyId)`
   - `UpdateCompanyPreferencesAsync(Guid companyId, UpdateCompanyPreferencesRequest)`
4. Implement `CompanyPreferenceService`:
   - Company preference management
   - Default value management

**Deliverables**:
- User preference service
- Company preference service
- Effective preference resolution

---

### Phase 6: Formatting Utilities (Week 2)
**Goal**: Create locale-aware formatting utilities

**Tasks**:
1. Create `ILocaleFormatter` interface:
   - `FormatCurrency(decimal amount, string currencyCode, string locale)`
   - `FormatDate(DateTime date, string locale, string format)`
   - `FormatNumber(decimal number, string locale)`
   - `FormatDateTime(DateTimeOffset dateTime, string locale)`
   - `ParseDate(string dateString, string locale, string format)`
   - `ParseNumber(string numberString, string locale)`
2. Implement `LocaleFormatter`:
   - Use .NET `CultureInfo` and `NumberFormatInfo`
   - Currency symbol placement
   - Date/time formatting
   - Number formatting with locale-specific separators
3. Create validation utilities:
   - `ValidateDateFormat(string format)`
   - `ValidateLocale(string locale)`
   - `ValidateCurrencyCode(string code)`

**Deliverables**:
- Locale formatter service
- Formatting utilities
- Validation utilities

---

### Phase 7: Command Handlers (Week 3)
**Goal**: Implement command handlers for preferences and resources

**Tasks**:
1. `UpdateUserPreferencesCommandHandler`:
   - Validate request
   - Update or create user preferences
   - Publish preference changed event
2. `UpdateCompanyPreferencesCommandHandler`:
   - Validate request (admin only)
   - Update company preferences
   - Publish company preference changed event
3. `CreateLocalizationResourceCommandHandler`:
   - Validate resource key and value
   - Create resource
   - Invalidate cache
4. `UpdateLocalizationResourceCommandHandler`:
   - Update resource
   - Invalidate cache
5. `DeleteLocalizationResourceCommandHandler`:
   - Delete resource
   - Invalidate cache
6. `CreateCurrencyCommandHandler`:
   - Validate currency code
   - Create currency
7. `UpdateExchangeRateCommandHandler`:
   - Validate rate
   - Update or create exchange rate
   - Invalidate cache

**Deliverables**:
- 7 command handlers
- Event publishing
- Cache invalidation

---

### Phase 8: Query Handlers (Week 3)
**Goal**: Implement query handlers for localization and currency data

**Tasks**:
1. `GetSupportedCurrenciesQueryHandler`:
   - Return active currencies
   - Include default currency flag
2. `GetSupportedLanguagesQueryHandler`:
   - Return active languages
   - Include RTL flag
3. `GetUserPreferencesQueryHandler`:
   - Return user preferences with defaults
4. `GetCompanyPreferencesQueryHandler`:
   - Return company preferences
5. `GetLocalizationResourcesQueryHandler`:
   - Return resources for language
   - Support filtering by category
6. `GetExchangeRatesQueryHandler`:
   - Return latest exchange rates
   - Support date filtering
7. `ConvertCurrencyQueryHandler`:
   - Perform currency conversion
   - Return formatted result

**Deliverables**:
- 7 query handlers
- Efficient data retrieval
- Caching where appropriate

---

### Phase 9: API Controllers (Week 3)
**Goal**: Create RESTful API endpoints

**Tasks**:
1. `LocalizationController`:
   - GET `/api/localization/languages` - List supported languages
   - GET `/api/localization/resources/{languageCode}` - Get resources for language
   - POST `/api/localization/resources` - Create resource (admin)
   - PUT `/api/localization/resources/{resourceId}` - Update resource (admin)
   - DELETE `/api/localization/resources/{resourceId}` - Delete resource (admin)
2. `CurrenciesController`:
   - GET `/api/currencies` - List currencies
   - GET `/api/currencies/{code}` - Get currency by code
   - POST `/api/currencies` - Create currency (admin)
   - PUT `/api/currencies/{code}` - Update currency (admin)
3. `ExchangeRatesController`:
   - GET `/api/exchange-rates` - Get latest rates
   - GET `/api/exchange-rates/{from}/{to}` - Get rate for pair
   - POST `/api/exchange-rates` - Create/update rate (admin)
   - GET `/api/exchange-rates/historical` - Get historical rates
4. `UserPreferencesController`:
   - GET `/api/users/{userId}/preferences` - Get user preferences
   - PUT `/api/users/{userId}/preferences` - Update user preferences
5. `CompanyPreferencesController`:
   - GET `/api/company/preferences` - Get company preferences
   - PUT `/api/company/preferences` - Update company preferences (admin)
6. `CurrencyConversionController`:
   - POST `/api/currency/convert` - Convert currency

**Deliverables**:
- 6 API controllers
- 15+ endpoints
- Proper authorization
- Error handling

---

### Phase 10: Validators & AutoMapper (Week 3)
**Goal**: Create validators and mapping configurations

**Tasks**:
1. Create FluentValidation validators:
   - `UpdateUserPreferencesRequestValidator`
   - `UpdateCompanyPreferencesRequestValidator`
   - `CreateLocalizationResourceRequestValidator`
   - `UpdateLocalizationResourceRequestValidator`
   - `CreateCurrencyRequestValidator`
   - `UpdateExchangeRateRequestValidator`
   - `CurrencyConversionRequestValidator`
2. Create AutoMapper profiles:
   - `CurrencyProfile`
   - `ExchangeRateProfile`
   - `UserPreferencesProfile`
   - `CompanyPreferencesProfile`
   - `LocalizationResourceProfile`
   - `SupportedLanguageProfile`

**Deliverables**:
- 7 validators
- 6 AutoMapper profiles

---

### Phase 11: Frontend - i18n Setup & Types (Week 4)
**Goal**: Set up internationalization infrastructure

**Tasks**:
1. Install and configure i18n library (next-i18next or react-i18next)
2. Create i18n configuration file
3. Set up translation file structure
4. Create TypeScript types:
   - `Currency.ts`
   - `ExchangeRate.ts`
   - `UserPreferences.ts`
   - `CompanyPreferences.ts`
   - `LocalizationResource.ts`
   - `SupportedLanguage.ts`
5. Create API client methods:
   - `LocalizationApi`
   - `CurrenciesApi`
   - `ExchangeRatesApi`
   - `UserPreferencesApi`
   - `CompanyPreferencesApi`
6. Create translation files for English and Hindi

**Deliverables**:
- i18n library configured
- TypeScript types
- API client methods
- Initial translation files

---

### Phase 12: Frontend - Language & Currency Selectors (Week 4)
**Goal**: Create UI components for language and currency selection

**Tasks**:
1. Create `LanguageSelector` component:
   - Dropdown with language options
   - Flag icons (optional)
   - RTL indicator
   - Save preference on change
2. Create `CurrencySelector` component:
   - Dropdown with currency options
   - Currency symbols
   - Save preference on change
3. Integrate selectors into site header/footer
4. Create `LocaleProvider` context:
   - Current locale state
   - Language and currency
   - Formatting functions
5. Create `useLocale` hook:
   - Access current locale
   - Format currency, date, number
   - Change locale

**Deliverables**:
- Language selector component
- Currency selector component
- Locale context and hook
- Header/footer integration

---

### Phase 13: Frontend - User Preferences Page (Week 4)
**Goal**: Create user preferences management page

**Tasks**:
1. Create `/profile/preferences` page:
   - Language selection
   - Currency selection
   - Date format selection
   - Time format selection
   - Number format selection
   - Timezone selection (if implemented)
   - Preview section
2. Create `PreferencesForm` component:
   - Form fields for all preferences
   - Validation
   - Save functionality
3. Create `FormatPreview` component:
   - Show formatted examples
   - Update on preference change
4. Integrate with API
5. Add toast notifications

**Deliverables**:
- User preferences page
- Preferences form component
- Format preview component

---

### Phase 14: Frontend - Localized Components (Week 5)
**Goal**: Apply localization to existing components

**Tasks**:
1. Update quotation components:
   - Localize labels and messages
   - Format currency amounts
   - Format dates
2. Update payment components:
   - Localize payment status
   - Format amounts
3. Update notification components:
   - Localize notification messages
4. Update form components:
   - Localize validation messages
   - Localize field labels
5. Update dashboard components:
   - Format all numbers and dates
   - Localize chart labels
6. Create `LocalizedText` component:
   - Wrapper for translated text
   - Fallback handling

**Deliverables**:
- Localized quotation components
- Localized payment components
- Localized notification components
- Localized form components
- Localized dashboard components

---

### Phase 15: Frontend - Admin Dashboards (Week 5)
**Goal**: Create admin interfaces for managing localization and currencies

**Tasks**:
1. Create `/admin/localization` page:
   - List all resource keys
   - Filter by language
   - Filter by category
   - Add/Edit/Delete resources
   - Import/Export CSV
   - Search functionality
2. Create `/admin/currencies` page:
   - List currencies
   - Add/Edit currencies
   - Set default currency
   - Manage exchange rates
   - View rate history
3. Create `LocalizationResourceTable` component:
   - Editable table
   - Bulk operations
4. Create `CurrencyManagementTable` component:
   - Currency CRUD
   - Exchange rate management
5. Create `ExchangeRateHistoryChart` component:
   - Visualize rate trends

**Deliverables**:
- Admin localization dashboard
- Admin currency dashboard
- Management components
- Import/Export functionality

---

### Phase 16: Integration - Email & PDF Localization (Week 6)
**Goal**: Integrate localization with email and PDF generation

**Tasks**:
1. Update email service (Spec-013):
   - Use user's preferred language
   - Localize email templates
   - Format currency and dates
2. Update PDF generation service:
   - Use locale for formatting
   - Support multiple languages
   - RTL support for PDFs
3. Create localized email templates:
   - Quotation sent
   - Payment received
   - Refund processed
   - etc.
4. Test email localization
5. Test PDF localization

**Deliverables**:
- Localized email service
- Localized PDF generation
- Email templates in multiple languages

---

### Phase 17: Integration - Reports & Analytics (Week 6)
**Goal**: Apply localization to reports and analytics

**Tasks**:
1. Update report generation (Spec-015):
   - Format currency based on user preference
   - Format dates based on locale
   - Localize report labels
2. Update dashboard queries:
   - Return formatted data
   - Support currency conversion
3. Update export services:
   - Localize exported data
   - Format based on locale
4. Test report localization

**Deliverables**:
- Localized reports
- Localized analytics
- Localized exports

---

### Phase 18: Testing & Quality Assurance (Week 6)
**Goal**: Comprehensive testing of all features

**Tasks**:
1. Backend unit tests:
   - Currency conversion accuracy
   - Exchange rate management
   - Localization service
   - Preference management
2. Backend integration tests:
   - API endpoints
   - Currency conversion flows
   - Preference update flows
3. Frontend unit tests:
   - Locale switching
   - Formatting functions
   - Component localization
4. Frontend integration tests:
   - Preference updates
   - Language switching
   - Currency switching
5. E2E tests:
   - User changes language
   - User changes currency
   - Admin manages resources
   - Admin manages currencies
6. Accessibility testing:
   - Keyboard navigation
   - Screen reader support
   - RTL layout testing
7. Browser compatibility testing
8. Performance testing:
   - Translation loading
   - Currency conversion speed
   - Cache effectiveness

**Deliverables**:
- Comprehensive test suite
- Test coverage ≥85% backend, ≥80% frontend
- Accessibility compliance
- Performance benchmarks

---

### Phase 19: Documentation & Deployment (Week 7)
**Goal**: Finalize documentation and prepare for deployment

**Tasks**:
1. API documentation:
   - Document all endpoints
   - Include examples
   - Document error responses
2. User guide:
   - How to change language
   - How to change currency
   - How to set preferences
   - Admin guides
3. Developer guide:
   - How to add new languages
   - How to add new currencies
   - How to add translations
   - Formatting guidelines
4. Migration guide:
   - Database migration steps
   - Data migration steps
   - Configuration steps
5. Update implementation summary

**Deliverables**:
- API documentation
- User guides
- Developer guides
- Migration guides

---

## Risk Mitigation

1. **Translation Quality**: Use professional translators for production languages
2. **Exchange Rate Accuracy**: Implement rate validation and alerts
3. **Performance Impact**: Implement aggressive caching strategy
4. **Missing Translations**: Implement robust fallback mechanism
5. **Currency Conversion Errors**: Add comprehensive validation and testing
6. **RTL Layout Issues**: Test thoroughly with RTL languages
7. **Third-Party API Failures**: Implement fallback to manual rates

---

## Success Metrics

- All UI elements translatable
- Currency conversion accuracy: 100%
- Translation coverage: ≥95% for supported languages
- Preference update response time: <200ms
- Exchange rate update frequency: Daily (configurable)
- User satisfaction with localization: ≥90%

---

## Timeline Summary

- **Week 1**: Database & DTOs
- **Week 2**: Services (Currency, Localization, Preferences)
- **Week 3**: Commands, Queries, Controllers, Validators
- **Week 4**: Frontend i18n setup, Selectors, Preferences page
- **Week 5**: Localized components, Admin dashboards
- **Week 6**: Integration (Email, PDF, Reports), Testing
- **Week 7**: Documentation, Final QA

**Total Duration**: 7 weeks

---

**End of Implementation Plan**

