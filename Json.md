# Jason.md — Inkrementelle Speicherung via `IJsonParseable`

> **Auftrag an die neue Instanz:** Führe den in diesem Dokument beschriebenen Plan vollständig aus.
> Arbeite Phase für Phase ab, halte dich an die Vorgaben und Konventionen, und validiere nach jeder Phase mit `dotnet build BeCreative.sln`.
> **Keine Multiple-Choice-Fragen stellen** — wenn etwas unklar ist, eine gezielte offene Frage an den Benutzer.

---

## 1. Zielsetzung

Das Interface `IJsonParseable` soll so erweitert werden, dass Objekte **nicht mehr komplett serialisiert** werden müssen, sondern gezielt eine einzelne veränderte Property in einem Sub-Baum gespeichert werden kann (inkrementelle Speicherung via Append-Log).

**Pilot-Anwendung:** `ConnectedFormula` mit ihrem großen `Pages`-Baum (`ItemCollectionPadItem` mit vielen PadItems).

**Architektur-Vorbild:** Das Append-Log-System von `TableFragments.cs` (Fragmentdateien, Hash-Dedup, periodische Konsolidierung durch Master).

---

## 2. Verbindliche Vorgaben des Benutzers

| Vorgabe | Bedeutung |
|---|---|
| **KEIN neues Interface** | Alle neuen Member kommen direkt in `IJsonParseable`. Auch `PointM` muss sie implementieren (mit `HasSubClasses => false` und leeren Stubs). |
| **`KeyName` statt `UniqueKey`** | Pfad-Identifikation erfolgt über die bestehende `IHasKeyName.KeyName`-Property. **KEIN neues Property einführen!** |
| **Append-Log pro Property** | Jede Property-Änderung wird als separater Log-Eintrag gespeichert (feingranular). |
| **Pfad key-basiert** | JSON-Pointer-ähnlich: `Items[<key>].Rotation`. **NICHT index-basiert.** |
| **Nur `IJsonParseable`/`IJsonStringable` und deren Ableitungen ändern** | Keine Migration anderer Klassen (insbesondere `IParseable`-Only-Klassen unangetastet lassen). |
| **`IParseable` bleibt unverändert** | Wird vom Benutzer zu einem späteren Zeitpunkt selbst entfernt. |
| **Subklassen dürfen ergänzt werden** | `AbstractPadItem`, `RectanglePadItem`, `ItemCollectionPadItem`, `PointM`, `ConnectedFormula` etc. dürfen angepasst werden. |

---

## 3. Architektonischer Ist-Zustand (Recherche-Ergebnisse)

### 3.1 Zwei parallele Serialisierungs-Stränge

| Interface | Serialisierung | Deserialisierung | Factory |
|---|---|---|---|
| `IParseable` (klassisch, String) | `ParseableItems() → List<string>` | `ParseThis(key, value)`, `ParseFinished(parsed)` | `ParseableItem.NewByParsing<T>(string)` |
| `IJsonParseable` (neu, JSON) | `ParseableJson() → JsonObject` | `ParseThisJson(key, value)`, `ParseFinishedJson(parsed)` | `ParseableItem.NewByParsingJson<T>(JsonElement)` |

Beide Systeme **koexistieren** absichtlich. Aktuell wird bei `ConnectedFormula` nur das String-Format gespeichert (obwohl `IJsonParseable` implementiert ist).

### 3.2 Interface-Definitionen

**`BlueBasics\Interfaces\IJsonParseable.cs`** (71 Zeilen, aktuell):

```csharp
public static class JsonParseableExtension {
    public static bool ParseJson(this IJsonParseable parsable, JsonElement toParse) {
        if (toParse.ValueKind != JsonValueKind.Object) { return false; }
        foreach (var pair in toParse.EnumerateObject()) {
            var accepted = parsable.ParseThisJson(pair.Name.ToLowerInvariant(), pair.Value);
            if (!accepted) {
                DebugPrint(ErrorType.Info, $"Nicht geparster Key: '{pair.Name}' in ...");
            }
        }
        parsable.ParseFinishedJson(toParse);
        return true;
    }
}

public interface IJsonParseable : IJsonStringable {
    void ParseFinishedJson(JsonElement parsed);
    bool ParseThisJson(string key, JsonElement value);
}
```

