# Quickstart: Clients API (Spec-006)

Base URL: https://api.crm.com/api/v1
Auth: Bearer JWT (SalesRep or Admin)

## List clients (paginated)

```bash
curl -s -H "Authorization: Bearer $TOKEN" \
  "$BASE/clients?pageNumber=1&pageSize=10"
```

## Get client by id

```bash
curl -s -H "Authorization: Bearer $TOKEN" \
  "$BASE/clients/$CLIENT_ID"
```

## Create client

```bash
curl -s -X POST -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "companyName": "XYZ Technologies",
    "contactName": "Rajesh Verma",
    "email": "rajesh.verma@xyz.com",
    "mobile": "+919876543211",
    "phoneCode": "+91",
    "gstin": "06ABCDE1234H1Z1",
    "stateCode": "06",
    "address": "456 Tech Park, New Delhi",
    "city": "New Delhi",
    "state": "Delhi",
    "pinCode": "110001"
  }' \
  "$BASE/clients"
```

## Update client

```bash
curl -s -X PUT -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "contactName": "New Contact",
    "email": "new.email@xyz.com"
  }' \
  "$BASE/clients/$CLIENT_ID"
```

## Delete client (soft delete)

```bash
curl -s -X DELETE -H "Authorization: Bearer $TOKEN" \
  "$BASE/clients/$CLIENT_ID"
```

Notes:
- Emails are stored lowercase and must be unique among active clients.
- Pagination defaults: pageSize=10 (max 100); out-of-range clamped to valid values.
- GSTIN is required for India B2B clients; optional otherwise.
