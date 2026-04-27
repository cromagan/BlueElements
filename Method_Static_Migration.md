# Method_* zu statischen Klassen migrieren

## Übersicht

Alle `Method_*`-Klassen werden von instanziierbaren Klassen zu statischen Klassen umgewandelt.
Die Basisklasse `Method` wird mit `static abstract` Members versehen, sodass kein Dispatch über Instanzen mehr nötig ist.

---

## Suchen/Ersetzen für alle Method_*.cs Dateien

Die folgenden Suchen/Ersetzen können in **allen** `Method_*.cs`-Dateien ausgeführt werden.
In Visual Studio: `Strg+Shift+H` → "In Dateien ersetzen" → Pfad auf den Projektordner setzen.

### Schritt 1: Klassendeklarationen (in Method_*.cs)

| Suchen (Regulärer Ausdruck) | Ersetzen | Hinweis |
|---|---|---|
| `public class (Method_\w+) : Method` | `public static class $1 : Method` | Alle direkten Method-Unterklassen |
| `internal class (Method_\w+) : Method` | `internal static class $1 : Method` | Interne Unterklassen |
| `public class (Method_\w+) : Method_TableGeneric` | `public static class $1 : Method_TableGeneric` | BlueTable-Unterklassen |
| `internal class (Method_\w+) : Method_TableGeneric` | `internal static class $1 : Method_TableGeneric` | Interne BlueTable-Unterklassen |
| `public class (Method_\w+) : Method, IComandBuilder` | `public static class $1 : Method, IComandBuilder` | IComandBuilder-Unterklassen (2 Stück) |

### Schritt 2: Override-Properties zu Static Override (in Method_*.cs)

| Suchen | Ersetzen | Hinweis |
|---|---|---|
| `public override List<List<string>> Args` | `public static override List<List<string>> Args` | |
| `public override string Command` | `public static override string Command` | |
| `public override List<string> Constants` | `public static override List<string> Constants` | |
| `public override string Description` | `public static override string Description` | |
| `public override bool GetCodeBlockAfter` | `public static override bool GetCodeBlockAfter` | |
| `public override int LastArgMinCount` | `public static override int LastArgMinCount` | |
| `public override MethodType MethodLevel` | `public static override MethodType MethodLevel` | |
| `public override bool MustUseReturnValue` | `public static override bool MustUseReturnValue` | |
| `public override string Returns` | `public static override string Returns` | |
| `public override string StartSequence` | `public static override string StartSequence` | |
| `public override string Syntax` | `public static override string Syntax` | |

### Schritt 3: Override-Methoden zu Static Override (in Method_*.cs)

#### 3a: Standard-DoIt (die meisten Klassen)
| Suchen | Ersetzen |
|---|---|
| `public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld)` | `public static override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld)` |

#### 3b: Virtual-DoIt-Override (nur 9 Klassen: Method_var, Method_If, Method_ForEach, Method_Do, Method_Exists, Method_IsNullOrEmpty, Method_IsNullOrZero, Method_ForEachRow, Method_ForEachRow2)
| Suchen | Ersetzen |
|---|---|
| `public override DoItFeedback DoIt(VariableCollection varCol, CanDoFeedback infos, ScriptProperties scp)` | `public static override DoItFeedback DoIt(VariableCollection varCol, CanDoFeedback infos, ScriptProperties scp)` |

### Schritt 4: Singleton-Felder entfernen (3 Dateien)

**Method_Break.cs:** Zeile `public static readonly Method Method = new Method_Break();` → **komplett löschen**

**Method_GetNote.cs:** Zeile `public static readonly Method Method = new Method_GetNote();` → **komplett löschen**

**Method_SetError.cs:** Zeile `public static readonly Method Method = new Method_SetError();` → **komplett löschen**

---

## Suchen/Ersetzen für Konsumenten (nach Basisklasse-Umstellung)

### Schritt 5: ScriptProperties (ScriptProperties.cs)

| Suchen | Ersetzen |
|---|---|
| `List<Method> allowedMethods` | `List<Type> allowedMethods` |
| `public List<Method> AllowedMethods` | `public List<Type> AllowedMethods` |

### Schritt 6: Alle ScriptProperties-Konstruktoren und AllowedMethods-Nutzung

In allen Dateien, die `new ScriptProperties(...)` aufrufen oder `scp.AllowedMethods` nutzen:

