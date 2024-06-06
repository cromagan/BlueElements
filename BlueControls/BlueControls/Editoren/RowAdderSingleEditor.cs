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
using BlueBasics.Interfaces;
using BlueControls.Controls;
using BlueControls.ItemCollectionPad.FunktionsItems_Formular;
using BlueDatabase;
using BlueDatabase.Enums;
using BlueDatabase.Interfaces;
using System.Collections.Generic;
using System.Windows.Controls;
using static BlueControls.ItemCollectionList.AbstractListItemExtension;

namespace BlueControls.Editoren;

public partial class RowAdderSingleEditor : EditorEasy, IHasDatabase {

    #region Fields

    private RowAdderPadItem? _parent;

    #endregion

    #region Constructors

    public RowAdderSingleEditor() {
        InitializeComponent();
    }

    #endregion

    #region Properties

    public Database? Database {
        get {
            if (_parent is null || _parent.IsDisposed) { return null; }
            return _parent.DatabaseInput;
        }
    }

    #endregion
}