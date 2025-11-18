"use client";

import React, { createContext, useContext, useState, useEffect, ReactNode } from "react";
import { LocalizationApi } from "../lib/api";
import { getUserPreferences, setUserPreferences, loadTranslations } from "../lib/i18n";
import { getAccessToken } from "../lib/session";
import type { UserPreferences, Currency, SupportedLanguage } from "../types/localization";

interface LocaleContextType {
  preferences: UserPreferences | null;
  currency: Currency | null;
  language: SupportedLanguage | null;
  formatCurrency: (amount: number, currencyCode?: string) => string;
  formatDate: (date: Date | string, format?: string) => string;
  formatNumber: (number: number) => string;
  formatDateTime: (dateTime: Date | string) => string;
  changeLanguage: (languageCode: string) => Promise<void>;
  changeCurrency: (currencyCode: string) => Promise<void>;
  updatePreferences: (prefs: Partial<UserPreferences>) => Promise<void>;
  loading: boolean;
}

const LocaleContext = createContext<LocaleContextType | undefined>(undefined);

export const LocaleProvider: React.FC<{ children: ReactNode }> = ({ children }) => {
  const [preferences, setPreferences] = useState<UserPreferences | null>(null);
  const [currency, setCurrency] = useState<Currency | null>(null);
  const [language, setLanguage] = useState<SupportedLanguage | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    loadLocaleData();
  }, []);

  async function loadLocaleData() {
    // Only fetch if user is authenticated
    if (!getAccessToken()) {
      // Use default preferences when not authenticated
      const defaultPrefs: UserPreferences = {
        userId: "",
        languageCode: "en",
        currencyCode: "INR",
        dateFormat: "dd/MM/yyyy",
        timeFormat: "24h",
        numberFormat: "en-IN",
        timeZone: "Asia/Kolkata",
      };
      setPreferences(defaultPrefs);
      setUserPreferences(defaultPrefs);
      setLoading(false);
      return;
    }

    try {
      setLoading(true);
      const prefs = await LocalizationApi.getUserPreferences(true);
      setPreferences(prefs);
      setUserPreferences(prefs);

      // Load currency
      if (prefs.currencyCode) {
        const currencies = await LocalizationApi.getSupportedCurrencies();
        const curr = currencies.find(c => c.currencyCode === prefs.currencyCode);
        setCurrency(curr || null);
      }

      // Load language
      const languages = await LocalizationApi.getSupportedLanguages();
      const lang = languages.find(l => l.languageCode === prefs.languageCode);
      setLanguage(lang || null);

      // Load translations
      await loadTranslations(prefs.languageCode);
    } catch (error: any) {
      // Silently ignore 401 errors (user not logged in)
      if (error?.message?.includes("401")) {
        // Use default preferences
        const defaultPrefs: UserPreferences = {
          userId: "",
          languageCode: "en",
          currencyCode: "INR",
          dateFormat: "dd/MM/yyyy",
          timeFormat: "24h",
          numberFormat: "en-IN",
          timeZone: "Asia/Kolkata",
        };
        setPreferences(defaultPrefs);
        setUserPreferences(defaultPrefs);
      } else {
        console.error("Failed to load locale data", error);
      }
    } finally {
      setLoading(false);
    }
  }

  function formatCurrency(amount: number, currencyCode?: string): string {
    const code = currencyCode || preferences?.currencyCode || "INR";
    const locale = preferences?.numberFormat || "en-IN";
    
    try {
      return new Intl.NumberFormat(locale, {
        style: "currency",
        currency: code,
        minimumFractionDigits: 2,
        maximumFractionDigits: 2,
      }).format(amount);
    } catch {
      return `${code} ${amount.toFixed(2)}`;
    }
  }

  function formatDate(date: Date | string, format?: string): string {
    const d = typeof date === "string" ? new Date(date) : date;
    const locale = preferences?.numberFormat || "en-IN";
    const dateFormat = format || preferences?.dateFormat || "dd/MM/yyyy";

    try {
      if (dateFormat === "dd/MM/yyyy") {
        return new Intl.DateTimeFormat("en-GB", {
          day: "2-digit",
          month: "2-digit",
          year: "numeric",
        }).format(d);
      } else if (dateFormat === "MM/dd/yyyy") {
        return new Intl.DateTimeFormat("en-US", {
          day: "2-digit",
          month: "2-digit",
          year: "numeric",
        }).format(d);
      } else if (dateFormat === "yyyy-MM-dd") {
        return new Intl.DateTimeFormat("en-CA", {
          year: "numeric",
          month: "2-digit",
          day: "2-digit",
        }).format(d);
      }
      return new Intl.DateTimeFormat(locale).format(d);
    } catch {
      return d.toLocaleDateString();
    }
  }

  function formatNumber(number: number): string {
    const locale = preferences?.numberFormat || "en-IN";
    
    try {
      return new Intl.NumberFormat(locale, {
        minimumFractionDigits: 2,
        maximumFractionDigits: 2,
      }).format(number);
    } catch {
      return number.toFixed(2);
    }
  }

  function formatDateTime(dateTime: Date | string): string {
    const dt = typeof dateTime === "string" ? new Date(dateTime) : dateTime;
    const locale = preferences?.numberFormat || "en-IN";
    const timeFormat = preferences?.timeFormat || "24h";

    try {
      const options: Intl.DateTimeFormatOptions = {
        year: "numeric",
        month: "2-digit",
        day: "2-digit",
        hour: "2-digit",
        minute: "2-digit",
        hour12: timeFormat === "12h",
      };
      return new Intl.DateTimeFormat(locale, options).format(dt);
    } catch {
      return dt.toLocaleString();
    }
  }

  async function changeLanguage(languageCode: string): Promise<void> {
    try {
      await LocalizationApi.updateUserPreferences({ languageCode });
      await loadTranslations(languageCode);
      const updated = await LocalizationApi.getUserPreferences(true);
      setPreferences(updated);
      setUserPreferences(updated);
      
      const languages = await LocalizationApi.getSupportedLanguages();
      const lang = languages.find(l => l.languageCode === languageCode);
      setLanguage(lang || null);

      // Reload page if RTL changes
      if (lang?.isRtl !== language?.isRtl) {
        window.location.reload();
      }
    } catch (error) {
      console.error("Failed to change language", error);
      throw error;
    }
  }

  async function changeCurrency(currencyCode: string): Promise<void> {
    try {
      await LocalizationApi.updateUserPreferences({ currencyCode });
      const updated = await LocalizationApi.getUserPreferences(true);
      setPreferences(updated);
      setUserPreferences(updated);
      
      const currencies = await LocalizationApi.getSupportedCurrencies();
      const curr = currencies.find(c => c.currencyCode === currencyCode);
      setCurrency(curr || null);
    } catch (error) {
      console.error("Failed to change currency", error);
      throw error;
    }
  }

  async function updatePreferences(prefs: Partial<UserPreferences>): Promise<void> {
    try {
      const updated = await LocalizationApi.updateUserPreferences(prefs);
      setPreferences(updated);
      setUserPreferences(updated);
      
      if (prefs.currencyCode) {
        const currencies = await LocalizationApi.getSupportedCurrencies();
        const curr = currencies.find(c => c.currencyCode === prefs.currencyCode);
        setCurrency(curr || null);
      }
      
      if (prefs.languageCode) {
        await loadTranslations(prefs.languageCode);
        const languages = await LocalizationApi.getSupportedLanguages();
        const lang = languages.find(l => l.languageCode === prefs.languageCode);
        setLanguage(lang || null);
      }
    } catch (error) {
      console.error("Failed to update preferences", error);
      throw error;
    }
  }

  return (
    <LocaleContext.Provider
      value={{
        preferences,
        currency,
        language,
        formatCurrency,
        formatDate,
        formatNumber,
        formatDateTime,
        changeLanguage,
        changeCurrency,
        updatePreferences,
        loading,
      }}
    >
      {children}
    </LocaleContext.Provider>
  );
};

export const useLocale = (): LocaleContextType => {
  const context = useContext(LocaleContext);
  if (context === undefined) {
    throw new Error("useLocale must be used within a LocaleProvider");
  }
  return context;
};

