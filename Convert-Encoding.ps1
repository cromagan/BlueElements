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
Write-Host "=== Encoding Summary ===" -ForegroundColor Cyan
Write-Host "OK (already UTF-8 with BOM): $counterOk" -ForegroundColor Green
Write-Host "Added BOM:                  $counterAddedBom" -ForegroundColor Green
Write-Host "Converted (CP1252/UTF16):   $counterConverted" -ForegroundColor Magenta
Write-Host "Errors (broken UTF-8):      $counterErrors" -ForegroundColor Red
Write-Host "Total files checked:        $($counterOk + $counterAddedBom + $counterConverted + $counterErrors)" -ForegroundColor Gray

# --- License Header ---
Write-Host ""
Write-Host "=== License Header ===" -ForegroundColor Cyan

$licenseHeader = "// Licensed under AGPL-3.0; see License.md for disclaimer and details.`r`n"
$counterLicenseAdded = 0
$counterLicenseOk = 0
$counterLicenseStripped = 0

Get-ChildItem -Recurse -Filter "*.cs" -File -EA SilentlyContinue | Where-Object { $_.FullName -notlike "*\obj\*" } | ForEach-Object {
    $file = $_.FullName
    $content = [System.IO.File]::ReadAllText($file)

    if ($content.StartsWith($licenseHeader)) {
        $script:counterLicenseOk++
        return
    }

    $lines = [System.Collections.Generic.List[string]]::new(($content -split "`r?`n"))
    $stripped = 0
    while ($lines.Count -gt 0 -and ($lines[0] -match '^\s*$' -or $lines[0] -match '^\s*//')) {
        $lines.RemoveAt(0)
        $stripped++
    }

    $newContent = $licenseHeader + "`r`n" + ($lines -join "`r`n")

    if ($newContent -ne $content) {
        $utf8Bom = New-Object System.Text.UTF8Encoding $true
        [System.IO.File]::WriteAllText($file, $newContent, $utf8Bom)
        $script:counterLicenseAdded++
        if ($stripped -gt 0) {
            $script:counterLicenseStripped++
            Write-Host "[License + Stripped $stripped lines] $file" -ForegroundColor Yellow
        } else {
            Write-Host "[License Added] $file" -ForegroundColor Green
        }
    }
}

Write-Host ""
Write-Host "License already present:  $counterLicenseOk" -ForegroundColor Green
Write-Host "License added/updated:     $counterLicenseAdded" -ForegroundColor Yellow
Write-Host "Leading lines stripped:    $counterLicenseStripped" -ForegroundColor Gray

# --- Code Style Transformations ---
Write-Host ""
Write-Host "=== Code Style Transformations ===" -ForegroundColor Cyan

$styleBraces = 0
$styleReturn = 0
$styleParenSpace = 0
$styleNeNull = 0
$styleEqNull = 0
$styleIsNotBrace = 0
$styleIsBrace = 0
$styleDispNot = 0
$styleDisp = 0
$styleTotal = 0

$keywords = @('if', 'for', 'while', 'switch', 'catch', 'using', 'lock', 'foreach', 'return', 'new', 'typeof', 'sizeof', 'checked', 'unchecked', 'nameof', 'default', 'base', 'this')

$disposableTypes = @('Table', 'RowItem', 'ColumnItem', 'GenericControl', 'RowCollection', 'ColumnCollection', 'FilterCollection', 'ColumnViewCollection', 'CellCollection', 'ColumnViewItem', 'ConnectedFormula', 'AbstractPadItem', 'AbstractListItem', 'BlueFont', 'ExtText', 'PointM', 'ExtChar', 'ScriptDescription', 'FlexiStrategyBase', 'CachedFile', 'CachedFileSystem', 'PictureView', 'QuickPicSelector', 'Row', 'Column', 'Filter')
$dispPattern = ($disposableTypes | ForEach-Object { [regex]::Escape($_) }) -join '|'

