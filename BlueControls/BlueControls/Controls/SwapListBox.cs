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

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Drawing;
using BlueBasics;
using BlueControls.Enums;
using BlueControls.EventArgs;
using BlueControls.Interfaces;
using BlueControls.ItemCollectionList;

namespace BlueControls.Controls;

public partial class SwapListBox : GenericControl, IBackgroundNone {

    #region Constructors

    public SwapListBox() => InitializeComponent();

    #endregion

    #region Events

    public event EventHandler<NotifyCollectionChangedEventArgs>? CollectionChanged;

    #endregion

    #region Properties

    public AddType AddAllowed {
        get => Main.AddAllowed;
        set => Main.AddAllowed = value;
    }

    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public ItemCollectionList.ItemCollectionList Item => Main.Item;

    #endregion

    #region Methods

    public void OnCollectionChanged(NotifyCollectionChangedEventArgs e) => CollectionChanged?.Invoke(this, e);

    internal void Check(List<string> list) => Main.Check(list);

    internal void SuggestionsAdd(ItemCollectionList.ItemCollectionList? item) {
        if (item == null) { return; }

        foreach (var thisi in item) {
            if (Main.Item[thisi.KeyName] == null && Suggest.Item[thisi.KeyName] == null) {
                Suggest.Item.Add(thisi.Clone() as AbstractListItem);
            }
        }
    }

    protected override void DrawControl(Graphics gr, States state) { }

    protected void MoveItemBetweenList(ListBox source, ListBox target, string @internal, bool doRemove) {
        var sourceItem = source.Item[@internal];
        var targetItem = target.Item[@internal];
        if (sourceItem != null && targetItem == null) {
            target.Item.Add(sourceItem.Clone() as AbstractListItem);
        } else if (sourceItem == null && targetItem == null) {
            targetItem = new TextListItem(@internal, @internal, null, false, true, string.Empty);
            target.Item.Add(targetItem);
        }

        if (sourceItem != null && doRemove) { source.Item.Remove(sourceItem); }
    }

    protected override void OnEnabledChanged(System.EventArgs e) {
        base.OnEnabledChanged(e);
        Main.Enabled = Enabled;
        Suggest.Enabled = Enabled;
    }

    private void Main_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
        foreach (var thisn in Main.Item) {
            Suggest.Item.Remove(thisn.KeyName);
        }

        if (e.OldItems != null) {
            foreach (var thisit in e.OldItems) {
                if (thisit is AbstractListItem bli) {
                    if (Suggest.Item[bli.KeyName] == null) {
                        Suggest.Item.Add(bli.Clone() as AbstractListItem);
                    }
                }
            }
        }

        if (e.Action == NotifyCollectionChangedAction.Reset) {
            Develop.DebugPrint_NichtImplementiert();
        }

        OnCollectionChanged(e);
    }

    private void Main_ItemClicked(object sender, AbstractListItemEventArgs e) => MoveItemBetweenList(Main, Suggest, e.Item.KeyName, true);

    private void Suggest_ItemClicked(object sender, AbstractListItemEventArgs e) => MoveItemBetweenList(Suggest, Main, e.Item.KeyName, true);

    #endregion
}