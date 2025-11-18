"use client";

import { useState, useEffect } from "react";
import { LocalizationApi } from "../../lib/api";
import { setUserPreferences, getUserPreferences } from "../../lib/i18n";
import type { Currency } from "../../types/localization";

export function CurrencySelector() {
  const [currencies, setCurrencies] = useState<Currency[]>([]);
  const [currentCurrency, setCurrentCurrency] = useState<string>("INR");
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    loadCurrencies();
    const prefs = getUserPreferences();
    if (prefs?.currencyCode) {
      setCurrentCurrency(prefs.currencyCode);
    }
  }, []);

  async function loadCurrencies() {
    try {
      const curr = await LocalizationApi.getSupportedCurrencies();
      setCurrencies(curr.filter(c => c.isActive));
    } catch (error) {
      console.error("Failed to load currencies", error);
    } finally {
      setLoading(false);
    }
  }

  async function handleCurrencyChange(currencyCode: string) {
    try {
      const prefs = getUserPreferences();
      if (prefs) {
        await LocalizationApi.updateUserPreferences({ currencyCode });
        const updatedPrefs = await LocalizationApi.getUserPreferences();
        setUserPreferences(updatedPrefs);
        setCurrentCurrency(currencyCode);
      }
    } catch (error) {
      console.error("Failed to change currency", error);
    }
  }

  if (loading) {
    return <div>Loading...</div>;
  }

  return (
    <select
      value={currentCurrency}
      onChange={(e) => handleCurrencyChange(e.target.value)}
      className="px-3 py-2 border rounded-md"
    >
      {currencies.map((curr) => (
        <option key={curr.currencyCode} value={curr.currencyCode}>
          {curr.symbol} {curr.displayName}
        </option>
      ))}
    </select>
  );
}

