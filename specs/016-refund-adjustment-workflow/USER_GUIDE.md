# Spec-016: Refund & Adjustment Workflow - User Guide

## Overview

This guide explains how to use the Refund & Adjustment Workflow system in the CRM application. The system allows you to process refunds for payments and make adjustments to quotations.

---

## Table of Contents

1. [Refunds](#refunds)
   - [Requesting a Refund](#requesting-a-refund)
   - [Approving/Rejecting Refunds](#approvingrejecting-refunds)
   - [Processing Refunds](#processing-refunds)
   - [Viewing Refund Status](#viewing-refund-status)
2. [Adjustments](#adjustments)
   - [Creating an Adjustment](#creating-an-adjustment)
   - [Approving/Rejecting Adjustments](#approvingrejecting-adjustments)
   - [Applying Adjustments](#applying-adjustments)
3. [Client Portal](#client-portal)
   - [Requesting Refunds](#requesting-refunds-as-a-client)
4. [Best Practices](#best-practices)

---

## Refunds

### Requesting a Refund

**Who can request:** Sales reps, Managers, Admins, Clients (via portal)

**Steps:**

1. Navigate to the **Payments** page or **Quotation Details** page
2. Find the payment you want to refund
3. Click **"Request Refund"** button
4. Fill in the refund form:
   - **Refund Amount**: Enter the amount to refund (leave empty for full refund)
   - **Refund Reason**: Provide a clear reason for the refund
   - **Reason Code**: Select the appropriate reason code:
     - Client Request
     - Error
     - Discount Adjustment
     - Cancellation
     - Duplicate Payment
     - Other
   - **Comments**: Add any additional comments (optional)
5. Click **"Submit Refund Request"**

**Note:** The refund amount cannot exceed the available refundable amount (Payment Amount - Already Refunded Amount).

---

### Approving/Rejecting Refunds

**Who can approve:** Managers, Admins (based on approval level)

**Approval Levels:**
- **Auto**: Automatically approved (for small amounts)
- **Manager**: Requires manager approval
- **Admin**: Requires admin approval

**Steps to Approve:**

1. Navigate to **Refunds** → **Pending Approvals**
2. Review the refund request details
3. Click **"Review"** on the refund you want to approve
4. In the approval dialog:
   - Click **"Approve"**
   - Add optional comments
   - Click **"Confirm Approval"**

**Steps to Reject:**

1. Navigate to **Refunds** → **Pending Approvals**
2. Click **"Review"** on the refund you want to reject
3. In the approval dialog:
   - Click **"Reject"**
   - Enter a **Rejection Reason** (required)
   - Add optional comments
   - Click **"Confirm Rejection"**

---

### Processing Refunds

**Who can process:** Finance team, Admins

**Steps:**

1. Navigate to **Refunds** → **Approved**
2. Find the approved refund you want to process
3. Click **"Process Refund"**
4. The system will:
   - Process the refund through the payment gateway
   - Update the payment's refunded amount
   - Send notifications to relevant parties

**Note:** Processing may take a few minutes depending on the payment gateway.

---

### Viewing Refund Status

**Refund Statuses:**
- **Pending**: Awaiting approval
- **Approved**: Approved and ready to process
- **Processing**: Being processed through gateway
- **Completed**: Successfully refunded
- **Failed**: Processing failed (check error details)
- **Rejected**: Request was rejected
- **Reversed**: Refund has been reversed

**Viewing Timeline:**

1. Navigate to **Refunds** → Click on a refund
2. View the **Timeline** section to see all events:
   - Request date and requester
   - Approval/rejection date and approver
   - Processing status updates
   - Completion date

---

## Adjustments

### Creating an Adjustment

**Who can create:** Sales reps, Managers, Admins

**Steps:**

1. Navigate to **Quotations** → Select a quotation
2. Scroll to the **Adjustments** section
3. Click **"Request Adjustment"**
4. Fill in the adjustment form:
   - **Adjustment Type**: Select type:
     - Discount Change
     - Amount Correction
     - Tax Correction
   - **Original Amount**: Current amount
   - **Adjusted Amount**: New amount
   - **Reason**: Provide a clear reason
5. Click **"Submit Adjustment Request"**

**Note:** Adjustments automatically recalculate taxes when applied.

---

### Approving/Rejecting Adjustments

**Who can approve:** Managers, Admins

**Steps:**

1. Navigate to **Quotations** → Select a quotation with pending adjustments
2. In the **Adjustments** section, find the pending adjustment
3. Click **"Approve"** or **"Reject"**
4. If rejecting, provide a rejection reason

---

### Applying Adjustments

**Who can apply:** Sales reps, Managers, Admins

**Steps:**

1. Navigate to **Quotations** → Select a quotation
2. In the **Adjustments** section, find an approved adjustment
3. Click **"Apply Adjustment"**
4. The system will:
   - Update the quotation amounts
   - Recalculate taxes (if applicable)
   - Update the quotation total

**Note:** Once applied, adjustments cannot be reversed. Ensure accuracy before applying.

---

## Client Portal

### Requesting Refunds as a Client

**Steps:**

1. Open the quotation link sent to you
2. If the quotation is accepted and payment is successful, you'll see a **"Payment Information"** section
3. Click **"Request Refund"** button
4. Fill in the refund form:
   - **Refund Amount**: Enter amount (or leave empty for full refund)
   - **Refund Reason**: Explain why you need a refund
   - **Reason Code**: Select appropriate code
   - **Comments**: Add any additional information
5. Click **"Submit Refund Request"**

**Note:** Your refund request will be reviewed by the company. You'll receive notifications about the status.

---

## Best Practices

### For Refunds

1. **Always provide clear reasons**: Help approvers understand why a refund is needed
2. **Check payment status**: Ensure payment is successful before requesting refund
3. **Verify amounts**: Double-check refund amounts before submitting
4. **Follow up**: Check refund status regularly and follow up if processing is delayed
5. **Document everything**: Add comments for future reference

### For Adjustments

1. **Verify calculations**: Ensure adjusted amounts are correct
2. **Review tax impact**: Understand how adjustments affect taxes
3. **Get approval early**: Request adjustments before sending quotations to clients
4. **Document reasons**: Always provide clear reasons for adjustments
5. **Test before applying**: Review the adjustment preview before applying

### General

1. **Use appropriate reason codes**: Select the most accurate reason code
2. **Add comments**: Provide context in comments fields
3. **Monitor timelines**: Check approval timelines regularly
4. **Communicate**: Notify relevant parties about refunds/adjustments
5. **Audit trail**: All actions are logged for audit purposes

---

## Common Scenarios

### Scenario 1: Client Requests Full Refund

1. Client submits refund request via portal
2. Manager reviews and approves
3. Finance processes refund
4. Client receives refund notification

### Scenario 2: Discount Adjustment After Quotation Sent

1. Sales rep creates adjustment request
2. Manager approves adjustment
3. Sales rep applies adjustment to quotation
4. Updated quotation is sent to client

### Scenario 3: Partial Refund for Service Cancellation

1. Sales rep requests partial refund
2. Manager approves (amount within threshold)
3. Finance processes refund
4. Payment status updated to "Partially Refunded"

---

## Troubleshooting

### Refund Status Stuck on "Processing"

- Check payment gateway status
- Verify gateway credentials
- Contact support if issue persists

### Adjustment Not Applying

- Ensure adjustment is approved
- Check quotation is not locked
- Verify user has permissions

### Cannot Request Refund

- Verify payment status is "Success"
- Check available refundable amount
- Ensure payment is not already fully refunded

---

## Support

For issues or questions:
- Contact your system administrator
- Check the audit trail for detailed logs
- Review the API documentation for technical details

---

## Glossary

- **Refund**: Return of payment to client
- **Adjustment**: Modification to quotation amounts
- **Approval Level**: Required approval authority (Auto/Manager/Admin)
- **Refundable Amount**: Payment amount minus already refunded amount
- **Timeline**: History of all events for a refund/adjustment

