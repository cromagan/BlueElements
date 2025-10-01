// Authors:
// Christian Peter
//
// Copyright (c) 2025 Christian Peter
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
using BlueControls;
using BlueControls.Forms;
using BlueControls.Interfaces;
using BlueControls.ItemCollectionList;
using System;
using System.Reflection;

#nullable enable

namespace BeCreative {

    [StandaloneInfo("Auswahl-Fenster", ImageCode.Stern)]
    public partial class Start : Form, IIsStandalone {

        #region Constructors

        public Start() {
            InitializeComponent();

            var types = Generic.GetTypesOfType<IIsStandalone>();

            foreach (var thisType in types) {
                var name = thisType.Name;
                ImageCode i = ImageCode.Fragezeichen;

                var attr = thisType.GetCustomAttribute<StandaloneInfoAttribute>();
                if (attr != null) {
                    name = attr.Name;
                    i = attr.Symbol;
                }

                var p = new TextListItem(name, name, QuickImage.Get(i, 24), false, true, name) {
                    Tag = thisType
                };
                Forms.ItemAdd(p);
            }
        }

        #endregion

        #region Methods

        private void Forms_ItemClicked(object sender, BlueControls.EventArgs.AbstractListItemEventArgs e) {
            if (e.Item.Tag is not Type t) { return; }

            var instance = (IIsStandalone)Activator.CreateInstance(t);

            if (instance is System.Windows.Forms.Form frm) {
                FormManager.RegisterForm(frm);
                frm.Show();
                Close();
                frm.BringToFront();
            }
        }

        #endregion
    }
}