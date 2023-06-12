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

using BlueBasics.Interfaces;
using BlueControls.Controls;
using BlueControls.ItemCollection;
using BlueDatabase;
using System.Collections.Generic;

namespace BlueControls.Interfaces;

/// <summary>
/// Wird verwendet, wenn das Steuerelement etwas empfangen kann
/// </summary>
public interface IItemAcceptSomething : IHasKeyName, IChangedFeedback, IHasVersion, IItemToControl {

    #region Properties

    List<int> InputColorId { get; }

    public DatabaseAbstract? InputDatabase { get; }

    /// <summary>
    /// Welcher Datenbank die eingehenden Filter entsprechen müssen.
    /// Wenn NULL zurück gegeben wird, ist freie Datenbankwahl
    /// </summary>
    public DatabaseAbstract? InputDatabaseMustBe { get; }

    public string Page { get; }

    public ItemCollectionPad? Parent { get; }

    /// <summary>
    /// Wenn InputDatabaseMustBe null zurück gibt, ob schon Filter gewählt werden dürfen.
    /// Typischerweise false, wenn auf die Output-Database gewartet werden soll.
    /// </summary>
    public bool WaitForDatabase { get; }

    #endregion

    #region Methods

    public void CalculateInputColorIds();

    public void UpdateSideOptionMenu();

    #endregion
}

public abstract class ItemAcceptSomething {
    //public void InputColorIdSet(IItemAcceptSomething item, List<int> value) {
    //    if (!_inputColorId.IsDifferentTo(value)) { return; }

    //    _inputColorId = value;
    //    item.OnChanged();
    //}

    #region Methods

    public virtual List<string> ParsableTags() {
        var result = new List<string>();

        //result.ParseableAdd("GetValueFromKey", _getValueFromkey ?? string.Empty);

        return result;
    }

    public virtual bool ParseThis(string tag, string value) {
        switch (tag) {
            //case "getvaluefromkey":
            //    _getValueFromkey = value.FromNonCritical();
            //    return true;
        }
        return false;
    }

    protected List<GenericControl> GetStyleOptions(IItemAcceptSomething item, int widthOfControl) {
        var l = new List<GenericControl>();

        l.Add(new FlexiControl("Eingang:", widthOfControl));

        //l.AddRange(base.GetStyleOptions(this, widthOfControl));
        return l;
    }

    #endregion
}