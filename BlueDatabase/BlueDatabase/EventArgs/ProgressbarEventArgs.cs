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


namespace BlueDatabase.EventArgs
{
    public class ProgressbarEventArgs : System.EventArgs
    {
        public ProgressbarEventArgs(string name, int current, int count, bool beginns, bool ends)
        {
            this.Name = name;
            this.Current = current;
            this.Count = count;
            this.Beginns = beginns;
            this.Ends = ends;
        }

        public int Count { get; private set; }
        public int Current { get; private set; }

        public string Name { get; private set; }

        public bool Beginns { get; private set; }
        public bool Ends { get; private set; }
    }
}
