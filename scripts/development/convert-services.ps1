# Script to help identify service conversion targets
# This helps us understand which services follow similar patterns

param(
    [string]$servicesPath = "..\..\src\Services"
)

# Services that need conversion (excluding already done ones)
$servicesToConvert = @(
    "ResourceFunctionService.cs",
    "ResourceLocationService.cs",
    "ResourceOrgService.cs",
    "ResourceProjAppSvcService.cs",
    "ResourceUnitDeptService.cs",
    "ResourceTypeService.cs",
    "ResourceComponentService.cs",
    "CustomComponentService.cs",
    "GeneratedNamesService.cs",
    "AdminService.cs",
    "AdminUserService.cs",
    "ImportExportService.cs",
    "PolicyService.cs",
    "ResourceNamingRequestService.cs"
)

Write-Host "Services remaining to convert: $($servicesToConvert.Count)"
foreach ($service in $servicesToConvert) {
    $filePath = Join-Path $servicesPath $service
    if (Test-Path $filePath) {
        $content = Get-Content $filePath -Raw
        $hasGetItems = $content -match "public static async Task<ServiceResponse> GetItems\(\)"
        $hasGetItem = $content -match "public static async Task<ServiceResponse> GetItem\(int id\)"
        $hasPostItem = $content -match "public static async Task<ServiceResponse> PostItem\("
        $hasDeleteItem = $content -match "public static async Task<ServiceResponse> DeleteItem\("
        $hasPostConfig = $content -match "public static async Task<ServiceResponse> PostConfig\("
        
        Write-Host "`n$service - Methods:"
        Write-Host "  GetItems: $hasGetItems"
        Write-Host "  GetItem: $hasGetItem"
        Write-Host "  PostItem: $hasPostItem"
        Write-Host "  DeleteItem: $hasDeleteItem"
        Write-Host "  PostConfig: $hasPostConfig"
    }
}
