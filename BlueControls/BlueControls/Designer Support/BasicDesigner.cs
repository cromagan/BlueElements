using System.Collections;
using System.Windows.Forms.Design;

namespace BlueControls.Designer_Support
{
    internal class BasicDesigner : ControlDesigner
    {
        protected override void PreFilterProperties(IDictionary properties)
        {
            properties.Remove("BackColor");
            properties.Remove("BackgroundImage");
            properties.Remove("BackgroundImageLayout");
            properties.Remove("Font");
            properties.Remove("ForeColor");
            properties.Remove("UseWaitCursor");
            properties.Remove("RightToLeft");
            properties.Remove("Cursor");
            properties.Remove("CausesValidation");
            properties.Remove("AccessibleDescription");
            properties.Remove("AccessibleName");
            properties.Remove("AccessibleRole");
            properties.Remove("GenerateMember");
            properties.Remove("ContextMenuStrip");
            properties.Remove("ImeMode");
            properties.Remove("AllowDrop");
            base.PreFilterProperties(properties);
        }


        //Protected Overrides Sub PostFilterProperties(ByVal Properties As System.Collections.IDictionary)
        //    Properties.Remove("BackColor")
        //    Properties.Remove("BackgroundImage")
        //    Properties.Remove("BackgroundImageLayout")
        //    Properties.Remove("Font")
        //    Properties.Remove("ForeColor")
        //    Properties.Remove("UseWaitCursor")
        //    Properties.Remove("RightToLeft")
        //    Properties.Remove("Cursor")
        //    Properties.Remove("CausesValidation")
        //    Properties.Remove("AccessibleDescription")
        //    Properties.Remove("AccessibleName")
        //    Properties.Remove("AccessibleRole")
        //    Properties.Remove("GenerateMember")
        //    Properties.Remove("ContextMenuStrip")
        //    Properties.Remove("ImeMode")
        //    Properties.Remove("AllowDrop")
        //    MyBase.PostFilterProperties(Properties)
        //End Sub
    }
}

