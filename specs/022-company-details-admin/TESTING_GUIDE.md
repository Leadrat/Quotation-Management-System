# Testing Guide: Company Details Feature

This guide provides step-by-step testing instructions for the Company Details Admin Configuration feature.

## Prerequisites

- ✅ Migrations applied (see APPLY_MIGRATIONS.md)
- ✅ Backend API running
- ✅ Frontend application running
- ✅ Admin user account created
- ✅ At least one client created (for quotation testing)

## Test Scenarios

### Scenario 1: Admin Configuration - Initial Setup

**Objective**: Verify admin can configure company details for the first time.

**Steps:**
1. Login as admin user
2. Navigate to Admin → Company Details
3. Verify page loads with empty form
4. Fill in the following:
   - **Tax Information:**
     - PAN: `ABCDE1234F`
     - TAN: `ABCD12345E`
     - GST: `27ABCDE1234F1Z5`
   - **Company Information:**
     - Company Name: `Test Company Ltd`
     - Address: `123 Business Street`
     - City: `Mumbai`
     - State: `Maharashtra`
     - Postal Code: `400001`
     - Country: `India`
   - **Contact Information:**
     - Email: `contact@testcompany.com`
     - Phone: `+91-22-12345678`
     - Website: `https://www.testcompany.com`
5. Click "Save Company Details"
6. Confirm in modal
7. **Expected**: Success message appears, form shows saved data

**Validation:**
- ✅ Form saves successfully
- ✅ Success notification appears
- ✅ Data persists after page refresh
- ✅ No validation errors for valid inputs

---

### Scenario 2: Tax Number Validation

**Objective**: Verify tax number format validation.

**Steps:**
1. Navigate to Company Details page
2. Enter invalid PAN: `ABC123` (too short)
3. Click "Save Company Details"
4. **Expected**: Validation error for PAN format

5. Enter invalid TAN: `ABCD123` (wrong format)
6. Click "Save Company Details"
7. **Expected**: Validation error for TAN format

8. Enter invalid GST: `27ABCDE1234F` (too short)
9. Click "Save Company Details"
10. **Expected**: Validation error for GST format

11. Enter valid tax numbers:
    - PAN: `ABCDE1234F`
    - TAN: `ABCD12345E`
    - GST: `27ABCDE1234F1Z5`
12. Click "Save Company Details"
13. **Expected**: Saves successfully

**Validation:**
- ✅ Invalid formats are rejected
- ✅ Valid formats are accepted
- ✅ Error messages are clear and helpful

---

### Scenario 3: Bank Details - India

**Objective**: Verify India bank details configuration.

**Steps:**
1. Navigate to Company Details page
2. Scroll to "Bank Details" section
3. Select "India" from country dropdown
4. Click "Add Bank Details"
5. Fill in:
   - Account Number: `1234567890`
   - IFSC Code: `HDFC0001234`
   - Bank Name: `HDFC Bank`
   - Branch Name: `Mumbai Branch`
6. Click "Save Company Details"
7. **Expected**: India bank details saved

**Validation:**
- ✅ India bank details section appears
- ✅ IFSC Code field is required
- ✅ IBAN and SWIFT fields are hidden
- ✅ Data saves correctly

---

### Scenario 4: Bank Details - Dubai

**Objective**: Verify Dubai bank details configuration.

**Steps:**
1. Navigate to Company Details page
2. Scroll to "Bank Details" section
3. Select "Dubai" from country dropdown
4. Click "Add Bank Details"
5. Fill in:
   - Account Number: `9876543210`
   - IBAN: `AE070331234567890123456`
   - SWIFT Code: `HDFCINBB`
   - Bank Name: `HDFC Bank Dubai`
   - Branch Name: `Dubai Branch`
6. Click "Save Company Details"
7. **Expected**: Dubai bank details saved

**Validation:**
- ✅ Dubai bank details section appears
- ✅ IBAN and SWIFT Code fields are required
- ✅ IFSC Code field is hidden
- ✅ Data saves correctly

---

### Scenario 5: Bank Details - Both Countries

**Objective**: Verify both India and Dubai bank details can coexist.

**Steps:**
1. Navigate to Company Details page
2. Add India bank details (from Scenario 3)
3. Add Dubai bank details (from Scenario 4)
4. Click "Save Company Details"
5. **Expected**: Both bank details saved

6. Refresh page
7. **Expected**: Both bank details sections visible with correct data

**Validation:**
- ✅ Both countries' bank details can be configured
- ✅ Each country shows appropriate fields
- ✅ Data persists correctly

---

### Scenario 6: Logo Upload

**Objective**: Verify logo upload functionality.

**Steps:**
1. Navigate to Company Details page
2. Scroll to "Logo Upload" section
3. Click "Choose File" and select a valid image (PNG, JPG, SVG, WEBP)
4. Verify preview appears
5. Click "Upload Logo"
6. **Expected**: Logo uploads successfully, preview updates

7. Try uploading invalid file type (e.g., .txt)
8. **Expected**: Error message about invalid file type

9. Try uploading file > 5MB
10. **Expected**: Error message about file size limit

**Validation:**
- ✅ Valid image files upload successfully
- ✅ Preview shows uploaded logo
- ✅ Invalid file types are rejected
- ✅ File size limit enforced
- ✅ Logo appears in saved company details

---

### Scenario 7: Quotation PDF Integration - India Client

**Objective**: Verify company details appear in quotation PDF for Indian client.

