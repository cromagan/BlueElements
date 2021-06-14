// Authors:
// Christian Peter
//
// Copyright (c) 2021 Christian Peter
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
using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Forms.Design;

namespace BlueControls.Designer_Support {

    public sealed class QuickPicSelector : UITypeEditor {

        #region Fields

        private readonly frmQuickPic fqp = new();
        private string C;
        private IWindowsFormsEditorService edSvc;

        #endregion

        #region Methods

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value) {
            edSvc = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));
            C = Convert.ToString(value);
            fqp.StartAll(C);
            //we add handler to the about form button1 in order to close the form when the button is clicked
            fqp.ButOK.Click += Click;
            Develop.Debugprint_BackgroundThread();
            edSvc.DropDownControl(fqp);
            return C;
        }

        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context) => UITypeEditorEditStyle.Modal;

        private void Click(object sender, System.EventArgs e) {
            if (edSvc != null) {
                C = fqp.ICode();
                edSvc.CloseDropDown();
            }
        }

        #endregion
    }
}