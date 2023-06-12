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

using BlueControls.Controls;
using BlueControls.ItemCollection;
using System;
using System.Collections.Generic;

namespace BlueControls.Interfaces;

/// <summary>
/// Wird verwendet, einen einzelnen Filter berechnen kann
/// </summary>
public interface IItemSendRow : IItemSendSomething {
}

public class ItemSendRow : ItemSendSomething {

    #region Methods

    public override List<string> ParsableTags() {
        var result = base.ParsableTags();

        //result.ParseableAdd("GetValueFromKey", _getValueFromkey ?? string.Empty);

        return result;
    }

    public override bool ParseThis(string tag, string value) {
        if (base.ParseThis(tag, value)) { return true; }

        switch (tag) {
            //case "getvaluefromkey":
            //    _getValueFromkey = value.FromNonCritical();
            //    return true;
        }
        return false;
    }

    internal string ErrorReason(IItemSendRow item) {
        var d = item.OutputDatabase;
        if (d == null || d.IsDisposed) {
            return "Ausgehende Datenbank nicht angegeben.";
        }

        return string.Empty;
    }

    internal List<GenericControl> GetStyleOptions(IItemSendRow item, int widthOfControl) {
        var l = new List<GenericControl>();
        l.AddRange(base.GetStyleOptions(item, widthOfControl));
        return l;
    }

    #endregion
}