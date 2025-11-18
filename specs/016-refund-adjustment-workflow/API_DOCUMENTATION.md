# Spec-016: Refund & Adjustment Workflow - API Documentation

## Overview

This document provides comprehensive API documentation for the Refund & Adjustment Workflow system (Spec-016).

## Base URL

```
/api/refunds
/api/adjustments
```

## Authentication

All endpoints require JWT authentication. Include the token in the Authorization header:

```
Authorization: Bearer <token>
```

---

## Refunds API

### 1. Create Refund Request

**Endpoint:** `POST /api/refunds`

**Description:** Initiates a new refund request for a payment.

**Request Body:**
```json
{
  "paymentId": "guid",
  "quotationId": "guid",
  "refundAmount": 1000.00,
  "refundReason": "Client request",
  "refundReasonCode": "CLIENT_REQUEST",
  "comments": "Optional comments"
}
```

**Response:** `200 OK`
```json
{
  "isSuccess": true,
  "data": {
    "refundId": "guid",
    "paymentId": "guid",
    "quotationId": "guid",
    "refundAmount": 1000.00,
    "refundStatus": "Pending",
    "refundReason": "Client request",
    "refundReasonCode": "CLIENT_REQUEST",
    "requestDate": "2024-01-15T10:30:00Z",
    "requestedByUserId": "guid",
    "requestedByUserName": "John Doe"
  }
}
```

**Error Responses:**
- `400 Bad Request`: Invalid request data
- `404 Not Found`: Payment not found
- `400 Bad Request`: Refund amount exceeds available amount

---

### 2. Get Refund by ID

**Endpoint:** `GET /api/refunds/{refundId}`

**Description:** Retrieves refund details by ID.

**Response:** `200 OK`
```json
{
  "isSuccess": true,
  "data": {
    "refundId": "guid",
    "paymentId": "guid",
    "quotationId": "guid",
    "refundAmount": 1000.00,
    "refundStatus": "Approved",
    "refundReason": "Client request",
    "refundReasonCode": "CLIENT_REQUEST",
    "requestDate": "2024-01-15T10:30:00Z",
    "approvalDate": "2024-01-15T11:00:00Z",
    "approvedByUserName": "Manager Name",
    "comments": "Approved for processing"
  }
}
```

---

### 3. Get Pending Refunds

**Endpoint:** `GET /api/refunds/pending`

**Query Parameters:**
- `approvalLevel` (optional): Filter by approval level (Auto, Manager, Admin)

**Response:** `200 OK`
```json
{
  "isSuccess": true,
  "data": [
    {
      "refundId": "guid",
      "refundAmount": 1000.00,
      "refundStatus": "Pending",
      "approvalLevel": "Manager",
      "requestedByUserName": "John Doe",
      "requestDate": "2024-01-15T10:30:00Z"
    }
  ]
}
```

---

### 4. Approve Refund

**Endpoint:** `POST /api/refunds/{refundId}/approve`

**Description:** Approves a pending refund request.

**Request Body:**
```json
{
  "comments": "Approved for processing"
}
```

**Response:** `200 OK`
```json
{
  "isSuccess": true,
  "data": {
    "refundId": "guid",
    "refundStatus": "Approved",
    "approvalDate": "2024-01-15T11:00:00Z"
  }
}
```

**Error Responses:**
- `404 Not Found`: Refund not found
- `400 Bad Request`: Refund is not in Pending status
- `403 Forbidden`: User does not have approval permissions

---

### 5. Reject Refund

**Endpoint:** `POST /api/refunds/{refundId}/reject`

**Description:** Rejects a pending refund request.

**Request Body:**
```json
{
  "rejectionReason": "Does not meet refund policy",
  "comments": "Optional comments"
}
```

**Response:** `200 OK`
```json
{
  "isSuccess": true,
  "data": {
    "refundId": "guid",
    "refundStatus": "Rejected",
    "rejectionReason": "Does not meet refund policy"
  }
}
```

---

### 6. Process Refund

**Endpoint:** `POST /api/refunds/{refundId}/process`

**Description:** Processes an approved refund through the payment gateway.

**Response:** `200 OK`
```json
{
  "isSuccess": true,
  "data": {
    "refundId": "guid",
    "refundStatus": "Processing",
    "gatewayRefundId": "gateway_refund_123"
  }
}
```

---

### 7. Get Refund Timeline

