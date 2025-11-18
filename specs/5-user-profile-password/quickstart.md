# Quickstart: Spec-005 User Profile & Password

## Prereqs
- Obtain a JWT access token via login.
- For admin reset, the caller must be in role Admin.

## Update Profile
```bash
curl -X PUT "https://api.crm.local/api/v1/users/{userId}/profile" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "firstName": "Jane",
    "lastName": "Doe",
    "phoneCode": "+1",
    "mobile": "+12025550123"
  }'
```

## Change Password (Self)
```bash
curl -X POST "https://api.crm.local/api/v1/auth/change-password" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "currentPassword": "OldPass!23",
    "newPassword": "NewPass!23",
    "confirmPassword": "NewPass!23"
  }'
```

## Admin Reset Password (Enqueue One-Time Link)
```bash
curl -X POST "https://api.crm.local/api/v1/users/{userId}/reset-password" \
  -H "Authorization: Bearer $ADMIN_TOKEN"
```

Notes:
- Reset link expires in 24h and is single-use.
- After password change, all refresh tokens are invalidated.
