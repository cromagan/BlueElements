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
Setze Inline Deklartionen ein, wo geht.  Ich meine so: ... is {} x. Oder auch mit den Typen

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

##Aufgabe
CanDoFeedback, DoItFeedback, DoItWithEndedPosFeedback, ScriptEndedFeedback, SplittedAttributesFeedback kann man das SINNVOLL zusammenlegen?

##Aufgabe
Alle ListItemAbstract Ableitungen komplett entfernen. Nur noch ein List-Item. Das aussehen wird durch die (vorhandenen) Renderer gemacht. Evtl. die Renderer erweitern. Evtl. neue Renderer einrichten.

##Aufgabe 
MiniToolbar: Kapitel ändern

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
In Table ist das Json laden nicht nicht implementiert.
Und TableFragments muss auf das neue Json Format geändert werden.


## Aufgabe
Ich denke, LogData ist nicht mehr nötig und einfach zu ersetzen.
Was meinst du?

## Aufgabe
Ist in TableViewForm ZWEIMAL der Tab mit gleichen Namen, geht das umschalten nicht.

## Aufgabe
Wird die Listbox gezoomt. verändern sich intern die Items nicht, sie werden immer mit Scale 1 gezeichnet

## Aufgabe
Der neue Editor muss anders gestaltet werden:
Leite von einer Listbox ab. Und diese Listbox soll mittels Property einen Editor erhalten. die Listbox soll den Editor steuern und alles selbst übernehmen.

## Aufgabe
Umbennennen in Explorer Soll eine Textbox IM angezeigten Explorer Fenster gehen.
Zudem fehlt der Befehl Neuer Ordner
Und F2, F5 und Entfernen muss funktionieren

## Aufgabe
public static ColumnViewItem Create(Table? table, string toParse) 
dauert ewig lange. Ich denke, das liegt an NewByTypeName.
Kann man das irgendwie cachen? Also NewByTypeName global mit AssemblyAwareCache?
Ich hab schon ein bisschen optimiert.

## Aufgabe
Prüfe die OnLoaded Logik von Tabellen.
TableFragments wirft ein "First=false" (BeSureUptoDate) vor dem ersten echten Laden.
Brauchen wir OnLoaded wirklich oder kann man das sogar wegkürzen?

## Aufgabe
Ändere ich ein Skript, weil es fehlerhaft war, wird in TableViewForm die Aufgabenbox nicht aktualisiert

## Aufgabe
Entferne ich den Fehler im EventScriptEditor, wird das ListItem nicht aktualisiert

## Aufgabe
IsFormat soll anstelle von bool einen STRING zurückgeben, WAS der GRund ist.
UserEdited in Tablview soll diese Info anzeigen.
IInputFormat muss erweitert werden mit MinTextLength und die überladungen mit von IsFormat mit valueRequired entfernt werden.
ColumnItem muss ebenfalls _valueRequired durch MinTextLength ersetzt werden. Als neues Feld, _valueRequired wird gelesen und bei True MinTextLength gesetzt.  Aber nur noch MinTextLength gespeichert.
Genau so muss IInputFormat sowas wie vorbiddenChars bekommen, dass dann _afterEditAutoRemoveChar ersetzt.
IsFormat muss dass dann prüfen, aber ColiumnItem nimmt es trotzdem her zum löschen. Wichtig: \r usw. muss ebenfalls funktionieren. 

Dann alle Formate anpassen.


## Aufgabe
Mache aus dem hier: 
IsFormat(FormatHolder_FilepathAndName.Instance, 
IsFormat(FormatHolder_Filepath.Instance
Schnelle neue Metjoden in IO. Sowas wie: IsValidFilePath und IsValidFilepathAndName

## Aufgabe
Prüfe alle LINQ, ob Exists RICHTIG ist und ob evtl.TrueForAll oder FalseForAll benutzt werden sollte.

## Aufgabe
Verändere ich den Wert, der für eine Verlinkte Zelle zuständig ist, ändert sich der Verlinkte wert nicht.
Aufgefallen, wenn die Zeile des neuen Wertes nicht existiert.

## Aufgabe
Mache einen neuen Renderer für ZELLEN.
Texte normal darstellen.
AUSSER die Zeile beginnt mit "CaptionStartSequence"
Beginnt eine der Mehrzeilingen Einträge mit der Sequenz, wird es als Überschrift dargestellt

Dann baue noch einen replace ein, der eine Sequenze entfernt und wie Tab behandelt. Evtl. kann das mit extText gelöst werden.

## Aufgabe
IInputFormat um eine `TextCase`-Eigenschaft ergänzen (None/Upper/Lower/Title), die die Groß-/Kleinschreibung bei der Eingabe erzwingt.
Alle TextBox-basierten Controls (insb. `TextBox`, `TextBoxSuggestions`, `FlexiControl`) müssen dies bei der Eingabe und beim `AfterEdit` berücksichtigen.

## Aufgabe
EventScript, werden die TableHead-Variablen richtig zurückgeschrieben?

## Aufgabe
ControlStratgies muss ISimpleEditor implementieren. Nur TableControlStrategie nutzt dieses für die Spaltenköpfe.
Der ColumnEditor und ConnectedFormual (Field) müssen diese Optionen anzeigen

## Aufgabe
ControlStratgies: Füge eine option hinzu "Border".
Dann schaltet ControlCategorey selbst bim erstellen des Controls eine Groupbox dazwischen und gibt die Groupbox als Control zurück.
Border soll auswählbar sein in den ISimpleEditor properties.

## Aufgabe
Schau, wo DisableAllEditing du noch benutzen kannst.








