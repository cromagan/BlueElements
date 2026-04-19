using System.Diagnostics;
using System.Drawing;

namespace BluePaint
{
     public partial class GenericTool
    {
		//Hinweis: Die folgende Prozedur ist für den Windows Form-Designer erforderlich.
		//Das Bearbeiten ist mit dem Windows Form-Designer möglich.  
		//Das Bearbeiten mit dem Code-Editor ist nicht möglich.
		[DebuggerStepThrough()]
		private void InitializeComponent()
		{
			this.SuspendLayout();
			//
			//AbstractTool
			//
			this.Name = "AbstractTool";
			this.Size = new Size(300, 400);
			this.ResumeLayout(false);
		}
	}
}