| Suchen | Ersetzen |
|---|---|
| `Method.AllMethods` | `Method.AllMethodTypes` |
| `Method.GetMethods(` | `Method.GetMethodTypes(` |
| `scp.AllowedMethods` | `scp.AllowedMethods` *(keine Änderung, der Typ ändert sich automatisch)* |
| `Method_Break.Method` | `typeof(Method_Break)` |
| `Method_SetError.Method` | `typeof(Method_SetError)` |
| `Method_GetNote.Method` | `typeof(Method_GetNote)` |

### Schritt 7: Befehlsreferenz.cs

| Suchen | Ersetzen |
|---|---|
| `if (e.Item is ReadableListItem { Item: Method thisc })` | `if (e.Item is MethodListItem ml)` |
| `GetUses(thisc, 5);` | `GetUses(ml.MethodType, 5);` |
| `co += thisc.HintText();` | `co += ml.HintText();` |
| `foreach (var thisc in Method.AllMethods)` | `foreach (var thisc in Method.AllMethodTypes)` |
| `lstCommands.ItemAdd(ItemOf(thisc));` | `lstCommands.ItemAdd(new MethodListItem(thisc));` |

Die Methode `GetUses` muss angepasst werden (siehe Etappe 4 in der Umsetzung).

### Schritt 8: ScriptEditorGeneric.cs

| Suchen | Ersetzen |
|---|---|
| `foreach (var thisc in Method.AllMethods)` | `foreach (var thisc in Method.AllMethodTypes)` |
| `if (thisc is IComandBuilder ic)` | `if (typeof(IComandBuilder).IsAssignableFrom(thisc))` |
| `if (e.Item.KeyName == ic.KeyName)` | `if (e.Item.KeyName == Method.GetCommand(thisc))` |
| `var c = ic.GetCode(this);` | `var c = Method.GetComandBuilderCode(thisc, this);` |
| `thisc.Command.Equals(e.HoveredWord` | `Method.GetCommand(thisc).Equals(e.HoveredWord` |
| `e.ToolTipTitle = thisc.Syntax;` | `e.ToolTipTitle = Method.GetSyntax(thisc);` |
| `e.ToolTipText = thisc.HintText();` | `e.ToolTipText = Method.GetHintText(thisc);` |
| `items.Add(new SnippetAutocompleteItem(thisc.Syntax + " "));` | `items.Add(new SnippetAutocompleteItem(Method.GetSyntax(thisc) + " "));` |
| `items.Add(new AutocompleteItem(thisc.Command));` | `items.Add(new AutocompleteItem(Method.GetCommand(thisc)));` |
| `if (!string.IsNullOrEmpty(thisc.Returns))` | `if (!string.IsNullOrEmpty(Method.GetReturns(thisc)))` |
| `items.Add(new SnippetAutocompleteItem("var " + thisc.Returns + " = " + thisc.Syntax + "; "));` | `items.Add(new SnippetAutocompleteItem("var " + Method.GetReturns(thisc) + " = " + Method.GetSyntax(thisc) + "; "));` |

Für die `IComandBuilder`-Liste (Assistent-Tab):
| Suchen | Ersetzen |
|---|---|
| `ic.ComandDescription()` | `Method.GetComandBuilderDescription(thisc)` |
| `ic.KeyName` | `Method.GetCommand(thisc)` |
| `ic.ComandImage()` | `Method.GetComandBuilderImage(thisc)` |

### Schritt 9: Method.cs interne Änderungen

| Suchen | Ersetzen |
|---|---|
| `public static List<Method> AllMethods` | `public static List<Type> AllMethodTypes` |
| `public static List<Method> GetMethods(` | `public static List<Type> GetMethodTypes(` |
| `foreach (var thism in AllMethods)` | `foreach (var thism in AllMethodTypes)` |
| `thism.MethodLevel` | `Method.GetMethodLevel(thism)` |
| `m.Add(thism);` | `m.Add(thism);` *(keine Änderung)* |

### Schritt 10: Weitere Konsumenten

**Table.cs:**
| Suchen | Ersetzen |
|---|---|
| `Method.AllMethods.FirstOrDefault(m => m.Command == "getnote")` | `Method.AllMethodTypes.FirstOrDefault(t => Method.GetCommand(t) == "getnote")` |
| `meth.Add(Method_SetError.Method);` | `meth.Add(typeof(Method_SetError));` |
| `var meth = Method.GetMethods(` | `var meth = Method.GetMethodTypes(` |

