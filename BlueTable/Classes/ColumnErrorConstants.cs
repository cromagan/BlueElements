// Authors:
// Christian Peter
//
// Copyright © 2026 Christian Peter
// https://github.com/cromagan/BlueElements
//
// License: GNU Affero General Public License v3.0
// https://github.com/cromagan/BlueElements/blob/master/LICENSE
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.

namespace BlueTable.Classes;

public static class ColumnErrorConstants {

    #region Fields

    public const string AddOtherCellsNotAllowed = "'Dropdownmenu alles hinzufügen' bei diesem Format nicht erlaubt.";
    public const string AdministratorNotAllowed = "'#Administrator' bei den Bearbeitern entfernen.";
    public const string CaptionMissing = "Spalten Beschriftung fehlt.";
    public const string CellLinkError = "Zell-Verlinkung fehlerhaft";
    public const string CellSizeTooLarge = "Zellengröße zu groß!";
    public const string CellSizeTooSmall = "Zellengröße zu klein!";
    public const string ChunkAutoFilterJokerInvalid = "Der Autofilter-Joker darf bei dieser Spalte nicht gesetzt sein.";
    public const string ChunkAutoFilterRequired = "Auto-Filter müssen bei dieser Spalte erlaubt sein.";
    public const string ChunkExtendedFilterInvalid = "Erweiterte Filter sind bei dieser Spalte nicht erlaubt.";
    public const string ChunkMustIgnoreRowFilter = "Diese Spalte muss bei Zeilenfiltern ignoriert werden.";
    public const string ChunkMustSaveContent = "Chunk Spalten der Inhalt gespeichert werden.";
    public const string ChunkNoRelation = "Beziehungen zu anderen Zeilen und Chunk-Wert nicht kombinierbar.";
    public const string ChunkOnlyInCbcb = "Chunk-Spalten nur in Tabellen des Typs CBCB erlaubt.";
    public const string ChunkScriptReadonly = "Diese Spalte darf im Skript nur als ReadOnly vorhanden sein.";
    public const string ChunkTextFilterRequired = "Texteingabe-Filter sind bei dieser Spalte nötig.";
    public const string CircularReference = "Zirkelbezug mit verknüpfter Tabelle.";
    public const string ColumnKeyDuplicate = "Der Spaltenname (Schlüssel) bereits vorhanden.";
    public const string ColumnKeyInvalid = "Der Spaltenname (Schlüssel) ist ungültig.";
    public const string ColumnKeyUndefined = "Der Spaltenname (Schlüssel) ist nicht definiert.";
    public const string DeselectAllNotAllowed = "'Dropdownmenu alles abwählen' bei diesem Format nicht erlaubt.";
    public const string DropdownNotSelectedAddAll = "Dropdownmenu nicht ausgewählt, 'alles hinzufügen' prüfen.";
    public const string DropdownNotSelectedDeselectAll = "Dropdownmenu nicht ausgewählt, 'alles abwählen' prüfen.";
    public const string DropdownNotSelectedItems = "Dropdownmenu nicht ausgewählt, Dropdown-Items vorhanden.";
    public const string EditDespiteLockNeedsMethod = "Wenn die Zeilensperre ignoriert werden soll, muss eine Bearbeitungsmethode definiert sein.";
    public const string FilterCombinationInvalid = "Filter Kombination nicht möglich.";
    public const string FilterRequiresMultiline = "Dieser Filter kann nur bei mehrzeiligen Spalten benutzt werden.";
    public const string FirstColumnMustSaveContent = "Bei der ersten Spalte muss der Inhalt gespeichert werden.";
    public const string FirstColumnNoRelation = "Beziehungen zu anderen Tabellen und Erstspalte nicht kombinierbar.";
    public const string FixedWidthRequired = "Bei Spalten ohne Inhaltsspeicherung muss eine feste Spaltenbreite angegeben werden.";
    public const string FormatDropdownOnly = "Format unterstützt nur Dropdown-Menü.";
    public const string FormatNoDropdownEdit = "Format unterstützt keine Auswahlmenü-Bearbeitung.";
    public const string FormatNoStandardEdit = "Format unterstützt keine Standard-Bearbeitung.";
    public const string InvalidGroupChar = "Unerlaubtes Zeichen bei den Gruppen, die eine Zelle bearbeiten dürfen.";
    public const string KeyColumnMustSaveContent = "Bei Schlüsselspalten muss der Inhalt gespeichert werden.";
    public const string KeyColumnNoRowRelation = "Beziehungen zu anderen Zeilen und Schlüsselspalte nicht kombinierbar.";
    public const string KeyColumnScriptReadonly = "Schlüsselspalten müssen im Script als Readonly vorhanden sein.";
    public const string LinkedCellScriptInvalid = "Spalten mit Verlinkungen zu anderen Tabellen können im Skript nicht verwendet werden. ImportLinked im Skript benutzen und den Skript-Type auf nicht vorhanden setzen.";
    public const string LinkedDataOnlyWithLinkedCells = "Nur verlinkte Zellen können Daten über verlinkte Zellen enthalten.";
    public const string LinkedKeyColumnMissing = "Die verknüpfte Schlüsselspalte existiert nicht.";
    public const string LinkedMustSaveContent = "Bei Spalten mit Verknüpfung zu anderen Tabellen der Inhalt gespeichert werden.";
    public const string LinkedTableMissing = "Verknüpfte Tabelle fehlt oder existiert nicht.";
    public const string MaxLengthTooLarge = "Maximallänge zu groß!";
    public const string MultilineNotSupported = "Format unterstützt keine mehrzeiligen Texte.";
    public const string MustIgnoreRowFilter = "Spalten ohne Inhaltsspeicherung müssen bei Zeilenfiltern ignoriert werden.";
    public const string NoAutoFilterRemoveJoker = "Wenn kein Autofilter erlaubt ist, immer anzuzeigende Werte entfernen";
    public const string NoDropdownItems = "Keine Dropdown-Items vorhanden bzw. Alles hinzufügen nicht angewählt.";
    public const string NoLinkedFilterDefined = "Keine Filter für verknüpfte Tabelle definiert.";
    public const string RelationMustSaveContent = "Bei Spalten mit Beziehungen Inhalt gespeichert werden.";
    public const string RelationNoAutoEdit = "Dieses Format unterstützt keine automatischen Bearbeitungen wie Runden, Ersetzungen, Fehlerbereinigung, immer Großbuchstaben, Erlaubte Zeichen oder Sortierung.";
    public const string RelationNotAllowedOnFirstColumn = "Diese Funktion ist bei der ersten Spalte nicht erlaubt.";
    public const string RelationRequiresMultiline = "Bei dieser Funktion muss mehrzeilig ausgewählt werden.";
    public const string RemoveEditPermissions = "Bearbeitungsberechtigungen entfernen, wenn keine Bearbeitung erlaubt ist.";
    public const string RendererMissing = "Es ist kein Renderer angebeben";
    public const string RoundMaxFiveDecimals = "Beim Runden maximal 5 Nachkommastellen möglich";
    public const string RoundOnlySingleLine = "Runden nur bei einzeiligen Texten möglich";
    public const string ScriptTypeUndefined = "Der Typ im Skript ist nicht definiert.";
    public const string SortOnlyMultiline = "Sortierung kann nur bei mehrzeiligen Feldern erfolgen.";
    public const string SpellCheckNotPossible = "Rechtschreibprüfung bei diesem Format nicht möglich.";
    public const string TableDisposed = "Tabelle verworfen";

    #endregion
}