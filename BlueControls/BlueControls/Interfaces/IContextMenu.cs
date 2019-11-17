#region BlueElements - a collection of useful tools, database and controls
// Authors: 
// Christian Peter
// 
// Copyright (c) 2019 Christian Peter
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
#endregion


using System;
using System.Collections.Generic;
using BlueControls.EventArgs;
using BlueControls.ItemCollection;

namespace BlueControls.Interfaces
{
    public interface IContextMenu
    {
        void GetContextMenuItems(System.Windows.Forms.MouseEventArgs e, ItemCollectionList Items, out object HotItem, List<string> Tags, ref bool Cancel, ref bool Translate);


        bool ContextMenuItemClickedInternalProcessig(object sender, ContextMenuItemClickedEventArgs e);


        void OnContextMenuInit(ContextMenuInitEventArgs e);

        void OnContextMenuItemClicked(ContextMenuItemClickedEventArgs e);

        event EventHandler<ContextMenuInitEventArgs> ContextMenuInit;
        event EventHandler<ContextMenuItemClickedEventArgs> ContextMenuItemClicked;
    }
}