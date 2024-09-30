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
using BlueBasics.Interfaces;
using BlueControls.Interfaces;
using BlueDatabase;
using System.Collections.Generic;
using BlueControls.BlueDatabaseDialogs;
using BlueControls.Controls;
using BlueControls.ItemCollectionList;
using BlueBasics.Enums;
using BlueControls.Enums;
using BlueControls.Forms;
using static BlueDatabase.Database;
using static BlueControls.ItemCollectionList.AbstractListItemExtension;

#nullable enable

namespace BlueControls.ItemCollectionPad.FunktionsItems_Formular.Abstract;

/// <summary>
/// Standard für Objekte, die einen Datenbank/Zeilenbezug haben.
/// Stellt auch alle Methode breit, zum Einrichten der Breite und Benutzer-Sichtbarkeiten.
/// Nur Tabs, die ein solches Objekt haben, werden als anzeigewürdig gewertet.
/// </summary>

public abstract class ReciverSenderControlPadItem : ReciverControlPadItem, IHasVersion {

    #region Fields

    private readonly List<string> _childIds = [];

    private Database? _databaseOutput;

    private bool _databaseOutputLoaded;

    private string _databaseOutputName = string.Empty;

    private int _outputColorId = -1;

    #endregion

    #region Constructors

    protected ReciverSenderControlPadItem(string keyName, ConnectedFormula.ConnectedFormula? parentFormula, Database? databaseOutput) : base(keyName, parentFormula) => DatabaseOutput = databaseOutput;

    #endregion

    #region Properties

    public Database? DatabaseOutput {
        get {
            if (DatabaseInputMustMatchOutputDatabase && DatabaseInput is { IsDisposed: false }) { return DatabaseInput; }

            if (_databaseOutputLoaded) { return _databaseOutput; }

            _databaseOutput = GetById(new ConnectionInfo(_databaseOutputName, null, string.Empty), false, null, true);

            _databaseOutputLoaded = true;

            return _databaseOutput;
        }
        // ReSharper disable once MemberCanBePrivate.Global -> FlexiCOntrolForProperty!
        set {
            if (IsDisposed) { return; }

            if (DatabaseInputMustMatchOutputDatabase && !AllowedInputFilter.HasFlag(Enums.AllowedInputFilter.None)) { return; }

            if (value == DatabaseOutput) { return; }

            _databaseOutput = value;
            _databaseOutputName = value?.KeyName ?? string.Empty;
            _databaseOutputLoaded = true;
            OnPropertyChanged();
            OnDoUpdateSideOptionMenu();
        }
    }

    public int OutputColorId {
        get => _outputColorId;

        private set {
            if (IsDisposed) { return; }

            if (_outputColorId == value) { return; }

            _outputColorId = value;
            OnPropertyChanged();
        }
    }

    #endregion

    #region Methods

    public static List<AbstractListItem> AllAvailableTables() {
        var ld = Database.AllAvailableTables(string.Empty);
        var ld2 = new List<AbstractListItem>();
        foreach (var thisd in ld) {
            thisd.Editor = typeof(DatabaseHeadEditor);
            ld2.Add(ItemOf(thisd));
        }
        return ld2;
    }

    //public void AddChild(IHasKeyName add) {
    //    var l = new List<string>();
    //    l.AddRange(ChildIds);
    //    l.Add(add.KeyName);
    //    l = l.SortedDistinctList();

    //    ChildIds = l.AsReadOnly();
    //}

    public override void AddedToCollection() {
        if (IsDisposed) { return; }
        base.AddedToCollection();

        if (Parent != null) {
            OutputColorId = -1;
            OutputColorId = Parent.GetFreeColorId();
        }
        OnPropertyChanged();
    }

    public override string ErrorReason() {
        if (DatabaseOutput is not { IsDisposed: false }) {
            if (DatabaseInputMustMatchOutputDatabase) {
                return "Eingehendes Objekt nicht gewählt.";
            }

            return "Ausgehende Datenbank nicht angegeben.";
        }

        return base.ErrorReason();
    }

    public override List<GenericControl> GetProperties(int widthOfControl) {
        List<GenericControl> result = [.. base.GetProperties(widthOfControl),
                                new FlexiControl("Ausgang:", widthOfControl, true)
        ];

        var enableOutput = true;
        Database? outp = null;

        if (DatabaseInputMustMatchOutputDatabase) {
            enableOutput = AllowedInputFilter.HasFlag(Enums.AllowedInputFilter.None);

            outp = DatabaseInput;
            if (outp is not null) { enableOutput = false; }
        }
        if (!enableOutput) {
            if (outp != null) {
                outp.Editor = typeof(DatabaseHeadEditor);
                result.Add(new FlexiControlForDelegate(outp.Edit, "Tabelle: " + outp.Caption, ImageCode.Datenbank));
            } else {
                result.Add(new FlexiControl("<ImageCode=Information|16> Ausgangsdatenbank wird über den Eingang gewählt.", widthOfControl, false));
            }
        } else {
            result.Add(new FlexiControlForProperty<Database?>(() => DatabaseOutput, AllAvailableTables()));
        }

        return result;
    }

    public override List<string> ParseableItems() {
        if (IsDisposed) { return []; }
        List<string> result = [.. base.ParseableItems()];

        if (DatabaseInputMustMatchOutputDatabase && DatabaseInput is { IsDisposed: false } db) {
            result.ParseableAdd("OutputDatabase", db.KeyName);
        } else {
            result.ParseableAdd("OutputDatabase", _databaseOutputName);
        }

        //result.ParseableAdd("SentToChildIds", _childIds, false);

        return result;
    }

    public override bool ParseThis(string key, string value) {
        switch (key) {
            case "database":
            case "outputdatabase":
                _databaseOutputName = value.FromNonCritical();
                _databaseOutputLoaded = false;

                if (_databaseOutputName.IsFormat(FormatHolder.FilepathAndName)) {
                    _databaseOutputName = _databaseOutputName.FilePath() + MakeValidTableName(_databaseOutputName.FileNameWithoutSuffix()) + "." + _databaseOutputName.FileSuffix();
                }

                return true;

            case "senttochildids":
                //value = value.Replace("\r", "|");

                //var tmp = value.FromNonCritical().SplitBy("|");
                //_childIds.Clear();
                //foreach (var thiss in tmp) {
                //    _childIds.Add(thiss.FromNonCritical());
                //}
                return true;
        }
        return base.ParseThis(key, value);
    }

    #endregion
}