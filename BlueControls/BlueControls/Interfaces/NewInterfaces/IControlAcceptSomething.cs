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

using BlueBasics;
using BlueBasics.Interfaces;
using BlueControls.Controls;
using BlueDatabase;
using System.Collections.Generic;

namespace BlueControls.Interfaces;

public interface IControlAcceptSomething : IDisposableExtendedWithEvent {

    #region Properties

    /// <summary>
    /// Ein Wert, der bei ParentDataChanged berechnet werden sollte.
    /// Enthält die DatabaseInput und auch den berechnete Zeile.
    /// </summary>
    public FilterCollection? FilterInput { get; set; }

    /// <summary>
    /// Wenn TRUE, sollte der Input Filter nicht mehr von den Parents verändert werden.
    /// Wird z.B. durch SetToRow gesetzt.
    /// </summary>
    public bool FilterManualSeted { get; set; }

    public string Name { get; set; }

    public List<IControlSendSomething> Parents { get; }

    #endregion

    #region Methods

    /// <summary>
    /// Wird ausgelöst, wenn eine relevante Änderung an den Daten erfolgt ist.
    /// Hier können die neuen temporären Daten berechnet werden und sollten auch angezeigt werden und ein Invalidate gesetzt werden
    /// Events können gekoppelt werden
    /// </summary>
    public void FilterInput_Changed(object sender, System.EventArgs e);

    /// <summary>
    /// Wird ausgelöst, bevor eine relevante Änderung an den Daten erfolgt.
    /// Hier können Daten, die angezeigt werden, zurückgeschrieben werden. Events entkoppelt werden
    /// </summary>
    public void FilterInput_Changing(object sender, System.EventArgs e);

    #endregion
}

public static class IControlAcceptSomethingExtension {

    #region Methods

    public static void ConnectChildParents(this IControlAcceptSomething child, List<IControlSendSomething> parents) {
        foreach (var thisParent in parents) {
            child.ConnectChildParents(thisParent);
        }
    }

    public static void ConnectChildParents(this IControlAcceptSomething child, IControlSendSomething parent) {
        if (child.FilterManualSeted) {
            Develop.DebugPrint(BlueBasics.Enums.FehlerArt.Fehler, "Manuelle Filterung kann keine Parents empfangen.");
        }

        child.Parents.AddIfNotExists(parent);

        if (parent.Childs.AddIfNotExists(child)) {
            parent.FilterOutput.Changing += child.FilterInput_Changing;
            parent.FilterOutput.Changed += child.FilterInput_Changed;
            parent.FilterOutput.DisposingEvent += FilterOutput_DispodingEvent;
            //child.DisposingEvent += Child_DisposingEvent;
            //parent.DisposingEvent += Parent_DisposingEvent;
        }
    }

    public static void DisconnectChildParents(this IControlAcceptSomething child, List<IControlSendSomething> parents) {
        var p = new List<IControlSendSomething>();
        p.AddRange(parents);

        foreach (var parent in p) {
            child.DisconnectChildParents(parent);
        }
    }

    public static void DisconnectChildParents(this IControlAcceptSomething child, IControlSendSomething parent) {
        child.Parents.Remove(parent);

        if (parent.Childs.Contains(child)) {
            parent.Childs.Remove(child);

            parent.FilterOutput.Changing -= child.FilterInput_Changing;
            parent.FilterOutput.Changed -= child.FilterInput_Changed;
            parent.FilterOutput.DisposingEvent -= FilterOutput_DispodingEvent;
        }
    }

    /// <summary>
    /// Nachdem das Control erzeugt wurde, werden hiermit die Einstellungen vom IItemAcceptSomething übernommen.
    /// </summary>
    /// <param name="dest"></param>
    /// <param name="parent"></param>
    /// <param name="source"></param>
    public static void DoInputSettings(this IControlAcceptSomething dest, ConnectedFormulaView parent, IItemAcceptSomething source) {
        dest.Name = source.DefaultItemToControlName();

        foreach (var thisKey in source.Parents) {
            var it = source.Parent?[thisKey];

            if (it is IItemToControl itc) {
                var ff = parent.SearchOrGenerate(itc);

                if (ff is IControlSendSomething ffx) {
                    dest.ConnectChildParents(ffx);
                }
            }
        }
    }

    public static FilterCollection? FilterOfSender(this IControlAcceptSomething item) {
        if (item.FilterManualSeted) { return item.FilterInput; }
        if (item.Parents.Count == 0) { return null; }
        if (item.Parents.Count == 1) {
            if (item.Parents[0].FilterOutput.Clone() is FilterCollection fc2) { return fc2; }
        }

        FilterCollection? fc = null;

        foreach (var thiss in item.Parents) {
            if (!thiss.IsDisposed && thiss.FilterOutput is FilterCollection fi) {
                if (fc == null) { fc = new FilterCollection(fi.Database); }
                fc.AddIfNotExists(fi);
            }
        }

        return fc;
    }

    public static void SetToRow(this IControlAcceptSomething item, RowItem? row) {
        if (item.Parents.Count > 0) {
            Develop.DebugPrint(BlueBasics.Enums.FehlerArt.Fehler, "Element wird von Parents gesteuert!");
        }

        item.FilterManualSeted = true;

        if (row?.Database == null && item.FilterInput == null) { return; }

        if (row?.Database is DatabaseAbstract db && !db.IsDisposed) {
            item.FilterInput ??= new FilterCollection(db);
            item.FilterInput.Database = db;
        }

        if (item.FilterInput != null) {
            item.FilterInput.Clear();
            if (row == null) {
                item.FilterInput.Add(new FilterItem());
            } else {
                item.FilterInput.Add(new FilterItem(row));
            }
        }
        item.FilterInput_Changed(item, System.EventArgs.Empty);
    }

    private static void FilterOutput_DispodingEvent(object sender, System.EventArgs e) {
        if (sender is IControlSendSomething parent) {
            foreach (var child in parent.Childs) {
                child.FilterInput_Changing(parent, System.EventArgs.Empty);
                child.DisconnectChildParents(parent);
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