**Endpoint:** `GET /api/refunds/{refundId}/timeline`

**Description:** Retrieves the complete timeline of events for a refund.

**Response:** `200 OK`
```json
{
  "isSuccess": true,
  "data": [
    {
      "timelineId": "guid",
      "refundId": "guid",
      "eventType": "REQUESTED",
      "eventDate": "2024-01-15T10:30:00Z",
      "actedByUserId": "guid",
      "actedByUserName": "John Doe",
      "comments": "Refund requested"
    },
    {
      "timelineId": "guid",
      "eventType": "APPROVED",
      "eventDate": "2024-01-15T11:00:00Z",
      "actedByUserName": "Manager Name",
      "comments": "Approved"
    }
  ]
}
```

---

### 8. Get Refunds by Payment

**Endpoint:** `GET /api/refunds/payment/{paymentId}`

**Description:** Retrieves all refunds for a specific payment.

**Response:** `200 OK`
```json
{
  "isSuccess": true,
  "data": [
    {
      "refundId": "guid",
      "refundAmount": 1000.00,
      "refundStatus": "Completed",
      "requestDate": "2024-01-15T10:30:00Z"
    }
  ]
}
```

---

### 9. Reverse Refund

**Endpoint:** `POST /api/refunds/{refundId}/reverse`

**Description:** Reverses a completed refund.

**Request Body:**
```json
{
  "reason": "Refund was processed in error",
  "comments": "Optional comments"
}
```

**Response:** `200 OK`
```json
{
  "isSuccess": true,
  "data": {
    "refundId": "guid",
    "refundStatus": "Reversed"
  }
}
```

---

### 10. Bulk Process Refunds

**Endpoint:** `POST /api/refunds/bulk-process`

**Description:** Processes multiple approved refunds in bulk.

**Request Body:**
```json
{
  "refundIds": ["guid1", "guid2", "guid3"]
}
```

**Response:** `200 OK`
```json
{
  "isSuccess": true,
  "data": {
    "processed": 2,
    "failed": 1,
    "results": [
      {
        "refundId": "guid1",
        "status": "Processing",
        "success": true
      }
    ]
  }
}
```

---

## Adjustments API

### 1. Create Adjustment Request

**Endpoint:** `POST /api/adjustments`

**Description:** Creates a new adjustment request for a quotation.

**Request Body:**
```json
{
  "quotationId": "guid",
  "adjustmentType": "DISCOUNT_CHANGE",
  "originalAmount": 5000.00,
  "adjustedAmount": 4500.00,
  "reason": "Discount adjustment"
}
```

**Response:** `200 OK`
```json
{
  "isSuccess": true,
  "data": {
    "adjustmentId": "guid",
    "quotationId": "guid",
    "adjustmentType": "DISCOUNT_CHANGE",
    "originalAmount": 5000.00,
    "adjustedAmount": 4500.00,
    "adjustmentDifference": -500.00,
    "status": "PENDING",
    "reason": "Discount adjustment",
    "requestDate": "2024-01-15T10:30:00Z"
  }
}
```

---

### 2. Get Adjustment by ID

**Endpoint:** `GET /api/adjustments/{adjustmentId}`

**Description:** Retrieves adjustment details by ID.

**Response:** `200 OK`
```json
{
  "isSuccess": true,
  "data": {
    "adjustmentId": "guid",
    "quotationId": "guid",
    "adjustmentType": "DISCOUNT_CHANGE",
    "originalAmount": 5000.00,
    "adjustedAmount": 4500.00,
    "adjustmentDifference": -500.00,
    "status": "APPROVED",
    "reason": "Discount adjustment",
    "requestDate": "2024-01-15T10:30:00Z",
    "approvalDate": "2024-01-15T11:00:00Z"
  }
}
```

---

### 3. Get Adjustments by Quotation

**Endpoint:** `GET /api/adjustments/quotation/{quotationId}`

**Description:** Retrieves all adjustments for a specific quotation.

**Response:** `200 OK`
```json
{
  "isSuccess": true,
  "data": [
    {
      "adjustmentId": "guid",
      "adjustmentType": "DISCOUNT_CHANGE",
      "originalAmount": 5000.00,
      "adjustedAmount": 4500.00,
      "status": "APPLIED",
      "requestDate": "2024-01-15T10:30:00Z"
    }
  ]
}
```

---

### 4. Approve Adjustment