**TableScriptDescription.cs:**
| Suchen | Ersetzen |
|---|---|
| `foreach (var thisc in Method.AllMethods)` | `foreach (var thisc in Method.AllMethodTypes)` |
| `thisc.MethodLevel >= MethodType.ManipulatesUser` | `Method.GetMethodLevel(thisc) >= MethodType.ManipulatesUser` |

**Variable.cs:**
| Suchen | Ersetzen |
|---|---|
| `foreach (var thisc in Method.AllMethods)` | `foreach (var thisc in Method.AllMethodTypes)` |
| `thisc.Command` | `Method.GetCommand(thisc)` |

**TimerPadItem.cs:**
| Suchen | Ersetzen |
|---|---|
| `new ScriptProperties("Timer", Method.AllMethods,` | `new ScriptProperties("Timer", Method.AllMethodTypes,` |

**ScriptButtonPadItem.cs:**
| Suchen | Ersetzen |
|---|---|
| `new ScriptProperties("ScriptButton", Method.AllMethods,` | `new ScriptProperties("ScriptButton", Method.AllMethodTypes,` |

**RowAdder.cs:**
| Suchen | Ersetzen |
|---|---|
| `var m = Method.GetMethods(MethodType.Sub);` | `var m = Method.GetMethodTypes(MethodType.Sub);` |

**DynamicSymbolPadItem.cs:**
| Suchen | Ersetzen |
|---|---|
| `var m = Method.GetMethods(MethodType.Standard);` | `var m = Method.GetMethodTypes(MethodType.Standard);` |

**VariableItemCollectionPad.cs:**
| Suchen | Ersetzen |
|---|---|
| `var m = Method.GetMethods(MethodType.ManipulatesUser);` | `var m = Method.GetMethodTypes(MethodType.ManipulatesUser);` |

### Schritt 11: Method_If, Method_ForEach, Method_Do, Method_ForEachRow, Method_ForEachRow2

Diese Klassen erstellen neue `ScriptProperties` mit `[.. scp.AllowedMethods, Method_Break.Method]`:

| Suchen | Ersetzen |
|---|---|
| `[.. scp.AllowedMethods, Method_Break.Method]` | `[.. scp.AllowedMethods, typeof(Method_Break)]` |

---

## Dateien mit manuellen Änderungen (nicht per Suchen/Ersetzen)

Folgende Dateien erfordern manuelle Anpassungen, die über einfaches Suchen/Ersetzen hinausgehen:

1. **BlueScript/Methods/Method.cs** — Komplette Umstellung auf `static abstract`, Dispatch-Infrastruktur, UsesInDB als Dictionary
2. **BlueScript/Classes/Script.cs** — Dispatch über Type statt über Instanz
3. **BlueScript/Methods/Method_TableGeneric.cs** — `abstract class` → `static abstract class` (C# unterstützt das nicht direkt → bleibt `abstract class`, aber mit statischen Membern)
4. **BlueControls/Classes/Befehlsreferenz.cs** — Neuer `MethodListItem` statt `ReadableListItem`
5. **BlueControls/Editoren/ScriptEditorGeneric.cs** — Type-basierter Dispatch
6. **BlueControls/Interfaces/IComandBuilder.cs** — Statische Methoden-Unterstützung prüfen
7. **BlueControls/AdditionalScriptMethods/Method_CheckBitmap.cs** — IComandBuilder statisch
8. **BlueControls/AdditionalScriptMethods/Method_MouseDownUp.cs** — IComandBuilder statisch

---

## Reihenfolge der Umsetzung

1. **Basisklasse Method.cs** umbauen (static abstract Members, Dispatch-Helper, UsesInDB)
2. **Method_TableGeneric.cs** anpassen
3. **Alle Method_*.cs** per Suchen/Ersetzen (Schritte 1-4)
4. **ScriptProperties.cs** und **Script.cs** anpassen (Schritte 5-6)
5. **UI-Konsumenten** anpassen (Schritte 7-8)
6. **Weitere Konsumenten** anpassen (Schritte 9-11)
7. **Bauen und Fehler beheben**
