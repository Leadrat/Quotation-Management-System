"use client";

import { useState, useEffect } from "react";
import { LocalizationApi } from "../../../../lib/api";
import type { Currency, ExchangeRate, CreateCurrencyRequest, UpdateExchangeRateRequest } from "../../../../types/localization";

export default function AdminCurrenciesPage() {
  const [currencies, setCurrencies] = useState<Currency[]>([]);
  const [exchangeRates, setExchangeRates] = useState<ExchangeRate[]>([]);
  const [loading, setLoading] = useState(true);
  const [activeTab, setActiveTab] = useState<"currencies" | "rates">("currencies");
  const [showCreateCurrency, setShowCreateCurrency] = useState(false);
  const [showCreateRate, setShowCreateRate] = useState(false);
  const [newCurrency, setNewCurrency] = useState<CreateCurrencyRequest>({
    currencyCode: "",
    displayName: "",
    symbol: "",
    decimalPlaces: 2,
    isDefault: false,
  });
  const [newRate, setNewRate] = useState<UpdateExchangeRateRequest>({
    fromCurrencyCode: "",
    toCurrencyCode: "",
    rate: 0,
    effectiveDate: new Date().toISOString().split("T")[0],
  });

  useEffect(() => {
    loadData();
  }, []);

  async function loadData() {
    try {
      setLoading(true);
      const [curr, rates] = await Promise.all([
        LocalizationApi.getSupportedCurrencies(),
        LocalizationApi.getExchangeRates(),
      ]);
      setCurrencies(curr);
      setExchangeRates(rates);
    } catch (error) {
      console.error("Failed to load data", error);
    } finally {
      setLoading(false);
    }
  }

  async function handleCreateCurrency() {
    try {
      await LocalizationApi.createCurrency(newCurrency);
      setShowCreateCurrency(false);
      setNewCurrency({ currencyCode: "", displayName: "", symbol: "", decimalPlaces: 2, isDefault: false });
      await loadData();
    } catch (error) {
      console.error("Failed to create currency", error);
      alert("Failed to create currency");
    }
  }

  async function handleCreateRate() {
    try {
      await LocalizationApi.updateExchangeRate(newRate);
      setShowCreateRate(false);
      setNewRate({ fromCurrencyCode: "", toCurrencyCode: "", rate: 0, effectiveDate: new Date().toISOString().split("T")[0] });
      await loadData();
    } catch (error) {
      console.error("Failed to create rate", error);
      alert("Failed to create rate");
    }
  }

  async function handleSetDefault(currencyCode: string) {
    try {
      // Update all currencies to set isDefault
      const updates = currencies.map(c => ({
        currencyCode: c.currencyCode,
        displayName: c.displayName,
        symbol: c.symbol,
        decimalPlaces: c.decimalPlaces,
        isDefault: c.currencyCode === currencyCode,
      }));
      
      // Note: This would require a bulk update endpoint or individual updates
      // For now, we'll just show a message
      alert("Default currency update requires backend support");
      await loadData();
    } catch (error) {
      console.error("Failed to set default currency", error);
      alert("Failed to set default currency");
    }
  }

  if (loading) {
    return <div className="p-6">Loading...</div>;
  }

  return (
    <div className="p-6">
      <div className="flex justify-between items-center mb-6">
        <h1 className="text-2xl font-bold">Currency & Exchange Rate Management</h1>
      </div>

      <div className="mb-4 border-b">
        <button
          onClick={() => setActiveTab("currencies")}
          className={`px-4 py-2 ${activeTab === "currencies" ? "border-b-2 border-blue-600 text-blue-600" : "text-gray-600"}`}
        >
          Currencies
        </button>
        <button
          onClick={() => setActiveTab("rates")}
          className={`px-4 py-2 ${activeTab === "rates" ? "border-b-2 border-blue-600 text-blue-600" : "text-gray-600"}`}
        >
          Exchange Rates
        </button>
      </div>

      {activeTab === "currencies" && (
        <div>
          <div className="mb-4">
            <button
              onClick={() => setShowCreateCurrency(true)}
              className="px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700"
            >
              Add Currency
            </button>
          </div>

          <div className="bg-white dark:bg-gray-800 rounded-lg shadow overflow-hidden">
            <table className="min-w-full divide-y divide-gray-200 dark:divide-gray-700">
              <thead className="bg-gray-50 dark:bg-gray-900">
                <tr>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                    Code
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                    Name
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                    Symbol
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                    Decimal Places
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                    Default
                  </th>
                  <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                    Actions
                  </th>
                </tr>
              </thead>
              <tbody className="bg-white dark:bg-gray-800 divide-y divide-gray-200 dark:divide-gray-700">
                {currencies.map((currency) => (
                  <tr key={currency.currencyCode}>
                    <td className="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900 dark:text-white">
                      {currency.currencyCode}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500 dark:text-gray-400">
                      {currency.displayName}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500 dark:text-gray-400">
                      {currency.symbol}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500 dark:text-gray-400">
                      {currency.decimalPlaces}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500 dark:text-gray-400">
                      {currency.isDefault ? (
                        <span className="px-2 py-1 bg-green-100 text-green-800 rounded">Default</span>
                      ) : (
                        <button
                          onClick={() => handleSetDefault(currency.currencyCode)}
                          className="text-blue-600 hover:text-blue-900"
                        >
                          Set Default
                        </button>
                      )}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-right text-sm font-medium">
                      <button className="text-blue-600 hover:text-blue-900 mr-4">Edit</button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      )}

      {activeTab === "rates" && (
        <div>
          <div className="mb-4">
            <button
              onClick={() => setShowCreateRate(true)}
              className="px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700"
            >
              Add Exchange Rate
            </button>
          </div>

          <div className="bg-white dark:bg-gray-800 rounded-lg shadow overflow-hidden">
            <table className="min-w-full divide-y divide-gray-200 dark:divide-gray-700">
              <thead className="bg-gray-50 dark:bg-gray-900">
                <tr>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                    From
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                    To
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                    Rate
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                    Effective Date
                  </th>
                  <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                    Actions
                  </th>
                </tr>
              </thead>
              <tbody className="bg-white dark:bg-gray-800 divide-y divide-gray-200 dark:divide-gray-700">
                {exchangeRates.map((rate) => (
                  <tr key={rate.exchangeRateId}>
                    <td className="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900 dark:text-white">
                      {rate.fromCurrencyCode}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500 dark:text-gray-400">
                      {rate.toCurrencyCode}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500 dark:text-gray-400">
                      {rate.rate.toFixed(4)}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500 dark:text-gray-400">
                      {new Date(rate.effectiveDate).toLocaleDateString()}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-right text-sm font-medium">
                      <button className="text-blue-600 hover:text-blue-900">Edit</button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      )}

      {showCreateCurrency && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
          <div className="bg-white dark:bg-gray-800 p-6 rounded-lg max-w-md w-full">
            <h2 className="text-xl font-bold mb-4">Create Currency</h2>
            <div className="space-y-4">
              <div>
                <label className="block text-sm font-medium mb-2">Currency Code</label>
                <input
                  type="text"
                  value={newCurrency.currencyCode}
                  onChange={(e) => setNewCurrency({ ...newCurrency, currencyCode: e.target.value.toUpperCase() })}
                  className="w-full px-3 py-2 border rounded-md"
                  placeholder="USD"
                />
              </div>
              <div>
                <label className="block text-sm font-medium mb-2">Display Name</label>
                <input
                  type="text"
                  value={newCurrency.displayName}
                  onChange={(e) => setNewCurrency({ ...newCurrency, displayName: e.target.value })}
                  className="w-full px-3 py-2 border rounded-md"
                />
              </div>
              <div>
                <label className="block text-sm font-medium mb-2">Symbol</label>
                <input
                  type="text"
                  value={newCurrency.symbol}
                  onChange={(e) => setNewCurrency({ ...newCurrency, symbol: e.target.value })}
                  className="w-full px-3 py-2 border rounded-md"
                />
              </div>
              <div>
                <label className="block text-sm font-medium mb-2">Decimal Places</label>
                <input
                  type="number"
                  value={newCurrency.decimalPlaces}
                  onChange={(e) => setNewCurrency({ ...newCurrency, decimalPlaces: parseInt(e.target.value) })}
                  className="w-full px-3 py-2 border rounded-md"
                />
              </div>
              <div>
                <label className="flex items-center">
                  <input
                    type="checkbox"
                    checked={newCurrency.isDefault}
                    onChange={(e) => setNewCurrency({ ...newCurrency, isDefault: e.target.checked })}
                    className="mr-2"
                  />
                  Set as Default
                </label>
              </div>
              <div className="flex gap-2">
                <button
                  onClick={handleCreateCurrency}
                  className="flex-1 px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700"
                >
                  Create
                </button>
                <button
                  onClick={() => setShowCreateCurrency(false)}
                  className="flex-1 px-4 py-2 bg-gray-300 text-gray-700 rounded-md hover:bg-gray-400"
                >
                  Cancel
                </button>
              </div>
            </div>
          </div>
        </div>
      )}

      {showCreateRate && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
          <div className="bg-white dark:bg-gray-800 p-6 rounded-lg max-w-md w-full">
            <h2 className="text-xl font-bold mb-4">Create Exchange Rate</h2>
            <div className="space-y-4">
              <div>
                <label className="block text-sm font-medium mb-2">From Currency</label>
                <select
                  value={newRate.fromCurrencyCode}
                  onChange={(e) => setNewRate({ ...newRate, fromCurrencyCode: e.target.value })}
                  className="w-full px-3 py-2 border rounded-md"
                >
                  <option value="">Select...</option>
                  {currencies.map((c) => (
                    <option key={c.currencyCode} value={c.currencyCode}>
                      {c.currencyCode} - {c.displayName}
                    </option>
                  ))}
                </select>
              </div>
              <div>
                <label className="block text-sm font-medium mb-2">To Currency</label>
                <select
                  value={newRate.toCurrencyCode}
                  onChange={(e) => setNewRate({ ...newRate, toCurrencyCode: e.target.value })}
                  className="w-full px-3 py-2 border rounded-md"
                >
                  <option value="">Select...</option>
                  {currencies.map((c) => (
                    <option key={c.currencyCode} value={c.currencyCode}>
                      {c.currencyCode} - {c.displayName}
                    </option>
                  ))}
                </select>
              </div>
              <div>
                <label className="block text-sm font-medium mb-2">Rate</label>
                <input
                  type="number"
                  step="0.0001"
                  value={newRate.rate}
                  onChange={(e) => setNewRate({ ...newRate, rate: parseFloat(e.target.value) })}
                  className="w-full px-3 py-2 border rounded-md"
                />
              </div>
              <div>
                <label className="block text-sm font-medium mb-2">Effective Date</label>
                <input
                  type="date"
                  value={newRate.effectiveDate}
                  onChange={(e) => setNewRate({ ...newRate, effectiveDate: e.target.value })}
                  className="w-full px-3 py-2 border rounded-md"
                />
              </div>
              <div className="flex gap-2">
                <button
                  onClick={handleCreateRate}
                  className="flex-1 px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700"
                >
                  Create
                </button>
                <button
                  onClick={() => setShowCreateRate(false)}
                  className="flex-1 px-4 py-2 bg-gray-300 text-gray-700 rounded-md hover:bg-gray-400"
                >
                  Cancel
                </button>
              </div>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}

