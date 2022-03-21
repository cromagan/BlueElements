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
using BlueControls.Enums;
using BlueDatabase;
using BlueDatabase.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using BlueControls.ItemCollection.ItemCollectionList;
using static BlueBasics.Converter;
using BlueDatabase.Enums;
using System.Linq.Expressions;

#nullable enable

namespace BlueControls.Controls {

    // https://stackoverflow.com/questions/724143/how-do-i-create-a-delegate-for-a-net-property
    // http://peisker.net/dotnet/propertydelegates.htm
    // http://geekswithblogs.net/akraus1/archive/2006/02/10/69047.aspx
    public class FlexiControlForProperty<t> : FlexiControl {

        #region Fields

        private readonly bool _isButton = false;
        private BlueBasics.Accessor<t>? _accessor;

        #endregion

        #region Constructors

        public FlexiControlForProperty(Expression<Func<t>> expr, ItemCollectionList? list) : this(expr, 1, list, ImageCode.None) { }

        public FlexiControlForProperty(Expression<Func<t>> expr, int rowCount) : this(expr, rowCount, null, ImageCode.None) { }

        public FlexiControlForProperty(Expression<Func<t>> expr, ImageCode image) : this(expr, 1, null, image) { }

        public FlexiControlForProperty(Expression<Func<t>> expr) : this(expr, 1, null, ImageCode.None) { }

        //public FlexiControlForProperty(object expr) : this(expr, 1, null, ImageCode.None) { }

        public FlexiControlForProperty() : base() {
            GenFehlerText();
            _IdleTimer.Tick += Checker_Tick;
            CaptionPosition = ÜberschriftAnordnung.Links_neben_Dem_Feld;
            EditType = EditTypeFormula.Textfeld;
            Size = new Size(200, 24);
        }

        private FlexiControlForProperty(Expression<Func<t>> expr, int rowCount, ItemCollectionList? list, ImageCode image) : this() {
            _accessor = new Accessor<t>(expr);

            _isButton = (image != ImageCode.None);

            UpdateControlData(rowCount, list, image);
            CheckEnabledState();
        }

        #endregion

        #region Methods

        internal bool CheckEnabledState() {
            if (DesignMode) {
                DisabledReason = string.Empty;
                return true;
            }

            if (_accessor == null) {
                DisabledReason = "Kein zugehöriges Objekt definiert.";
                return false;
            }

            if (!_accessor.CanWrite) {
                DisabledReason = "Feld kann generell nicht beschrieben werden.";
                return false;
            }
            //if (_alwaysDiabled) {
            //    DisabledReason = "Feld ist schreibgeschützt.";
            //    return false;
            //}
            DisabledReason = string.Empty;
            return true;
        }

        protected override void Dispose(bool disposing) {
            _IdleTimer.Tick -= Checker_Tick;
            //if (_propertyObject is IReloadable LS) { LS.LoadedFromDisk -= OnLoadedFromDisk; }
            base.Dispose(disposing);
        }

        protected override void OnButtonClicked() {
            base.OnButtonClicked();
            if (_isButton) {
                _accessor.Set(default);
            }

            //if (_methInfo != null) {
            //    _methInfo.Invoke(_propertyObject, null);
            //}
        }

        protected override void OnControlAdded(ControlEventArgs e) {
            CheckEnabledState();
            base.OnControlAdded(e);
        }

        protected override void OnHandleDestroyed(System.EventArgs e) {
            FillPropertyNow();
            base.OnHandleDestroyed(e);
        }

        protected override void OnValueChanged() {
            FillPropertyNow(); // erst befüllen, bevor das Event ausgelöst wird
            GenFehlerText(); // erst Standard fehler Text, bevor das Event ausgelöst wird
            base.OnValueChanged();
        }

        private void Checker_Tick(object sender, System.EventArgs e) {
            //if (_IsFilling) { return; }
            if (!Allinitialized) { return; }
            if (LastTextChange != null) { return; } // Noch am bearbeiten
            SetValueFromProperty();
        }

