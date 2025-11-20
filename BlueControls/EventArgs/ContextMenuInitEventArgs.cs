// Authors:
// Christian Peter
//
// Copyright (c) 2025 Christian Peter
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

using BlueControls.ItemCollectionList;
using System.Collections.Generic;
using System.Windows.Forms;

namespace BlueControls.EventArgs;

public class ContextMenuInitEventArgs : System.EventArgs {

    #region Constructors

    public ContextMenuInitEventArgs(object? hotItem, List<AbstractListItem> contextMenu, MouseEventArgs? mouse) {
        ContextMenu = contextMenu;
        HotItem = hotItem;
        Translate = true;
        Mouse = mouse;
    }

    #endregion

    #region Properties

    public List<AbstractListItem> ContextMenu { get; }
    public object? HotItem { get; }
    public MouseEventArgs? Mouse { get; }
    public bool Translate { get; set; }

    #endregion
}