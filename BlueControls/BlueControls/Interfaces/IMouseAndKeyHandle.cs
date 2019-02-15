

namespace BlueControls.Interfaces
{
    public interface IMouseAndKeyHandle
    {
        bool MouseDown(object sender, System.Windows.Forms.MouseEventArgs e, decimal cZoom, decimal MoveX, decimal MoveY);
        bool MouseMove(object sender, System.Windows.Forms.MouseEventArgs e, decimal cZoom, decimal MoveX, decimal MoveY);
        bool MouseUp(object sender, System.Windows.Forms.MouseEventArgs e, decimal cZoom, decimal MoveX, decimal MoveY);
        bool KeyUp(object sender, System.Windows.Forms.KeyEventArgs e, decimal cZoom, decimal MoveX, decimal MoveY);

        //Function BlueCreativePad_MouseWheel(sender As Object, e As System.Windows.Forms.MouseEventArgs) As Boolean




        //Function MouseClick(sender As Object, e As System.Windows.Forms.MouseEventArgs) As Boolean
        //Function MouseDoubleClick(sender As Object, e As System.Windows.Forms.MouseEventArgs) As Boolean
        //Function MouseEnter(sender As Object, e As System.EventArgs) As Boolean
        //Function MouseHover(sender As Object, e As System.EventArgs) As Boolean
        //Function MouseLeave(sender As Object, e As System.EventArgs) As Boolean

    }
}