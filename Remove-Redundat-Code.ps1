$files = Get-ChildItem -Path "." -Filter "*.cs" -Recurse -File
$patterns = @(
    '^\s*public override bool GetCodeBlockAfter => false;\s*$'
    '^\s*public override List<string> Constants => \[\];\s*$'
    '^\s*public override List<List<string>> Args => \[\];\s*$'
    '^\s*public override bool MustUseReturnValue => false;\s*$'
    '^\s*public override string Returns => string\.Empty;\s*$'
    '^\s*public override string StartSequence => "\(";\s*$'
    '^\s*public override int LastArgMinCount => -1;\s*$'
    '^\s*public override MethodType MethodLevel => MethodType\.Standard;\s*$'
)
$regex = '(?:' + ($patterns -join '|') + ')'
$count = 0

foreach ($file in $files) {
    $lines = [System.IO.File]::ReadAllLines($file.FullName, [System.Text.Encoding]::UTF8)
    $keep = [System.Collections.Generic.List[string]]::new()
    $changed = $false
    foreach ($line in $lines) {
        if ($line -match $regex) {
            $changed = $true
        } else {
            $keep.Add($line)
        }
    }
    if ($changed) {
        $newContent = $keep -join "`r`n"
        [System.IO.File]::WriteAllText($file.FullName, $newContent, [System.Text.UTF8Encoding]::new($true))
        $removed = $lines.Count - $keep.Count
        $count += $removed
        Write-Host "$removed x entfernt: $($file.FullName)"
    }
}

Write-Host "`nFertig: $count Zeilen in $($files.Count) Dateien geprueft."
