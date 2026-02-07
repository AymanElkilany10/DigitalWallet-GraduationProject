# Fix Controller Errors - Clean Solution
# Simply pass result.Errors directly to ErrorResponse
# C# will automatically select the correct overload (List<string>)

$ErrorActionPreference = "Stop"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Fixing Controller Error Handling" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

$projectRoot = "C:\Users\SoftLaptop\Desktop\DigitalWallet-Graduation-Project-"

$files = @(
    "$projectRoot\DigitalWallet.API\Controllers\BillPaymentController.cs",
    "$projectRoot\DigitalWallet.API\Controllers\FakeBankController.cs",
    "$projectRoot\DigitalWallet.API\Controllers\MoneyRequestController.cs"
)

$totalFixed = 0

foreach ($filePath in $files) {
    if (-not (Test-Path $filePath)) {
        Write-Host "⚠️  File not found: $filePath" -ForegroundColor Yellow
        continue
    }
    
    Write-Host "📄 Processing: $(Split-Path $filePath -Leaf)" -ForegroundColor White
    
    $content = Get-Content $filePath -Raw
    $originalContent = $content
    
    # Pattern: result.Errors ?? Array.Empty<string>()
    # Replace with: result.Errors
    $pattern1 = 'result\.Errors \?\? Array\.Empty<string>\(\)'
    $replacement1 = 'result.Errors'
    $content = $content -replace $pattern1, $replacement1
    
    # Pattern: result.Errors?.FirstOrDefault() ?? "message", result.Errors ?? Array.Empty<string>()
    # Replace with: result.ErrorMessage ?? "message", result.Errors
    $pattern2 = '(result\.Errors\?\.FirstOrDefault\(\) \?\? "[^"]+"), result\.Errors \?\? Array\.Empty<string>\(\)'
    $replacement2 = '$1, result.Errors'
    $content = $content -replace $pattern2, $replacement2
    
    if ($content -ne $originalContent) {
        $content | Set-Content $filePath -NoNewline
        $fixCount = ([regex]::Matches($originalContent, 'Array\.Empty<string>\(\)')).Count
        $totalFixed += $fixCount
        Write-Host "   ✓ Fixed $fixCount occurrence(s)" -ForegroundColor Green
    } else {
        Write-Host "   • No changes needed" -ForegroundColor Gray
    }
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "✅ Total fixes applied: $totalFixed" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Next: Run 'dotnet build' to verify" -ForegroundColor Yellow
