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
using BlueControls.Interfaces;
using BlueDatabase;
using System.Collections.Generic;
using System.ComponentModel;
using System;
using BlueControls.ItemCollectionPad.FunktionsItems_Formular;
using BlueControls.Enums;
using System.Drawing;
using System.Windows.Forms;
using BlueDatabase.Interfaces;
using BlueBasics.Interfaces;

using BlueBasics;

using BlueBasics.Enums;

using BlueBasics.Interfaces;

using BlueDatabase.Enums;
using BlueDatabase.EventArgs;

using BlueDatabase.Interfaces;
using System;
using System.Collections.Generic;

using System.Collections.ObjectModel;

using System.Drawing;

using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using static BlueBasics.Constants;
using static BlueBasics.Converter;
using static BlueBasics.IO;
using static BlueDatabase.Database;

namespace BlueControls.Controls;

internal class AdderItem : IHasDatabase, IReadableTextWithKey {

    #region Constructors

    public AdderItem(Database database, ColumnItem entityIDColumn, string generatedentityID, ColumnItem? originIDColumn, ColumnItem textKeyColumn, string generatedTextKey, ColumnItem? additinalTextColumn, string additionaltext) {
        Database = database;
        EntityIDColumn = entityIDColumn;
        GeneratedentityID = generatedentityID;
        OriginIDColumn = originIDColumn;
        TextKeyColumn = textKeyColumn;
        KeyName = generatedTextKey;
        AdditinalTextColumn = additinalTextColumn;
        Additionaltext = additionaltext;
    }

    #endregion

    #region Properties

    public ColumnItem? AdditinalTextColumn { get; }
    public string Additionaltext { get; }
    public Database? Database { get; }
    public ColumnItem EntityIDColumn { get; }
    public string GeneratedentityID { get; }

    /// <summary>
    /// Enstpricht TextKey  (Zutaten/Mehl/)
    /// </summary>
    public string KeyName { get; }

    public ColumnItem? OriginIDColumn { get; }
    public string QuickInfo => KeyName;
    public ColumnItem TextKeyColumn { get; }

    #endregion

    #region Methods

    public string ReadableText() {
        var t = KeyName.CountString("/");

        return new string(' ', t * 4) + KeyName.TrimEnd("/").FileNameWithSuffix();
    }

    public QuickImage? SymbolForReadableText() => null;

    #endregion
}