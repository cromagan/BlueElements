// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueControls.Classes;

/// <summary>
/// Diese Element kennt den Schlüssel und alle Zeilen, die erzeugt werden müssen.
/// </summary>
internal class AdderItem : IReadableTextWithKey {

    #region Constructors

    public AdderItem(string generatedTextKey) {
        Last = generatedTextKey.TrimEnd('\\').FileNameWithSuffix().Trim('+');
        KeyName = generatedTextKey;
    }

    #endregion

    #region Properties

    /// <summary>
    /// Enstpricht TextKey  (ZUTATEN\\MEHL\\) OHNE #
    /// </summary>
    public string KeyName { get; }

    public List<string> KeysAndInfo { get; } = [];

    public string Last { get; }

    public string QuickInfo => KeyName;

    #endregion

    #region Methods

    public static void AddRowsToTable(ColumnItem? originIdColumn, List<string> keysAndInfo, string generatedEntityId, ColumnItem? additionalInfoColumn) {
        if (originIdColumn?.Table is not { IsDisposed: false } tb) { return; }

        foreach (var thisKeyAndInfo in keysAndInfo) {
            var key = OriginId(thisKeyAndInfo, originIdColumn, generatedEntityId);

            var keyName = key.TrimStart(generatedEntityId + "\\");

            if (!string.IsNullOrEmpty(keyName)) {
                var r = tb.Row.GenerateAndAdd(key, "Zeilengenerator im Formular");

                if (r != null) {
                    originIdColumn.MaxCellLength = Math.Max(originIdColumn.MaxCellLength, key.Length);
                    originIdColumn.MultiLine = false;
                    originIdColumn.AfterEditAutoCorrect = false;
                    originIdColumn.AfterEditQuickSortRemoveDouble = false;
                    originIdColumn.ScriptType = ScriptType.String_Readonly;
                    originIdColumn.AdminInfo = "Diese Spalte wird als Erkennung für den Textgenerator benutzt.";
                    r.CellSet(originIdColumn, key, "Zeilengenerator im Formular");

                    if (additionalInfoColumn != null) {
                        var info = thisKeyAndInfo.SplitBy("#")[1];

                        additionalInfoColumn.MaxCellLength = Math.Max(additionalInfoColumn.MaxCellLength, info.Length);
                        additionalInfoColumn.MultiLine = false;
                        additionalInfoColumn.AfterEditAutoCorrect = false;
                        additionalInfoColumn.AfterEditQuickSortRemoveDouble = false;
                        additionalInfoColumn.ScriptType = ScriptType.String_Readonly;
                        additionalInfoColumn.AdminInfo = "Diese Spalte wird für Zusatzinfos de Textgenerators benutzt.";
                        r.CellSet(additionalInfoColumn, info, "Zeilengenerator im Formular");
                    }
                    r.UpdateRow(true, "Zeile erstellt");
                }
            }
        }
    }

    public static void RemoveRowsFromTable(ColumnItem? originIdColumn, string generatedEntityId, string keyName) {
        if (originIdColumn?.Table is not { IsDisposed: false } tb) { return; }

        var fi = new FilterCollection(tb, "Zeilengenerator im Formular");
        var key = OriginId(keyName + "#", originIdColumn, generatedEntityId);
        fi.Add(new FilterItem(originIdColumn, FilterType.Istgleich_UND_GroßKleinEgal, key));
        RowCollection.Remove(fi.Rows, "Zeilengenerator im Formular");
        fi.Dispose();
    }

    public string ReadableText() => Last;

    public QuickImage? SymbolForReadableText() => null;

    private static string OriginId(string keyAndInfos, ColumnItem? originIdColumn, string generatedEntityId) {
        if (originIdColumn?.Table is not { IsDisposed: false }) { return string.Empty; }
        return generatedEntityId + "\\" + keyAndInfos.SplitBy("#")[0];
    }

    #endregion
}