**`BlueBasics\Interfaces\IJsonStringable.cs`** (30 Zeilen):

```csharp
public interface IJsonStringable {
    JsonObject ParseableJson();
}
```

**`BlueBasics\Interfaces\IParseable.cs`** (analog, mit `ParseableExtension.Parse`-Methode).

### 3.3 `ParseableItem` (abstrakte Basisklasse)

**Datei:** `BlueBasics\Classes\ParseableItem.cs` (155 Zeilen)

- Implementiert **nur `IParseable`** (nicht `IJsonParseable`)
- Stellt **beide** Factory-Methoden bereit: `NewByParsing<T>(string)` und `NewByParsingJson<T>(JsonElement)`
- Typ-Auflösung über statische `ClassId`-Property der konkreten Subklasse via Reflection (`Generic.GetEnumerableOfType<T>()`)
- `Clone()` macht Serialisierungs-Roundtrip
- Default `ParseableItems()` liefert nur `ClassId`
- **WICHTIG:** Default-Implementierungen für die NEUEN Member (`HasSubClasses`, `BuildJsonForKey`, `SubItemChanged`) hier als virtuelle Defaults hinzufügen!

### 3.4 JSON-Hilfs-Infrastruktur

**Datei:** `BlueBasics\Extensions\JsonExtensions.cs` (190 Zeilen)

Wichtige Methoden:
- `GetBool/GetInt/GetFloat/GetString/GetEnum` — typsicheres Lesen mit Default-Werten
- `Set(key, JsonNode?)` — fluent `JsonObject[key] = value`
- `SetArrayIfNotEmpty<T>(key, IEnumerable<T>)` — baut JsonArray aus `IJsonStringable`-Liste
- `IsArray` / `IsObject`

**Es gibt KEINE eigene `Json`/`JSON`-Klasse** — alles via Extensions.

### 3.5 Aktuell `IJsonParseable`-implementierende Klassen

| Klasse | Datei | Zeile | Besonderheit |
|---|---|---|---|
| `AbstractPadItem` (abstract) | `BlueControls\Classes\ItemCollectionPad\Abstract\AbstractPadItem.cs` | 12 | erbt `ParseableItem` + `IJsonParseable`. Liefert Default-Impl für JSON (virtuell) |
| `RectanglePadItem` (abstract) | `…\Abstract\RectanglePadItem.cs` | — | Vererbungs-Dispatch mit `base.` |
| `FixedRectanglePadItem` | `…\Abstract\FixedRectanglePadItem.cs` | 146, 152 | |
| `ItemCollectionPadItem` (sealed, **rekursiv**) | `BlueControls\Classes\ItemCollectionPad\ItemCollectionPadItem.cs` | 17, 751, 853 | komplexe Implementierung |
| `PointM` (sealed) | `BlueControls\Classes\PointM.cs` | 10, 212, 278 | **ohne `ParseableItem`** — implementiert IParseable+IJsonParseable direkt |
| `ConnectedFormula` (sealed) | `BlueControls\Controls\ConnectedFormula\ConnectedFormula.cs` | 22, 299, 469 | **ohne `ParseableItem`** — Basisklasse `CachedFile` |

### 3.6 Detail: `PointM` (einfachster Fall)

**Datei:** `BlueControls\Classes\PointM.cs`

- **Properties** (für `BuildJsonForKey` relevant): `name`, `x`, `y`, `distance`, `angle`, `parentName`
- `ParseableJson()` in Zeile 212-227
- `ParseThisJson(key, value)` in Zeile 278-306
- `ParseFinishedJson` leer
- **Für diesen Plan:** `HasSubClasses => false`, `SubItemChanged` leeres Event, `BuildJsonForKey` Switch über die 6 Properties.

### 3.7 Detail: `AbstractPadItem`

**Datei:** `BlueControls\Classes\ItemCollectionPad\Abstract\AbstractPadItem.cs` (727 Zeilen)

