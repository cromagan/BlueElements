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

using BlueBasics;
using BlueBasics.Enums;
using BlueBasics.Interfaces;
using BlueControls.Controls;
using BlueControls.Enums;
using BlueControls.Interfaces;
using static BlueControls.ItemCollectionList.AbstractListItemExtension;
using BlueControls.ItemCollectionList;
using BlueControls.ItemCollectionPad.FunktionsItems_Formular.Abstract;

//using BlueDatabase;

using static BlueDatabase.Database;
using BlueDatabase.Enums;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using static BlueBasics.Converter;

using BlueBasics;
using BlueBasics.Interfaces;
using BlueControls.Controls;

using BlueDatabase;

using System.Collections.Generic;
using System.Collections.ObjectModel;
using static BlueDatabase.Database;
using BlueControls.ItemCollectionList;
using static BlueControls.ItemCollectionList.AbstractListItemExtension;

using System.Windows.Input;
using BlueControls.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace BlueControls.ItemCollectionPad.FunktionsItems_Formular;

internal class RowAdderSingle : ParsebleItem, IErrorCheckable, IHasKeyName, IReadableText {

    #region Fields

    private string _additionalText = string.Empty;

    private Database? _database;

    /// <summary>
    /// Erkennung, welche ID die Zeile hat
    /// </summary>
    private string _rowID = string.Empty;

    private string tempDatabaseNametoLoad = string.Empty;

    #endregion

    #region Constructors

    public RowAdderSingle() : base(Generic.GetUniqueKey()) { }

    #endregion

    #region Properties

    [Description("Ein zusätzlicher Text, der erzeugt wird.")]
    public string AdditionalText {
        get => _additionalText;
        set {
            //if (IsDisposed) { return; }
            if (_additionalText == value) { return; }
            _additionalText = value;
            OnPropertyChanged();
        }
    }

    public Database? Database { get; set; }

    [Description("Eine Id, die mit Variablen der erzeugt wird.")]
    public string RowID {
        get => _rowID;
        set {
            //if (IsDisposed) { return; }
            if (_rowID == value) { return; }
            _rowID = value;
            OnPropertyChanged();
        }
    }

    #endregion

    #region Methods

    public Database? DatabaseGet() {
        if (string.IsNullOrEmpty(tempDatabaseNametoLoad)) { return _database; }
        _database = GetById(new ConnectionInfo(tempDatabaseNametoLoad, null, string.Empty), false, null, true);

        tempDatabaseNametoLoad = string.Empty;
        return _database;
    }

    public string ErrorReason() {
        if (Database == null || Database.IsDisposed) { return "Datenbank fehlt."; }

        if (string.IsNullOrEmpty(_rowID)) { return "Id-Generierung fehlt"; }
        if (!_rowID.Contains("~")) { return "ID-Generierung muss mit Variablen definiert werden."; }

        return string.Empty;
    }

    public List<GenericControl> GetStyleOptions() {
        var l = new List<GenericControl>();
        //new FlexiControl("Ausgang:", widthOfControl, true),
        l.Add(new FlexiControlForProperty<Database?>(() => Database, ItemSendFilter.AllAvailableTables()));

        if (Database != null && Database.IsDisposed) {
            l.Add(new FlexiControlForProperty<string>(() => RowID));
            l.Add(new FlexiControlForProperty<string>(() => AdditionalText));
        }

        return l;
    }

    public override bool ParseThis(string key, string value) {
        switch (key) {
            case "database":
                tempDatabaseNametoLoad = value.FromNonCritical();

                if (tempDatabaseNametoLoad.IsFormat(FormatHolder.FilepathAndName)) {
                    tempDatabaseNametoLoad = tempDatabaseNametoLoad.FilePath() + MakeValidTableName(tempDatabaseNametoLoad.FileNameWithoutSuffix()) + "." + tempDatabaseNametoLoad.FileSuffix();
                }

                return true;

            case "rowid":
                _rowID = value.FromNonCritical();
                return true;

            case "additionaltext":
                _additionalText = value.FromNonCritical();
                return true;
        }
        return false;
    }

    public string ReadableText() {
        var b = ErrorReason();
        if (!string.IsNullOrEmpty(b) || Database == null) { return b; }
        return Database.Caption;
    }

    public QuickImage? SymbolForReadableText() => null;

    public override string ToString() {
        //if (IsDisposed) { return string.Empty; }
        List<string> result = [];

        result.ParseableAdd("Database", DatabaseGet()); // Nicht _database, weil sie evtl. noch nicht geladen ist
        result.ParseableAdd("RowId", _rowID);
        result.ParseableAdd("AdditionalText", _additionalText);
        //result.ParseableAdd("Page", _page);
        //result.ParseableAdd("Print", _beiExportSichtbar);
        //result.ParseableAdd("QuickInfo", QuickInfo);
        //result.ParseableAdd("ZoomPadding", _zoomPadding);

        //foreach (var thisPoint in MovablePoint) {
        //    result.ParseableAdd("Point", thisPoint as IStringable);
        //}
        //if (!string.IsNullOrEmpty(Gruppenzugehörigkeit)) {
        //    result.ParseableAdd("RemoveTooGroup", Gruppenzugehörigkeit);
        //}

        //result.ParseableAdd("Tags", Tags, false);

        return result.Parseable(base.ToString());
    }

    #endregion
}