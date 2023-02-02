// Authors:
// Christian Peter
//
// Copyright (c) 2023 Christian Peter
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

using BlueBasics;
using BlueBasics.Enums;
using BlueBasics.Interfaces;
using BlueDatabase.Interfaces;
using System.Collections.Generic;

namespace BlueDatabase;

/// <summary>
/// Informationen über eine Datenbank oder wie diese erzeugt werden kann.
/// </summary>
public class ConnectionInfo : IReadableText {

    #region Fields

    private string _tablename = string.Empty;

    #endregion

    #region Constructors

    /// <summary>
    /// Versucht das beste daraus zu machen....
    /// </summary>
    /// <param name="uniqueID"></param>
    public ConnectionInfo(string uniqueID, string? preveredFileFormatId) {
        var alf = new List<DatabaseAbstract>();// könnte sich ändern, deswegen Zwischenspeichern
        alf.AddRange(DatabaseAbstract.AllFiles);

        #region Ist es NUR ein Dateiname? Dann im Single und Multiuser suchen und zur Not eine preveredFileFormatID zurück geben

        if (uniqueID.IsFormat(FormatHolder.FilepathAndName) &&
            uniqueID.FileSuffix().ToUpper() == "MDB") {
            foreach (var thisDB in alf) {
                var d = thisDB.ConnectionData;

                if (d.UniqueID.ToUpper().EndsWith(uniqueID.ToUpper())) {
                    TableName = d.TableName;
                    Provider = d.Provider;
                    DatabaseID = d.DatabaseID;
                    AdditionalData = d.AdditionalData;
                    return;
                }
            }

            TableName = SQLBackAbstract.MakeValidTableName(uniqueID.FileNameWithoutSuffix());
            Provider = null;
            DatabaseID = preveredFileFormatId ?? Database.DatabaseId;
            AdditionalData = uniqueID;

            return;
        }

        #endregion

        var x = (uniqueID + "||||").SplitBy("|");

        #region Prüfen, ob eine ConnectionInfo als String übergeben wurde

        if (SQLBackAbstract.IsValidTableName(x[0]) && !string.IsNullOrEmpty(x[1])) {
            TableName = x[0];
            Provider = null;
            DatabaseID = x[1];
            AdditionalData = x[2];
            return;
        }

        #endregion

        #region  Prüfen, ob eine vorhandene Datenbank den Provider machen kann

        foreach (var thisDB in alf) {
            //var d = thisDB.ConnectionData;

            if (thisDB.ConnectionDataOfOtherTable(x[0], true) is ConnectionInfo nci) {
                TableName = nci.TableName;
                Provider = nci.Provider;
                DatabaseID = nci.DatabaseID;
                AdditionalData = nci.AdditionalData;
                return;
            }
        }

        #endregion

        //if (d.DatabaseID == Database.DatabaseId) {
        //    // Dateisystem
        //    var dn = d.AdditionalData.FilePath() + x[0] + ".mdb";
        //    if (System.IO.File.Exists(dn)) {
        //        TableName = x[0].ToUpper();
        //        Provider = thisDB;
        //        DatabaseID = Database.DatabaseId;
        //        AdditionalData = dn;
        //    }
        //}

        //if (d.DatabaseID == DatabaseMultiUser.DatabaseId) {
        //    // Dateisystem
        //    var dn = d.AdditionalData.FilePath() + x[0] + ".mdb";
        //    if (System.IO.File.Exists(dn)) {
        //        TableName = x[0].ToUpper();
        //        Provider = thisDB;
        //        DatabaseID = DatabaseMultiUser.DatabaseId;
        //        AdditionalData = dn;
        //    }
        //}
        //var tbn = SQLBackAbstract.MakeValidTableName(uniqueID.FileNameWithoutSuffix());

        //if (!SQLBackAbstract.IsValidTableName(x[0])) {
        //    var ci = DatabaseAbstract.ProviderOf(x[0]);

        //    if (ci != null) {
        //        TableName = ci.TableName;
        //        Provider = ci.Provider;
        //        DatabaseID = ci.DatabaseID;
        //        AdditionalData = ci.AdditionalData;
        //    }
        //}

        Develop.DebugPrint(FehlerArt.Warnung, "Datenbank konnte nicht gefunden werden: " + uniqueID);
    }

    public ConnectionInfo(string tablename, DatabaseAbstract? provider, string connectionString, string? additionalInfo) {
        TableName = tablename.ToUpper();
        Provider = provider;
        DatabaseID = connectionString;
        AdditionalData = additionalInfo ?? string.Empty;
    }

    #endregion

    #region Properties

    /// <summary>
    /// z.B. wenn ein Dateiname oder sowas mitgegeben werden soll.
    /// Ist nur wichtig für von DatabaseAbstract abgeleiten Klassen und nur diese können damit umgehen.
    /// </summary>
    public string AdditionalData { get; set; } = string.Empty;

    /// <summary>
    /// Eine Kennung, die von von DatabaseAbstract abgeleiten Klassen erkannt werden kann.
    /// Enthänt nur eine Wert wie z.B. DatabaseSQL.
    /// Um eine Datenbank wieder zu finden, muss uniqueID verwendet werden.
    /// </summary>
    public string DatabaseID { get; } = string.Empty;

    /// <summary>
    /// Welche bereits vorhandene Datenbank den in dieser Klasse aufgezeigten Tabellenamen erzeugen kann
    /// </summary>
    public DatabaseAbstract? Provider { get; set; }

    /// <summary>
    /// Die Tabelle, um die es geht.
    /// </summary>
    public string TableName {
        get => _tablename; set {
            if (!SQLBackAbstract.IsValidTableName(value)) {
                Develop.DebugPrint(FehlerArt.Fehler, "Tabellenname ungültig: " + value);
            }

            _tablename = value;
        }
    }

    /// <summary>
    /// Eindeutiger Schlüssel, mit dem eine Datenbank von vorhandenen Datenbanken wieder gefunden werden kann.
    /// </summary>
    public string UniqueID => TableName + "|" + DatabaseID + "|" + AdditionalData;

    #endregion

    #region Methods

    public string ReadableText() => TableName;

    public QuickImage? SymbolForReadableText() {
        if (AdditionalData.ToLower().Contains(".mdb")) { return QuickImage.Get(ImageCode.Diskette, 16); }
        return QuickImage.Get(ImageCode.Datenbank, 16);
    }

    #endregion
}