Get-ChildItem -Recurse -Filter "*.cs" -File -EA SilentlyContinue | Where-Object { $_.FullName -notlike "*\obj\*" } | ForEach-Object {
    $file = $_.FullName
    $content = [System.IO.File]::ReadAllText($file)
    $original = $content

    $c1 = $false; $c3 = $false; $c4 = $false; $c5 = $false; $c6 = $false; $c7a = $false; $c7b = $false; $c7 = $false; $c8 = $false

    # 1. Leere Klammern uber zwei Zeilen -> eine Zeile
    $prev = $content
    $content = [regex]::Replace($content, '(\r?\n[ \t]*)?\{[ \t]*\r?\n(?:[ \t]*\r?\n)*[ \t]*\}', {
        param($m)
        if ($m.Groups[1].Success) { return ' { }' }
        return '{}'
    })
    $c1 = ($content -ne $prev)

    # 3. Return in if ohne Klammern -> if() {return;}
    $prev = $content
    $content = [regex]::Replace($content, '\b(if\s*\((?:[^()]|\((?:[^()]|\([^()]*\))*\))*\))\s*(return\b[^\r\n;]*;)', {
        param($m)
        return "$($m.Groups[1].Value) {$($m.Groups[2].Value)}"
    })
    $c3 = ($content -ne $prev)

    # 4. Leerzeichen nach ( bei Methodenaufrufen entfernen
    $prev = $content
    $content = [regex]::Replace($content, '\b(\w+)\(([ \t]+)', {
        param($m)
        $word = $m.Groups[1].Value
        if ($keywords -contains $word) { return $m.Value }
        return "$word("
    })
    $c4 = ($content -ne $prev)

    # 5. != null -> is not null
    $prev = $content
    $content = $content -creplace '\s*!=\s*null\b', ' is not null'
    $c5 = ($content -ne $prev)

    # 6. == null -> is null
    $prev = $content
    $content = $content -creplace '\s*==\s*null\b', ' is null'
    $c6 = ($content -ne $prev)

    # 7a. (disposable) is not { } -> is not { IsDisposed: false }
    $prev = $content
    $content = $content -creplace "\b($dispPattern)\s+is\s+not\s*\{\s*\}", '$1 is not { IsDisposed: false }'
    $c7a = ($content -ne $prev)

    # 7b. (disposable) is { } -> is { IsDisposed: false }
    $prev = $content
    $content = $content -creplace "\b($dispPattern)\s+is\s*\{\s*\}", '$1 is { IsDisposed: false }'
    $c7b = ($content -ne $prev)

    # 7. is not { } -> is null (only before ) & |)
    $prev = $content
    $content = $content -creplace 'is\s+not\s*\{\s*\}(?=\s*[)&|])', 'is null'
    $c7 = ($content -ne $prev)

    # 8. is { } -> is not null (only before ) & |)
    $prev = $content
    $content = $content -creplace 'is\s*\{\s*\}(?=\s*[)&|])', 'is not null'
    $c8 = ($content -ne $prev)

    if ($c1) { $script:styleBraces++ }
    if ($c3) { $script:styleReturn++ }
    if ($c4) { $script:styleParenSpace++ }
    if ($c5) { $script:styleNeNull++ }
    if ($c6) { $script:styleEqNull++ }
    if ($c7) { $script:styleIsNotBrace++ }
    if ($c8) { $script:styleIsBrace++ }
    if ($c7a) { $script:styleDispNot++ }
    if ($c7b) { $script:styleDisp++ }

    if ($content -ne $original) {
        $utf8Bom = New-Object System.Text.UTF8Encoding $true
        [System.IO.File]::WriteAllText($file, $content, $utf8Bom)
        $script:styleTotal++
        $changes = @()
        if ($c1) { $changes += "Braces" }
        if ($c3) { $changes += "IfReturn" }
        if ($c4) { $changes += "ParenSpace" }
        if ($c5) { $changes += "NeNull" }
        if ($c6) { $changes += "EqNull" }
        if ($c7) { $changes += "IsNotBrace" }
        if ($c8) { $changes += "IsBrace" }
        if ($c7a) { $changes += "DispNot" }
        if ($c7b) { $changes += "Disp" }
        Write-Host "[Style: $($changes -join ', ')] $file" -ForegroundColor Yellow
    }
}

Write-Host ""
Write-Host "=== Style Summary ===" -ForegroundColor Cyan
Write-Host "Empty braces collapsed:     $styleBraces" -ForegroundColor Green
Write-Host "If-return braced:           $styleReturn" -ForegroundColor Green
Write-Host "Paren space removed:        $styleParenSpace" -ForegroundColor Green
Write-Host "!= null -> is not null:    $styleNeNull" -ForegroundColor Green
Write-Host "== null -> is null:        $styleEqNull" -ForegroundColor Green
Write-Host "is not { } -> is null:     $styleIsNotBrace" -ForegroundColor Green
Write-Host "is { } -> is not null:     $styleIsBrace" -ForegroundColor Green
Write-Host "Disposable is not { } -> IsDisposed: $styleDispNot" -ForegroundColor Green
Write-Host "Disposable is { } -> IsDisposed:     $styleDisp" -ForegroundColor Green
Write-Host "Total files modified:       $styleTotal" -ForegroundColor Yellow