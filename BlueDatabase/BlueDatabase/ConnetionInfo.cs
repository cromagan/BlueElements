// Authors:
// Christian Peter
//
// Copyright (c) 2022 Christian Peter
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
using System.Collections.Generic;
using static BlueBasics.Extensions;

namespace BlueDatabase;

/// <summary>
/// Informationen über eine Datenbank oder wie diese erzeugt werden kann.
/// </summary>
public class ConnectionInfo {

    #region Fields

    private string _tablename = string.Empty;

    #endregion

    #region Constructors

    /// <summary>
    /// Versucht das beste daraus zu machen....
    /// </summary>
    /// <param name="uniqueID"></param>
    public ConnectionInfo(string uniqueID) {
        if (uniqueID.FileSuffix().ToUpper() == "MDB" &&
            uniqueID.Substring(1, 1)==":" &&
            System.IO.File.Exists(uniqueID)) {
            TableName = SQLBackAbstract.MakeValidTableName(uniqueID.FileNameWithoutSuffix());
            Provider = null;
            DatabaseID = Database.DatabaseID;
            AdditionalData = uniqueID;
            return;
        }

        var x = (uniqueID + "||||").SplitBy("|");


        var alf = new List<DatabaseAbstract>();// könnte sich ändern, deswegen Zwischenspeichern
        alf.AddRange(DatabaseAbstract.AllFiles);


        foreach (var thisDB in alf) {
            var d = thisDB.ConnectionData;

            if (d.DatabaseID == x[1]) {
                TableName = d.TableName;
                Provider = thisDB;
                DatabaseID = d.DatabaseID;
                AdditionalData = d.AdditionalData;
                return;
            }

            if (d.DatabaseID == Database.DatabaseID) {
                var dn = d.AdditionalData.FilePath() + x[0] + ".mdb";
                if (System.IO.File.Exists(dn)) {
                    TableName = x[0].ToUpper();
                    Provider = thisDB;
                    DatabaseID = Database.DatabaseID;
                    AdditionalData = dn;
                }
            }
        }

        //#region Wenn die Connection einem Dateinamen entspricht, versuchen den zu laden

        //if (FileExists(ci.AdditionalData)) {
        //    if (ci.AdditionalData.FileSuffix().ToLower() == "mdb") {
        //        return new Database(ci.AdditionalData, false, false, ci.TableName);
        //    }

        //    //if (connectionID.FileSuffix().ToLower() == "mdf") {
        //    //    var x = new SQLBackMicrosoftCE(connectionID, false);

        //    //    var tables = x.Tables();
        //    //    var tabn = string.Empty;
        //    //    if (tables.Count >= 1) { tabn = tables[0]; }

        //    //    if (string.IsNullOrEmpty(tabn)) {
        //    //        tabn = "MAIN";
        //    //        x.RepairAll(tabn);
        //    //    }

        //    //    return new DatabaseSQL(x, false, tabn);
        //    //}
        //}

        //#endregion

        ////SQLBackAbstract.GetSQLBacks();

        //if (SQLBackAbstract.ConnectedSQLBack != null) {
        //    foreach (var thisSQL in SQLBackAbstract.ConnectedSQLBack) {
        //        var h = thisSQL.HandleMe(ci);
        //        if (h != null) { return new DatabaseSQL(h, false, ci.TableName); }
        //    }
        //}
    }

    public ConnectionInfo(string tablename, DatabaseAbstract? provider, string connectionString, string? additionalInfo) {
        TableName = tablename.ToUpper();
        Provider = provider;
        DatabaseID = connectionString;
        AdditionalData = additionalInfo;
    }

    #endregion

    #region Properties

    /// <summary>
    /// z.B. wenn ein Dateiname oder sowas mitgegeben werden soll.
    /// Ist nur wichtig für von DatabaseAbstract abgeleiten Klassen und nur diese können damit umgehen.
    /// </summary>
    public string? AdditionalData { get; set; }

    /// <summary>
    /// Eine Kennung, die von von DatabaseAbstract abgeleiten Klassen erkannt werden kann.
    /// </summary>
    public string DatabaseID { get; private set; }

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
                Develop.DebugPrint(BlueBasics.Enums.FehlerArt.Fehler, "Tabellenname ungültig: " + value);
            }

            _tablename = value;
        }
    }

    /// <summary>
    /// Eindeutiger Schlüssel, mit dem eine Datenbank von vorhandenen Datenbanken wieder gefunden werden kann.
    /// </summary>
    public string UniqueID { get => TableName +"|"+ DatabaseID + "|" + AdditionalData; }

    #endregion
}