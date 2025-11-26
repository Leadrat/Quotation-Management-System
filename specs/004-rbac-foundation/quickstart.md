# Quickstart: Spec-004 Role Management & RBAC Foundation

Date: 2025-11-12

## Prerequisites
- API running with JWT auth (Spec-003 complete)
- Admin user credentials available
- Environment:
  - API_BASE=https://api.crm.com
  - Obtain Admin access token via /api/v1/auth/login

## Admin Login (retrieve tokens)

```bash
curl -s -X POST "$API_BASE/api/v1/auth/login" \
  -H 'Content-Type: application/json' \
  -d '{"email":"admin@crm.com","password":"Admin@123"}' | jq .
# Save access token
export ACCESS=$(curl -s -X POST "$API_BASE/api/v1/auth/login" -H 'Content-Type: application/json' -d '{"email":"admin@crm.com","password":"Admin@123"}' | jq -r .accessToken)
```

## List Roles (Admin only)

```bash
curl -s "$API_BASE/api/v1/roles?pageNumber=1&pageSize=10" \
  -H "Authorization: Bearer $ACCESS" | jq .
```

## Get Role by Id (Admin only)

```bash
ADMIN_ROLE_ID=AA668EE7-79E9-4AF3-B3ED-1A47F104B8EA
curl -s "$API_BASE/api/v1/roles/$ADMIN_ROLE_ID" \
  -H "Authorization: Bearer $ACCESS" | jq .
```

## Create Custom Role (Admin only)

```bash
curl -s -X POST "$API_BASE/api/v1/roles" \
  -H 'Content-Type: application/json' \
  -H "Authorization: Bearer $ACCESS" \
  -d '{"roleName":"Support","description":"Support staff"}' | jq .
```

## Update Custom Role (Admin only)

```bash
CUSTOM_ID="<role-guid>"
curl -s -X PUT "$API_BASE/api/v1/roles/$CUSTOM_ID" \
  -H 'Content-Type: application/json' \
  -H "Authorization: Bearer $ACCESS" \
  -d '{"description":"Updated description","isActive":true}' | jq .
```

## Delete (Soft) Custom Role (Admin only)

```bash
curl -s -X DELETE "$API_BASE/api/v1/roles/$CUSTOM_ID" \
  -H "Authorization: Bearer $ACCESS" | jq .
```

## Notes
- Built-in roles (Admin, Manager, SalesRep, Client) are immutable (cannot be deleted/renamed/deactivated).
- Deactivate/Delete for custom roles requires zero active users assigned; reassign first.
- Pagination defaults: pageNumber=1, pageSize=10 (max 100).
- JWT includes `role` and `role_id` claims.
