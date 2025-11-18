export interface Currency {
  currencyCode: string;
  displayName: string;
  symbol: string;
  decimalPlaces: number;
  isDefault: boolean;
  isActive: boolean;
}

export interface ExchangeRate {
  exchangeRateId: string;
  fromCurrencyCode: string;
  toCurrencyCode: string;
  rate: number;
  effectiveDate: string;
  expiryDate?: string;
  source?: string;
  isActive: boolean;
}

export interface SupportedLanguage {
  languageCode: string;
  displayName: string;
  displayNameEn: string;
  nativeName: string;
  isRTL: boolean;
  isActive: boolean;
  flagIcon?: string;
}

export interface UserPreferences {
  userId: string;
  languageCode: string;
  currencyCode?: string;
  dateFormat: string;
  timeFormat: string;
  numberFormat: string;
  timezone?: string;
  firstDayOfWeek: number;
}

export interface CompanyPreferences {
  companyId: string;
  defaultLanguageCode: string;
  defaultCurrencyCode: string;
  dateFormat: string;
  timeFormat: string;
  numberFormat: string;
  timezone?: string;
  firstDayOfWeek: number;
}

export interface LocalizationResource {
  resourceId: string;
  languageCode: string;
  resourceKey: string;
  resourceValue: string;
  category?: string;
  isActive: boolean;
}

export interface CurrencyConversionRequest {
  amount: number;
  fromCurrencyCode: string;
  toCurrencyCode: string;
  asOfDate?: string;
}

export interface CurrencyConversionResponse {
  originalAmount: number;
  fromCurrencyCode: string;
  convertedAmount: number;
  toCurrencyCode: string;
  exchangeRate: number;
  formattedOriginalAmount?: string;
  formattedConvertedAmount?: string;
}

export interface UpdateUserPreferencesRequest {
  languageCode?: string;
  currencyCode?: string;
  dateFormat?: string;
  timeFormat?: string;
  numberFormat?: string;
  timezone?: string;
  firstDayOfWeek?: number;
}

export interface UpdateCompanyPreferencesRequest {
  defaultLanguageCode?: string;
  defaultCurrencyCode?: string;
  dateFormat?: string;
  timeFormat?: string;
  numberFormat?: string;
  timezone?: string;
  firstDayOfWeek?: number;
}

export interface CreateLocalizationResourceRequest {
  languageCode: string;
  resourceKey: string;
  resourceValue: string;
  category?: string;
}

export interface UpdateLocalizationResourceRequest {
  resourceValue?: string;
  category?: string;
  isActive?: boolean;
}

export interface CreateCurrencyRequest {
  currencyCode: string;
  displayName: string;
  symbol: string;
  decimalPlaces: number;
  isDefault: boolean;
}

export interface UpdateExchangeRateRequest {
  fromCurrencyCode: string;
  toCurrencyCode: string;
  rate: number;
  effectiveDate: string;
  expiryDate?: string;
  source?: string;
}

