# Spec-017: User Guide - Multi-Currency, Multi-Language & Localization

## Overview

This guide explains how to use the multi-currency and multi-language features in the CRM system.

---

## Changing Your Language

### Method 1: Using the Language Selector

1. Look for the language selector in the header (top right of the page)
2. Click on the dropdown menu
3. Select your preferred language (e.g., English, Hindi, Arabic)
4. The page will reload with your selected language

### Method 2: Using Preferences Page

1. Navigate to **Settings** → **Preferences**
2. Find the **Language** section
3. Select your preferred language from the dropdown
4. Click **Save Preferences**
5. The page will reload with your selected language

**Note**: Some languages (like Arabic) use Right-to-Left (RTL) layout, which will automatically be applied.

---

## Changing Your Currency

### Method 1: Using the Currency Selector

1. Look for the currency selector in the header (next to the language selector)
2. Click on the dropdown menu
3. Select your preferred currency (e.g., USD, INR, EUR)
4. All monetary values will be displayed in your selected currency

### Method 2: Using Preferences Page

1. Navigate to **Settings** → **Preferences**
2. Find the **Currency** section
3. Select your preferred currency from the dropdown
4. Click **Save Preferences**
5. All monetary values will update to your selected currency

**Note**: Currency conversion uses the latest exchange rates configured by administrators.

---

## Setting Date and Time Formats

1. Navigate to **Settings** → **Preferences**
2. Configure the following:
   - **Date Format**: Choose from dd/MM/yyyy, MM/dd/yyyy, or yyyy-MM-dd
   - **Time Format**: Choose 24-hour or 12-hour (AM/PM)
   - **Number Format Locale**: Enter locale code (e.g., en-IN, en-US)
   - **Timezone**: Enter your timezone (e.g., Asia/Kolkata, America/New_York)
   - **First Day of Week**: Choose Sunday or Monday
3. Click **Save Preferences**
4. View the **Format Preview** section to see how your settings affect formatting

---

## Viewing Formatted Values

After setting your preferences, you'll see:

- **Currency amounts** formatted with your selected currency symbol and locale
- **Dates** formatted according to your date format preference
- **Numbers** formatted with locale-specific separators (commas, periods)
- **Date and time** combined according to your preferences

---

## For Administrators

### Managing Localization Resources

1. Navigate to **Admin** → **Localization**
2. Select a language from the dropdown
3. View, edit, or delete translation resources
4. Click on any resource value to edit it inline
5. Use the **Add Resource** button to create new translations
6. Filter by category or search by key/value

### Managing Currencies

1. Navigate to **Admin** → **Currencies**
2. Click the **Currencies** tab
3. View all supported currencies
4. Click **Add Currency** to create a new currency
5. Set a currency as default using the **Set Default** button

### Managing Exchange Rates

1. Navigate to **Admin** → **Currencies**
2. Click the **Exchange Rates** tab
3. View all exchange rates
4. Click **Add Exchange Rate** to create a new rate
5. Enter:
   - From Currency
   - To Currency
   - Rate (e.g., 1 USD = 83.25 INR)
   - Effective Date

**Note**: Exchange rates should be updated regularly to ensure accurate currency conversions.

---

## Tips and Best Practices

1. **Language Selection**: Choose a language you're comfortable with. The system will remember your preference.

2. **Currency Selection**: Select the currency you primarily work with. All quotations, payments, and reports will display in this currency.

3. **Date Formats**: Choose a format that matches your regional conventions:
   - **dd/MM/yyyy**: Common in India, UK, and many countries
   - **MM/dd/yyyy**: Common in the United States
   - **yyyy-MM-dd**: ISO format, common in technical contexts

4. **Timezone**: Set your timezone to ensure all timestamps are displayed in your local time.

5. **Format Preview**: Use the format preview on the preferences page to see how your settings will affect the display.

---

## Troubleshooting

### Language not changing
- Clear your browser cache
- Ensure you clicked **Save Preferences**
- Check if the language is active (contact administrator)

### Currency not displaying correctly
- Verify the currency is active
- Check if exchange rates are configured
- Contact administrator if conversion seems incorrect

### Dates not formatting correctly
- Verify your date format preference is saved
- Check your browser's locale settings
- Try a different date format

### Missing translations
- Some UI elements may still show in English if translations are missing
- Contact your administrator to add missing translations

---

## Support

For issues or questions:
1. Check this guide first
2. Contact your system administrator
3. Submit a support ticket if available

---

**Last Updated**: 2025-01-XX

