# Test script to verify quotations endpoint is working after DateTime fix
Write-Host "Testing Quotations API Endpoint..." -ForegroundColor Green

# Test the quotations list endpoint with date filters (this was failing before)
try {
    Write-Host "Testing GET /api/v1/quotations with date filters..." -ForegroundColor Yellow
    $response = Invoke-RestMethod -Uri "http://localhost:5000/api/v1/quotations?pageNumber=1&pageSize=10&dateFrom=2025-11-07" -Method GET -Headers @{
        "Authorization" = "Bearer YOUR_JWT_TOKEN_HERE"
        "Content-Type" = "application/json"
    } -ErrorAction Stop
    
    Write-Host "✅ Quotations endpoint working: $($response | ConvertTo-Json -Depth 2)" -ForegroundColor Green
} catch {
    Write-Host "❌ Quotations endpoint failed: $($_.Exception.Message)" -ForegroundColor Red
    if ($_.Exception.Response) {
        Write-Host "Status Code: $($_.Exception.Response.StatusCode)" -ForegroundColor Red
    }
    if ($_.ErrorDetails) {
        Write-Host "Error Details: $($_.ErrorDetails.Message)" -ForegroundColor Red
    }
}

# Test without date filters
try {
    Write-Host "Testing GET /api/v1/quotations without date filters..." -ForegroundColor Yellow
    $response = Invoke-RestMethod -Uri "http://localhost:5000/api/v1/quotations?pageNumber=1&pageSize=10" -Method GET -Headers @{
        "Authorization" = "Bearer YOUR_JWT_TOKEN_HERE"
        "Content-Type" = "application/json"
    } -ErrorAction Stop
    
    Write-Host "✅ Quotations endpoint (no dates) working: $($response | ConvertTo-Json -Depth 2)" -ForegroundColor Green
} catch {
    Write-Host "❌ Quotations endpoint (no dates) failed: $($_.Exception.Message)" -ForegroundColor Red
    if ($_.Exception.Response) {
        Write-Host "Status Code: $($_.Exception.Response.StatusCode)" -ForegroundColor Red
    }
    if ($_.ErrorDetails) {
        Write-Host "Error Details: $($_.ErrorDetails.Message)" -ForegroundColor Red
    }
}

Write-Host "Quotations endpoint testing completed." -ForegroundColor Green