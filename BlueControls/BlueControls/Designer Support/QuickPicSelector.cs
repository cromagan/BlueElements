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

using BlueBasics;
using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Forms.Design;

namespace BlueControls.Designer_Support;

public sealed class QuickPicSelector : UITypeEditor {

    #region Fields

    private readonly QuickPic _fqp = new();
    private string _c;
    private IWindowsFormsEditorService _edSvc;

    #endregion

    #region Methods

    public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value) {
        _edSvc = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));
        _c = Convert.ToString(value);
        _fqp.StartAll(_c);
        //we add handler to the about form button1 in order to close the form when the button is clicked
        _fqp.ButOK.Click += Click;
        Develop.Debugprint_BackgroundThread();
        _edSvc.DropDownControl(_fqp);
        return _c;
    }

    public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context) => UITypeEditorEditStyle.Modal;

    private void Click(object sender, System.EventArgs e) {
        if (_edSvc == null) {
            return;
        }

        _c = _fqp.ICode();
        _edSvc.CloseDropDown();
    }

    #endregion
}