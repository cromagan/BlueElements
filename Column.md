# Column.md — `EditStrategy` ersetzt `EditableWithDropdown` / `EditableWithTextInput`

> **Wichtig: Keine Rückfragen erlaubt.** Dieser Plan wird ohne Nachfragen umgesetzt.
> Treten during der Umsetzung Entscheidungsprobleme auf, wird die sinnvollste Option
> gewählt und das Vorgehen in der Commit-/Zusammenfassung dokumentiert.

## Ziel

Die beiden Bearbeitungs-Flags `EditableWithDropdown` und `EditableWithTextInput` am
`ColumnItem` werden entfernt und durch ein einziges Property
`EditTypeTable EditStrategy` ersetzt. Die Strategy-Klassen beschreiben ihre
Fähigkeiten selbst (`SupportsSuggestions`, neu: `SupportsTextEdit`). Die bisherige
Heuristik `UserEditDialogTypeInTable` und die verteilten Fallback-If-Bäume entfallen.
Zusätzlich wird eine passende `None`-Strategy erzeugt, damit **jeder**
`EditTypeTable`-Wert eine Strategy hat — auch „nicht editierbar" ist nur eine Strategy.

## Konzept

1. **`ColumnItem.EditStrategy`** (Typ `EditTypeTable`, Default `Textfeld`) ist die
   direkte, serialisierte Strategy-Auswahl der Spalte.
2. **Fähigkeiten sitzen in den Strategien** (fester Wert pro Klasse, nicht konfigurierbar):
   - `SupportsSuggestions` — existiert bereits (`FlexiBase.cs`)
   - `SupportsTextEdit` — neu, virtuell an `FlexiBase`
3. **Der einzige Dropdown↔Text-Switch** passiert zentral in `TableView.BeginEdit`:
   Dropdown-Strategy ohne Items und ohne Text-Fähigkeit → Fallback auf `Textfeld`.
4. **Keine JSON-Migration:** Json-Formate sind nicht live — alte Keys
   (`editablewithdropdown`/`editablewithtextinput`) werden weder gelesen noch
   geschrieben. Die Migrationstabelle dient nur als Umrechnungs-Vorlage für die
   manuelle Code-Konvertierung (FormatHolder, Systemspalten).
5. **`UserEditDialogTypeInFormula`** prüft künftig Fähigkeiten der angefragten
   Strategy gegen die Fähigkeiten der Spalten-Strategy, statt der Bool-Flags.

## Migrationstabelle (alt → neu)

Spiegelt die alte `UserEditDialogTypeInTable`-Heuristik exakt wider — inklusive
`DropDownItems` und Autosort (`AfterEditQuickSortRemoveDouble`).
**Von oben nach unten abprüfen, die erste passende Zeile gewinnt.**

„Items vorhanden" = `DropDownItems.Count > 0` oder `ShowValuesOfOtherCellsInDropdown`.

| # | `EditableWithDropdown` | `EditableWithTextInput` | Zusatzbedingungen | `EditStrategy` |
|---|-----------------------|-------------------------|-------------------|----------------|
| 1 | false                 | false                   | —                 | `None` |
| 2 | false                 | true                    | Items vorhanden   | `Textfeld_mit_Vorschlägen` |
| 3 | false                 | true                    | keine Items       | `Textfeld` |
| 4 | true                  | false                   | —                 | `Dropdown_Single` |
| 5 | true                  | true                    | Items vorhanden **und** (`TextFormatingAllowed` oder (`MultiLine` **und nicht** Autosort)) | `Textfeld_mit_Vorschlägen` |
| 6 | true                  | true                    | `MultiLine`       | `Dropdown_Single` |
| 7 | true                  | true                    | sonst             | `Textfeld_mit_Auswahlknopf` |

## Schritte

### 1. `BlueTable/Enums/EditTypeTable.cs`
Komplett entfernen

### 2. Neue `None`-Strategy: `BlueControls/Controls/FlexiControlStrategies/FlexiNone.cs`
- `public class FlexiNone : FlexiBase`
- `Control => null` — erzeugt kein Control, `CreateControl()` bleibt leer.
- `SupportsTextEdit => false`, `SupportsSuggestions => false` (Base-Default).
- `SetValueToControlInternal` / `SubscribeEvents` / `UnsubscribeEvents` als No-Ops.
- Repräsentiert „Spalte nicht benutzer-editierbar". Damit entfällt das Sonderfälle-
  handling für `None` an den Aufrufern: `EditStrategy == None` ist schlicht die
  Strategy, die nichts kann.

