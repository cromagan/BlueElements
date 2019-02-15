using System;
using BlueControls.EventArgs;

namespace BlueControls.Interfaces
{
    public interface IContextMenu
    {
        //bool isContextMenuCurentlyShowing();

        void ContextMenu_Show(object sender, System.Windows.Forms.MouseEventArgs e);
        //void ContextMenu_Close();

        event EventHandler<ContextMenuInitEventArgs> ContextMenuInit;
        event EventHandler<ContextMenuItemClickedEventArgs> ContextMenuItemClicked;
    }
}