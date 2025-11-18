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
    } catch (error) {
      console.error("Failed to load languages", error);
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

