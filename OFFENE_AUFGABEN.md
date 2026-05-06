# Offene Aufgaben


## Aufgabe: FlexiControl — Enabled-Pattern bereinigen
`new bool Enabled` versteckt das Base-Property. Das soll aufhören.

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
 siehe     public FlexiControlForProperty(Expression<Func<T>>? expr, string captionText, int rowCount, List<AbstractListItem>? allPossibleItems, CheckBehavior checkBehavior, AddType addallowed, bool autoSort) : base() {
 Es fehlt in der Routine BeginnEdit und EndEdit
 

##Aufgabe:
Das Timer-Element für Connected Formula braucht ein neues Attribut:
User-Idle (oder ien besserer Name)
DAs Script wird nicht ausgführt, wenn der Benutzer dies Zeitspanne nicht "abgewartet" hat.

##Aufgabe:
Drawing-Helpers nicht als Enum sondern als eigene Klassen.

##Aufgabe
In allen Klassen, die von ParseableItem erben (sowie in NoteEntry), müssen die Setter der Properties, die über FlexiControlForProperty gebunden werden, OnPropertyChanged(nameof(PropertyName)) aufrufen, damit der automatische Refresh über INotifyPropertyChanged funktioniert.

##Daueraufgabe
1) Alle normalen Disposes auf DisposesExtended umschreiben
2) DisposesExtended umschreiben auf Atomic Lock:
        if (Interlocked.CompareExchange(ref _isDisposedFlag, 1, 0) == 1) { return; }
3) Alle Events auf null setzen im Dispose Pattern
4) Routinen, die manuell die Events nach einem Dispose deabboniert, diese deabbonements entfernen.
		
##Aufgabe
ConnectedFormula Editor weg von Tabs, sondern die Pages links als Vorschau anzeigen, ähnlich Powerpoint


##Aufgabe
call in Tabellen Scripten geben zwar Extended richtig weiter, aber dann die passen die erlaubten Methoden nicht.


##Aufgabe
RowColors werden entweder nicht richtig gespeichert oder nicht immer angezeigt.

##Aufgabe  
sollte der Auskommentierte Bereich repariert (beschleunigt) und wieder scharf geschalten werden?
  public string ErrorReason() {
        foreach (var thisf in this) {
            if (thisf.ErrorReason() is { Length: > 0 } f) { return f; }

            if (_table != thisf.Table && thisf.FilterType != FilterType.AlwaysFalse) {
                return "Filter haben unterschiedliche Tabellen";
            }
        }

        //if (_table?.Column.ChunkValueColumn is { } cvc && this.Count > 0) {
        //    if (string.IsNullOrEmpty(InitValue(cvc, true, this.ToArray()))) { return "Chunk-Wert Filter fehlt."; }
        //}
        return string.Empty;
    }
	
##Aufgabe
Erstell eine Konzept für FilterFix, FilterCombined, FilterInput
in TableView und TableView with Filters.
Überarebeite, repariere und kommentiere.
        