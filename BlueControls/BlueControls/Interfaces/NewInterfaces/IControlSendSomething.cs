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

using BlueBasics.Interfaces;
using BlueControls.Controls;
using BlueDatabase;
using System.Collections.Generic;

namespace BlueControls.Interfaces;

public interface IControlSendSomething : IDisposableExtendedWithEvent {

    #region Properties

    public List<IControlAcceptSomething> Childs { get; }

    /// <summary>
    /// Sollte von DoOuputSettings befüllt werden.
    /// </summary>
    public FilterCollection FilterOutput { get; }

    public string Name { get; set; }

    #endregion
}

public static class IControlSendSomethingExtension {

    #region Methods

    public static void ConnectChildParents(this IControlSendSomething parent, IControlAcceptSomething child) {
        parent.Childs.Add(child);
        child.Parents.Add(parent);

        parent.FilterOutput.Changing += child.FilterInput_Changing;
        parent.FilterOutput.Changed += child.FilterInput_Changed;
        parent.FilterOutput.DisposingEvent += FilterOutput_DispodingEvent;
        //child.DisposingEvent += Child_DisposingEvent;
        //parent.DisposingEvent += Parent_DisposingEvent;
    }

    public static void DisconnectChildParents(this IControlSendSomething parent, IControlAcceptSomething child) {
        parent.Childs.Remove(child);
        child.Parents.Remove(parent);

        parent.FilterOutput.Changing -= child.FilterInput_Changing;
        parent.FilterOutput.Changed -= child.FilterInput_Changed;
        parent.FilterOutput.DisposingEvent -= FilterOutput_DispodingEvent;
    }

    public static void DoOutputSettings(this IControlSendSomething dest, ConnectedFormulaView parent, IItemSendSomething source) {
        dest.Name = source.DefaultItemToControlName();
        dest.FilterOutput.Database = source.DatabaseOutput;
    }

    private static void FilterOutput_DispodingEvent(object sender, System.EventArgs e) {
        if (sender is IControlSendSomething parent) {
            foreach (var child in parent.Childs) {
                child.FilterInput_Changing(parent, System.EventArgs.Empty);
                parent.DisconnectChildParents(child);
                //parent.FilterOutput.Changing -= child.FilterInput_Changing;
                //parent.FilterOutput.Changed -= child.FilterInput_Changed;
                //parent.FilterOutput.DisposingEvent -= FilterOutput_DispodingEvent;
                //child.DisposingEvent += Child_DisposingEvent;
                //item.DisposingEvent += Parent_DisposingEvent;
            }
        }
    }

    #endregion
}