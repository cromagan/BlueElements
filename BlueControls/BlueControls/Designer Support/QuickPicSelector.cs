using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Forms.Design;
using BlueBasics;

namespace BlueControls.Designer_Support
{
    public sealed class QuickPicSelector : UITypeEditor
    {
        private IWindowsFormsEditorService edSvc;
        private readonly frmQuickPic fqp = new frmQuickPic();
        private string C;

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object Value)
        {

            edSvc = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));

            C = Convert.ToString(Value);
            fqp.StartAll(C);

            //we add handler to the about form button1 in order to close the form when the button is clicked
            fqp.ButOK.Click += Click;
            Develop.Debugprint_BackgroundThread();
            edSvc.DropDownControl(fqp);

            return C;
        }


        private void Click(object sender, System.EventArgs e)
        {
            if (edSvc != null)
            {
                C = fqp.ICode();
                edSvc.CloseDropDown();
            }
        }


        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.Modal;
        }
    }
}