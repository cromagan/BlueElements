# TableStress — Protokoll-Auswertung
# usage: ProtokollAuswertung.ps1 -Ordner <pfad> [-Minuten 30]
# Liest alle Stress-Protokolle (B*-*.txt) im Ordner und fasst zusammen:
# FEHLER-Zeilen, MELDUNG-Klassen, Aktions-Zähler, FRAGMENTE-Zeilen und fehlende ENDE-Zeilen.
param(
    [Parameter(Mandatory = $true)][string]$Ordner,
    [int]$Minuten = 30
)

$grenze = (Get-Date).AddMinutes(-$Minuten)
$protokolle = Get-ChildItem -Path $Ordner -Filter "B*-*.txt" -File |
    Where-Object { $_.LastWriteTime -ge $grenze } |
    Sort-Object LastWriteTime

if (-not $protokolle) {
    Write-Output "KEINE PROTKOLLE gefunden in $Ordner (seit $grenze)"
    exit 1
}

$fehlerGesamt = 0
$warnGesamt = 0

foreach ($p in $protokolle) {
    $zeilen = Get-Content -Path $p.FullName -Encoding UTF8
    $erste = ($zeilen | Select-Object -First 1)
    $fehler = @($zeilen | Where-Object { $_ -match '\|FEHLER\|' })
    $warn = @($zeilen | Where-Object { $_ -match '\|MELDUNG\|Warning' })
    $ende = @($zeilen | Where-Object { $_ -match '\|ENDE\|' })
    $frag = @($zeilen | Where-Object { $_ -match '\|FRAGMENTE\|' })
    $aktionen = @($zeilen | Where-Object { $_ -match '\|' } | ForEach-Object { ($_ -split '\|')[2] } | Group-Object | Sort-Object Count -Descending)

    Write-Output ("")
    Write-Output ("=== {0}  ({1} Zeilen) ===" -f $p.Name, $zeilen.Count)
    Write-Output ("    Erste Zeile: {0}" -f $erste)

    if ($ende.Count -eq 0) { Write-Output "    !! KEIN ENDE (Watchdog-Kill oder Absturz?)" }

    $aktionText = ($aktionen | ForEach-Object { "{0}={1}" -f $_.Name, $_.Count }) -join " "
    Write-Output ("    Aktionen: {0}" -f $aktionText)

    foreach ($f in $frag) { Write-Output ("    {0}" -f $f) }

    if ($fehler.Count -gt 0) {
        $fehlerGesamt += $fehler.Count
        Write-Output ("    --- {0} FEHLER:" -f $fehler.Count)
        foreach ($f in ($fehler | Select-Object -First 30)) { Write-Output ("    {0}" -f $f) }
        if ($fehler.Count -gt 30) { Write-Output ("    ... {0} weitere FEHLER unterdrückt" -f ($fehler.Count - 30)) }
    }

    if ($warn.Count -gt 0) {
        $warnGesamt += $warn.Count
        $klassen = $warn | ForEach-Object { ($_ -split '\|')[3] } | Group-Object | Sort-Object Count -Descending
        Write-Output ("    --- {0} Warnungen:" -f $warn.Count)
        foreach ($k in $klassen | Select-Object -First 15) {
            $beispiel = ($warn | Where-Object { ($_ -split '\|')[3] -eq $k.Name } | Select-Object -First 1)
            Write-Output ("    {0}x {1}" -f $k.Count, $beispiel)
        }
    }
}

Write-Output ("")
Write-Output ("SUMME: {0} Protokolle, {1} FEHLER, {2} Warnungen" -f $protokolle.Count, $fehlerGesamt, $warnGesamt)
if ($fehlerGesamt -gt 0) { exit 2 } else { exit 0 }
