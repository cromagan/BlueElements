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
using System;
using System.Collections.Generic;
using System.ComponentModel;
using BlueDatabase;
using BlueControls.Editoren;
using System.Data.Common;

namespace BlueControls.ItemCollectionPad.FunktionsItems_Formular;

public class RowAdderSingleRow : IParseable, IReadableTextWithKey, IErrorCheckable, IHasKeyName, IReadableText, IEditable, ISimpleEditor {

    #region Fields

    private string _additionalText = string.Empty;

    /// <summary>
    /// Die Herkunft-Id, die mit Variablen der erzeugt wird.
    /// Diese Id muss für jede Zeile der eingehenden Datenbank einmalig sein.
    /// Die Struktur muss wie ein Dateipfad aufgebaut sein. z.B. Kochen\\Zutaten\\Vegetarisch\\Mehl
    /// </summary>
    private string _textKey = string.Empty;

    #endregion

    #region Constructors

    public RowAdderSingleRow(RowAdderSingle parent, string toParse) : this(parent) => this.Parse(toParse);

    public RowAdderSingleRow(RowAdderSingle parent) : base() {
        KeyName = Generic.GetUniqueKey();
        Parent = parent;
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

    public string CaptionForEditor => "Import Element One Row";

    public Database? Database {
        get {
            return Parent?.Database;
        }
    }

    public string Description => "Ein Element, das beschreibt, wie die Daten zusammengetragen werden.";
    public Type? Editor { get; set; }
    public string KeyName { get; private set; } = string.Empty;
    public RowAdderSingle? Parent { get; private set; } = null;
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
        var result = new List<GenericControl>();
        //new FlexiControl("Ausgang:", widthOfControl, true),
        result.Add(new FlexiControlForProperty<Database?>(() => Database, ItemSendFilter.AllAvailableTables(), widthOfControl));

        if (Database != null && !Database.IsDisposed) {
            if (Parent?.Parent?.AdditinalTextColumn is ColumnItem Column) {
                result.Add(new FlexiControlForProperty<string>(() => Column.AdminInfo, 5, widthOfControl));
            }

            result.Add(new FlexiControlForProperty<string>(() => TextKey, 5, widthOfControl));
            result.Add(new FlexiControlForProperty<string>(() => AdditionalText, 5, widthOfControl));
        }

        return result;
    }

    public void ParseFinished(string parsed) { }

    public bool ParseThis(string key, string value) {
        switch (key) {
            case "textkey":
                _textKey = value.FromNonCritical();
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
        return _textKey;
    }

    public QuickImage? SymbolForReadableText() => null;

    public override string ToString() {
        List<string> result = [];

        result.ParseableAdd("TextKey", _textKey);
        result.ParseableAdd("AdditionalText", _additionalText);

        return result.Parseable();
    }

    #endregion
}