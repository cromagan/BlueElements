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
using BlueControls.ItemCollectionPad.FunktionsItems_Formular;
using BlueDatabase;
using System.Collections.Generic;

namespace BlueControls.Controls;

internal class AdderItemSingle {

    #region Constructors

    public AdderItemSingle(string generatedTextKey, RowItem thisRow, string count, bool realAdder, List<RowAdderSingleCell> columns) {
        GeneratedTextKey = generatedTextKey;
        //RowHash = thisRow.Hash();
        //RowKey = thisRow.KeyName;
        Count = count.ReduceToChars("0123456789.");
        Columns = columns;
        RealAdder = realAdder;
        Row = thisRow;
    }

    #endregion

    #region Properties

    public List<RowAdderSingleCell> Columns { get; private set; }
    public string Count { get; }
    public string GeneratedTextKey { get; }
    public bool RealAdder { get; set; }

    public RowItem Row { get; set; }

    #endregion
}