- Properties (für `BuildJsonForKey` relevant): `type`, `key` (KeyName), `enabled`, `print` (`_beiExportSichtbar`), `quickInfo`, `page`
- **Sub-Collections:** `MovablePoint` (Liste von PointM), `JointPoints`, `Tags`
- `ParseableJson()` in Zeile 376-390 (virtuell)
- `ParseThisJson(key, value)` in Zeile 476-540 (virtuell, behandelt Sub-Objekte via `ParseJson`-Rekursion)
- **Für diesen Plan:** `HasSubClasses => true`. Sub-Collections' `SubItemChanged` abonnieren, Bubbling mit Präfix `Points[<key>].` bzw. `JointPoints[<key>].`. In Property-Settern (`_enabled`, `_beiExportSichtbar`, `KeyName`, `QuickInfo`, `Page`) `RaiseSubItemChanged` aufrufen.

### 3.8 Detail: `RectanglePadItem`, `FixedRectanglePadItem`

- Vererbungs-Dispatch wie bei `ParseThisJson` (mit `base.BuildJsonForKey`)
- Eigene Properties ergänzen (z.B. `_drehwinkel` / `rotation` in `RectanglePadItem`)

### 3.9 Detail: `ItemCollectionPadItem` (Container)

**Datei:** `BlueControls\Classes\ItemCollectionPad\ItemCollectionPadItem.cs` (1394 Zeilen)

- `_internal`: `ObservableCollection<AbstractPadItem>` (Zeile 24) — die eigentliche Child-Liste
- Kann selbst wieder `ItemCollectionPadItem` enthalten (Verschachtelung!)
- sealed
- **Für diesen Plan:** Bei Child-Add/Remove `SubItemChanged` abonnieren bzw. abmelden (klassisch, **KEINE Lambdas** — `AGENTS.md`!). Bubbling: präfixt Pfad mit `Items[<childKey>].`. Eigene Properties ebenfalls raisen.

### 3.10 Detail: `ConnectedFormula` (Root, Hauptprofiteur)

**Datei:** `BlueControls\Controls\ConnectedFormula\ConnectedFormula.cs` (608 Zeilen)

- Basisklasse: `CachedFile` (`BlueBasics\Classes\FileSystemCaching\CachedFile.cs`, 743 Zeilen)
- Implementiert u.a. `IParseable, IJsonParseable, IMultiUserCapable, INotifyPropertyChanged`
- **Zentraler teurer Member:** `Pages` (ItemCollectionPadItem?), Zeile 106-127
- **Pages-Getter** (Zeile 106-127): Lazy Load via `this.Parse(Constants.Win1252.GetString(Content))` bei `!IsParsed`
- `ParseableItems()` in Zeile 270-286 (alt, String)
- `ParseableJson()` in Zeile 299-313
- `ParseThis(key, value)` in Zeile 430-467
- `ParseThisJson(key, value)` in Zeile 469-503
- **`ParseFinished(parsed)` in Zeile 318-423 — teure Reparatur-Logik** (stellt sicher, dass Head existiert, Sub-Collections valide sind, jede Page ein `RowEntryPadItem` hat, Parent-Referenzen korrekt sind). Wird nur bei Vollladung aufgerufen, nicht bei partiellem Update.
- **`OnPropertyChanged` in Zeile 575-588 — aktuell VOLL-SERIALISATION:**

```csharp
private void OnPropertyChanged([CallerMemberName] string propertyName = "unknown") {
    if (IsDisposed) { return; }
    if (IsSaving || IsLoading || _finishingParse || !IsParsed) { return; }

    if (((IMultiUserCapable)this).AcquireWriteAccess() is { Length: > 0 } f) {
        Develop.DebugError(...);
        return;
    }

    var text = ParseableItems().FinishParseable();   // ← ALT, VOLL
    Content = Constants.Win1252.GetBytes(text);
    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
```

- `OnReleasingWriteAccess()` (Zeile 263-265) → ruft `Save()` auf, wenn MultiUser-Lock freigegeben wird
- `_liveInstances` (Zeile 30) — `ConcurrentDictionary<string, ConnectedFormula>` Cache
- `StandardFormulaFile` etc. sind nur Strings, keine ConnectedFormula-Objekte

### 3.11 `TableFragments` als Vorbild für das Append-Log

**Datei:** `BlueTable\Classes\TableFragments.cs` (563 Zeilen)

