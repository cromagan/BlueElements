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
using static BlueBasics.Converter;
using BlueDatabase.Enums;

namespace BlueControls.ItemCollectionPad.FunktionsItems_Formular;

public class RowAdderSingleCell : IParseable, IErrorCheckable, IHasKeyName {

    #region Constructors

    public RowAdderSingleCell(RowAdderSingleRow parent, string toParse) : this(parent) => this.Parse(toParse);

    public RowAdderSingleCell(RowAdderSingleRow parent) {
        Parent = parent;
    }

    #endregion

    #region Properties

    public string Column { get; internal set; } = string.Empty;
    public string KeyName => Column;

    public RowAdderSingleRow Parent { get; }

    public string ReplaceableText { get; internal set; } = string.Empty;

    public List<string> DropdownItems { get; internal set; } = new();



    #endregion

    #region Methods

    public string ErrorReason() => string.Empty;

    public void ParseFinished(string parsed) { }

    public bool ParseThis(string key, string value) {
        switch (key) {
            case "column":
                Column = value.FromNonCritical();
                return true;

            case "text":
                ReplaceableText = value.FromNonCritical();
                return true;

            case "dropdownitems":
                DropdownItems = value.FromNonCritical().SplitAndCutByCrToList();
                return true;


        }
        return false;
    }

    public new string ToString() {
        //if (!ToStringPossible) { return string.Empty; }

        List<string> result = [];
        result.ParseableAdd("Column", Column);
        result.ParseableAdd("Text", ReplaceableText);
        result.ParseableAdd("DropdownItems", DropdownItems.JoinWithCr());
        return result.Parseable();
    }

    internal string ReplacedText(RowItem thisRow, RowItem uniqueRow) {
        var nt = thisRow.ReplaceVariables(ReplaceableText, false, true, null);
        nt = uniqueRow.ReplaceVariables(nt, false, true, null);
        return nt;
    }

    #endregion
}