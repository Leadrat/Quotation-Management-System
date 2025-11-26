# Test script to verify notification endpoints are working
# This script tests the notification API endpoints to ensure MediatR dependency resolution is working

Write-Host "Testing Notification API Endpoints..." -ForegroundColor Green

# Test the unread count endpoint (this was failing before)
try {
    Write-Host "Testing GET /api/v1/notifications/unread-count..." -ForegroundColor Yellow
    $response = Invoke-RestMethod -Uri "http://localhost:5000/api/v1/notifications/unread-count" -Method GET -Headers @{
        "Authorization" = "Bearer YOUR_JWT_TOKEN_HERE"
        "Content-Type" = "application/json"
    } -ErrorAction Stop
    
    Write-Host "✅ Unread count endpoint working: $($response | ConvertTo-Json)" -ForegroundColor Green
} catch {
    Write-Host "❌ Unread count endpoint failed: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "Status Code: $($_.Exception.Response.StatusCode)" -ForegroundColor Red
}

# Test the notifications list endpoint
try {
    Write-Host "Testing GET /api/v1/notifications..." -ForegroundColor Yellow
    $response = Invoke-RestMethod -Uri "http://localhost:5000/api/v1/notifications?pageNumber=1&pageSize=10" -Method GET -Headers @{
        "Authorization" = "Bearer YOUR_JWT_TOKEN_HERE"
        "Content-Type" = "application/json"
    } -ErrorAction Stop
    
    Write-Host "✅ Notifications list endpoint working: $($response | ConvertTo-Json)" -ForegroundColor Green
} catch {
    Write-Host "❌ Notifications list endpoint failed: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "Status Code: $($_.Exception.Response.StatusCode)" -ForegroundColor Red
}

# Test the new preferences endpoint
try {
    Write-Host "Testing GET /api/v1/notifications/preferences..." -ForegroundColor Yellow
    $response = Invoke-RestMethod -Uri "http://localhost:5000/api/v1/notifications/preferences" -Method GET -Headers @{
        "Authorization" = "Bearer YOUR_JWT_TOKEN_HERE"
        "Content-Type" = "application/json"
    } -ErrorAction Stop
    
    Write-Host "✅ Preferences endpoint working: $($response | ConvertTo-Json)" -ForegroundColor Green
} catch {
    Write-Host "❌ Preferences endpoint failed: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "Status Code: $($_.Exception.Response.StatusCode)" -ForegroundColor Red
}

# Test updating preferences
try {
    Write-Host "Testing PUT /api/v1/notifications/preferences..." -ForegroundColor Yellow
    $preferencesData = @{
        emailNotifications = $true
        smsNotifications = $false
        inAppNotifications = $true
        notificationTypes = @(
            @{ type = "QuotationSent"; enabled = $true; channels = @("EMAIL", "IN_APP") }
        )
    } | ConvertTo-Json -Depth 3
    
    $response = Invoke-RestMethod -Uri "http://localhost:5000/api/v1/notifications/preferences" -Method PUT -Headers @{
        "Authorization" = "Bearer YOUR_JWT_TOKEN_HERE"
        "Content-Type" = "application/json"
    } -Body $preferencesData -ErrorAction Stop
    
    Write-Host "✅ Update preferences endpoint working: $($response | ConvertTo-Json)" -ForegroundColor Green
} catch {
    Write-Host "❌ Update preferences endpoint failed: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "Status Code: $($_.Exception.Response.StatusCode)" -ForegroundColor Red
}

Write-Host "Notification endpoint testing completed." -ForegroundColor Green