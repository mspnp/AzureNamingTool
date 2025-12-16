# Test script for Bulk Resource Name Generation API
# This script tests the /api/v2/ResourceNamingRequests/GenerateBulk endpoint

param(
    [Parameter(Mandatory=$false)]
    [string]$ApiKey = "YourAPIKeyHere",
    
    [Parameter(Mandatory=$false)]
    [string]$BaseUrl = "http://localhost:5222"
)

$headers = @{
    "APIKey" = $ApiKey
    "Content-Type" = "application/json"
}

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Bulk API Endpoint Tests" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Test 1: All Success - Generate names for multiple resource types
Write-Host "Test 1: All Success - Multiple resource types with shared components" -ForegroundColor Yellow
$test1Body = @{
    resourceTypes = @("rg", "vnet", "nsg")
    resourceLocation = "use"
    resourceInstance = "001"
    continueOnError = $true
    validateOnly = $false
    createdBy = "API-Test"
} | ConvertTo-Json

try {
    $response1 = Invoke-RestMethod -Uri "$BaseUrl/api/v2/ResourceNamingRequests/GenerateBulk" `
        -Method Post `
        -Headers $headers `
        -Body $test1Body
    
    Write-Host "✓ Success!" -ForegroundColor Green
    Write-Host "Total Requested: $($response1.data.totalRequested)" -ForegroundColor White
    Write-Host "Success Count: $($response1.data.successCount)" -ForegroundColor Green
    Write-Host "Failure Count: $($response1.data.failureCount)" -ForegroundColor $(if($response1.data.failureCount -eq 0){"Green"}else{"Red"})
    Write-Host "Generated Names:" -ForegroundColor White
    foreach ($result in $response1.data.results) {
        if ($result.success) {
            Write-Host "  - $($result.resourceType): $($result.resourceName)" -ForegroundColor Green
        } else {
            Write-Host "  - $($result.resourceType): FAILED - $($result.errorMessage)" -ForegroundColor Red
        }
    }
} catch {
    Write-Host "✗ Failed: $($_.Exception.Message)" -ForegroundColor Red
    if ($_.ErrorDetails.Message) {
        Write-Host "Details: $($_.ErrorDetails.Message)" -ForegroundColor Red
    }
}
Write-Host ""

# Test 2: Partial Success - One invalid resource type
Write-Host "Test 2: Partial Success - Mix of valid and invalid resource types" -ForegroundColor Yellow
$test2Body = @{
    resourceTypes = @("rg", "invalid-type", "vnet")
    resourceLocation = "usw"
    resourceInstance = "002"
    continueOnError = $true
    validateOnly = $false
    createdBy = "API-Test"
} | ConvertTo-Json

try {
    $response2 = Invoke-RestMethod -Uri "$BaseUrl/api/v2/ResourceNamingRequests/GenerateBulk" `
        -Method Post `
        -Headers $headers `
        -Body $test2Body
    
    Write-Host "✓ Request completed!" -ForegroundColor Yellow
    Write-Host "Total Requested: $($response2.data.totalRequested)" -ForegroundColor White
    Write-Host "Success Count: $($response2.data.successCount)" -ForegroundColor Green
    Write-Host "Failure Count: $($response2.data.failureCount)" -ForegroundColor Red
    Write-Host "Results:" -ForegroundColor White
    foreach ($result in $response2.data.results) {
        if ($result.success) {
            Write-Host "  - $($result.resourceType): $($result.resourceName)" -ForegroundColor Green
        } else {
            Write-Host "  - $($result.resourceType): FAILED - $($result.errorMessage)" -ForegroundColor Red
        }
    }
} catch {
    Write-Host "✗ Failed: $($_.Exception.Message)" -ForegroundColor Red
    if ($_.ErrorDetails.Message) {
        Write-Host "Details: $($_.ErrorDetails.Message)" -ForegroundColor Red
    }
}
Write-Host ""

# Test 3: Resource Type Overrides
Write-Host "Test 3: Resource Type Overrides - Different instance per resource type" -ForegroundColor Yellow
$test3Body = @{
    resourceTypes = @("rg", "vnet", "nsg")
    resourceLocation = "use"
    resourceInstance = "001"
    resourceTypeOverrides = @{
        "vnet" = @{
            resourceInstance = "002"
        }
        "nsg" = @{
            resourceInstance = "003"
            resourceLocation = "usw"
        }
    }
    continueOnError = $true
    validateOnly = $false
    createdBy = "API-Test"
} | ConvertTo-Json -Depth 5