- Statisches Config: `DeleteFragmentsAfter = 130` (Min.), `DoComplete = 60` (Min.), `SuffixOfFragments = "frg"`
- Pro aktiver User-Session: eine persönliche `.frg`-Datei (Append-Modus, `FileShare.Read`)
- `_writer: StreamWriter?` (Zeile 66) — offenes File-Handle
- `_processedHashes: ConcurrentDictionary<string, byte>` (Zeile 50) — Dedup-Cache
- `WriteValueToDiscOrServer` (Zeile 249-278) — Anhängen ans Fragment
- `BeSureToBeUpToDate` (Zeile 124-169) — Neu-Einlesen der Fragmente
- `GetLastChanges` (Zeile 389-424) — Liest alle .frg-Dateien, dedup über `_processedHashes`
- `DoWorkAfterLastChanges` (Zeile 314-379) — Master konsolidiert alle 60 Min. in Hauptdatei
- Hash-Logik in `UndoItem.Hash()` (`BlueTable\Classes\UndoItem.cs:60-69`)

---

## 4. Konventionen (aus `AGENTS.md` — zwingend einhalten!)

- **File-scoped Namespaces** (`namespace Foo;`)
- **Ägyptische Klammern**: `void Foo() {` — keine neue Zeile vor `{`
- **Keine neue Zeile vor** `else`, `catch`, `finally`
- **`using` außerhalb des Namespace**
- **`var` bevorzugt** (auch bei built-in Typen)
- **Primary Constructors** bevorzugt
- **Switch Statements**, NICHT Switch Expressions
- **Geschweifte Klammern immer** (auch bei Single-Statement)
- **Event-Handler klassisch verdrahten** — `obj.Event += HandlerMethod;` statt Lambdas. Ausnahme nur bei zwingendem lokalen Variablen-Zugriff.
- **4 Leerzeichen Einrückung, CRLF, keine Tabs**
- **`readonly`-Felder** bevorzugt
- **KEINE optionalen Parameter** neu einführen!
- **`AsSpan()` / Range-Syntax `[x..y]`** statt `Substring`
- **Nullable Reference Types** aktiviert — Warnungen **niemals** mit `!` unterdrücken
- **XML-Doc-Kommentare**: Bestehende erhalten/überarbeiten, nicht löschen
- **Alle `.cs`-Dateien UTF-8 mit BOM**
- **Helper-Klassen nutzen**: `Develop`, `Converter`, `IO`, `Generic`, `Geometry`, `Constants`, `OperationResult` — nicht die System-Äquivalente

### Selbstständige Reparaturen bei Dateiänderungen:
- Überflüssige String-Checks vermeiden (Pattern `if (s is { Length: > 0 } f) {return f;}`)
- Unbenutzte Felder/Routinen: `// TODO: Unused` außer es steht schon `Used` dort
- Felder statt redundanter lokaler Variablen nutzen
- Fehlende Nullable-Prüfungen hinzufügen

### GIT-Regeln:
- **NIEMALS eigenständig Git rückgängig machen!** Auch nicht eigene Änderungen!
- **NIEMALS `git stash` nutzen!**
- Lieber Arbeit beenden und nachfragen.

### Wenn ratlos:
- Develop.Diagnose-Aufrufe einbauen, `DiagFlag = true` setzen
- Vor dem Löschen von Debug-Code den Benutzer fragen

---

## 5. Implementierungs-Plan

### Phase 1: Interface- & Infrastruktur-Erweiterung (BlueBasics)

#### 1.1 `IJsonParseable` erweitern
**Datei:** `BlueBasics\Interfaces\IJsonParseable.cs`

Neue Interface-Member hinzufügen:

```csharp
public interface IJsonParseable : IJsonStringable {
    void ParseFinishedJson(JsonElement parsed);
    bool ParseThisJson(string key, JsonElement value);

    // NEU:
    bool HasSubClasses { get; }
    JsonObject? BuildJsonForKey(string key);
    event EventHandler<JsonPathChangedEventArgs>? SubItemChanged;
}
```

#### 1.2 Neue EventArgs-Klasse
**Datei:** neu — `BlueBasics\Classes\JsonPathChangedEventArgs.cs` (oder in `IJsonParseable.cs` direkt)

