# Method_* zu statischen Klassen migrieren
# PowerShell-Skript für automatische Suchen/Ersetzen

param(
    [string]$RootPath = (Get-Location),
    [switch]$WhatIf
)

$ErrorActionPreference = "Stop"

Write-Host "Suche alle Method_*.cs Dateien (ohne obj/)..." -ForegroundColor Cyan
$files = Get-ChildItem -Path $RootPath -Recurse -Filter "Method_*.cs" | Where-Object { $_.FullName -notlike "*\obj\*" }
$count = ($files | Measure-Object).Count
Write-Host "Gefunden: $count Dateien" -ForegroundColor Green

if ($count -eq 0) {
    Write-Host "Keine Dateien gefunden. Abbruch." -ForegroundColor Red
    exit 1
}

Write-Host ""

# ============================================================================
# Schritt 1: Properties: public override → public static
# ============================================================================
Write-Host "Schritt 1: Properties umstellen (override → static)..." -ForegroundColor Cyan

$propertyReplacements = @(
    @{ From = 'public override List<List<string>> Args';       To = 'public static List<List<string>> Args' },
    @{ From = 'public override string Command';               To = 'public static string Command' },
    @{ From = 'public override List<string> Constants';        To = 'public static List<string> Constants' },
    @{ From = 'public override string Description';            To = 'public static string Description' },
    @{ From = 'public override bool GetCodeBlockAfter';       To = 'public static bool GetCodeBlockAfter' },
    @{ From = 'public override int LastArgMinCount';        To = 'public static int LastArgMinCount' },
    @{ From = 'public override MethodType MethodLevel';      To = 'public static MethodType MethodLevel' },
    @{ From = 'public override bool MustUseReturnValue';      To = 'public static bool MustUseReturnValue' },
    @{ From = 'public override string Returns';              To = 'public static string Returns' },
    @{ From = 'public override string StartSequence';         To = 'public static string StartSequence' },
    @{ From = 'public override string Syntax';                To = 'public static string Syntax' }
)

$propChanged = 0
foreach ($f in $files) {
    $content = [System.IO.File]::ReadAllText($f.FullName)
    $original = $content
    foreach ($r in $propertyReplacements) {
        $content = $content.Replace($r.From, $r.To)
    }
    if ($content -ne $original) {
        if (-not $WhatIf) { [System.IO.File]::WriteAllText($f.FullName, $content) }
        $propChanged++
    }
}
Write-Host "  Properties geaendert: $propChanged Dateien" -ForegroundColor Green

# ============================================================================
# Schritt 2: Methoden: DoIt → DoItSplitted / DoItVirtual
# ============================================================================
Write-Host "Schritt 2: Methoden umbenennen (DoIt → DoItSplitted / DoItVirtual)..." -ForegroundColor Cyan

$methodReplacements = @(
    @{ From = 'public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld)'; To = 'public static DoItFeedback DoItSplitted(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld)' },
    @{ From = 'public override DoItFeedback DoIt(VariableCollection varCol, CanDoFeedback infos, ScriptProperties scp)';                           To = 'public static DoItFeedback DoItVirtual(VariableCollection varCol, CanDoFeedback infos, ScriptProperties scp)' }
)

$methChanged = 0
foreach ($f in $files) {
    $content = [System.IO.File]::ReadAllText($f.FullName)
    $original = $content
    foreach ($r in $methodReplacements) {
        $content = $content.Replace($r.From, $r.To)
    }
    if ($content -ne $original) {
        if (-not $WhatIf) { [System.IO.File]::WriteAllText($f.FullName, $content) }
        $methChanged++
    }
}
Write-Host "  Methoden geaendert: $methChanged Dateien" -ForegroundColor Green

# ============================================================================
# Schritt 3: Klassendeklarationen → sealed + IMethod
# ============================================================================
Write-Host "Schritt 3: Klassendeklarationen anpassen (sealed + IMethod)..." -ForegroundColor Cyan

