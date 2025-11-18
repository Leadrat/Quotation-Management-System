import { LocalizationApi } from "./api";
import { getAccessToken } from "./session";
import type { SupportedLanguage, UserPreferences } from "../types/localization";

let currentLanguage = "en";
let translations: Record<string, string> = {};
let userPreferences: UserPreferences | null = null;

export async function initializeI18n(preferences?: UserPreferences) {
  if (preferences) {
    userPreferences = preferences;
    currentLanguage = preferences.languageCode;
  } else {
    try {
      userPreferences = await LocalizationApi.getUserPreferences(true);
      currentLanguage = userPreferences.languageCode;
    } catch (error) {
      console.warn("Failed to load user preferences, using defaults", error);
      currentLanguage = "en";
    }
  }

  await loadTranslations(currentLanguage);
}

export async function loadTranslations(languageCode: string) {
  // Only load if authenticated
  if (!getAccessToken()) {
    translations = {};
    currentLanguage = languageCode;
    return;
  }
  try {
    translations = await LocalizationApi.getLocalizationResources(languageCode);
    currentLanguage = languageCode;
  } catch (error: any) {
    // Silently ignore 401 errors
    if (!error?.message?.includes("401")) {
      console.warn(`Failed to load translations for ${languageCode}`, error);
    }
    translations = {};
  }
}

export function t(key: string, params?: Record<string, string | number>): string {
  let value = translations[key] || key;
  
  if (params) {
    Object.entries(params).forEach(([paramKey, paramValue]) => {
      value = value.replace(new RegExp(`{{${paramKey}}}`, "g"), String(paramValue));
    });
  }
  
  return value;
}

export function getCurrentLanguage(): string {
  return currentLanguage;
}

export function getUserPreferences(): UserPreferences | null {
  return userPreferences;
}

export function setUserPreferences(prefs: UserPreferences) {
  userPreferences = prefs;
  currentLanguage = prefs.languageCode;
  loadTranslations(prefs.languageCode);
}

export function formatCurrency(amount: number, currencyCode?: string): string {
  const code = currencyCode || userPreferences?.currencyCode || "INR";
  const locale = userPreferences?.numberFormat || "en-IN";
  
  try {
    return new Intl.NumberFormat(locale, {
      style: "currency",
      currency: code,
    }).format(amount);
  } catch {
    return `${code} ${amount.toFixed(2)}`;
  }
}

export function formatDate(date: Date | string, format?: string): string {
  const d = typeof date === "string" ? new Date(date) : date;
  const locale = userPreferences?.numberFormat || "en-IN";
  const dateFormat = format || userPreferences?.dateFormat || "dd/MM/yyyy";
  
  try {
    if (dateFormat === "dd/MM/yyyy") {
      return new Intl.DateTimeFormat("en-GB", {
        day: "2-digit",
        month: "2-digit",
        year: "numeric",
      }).format(d);
    }
    return new Intl.DateTimeFormat(locale, {
      day: "2-digit",
      month: "2-digit",
      year: "numeric",
    }).format(d);
  } catch {
    return d.toLocaleDateString();
  }
}

export function formatNumber(number: number): string {
  const locale = userPreferences?.numberFormat || "en-IN";
  
  try {
    return new Intl.NumberFormat(locale).format(number);
  } catch {
    return number.toString();
  }
}

export function formatDateTime(dateTime: Date | string): string {
  const d = typeof dateTime === "string" ? new Date(dateTime) : dateTime;
  const locale = userPreferences?.numberFormat || "en-IN";
  const timeFormat = userPreferences?.timeFormat || "24h";
  
  try {
    const options: Intl.DateTimeFormatOptions = {
      day: "2-digit",
      month: "2-digit",
      year: "numeric",
      hour: "2-digit",
      minute: "2-digit",
    };
    
    if (timeFormat === "12h") {
      options.hour12 = true;
    } else {
      options.hour12 = false;
    }
    
    return new Intl.DateTimeFormat(locale, options).format(d);
  } catch {
    return d.toLocaleString();
  }
}

export function isRTL(): boolean {
  // This would be determined from the language settings
  // For now, return false as we need to check the language
  return false;
}

