# Json.md — Inkrementelle Speicherung via `IJsonParseable`

Kurz-Dokumentation der implementierten Architektur.
Pilot-Anwendung: `ConnectedFormula` mit ihrem `Pages`-Baum.

> **Aktueller Status (Stand 07/2026):** Das Append-Log (`JsonAppendLog`,
> Abschnitte 6 und 7) ist in `ConnectedFormula` **nicht** aktiviert. Die
> Konsolidierung erfolgt ausschließlich über eine Voll-Serialisierung in
> `BuildContent` beim Speichern. Sub-Item-Änderungen aus dem `Pages`-Baum
> werden über `PropertyChangedExt` bis zur Wurzel (`ConnectedFormula`)
> durchgereicht und lösen dort lediglich `MarkDirty` aus — ein gezieltes
> Patchen einzelner Properties entfällt damit vorerst. Die Klasse
> `JsonAppendLog` bleibt als wiederverwendbare Komponente für eine spätere
> inkrementelle Speicherung erhalten.

---

## 1. Ziel

Einzelne veränderte Properties gezielt speichern (Append-Log), statt immer den ganzen Baum zu serialisieren.
Vorbild: `TableFragments`-Append-Log.

---

## 2. Die Interface-Member

Aufgeteilt auf **zwei Interfaces** in `BlueBasics.Interfaces`:

### `IJsonStringable` (reine Serialisierung)

| Member | Zweck |
|--------|-------|
| `JsonObject ParseableJson()` | Serialisiert den gesamten Zustand in ein frisches `JsonObject`. |

### `IJsonParseable : IJsonStringable` (Serialization + Parsing + Änderungs-Tracking)

| Member | Zweck |
|--------|-------|
| `bool HasSubClasses` | `true` wenn das Objekt Sub-Items enthalten kann. Leaf-Klassen (PointM) → `false`. |
| `event EventHandler<JsonPathChangedEventArgs>? SubItemChanged` | Bubbelt Property-Änderungen nach oben. EventArgs: `RelativePath`, `Partial`, `SourceKey`. |
| `IJsonParseable? GetSubItemByKey(string containerName, string key)` | Löst einen Pfad-Knoten auf (z.B. Container `"Items"`, Key `"btnSubmit"`). |
| `void ParseThisJson(JsonObject json)` | Spiegelt `ParseableJson()`: die Implementierung liest ihre eigenen Keys direkt aus `json` (z.B. `_drehwinkel = json.GetInt("rotation");`) und ruft am Ende `base.ParseThisJson(json)` auf. Partial-Updates funktionieren automatisch — nicht vorhandene Keys werden übersprungen. Keys sind kleingeschrieben. |
| `void RaiseSubItemChanged(string relativePath, object? value)` | Löst `SubItemChanged` aus. `value` ist entweder ein primitives Objekt (`int`, `string`, `bool`, …) — dann wird daraus per Extension das Partial-JSON gebaut — oder ein fertiges `JsonObject` für Bubbling. |

`ParseableItem` liefert virtuelle Defaults (`false` / `null`), sodass bestehende Subklassen nicht brechen. `ParseableItem` implementiert `IJsonParseable` formal **nicht** (es fehlen ParseThisJson/ParseFinishedJson), nutzt aber über die interne Helper `JsonParseableExtension.BuildPartialJson` dieselbe Logik.
`PointM` und `ConnectedFormula` erben **nicht** von `ParseableItem` und implementieren alle Member selbst — inkl. `RaiseSubItemChanged` als Einzeiler, der an `JsonParseableExtension.BuildSubItemEventArgs` delegiert.

---

## 3. Pfad-Format

Key-basiert (JSON-Pointer-ähnlich), **niemals index-basiert**:

```
Items[btnSubmit].Rotation
Items[page1].Points[LO].X
Caption                       ← Pages-eigene Property (ohne Container-Präfix)
CreateDate                    ← ConnectedFormula-eigene Property
```

Aufgelöst wird durch `JsonParseableExtension.ApplyPartialJson(root, path, value)`:
erstes Token bis `[` = Container-Name → `GetSubItemByKey` → rekursiv absteigen → am Blatt wird `{leafName: value}` als `JsonObject` an `ParseThisJson` übergeben.

---

## 4. Bubbling (wie Events nach oben wandern)

```
PointM.X ändert sich
  → (aktuell nicht gefeuert, PointM.HasSubClasses = false)
AbstractPadItem.Drehwinkel ändert sich
  → RaiseSubItemChanged("rotation", _drehwinkel)
  → SubItemChanged-Event feuert (Partial = {"rotation": 45})
ItemCollectionPadItem (Container) fängt ab
  → Child_SubItemChanged: präfixt mit "Items[<childKey>]."
  → raist eigenes SubItemChanged mit "Items[btnSubmit].rotation"
    (value = e.Partial, also das fertige JsonObject — wird 1:1 durchgereicht)
ConnectedFormula fängt ab
  → Pages_SubItemChanged → _changeLog.Append(path, value)
```

Container müssen in `Add`/`Remove` (bzw. `Point_CollectionChanged`) `SubItemChanged` abonnieren/abmelden — **klassisch, keine Lambdas**.

---

## 5. RaiseSubItemChanged in Property-Settern

Jeder relevante Property-Setter ruft nach der Wertänderung auf:

