# Encoding Check and Convert Script for .cs Files
# Checks all .cs files and converts them to UTF-8 with BOM
# Run with: powershell -ExecutionPolicy Bypass -File .\Convert-Encoding.ps1

$ErrorActionPreference = "Stop"

Write-Host "=== BlueElements Encoding Converter ===" -ForegroundColor Cyan
Write-Host "This script converts all .cs files to UTF-8 with BOM" -ForegroundColor Gray
Write-Host ""

$counterOk = 0
$counterAddedBom = 0
$counterConverted = 0
$counterErrors = 0

Get-ChildItem -Recurse -Filter "*.cs" -File -EA SilentlyContinue | Where-Object { $_.FullName -notlike "*\obj\*" } | ForEach-Object {
    $file = $_.FullName
    $bytes = [System.IO.File]::ReadAllBytes($file)

    # Empty file
    if ($bytes.Length -eq 0) {
        Write-Host "[EMPTY] $file" -ForegroundColor Yellow
        $script:counterOk++
        return
    }

    # Check for UTF-8 BOM
    if ($bytes[0] -eq 0xEF -and $bytes[1] -eq 0xBB -and $bytes[2] -eq 0xBF) {
        $content = [System.Text.Encoding]::UTF8.GetString($bytes, 3, $bytes.Length - 3)
        if ($content -match '�') {
            Write-Host "[BROKEN] $file" -ForegroundColor Red
            $script:counterErrors++
        } else {
            $script:counterOk++
        }
        return
    }

    # Check for UTF-16 LE
    if ($bytes[0] -eq 0xFF -and $bytes[1] -eq 0xFE) {
        Write-Host "[UTF16-LE -> UTF8] $file" -ForegroundColor Magenta
        $content = [System.Text.Encoding]::Unicode.GetString($bytes)
        $utf8Bom = New-Object System.Text.UTF8Encoding $true
        [System.IO.File]::WriteAllText($file, $content, $utf8Bom)
        $script:counterConverted++
        return
    }

    # Check for UTF-16 BE
    if ($bytes[0] -eq 0xFE -and $bytes[1] -eq 0xFF) {
        Write-Host "[UTF16-BE -> UTF8] $file" -ForegroundColor Magenta
        $content = [System.Text.Encoding]::BigEndianUnicode.GetString($bytes)
        $utf8Bom = New-Object System.Text.UTF8Encoding $true
        [System.IO.File]::WriteAllText($file, $content, $utf8Bom)
        $script:counterConverted++
        return
    }

    # Validate UTF-8
    $isValidUtf8 = $true
    $i = 0
    while ($i -lt $bytes.Length) {
        $b = $bytes[$i]
        if ($b -le 0x7F) {
            $i++
        } elseif ($b -ge 0xC2 -and $b -le 0xDF -and $i + 1 -lt $bytes.Length -and $bytes[$i + 1] -ge 0x80 -and $bytes[$i + 1] -le 0xBF) {
            $i += 2
        } elseif ($b -ge 0xE0 -and $b -le 0xEF -and $i + 2 -lt $bytes.Length -and $bytes[$i + 1] -ge 0x80 -and $bytes[$i + 1] -le 0xBF -and $bytes[$i + 2] -ge 0x80 -and $bytes[$i + 2] -le 0xBF) {
            $i += 3
        } elseif ($b -ge 0xF0 -and $b -le 0xF4 -and $i + 3 -lt $bytes.Length -and $bytes[$i + 1] -ge 0x80 -and $bytes[$i + 1] -le 0xBF -and $bytes[$i + 2] -ge 0x80 -and $bytes[$i + 2] -le 0xBF -and $bytes[$i + 3] -ge 0x80 -and $bytes[$i + 3] -le 0xBF) {
            $i += 4
        } else {
            $isValidUtf8 = $false
            break
        }
    }

    if ($isValidUtf8) {
        # Valid UTF-8 without BOM - add BOM
        $content = [System.Text.Encoding]::UTF8.GetString($bytes)
        $utf8Bom = New-Object System.Text.UTF8Encoding $true
        [System.IO.File]::WriteAllText($file, $content, $utf8Bom)
        Write-Host "[BOM Added] $file" -ForegroundColor Green
        $script:counterAddedBom++
    } else {
        # Non-UTF8 (CP1252 or similar) - convert to UTF-8 with BOM
        $cp1252 = [System.Text.Encoding]::GetEncoding(1252)
        $content = $cp1252.GetString($bytes)
        $utf8Bom = New-Object System.Text.UTF8Encoding $true
        [System.IO.File]::WriteAllText($file, $content, $utf8Bom)
        Write-Host "[Converted] $file" -ForegroundColor Magenta
        $script:counterConverted++
    }
}

Write-Host ""
Write-Host "=== Summary ===" -ForegroundColor Cyan
Write-Host "OK (already UTF-8 with BOM): $counterOk" -ForegroundColor Green
Write-Host "Added BOM:                  $counterAddedBom" -ForegroundColor Green
Write-Host "Converted (CP1252/UTF16):   $counterConverted" -ForegroundColor Magenta
Write-Host "Errors (broken UTF-8):      $counterErrors" -ForegroundColor Red
Write-Host "Total files checked:        $($counterOk + $counterAddedBom + $counterConverted + $counterErrors)" -ForegroundColor Gray