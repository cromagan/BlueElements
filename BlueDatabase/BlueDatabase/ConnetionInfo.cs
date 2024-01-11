// Authors:
// Christian Peter
//
// Copyright (c) 2024 Christian Peter
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

#nullable enable

using System.Collections.Generic;
using BlueBasics;
using BlueBasics.Enums;
using BlueBasics.Interfaces;
using static BlueDatabase.Database;

namespace BlueDatabase;

/// <summary>
/// Informationen über eine Datenbank oder wie diese erzeugt werden kann.
/// </summary>
public class ConnectionInfo : IReadableTextWithKey {

    #region Fields

    private string _tablename = string.Empty;

    #endregion

    #region Constructors

    /// <summary>
    /// Versucht das beste daraus zu machen....
    /// </summary>
    /// <param name="uniqueId"></param>
    /// <param name="preveredFileFormatId"></param>
    /// <param name="mustbefreezed"></param>
    public ConnectionInfo(string uniqueId, string? preveredFileFormatId, string mustbefreezed) {
        var alf = new List<Database>();// könnte sich ändern, deswegen Zwischenspeichern
        alf.AddRange(AllFiles);

        #region Ist es NUR ein Dateiname? Dann im Single und Multiuser suchen und zur Not eine preveredFileFormatID zurück geben

        if (uniqueId.IsFormat(FormatHolder.FilepathAndName) &&
            uniqueId.FileSuffix().ToUpper() is "MDB" or "BDB" or "MBDB") {
            foreach (var thisDb in alf) {
                var d = thisDb.ConnectionData;

                if (d.UniqueId.ToUpper().EndsWith(uniqueId.ToUpper())) {
                    TableName = d.TableName;
                    Provider = d.Provider;
                    DatabaseId = d.DatabaseId;
                    AdditionalData = d.AdditionalData;
                    return;
                }
            }

            TableName = MakeValidTableName(uniqueId.FileNameWithoutSuffix());
            Provider = null;
            DatabaseId = preveredFileFormatId ?? DatabaseMu.DatabaseId;
            AdditionalData = uniqueId;
            MustBeFreezed = mustbefreezed;

            return;
        }

        #endregion

        var x = (uniqueId + "||||").SplitBy("|");

        #region Prüfen, ob eine ConnectionInfo als String übergeben wurde

        if (IsValidTableName(x[0], false) && !string.IsNullOrEmpty(x[1])) {
            TableName = x[0];
            Provider = null;
            DatabaseId = x[1];
            AdditionalData = x[2];
            return;
        }

        #endregion

        #region  Prüfen, ob eine vorhandene Datenbank den Provider machen kann

        foreach (var thisDb in alf) {
            //var d = thisDB.ConnectionData;

            if (thisDb.ConnectionDataOfOtherTable(x[0], true) is ConnectionInfo nci) {
                TableName = nci.TableName;
                Provider = nci.Provider;
                DatabaseId = nci.DatabaseId;
                AdditionalData = nci.AdditionalData;
                return;
            }
        }

        #endregion
    }

    public ConnectionInfo(string tablename, Database? provider, string databaseId, string additionalInfo, string mustbefreezed) {
        TableName = tablename.ToUpper();
        Provider = provider;
        DatabaseId = databaseId;
        AdditionalData = additionalInfo;
        MustBeFreezed = mustbefreezed;
    }

    #endregion

    //public event EventHandler? Changed;

    #region Properties

    /// <summary>
    /// z.B. wenn ein Dateiname oder sowas mitgegeben werden soll.
    /// Ist nur wichtig für von Database abgeleiten Klassen und nur diese können damit umgehen.
    /// </summary>
    public string AdditionalData { get; } = string.Empty;

    /// <summary>
    /// Eine Kennung, die von von Database abgeleiten Klassen erkannt werden kann.
    /// Enthält nur einen Wert wie z.B. DatabaseSQL.
    /// Um eine Datenbank wieder zu finden, muss uniqueID verwendet werden.
    /// </summary>
    public string DatabaseId { get; } = string.Empty;

    public string KeyName => UniqueId;
    public string MustBeFreezed { get; } = string.Empty;

    /// <summary>
    /// Welche bereits vorhandene Datenbank den in dieser Klasse aufgezeigten Tabellenamen erzeugen kann
    /// </summary>
    public Database? Provider { get; set; }

    /// <summary>
    /// Die Tabelle, um die es geht.
    /// </summary>
    public string TableName {
        get => _tablename;
        private set {
            if (!IsValidTableName(value, false)) {
                Develop.DebugPrint(FehlerArt.Fehler, "Tabellenname ungültig: " + value);
            }

            _tablename = value;
        }
    }

    /// <summary>
    /// Eindeutiger Schlüssel, mit dem eine Datenbank von vorhandenen Datenbanken wieder gefunden werden kann.
    /// </summary>
    public string UniqueId => TableName + "|" + DatabaseId + "|" + AdditionalData;

    #endregion

    //public void OnChanged() => Changed?.Invoke(this, System.EventArgs.Empty);

    #region Methods

    public string ReadableText() => TableName;

    public QuickImage SymbolForReadableText() {
        if (AdditionalData.ToLower().Contains(".bdb")) { return QuickImage.Get(ImageCode.Diskette, 16); }
        if (AdditionalData.ToLower().Contains(".mdb")) { return QuickImage.Get(ImageCode.Diskette, 16); }
        if (AdditionalData.ToLower().Contains(".mbdb")) { return QuickImage.Get(ImageCode.Diskette, 16); }
        return QuickImage.Get(ImageCode.Datenbank, 16);
    }

    #endregion
}