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

using BlueControls.ItemCollectionPad.Abstract;
using BlueDatabase;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Forms;

namespace BlueControls.Interfaces;

public interface IMouseAndKeyHandle {

    #region Properties

    public ObservableCollection<AbstractPadItem> Items { get; }

    IMouseAndKeyHandle? Parent { get; set; }

    public RowItem? SheetStyle { get; set; }

    public float SheetStyleScale { get; set; }

    #endregion

    #region Methods

    List<AbstractPadItem> HotItems(MouseEventArgs e, float zoom, float shiftX, float shiftY);

    bool KeyUp(KeyEventArgs e, float zoom, float shiftX, float shiftY);

    bool MouseDown(MouseEventArgs e, float zoom, float shiftX, float shiftY);

    bool MouseMove(MouseEventArgs e, float zoom, float shiftX, float shiftY);

    bool MouseUp(MouseEventArgs e, float zoom, float shiftX, float shiftY);

    #endregion
}