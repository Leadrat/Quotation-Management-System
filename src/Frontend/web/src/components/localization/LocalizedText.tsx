"use client";

import { useEffect, useState } from "react";
import { LocalizationApi } from "../../lib/api";
import { getUserPreferences } from "../../lib/i18n";

interface LocalizedTextProps {
  resourceKey: string;
  fallback?: string;
  languageCode?: string;
  className?: string;
}

export function LocalizedText({ 
  resourceKey, 
  fallback, 
  languageCode,
  className 
}: LocalizedTextProps) {
  const [text, setText] = useState<string>(fallback || resourceKey);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    loadTranslation();
  }, [resourceKey, languageCode]);

  async function loadTranslation() {
    try {
      const prefs = getUserPreferences();
      const lang = languageCode || prefs?.languageCode || "en";
      
      const resources = await LocalizationApi.getLocalizationResources(lang);
      const resourceValue = resources[resourceKey];
      
      if (resourceValue) {
        setText(resourceValue);
      } else if (fallback) {
        setText(fallback);
      } else {
        setText(resourceKey);
      }
    } catch (error) {
      console.error("Failed to load translation", error);
      setText(fallback || resourceKey);
    } finally {
      setLoading(false);
    }
  }

  if (loading && !fallback) {
    return <span className={className}>{resourceKey}</span>;
  }

  return <span className={className}>{text}</span>;
}

