// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using System.Globalization;
using System.Drawing.Design;
using System.Windows.Forms.Design;

namespace BlueControls.Designer_Support;

public sealed class QuickPicSelector : UITypeEditor, IDisposable {

    #region Fields

    private readonly QuickPicDesigner _fqp = new();
    private IWindowsFormsEditorService? _edSvc;

    public void Dispose() => _fqp?.Dispose();

    #endregion

    #region Methods

    public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value) {
        _edSvc = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));
        var _c = Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty;
        _fqp.StartAll(_c);
        //we add handler to the about form button1 in order to close the form when the button is clicked
        _fqp.btnOk.Click += Click;
        Develop.Debugprint_BackgroundThread();
        _edSvc.DropDownControl(_fqp);
        return _c;
    }

    public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context) => UITypeEditorEditStyle.Modal;

    private void Click(object? sender, System.EventArgs e) {
        if (_edSvc == null) {
            return;
        }

  
        _edSvc.CloseDropDown();
    }

    #endregion
}