### 3. `FlexiBase.cs` — Fähigkeiten
- Neu: `public virtual bool SupportsTextEdit => false;`
- `TextInputAllowed`-Property entfernen — Abfragende nutzen `SupportsTextEdit`.
- `UserEditDialogType`-Property in `EditStrategy` umbenennen (Konsistenz mit `ColumnItem`).

### 4. Strategie-Subklassen — Überschreibungen

| Klasse                  | `SupportsTextEdit` | `SupportsSuggestions` |
|-------------------------|--------------------|------------------------|
| `FlexiNone`             | —                  | —                      |
| `FlexiTextBox`          | `true`             | —                      |
| `FlexiTextBoxSuggestions` | `true`           | `true` (schon)         |
| `FlexiComboBox`         | `true`             | `true` (schon)         |
| `FlexiListBox` / `FlexiListBoxFramed` | —      | `true` (schon)         |

### 5. `IColumnInputFormat.cs` (BlueTable)
- `EditableWithDropdown` / `EditableWithTextInput` entfernen.
- `EditTypeTable EditStrategy { get; set; }` aufnehmen.
- `GetStyleFrom`-Extension kopiert künftig `EditStrategy` mit (löst das Problem
  „Werte werden nur einzeln aufgelöst": die Strategy wandert als eigenes Feld mit).

### 6. `ColumnItem.cs` (Kernstück)
- Feld `_editStrategy` + Property mit `ChangeData`-Notification; neuer
  `TableDataType.EditStrategy`-Wert.
- Serialisierung: `json.Set("editstrategy", (int)_editStrategy)`; Parse liest nur
  den neuen Key (Default `Textfeld`). **Keine Migration alter Json-Keys** —
  Json-Formate sind nicht live.
- **Löschen:** `UserEditDialogTypeInTable` (alle Overloads).
- `ErrorReason_Editing`: auf `EditStrategy == None` bzw. Strategies-Fähigkeiten
  umschreiben (u. a. `EditAllowedDespiteLock` ohne Methode → Fehler).
- `SymbolForReadableText`: Icon aus `EditStrategy` ableiten.
- `UserEditDialogTypeInFormula`: statt Bool-Flags → Fähigkeitsvergleich der
  Strategien (angefragte vs. Spalten-Strategy). Spezialfälle bleiben strukturell
  erhalten: `Textfeld` immer erlaubt, `RelationType.DropDownValues`,
  `CellValues`-Rekursion.
- Systemspalten-Setup (~12 Stellen): Flag-Zuweisungen → `_editStrategy = ...`
  (z. B. Creator: `Textfeld`, SysRowSortIndex: `DragDrop`, Changer: `None`).

### 7. `TableView.cs` — Fallback statt If-Baum
- `CreateEditStrategy`: `EditTypeTable.None => new FlexiNone()` ergänzen.
- `BeginEdit` vereinfachen:
  ```csharp
  if (strategy is FlexiNone) { NotEditableInfo(...); return; }
  if (strategy.SupportsSuggestions && items is not { Count: > 0 } && !strategy.SupportsTextEdit) {
      strategy = GetOrCreateEditStrategy(EditTypeTable.Textfeld);
  }
  ```
- `strategy.TextInputAllowed = ...` entfällt.
- „Erweiterte Eingabe"-Eintrag bei Dropdown-Strategies immer anbieten
  (Fluchtweg für Sonderwerte, bisher an `EditableWithTextInput` gekoppelt).

### 8. `RowBackground.cs`
- `dia = contentHolderCellColumn.EditStrategy;` — PowerEdit-Sonderfall-Heuristik entfällt.
- Items-Nachjustierung (0/>30 Items → Textfeld) entfällt — übernimmt Schritt 7.
- Switch-Anweisung um `CSV_Tabelle`-Case erweitern (später aktiviert).

### 9. Aufrufer umstellen
- `CellCollection.cs`: Editierbarkeit über `column.EditStrategy` bzw. Strategy-Fähigkeiten.
- `FlexiControlForCell.cs`: `f.EditStrategy = backcolumn.EditStrategy;` —
  `TextInputAllowed`-Zeile entfällt.
- `FlexiControl.cs`: `TextInputAllowed`-Weitergabe entfernen.

### 10. `ColumnEditor.cs` + `.Designer.cs`
- `btnEditableStandard` / `btnEditableDropdown` entfernen.
- Neue Combobox „Bearbeitungs-Methode" mit lesbaren Einträgen:
  *Keine Bearbeitung* (`None`), *Textfeld*, *Textfeld mit Vorschlägen*,
  *Textfeld mit Auswahlknopf*, *Dropdown*, *Dropdown mit Rahmen*.
- Validation-Solutions (ehem. `:690`/`:697`) auf die Combobox umleiten
  (z. B. „Bearbeitungsmethode wählen" statt „Dropdown aktivieren").

### 11. FormatHolder + Infrastruktur
- Alle `ColumnFormatHolder*.cs` gemäß Migrationstabelle umstellen — in Tabellen-
  reihenfolge abprüfen, inklusive `DropDownItems`/`ShowValuesOfOtherCellsInDropdown`
  und Autosort (`AfterEditQuickSortRemoveDouble`) des jeweiligen Halters
  (z. B. `Bit` → Regel 4 `Dropdown_Single`, `TextOneLine` → Regel 3 `Textfeld`,
  `TextMultiline` → Regel 3 `Textfeld`, `ItemsSelect` → Regel 5 oder 7 je nach
  `TextFormatingAllowed` des Basis-FormatHalters).
- `TableDataType.cs`: neuen Wert `EditStrategy` ergänzen; alte Werte 141/142 als
  veraltet behalten (Chunk-Kompatibilität) und beim Lesen gemäß Migrationstabelle
  migrieren.
- `Chunk.cs`: auf `TableDataType.EditStrategy` speichern.
- `GlobalMonitor.cs`: `EditStrategy = EditTypeTable.None`.
- `VariableEditor.cs`: `EditStrategy = EditTypeTable.Textfeld`.

### 12. Verifikation
- `dotnet build BeCreative.sln` — fehlerfrei.
- Manuelle Tests:
  - Doppelklick auf Zellen jedes Strategy-Typs.
  - `None`-Spalte: Doppelklick zeigt „nicht editierbar", kein Control.
  - Dropdown ohne Items → Fallback auf `Textfeld`.
  - ColumnEditor: Combobox auswählen, speichern, neu laden.
  - Spalten mit `DropDownItems`/Autosort-Kombinationen: erwartete Strategy gemäß
    Migrationstabelle prüfen (nur Code-Konvertierung, keine Json-Migration).

## Getroffene Detail-Entscheidungen

1. „Erweiterte Eingabe" bei Dropdowns immer anbieten (statt am gelöschten Flag).
2. `TextInputAllowed` komplett entfernen — auch Aufrufer in `FlexiControl` /
   `FlexiControlForCell`; `SupportsTextEdit` als fester Wert pro Strategy.
3. `None` wird eine echte Strategy (`FlexiNone`) statt eines Sonderfalls.
4. Serialisierung als `int` unter Key `"editstrategy"`; alte Json-Keys werden
   weder gelesen noch geschrieben — Json-Formate sind nicht live, keine Migration.
   Migration alt→neu nur als Code-Konvertierung (FormatHolder, Systemspalten)
   sowie lesend für alte `TableDataType`-Werte 141/142 (Chunk-Kompatibilität).

## Konventionen bei der Umsetzung

- Eine Datei pro Typ, UTF-8 mit BOM, `.editorconfig`-Stil (ägyptische Klammern,
  `var`, file-scoped Namespace, keine Lambdas für Event-Handler).
- Bei Form-/Control-Änderungen (ColumnEditor) die `.Designer.cs` mitpflegen.
- Betroffene Kommentare prägnant halten und auf neuen Stand bringen.
- die Stratgy wird im ColumnItem als Keyname gespeichert. Ein neuer Cache muss für Stratgies angelegt werden. (Für das Caching gibt es bereits klassen)