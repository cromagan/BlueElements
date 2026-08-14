# Ableitungs-Renamer für BlueElements
# Listet alle abstrakten Klassen auf ("virtuelle" Klassen).
# Nach Eingabe einer Nummer werden alle direkten Ableitungen der gewählten Basisklasse
# geprüft: Endet der Klassenname nicht auf den Basisklassen-Namen (Muster XXX<Basisname>),
# wird die Klasse umbenannt - nach Modus-Ausfrage entweder nur in der jeweiligen
# Datei oder codebase-weit (Deklaration, alle Vorkommnisse und Dateinamen).
# Dateinamen werden außerdem für bereits konforme Ableitungen und die Basisklasse
# selbst an den Klassennamen angeglichen (z. B. Url.cs -> UrlColumnFormat.cs).
# Danach erscheint die Statistik und das Script startet wieder von vorn (neuer Scan).
# Beenden mit leerer Eingabe. Sicherheits-Ausschlüsse (werden nur gemeldet, nicht geändert):
#   - Zielname existiert bereits als Typ
#   - mehr als $OccurrenceLimit Vorkommnisse (schützt vor Framework-Namen wie DateTime/String)
# Gleichnamige Klassen in anderen Namespaces (andere Basisklasse) werden MIT umbenannt
# und in der Statistik als "Mit umbenannt" aufgeführt.
# Aufruf: powershell -ExecutionPolicy Bypass -File .\Rename-DerivedClasses.ps1

$ErrorActionPreference = "Stop"
$root = $PSScriptRoot
if (-not $root) { $root = (Get-Location).Path }
$OccurrenceLimit = 150

function Get-FirstBaseName([string]$BaseList) {
    if ([string]::IsNullOrWhiteSpace($BaseList)) { return $null }
    $wi = $BaseList.IndexOf("where")
    if ($wi -ge 0) { $BaseList = $BaseList.Substring(0, $wi) }
    $depth = 0
    for ($i = 0; $i -lt $BaseList.Length; $i++) {
        $c = $BaseList[$i]
        if ($c -eq "<") { $depth++ }
        elseif ($c -eq ">") { $depth-- }
        elseif ($c -eq "," -and $depth -eq 0) { $BaseList = $BaseList.Substring(0, $i); break }
    }
    $first = $BaseList.Trim()
    $lt = $first.IndexOf("<")
    if ($lt -ge 0) { $first = $first.Substring(0, $lt) }
    $dot = $first.LastIndexOf(".")
    if ($dot -ge 0) { $first = $first.Substring($dot + 1) }
    if ($first.Length -eq 0) { return $null }
    return $first
}

function Scan-Codebase {
    $fileItems = Get-ChildItem -LiteralPath $root -Recurse -File |
        Where-Object { $_.Extension -eq ".cs" -and $_.FullName -notmatch "\\(bin|obj|\.vs|packages)\\" }

    $declRegex = [regex]"(?<![\w.])(?<mods>(?:(?:public|internal|protected|private|new|static|abstract|sealed|partial)[ \t]+)*)(?<kind>class|struct|interface|record|enum)[ \t]+(?<name>[A-Za-z_][A-Za-z0-9_]*)[ \t]*(?:<[^>]*>)?\s*(?::(?<bases>[^{]+))?"

    $localFiles = New-Object System.Collections.Generic.List[object]
    $localClasses = New-Object System.Collections.Generic.List[object]

    foreach ($f in $fileItems) {
        $bytes = [System.IO.File]::ReadAllBytes($f.FullName)
        $enc = "utf8"
        $text = $null
        if ($bytes.Length -ge 3 -and $bytes[0] -eq 0xEF -and $bytes[1] -eq 0xBB -and $bytes[2] -eq 0xBF) {
            $enc = "utf8bom"
            $text = [System.Text.Encoding]::UTF8.GetString($bytes, 3, $bytes.Length - 3)
        } elseif ($bytes.Length -ge 2 -and $bytes[0] -eq 0xFF -and $bytes[1] -eq 0xFE) {
            $enc = "utf16le"
            $text = [System.Text.Encoding]::Unicode.GetString($bytes, 2, $bytes.Length - 2)
        } elseif ($bytes.Length -ge 2 -and $bytes[0] -eq 0xFE -and $bytes[1] -eq 0xFF) {
            $enc = "utf16be"
            $text = [System.Text.Encoding]::BigEndianUnicode.GetString($bytes, 2, $bytes.Length - 2)
        } else {
            $text = [System.Text.Encoding]::UTF8.GetString($bytes)
        }
        if ([string]::IsNullOrEmpty($text)) { continue }

        $rel = $f.FullName.Substring($root.Length).TrimStart([System.IO.Path]::DirectorySeparatorChar)
        $localFiles.Add([pscustomobject]@{ Path = $f.FullName; Rel = $rel; Text = $text; Enc = $enc })

        foreach ($m in $declRegex.Matches($text)) {
            $lineStart = $text.LastIndexOf([char]10, $m.Index)
            $before = $text.Substring($lineStart + 1, $m.Index - $lineStart - 1)
            if ($before.Contains("//")) { continue }
            $mods = $m.Groups["mods"].Value
            $localClasses.Add([pscustomobject]@{
                Name = $m.Groups["name"].Value
                Kind = $m.Groups["kind"].Value
                IsAbstract = [bool]($mods -cmatch "abstract")
                Base = (Get-FirstBaseName $m.Groups["bases"].Value)
                File = $rel
            })
        }
    }

    return @{ Files = $localFiles; Classes = $localClasses }
}