$classChanged = 0
foreach ($f in $files) {
    $content = [System.IO.File]::ReadAllText($f.FullName)
    $original = $content

    if ($content -match 'class Method_TableGeneric' -or $content -match 'class Method_Row_Extension') { continue }

    $content = $content -replace '(sealed class (Method_\w+) : Method\b)', '$1, IMethod'
    $content = $content -replace '(internal sealed class (Method_\w+) : Method\b)', '$1, IMethod'

    if ($content -ne $original) {
        if (-not $WhatIf) { [System.IO.File]::WriteAllText($f.FullName, $content) }
        $classChanged++
    }
}
Write-Host "  Klassen geaendert: $classChanged Dateien" -ForegroundColor Green

# ============================================================================
# Schritt 4: Singleton-Felder entfernen (3 Dateien)
# ============================================================================
Write-Host "Schritt 4: Singleton-Felder entfernen..." -ForegroundColor Cyan

$singletonFiles = @(
    @{ Path = "BlueScript\Methods\Method_Break.cs"; Pattern = 'public static readonly Method Method = new Method_Break\(\);' },
    @{ Path = "BlueControls\AdditionalScriptMethods\Method_GetNote.cs"; Pattern = 'public static readonly Method Method = new Method_GetNote\(\);' },
    @{ Path = "BlueTable\AdditionalScriptMethods\Method_SetError.cs"; Pattern = 'public static readonly Method Method = new Method_SetError\(\);' }
)

$singletonChanged = 0
foreach ($sf in $singletonFiles) {
    $fullPath = Join-Path $RootPath $sf.Path
    if (Test-Path $fullPath) {
        $content = [System.IO.File]::ReadAllText($fullPath)
        $original = $content
        $content = $content -replace '\s*public static readonly Method Method = new Method_\w+\(\);\r?\n', ''
        if ($content -ne $original) {
            if (-not $WhatIf) { [System.IO.File]::WriteAllText($fullPath, $content) }
            $singletonChanged++
            Write-Host "  Entfernt: $($sf.Path)" -ForegroundColor Yellow
        }
    }
}
Write-Host "  Singletons entfernt: $singletonChanged Dateien" -ForegroundColor Green

# ============================================================================
# Schritt 5: Konsumenten aktualisieren
# ============================================================================
Write-Host "Schritt 5: Konsumenten aktualisieren..." -ForegroundColor Cyan