```csharp
public class JsonPathChangedEventArgs : EventArgs {
    public string RelativePath { get; init; }    // vom Auslöser zum Abonnenten, z.B. "Rotation"
    public JsonObject Partial { get; init; }     // partielles JSON für genau diese Property
    public string SourceKey { get; init; }       // KeyName des ursprünglichen Auslösers
    // Primary Constructor / Init-Pattern gemäß Code-Stil
}
```

#### 1.3 Erweiterungsmethoden in `JsonParseableExtension`
**Datei:** `BlueBasics\Interfaces\IJsonParseable.cs`

Neue statische Methoden:

```csharp
// Löst einen key-basierten Pfad wie "Items[btnSubmit].Rotation" auf und wendet
// value über ParseThisJson an. Gibt false zurück, wenn Pfad nicht auflösbar.
public static bool ApplyPartialJson(this IJsonParseable root, string path, JsonElement value);

// Schutz-Methode für Subklassen: Bubbelt das Event nach oben und präfixt den Pfad
// mit dem eigenen KeyName. Ignoriert Aufrufe, wenn HasSubClasses false ist.
public static void RaiseSubItemChanged(IJsonParseable sender, string relativePath, JsonObject partial);
```

**Pfad-Resolver-Logik:**
- Erster Token bis `[` oder `.` ist der Container-Name (z.B. `Items`)
- Token in `[...]` ist der `KeyName` des Child
- Rest nach `.` ist rekursiv der Restpfad
- Am Blatt: `ParseThisJson(key, value)` aufrufen

#### 1.4 Klasse `JsonAppendLog` (neu)
**Datei:** neu — `BlueBasics\Classes\JsonAppendLog.cs`

Orientiert am `TableFragments._writer`-Pattern, aber generisch:

- `StreamWriter` offen im Append-Modus (`FileShare.Read`)
- Format pro Zeile (UTF-8): `{"path":"…","value":<json>,"hash":"…"}`
- Hash via `Generic.GetSHA256HashString()` (analog `UndoItem.Hash()`)
- Thread-safe via `SemaphoreSlim` (analog `CachedFile.cs`)

Methoden:
- `void Append(string path, JsonElement value)` — schreibt Zeile + Hash
- `IEnumerable<(string path, JsonElement value)> ReadAllChanges()` — liest Log
- `void Clear()` — nach Konsolidierung
- `void ConsolidateInto(JsonObject root)` — wendet alle Änderungen auf root an, leert Log

**Konstruktor:** nimmt Dateipfad entgegen.

#### 1.5 Default-Implementierungen in `ParseableItem`
**Datei:** `BlueBasics\Classes\ParseableItem.cs`

Virtuelle Defaults für die neuen Member, damit bestehende Subklassen nicht brechen:

```csharp
public virtual bool HasSubClasses => false;
public virtual JsonObject? BuildJsonForKey(string key) => null;
public event EventHandler<JsonPathChangedEventArgs>? SubItemChanged;
```

**Achtung:** `PointM` und `ConnectedFormula` erben **NICHT** von `ParseableItem` — sie müssen die neuen Member selbst implementieren.

#### ✅ Build-Check nach Phase 1
`dotnet build BeCreative.sln` muss ohne Fehler durchlaufen (Defaults verhindern Brüche).

---

### Phase 2: Migration der einfachsten Klasse — `PointM`

**Datei:** `BlueControls\Classes\PointM.cs`

- `HasSubClasses => false`
- `BuildJsonForKey(key)` implementieren: Switch über `name`, `x`, `y`, `distance`, `angle`, `parentName` (analog `ParseableJson()` Zeile 212-227, aber nur eine Property)
- `SubItemChanged` als leeres Event deklarieren (wird nie gefeuert)

#### ✅ Build-Check nach Phase 2

---

### Phase 3: Migration der PadItems

#### 3.1 `AbstractPadItem`
**Datei:** `BlueControls\Classes\ItemCollectionPad\Abstract\AbstractPadItem.cs`

