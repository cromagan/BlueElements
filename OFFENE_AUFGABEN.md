# Offene Aufgaben

Alle Aufgaben sind so beschrieben, dass ein Agent sofort ohne Rückfragen loslegen kann.

---


---

## Aufgabe 2: FlexiControl — Enabled-Pattern bereinigen

**Priorität:** Mittel | **Risiko:** Niedrig | **Geschätzter Aufwand:** Mittel

### Was

`new bool Enabled` versteckt das Base-Property. Das soll aufhören.

### Dateien

- `BlueControls\Controls\FlexiControl.cs`
- Alle 58 Dateien, die `FlexiControl` referenzieren (Suche nach `.Enabled` im Kontext von FlexiControl)

### Schritte

1. **Analyse:** Zuerst mit `rg "\.Enabled"` in allen Dateien suchen, die FlexiControl oder FlexiControlForProperty nutzen. Prüfen, ob irgendwo jemand `flexiControl.Enabled = true/false` setzt oder `if (flexiControl.Enabled)` abfragt.

2. **Entscheidung:**
   - Falls `.Enabled` von außen **nicht** gesetzt wird: Property auf `private` oder `protected` ändern und `[Browsable(false)]` behalten.
   - Falls `.Enabled` von außen **doch** gesetzt wird: Property in `IsEditable` umbenennen und alle Aufrufer aktualisieren.

3. **Umsetzung:**
   - Das `new`-Keyword entfernen (nicht mehr verstecken)
   - Alternativ: `Enabled` als `private` deklarieren und Logik beibehalten
   - Sicherstellen, dass `UpdateControls()` und `DrawControl()` weiterhin korrekt mit dem Enabled/DisabledReason-Zustand arbeiten

### Prüfung

```bash
dotnet build BeCreative.sln
```

---

## Aufgabe 3: FlexiControl — Strategy-Pattern für EditType

**Priorität:** Hoch | **Risiko:** Mittel | **Geschätzter Aufwand:** Groß

### Was

Die gigantischen Switch-Kaskaden in `FlexiControl.cs` in einzelne Strategy-Klassen auslagern.
Das ist das Haupt-Refactoring und reduziert FlexiControl von ~1132 auf ~400 Zeilen.

### Neue Dateien erstellen

Alle unter `BlueControls\Controls\FlexiControl\Strategies\`:

| Datei | Klasse | Verantwortet |
|-------|--------|-------------|
| `IFlexiStrategy.cs` | `IFlexiStrategy` | Interface |
| `FlexiStrategyFactory.cs` | `FlexiStrategyFactory` | `EditTypeFormula` → `IFlexiStrategy` Zuordnung |
| `FlexiStrategy_TextBox.cs` | `FlexiStrategyTextBox` | TextBox erstellen, Wert setzen, Events |
| `FlexiStrategy_ComboBox.cs` | `FlexiStrategyComboBox` | ComboBox erstellen, Wert setzen, Events |
| `FlexiStrategy_ListBox.cs` | `FlexiStrategyListBox` | ListBox erstellen, Wert setzen, Events |
| `FlexiStrategy_SwapListBox.cs` | `FlexiStrategySwapListBox` | SwapListBox erstellen, Wert setzen, Events |
| `FlexiStrategy_ButtonYesNo.cs` | `FlexiStrategyButtonYesNo` | Ja/Nein-Button erstellen, Wert setzen, Events |
| `FlexiStrategy_ButtonCommand.cs` | `FlexiStrategyButtonCommand` | Kommando-Button erstellen, Events |
| `FlexiStrategy_ButtonColor.cs` | `FlexiStrategyButtonColor` | Farb-Button erstellen, Wert setzen |
| `FlexiStrategy_Line.cs` | `FlexiStrategyLine` | Trennlinie erstellen |
| `FlexiStrategy_GroupBox.cs` | `FlexiStrategyGroupBox` | Gruppen-Überschrift erstellen |
| `FlexiStrategy_Caption.cs` | `FlexiStrategyCaption` | Nur-Text-Anzeige (Caption) erstellen |

### Interface

```csharp
namespace BlueControls.Controls.FlexiControl.Strategies;

