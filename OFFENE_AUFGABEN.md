# Offene Aufgaben

Alle Aufgaben sind so beschrieben, dass ein Agent sofort ohne Rückfragen loslegen kann.

---


---

## Aufgabe: FlexiControl — Enabled-Pattern bereinigen

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



## Aufgabe: FlexiControlForProperty — Typsicheres Value

**Priorität:** Niedrig | **Risiko:** Hoch | **Geschätzter Aufwand:** Groß


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


## Aufgabe:
Muss noch analysiert werden:
   private void DrawMarkingZone(Graphics gr, float zoom, MarkState state, int markStart, int markEnd, int offsetX, int offsetY)
   
   in ExtText. Den Switch Case durch eine "Render-Klasse" ersetzen.
   
## Aufgabe:
Vorher muss Aufgabe vorher erledigt werden.
public enum MarkState entfernen und die Klassen Zeichnungs-Collection auslagern
   

## Aufgabe:
Muss noch analysiert werden:
Einen Renderer (abgeleitet von Renderer_Abstract) für CellNote erstellen 

##Dauer-Aufgabe:
Daueraufgabe, wird mehrfach ausgeführt.
Suche nach der nächsten Interfaces-Extenssiond und baue diese direkt in das Interface ein. Der Code stammt aus alter Frameworks Zeit uns ist veraltet.

##Dauer-Aufgabe:
Daueraufgabe, wird mehrfach ausgeführt.
Suche in der nächsten Datei nach Propertys, bei denen der Setter durch init ersetzt werden kann.
Ergänze die bereits geprüfte Datei hier und ignoriere diese:

	
##Aufgabe:
Muss noch analysiert werden:
Entferne     internal FlexiStrategyBase? Strategy => _strategy;
aus
FlexiControl
Es soll direkt das FlexControl angesprochen werden

##Aufgabe:
Muss noch analysiert werden:
Diese Properties fehlen bei FlexiControl 
 CheckBehavior checkBehavior, AddType addallowed, bool autoSort
 siehe     public FlexiControlForProperty(Expression<Func<T>>? expr, string captionText, int rowCount, List<AbstractListItem>? allPossibleItems, CheckBehavior checkBehavior, AddType addallowed, bool autoSort) : base() {
 Zudem fehlt in der Routine BeginnEdit und EndEdit
 

##Aufgabe:
Es wird ein neuees Element benötigt, ähnlich dem ScriptButton oder Timer button.
Einfach ein Script, das ausgeführt wird, wenn sich die eingehenden Zeilen ändern.
Dazu braucht es eine Variable InputRowCount


##Aufgabe:
Muss noch analysiert werden:
Diese Properties fehlen bei FlexiControl 
 CheckBehavior checkBehavior, AddType addallowed, bool autoSort
 siehe     public FlexiControlForProperty(Expression<Func<T>>? expr, string captionText, int rowCount, List<AbstractListItem>? allPossibleItems, CheckBehavior checkBehavior, AddType addallowed, bool autoSort) : base() {
 Zudem fehlt in der Routine BeginnEdit und EndEdit
 
##Aufgabe:
Muss noch analysiert werden:
Screenshot: Es stimmt die Anzeige des Roten Kästchens nicht. Die MouseDown koordinate ist anscheinend Fehlerhaft 



##Aufgabe:
Überprüfe die Meldung.
Totes Fenster

##Aufgabe:
Drawing-Helpers nicht als Enum sondern als eigene Klassen.

##Aufgabe
In allen Klassen, die von ParseableItem erben (sowie in NoteEntry), müssen die Setter der Properties, die über FlexiControlForProperty gebunden werden, OnPropertyChanged(nameof(PropertyName)) aufrufen, damit der automatische Refresh über INotifyPropertyChanged funktioniert.

##Daueraufgabe
DisposesExtended umschreiben auf Atomic Lock:
        if (Interlocked.CompareExchange(ref _isDisposedFlag, 1, 0) == 1) { return; }
