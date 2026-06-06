# Aufgaben

## `WriteValueToDiscOrServer` — Auffälligkeiten / potentielle Bugs

### Kontext

- Basis: `BlueTable/Classes/Table.cs:2230` — ignoriert beide Parameter.
- Overrides:
  - `TableChunk.cs:684` — nutzt `oldChunkValue` und `newChunkValue` für Dirty-Chunk-Tracking.
  - `TableChunkFragments.cs:229` — nutzt nur `newChunkValue` (Chunk-Ziel + UndoItem).
  - `TableFragments.cs:238` — nutzt nur `newChunkValue` (UndoItem).
  - `TableFile.cs:414` — ignoriert beide Parameter.
- Aufrufer von `ChangeData` (welches `WriteValueToDiscOrServer` mit den Werten versorgt): siehe `BlueTable/Classes/Table.cs:1082`.

### Auffälligkeiten

1. **`RowItem.CellSet` (`RowItem.cs:342-343`)** — verwirrende Reihenfolge:
   ```
   var newChunkValue = ChunkValue;
   var oldChunkValue = newChunkValue;
   ```
   Semantisch korrekt, aber `oldChunkValue` wird nicht vom ursprünglichen `ChunkValue` gelesen, sondern von der soeben zugewiesenen `newChunkValue`. Erst danach (Z.346) wird `newChunkValue` ggf. überschrieben. Ergebnis stimmt, Benennung/Lesbarkeit ist irreführend.

2. **`oldChunkValue` ist in `TableChunkFragments` und `TableFragments` ein Dead-Parameter.** Wird in beiden Overrides nicht gelesen. Falls Undo/Redo jemals den "alten" Chunk benötigt, fehlt die Information. Aktuell funktional unkritisch, aber unbenutzter Parameter.

3. **Alle ~70 Column-Setter in `ColumnItem.cs`** rufen den 4-arg-`ChangeData`-Overload (`Table.cs:1045`) auf, der für `oldChunkValue`/`newChunkValue` fest `string.Empty` übergibt. Funktioniert nur, weil `TableChunk.GetChunkId` für Nicht-Cell-Operationen unabhängig vom Wert `Chunk_MainData` (oder `Chunk_Master`/`Chunk_Variables`/`Chunk_AdditionalUseCases`) liefert (`TableChunk.cs:395-408`). Sobald jemand `GetChunkId` ändert, bricht diese implizite Annahme.

4. **`UpdateColumnArrangementsAfterRename` (`Table.cs:2418`)** ruft `WriteValueToDiscOrServer` **direkt** auf und umgeht dabei `ChangeData`. Dadurch wird `AcquireWriteAccess` (`Table.cs:1069/1071`) **nicht** ausgeführt. Bei einer eingefrorenen oder verworfenen Tabelle würde der Direktaufruf trotzdem durchgehen. Sollte vermutlich ebenfalls über `ChangeData` laufen (oder einen Guard haben).

5. **`RowCollection.RemoveRow` (`RowCollection.cs:300`)** übergibt `string.Empty` als `oldChunkValue`, obwohl `r.ChunkValue` bekannt ist. Funktional unkritisch (s. Punkt 3), aber inkonsistent zur symmetrischen Erwartung von "alt/neu".

6. **`RowCollection.AddRow` (`RowCollection.cs:817`)** und **`RowItem.cs:550` (Auto-Repair)** — gleiches Muster: `oldChunkValue = string.Empty`, obwohl der Vorgänger-Chunk bekannt wäre (bei `AddRow` gibt es keinen, daher korrekt; bei Auto-Repair Z.550 ist der alte Wert in `oldvalue` greifbar, wird aber nicht als Chunk-Wert weitergereicht).

### Nicht-Bugs (zur Klarstellung)

- `RowItem.CellSet:343` — `oldChunkValue = newChunkValue` **vor** der Überschreibung in Z.346 ist semantisch korrekt, da die zwei Zuweisungen direkt aufeinander folgen.
- `TableChunk.WriteValueToDiscOrServer:694` (`oldId != chunkId`) verhindert korrekt redundante Dirty-Markierungen, wenn `oldChunkValue == newChunkValue`.