**Steps:**
1. Ensure company details are configured (from Scenario 1)
2. Ensure India bank details are configured (from Scenario 3)
3. Create a quotation for an Indian client (client with Indian address/state code)
4. Download quotation PDF
5. **Expected**: PDF contains:
   - Company logo (if uploaded)
   - Company name and address
   - Tax information (PAN, TAN, GST)
   - Contact information
   - **India bank details** (IFSC Code visible)
   - Legal disclaimer

**Validation:**
- ✅ Company details appear in PDF
- ✅ India bank details shown (not Dubai)
- ✅ Logo appears if uploaded
- ✅ All company information is accurate

---

### Scenario 8: Quotation PDF Integration - Dubai Client

**Objective**: Verify company details appear in quotation PDF for Dubai client.

**Steps:**
1. Ensure company details are configured
2. Ensure Dubai bank details are configured
3. Create a quotation for a Dubai/UAE client
4. Download quotation PDF
5. **Expected**: PDF contains:
   - Company logo (if uploaded)
   - Company name and address
   - Tax information
   - Contact information
   - **Dubai bank details** (IBAN and SWIFT visible, not IFSC)
   - Legal disclaimer

**Validation:**
- ✅ Company details appear in PDF
- ✅ Dubai bank details shown (not India)
- ✅ IBAN and SWIFT Code visible
- ✅ IFSC Code not visible

---

### Scenario 9: Quotation Email Integration

**Objective**: Verify company details appear in quotation emails.

**Steps:**
1. Ensure company details are configured
2. Create a quotation
3. Send quotation via email
4. Check email content
5. **Expected**: Email contains:
   - Company name in subject
   - Company footer with address
   - Contact information
   - Tax information
   - Bank details
   - Legal disclaimer

**Validation:**
- ✅ Company name in email subject
- ✅ Company details in email footer
- ✅ All information is accurate

---

### Scenario 10: Historical Accuracy

**Objective**: Verify old quotations preserve original company details.

**Steps:**
1. Configure company details (e.g., Company Name: "Old Company")
2. Create Quotation #1
3. Update company details (e.g., Company Name: "New Company")
4. Create Quotation #2
5. Download PDF for Quotation #1
6. **Expected**: PDF shows "Old Company"
7. Download PDF for Quotation #2
8. **Expected**: PDF shows "New Company"

**Validation:**
- ✅ Old quotations preserve original company details
- ✅ New quotations use updated company details
- ✅ Historical accuracy maintained

---

### Scenario 11: Cache Invalidation

**Objective**: Verify cache is invalidated on updates.

**Steps:**
1. Get company details via API (should be cached)
2. Update company details via API
3. Get company details again immediately
4. **Expected**: Updated data returned (not cached old data)

**Validation:**
- ✅ Cache is invalidated on update
- ✅ Fresh data is retrieved after update

---

### Scenario 12: Error Handling

**Objective**: Verify graceful error handling.

**Steps:**
1. Try to access Company Details page as non-admin user
2. **Expected**: Redirected or access denied

3. Try to save with invalid data
4. **Expected**: Validation errors shown

5. Try to upload invalid logo file
6. **Expected**: Error message shown

7. Create quotation without company details configured
8. **Expected**: Quotation created (with warning logged), PDF works without company details

**Validation:**
- ✅ Access control enforced
- ✅ Validation errors are clear
- ✅ System handles missing data gracefully

---

## API Testing (Postman/curl)

### Test 1: Get Company Details

```bash
curl -X GET "http://localhost:5000/api/v1/company-details" \
  -H "Authorization: Bearer {token}"
```

### Test 2: Update Company Details

```bash
curl -X PUT "http://localhost:5000/api/v1/company-details" \
  -H "Authorization: Bearer {token}" \
  -H "Content-Type: application/json" \
  -d '{
    "panNumber": "ABCDE1234F",
    "companyName": "Test Company",
    "bankDetails": [
      {
        "country": "India",
        "accountNumber": "1234567890",
        "ifscCode": "HDFC0001234",
        "bankName": "HDFC Bank"
      }
    ]
  }'
```

### Test 3: Upload Logo

```bash
curl -X POST "http://localhost:5000/api/v1/company-details/logo" \
  -H "Authorization: Bearer {token}" \
  -F "file=@logo.png"
```

---

## Checklist

- [ ] Admin can access Company Details page
- [ ] Company details can be saved
- [ ] Tax number validation works
- [ ] India bank details can be configured
- [ ] Dubai bank details can be configured
- [ ] Both countries' bank details can coexist
- [ ] Logo upload works
- [ ] Company details appear in quotation PDFs
- [ ] Country-specific bank details appear correctly
- [ ] Company details appear in quotation emails
- [ ] Historical accuracy maintained
- [ ] Cache invalidation works
- [ ] Error handling is graceful
- [ ] Access control enforced

---

## Troubleshooting

### Issue: Company details not appearing in PDF
- **Check**: Is company details configured?
- **Check**: Is CompanyDetailsSnapshot column populated in Quotations table?
- **Solution**: Configure company details first, then create new quotations

### Issue: Wrong bank details in PDF
- **Check**: Client's country is correctly set
- **Check**: Bank details for that country are configured
- **Solution**: Verify client country and ensure bank details exist for that country

### Issue: Logo not appearing
- **Check**: Logo file was uploaded successfully
- **Check**: LogoUrl is set in company details
- **Check**: File path is accessible
- **Solution**: Re-upload logo and verify LogoUrl is saved

---

**Testing completed by**: _______________  
**Date**: _______________  
**Status**: ☐ Passed  ☐ Failed  ☐ Needs Review

