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

using System.Collections.Generic;
using System.Drawing;
using BlueBasics;
using BlueBasics.Interfaces;
using BlueControls.Enums;
using BlueDatabase;
using BlueDatabase.Enums;

namespace BlueControls.CellRenderer;

public abstract class AbstractCellRenderer : IReadableTextWithKey {

    #region Fields

    private static List<AbstractCellRenderer>? _allrenderer;

    #endregion

    #region Properties

    public static List<AbstractCellRenderer> AllRenderer {
        get {
            _allrenderer ??= Generic.GetInstaceOfType<AbstractCellRenderer>();
            return _allrenderer;
        }
    }

    public abstract string KeyName { get; }

    public abstract string QuickInfo { get; }

    #endregion

    #region Methods

    public abstract void Draw(Graphics gr, string content, Rectangle drawarea,
        Design design, States state,
        ColumnItem? column,
        ShortenStyle style, float scale);

    public abstract string ReadableText();

    public abstract QuickImage? SymbolForReadableText();

    #endregion
}