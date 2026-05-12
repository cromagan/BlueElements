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
private void DrawMarkingZone(Graphics gr, float zoom, MarkState state, int markStart, int markEnd, int offsetX, int offsetY)
in ExtText. Den Switch Case durch eine "Render-Klasse" ersetzen.
   
## Aufgabe:
Vorher muss Aufgabe vorher erledigt werden.
public enum MarkState entfernen und die Klassen Zeichnungs-Collection auslagern
   
## Aufgabe:
Muss noch analysiert werden:
Einen Renderer (abgeleitet von Renderer_Abstract) für CellNote erstellen 



##Aufgabe:
 siehe     public FlexiControlForProperty(Expression<Func<T>>? expr, string captionText, int rowCount, List<AbstractListItem>? allPossibleItems, CheckBehavior checkBehavior, AddType addallowed, bool autoSort) : base() {
 Es fehlt in der Routine BeginnEdit und EndEdit
 

##Aufgabe:
Drawing-Helpers nicht als Enum sondern als eigene Klassen.

##Aufgabe
In allen Klassen, die von ParseableItem erben (sowie in NoteEntry), müssen die Setter der Properties, die über FlexiControlForProperty gebunden werden, OnPropertyChanged(nameof(PropertyName)) aufrufen, damit der automatische Refresh über INotifyPropertyChanged funktioniert.


		
##Aufgabe
ConnectedFormula Editor weg von Tabs, sondern die Pages links als Vorschau anzeigen, ähnlich Powerpoint

##Aufgabe
- **`FromNonCritical` / `ToNonCritical` / `TagGet` entfernen** — Ziel: alles über JSON. Wenn bei einer Änderung eine einfache Gelegenheit besteht, diese Formate abzuschaffen, mit umsetzen. Danach den Nutzer fragen: *„Mit Rückwärtskompatibilität (alter Code funktioniert noch)"* oder *„Ohne — alter Code bricht"*.

##Aufgabe
ColumnArrangement Editor complett überarbeiten


##Aufgabe
Wird ein button mit OptionText Design angeklickt, wird er kurz deaktivuert und wieder aktiviert. Der Text-Aufbau dauert sehr lange.

##Aufgabe
LastArgMinCount durch ein Enum ersetzen

##Aufgabe
QuickInfo hat HTML-Tags ansteller der Zeichen (z.B. ö)

##Aufgabe
Wozu wird "DropMessage" noch benötigt? Optimiern!

##Aufgabe 
 _parent?.StyleChanged -= _parent_StyleChanged
 entfernen. Das Parent sollte das Child pushen
 
 ##Aufgabe
 Entferne DisposingEvent und ersetze IDisposableExtendedWithEvent dich IDisposableExtended
 Entferne OnColumnDisposed
 
 ##Aufgabe
 TextBoxSuggestions
 löst LostFocus nicht richtig aus
 Die Chips zeigen keinen Text analysiert
 De chipfläche muss ein klares ende (Umrdanung) haben. Nutze dazu das gleiche Zeichnen der Groupbox im Minimal-Stil
 Die Chipgläche ist zu groß (unten / oben) - TableView soll eine Schätzing und grobe berechung der Große vorab mache
 Die Textfläche wird bei Bottom nicht richig angezeigt
 
##Aufgabe
ListItem, PadItems, Renderer, Cells, ExtChar kommt mir seltsam vor. Als ob die zusammengehören würden. Also ob man sie zusammenlegen könnte.
Besonders, weil fast jedes Control ein Listitem oder anders rum ein CellItem anzeigen kann.
Analysiere:
 - Kann man eins oder mehr zusammenlegen?
 - Kann man gemeinsame Interfaces definieren?
 
##Aufgabe
in den Listbox wird der MousOver Effekt komisch hell angezeigt und nicht Windows dunkelblau.



##Aufgabe
Schau dir mal das an, das ist alt und unnötig, oder?
    [DefaultValue(0)]
    public new int TabIndex {
        get => 0;
        set => base.TabIndex = 0;
    }

    /// <summary>
    /// Benötigt, dass der Designer das nicht erstellt
    /// </summary>
    [DefaultValue(false)]
    public new bool TabStop {
        get => false;
        set => base.TabStop = false;
    }


##Aufgabe
Leite Textbox von Zoompad ab



##Aufgabe
Alle Filter aus nur enablend wenn es sich rentiert

##Aufgabe
Der Aufbau von TableVuew ist sehr langsam

##Aufgabe
IEditable überspielen die Werte nicht richtig

##Aufgabe
TableViewForm zeigt die anzahl der Zeilen nicht richtig an


## DAUERAUFGABEN ##


##Aufgabe
DrawImageUnscaled anstelle DrawImage benutzen, wo möglich

##Aufgabe
Alle IEditable müssen PropertyChanged unterstützen, so dass der Editor sofort den OK-Button bei Fehlern deaktiviern kann.


##Aufgabe
Suche nach der nächsten Interfaces-Extenssiond und baue diese direkt in das Interface ein. Der Code stammt aus alter Frameworks Zeit uns ist veraltet.

##Aufgabe
Suche in der nächsten Datei nach Propertys, bei denen der Setter durch init ersetzt werden kann.
Ergänze die bereits geprüfte Datei hier und ignoriere diese:

##Aufgabe
Setze Pattern-Matching ein, wo möglich

##Aufgabe
Überprüfe Tulples, String und Bool Rückgaben, ob diese Durch OperationResult verbessert werden können.


##Aufgabe
Mach eine Datei mit MagicStrings und mache alle Rückgaben mit den neuen Magic-Strings.
Beachte, dass Mehrsprachigkeit gewahrt sein muss und benutze bei bedarf {0} {1}

##Aufgabe
Führe die Aufgaben nacheinander durch:
1) Alle normalen Disposes auf IDisposedExtendeded umschreiben
2) IDisposedExtendeded umschreiben auf Atomic Lock:
       if (Interlocked.CompareExchange(ref _isDisposedFlag, 1, 0) != 0) { return; }
3) Alle Events auf null setzen im Dispose Pattern
4) Elemente, die ebenfalls Disposed werden können, disposen (Beispiel: TextboxWithSuggestions disposed die innere Textbox)
5) obsolete Deabbonements entfernen (Wenn der innere Dispose das erledigt)

##Aufgabe
Entferne DisposingEvent und ersetze IDisposableExtendedWithEvent dich IDisposableExtended
 
##Aufgabe
Entferne alle Using Zuweisungen wie: using AsciiKey = BlueControls.Enums.AsciiKey; und qualifiziere sie voll
Entferne alle Usings: System.Windows.Forms;  und qualifiziere sie voll