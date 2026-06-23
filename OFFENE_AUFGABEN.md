# Offene Aufgaben

## Aufgabe: FlexiControl — Enabled-Pattern bereinigen
`new bool Enabled` versteckt das Base-Property. Das soll aufhören.

## Aufgabe: FlexiControlForProperty — Typsicheres Value
Die Switch-Kaskaden in `SetValueFromProperty()` und `FillPropertyNow()` durch generische Konvertierung ersetzen.

## Aufgabe:
Drawing-Helpers nicht als Enum sondern als eigene Klassen.

## Aufgabe
In allen Klassen, die von ParseableItem erben (sowie in NoteEntry), müssen die Setter der Properties, die über FlexiControlForProperty gebunden werden, OnPropertyChanged(nameof(PropertyName)) aufrufen, damit der automatische Refresh über INotifyPropertyChanged funktioniert.
		
##Aufgabe
- **`FromNonCritical` / `ToNonCritical` / `TagGet` entfernen** — Ziel: alles über JSON. Wenn bei einer Änderung eine einfache Gelegenheit besteht, diese Formate abzuschaffen, mit umsetzen. Danach den Nutzer fragen: *„Mit Rückwärtskompatibilität (alter Code funktioniert noch)"* oder *„Ohne — alter Code bricht"*.

##Aufgabe
Wird ein button mit OptionText Design angeklickt, wird er kurz deaktivuert und wieder aktiviert. Der Text-Aufbau dauert sehr lange.

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
ReadableListItem benötigt einen Dispose Pattern, dass das Item freigegeben werden kann.
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
Nach längerer Wartezeit erscheint im Tableview Tabellen Inkonsitent.
Es wurden anscheinende neue Daten geladen. Im genannten Beispiel war es TableChunk.

##Aufgabe
Wird ein Formular mit einer Row befüllt, und das Formular ist nicht angezeigt worden, kommt bei Visible dann keine Anzeige

##Aufgabe
Pens und Brushes etc. von Lock umstellen auf
private static readonly ThreadLocal<Dictionary<string, Pen>> _threadPens = 
    new(() => new Dictionary<string, Pen>());

##Aufgabe
Parellelle ForEach optimieren:
Anstelle von Locks das Ergebnis merken und außerhalb zusammengehören

##Aufgabe
Lade Chunks von .... in den Hintergrund verlagern

##Aufgabe
Ist in TableViewForm ZWEIMAL der Tab mit gleichen Namen, geht das umschalten nicht.

##Aufgabe
Trotz Freeze müssen Scripte, die nix ändern, ausgeführt werden.

##Aufgabe
Befehle wie Import Linked liefern Feedback in form einer Fehlermeldung.
Ergänze, dass Klickbare Felder zurückgegeben werden. Im Falle von ImportLinked, soll eine TableView mit der Filterung aufgehen.
Orientiere dich an CellLink in ExtText



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

##Aufgabe
Finde stellen, wo  die Dreierabfrage sinn mach
if (Generic.Ending || IsDisposed || Disposing) { return; }
und ergänze diese

#Aufgabe
CollectionRenderingData in eine eigen Datei verschieben.
Füge Summary hinzu, was die Aufgabe der klasse ist.

#Aufgabe
Erstelle Virtuelle Spalten für eine ANSICHT. Die Virtuellen Spalten speichern den Wert in der Tabelle. Nun soll es anders sein: Die Werte sollen in Table gespeichert werden. Und so auch die RowChecked Werte (Zeilenfarben etc.)

##Aufgabe
Externe Änderung an 'xxx' erkannt, lokale ungespeicherte Änderungen werden verworfen.

##Aufgabe
    Freeze($"Hauptchunk fehlt auf der Festplatte und kein gültiges Backup vorhanden");
Die Meldung kommt, obwohl es vorhanden ist

##Aufgabe
Deadlock in WaitDiskOperationFinished bei   _loadSemaphore.Wait();

##Aufgabe
CurrentArrangement mit dessen Logik ist alt und nicht effizienzt. Auch der Demstsprechende Editor

##Aufgabe
Logdata ist oft als Logdata?. Kann es so refactorisiert werden, dass Logdata immer gefordert ist?

##Aufgabe
Ansicht bearbeiten abspecken. So, dass nur die Kopfdaten bearbeitet werden können. In den Kopfdaten muss der Ansichtname bearbeitbar sein

##Aufgabe
Den Auswahldialog der Ansichten so  erweitert, dass das hinzufügen und löschen möglich ist. Die Listbox (Nicht ListboxCore) hat bereits entsprechende Funktionen

##Aufgabe
CanDoFeedback, DoItFeedback, DoItWithEndedPosFeedback, ScriptEndedFeedback, SplittedAttributesFeedback kann man das SINNVOLL zusammenlegen?

##Aufgabe
Alle ListItemAbstract Ableitungen komplett entfernen. Nur noch ein List-Item. Das aussehen wird durch die (vorhandenen) Renderer gemacht. Evtl. die Renderer erweitern. Evtl. neue Renderer einrichten.

##Aufgabe:
Wenn SYS_ROWSORTINDEX vorhanden ist, muss das Kapitel-Bearbeiten anderne grenzen haben, und zwar nicht ALLE ändern, sondern nur den Block

##Aufgabe 
Kontextmenu: Kapitel ändern

#Aufgabe
Chunk und ConnectedFormula das get vereinheitlichen

##Aufgabe
CachedFileSystem möchte ich komplett entfernen. Welcher kleiner schritt wäre möglich, um meinem Ziel ein Stück näher zu kommen?

##Aufgabe
Varibalen werden oft so ~variable~ angegeben. Mach einen neuen ExtTextBausetein für Spalten.