try {
    $response3 = Invoke-RestMethod -Uri "$BaseUrl/api/v2/ResourceNamingRequests/GenerateBulk" `
        -Method Post `
        -Headers $headers `
        -Body $test3Body
    
    Write-Host "✓ Success!" -ForegroundColor Green
    Write-Host "Generated Names with Overrides:" -ForegroundColor White
    foreach ($result in $response3.data.results) {
        if ($result.success) {
            Write-Host "  - $($result.resourceType): $($result.resourceName)" -ForegroundColor Green
        } else {
            Write-Host "  - $($result.resourceType): FAILED - $($result.errorMessage)" -ForegroundColor Red
        }
    }
} catch {
    Write-Host "✗ Failed: $($_.Exception.Message)" -ForegroundColor Red
    if ($_.ErrorDetails.Message) {
        Write-Host "Details: $($_.ErrorDetails.Message)" -ForegroundColor Red
    }
}
Write-Host ""

# Test 4: Validate Only Mode
Write-Host "Test 4: Validate Only - Generate without persisting" -ForegroundColor Yellow
$test4Body = @{
    resourceTypes = @("rg", "vnet")
    resourceLocation = "usc"
    resourceInstance = "999"
    continueOnError = $true
    validateOnly = $true
    createdBy = "API-Test"
} | ConvertTo-Json

try {
    $response4 = Invoke-RestMethod -Uri "$BaseUrl/api/v2/ResourceNamingRequests/GenerateBulk" `
        -Method Post `
        -Headers $headers `
        -Body $test4Body
    
    Write-Host "✓ Validation completed!" -ForegroundColor Green
    Write-Host "Validated Names (not persisted):" -ForegroundColor White
    foreach ($result in $response4.data.results) {
        if ($result.success) {
            Write-Host "  - $($result.resourceType): $($result.resourceName)" -ForegroundColor Cyan
        }
    }
} catch {
    Write-Host "✗ Failed: $($_.Exception.Message)" -ForegroundColor Red
}
Write-Host ""

# Test 5: ContinueOnError = false
Write-Host "Test 5: Stop on First Error - ContinueOnError = false" -ForegroundColor Yellow
$test5Body = @{
    resourceTypes = @("rg", "invalid-type", "vnet")
    resourceLocation = "usw"
    resourceInstance = "001"
    continueOnError = $false
    validateOnly = $false
    createdBy = "API-Test"
} | ConvertTo-Json

try {
    $response5 = Invoke-RestMethod -Uri "$BaseUrl/api/v2/ResourceNamingRequests/GenerateBulk" `
        -Method Post `
        -Headers $headers `
        -Body $test5Body
    
    Write-Host "Request completed" -ForegroundColor Yellow
    Write-Host "Processing stopped at first error:" -ForegroundColor White
    Write-Host "Total Requested: $($response5.data.totalRequested)" -ForegroundColor White
    Write-Host "Processed: $($response5.data.results.Count)" -ForegroundColor White
    foreach ($result in $response5.data.results) {
        if ($result.success) {
            Write-Host "  - $($result.resourceType): $($result.resourceName)" -ForegroundColor Green
        } else {
            Write-Host "  - $($result.resourceType): FAILED - $($result.errorMessage)" -ForegroundColor Red
        }
    }
} catch {
    Write-Host "✗ Failed: $($_.Exception.Message)" -ForegroundColor Red
}
Write-Host ""

# Test 6: Validation Error - Empty resource types
Write-Host "Test 6: Validation - Empty resource types list" -ForegroundColor Yellow
$test6Body = @{
    resourceTypes = @()
    resourceLocation = "use"
    resourceInstance = "001"
    createdBy = "API-Test"
} | ConvertTo-Json

try {
    $response6 = Invoke-RestMethod -Uri "$BaseUrl/api/v2/ResourceNamingRequests/GenerateBulk" `
        -Method Post `
        -Headers $headers `
        -Body $test6Body
    
    Write-Host "✗ Should have failed validation!" -ForegroundColor Red
} catch {
    if ($_.Exception.Response.StatusCode -eq 400) {
        Write-Host "✓ Correctly rejected with 400 Bad Request" -ForegroundColor Green
        if ($_.ErrorDetails.Message) {
            $errorResponse = $_.ErrorDetails.Message | ConvertFrom-Json
            Write-Host "Error Message: $($errorResponse.error.message)" -ForegroundColor Yellow
        }
    } else {
        Write-Host "✗ Unexpected error: $($_.Exception.Message)" -ForegroundColor Red
    }
}
Write-Host ""

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Tests Complete!" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
