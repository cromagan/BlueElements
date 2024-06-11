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
using BlueControls.Controls;
using BlueControls.Interfaces;
using static BlueDatabase.Database;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using static BlueBasics.Converter;

using BlueDatabase;

namespace BlueControls.ItemCollectionPad.FunktionsItems_Formular;

public class RowAdderSingle : IParseable, IReadableTextWithKey, IErrorCheckable, IHasKeyName, IReadableText, IEditable, ISimpleEditor {

    #region Fields

    private static RowAdderPadItem? _parent = null;
    private string _additionalText = string.Empty;
    private int _count = 0;
    private Database? _database;

    /// <summary>
    /// Die Herkunft-Id, die mit Variablen der erzeugt wird.
    /// Diese Id muss für jede Zeile der eingehenden Datenbank einmalig sein.
    /// Die Struktur muss wie ein Dateipfad aufgebaut sein. z.B. Kochen\\Zutaten\\Vegetarisch\\Mehl
    /// </summary>
    private string _textKey = string.Empty;

    private string tempDatabaseNametoLoad = string.Empty;

    #endregion

    //public RowAdderSingle() : base(Generic.GetUniqueKey()) { }

    #region Constructors

    public RowAdderSingle(RowAdderPadItem parent, string toParse) : this(parent, -1) => this.Parse(toParse);

    public RowAdderSingle(RowAdderPadItem parent, int count) : base() {
        KeyName = Generic.GetUniqueKey();
        _parent = parent;

        if (count >= 0) {
            _count = count;
        }
    }

    #endregion

    #region Properties

    [Description("Ein zusätzlicher Text, der mit Variablen erzeugt wird.\r\nKann jede Art von Informationen enthalten")]
    public string AdditionalText {
        get => _additionalText;
        set {
            //if (IsDisposed) { return; }
            if (_additionalText == value) { return; }
            _additionalText = value;
            //OnPropertyChanged();
        }
    }

    public string CaptionForEditor => "Import Element";

    //[Description("Aus dieser Datenbank werden die Zeilen konvertiert und importiert.")]
    //public Database? Database { get; set; }

    public int Count => _count;

    public Database? Database {
        get {
            if (string.IsNullOrEmpty(tempDatabaseNametoLoad)) { return _database; }
            _database = GetById(new ConnectionInfo(tempDatabaseNametoLoad, null, string.Empty), false, null, true);

            tempDatabaseNametoLoad = string.Empty;
            return _database;
        }

        set {
            _database = value;
            tempDatabaseNametoLoad = string.Empty;
        }
    }

    public string Description => "Ein Element, das beschreibt, wie die Daten zusammengetragen werden.";

    public Type? Editor { get; set; }

    public string KeyName { get; private set; } = string.Empty;

    public string QuickInfo => ReadableText();

    /// <summary>
    /// Die Herkunft-Id, die mit Variablen der erzeugt wird.
    /// Diese Id muss für jede Zeile der eingehenden Datenbank einmalig sein.
    /// Die Struktur muss wie ein Dateipfad aufgebaut sein. z.B. Kochen\\Zutaten\\Vegetarisch\\Mehl
    /// </summary>
    [Description("Die Herkunft-Id, die mit Variablen der erzeugt wird.\r\nDiese Id muss für jede Zeile der eingehenden Datenbank einmalig sein.\r\nDie Struktur muss wie ein Dateipfad aufgebaut sein. z.B. Kochen\\Zutaten\\Vegetarisch\\Mehl")]
    public string TextKey {
        get => _textKey;
        set {
            //if (IsDisposed) { return; }
            if (_textKey == value) { return; }
            _textKey = value;
            //OnPropertyChanged();
        }
    }

    #endregion

    #region Methods

    public string ErrorReason() {
        if (Database is not Database db || db.IsDisposed) { return "Datenbank fehlt."; }

        if (string.IsNullOrEmpty(_textKey)) { return "TextKey-Id-Generierung fehlt"; }
        //if (!_textKey.Contains("~")) { return "TextKey-ID-Generierung muss mit Variablen definiert werden."; }

        return string.Empty;
    }

    public List<GenericControl> GetProperties(int widthOfControl) {
        var l = new List<GenericControl>();
        //new FlexiControl("Ausgang:", widthOfControl, true),
        l.Add(new FlexiControlForProperty<Database?>(() => Database, ItemSendFilter.AllAvailableTables()));

        if (Database != null && !Database.IsDisposed) {
            l.Add(new FlexiControlForProperty<string>(() => TextKey, 5));
            l.Add(new FlexiControlForProperty<string>(() => AdditionalText, 5));
        }

        return l;
    }

    public void ParseFinished(string parsed) { }

    public bool ParseThis(string key, string value) {
        switch (key) {
            case "database":
                tempDatabaseNametoLoad = value.FromNonCritical();

                if (tempDatabaseNametoLoad.IsFormat(FormatHolder.FilepathAndName)) {
                    tempDatabaseNametoLoad = tempDatabaseNametoLoad.FilePath() + MakeValidTableName(tempDatabaseNametoLoad.FileNameWithoutSuffix()) + "." + tempDatabaseNametoLoad.FileSuffix();
                }

                return true;

            case "textkey":
                _textKey = value.FromNonCritical();
                return true;

            case "additionaltext":
                _additionalText = value.FromNonCritical();
                return true;

            case "count":
                _count = IntParse(value);
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
        List<string> result = [];

        result.ParseableAdd("Database", Database); // Nicht _database, weil sie evtl. noch nicht geladen ist
        result.ParseableAdd("TextKey", _textKey);
        result.ParseableAdd("AdditionalText", _additionalText);
        result.ParseableAdd("Count", _count);

        return result.Parseable();
    }

    #endregion
}