Write-Host "=== BlueElements Ableitungs-Renamer ===" -ForegroundColor Cyan

$sessionRenames = 0
$sessionOcc = 0
$sessionRenamedFiles = 0

while ($true) {
    Write-Host ""
    Write-Host "Scanne .cs-Dateien..." -ForegroundColor Gray
    $scan = Scan-Codebase
    $fileCache = $scan.Files
    $classes = $scan.Classes
    Write-Host ("{0} Dateien, {1} Typ-Deklarationen gefunden." -f $fileCache.Count, $classes.Count) -ForegroundColor Gray

    $candidates = @($classes | Where-Object { $_.Kind -eq "class" -and $_.IsAbstract } |
        Group-Object Name | Sort-Object Name)

    if ($candidates.Count -eq 0) {
        Write-Host "Keine abstrakten Klassen gefunden." -ForegroundColor Yellow
        break
    }

    Write-Host ""
    Write-Host ("Abstrakte Klassen: {0}" -f $candidates.Count) -ForegroundColor Cyan
    $idx = 0
    foreach ($g in $candidates) {
        $idx++
        Write-Host ("[{0,3}] {1,-42} {2}" -f $idx, $g.Name, $g.Group[0].File)
    }

    Write-Host ""
    $answer = Read-Host "Nummer der Basisklasse eingeben (leer = beenden)"
    if ([string]::IsNullOrWhiteSpace($answer)) { break }

    $selNum = 0
    if (-not [int]::TryParse($answer.Trim(), [ref]$selNum) -or $selNum -lt 1 -or $selNum -gt $candidates.Count) {
        Write-Host "Ungültige Eingabe." -ForegroundColor Red
        continue
    }

    $sel = $candidates[$selNum - 1].Group[0]
    $selName = $sel.Name

    Write-Host ""
    $modeAnswer = Read-Host "Ersetzen: (1) nur in der jeweiligen Datei, (2) global (leer = zurück zur Liste)"
    $scopeDatei = $false
    if ($modeAnswer.Trim() -eq "1") { $scopeDatei = $true }
    elseif ($modeAnswer.Trim() -ne "2") { continue }

    # Alle Namen für Kollisionsprüfungen
    $declCount = [System.Collections.Generic.Dictionary[string, int]]::new([System.StringComparer]::Ordinal)
    foreach ($c in $classes) {
        if ($declCount.ContainsKey($c.Name)) { $declCount[$c.Name] = $declCount[$c.Name] + 1 } else { $declCount[$c.Name] = 1 }
    }
    $allTypeNames = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::Ordinal)
    foreach ($k in $declCount.Keys) { [void]$allTypeNames.Add($k) }

    # Direkte Ableitungen der gewählten Klasse
    $derivGroups = @($classes | Where-Object { $_.Kind -eq "class" -and $_.Base -ceq $selName -and $_.Name -cne $selName } | Group-Object Name)

    $renames = [System.Collections.Generic.Dictionary[string, string]]::new([System.StringComparer]::Ordinal)
    $conforms = New-Object System.Collections.Generic.List[string]
    $skipped = New-Object System.Collections.Generic.List[object]
    $collateral = New-Object System.Collections.Generic.List[object]

    foreach ($dg in $derivGroups) {
        $d = $dg.Group[0]
        if ($d.Name.EndsWith($selName, [System.StringComparison]::Ordinal)) {
            $conforms.Add($d.Name)
            continue
        }
        $newName = $d.Name + $selName
        # Gleichnamige Klassen mit anderer Basisklasse (anderer Namespace) werden mit umbenannt
        $sameNameDecls = @($classes | Where-Object { $_.Name -ceq $d.Name })
        foreach ($snd in $sameNameDecls) {
            if ($snd.Base -cne $selName) {
                $collateral.Add([pscustomobject]@{ Name = $d.Name; File = $snd.File; Base = $snd.Base; NewName = $newName })
            }
        }
        if ($allTypeNames.Contains($newName)) {
            $skipped.Add([pscustomobject]@{ Name = $d.Name; Reason = "Zielname '$newName' existiert bereits als Typ" })
            continue
        }
        $cntRx = [regex]("(?<![\w.])" + [regex]::Escape($d.Name) + "\b")
        $cntFiles = $fileCache
        if ($scopeDatei) {
            $rels = @($classes | Where-Object { $_.Name -ceq $d.Name -and $_.Kind -eq "class" } | ForEach-Object { $_.File })
            $cntFiles = @($fileCache | Where-Object { $rels -contains $_.Rel })
        }
        $cnt = 0
        foreach ($fc in $cntFiles) { $cnt += $cntRx.Matches($fc.Text).Count }
        if ($cnt -gt $OccurrenceLimit) {
            $skipped.Add([pscustomobject]@{ Name = $d.Name; Reason = "$cnt Vorkommnisse (Grenze $OccurrenceLimit) - manuell prüfen" })
            continue
        }
        $renames[$d.Name] = $newName
    }

    # Vorkommnisse ersetzen - je nach Modus nur in den Deklarationsdateien oder überall
    $totalOcc = 0
    $changedFiles = 0
    if ($renames.Count -gt 0) {
        $scopePaths = $null
        if ($scopeDatei) {
            $scopePaths = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::OrdinalIgnoreCase)
            foreach ($key in @($renames.Keys)) {
                $rels = @($classes | Where-Object { $_.Name -ceq $key -and $_.Kind -eq "class" } | ForEach-Object { $_.File })
                foreach ($fc in $fileCache) {
                    if ($rels -contains $fc.Rel) { [void]$scopePaths.Add($fc.Path) }
                }
            }
        }
        $alt = ($renames.Keys | Sort-Object Length -Descending | ForEach-Object { [regex]::Escape($_) }) -join "|"
        $rx = [regex]("(?<![\w.])(" + $alt + ")\b")
        foreach ($fc in $fileCache) {
            if ($scopePaths -and -not $scopePaths.Contains($fc.Path)) { continue }
            if (-not $rx.IsMatch($fc.Text)) { continue }
            $hits = $rx.Matches($fc.Text).Count
            $newText = $rx.Replace($fc.Text, { param($mm) $renames[$mm.Groups[1].Value] })
            $encObj = $null
            switch ($fc.Enc) {
                "utf8bom" { $encObj = New-Object System.Text.UTF8Encoding $true }
                "utf16le" { $encObj = [System.Text.Encoding]::Unicode }
                "utf16be" { $encObj = [System.Text.Encoding]::BigEndianUnicode }
                default { $encObj = New-Object System.Text.UTF8Encoding $false }
            }
            [System.IO.File]::WriteAllText($fc.Path, $newText, $encObj)
            $totalOcc += $hits
            $changedFiles++
        }
    }

    # Dateinamen an Klassennamen angleichen (inkl. .designer.cs):
    # für die Basisklasse selbst sowie für umbenannte UND bereits konforme Ableitungen.
    # Enthält der Dateiname den Klassennamen bereits, passiert nichts.
    $renamedFiles = New-Object System.Collections.Generic.List[string]

    function Sync-FileName([string]$className, [object[]]$decls, [System.Collections.Generic.List[string]]$log) {
        $nameRx = [regex]("(?<![\w.])" + [regex]::Escape($className) + "\b")
        foreach ($decl in $decls) {
            $src = [System.IO.Path]::Combine($root, $decl.File)
            if (-not (Test-Path -LiteralPath $src)) { continue }
            $leaf = [System.IO.Path]::GetFileName($src)
            if ($nameRx.IsMatch($leaf)) { continue }
            $suffix = ".cs"
            if ($decl.File -imatch "\.designer\.cs$") { $suffix = ".designer.cs" }
            $newLeaf = $className + $suffix
            $dir = [System.IO.Path]::GetDirectoryName($src)
            $target = [System.IO.Path]::Combine($dir, $newLeaf)
            if (Test-Path -LiteralPath $target) {
                Write-Host ("  ! Umbenennen übersprungen, Ziel existiert: {0}" -f $newLeaf) -ForegroundColor Yellow
                continue
            }
            Rename-Item -LiteralPath $src -NewName $newLeaf
            $log.Add("$leaf -> $newLeaf")
        }
    }

    Sync-FileName $selName @($classes | Where-Object { $_.Name -ceq $selName -and $_.Kind -eq "class" }) $renamedFiles

    foreach ($dg in $derivGroups) {
        $oldName = $dg.Group[0].Name
        if ($renames.ContainsKey($oldName)) {
            Sync-FileName $renames[$oldName] @($classes | Where-Object { $_.Name -ceq $oldName -and $_.Kind -eq "class" }) $renamedFiles
        } elseif ($conforms -contains $oldName) {
            Sync-FileName $oldName @($classes | Where-Object { $_.Name -ceq $oldName -and $_.Kind -eq "class" }) $renamedFiles
        }
    }

    $sessionRenames += $renames.Count
    $sessionOcc += $totalOcc
    $sessionRenamedFiles += $renamedFiles.Count

    Write-Host ""
    Write-Host "=== Statistik ===" -ForegroundColor Cyan
    Write-Host ("Modus                : {0}" -f $(if ($scopeDatei) { "nur jeweilige Datei" } else { "global" }))
    Write-Host ("Basisklasse          : {0}" -f $selName)
    Write-Host ("Datei                : {0}" -f $sel.File)
    Write-Host ("Direkte Ableitungen  : {0}" -f $derivGroups.Count)
    if ($conforms.Count -gt 0) {
        Write-Host ("Konform (keine Aktion): {0}" -f ($conforms -join ", ")) -ForegroundColor DarkGreen
    }
    foreach ($k in $renames.Keys) {
        Write-Host ("Umbenannt            : {0} -> {1}" -f $k, $renames[$k]) -ForegroundColor Green
    }
    foreach ($c in $collateral) {
        Write-Host ("Mit umbenannt        : {0} -> {1} ({2}, Basis: {3})" -f $c.Name, $c.NewName, $c.File, $c.Base) -ForegroundColor Magenta
    }
    foreach ($s in $skipped) {
        Write-Host ("Übersprungen         : {0} - {1}" -f $s.Name, $s.Reason) -ForegroundColor Yellow
    }
    Write-Host ("Vorkommnisse ersetzt : {0} in {1} Dateien" -f $totalOcc, $changedFiles)
    Write-Host ("Dateien umbenannt    : {0}" -f $renamedFiles.Count)
    foreach ($r in $renamedFiles) {
        Write-Host ("  {0}" -f $r)
    }
    Write-Host ""
    Read-Host "ENTER = weiter (neuer Scan)" | Out-Null
}

Write-Host ""
Write-Host "=== Gesamt (Session) ===" -ForegroundColor Cyan
Write-Host ("Klassen umbenannt    : {0}" -f $sessionRenames)
Write-Host ("Vorkommnisse ersetzt : {0}" -f $sessionOcc)
Write-Host ("Dateien umbenannt    : {0}" -f $sessionRenamedFiles)
Read-Host "ENTER zum Beenden" | Out-Null