        private void FillPropertyNow() {
            if (!Allinitialized) { return; }
            if (!CheckEnabledState()) { return; } // Versuch. Eigentlich darf das Steuerelement dann nur empfangen und nix ändern.

            if (_accessor == null || !_accessor.CanRead) { return; }
            if (_isButton) { return; }

            var oldVal = string.Empty;
            t x = _accessor.Get();

            t toSet = x;

            if (x is string s) {
                oldVal = s;
                toSet = (t)Convert.ChangeType(Value, typeof(t));
            } else if (x is List<string> ls) {
                oldVal = ls.JoinWithCr();
                toSet = (t)Convert.ChangeType(Value.SplitAndCutByCrToList(), typeof(t));
            } else if (x is bool bo) {
                oldVal = bo.ToPlusMinus();
                toSet = (t)Convert.ChangeType(Value.FromPlusMinus(), typeof(t));
            } else if (x is Color co) {
                oldVal = co.ToHtmlCode();
                toSet = (t)Convert.ChangeType(Value.FromHtmlCode(), typeof(t));
            } else if (x is int iv) {
                oldVal = iv.ToString();
                IntTryParse(Value, out var i);
                toSet = (t)Convert.ChangeType(Value, typeof(t)); //i;
            } else if (x is Enum enx) {
                oldVal = enx. ((int)enx).ToString();
                IntTryParse(Value, out var i);
                toSet = (t)Convert.ChangeType(i, typeof(t)); // i;
            } else if (x is double db) {
                oldVal = db.ToString(Constants.Format_Float2);
                DoubleTryParse(Value, out var i);
                toSet = (t)Convert.ChangeType(i, typeof(t)); //i;
            } else if (x is float fl) {
                oldVal = fl.ToString(Constants.Format_Float2);
                FloatTryParse(Value, out var i);
                toSet = (t)Convert.ChangeType(i, typeof(t)); //i;
            } else {
                Develop.DebugPrint(FehlerArt.Fehler, "Art unbekannt!");
            }
            if (oldVal == Value) { return; }

            _accessor.Set(toSet);
        }

        private void GenFehlerText() {
            //if ( _accessor != null) {
            //    InfoText = string.Empty;
            //    return;
            //}
            ////if (_FehlerWennLeer && string.IsNullOrEmpty(Value)) {
            ////    InfoText = "Dieses Feld darf nicht leer sein.";
            ////    return;
            ////}
            //if (string.IsNullOrEmpty(Value)) {
            //    InfoText = string.Empty;
            //    return;
            //}
            ////if (_FehlerFormatCheck && !Value.IsFormat(Format)) {
            ////    InfoText = "Der Wert entspricht nicht dem erwarteten Format.";
            ////    return;
            ////}
            InfoText = string.Empty;
        }

        //private void OnLoadedFromDisk(object sender, System.EventArgs e) {
        //    FillPropertyNow();
        //    _propertyObject = null;  //Das Objekt ist tot und irgendwo im Nirvana verschwunden
        //    UpdateControlData(false, 1, null, enImageCode.None);
        //    CheckEnabledState();
        //    LoadedFromDisk?.Invoke(this, System.EventArgs.Empty);
        //}
        private void SetValueFromProperty() {
            if (_accessor == null || !_accessor.CanRead) {
                ValueSet(string.Empty, true, false);
                InfoText = string.Empty;
                return;
            }
            object x = _accessor.Get();

            if (x is null) {
                ValueSet(string.Empty, true, false);
            } else if (x is string s) {
                ValueSet(s, true, false);
            } else if (x is List<string> ls) {
                ValueSet(ls.JoinWithCr(), true, false);
            } else if (x is bool bo) {
                ValueSet(bo.ToPlusMinus(), true, false);
            } else if (x is int iv) {
                ValueSet(iv.ToString(), true, false);
            } else if (x is Enum) {
                ValueSet(((int)x).ToString(), true, false);
            } else if (x is double dc) {
                ValueSet(dc.ToString(Constants.Format_Float2), true, false);
            } else if (x is double db) {
                ValueSet(db.ToString(Constants.Format_Float2), true, false);
            } else if (x is float fl) {
                ValueSet(fl.ToString(Constants.Format_Float2), true, false);
            } else if (x is Color co) {
                ValueSet(co.ToHtmlCode(), true, false);
            } else {
                Develop.DebugPrint(FehlerArt.Fehler, "Art unbekannt!");
            }
        }

