# FragmentDump.ps1 — parst eine Fragment-Datei (.tblc/.tbls/.tblf, gezippt oder unkomprimiert)
# und gibt je Satz: Pos, Depth, ValueId/Tag, DataType, Content-Kurztext, Meta.
# usage: FragmentDump.ps1 -Datei <pfad> [-NurKopf 50]
param(
    [Parameter(Mandatory = $true)][string]$Datei,
    [int]$NurKopf = 0
)

Add-Type -AssemblyName System.IO.Compression
Add-Type -AssemblyName System.IO.Compression.FileSystem

function Get-ContentBytes([string]$pfad) {
    $raw = [System.IO.File]::ReadAllBytes($pfad)
    if ($raw.Length -gt 4 -and [BitConverter]::ToInt32($raw, 0) -eq 67324752) {
        $ms = New-Object System.IO.MemoryStream(,$raw)
        $zip = New-Object System.IO.Compression.ZipArchive($ms)
        $entry = $zip.GetEntry("Main.bin")
        if (-not $entry) { throw "Kein Main.bin im ZIP: $pfad" }
        $out = New-Object System.IO.MemoryStream
        $entry.Open().CopyTo($out)
        return $out.ToArray()
    }
    return $raw
}

function KurzText([byte[]]$bytes, [int]$dataType) {
    # FragmentDataType: 1=Bool, 2=String, 3=Int, 4=Float, 5=DateTime, 6=Color, 7=Json, 10=ListSet, 11=ListKeyObject, 12=Long, 13=Double, 14=Decimal, 15=KeyNameType, 16=UInt
    try {
        switch ($dataType) {
            1 { return [System.Text.Encoding]::Latin1.GetString($bytes) }
            2 { $t = [System.Text.Encoding]::UTF8.GetString($bytes); if ($t.Length -gt 60) { $t = $t.Substring(0, 60) + "…" }; return $t }
            3 { if ($bytes.Length -eq 4) { return [BitConverter]::ToInt32($bytes, 0).ToString() } }
            4 { if ($bytes.Length -eq 4) { return [BitConverter]::ToSingle($bytes, 0).ToString() } }
            5 { if ($bytes.Length -eq 8) { $ticks = [BitConverter]::ToInt64($bytes, 0); return (New-Object DateTime($ticks)).ToString("yyyy-MM-dd HH:mm:ss.fff") } }
            10 { return "ListSet " + $bytes.Length + "B" }
            11 { return "ListKeyObject" }
            12 { if ($bytes.Length -eq 8) { return [BitConverter]::ToInt64($bytes, 0).ToString() } }
            15 { $t = [System.Text.Encoding]::UTF8.GetString($bytes); if ($t.Length -gt 60) { $t = $t.Substring(0, 60) + "…" }; return $t }
            default { return $bytes.Length.ToString() + "B" }
        }
    } catch { return "?" }
    return ""
}

$content = Get-ContentBytes $Datei
Write-Output ("Datei: {0}  Inhalt: {1} Bytes" -f $Datei, $content.Length)

$pos = 0
$n = 0
while ($pos + 6 -le $content.Length) {
    $storageType = $content[$pos]
    $depth = $content[$pos + 1]
    $len1 = [BitConverter]::ToInt32($content, $pos + 2)
    if ($len1 -lt 1 -or ($pos + 6 + $len1) -gt $content.Length) {
        Write-Output ("{0,8} UNGÜLTIG storageType={1} depth={2} len1={3}" -f $pos, $storageType, $depth, $len1)
        break
    }

    $valueId = $content[$pos + 6]
    $dataType = $content[$pos + 7]
    $len2 = [BitConverter]::ToInt32($content, $pos + 8)
    $contentStart = $pos + 12

    $istTag = $valueId -ge 200
    $meta = ""
    $metaStart = $contentStart + $len2
    $metaLen = ($pos + 6 + $len1) - $metaStart
    if ($metaLen -gt 0 -and $metaStart -lt $content.Length) {
        $us1 = [Array]::IndexOf($content, 0x1F, $metaStart, $metaLen)
        if ($us1 -ge 0) {
            $user = [System.Text.Encoding]::UTF8.GetString($content, $metaStart, $us1 - $metaStart)
            $ticks = [BitConverter]::ToInt64($content, $us1 + 1)
            $zeit = (New-Object DateTime($ticks)).ToString("HH:mm:ss.fff")
            $us2 = [Array]::IndexOf($content, 0x1F, $us1 + 1, $metaLen - ($us1 - $metaStart) - 1)
            $kommentar = if ($us2 -ge 0) { [System.Text.Encoding]::UTF8.GetString($content, $us2 + 1, ($metaStart + $metaLen) - $us2 - 1) } else { "" }
            $meta = "META User=$user Zeit=$zeit Kommentar=$kommentar"
        }
    }

    $contentBytes = if ($len2 -ge 0 -and ($contentStart + $len2) -le $content.Length) { $content[$contentStart..($contentStart + $len2 - 1)] } else { ,@() }
    $tagText = if ($istTag) { "TAG $valueId" } else { "Leaf $valueId" }

    Write-Output ("{0,8} T{1,-2} {2,-10} DT={3,-2} len2={4,-6} {5} {6}" -f $pos, $depth, $tagText, $dataType, $len2, (KurzText $contentBytes $dataType), $meta)

    $pos += 6 + $len1
    $n++

    if ($NurKopf -gt 0 -and $n -ge $NurKopf) {
        Write-Output ("... abgebrochen nach {0} Sätzen (NurKopf)" -f $n)
        break
    }
}

Write-Output ("Sätze gesamt: {0}" -f $n)
