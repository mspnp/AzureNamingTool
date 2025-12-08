# Fix CS0168 warnings - Replace "Exception ex" with "Exception" in catch blocks
# This script fixes unused exception variables in catch blocks

$files = @(
    "src\Helpers\CacheHelper.cs",
    "src\Helpers\ValidationHelper.cs",
    "src\Helpers\FileSystemHelper.cs",
    "src\Helpers\ModalHelper.cs",
    "src\Helpers\IdentityHelper.cs",
    "src\Helpers\ConfigurationHelper.cs",
    "src\Helpers\GeneralHelper.cs"
)

foreach ($file in $files) {
    $fullPath = Join-Path $PSScriptRoot $file
    if (Test-Path $fullPath) {
        Write-Host "Processing $file..." -ForegroundColor Cyan
        
        $content = Get-Content $fullPath -Raw
        $originalContent = $content
        
        # Replace "catch (Exception ex)" with "catch (Exception)" when ex is not used
        # Look for catch blocks that only have comments or empty blocks
        $content = $content -replace 'catch \(Exception ex\)\s*\{\s*\}', 'catch (Exception) { }'
        $content = $content -replace 'catch \(Exception ex\)\s*\{\s*(//[^\r\n]*[\r\n]+\s*)*\}', 'catch (Exception) {$1}'
        
        if ($content -ne $originalContent) {
            Set-Content -Path $fullPath -Value $content -NoNewline
            Write-Host "  ✓ Fixed unused exception variables" -ForegroundColor Green
        } else {
            Write-Host "  - No changes needed" -ForegroundColor Yellow
        }
    } else {
        Write-Host "File not found: $fullPath" -ForegroundColor Red
    }
}

Write-Host "`nDone! Run 'dotnet build' to verify fixes." -ForegroundColor Green