**Endpoint:** `POST /api/adjustments/{adjustmentId}/approve`

**Description:** Approves a pending adjustment request.

**Request Body:**
```json
{
  "comments": "Approved"
}
```

**Response:** `200 OK`
```json
{
  "isSuccess": true,
  "data": {
    "adjustmentId": "guid",
    "status": "APPROVED",
    "approvalDate": "2024-01-15T11:00:00Z"
  }
}
```

---

### 5. Reject Adjustment

**Endpoint:** `POST /api/adjustments/{adjustmentId}/reject`

**Description:** Rejects a pending adjustment request.

**Request Body:**
```json
{
  "rejectionReason": "Invalid adjustment",
  "comments": "Optional comments"
}
```

**Response:** `200 OK`
```json
{
  "isSuccess": true,
  "data": {
    "adjustmentId": "guid",
    "status": "REJECTED"
  }
}
```

---

### 6. Apply Adjustment

**Endpoint:** `POST /api/adjustments/{adjustmentId}/apply`

**Description:** Applies an approved adjustment to the quotation.

**Response:** `200 OK`
```json
{
  "isSuccess": true,
  "data": {
    "adjustmentId": "guid",
    "status": "APPLIED",
    "appliedDate": "2024-01-15T12:00:00Z",
    "quotation": {
      "quotationId": "guid",
      "totalAmount": 4500.00,
      "updatedAt": "2024-01-15T12:00:00Z"
    }
  }
}
```

---

## Enums

### RefundStatus
- `Pending`: Refund request is pending approval
- `Approved`: Refund has been approved
- `Processing`: Refund is being processed through gateway
- `Completed`: Refund has been successfully processed
- `Failed`: Refund processing failed
- `Rejected`: Refund request was rejected
- `Reversed`: Refund has been reversed

### RefundReasonCode
- `CLIENT_REQUEST`: Client requested refund
- `ERROR`: Processing error
- `DISCOUNT_ADJUSTMENT`: Discount adjustment
- `CANCELLATION`: Service cancellation
- `DUPLICATE_PAYMENT`: Duplicate payment
- `OTHER`: Other reason

### AdjustmentType
- `DISCOUNT_CHANGE`: Discount percentage change
- `AMOUNT_CORRECTION`: Amount correction
- `TAX_CORRECTION`: Tax correction

### AdjustmentStatus
- `PENDING`: Adjustment request is pending approval
- `APPROVED`: Adjustment has been approved
- `REJECTED`: Adjustment request was rejected
- `APPLIED`: Adjustment has been applied to quotation

---

## Error Responses

All endpoints may return the following error responses:

**400 Bad Request:**
```json
{
  "isSuccess": false,
  "errors": [
    {
      "field": "refundAmount",
      "message": "Refund amount must be greater than 0"
    }
  ]
}
```

**401 Unauthorized:**
```json
{
  "isSuccess": false,
  "message": "Unauthorized"
}
```

**403 Forbidden:**
```json
{
  "isSuccess": false,
  "message": "You do not have permission to perform this action"
}
```

**404 Not Found:**
```json
{
  "isSuccess": false,
  "message": "Resource not found"
}
```

**500 Internal Server Error:**
```json
{
  "isSuccess": false,
  "message": "An error occurred while processing your request"
}
```

---

## Rate Limiting

- Refund requests: 10 per minute per user
- Adjustment requests: 20 per minute per user
- Bulk operations: 5 per hour per user

---

## Webhooks

### Refund Webhook Events

The system sends webhook events for refund status changes:

**Event:** `refund.completed`
```json
{
  "event": "refund.completed",
  "refundId": "guid",
  "paymentId": "guid",
  "refundAmount": 1000.00,
  "gatewayRefundId": "gateway_refund_123",
  "timestamp": "2024-01-15T12:00:00Z"
}
```

**Event:** `refund.failed`
```json
{
  "event": "refund.failed",
  "refundId": "guid",
  "paymentId": "guid",
  "error": "Gateway error message",
  "timestamp": "2024-01-15T12:00:00Z"
}
```

---

## Notes

1. All monetary amounts are in the base currency (INR by default)
2. All timestamps are in UTC
3. Refund amounts cannot exceed the available refundable amount (PaymentAmount - RefundedAmount)
4. Adjustments automatically recalculate taxes when applied
5. Approval workflows depend on refund/adjustment amounts and user roles