        private void UpdateControlData(int textLines, ItemCollectionList? list, ImageCode image) {

            #region Caption setzen

            var x = _accessor.Name.SplitAndCutBy("__");
            Caption = x[0].Replace("_", " ") + ":";
            FileEncryptionKey = string.Empty;

            #endregion Caption setzen

            #region Art des Steuerelements bestimmen

            if (_isButton) {
                EditType = EditTypeFormula.Button;
                CaptionPosition = ÜberschriftAnordnung.ohne;
                var s0 = BlueFont.MeasureStringOfCaption(Caption.TrimEnd(":"));
                Size = new Size((int)s0.Width + 50 + 22, 30);
                var c0 = (Button)CreateSubControls();
                c0.Text = Caption.TrimEnd(":");
                if (image != ImageCode.None) {
                    c0.ImageCode = QuickImage.Get(image, 22).ToString();
                }
            } else {
                switch (_accessor.TypeFullname.ToLower()) {
                    case "system.boolean": {
                            EditType = EditTypeFormula.Ja_Nein_Knopf;
                            var s1 = BlueFont.MeasureStringOfCaption(Caption);
                            Size = new Size((int)s1.Width + 30, 22);
                            break;
                        }
                    default: // Alle enums sind ein eigener Typ.... deswegen alles in die Textbox
                        {
                            if (list != null) {
                                EditType = EditTypeFormula.Textfeld_mit_Auswahlknopf;
                                list.Appearance = BlueListBoxAppearance.ComboBox_Textbox;
                                var s2 = BlueFont.MeasureStringOfCaption(Caption);
                                var (biggestItemX, biggestItemY, _, _) = list.ItemData(); // BiggestItemX, BiggestItemY, HeightAdded, SenkrechtAllowed
                                var x2 = Math.Max((int)(biggestItemX + 20 + s2.Width), 200);
                                var y2 = Math.Max(biggestItemY + (Skin.PaddingSmal * 2), 24);
                                Size = new Size(x2, y2);
                                StyleComboBox((ComboBox)CreateSubControls(), list, ComboBoxStyle.DropDownList);
                            } else {
                                EditType = EditTypeFormula.Textfeld;
                                var tmpName = _accessor.TypeFullname.ToLower();
                                if (textLines >= 2) {
                                    CaptionPosition = ÜberschriftAnordnung.Über_dem_Feld;
                                    Size = new Size(200, 16 + (24 * textLines));
                                    MultiLine = true;
                                    tmpName = "system.string";
                                } else {
                                    CaptionPosition = ÜberschriftAnordnung.Links_neben_Dem_Feld;
                                    Size = new Size(200, 24);
                                    MultiLine = false;
                                }

                                switch (tmpName) {
                                    case "system.string": this.SetFormat(VarType.Text); break;
                                    case "system.int32": this.SetFormat(VarType.Integer); break;
                                    case "system.float": this.SetFormat(VarType.Float); break;
                                    case "system.double": this.SetFormat(VarType.Float); break;
                                    case "system.drawing.color": this.SetFormat(VarType.Text); break;
                                    default: this.SetFormat(VarType.Text); break;
                                }

                                StyleTextBox((TextBox)CreateSubControls());
                            }
                            break;
                        }
                }
            }

            #endregion Art des Steuerelements bestimmen

            QuickInfo = _accessor.QuickInfo;

            SetValueFromProperty();
            GenFehlerText();
        }

        #endregion
    }
}