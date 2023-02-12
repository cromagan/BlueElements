﻿// Authors:
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
using BlueDatabase.Interfaces;

namespace BlueDatabase;

/// <summary>
/// Informationen über eine Datenbank oder wie diese erzeugt werden kann.
/// </summary>
public class ConnectionInfo : IReadableTextWithChangingAndKey {

    #region Fields

    private readonly string _databaseId = string.Empty;
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
        alf.AddRange(DatabaseAbstract.AllFiles);

        #region Ist es NUR ein Dateiname? Dann im Single und Multiuser suchen und zur Not eine preveredFileFormatID zurück geben

        if (uniqueId.IsFormat(FormatHolder.FilepathAndName) &&
            uniqueId.FileSuffix().ToUpper() == "MDB") {
            foreach (var thisDb in alf) {
                var d = thisDb.ConnectionData;

                if (d.UniqueID.ToUpper().EndsWith(uniqueId.ToUpper())) {
                    TableName = d.TableName;
                    Provider = d.Provider;
                    _databaseId = d.DatabaseID;
                    AdditionalData = d.AdditionalData;
                    return;
                }
            }

            TableName = SQLBackAbstract.MakeValidTableName(uniqueId.FileNameWithoutSuffix());
            Provider = null;
            _databaseId = preveredFileFormatId ?? Database.DatabaseId;
            AdditionalData = uniqueId;

            return;
        }

        #endregion

        var x = (uniqueId + "||||").SplitBy("|");

        #region Prüfen, ob eine ConnectionInfo als String übergeben wurde

        if (SQLBackAbstract.IsValidTableName(x[0]) && !string.IsNullOrEmpty(x[1])) {
            TableName = x[0];
            Provider = null;
            _databaseId = x[1];
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
                _databaseId = nci.DatabaseID;
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

        Develop.DebugPrint(FehlerArt.Warnung, "Datenbank konnte nicht gefunden werden: " + uniqueId);
    }

    public ConnectionInfo(string tablename, DatabaseAbstract? provider, string connectionString, string? additionalInfo) {
        TableName = tablename.ToUpper();
        Provider = provider;
        _databaseId = connectionString;
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
    /// Enthänt nur eine Wert wie z.B. DatabaseSQL.
    /// Um eine Datenbank wieder zu finden, muss uniqueID verwendet werden.
    /// </summary>
    public string DatabaseID => _databaseId;

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

    public void OnChanged() => Changed?.Invoke(this, System.EventArgs.Empty);

    public string ReadableText() => TableName;

    public QuickImage? SymbolForReadableText() {
        if (AdditionalData.ToLower().Contains(".mdb")) { return QuickImage.Get(ImageCode.Diskette, 16); }
        return QuickImage.Get(ImageCode.Datenbank, 16);
    }

    #endregion
}