- `HasSubClasses => true` (override)
- `BuildJsonForKey(key)` virtuell implementieren: Switch extrahiert aus `ParseableJson()` (Zeile 376-390), gibt `JsonObject` mit genau einer Property zurück oder `null` bei unbekanntem Key
- In Property-Settern (`_enabled`, `_beiExportSichtbar`, `KeyName`, `QuickInfo`, `Page`): `JsonParseableExtension.RaiseSubItemChanged(this, "<propName>", partial)` aufrufen
- Konstruktor: `SubItemChanged` von jedem `MovablePoint` und jedem `JointPoint` abonnieren (klassisch: `point.SubItemChanged += Point_SubItemChanged;`)
- Handler-Methode `Point_SubItemChanged`: Präfix mit `Points[<pointKey>].` und eigenes Event raisen

#### 3.2 `RectanglePadItem`
**Datei:** `BlueControls\Classes\ItemCollectionPad\Abstract\RectanglePadItem.cs`

- `BuildJsonForKey` überschreiben: eigenen Key behandeln (`rotation` / `_drehwinkel`), sonst `base.BuildJsonForKey(key)`
- Setter von `_drehwinkel`: `RaiseSubItemChanged(this, "rotation", partial)`

#### 3.3 `FixedRectanglePadItem`, `FixedRectangleBitmapPadItem`
**Dateien:** entsprechend

- Gleiche Pattern wie 3.2 für ihre eigenen Properties

#### 3.4 `ItemCollectionPadItem` (Container)
**Datei:** `BlueControls\Classes\ItemCollectionPad\ItemCollectionPadItem.cs`

- `HasSubClasses => true`
- Bei Child-Add/Remove (in den jeweiligen Add/Remove-Methoden von `_internal`):
  - Add: `child.SubItemChanged += Child_SubItemChanged;`
  - Remove: `child.SubItemChanged -= Child_SubItemChanged;`
- Handler `Child_SubItemChanged(sender, e)`:
  - Eigenen Pfad berechnen: `Items[<childKey>].<e.RelativePath>`
  - `SubItemChanged?.Invoke(this, new JsonPathChangedEventArgs { ... })` raisen
- Eigene Properties raisen

#### ✅ Build-Check nach Phase 3

---

### Phase 4: Integration in `ConnectedFormula` (Root, Append-Log-Verwaltung)

**Datei:** `BlueControls\Controls\ConnectedFormula\ConnectedFormula.cs`

#### 4.1 Neues privates Feld & Property
```csharp
private JsonAppendLog? _changeLog;
```

#### 4.2 Log-Datei verwalten
- Pfad: neben `.cfo`-Datei, Suffix `.cfolog`
- Anlegen/Laden beim ersten `Pages`-Zugriff
- Schließen in `Dispose`

#### 4.3 `OnPropertyChanged` umstellen (Zeile 575-588)

**Vorher:** Komplette Serialisierung via `ParseableItems().FinishParseable()`.

**Nachher:**
- Wenn das PropertyChanged von einem SubItem stammt (via `SubItemChanged`-Event): nur `path` + `partial` ans `_changeLog.Append(...)` anhängen
- Wenn das PropertyChanged eine direkte Property von `ConnectedFormula` selbst ist: genauso verfahren (Path ist nur der Property-Name)
- `Content` nicht mehr überschreiben — nur noch `Save()` am Ende (Lock-Release) konsolidiert

#### 4.4 `Pages`-Getter erweitern (Zeile 106-127)

Beim ersten Laden (Lazy):
1. Bisheriges `this.Parse(...)` ausführen
2. Falls `.cfolog` existiert: alle Einträge via `this.ApplyPartialJson(path, value)` anwenden
3. `IsParsed = true` setzen

#### 4.5 SubItemChanged-Abonnement
- In `Pages`-Setter / beim Aufbau: `Pages.SubItemChanged += Pages_SubItemChanged;`
- `Pages_SubItemChanged`: leitet an `_changeLog.Append(...)` weiter

#### 4.6 Periodische Konsolidierung
- Analog `TableFragments.DoWorkAfterLastChanges` (Zeile 314-379)
- Timer oder bei `OnReleasingWriteAccess`: wenn Master und älter als X / Log größer als Y:
  - Alle Log-Einträge auf vollständiges JSON anwenden
  - `Content = Win1252.GetBytes(ParseableItems().FinishParseable())` (vollständige Speicherung)
  - `_changeLog.Clear()`
  - `Save()`

