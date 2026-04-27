$filePath = Join-Path $PSScriptRoot "Deutsch.bin"

if (-not (Test-Path $filePath)) {
    Write-Host "Datei nicht gefunden!" -ForegroundColor Red
    exit
}

$word = New-Object -ComObject Word.Application
$word.Visible = $false
$word.Options.CheckGrammarAsYouType = $false
$word.Options.CheckSpellingAsYouType = $false

try {
    # Ein leeres Dokument ist notwendig, damit GetSpellingSuggestions funktioniert
    $doc = $word.Documents.Add()

    # Einlesen der Wortliste als Array
    $lines = Get-Content -Path $filePath | Where-Object { $_.Trim() -ne "" }
    $totalLines = $lines.Count
    $correctedList = New-Object System.Collections.Generic.List[string]
    
    $total = 0
    $removed = 0

    Write-Host "Starte Korrektur von $totalLines Wörtern..." -ForegroundColor Cyan

    foreach ($alt in $lines) {
        if ($word.CheckSpelling($alt)) {
            $correctedList.Add($alt)
        } else {
            # Hier wird nun das Dokument-Kontext genutzt
            $sug = $word.GetSpellingSuggestions($alt)
            if ($null -ne $sug -and $sug.Count -gt 0) {
                $neu = $sug.Item(1).Name
                $correctedList.Add($neu)
                Write-Host "$alt -> $neu" -ForegroundColor Gray
                $total++
            } else {
                Write-Host "$alt entfernt (kein Vorschlag)" -ForegroundColor Yellow
                $removed++
            }
        }
    }

    # Finales Speichern mit Sortierung
    Write-Host "Finales Sortieren und Speichern..." -ForegroundColor Cyan
    $finalLines = $correctedList | Sort-Object -Unique
    [System.IO.File]::WriteAllLines($filePath, $finalLines, [System.Text.Encoding]::UTF8)
    
    Write-Host "Fertig." -ForegroundColor Green
    Write-Host "Ersetzungen: $total"
    Write-Host "Entfernt: $removed"
}
catch {
    Write-Host "Fehler: $($_.Exception.Message)" -ForegroundColor Red
}
finally {
    if ($null -ne $doc) { $doc.Close(0) }
    if ($null -ne $word) { $word.Quit() }
    Write-Host "Vorgang beendet."
    Read-Host "Taste druecken"
}