```csharp
RaiseSubItemChanged("<propertyName>", <newValue>);
// z.B.
RaiseSubItemChanged("rotation", _drehwinkel);
RaiseSubItemChanged("enabled", _enabled);
RaiseSubItemChanged("key", value);
```

`RaiseSubItemChanged` ist im Interface `IJsonParseable` deklariert und in jeder
Implementierung (ParseableItem, PointM, ConnectedFormula) ein Einzeiler, der die
EventArgs-Erzeugung an die Extension `JsonParseableExtension.BuildSubItemEventArgs`
bzw. (für ParseableItem) an die interne Helper-Methode `BuildPartialJson` delegiert.

Der `object?`-Wert wird von der Helper-Methode in ein `JsonObject` umgewandelt:
- Ist der Wert bereits ein `JsonObject` (beim Bubbling aus Sub-Containern), wird
  er unverändert übernommen.
- Sonst dient das letzte Pfad-Segment als Key und der Wert wird als `JsonNode`
  eingetragen: `"rotation"` + `45` → `{"rotation": 45}`.

Beim Bubbling reicht der Container das Partial einfach durch:

```csharp
RaiseSubItemChanged($"Items[{child.KeyName}].{e.RelativePath}", e.Partial);
```

---

## 6. Append-Log (`JsonAppendLog`)

**Datei:** `BlueBasics\Classes\JsonAppendLog.cs`

- Pro-Session StreamWriter im Append-Modus (`FileShare.Read`), thread-safe via `SemaphoreSlim`.
- Format pro Zeile: `{"path":"…","value":<json>,"hash":"…"}`
- `Append(path, value)` — hängt Änderung an.
- `ReadAllChanges(filename)` — statisch, liest alle Einträge.
- `Clear()` — schließt Writer, löscht Datei (nach Konsolidierung).

Log-Datei für ConnectedFormula: `<filename>.cfolog` (also `.cfo` + `log`).

---

## 7. Integration in `ConnectedFormula`

| Was | Wo |
|-----|-----|
| Change-Log anlegen | `EnsureChangeLog()` — lazy beim ersten Append |
| Beim Laden anwenden | `Pages`-Getter: nach `this.Parse(...)` → `ApplyChangeLog()` |
| Sub-Item-Änderungen loggen | `Pages_SubItemChanged` → extrahiert Wert aus Partial → `_changeLog.Append` |
| Eigene Property-Änderungen loggen | Property-Setter ruft `LogChange(jsonKey, value)` — Wert wird vom Setter explizit übergeben |
| Konsolidierung | `OnSaved()` → `_changeLog.Clear()` (Content wurde via `BuildContent` voll serialisiert) |
| Cleanup | `Dispose` → `_changeLog.Dispose()` |

`ApplyChangeLog()` setzt `_applyingChangeLog = true`, damit während des Applys keine neuen Log-Einträge oder `MarkDirty`-Aufrufe entstehen.

**Apply-Logik für verschiedene Pfad-Typen:**
- Pfad ohne `[`: Routing über bekannte Self-Keys (`type`, `version`, `createDate`, `creator`, `notAllowedChilds`, `page` — vgl. `ParseableJson`). Ist der Key darunter, wird `{key: value}` an `this.ParseThisJson` übergeben, sonst an `Pages.ParseThisJson`.
- Pfad mit `[`: über `Pages.ApplyPartialJson(path, value)` in den Sub-Baum absteigen.

---

## 8. Was beachten?

- **KeyName-Eindeutigkeit** innerhalb einer Collection muss gegeben sein (sonst Pfad nicht auflösbar).
- **`ParseFinished`** (teure Reparatur-Logik in ConnectedFormula) wird bei partiellem Update **nicht** aufgerufen — nur bei Vollladung.
- **PointM** hat `HasSubClasses = false`, `SubItemChanged` wird deklariert aber nie gefeuert. Changes werden über bestehendes `Moved`-Event vom Parent verarbeitet.
- **Abwärtskompatibilität:** Alte `.cfo` ohne `.cfolog` funktionieren — `ApplyChangeLog` prüft Datei-Existenz.
- **Multi-User** (Phase 4.7): aktuell Single-User (eine `.cfolog` pro Datei). Für Multi-User müssten pro-Session-Dateien wie bei `TableFragments` angelegt werden.
- **`IParseable` bleibt unangetastet** — beide Systeme koexistieren.

---

## 9. Betroffene Dateien

**Neu:** `JsonAppendLog.cs`, `JsonPathChangedEventArgs` (in `IJsonParseable.cs`),
`IJsonStringable.cs` (Interface für `ParseableJson()`).

**Geändert:**
- `IJsonParseable.cs` — Interface (inkl. `RaiseSubItemChanged`), Extension `BuildSubItemEventArgs` + interne Helper `BuildPartialJson`/`AsJsonNode`, `ApplyPartialJson`
- `ParseableItem.cs` — Defaults + `RaiseSubItemChanged` (public, neue Signatur mit `object?`)
- `PointM.cs`, `AbstractPadItem.cs`, `RectanglePadItem.cs`, `FixedRectanglePadItem.cs`
- `ItemCollectionPadItem.cs` — Container-Bubbling
- `ConnectedFormula.cs` — Append-Log-Integration + `RaiseSubItemChanged`-Implementierung