#### 4.7 Multi-User-Koexistenz
- Bestehendes `IMultiUserCapable`-Locking wiederverwenden
- `.cfolog` pro User-Session (wie `TableFragments._myFragmentsFilename`)
- Beim Laden: alle `.cfolog`-Dateien der anderen User auch berücksichtigen (analog `GetLastChanges`)

#### ✅ Build-Check nach Phase 4

---

### Phase 5: Manuelle Validierung

**Keine CI vorhanden — alles manuell.**

#### Test-Checkliste:
1. **Roundtrip-Test ConnectedFormula:**
   - Bestehende `.cfo` öffnen, Property ändern, Datei schließen, neu öffnen → Änderung da?
   - `.cfolog` prüfen (nur der eine Eintrag sollte drin sein)
2. **Konsolidierung testen:** Master-Wechsel erzwingen, prüfen, dass `.cfolog` leer und `.cfo` aktualisiert ist.
3. **Sub-PadItem-Änderung:** z.B. `Rotation` eines Buttons ändern → nur dieser Pfad im Log, nicht die ganze Page.
4. **Klassisches Laden:** Alte `.cfo` ohne Log muss weiterhin funktionieren (Abwärtskompatibilität!).
5. **PointM:** Bei `HasSubClasses == false` darf `SubItemChanged` nie gefeuert werden und `BuildJsonForKey` trotzdem korrekt arbeiten.
6. **Build-Prüfung:** `dotnet build BeCreative.sln` muss ohne Fehler durchlaufen.
7. **Abwärtskompatibilität IParseable:** Bestehende `.cfo`-Dateien (String-Format) müssen weiterhin ladbar bleiben.

---

## 6. Betroffene Dateien (komplette Liste)

### Neu:
- `BlueBasics\Classes\JsonAppendLog.cs`
- `BlueBasics\Classes\JsonPathChangedEventArgs.cs` (oder innerhalb `IJsonParseable.cs`)

### Geändert (BlueBasics):
- `BlueBasics\Interfaces\IJsonParseable.cs` — Interface + Erweiterungsmethoden
- `BlueBasics\Classes\ParseableItem.cs` — Default-Impl für neue Member

### Geändert (BlueControls):
- `BlueControls\Classes\PointM.cs`
- `BlueControls\Classes\ItemCollectionPad\Abstract\AbstractPadItem.cs`
- `BlueControls\Classes\ItemCollectionPad\Abstract\RectanglePadItem.cs`
- `BlueControls\Classes\ItemCollectionPad\Abstract\FixedRectanglePadItem.cs`
- `BlueControls\Classes\ItemCollectionPad\Abstract\FixedRectangleBitmapPadItem.cs` (falls vorhanden)
- `BlueControls\Classes\ItemCollectionPad\ItemCollectionPadItem.cs`
- `BlueControls\Controls\ConnectedFormula\ConnectedFormula.cs`

### Unangetastet (Vorgabe!):
- Alle `IParseable`-Only-Klassen (`Variable`, `UniqueValueDefinition`, `Renderer_Abstract`, `TableScriptDescription`, `ColumnViewCollection`, `ColumnViewItem`, `RowSortDefinition`, `UndoItem`)
- `IParseable` selbst
- Die gesamte Table-Hierarchie (`Table`, `TableFile`, `TableChunk`, `TableFragments`, `ColumnItem`, `RowItem`, `CellCollection`)

---

## 7. Risiken & Mitigationen

