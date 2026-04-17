[Console]::OutputEncoding = [System.Text.Encoding]::UTF8

$ErrorActionPreference = "Continue"

$RepoPath = $null
$SolutionFile = $null

Write-Host "=== Visual Studio Cleanup Script ===" -ForegroundColor Cyan

$VSInstances = Get-Process -Name "devenv" -ErrorAction SilentlyContinue
if ($VSInstances) {
    Write-Host "`nVisual Studio ist aktiv. Prozesse:" -ForegroundColor Yellow
    $VSInstances | ForEach-Object { Write-Host "  - PID $($_.Id): $($_.MainWindowTitle)" }

    $continue = Read-Host "`nVisual Studio schliessen? (j/n)"
    if ($continue -eq "j") {
        Write-Host "Visual Studio wird beendet..."

        foreach ($instance in $VSInstances) {
            $windowTitle = $instance.MainWindowTitle
            if ($windowTitle -match '\.sln$') {
                $SolutionFile = $windowTitle
            } elseif ($windowTitle -match '^[^\\\/]+[\\\/][^\\\/]+[\\\/]' -and -not $SolutionFile) {
                $RepoPath = $windowTitle
            }

            Stop-Process -Id $instance.Id -Force
            Write-Host "  Beendet: PID $($instance.Id)"
        }

        Start-Sleep -Seconds 3
    } else {
        Write-Host "Vorgang abgebrochen." -ForegroundColor Red
        exit
    }
} else {
    Write-Host "`nVisual Studio ist nicht aktiv." -ForegroundColor Gray
}

$TargetPath = if ($RepoPath) { $RepoPath } elseif ($SolutionFile) { Split-Path $SolutionFile } else { $null }

if (-not $TargetPath) {
    Write-Host "`nBitte geben Sie den Pfad zum Repository/Projekt ein:" -ForegroundColor Yellow
    $TargetPath = Read-Host "(leer = aktuelles Verzeichnis)"
    if ([string]::IsNullOrWhiteSpace($TargetPath)) {
        $TargetPath = Get-Location
    }
}

Write-Host "`nBereinige: $TargetPath" -ForegroundColor Cyan

$CleanupPatterns = @(
    "**/bin",
    "**/obj",
    "**/.vs",
    "**/packages",
    "**/*.suo",
    "**/*.user",
    "**/*.suo",
    "**/*.cache",
    "**/~$*",
    "**/.idea",
    "**/*.log",
    "**/AppData/Local/Temp/**",
    "**/Debug",
    "**/Release",
    "**/x64",
    "**/x86",
    "**/.git/refs/stash",
    "**/.git/logs"
)

$RemovedCount = 0
$RemovedSize = 0

foreach ($pattern in $CleanupPatterns) {
    $fullPattern = Join-Path $TargetPath $pattern
    $items = Get-ChildItem -Path $fullPattern -Force -ErrorAction SilentlyContinue

    foreach ($item in $items) {
        try {
            $size = (Get-Item $item.FullName -Force -ErrorAction SilentlyContinue | Measure-Object -Property Length -Sum -ErrorAction SilentlyContinue).Sum
            if ($null -eq $size) { $size = 0 }

            Remove-Item -Path $item.FullName -Recurse -Force -ErrorAction Stop
            $RemovedCount++
            $RemovedSize += $size
            Write-Host "  Geloescht: $($item.FullName)" -ForegroundColor DarkGray
        } catch {
            Write-Host "  Fehler beim Loeschen: $($item.FullName)" -ForegroundColor Red
        }
    }
}

$sizeInMB = [math]::Round($RemovedSize / 1MB, 2)
Write-Host "`n=== Bereinigung abgeschlossen ===" -ForegroundColor Green
Write-Host "  Elemente entfernt: $RemovedCount" -ForegroundColor White
Write-Host "  Speicher freigegeben: $sizeInMB MB" -ForegroundColor White

if ($SolutionFile -and (Test-Path $SolutionFile)) {
    $reopen = Read-Host "`nVisual Studio mit '$($SolutionFile)' wieder oeffnen? (j/n)"
    if ($reopen -eq "j") {
        Write-Host "Starte Visual Studio..."
        Start-Process -FilePath "devenv" -ArgumentList "`"$SolutionFile`""
    }
} elseif ($TargetPath) {
    $slnFiles = Get-ChildItem -Path $TargetPath -Filter "*.sln" -Recurse -ErrorAction SilentlyContinue | Select-Object -First 1
    if ($slnFiles) {
        $reopen = Read-Host "`nVisual Studio mit '$($slnFiles.FullName)' wieder oeffnen? (j/n)"
        if ($reopen -eq "j") {
            Write-Host "Starte Visual Studio..."
            Start-Process -FilePath "devenv" -ArgumentList "`"$($slnFiles.FullName)`""
        }
    } else {
        Write-Host "`nKeine Solution-Datei gefunden." -ForegroundColor Yellow
    }
}

Write-Host "`nFertig. Druecken Sie eine beliebige Taste zum Beenden..."
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