$consumerFiles = @(
    @{ Path = "BlueScript\Classes\ScriptProperties.cs"; Replacements = @(
        @{ From = 'List<Method> allowedMethods'; To = 'List<Type> allowedMethods' },
        @{ From = 'public List<Method> AllowedMethods'; To = 'public List<Type> AllowedMethods' }
    ) },
    @{ Path = "BlueTable\Classes\Table.cs"; Replacements = @(
        @{ From = 'Method.GetMethods('; To = 'Method.GetMethodTypes(' },
        @{ From = 'meth.Add(Method_SetError.Method);'; To = 'meth.Add(typeof(Method_SetError));' },
        @{ From = 'Method.AllMethods.FirstOrDefault(m => m.Command == "getnote")'; To = 'Method.AllMethodTypes.FirstOrDefault(t => Method.GetCommand(t) == "getnote")' }
    ) },
    @{ Path = "BlueTable\Classes\TableScriptDescription.cs"; Replacements = @(
        @{ From = 'thisc.MethodLevel >= MethodType.ManipulatesUser'; To = 'Method.GetMethodLevel(thisc) >= MethodType.ManipulatesUser' },
        @{ From = 'thisc.Command'; To = 'Method.GetCommand(thisc)' }
    ) },
    @{ Path = "BlueScript\Variables\Variable.cs"; Replacements = @(
        @{ From = 'thisc.Command'; To = 'Method.GetCommand(thisc)' }
    ) },
    @{ Path = "BlueControls\Classes\ItemCollectionPad\FunktionsItems_Formular\TimerPadItem.cs"; Replacements = @(
        @{ From = 'Method.AllMethods'; To = 'Method.AllMethodTypes' }
    ) },
    @{ Path = "BlueControls\Classes\ItemCollectionPad\FunktionsItems_Formular\ScriptButtonPadItem.cs"; Replacements = @(
        @{ From = 'Method.AllMethods'; To = 'Method.AllMethodTypes' }
    ) },
    @{ Path = "BlueControls\Controls\ConnectedFormula\RowAdder.cs"; Replacements = @(
        @{ From = 'Method.GetMethods(MethodType.Sub)'; To = 'Method.GetMethodTypes(MethodType.Sub)' }
    ) },
    @{ Path = "BlueControls\Classes\ItemCollectionPad\DynamicSymbolPadItem.cs"; Replacements = @(
        @{ From = 'Method.GetMethods(MethodType.Standard)'; To = 'Method.GetMethodTypes(MethodType.Standard)' }
    ) },
    @{ Path = "BlueControls\AdditionalScriptMethods\VariableItemCollectionPad.cs"; Replacements = @(
        @{ From = 'Method.GetMethods(MethodType.ManipulatesUser)'; To = 'Method.GetMethodTypes(MethodType.ManipulatesUser)' }
    ) },
    @{ Path = "BlueScript\Methods\Method_Do.cs"; Replacements = @(
        @{ From = 'Method_Break.Method'; To = 'typeof(Method_Break)' }
    ) },
    @{ Path = "BlueScript\Methods\Method_ForEach.cs"; Replacements = @(
        @{ From = 'Method_Break.Method'; To = 'typeof(Method_Break)' }
    ) },
    @{ Path = "BlueTable\AdditionalScriptMethods\Method_ForEachRow.cs"; Replacements = @(
        @{ From = 'Method_Break.Method'; To = 'typeof(Method_Break)' }
    ) },
    @{ Path = "BlueTable\AdditionalScriptMethods\Method_ForEachRow2.cs"; Replacements = @(
        @{ From = 'Method_Break.Method'; To = 'typeof(Method_Break)' }
    ) },
    @{ Path = "BlueControls\AdditionalScriptMethods\Method_GetNote.cs"; Replacements = @(
        @{ From = 'public static readonly Method Method = new Method_GetNote();'; To = '' }
    ) },
    @{ Path = "BlueTable\AdditionalScriptMethods\Method_SetError.cs"; Replacements = @(
        @{ From = 'public static readonly Method Method = new Method_SetError();'; To = '' }
    ) }
)

$consChanged = 0
foreach ($cf in $consumerFiles) {
    $fullPath = Join-Path $RootPath $cf.Path
    if (Test-Path $fullPath) {
        $content = [System.IO.File]::ReadAllText($fullPath)
        $original = $content
        foreach ($r in $cf.Replacements) {
            $content = $content.Replace($r.From, $r.To)
        }
        if ($content -ne $original) {
            if (-not $WhatIf) { [System.IO.File]::WriteAllText($fullPath, $content) }
            $consChanged++
            Write-Host "  Aktualisiert: $($cf.Path)" -ForegroundColor Yellow
        }
    }
}
Write-Host "  Konsumenten aktualisiert: $consChanged Dateien" -ForegroundColor Green

# ============================================================================
# Zusammenfassung
# ============================================================================
Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "ZUSAMMENFASSUNG" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Properties geaendert:   $propChanged" -ForegroundColor White
Write-Host "  Methoden umbenannt:     $methChanged" -ForegroundColor White
Write-Host "  Klassen angepasst:      $classChanged" -ForegroundColor White
Write-Host "  Singletons entfernt:      $singletonChanged" -ForegroundColor White
Write-Host "  Konsumenten aktualisiert: $consChanged" -ForegroundColor White
Write-Host ""
if ($WhatIf) {
    Write-Host "TESTMODUS (-WhatIf): Keine Dateien wurden geaendert." -ForegroundColor Yellow
} else {
    Write-Host "Fertig! Alle Ersetzungen wurden durchgefuehrt." -ForegroundColor Green
    Write-Host "Starte jetzt: dotnet build BeCreative.sln" -ForegroundColor Cyan
}