| Risiko | Mitigation |
|---|---|
| `KeyName` ist nicht eindeutig (zwei PadItems mit gleichem Key in derselben Collection) | Vor Phase 3 prüfen: ist `KeyName`-Eindeutigkeit innerhalb einer `ItemCollectionPadItem` garantiert? Ggf. `Develop.DebugPrint` bei Duplikaten im Debug einbauen. |
| `ParseFinished` (ConnectedFormula Zeile 318-423) macht teure Reparaturen — kollidiert mit partiellem Update? | Partielles Update geht über `ApplyPartialJson`, das `ParseFinished` **nicht** aufruft. Nur bei Vollladung. Vorher validieren, dass keine inkonsistenten Zustände entstehen. |
| `ItemCollectionPadItem` ist `sealed` — funktioniert Vererbung? | Ja — `ItemCollectionPadItem` implementiert `IJsonParseable` direkt (nicht via `AbstractPadItem`). Neue Member direkt dort implementieren. |
| Memory-Leaks durch nicht abbestellte Event-Abonnements | Konsistente Subscribe/Unsubscribe-Paare in Add/Remove-Methoden. In `Dispose` alle Abonnements abbestellen. |
| Append-Log wächst ungebremst | Automatische Konsolidierung nach Zeit/Limit (wie `TableFragments.DoComplete = 60` Min.). |
| Race Conditions zwischen Append und Konsolidierung | `SemaphoreSlim` (wie `CachedFile`), Locking während Schreiboperationen. |
| Abwärtskompatibilität: alte `.cfo` ohne `.cfolog` | In `Pages`-Getter prüfen: existiert Log? Wenn ja, anwenden; wenn nein, klassisch laden. |

---

## 8. Empfohlene Implementierungs-Reihenfolge

1. **Phase 1:** Interface-Erweiterung + `JsonAppendLog` + Defaults in `ParseableItem` → **Build grün**
2. **Phase 2:** `PointM` als einfachsten Fall → **Build grün, manuell testen**
3. **Phase 3.1-3.3:** `AbstractPadItem` + `RectanglePadItem` + `FixedRectanglePadItem`
4. **Phase 3.4:** `ItemCollectionPadItem` (Container, Bubbling) → **Build grün**
5. **Phase 4:** `ConnectedFormula` (Root, Append-Log-Integration, Konsolidierung) → **Build grün**
6. **Phase 5:** Manuelle Tests nach Checkliste

**Nach JEDEM Phasen-Abschluss:** `dotnet build BeCreative.sln` ausführen und ohne Fehler durchlaufen lassen.

---

## 9. Hilfsklassen & zentrale Einstiegspunkte (aus `AGENTS.md`)

| Klasse | Zweck |
|---|---|
| `Develop` | Logging (`DebugPrint()`, `Message()`), App-Start, Prozess-Steuerung |
| `Converter` | Sichere Typkonvertierung (`IntParse()`, `DoubleParse()`, …) |
| `IO` | Datei-I/O mit Retry-Logik (`FileExists()`, `ReadAllText()`, `DeleteFile()`, `CanWriteFile()`) |
| `Generic` | `GetUniqueKey()`, `GetMD5Hash()`, `GetSHA256HashString()`, `LaunchBrowser()` |
| `Constants` | Globale Konstanten, Zeichensätze, DateTime-Formate, `Win1252`-Encoding |
| `OperationResult` | Ergebnis-Typ für Dateioperationen |

JSON-Extensions in `BlueBasics\Extensions\JsonExtensions.cs`: `GetBool/GetInt/GetFloat/GetString/GetEnum/GetJson`, `Set(key, JsonNode?)`, `SetArrayIfNotEmpty<T>`, `IsArray`, `IsObject`.

---

## 10. Wichtige Hinweise für die Arbeit

- **Frage nicht mit Multiple Choice**, sondern offen, falls etwas unklar ist.
- **Keine Code-Kommentare** hinzufügen, außer der Benutzer fordert es (`AGENTS.md`: "DO NOT ADD ANY COMMENTS unless asked").
- **Bestehende Kommentare erhalten** und ggf. aktualisieren.
- **Kein `Console.WriteLine`** und kein unnötiges `Develop.DebugPrint` (nur bei echten Fehlern).
- **Vor dem Löschen von Debug-Code den Benutzer fragen.**
- **Bei Ratlosigkeit:** `Develop.Diagnose`-Aufrufe einbauen, `DiagFlag = true` setzen.
- **Datei-Encoding:** Alle `.cs`-Dateien UTF-8 mit BOM. Ggf. `Convert-Encoding.ps1` laufen lassen.
- **Git:** NIEMALS eigenständig rückgängig, NIEMALS `stash`.

---

**Ende des Dokuments. Jetzt: Loslegen!**
