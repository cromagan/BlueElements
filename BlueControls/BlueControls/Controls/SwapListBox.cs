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
using BlueControls.Enums;
using BlueControls.EventArgs;
using BlueControls.Interfaces;
using BlueControls.ItemCollectionList;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace BlueControls.Controls;

public partial class SwapListBox : GenericControl, IBackgroundNone {

    #region Constructors

    public SwapListBox() => InitializeComponent();

    #endregion

    #region Events

    public event EventHandler? ItemCheckedChanged;

    #endregion

    #region Properties

    public AddType AddAllowed {
        get => Main.AddAllowed;
        set => Main.AddAllowed = value;
    }

    public IReadOnlyCollection<string> Checked => Main.Checked;

    #endregion

    #region Methods

    public void OnItemCheckedChanged() => ItemCheckedChanged?.Invoke(this, System.EventArgs.Empty);

    internal void Check(List<string> toCheck) {
        foreach (var thisCheck in toCheck) {
            MoveItemBetweenList(Suggest, Main, thisCheck, true);
        }

        List<string> l = [..Main.Checked];

        foreach (var thisl in l) {
            if (!toCheck.Contains(thisl, false)) {
                MoveItemBetweenList(Main, Suggest, thisl, true);
            }
        }
    }

    internal void SuggestionsAdd(List<AbstractListItem>? item) {
        if (item == null) { return; }

        foreach (var thisi in item) {
            if (Main[thisi.KeyName] == null && Suggest[thisi.KeyName] == null) {
                Suggest.AddAndCheck(thisi);
            }
        }
    }

    internal void SuggestionsClear() => Suggest.UncheckAll();

    internal void UnCheck() => UnCheck(Main.Checked);

    internal void UnCheck(IEnumerable<string> list) {
        foreach (var thisIt in list) {
            MoveItemBetweenList(Main, Suggest, thisIt, true);
        }
    }

    protected override void DrawControl(Graphics gr, States state) { }

    protected void MoveItemBetweenList(ListBox source, ListBox target, string @internal, bool doRemove) {
        var sourceItem = source[@internal];
        var targetItem = target[@internal];

        var did = false;
        if (sourceItem != null && targetItem == null) {
            target.AddAndCheck(sourceItem);
            did = true;
        } else if (sourceItem == null && targetItem == null) {
            targetItem = new TextListItem(@internal, @internal, null, false, true, string.Empty);
            target.AddAndCheck(targetItem);
            did = true;
        }

        if (sourceItem != null && doRemove) { source.UnCheck(sourceItem); }

        if (did) { OnItemCheckedChanged(); }
    }

    protected override void OnEnabledChanged(System.EventArgs e) {
        base.OnEnabledChanged(e);
        Main.Enabled = Enabled;
        Suggest.Enabled = Enabled;
        btnFilterDel.Enabled = Enabled;
        txbFilter.Enabled = Enabled;
    }

    private void btnFilterDel_Click(object sender, System.EventArgs e) => txbFilter.Text = string.Empty;

    //private void Main_ItemCheckedChanged(object sender, System.EventArgs e) {
    //    foreach (var thisn in Main.Item) {
    //        Suggest.UnCheck(thisn.KeyName);
    //    }

    //    //if (e.OldItems != null) {
    //    //    foreach (var thisit in e.OldItems) {
    //    //        if (thisit is AbstractListItem bli) {
    //    //            if (Suggest.Item[bli.KeyName] == null) {
    //    //                Suggest.ItemAdd(bli.Clone() as AbstractListItem);
    //    //            }
    //    //        }
    //    //    }
    //    //}

    //    //if (e.Action == NotifyCollectionChangedAction.Reset) {
    //    //    Develop.DebugPrint_NichtImplementiert();
    //    //}

    //    OnItemCheckedChanged();
    //}

    private void Main_ItemClicked(object sender, AbstractListItemEventArgs e) => MoveItemBetweenList(Main, Suggest, e.Item.KeyName, true);

    private void Suggest_ItemClicked(object sender, AbstractListItemEventArgs e) => MoveItemBetweenList(Suggest, Main, e.Item.KeyName, true);

    private void txbFilter_TextChanged(object sender, System.EventArgs e) {
        Main.FilterText = txbFilter.Text;
        Suggest.FilterText = txbFilter.Text;
        btnFilterDel.Enabled = Enabled && !string.IsNullOrEmpty(txbFilter.Text);
    }

    #endregion
}