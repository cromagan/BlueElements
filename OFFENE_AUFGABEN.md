# Offene Aufgaben

## Aufgabe: FlexiControl — Enabled-Pattern bereinigen
`new bool Enabled` versteckt das Base-Property. Das soll aufhören.

## Aufgabe: FlexiControlForProperty — Typsicheres Value
Die Switch-Kaskaden in `SetValueFromProperty()` und `FillPropertyNow()` durch generische Konvertierung ersetzen.


## Aufgabe
In allen Klassen, die von ParseableItem erben (sowie in NoteEntry), müssen die Setter der Properties, die über FlexiControlForProperty gebunden werden, OnPropertyChanged(nameof(PropertyName)) aufrufen, damit der automatische Refresh über INotifyPropertyChanged funktioniert.
		
##Aufgabe
Wird ein button mit OptionText Design angeklickt, wird er kurz deaktivuert und wieder aktiviert. Der Text-Aufbau dauert sehr lange.

##Aufgabe
 _parent?.StyleChanged -= _parent_StyleChanged
 entfernen. Das Parent sollte das Child pushen
 
 ##Aufgabe
 Entferne DisposingEvent und ersetze IDisposableExtendedWithEvent dich IDisposableExtended
 Entferne OnColumnDisposed
 Der Code muss trotzdem npch funktionieren
 
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
Alle Filter aus nur enablend wenn es sich rentiert

##Aufgabe
Mache einen LinkedCell Editor und tu ihn Richtig in den HeadEditor einbetten

##Aufgabe:
Kann man  object? Object { get; set; } aus ScriptEditorGeneric entfernen und anders lösen?

##Aufgabe
Der Editor von QuickImage muss überarbeitet werden und von EditorEasy abgeleitet werden.

##Aufgabe
**InputItem-Konsistenz in IIsEditor-Implementierungen**
- Auf EditorEasy umstellen: ColumnEditor, TableScriptEditor, TableHeadEditor, ConnectedFormulaEditor, TableViewForm

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
Mach eine komplett eigene Routine, die vor dem Skript-Test-Ausführen folgenes mach:
- Alle var Befehel checken und die Variablelnamen ermitteln.
- Ein einfacher Syntax-Check, welche Befehle/Variablen nicht geparsed werden können.

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
Wird ein Formular mit einer Row befüllt, und das Formular ist nicht angezeigt worden, kommt bei Visible dann keine Anzeige

##Aufgabe
Pens und Brushes etc. von Lock umstellen auf
private static readonly ThreadLocal<Dictionary<string, Pen>> _threadPens = 
    new(() => new Dictionary<string, Pen>());

##Aufgabe
Parellelle ForEach optimieren:
Anstelle von Locks das Ergebnis merken und außerhalb zusammengehören

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
Erstelle Virtuelle Spalten für eine ANSICHT. Die Virtuellen Spalten speichern den Wert in der Tabelle. Nun soll es anders sein: Die Werte sollen in Table gespeichert werden. Und so auch die RowChecked Werte (Zeilenfarben etc.)

##Aufgabe
CanDoFeedback, DoItFeedback, DoItWithEndedPosFeedback, ScriptEndedFeedback, SplittedAttributesFeedback kann man das SINNVOLL zusammenlegen?

##Aufgabe
Alle ListItemAbstract Ableitungen komplett entfernen. Nur noch ein List-Item. Das aussehen wird durch die (vorhandenen) Renderer gemacht. Evtl. die Renderer erweitern. Evtl. neue Renderer einrichten.

##Aufgabe 
Kontextmenu: Kapitel ändern

##Aufgabe
Varibalen werden oft so ~variable~ angegeben. Mach einen neuen ExtTextBausetein für Spalten.

## Aufgabe
Mach ein neues Kontrol. Eine Mischung aus TabControl und Listbox.
Das Control versteckt mit einer Regsiterkarte aussieht und sich Links Rechts oben oder unten "versteckt".
Fährt man mit der Maus darüber fährt es raus.

## Aufgabe
Alle Json wie diese json["type"] = MyClassId; zu json.Set (Hilfemethode) ändern.

Alle  public void ParseJson(JsonObject json) 
Alle Aufrufe so apassen, das sie ohne If auskommen uns sinngemäß so aufgerufen werden. Also mit Default Wert
KeyName = json.GetString("key", KeyName);

Alle json Keys in kleinschreibung.


## Aufgabe
Ich denke, LogData ist nicht mehr nötig und einfach zu ersetzen.
Was meinst du?

## Aufgabe
Ist in TableViewForm ZWEIMAL der Tab mit gleichen Namen, geht das umschalten nicht.

## Aufgabe
Wird die Listbox gezoomt. verändern sich intern die Items nicht, sie werden immer mit Scale 1 gezeichnet

