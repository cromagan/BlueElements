$Path = Join-Path $PSScriptRoot "Deutsch.bin"
$Content = Get-Content -Path $Path -Encoding UTF8

# 1) Nach dem Einlesen, Leerzeichen durch \r\n ersetzen
$Content = $Content -replace ' ', "`r`n"

$Clean = $Content | Where-Object { $_.Trim() -ne "" }
$Sorted = $Clean | Sort-Object -Unique

# 2) Vor dem Speichern fragen, ob alles in Kleinbuchstaben gespeichert werden soll
$Confirmation = Read-Host "Soll alles in Kleinbuchstaben gespeichert werden? (j/n)"
if ($Confirmation -eq 'j') {
    $Sorted = $Sorted.ToLower()
}

[System.IO.File]::WriteAllText($Path, ($Sorted -join "`r`n"), [System.Text.Encoding]::UTF8)