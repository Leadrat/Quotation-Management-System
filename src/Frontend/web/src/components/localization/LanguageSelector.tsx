"use client";

import { useState, useEffect } from "react";
import { LocalizationApi } from "../../lib/api";
import { formatCurrency, loadTranslations, setUserPreferences, getUserPreferences } from "../../lib/i18n";
import type { SupportedLanguage } from "../../types/localization";

export function LanguageSelector() {
  const [languages, setLanguages] = useState<SupportedLanguage[]>([]);
  const [currentLang, setCurrentLang] = useState<string>("en");
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    loadLanguages();
    const prefs = getUserPreferences();
    if (prefs) {
      setCurrentLang(prefs.languageCode);
    }
  }, []);

  async function loadLanguages() {
    try {
      const langs = await LocalizationApi.getSupportedLanguages();
      setLanguages(langs.filter(l => l.isActive));
    } catch (error: any) {
      console.error("Failed to load languages", error);
      // If API base URL is not configured, silently fail and use default language
      if (error?.message?.includes("No such host") || error?.message?.includes("API URL cannot be empty")) {
        console.warn("API base URL not configured. Please set NEXT_PUBLIC_API_BASE_URL environment variable.");
      }
      // Set empty languages array so component doesn't break
      setLanguages([]);
    } finally {
      setLoading(false);
    }
  }

  async function handleLanguageChange(languageCode: string) {
    try {
      const prefs = getUserPreferences();
      if (prefs) {
        await LocalizationApi.updateUserPreferences({ languageCode });
        await loadTranslations(languageCode);
        const updatedPrefs = await LocalizationApi.getUserPreferences();
        setUserPreferences(updatedPrefs);
        setCurrentLang(languageCode);
        window.location.reload(); // Reload to apply RTL if needed
      }
    } catch (error) {
      console.error("Failed to change language", error);
    }
  }

  if (loading) {
    return <div>Loading...</div>;
  }

  // If no languages loaded (e.g., API not configured), show default English option
  if (languages.length === 0) {
    return (
      <select
        value={currentLang}
        onChange={(e) => handleLanguageChange(e.target.value)}
        className="px-3 py-2 border rounded-md"
        disabled
      >
        <option value="en">English</option>
      </select>
    );
  }

  return (
    <select
      value={currentLang}
      onChange={(e) => handleLanguageChange(e.target.value)}
      className="px-3 py-2 border rounded-md"
    >
      {languages.map((lang) => (
        <option key={lang.languageCode} value={lang.languageCode}>
          {lang.displayNameEn}
        </option>
      ))}
    </select>
  );
}

