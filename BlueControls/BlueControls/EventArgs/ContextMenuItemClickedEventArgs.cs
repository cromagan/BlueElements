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

using System.Collections.Generic;

namespace BlueControls.EventArgs;

public class ContextMenuItemClickedEventArgs : System.EventArgs {

    #region Constructors

    public ContextMenuItemClickedEventArgs(string clickedComand, object hotItem, List<string>? tags) {
        HotItem = hotItem;
        Tags = tags ?? new List<string>();
        ClickedComand = clickedComand;
    }

    #endregion

    #region Properties

    public string ClickedComand { get; }
    public object HotItem { get; }
    public List<string> Tags { get; }

    #endregion
}