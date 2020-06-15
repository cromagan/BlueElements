#region BlueElements - a collection of useful tools, database and controls
// Authors: 
// Christian Peter
// 
// Copyright (c) 2020 Christian Peter
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
#endregion

using BlueBasics.Enums;
using BlueControls.Designer_Support;
using BlueControls.Enums;
using BlueControls.Interfaces;
using System.ComponentModel;
using System.Drawing;

namespace BlueControls.Controls
{

    [Designer(typeof(BasicDesigner))]
    public class Line : GenericControl, IBackgroundNone
    {


        #region Constructor
        public Line() : base(false, false)
        {

            // Dieser Aufruf ist für den Designer erforderlich.
            //  InitializeComponent()

            // Fügen Sie Initialisierungen nach dem InitializeComponent()-Aufruf hinzu.
            SetNotFocusable();
            _MouseHighlight = false;
        }
        #endregion




        private enOrientation _Orientation = enOrientation.Waagerecht;


        [DefaultValue(enOrientation.Waagerecht)]
        public enOrientation Orientation
        {
            get
            {
                return _Orientation;
            }
            set
            {
                if (value == _Orientation)
                {
                    return;
                }
                _Orientation = value;
                CheckSize();
                Invalidate();

            }
        }

        [DefaultValue(0)]
        public new int TabIndex
        {
            get
            {
                return 0;
            }

            set
            {
                base.TabIndex = 0;
            }
        }

        [DefaultValue(false)]
        public new bool TabStop
        {
            get
            {
                return false;
            }
            set
            {
                base.TabStop = false;
            }
        }


        protected override void InitializeSkin()
        {

        }

        public void CheckSize()
        {
            if (_Orientation == enOrientation.Waagerecht)
            {
                if (Width < 10) { Width = 10; }
                Height = 2;
            }
            else
            {
                Width = 2;
                if (Height < 10) { Height = 10; }
            }


        }


        protected override void DrawControl(Graphics gr, enStates state)
        {
            CheckSize();
            var DP = new Pen(SystemColors.ControlDark);
            var LP = new Pen(SystemColors.ControlLight);

            if (_Orientation == enOrientation.Waagerecht)
            {
                gr.DrawLine(DP, 0, 0, Width - 1, 0);
                gr.DrawLine(LP, 1, 1, Width, 1);
            }
            else
            {
                gr.DrawLine(DP, 0, 0, 0, Height - 1);
                gr.DrawLine(LP, 1, 1, 1, Height);
            }
        }
    }
}
