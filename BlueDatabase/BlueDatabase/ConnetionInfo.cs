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

using System;
using System.Collections.Generic;
using BlueBasics;
using BlueBasics.Enums;
using BlueBasics.Interfaces;
using static BlueDatabase.DatabaseAbstract;

namespace BlueDatabase;

/// <summary>
/// Informationen über eine Datenbank oder wie diese erzeugt werden kann.
/// </summary>
public class ConnectionInfo : IReadableTextWithChangingAndKey {

    #region Fields

    private string _additionalData = string.Empty;
    private DatabaseAbstract? _provider;
    private string _tablename = string.Empty;

    #endregion

    #region Constructors

    /// <summary>
    /// Versucht das beste daraus zu machen....
    /// </summary>
    /// <param name="uniqueId"></param>
    /// <param name="preveredFileFormatId"></param>
    public ConnectionInfo(string uniqueId, string? preveredFileFormatId) {
        var alf = new List<DatabaseAbstract>();// könnte sich ändern, deswegen Zwischenspeichern
        alf.AddRange(AllFiles);

        #region Ist es NUR ein Dateiname? Dann im Single und Multiuser suchen und zur Not eine preveredFileFormatID zurück geben

        if (uniqueId.IsFormat(FormatHolder.FilepathAndName) &&
            uniqueId.FileSuffix().ToUpper() is "MDB" or "BDB") {
            foreach (var thisDb in alf) {
                var d = thisDb.ConnectionData;

                if (d.UniqueID.ToUpper().EndsWith(uniqueId.ToUpper())) {
                    TableName = d.TableName;
                    Provider = d.Provider;
                    DatabaseID = d.DatabaseID;
                    AdditionalData = d.AdditionalData;
                    return;
                }
            }

            TableName = MakeValidTableName(uniqueId.FileNameWithoutSuffix());
            Provider = null;
            DatabaseID = preveredFileFormatId ?? Database.DatabaseId;
            AdditionalData = uniqueId;

            return;
        }

        #endregion

        var x = (uniqueId + "||||").SplitBy("|");

        #region Prüfen, ob eine ConnectionInfo als String übergeben wurde

        if (IsValidTableName(x[0], false) && !string.IsNullOrEmpty(x[1])) {
            TableName = x[0];
            Provider = null;
            DatabaseID = x[1];
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
                DatabaseID = nci.DatabaseID;
                AdditionalData = nci.AdditionalData;
                return;
            }
        }

        #endregion
    }

    public ConnectionInfo(string tablename, DatabaseAbstract? provider, string connectionString, string? additionalInfo) {
        TableName = tablename.ToUpper();
        Provider = provider;
        DatabaseID = connectionString;
        AdditionalData = additionalInfo ?? string.Empty;
    }

    #endregion

    #region Events

    public event EventHandler? Changed;

    #endregion

    #region Properties

    /// <summary>
    /// z.B. wenn ein Dateiname oder sowas mitgegeben werden soll.
    /// Ist nur wichtig für von DatabaseAbstract abgeleiten Klassen und nur diese können damit umgehen.
    /// </summary>
    public string AdditionalData {
        get => _additionalData;
        set {
            if (_additionalData == value) { return; }
            _additionalData = value;
            OnChanged();
        }
    }

    /// <summary>
    /// Eine Kennung, die von von DatabaseAbstract abgeleiten Klassen erkannt werden kann.
    /// Enthält nur einen Wert wie z.B. DatabaseSQL.
    /// Um eine Datenbank wieder zu finden, muss uniqueID verwendet werden.
    /// </summary>
    public string DatabaseID { get; } = string.Empty;

    public string KeyName => UniqueID;

    /// <summary>
    /// Welche bereits vorhandene Datenbank den in dieser Klasse aufgezeigten Tabellenamen erzeugen kann
    /// </summary>
    public DatabaseAbstract? Provider {
        get => _provider;
        set {
            if (_provider == value) { return; }
            _provider = value;
            OnChanged();
        }
    }

    /// <summary>
    /// Die Tabelle, um die es geht.
    /// </summary>
    public string TableName {
        get => _tablename;
        set {
            if (!IsValidTableName(value, false)) {
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

    public void OnChanged() => Changed?.Invoke(this, System.EventArgs.Empty);

    public string ReadableText() => TableName;

    public QuickImage SymbolForReadableText() {
        if (AdditionalData.ToLower().Contains(".bdb")) { return QuickImage.Get(ImageCode.Diskette, 16); }
        if (AdditionalData.ToLower().Contains(".mdb")) { return QuickImage.Get(ImageCode.Diskette, 16); }
        return QuickImage.Get(ImageCode.Datenbank, 16);
    }

    #endregion
}