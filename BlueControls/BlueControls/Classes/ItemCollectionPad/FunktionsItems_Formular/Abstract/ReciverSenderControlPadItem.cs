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
using System.Collections.ObjectModel;
using static BlueDatabase.Database;

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

    private bool _databaseOutputLoaded = false;

    private string _databaseOutputName = string.Empty;

    private int _outputColorId = -1;

    #endregion

    #region Constructors

    protected ReciverSenderControlPadItem(string keyName, ConnectedFormula.ConnectedFormula? parentFormula) : base(keyName, parentFormula) { }

    #endregion

    #region Properties

    public ReadOnlyCollection<string> ChildIds {
        get => new(_childIds);

        set {
            if (IsDisposed) { return; }

            if (!_childIds.IsDifferentTo(value)) { return; }

            _childIds.Clear();
            _childIds.AddRange(value);
            DoChilds();
            OnPropertyChanged();
        }
    }

    public Database? DatabaseOutput {
        get {
            if (DatabaseInputMustMatchOutputDatabase && !AllowedInputFilter.HasFlag(Enums.AllowedInputFilter.None)) {
                return DatabaseInput;
            }

            if (_databaseOutputLoaded) {
                return _databaseOutput;
            }

            _databaseOutput = GetById(new ConnectionInfo(_databaseOutputName, null, string.Empty), false, null, true);

            _databaseOutputLoaded = true;

            return _databaseOutput;
        }
        set {
            if (IsDisposed) {
                return;
            }

            if (DatabaseInputMustMatchOutputDatabase && !AllowedInputFilter.HasFlag(Enums.AllowedInputFilter.None)) {
                return;
            }

            if (value == DatabaseOutput) {
                return;
            }

            _databaseOutput = value;
            _databaseOutputName = value?.KeyName ?? string.Empty;
            _databaseOutputLoaded = true;
            DoChilds();
            OnPropertyChanged();
            UpdateSideOptionMenu();
        }
    }

    public int OutputColorId {
        get => _outputColorId;

        set {
            if (IsDisposed) { return; }

            if (_outputColorId == value) { return; }

            _outputColorId = value;
            OnPropertyChanged();
        }
    }

    #endregion

    #region Methods

    public void AddChild(IHasKeyName add) {
        var l = new List<string>();
        l.AddRange(ChildIds);
        l.Add(add.KeyName);
        l = l.SortedDistinctList();

        ChildIds = l.AsReadOnly();
    }

    public override void AddedToCollection() {
        if (IsDisposed) { return; }
        base.AddedToCollection();

        if (Parent != null) {
            OutputColorId = -1;
            OutputColorId = Parent.GetFreeColorId(Page);
        }
        DoChilds();
        OnPropertyChanged();
    }

    public void DoChilds() {
        if (Parent == null) { return; }

        foreach (var thisChild in ChildIds) {
            var item2 = Parent[thisChild];

            if (item2 is ReciverControlPadItem ias) {
                ias.CalculateInputColorIds();
            }
        }
    }

    public override string ErrorReason() {
        if (DatabaseOutput is not Database db || db.IsDisposed) {
            return "Ausgehende Datenbank nicht angegeben.";
        }

        return base.ErrorReason();
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
                value = value.Replace("\r", "|");

                var tmp = value.FromNonCritical().SplitBy("|");
                _childIds.Clear();
                foreach (var thiss in tmp) {
                    _childIds.Add(thiss.FromNonCritical());
                }
                return true;
        }
        return base.ParseThis(key, value);
    }

    public void RemoveChild(ReciverControlPadItem remove) {
        var l = new List<string>();
        l.AddRange(_childIds);
        _ = l.Remove(remove.KeyName);

        ChildIds = l.AsReadOnly();
        remove.CalculateInputColorIds();
    }

    public override string ToParseableString() {
        if (IsDisposed) { return string.Empty; }

        List<string> result = [];

        result.ParseableAdd("OutputDatabase", _databaseOutputName);
        result.ParseableAdd("SentToChildIds", _childIds, false);

        return result.Parseable(base.ToParseableString());
    }

    #endregion
}