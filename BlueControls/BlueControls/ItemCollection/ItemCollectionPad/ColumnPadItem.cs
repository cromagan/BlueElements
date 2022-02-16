// Authors:
// Christian Peter
//
// Copyright (c) 2022 Christian Peter
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

using BlueBasics;
using BlueBasics.Enums;
using BlueControls.Controls;
using BlueControls.Enums;
using BlueControls.Interfaces;
using BlueScript;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;

using BlueBasics.Enums;
using BlueControls.Controls;
using BlueControls.Enums;

using BlueControls.EventArgs;
using BlueControls.Forms;
using BlueControls.ItemCollection;
using BlueDatabase;

using System;
using System.Drawing;

using static BlueBasics.Converter;

namespace BlueControls.ItemCollection {

    public class ColumnPadItem : BitmapPadItem {

        #region Fields

        public readonly ColumnItem Column;

        #endregion

        #region Constructors

        public ColumnPadItem(ColumnItem c) : base(c.Name) {
            Column = c;
            Bild_Modus = enSizeModes.Verzerren;

            //Bitmap Bitmap { get; set; }
        }

        #endregion
    }
}