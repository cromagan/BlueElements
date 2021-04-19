#region BlueElements - a collection of useful tools, database and controls
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
#endregion


using System.Collections.Generic;


namespace BlueControls.ItemCollection {
    internal class clsPhysicPadItem : clsAbstractPhysicPadItem {


        public readonly List<clsKraft> Kraft = new();



        public clsPhysicPadItem(ItemCollectionPad parent) : base(parent, string.Empty) {

        }


        protected override string ClassId() {
            return "Physics-Object";
        }
        //public override bool Contains(PointF value, decimal zoomfactor)
        //{
        //    throw new NotImplementedException();
        //}


        //public override RectangleM UsedArea()
        //{
        //    throw new NotImplementedException();
        //}

        //protected override string ClassId()
        //{
        //    throw new NotImplementedException();
        //}



        //protected override void GenerateInternalRelationExplicit()
        //{
        //    throw new NotImplementedException();
        //}

        //protected override void ParseFinished()
        //{
        //    throw new NotImplementedException();
        //}
    }
}