public interface IFlexiStrategy {
    Control? CreateControl(FlexiControl owner);
    void SetValue(Control control, string value);
    void SubscribeEvents(Control control, FlexiControl owner);
    void UnsubscribeEvents(Control control);
}
```

### Umsetzungsschritte

1. **Interface erstellen** — `IFlexiStrategy.cs`
2. **Factory erstellen** — `FlexiStrategyFactory.cs` mit statischer Methode `IFlexiStrategy? GetStrategy(EditTypeFormula editType)`
3. **Strategies implementieren** — Jeweils eine Klasse, die die Logik aus den aktuellen `Control_Create_*` und `UpdateValueTo_*` Methoden übernimmt
4. **FlexiControl umschreiben:**
   - Neues Feld: `private IFlexiStrategy? _strategy;`
   - In `CreateSubControls()`: `_strategy = FlexiStrategyFactory.GetStrategy(_editType);` dann `c = _strategy?.CreateControl(this);`
   - In `UpdateValueToControl()`: `_strategy?.SetValue(control, Value);` statt Switch
   - In `UnsubscribeEvents()`: `_strategy?.UnsubscribeEvents(control);` statt Switch
   - Event-Handler wie `ValueChanged_ComboBox`, `ValueChanged_TextBox`, etc. in die jeweiligen Strategy-Klassen verschieben. Die Strategy braucht Zugriff auf `FlexiControl.ValueSet()` — entweder über Interface-Übergabe oder indem die Events in der Strategy behandelt werden und `owner.ValueSet()` aufrufen.
5. **Alte Methoden in FlexiControl.cs löschen:** Alle `Control_Create_*`, `UpdateValueTo_*`, `ValueChanged_*`, `YesNoButton_CheckedChanged`, `ListBox_ItemCheckedChanged`, `SwapListBox_ItemCheckedChanged`, `CommandButton_Click`
6. **`StandardBehandlung` bleibt in FlexiControl** — das ist Layout-Logik, keine Strategy

### Wichtige Details für die Strategies

- `FlexiStrategyTextBox`: Braucht `StyleTextBox()` — die Methode bleibt in FlexiControl, wird von der Strategy aufgerufen
- `FlexiStrategyComboBox`: Braucht `StyleComboBox()` — gleiches Prinzip
- `FlexiStrategyListBox` und `FlexiStrategySwapListBox`: Brauchen `StyleListBox()` / `StyleSwapListBox()` — bleiben in FlexiControl
- `FlexiStrategyCaption`: Sonderfall — erstellt kein Control sondern ruft `Control_Create_Caption()` auf. Nach Refactoring wird daraus ein Aufruf in `StandardBehandlung`
- Die statischen Methoden `StyleComboBox`, `StyleTextBox`, `StyleListBox`, `StyleSwapListBox` **bleiben in FlexiControl** — sie werden von außen aufgerufen (FlexiControlForCell, FlexiControlForProperty)

### Prüfung

```bash
dotnet build BeCreative.sln
```

---

## Aufgabe 4: FlexiControlForProperty — Timer-Polling entfernen

**Priorität:** Mittel | **Risiko:** Mittel-Hoch | **Geschätzter Aufwand:** Mittel

**Abhängigkeit:** Aufgabe 3 sollte vorher erledigt sein.

### Was

Den 1-Sekunden-Timer in `FlexiControlForProperty<T>` durch ein sauberes Refresh-System ersetzen.

### Dateien

- `BlueControls\Controls\FlexiControlForProperty.cs`
- `BlueBasics\Classes\Accessor.cs` (falls Option A gewählt)

### Option A — INotifyPropertyChanged (sauber, mehr Aufwand)

1. `Accessor<T>` um Event erweitern:
   ```csharp
   public event Action? ValueChanged;
   ```
2. `FlexiControlForProperty<T>` subscribt sich:
   ```csharp
   _accessor.ValueChanged += () => BeginInvoke(new Action(SetValueFromProperty));
   ```
3. Problem: Wer feuert das Event? Alle Setter der Properties müssen manuell feuern — das erfordert Änderungen an vielen Klassen. Daher eher unrealistisch für dieses Projekt.

### Option B — Explizites Refresh (pragmatisch, empfohlen)

1. Timer und `Checker_Tick()` entfernen
2. Neue public Methode:
   ```csharp
   public void RefreshFromProperty() {
       if (!Allinitialized || IsDisposed) { return; }
       SetValueFromProperty();
   }
   ```
3. In `OnHandleDestroyed`: `FillPropertyNow()` beibalten (bereits vorhanden)
4. Alle Aufrufer prüfen: Wenn sich ein Property-Wert ändert und ein FlexiControlForProperty davon betroffen sein könnte, muss der Aufrufer `RefreshFromProperty()` aufrufen. In der Praxis passiert dies meist durch Form-Refresh, der die Controls ohnehin neu befüllt.

### Prüfung

```bash
dotnet build BeCreative.sln
```

---

## Aufgabe 5: FlexiControlForProperty — Typsicheres Value

**Priorität:** Niedrig | **Risiko:** Hoch | **Geschätzter Aufwand:** Groß

**Abhängigkeit:** Aufgabe 3 und 4 sollten vorher erledigt sein.

### Was

Die Switch-Kaskaden in `SetValueFromProperty()` und `FillPropertyNow()` durch generische Konvertierung ersetzen.

### Dateien

- `BlueControls\Controls\FlexiControlForProperty.cs`
- `BlueBasics\ClassesStatic\Converter.cs` (evtl. Erweiterung)

### Ansatz

1. Neues Interface/abstrakte Klasse für Typ-Konvertierung:
   ```csharp
   // In FlexiControlForProperty<T>:
   private string ConvertToDisplayString(T? value) => value switch {
       null => string.Empty,
       bool b => b.ToPlusMinus(),
       int i => i.ToString1(),
       double d => d.ToString1_2(),
       float f => f.ToString1_2(),
       Color c => c.ToHtmlCode(),
       Enum e => ((int)e).ToString1(),
       string s => s,
       List<string> l => string.Join('\r', l),
       ReadOnlyCollection<string> l => string.Join('\r', l),
       Table t => t.KeyName,
       IEditable => string.Empty,
       _ => Develop.DebugError(value?.GetType().Name + " unbekannt"), string.Empty
   };
   ```

2. Entsprechend `ConvertFromDisplayString(string value)` implementieren.

3. `SetValueFromProperty()` und `FillPropertyNow()` vereinfachen auf jeweils ~5 Zeilen statt je ~50 Zeilen Switch.

### Prüfung

```bash
dotnet build BeCreative.sln
```

---

## Aufgabe 6: FlexiControl — InvokeRequired-Boilerplate reduzieren

**Priorität:** Niedrig | **Risiko:** Niedrig | **Geschätzter Aufwand:** Klein

### Was

Jeder Property-Setter hat das gleiche Muster:
```csharp
set {
    if (field == value) { return; }
    if (InvokeRequired) {
        Invoke(new Action(() => { field = value; UpdateControls(); }));
        return;
    }
    field = value;
    UpdateControls();
}
```

Das ist ~10 Mal dupliziert. Ein Hilfsmethoden-Set reduziert das.

### Dateien

- `BlueControls\Controls\FlexiControl.cs`

### Schritte

1. Hilfsmethode erstellen:
   ```csharp
   private void SetField<T>(ref T field, T value, Action? onChange = null) {
       if (EqualityComparer<T>.Default.Equals(field, value)) { return; }
       if (InvokeRequired) {
           Invoke(new Action(() => SetField(ref field, value, onChange)));
           return;
       }
       field = value;
       onChange?.Invoke();
   }
   ```

2. Alle Setter umschreiben, z.B.:
   ```csharp
   public bool MultiLine { get; private set; }
   // wird zu:
   private bool _multiLine;
   public bool MultiLine {
       get => _multiLine;
       set => SetField(ref _multiLine, value, UpdateControls);
   }
   ```

3. **Achtung:** Einige Setter machen `RemoveAll()` statt `UpdateControls()` — diese müssen separat behandelt werden (z.B. zweiter Parameter `Action changeAction`).

### Prüfung

```bash
dotnet build BlueControls\BlueControls.csproj
```
