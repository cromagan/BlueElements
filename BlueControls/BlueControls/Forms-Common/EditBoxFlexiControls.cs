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
using BlueControls.Controls;
using System;
using System.Collections.Generic;

namespace BlueControls.Forms {

    public partial class EditBoxFlexiControl : DialogWithOkAndCancel {

        #region Fields

        private List<string> GiveBack = null;

        #endregion

        #region Constructors

        private EditBoxFlexiControl() : this(null) { }

        private EditBoxFlexiControl(List<FlexiControl>? flexis) : base(true, false) {
            InitializeComponent();
            var top = Skin.Padding;
            var we = 300 + (Skin.Padding * 4);

            if (flexis != null) {
                foreach (var ThisFlexi in flexis) {
                    Controls.Add(ThisFlexi);
                    ThisFlexi.DisabledReason = string.Empty;
                    ThisFlexi.Left = Skin.Padding;
                    ThisFlexi.Top = top;
                    ThisFlexi.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
                    top = top + Skin.Padding + ThisFlexi.Height;
                    we = Math.Max(we, ThisFlexi.Width + (Skin.Padding * 4));
                    ThisFlexi.ButtonClicked += FlexiButtonClick;
                }
                we = Math.Min(we, 1500);
                foreach (var ThisFlexi in flexis) {
                    ThisFlexi.Width = Width - (Skin.Padding * 4);
                }
            }
            Setup(we, top);
        }

        #endregion

        #region Methods

        public static List<string> Show(List<FlexiControl> flexis) {
            EditBoxFlexiControl MB = new(flexis);
            MB.ShowDialog();
            return MB.GiveBack;
        }

        protected override void SetValue(bool canceled) {
            GiveBack = new List<string>();
            if (!canceled) {
                foreach (var thisObj in Controls) {
                    if (thisObj is FlexiControl ThisFlexi) {
                        if (!string.IsNullOrEmpty(ThisFlexi.ValueId)) {
                            GiveBack.TagSet(ThisFlexi.ValueId, ThisFlexi.Value.ToNonCritical());
                        }
                    }
                }
            }
        }

        private void FlexiButtonClick(object sender, System.EventArgs e) => Ok();

        #endregion
    }
}