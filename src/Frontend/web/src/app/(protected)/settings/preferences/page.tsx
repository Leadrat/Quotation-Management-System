"use client";

import { useState, useEffect } from "react";
import { LocalizationApi } from "../../../../lib/api";
import { getUserPreferences, setUserPreferences } from "../../../../lib/i18n";
import { useLocale } from "../../../../context/LocaleContext";
import { FormatPreview } from "../../../../components/localization";
import type { UserPreferences, UpdateUserPreferencesRequest, SupportedLanguage, Currency } from "../../../../types/localization";

export default function PreferencesPage() {
  const { preferences: localePrefs, updatePreferences } = useLocale();
  const [preferences, setPreferences] = useState<UserPreferences | null>(localePrefs);
  const [languages, setLanguages] = useState<SupportedLanguage[]>([]);
  const [currencies, setCurrencies] = useState<Currency[]>([]);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);

  useEffect(() => {
    loadData();
  }, []);

  async function loadData() {
    try {
      const [prefs, langs, curr] = await Promise.all([
        LocalizationApi.getUserPreferences(true),
        LocalizationApi.getSupportedLanguages(),
        LocalizationApi.getSupportedCurrencies(),
      ]);
      setPreferences(prefs);
      setLanguages(langs.filter(l => l.isActive));
      setCurrencies(curr.filter(c => c.isActive));
      setUserPreferences(prefs);
    } catch (error) {
      console.error("Failed to load preferences", error);
    } finally {
      setLoading(false);
    }
  }

  async function handleSave() {
    if (!preferences) return;
    
    setSaving(true);
    try {
      const update: UpdateUserPreferencesRequest = {
        languageCode: preferences.languageCode,
        currencyCode: preferences.currencyCode,
        dateFormat: preferences.dateFormat,
        timeFormat: preferences.timeFormat,
        numberFormat: preferences.numberFormat,
        timezone: preferences.timezone,
        firstDayOfWeek: preferences.firstDayOfWeek,
      };
      
      await updatePreferences(update);
      const updated = await LocalizationApi.getUserPreferences(true);
      setPreferences(updated);
      setUserPreferences(updated);
      alert("Preferences saved successfully!");
    } catch (error) {
      console.error("Failed to save preferences", error);
      alert("Failed to save preferences");
    } finally {
      setSaving(false);
    }
  }

  if (loading) {
    return <div className="p-6">Loading...</div>;
  }

  if (!preferences) {
    return <div className="p-6">Failed to load preferences</div>;
  }

  return (
    <div className="p-6 max-w-4xl mx-auto">
      <h1 className="text-2xl font-bold mb-6">User Preferences</h1>
      
      <div className="space-y-6">
        <div>
          <label className="block text-sm font-medium mb-2">Language</label>
          <select
            value={preferences.languageCode}
            onChange={(e) => setPreferences({ ...preferences, languageCode: e.target.value })}
            className="w-full px-3 py-2 border rounded-md"
          >
            {languages.map((lang) => (
              <option key={lang.languageCode} value={lang.languageCode}>
                {lang.displayNameEn}
              </option>
            ))}
          </select>
        </div>

        <div>
          <label className="block text-sm font-medium mb-2">Currency</label>
          <select
            value={preferences.currencyCode || ""}
            onChange={(e) => setPreferences({ ...preferences, currencyCode: e.target.value || undefined })}
            className="w-full px-3 py-2 border rounded-md"
          >
            <option value="">Default</option>
            {currencies.map((curr) => (
              <option key={curr.currencyCode} value={curr.currencyCode}>
                {curr.symbol} {curr.displayName}
              </option>
            ))}
          </select>
        </div>

        <div>
          <label className="block text-sm font-medium mb-2">Date Format</label>
          <input
            type="text"
            value={preferences.dateFormat}
            onChange={(e) => setPreferences({ ...preferences, dateFormat: e.target.value })}
            className="w-full px-3 py-2 border rounded-md"
            placeholder="dd/MM/yyyy"
          />
        </div>

        <div>
          <label className="block text-sm font-medium mb-2">Time Format</label>
          <select
            value={preferences.timeFormat}
            onChange={(e) => setPreferences({ ...preferences, timeFormat: e.target.value })}
            className="w-full px-3 py-2 border rounded-md"
          >
            <option value="24h">24 Hour</option>
            <option value="12h">12 Hour (AM/PM)</option>
          </select>
        </div>

        <div>
          <label className="block text-sm font-medium mb-2">Number Format Locale</label>
          <input
            type="text"
            value={preferences.numberFormat}
            onChange={(e) => setPreferences({ ...preferences, numberFormat: e.target.value })}
            className="w-full px-3 py-2 border rounded-md"
            placeholder="en-IN"
          />
        </div>

        <div>
          <label className="block text-sm font-medium mb-2">Timezone</label>
          <input
            type="text"
            value={preferences.timezone || ""}
            onChange={(e) => setPreferences({ ...preferences, timezone: e.target.value || undefined })}
            className="w-full px-3 py-2 border rounded-md"
            placeholder="Asia/Kolkata"
          />
        </div>

        <div>
          <label className="block text-sm font-medium mb-2">First Day of Week</label>
          <select
            value={preferences.firstDayOfWeek}
            onChange={(e) => setPreferences({ ...preferences, firstDayOfWeek: parseInt(e.target.value) })}
            className="w-full px-3 py-2 border rounded-md"
          >
            <option value="0">Sunday</option>
            <option value="1">Monday</option>
          </select>
        </div>

        <div>
          <button
            onClick={handleSave}
            disabled={saving}
            className="px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700 disabled:opacity-50"
          >
            {saving ? "Saving..." : "Save Preferences"}
          </button>
        </div>

        <FormatPreview />
      </div>
    </div>
  );
}

