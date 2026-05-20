# Offene Aufgaben


## Aufgabe: QuickInfo auf disabled Controls anzeigen
`WM_NCHITTEST` wird auch an disabled Controls gesendet.
In `GenericControl.WndProc` abfangen, `ContainsMouse=true` + `DoQuickInfo()`.
`QuickInfo.Show()` bekommt Screen-Bounds des Source-Controls.
Der bestehende QuickInfo-Timer prüft per `Cursor.Position`, ob die Maus noch im Bereich ist → sonst Close.
Dateien: `GenericControl.cs`, `QuickInfo.cs`. In `OnEnabledChanged` `ContainsMouse=false` setzen.

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
Mache einen LinkedCell Editor und tu ihn Richtig in den HeadEditor einbetten

##Aufgabe:
Kann man  object? Object { get; set; } aus ScriptEditorGeneric entfernen und anders lösen?

##Aufgabe
Der Editor von QuickImage muss überarbeitet werden und von EditorEasy abgeleitet werden.


##Aufgabe
**InputItem-Konsistenz in IIsEditor-Implementierungen**
- Auf EditorEasy umstellen: ColumnEditor, TableScriptEditor, TableHeadEditor, ConnectedFormulaEditor, TableViewForm, ColumnArrangementPadEditor

##Aufgabe
ReadableListItem benötigt einen Dispose Pattern, dass ds Item freigegeben werden kann.
AbstractListItem mit Dispose ausstatten und und in allen Ableitungen richtig implementieren


##Aufgabe
UpdateList in TableScriptEditor nutzt ein zweistufiges System.
Mach ein neues Interface: ICategory
EventScript soll das implementiern und UpdateList in Listboxen das automatisch berücksichtigen
ColumnItem auch (eine Kombi aus Überschrift 1,2,3)
ContextmenuItemns auch

##Aufgabe
Spalten-Verlinkung muss ein EditorEasy werden.

##Aufgabe
Skript-Überprüfung muss auch alle Ifs und Verzweigungen prüfen.

##Aufgabe
Logik zur Thread-Synchronisierung (Invoke) und Logik zur Vererbung (virtual) gehören niemals in dieselbe Methode.

##Aufgabe
TableViewWithFilters ein Event feuern lassen, wo die auffangende Form Filter injizieren kann

##Aufgabe
TableViewWithFilters diese Events bubbeln lassen
                tbold.Loaded -= Tb_Loaded;
                tbold.InvalidateView -= Tb_InvalidateView;
			
##Aufgabe		
TableViewWithFilters	
protected Controls.TableViewWithFilters TableView;
private machen!

##Aufgabe
Variablen des TableHead werden nicht gespeichert

##Aufgabe
Nach längerer Wartezeit erscheint im Tableview Tabellen Inkonsitent.
Es wurden anscheinende neue Daten geladen. Im genannten Beispiel war es TableChunk.

##Aufgabe
Wird ein Formular mit einer Row befüllt, und das Formular ist nicht angezeigt worden, kommt bei Visible dann keine Anzeige

##Aufgabe
Es kommt beim Beenden die Meldung: xx Tabellen gespeichert, dann dauerts. irgendwas hängt.

##Aufgabe
Drücke im EventScriptEditor auf "-", wird die ganze Skript-Liste gelöscht.
Beim neuen Einstiegt ist anber nur das eine gelöscht

##Aufgabe
eine cbdb Datei wird in Dauerschleife gespeichert. Ich sehe immer die Tmp-Datei, dann veschwindet sie wieder und kommt gleich wieder.
Es ist keine blk Datei vorhanden.

##Aufgabe
Beim Beenden werden Blockfiles noch eingelesen?
public bool IsMyLock() 
macht EnsureLoaded();

##Aufgabe
public bool Parse(byte[] data, bool isMain, Reason reason, HashSet<string>? parsedRowKeys) {

wird aufgerufen trotz freeze?

##Aufgabe
BeSureToBeUpToDate(false) wird am Programmende aufgerufen? (TableUpdater)

##aufgabe
RevokeWriteAccessAll braucht beim Beenden unglaublich lange




## DAUERAUFGABEN ##

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



