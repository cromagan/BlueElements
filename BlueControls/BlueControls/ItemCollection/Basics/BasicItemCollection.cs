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

using BlueBasics;
using BlueBasics.Enums;
using BlueControls.ItemCollection.Basics;
using System;
namespace BlueControls.ItemCollection
{
    public class BasicItemCollection<t> : ListExt<t> where t : BasicItem
    {


        #region  Event-Deklarationen + Delegaten 
        //public event EventHandler Changed;
        public event EventHandler DoInvalidate;
        #endregion





        public void OnDoInvalidate()
        {
            DoInvalidate?.Invoke(this, System.EventArgs.Empty);
        }


        protected override void OnItemAdded(t item)
        {
            if (string.IsNullOrEmpty(item.Internal))
            {
                Develop.DebugPrint(enFehlerArt.Fehler, "Der Auflistung soll ein Item hinzugefügt werden, welches keinen Namen hat " + item.Internal);
            }

            base.OnItemAdded(item);
            OnDoInvalidate();
        }


        //public void OnChanged()
        //{
        //    //if (IsParsing)
        //    //{
        //    //    Develop.DebugPrint(enFehlerArt.Warnung, "Falscher Parsing Zugriff!");
        //    //    return;
        //    //}
        //    q
        //    Changed?.Invoke(this, System.EventArgs.Empty);
        //}


        protected override void OnListOrItemChanged()
        {
            base.OnListOrItemChanged();
            OnDoInvalidate();
        }

    }
}
