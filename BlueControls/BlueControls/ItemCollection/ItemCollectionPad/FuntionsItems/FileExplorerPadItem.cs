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
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using BlueBasics;
using BlueControls.Controls;
using BlueControls.Interfaces;
using BlueDatabase.Enums;

namespace BlueControls.ItemCollection;

public class FileExplorerPadItem : CustomizableShowPadItem, IItemToControl {

    #region Fields

    private string _pfad = string.Empty;

    #endregion

    #region Constructors

    public FileExplorerPadItem(string internalname) : base(internalname) => SetCoordinates(new RectangleF(0, 0, 50, 30), true);

    #endregion

    #region Properties

    public static string ClassId => "FI-FileExplorer";

    [Description("Der Dateipfad, dessen Dateien angezeigt werden sollen.\r\nEs können Variablen aus dem Skript benutzt werden.\r\nDiese müssen im Format ~variable~ angegeben werden.")]
    public string Pfad {
        get => _pfad;

        set {
            if (value == _pfad) { return; }
            _pfad = value;
            RaiseVersion();
            OnChanged();
        }
    }

    protected override int SaveOrder => 4;

    #endregion

    #region Methods

    public override Control CreateControl(ConnectedFormulaView parent) {
        var con = new FileBrowser {
            OriginalText = Pfad,
            Name = DefaultItemToControlName()
        };

        if (GetRowFrom is ICalculateRowsItemLevel rfw2) {
            var ff = parent.SearchOrGenerate(rfw2);
            if (ff is ICalculateRowsControlLevel cc) { cc.Childs.Add(con); }
        }
        return con;
    }

    public override List<GenericControl> GetStyleOptions() {
        List<GenericControl> l = new();
        l.AddRange(base.GetStyleOptions());
        l.Add(new FlexiControlForProperty<string>(() => Pfad));
        return l;
    }

    public override bool ParseThis(string tag, string value) {
        if (base.ParseThis(tag, value)) { return true; }
        switch (tag) {
            case "pfad":
                _pfad = value.FromNonCritical();
                return true;
        }
        return false;
    }

    public override string ToString() {
        var result = new List<string>();
        result.ParseableAdd("Pfad", _pfad);
        return result.Parseable(base.ToString());
    }

    protected override void DrawExplicit(Graphics gr, RectangleF positionModified, float zoom, float shiftX, float shiftY, bool forPrinting) {
        var id = -1;
        if (GetRowFrom != null) { id = GetRowFrom.Id; }

        if (!forPrinting) {
            DrawColorScheme(gr, positionModified, zoom, id);
        }

        DrawFakeControl(gr, positionModified, zoom, ÜberschriftAnordnung.Über_dem_Feld, "C:\\");

        base.DrawExplicit(gr, positionModified, zoom, shiftX, shiftY, forPrinting);
    }

    #endregion

    //protected override BasicPadItem? TryCreate(string id, string name) {
    //    if (id.Equals(ClassId, StringComparison.OrdinalIgnoreCase)) {
    //        return new FileExplorerPadItem(name);
    //    }
    //    return null;
    //}
}