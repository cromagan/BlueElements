#region BlueElements - a collection of useful tools, database and controls
// Authors: 
// Christian Peter
// 
// Copyright (c) 2019 Christian Peter
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


using BlueControls.ItemCollection;

namespace BlueControls.EventArgs
{
    public class ItemSwapEventArgs : AllreadyHandledEventArgs
    {


        public ItemSwapEventArgs(BasicListItem ItemToSwap1, BasicListItem ItemToSwap2, bool AllreadyHandled) : base(AllreadyHandled)
        {
            this.ItemToSwap1 = ItemToSwap1;
            this.ItemToSwap2 = ItemToSwap2;
        }

        public BasicListItem ItemToSwap1 { get; set; }
        public BasicListItem ItemToSwap2 { get; set; }

    }
}
