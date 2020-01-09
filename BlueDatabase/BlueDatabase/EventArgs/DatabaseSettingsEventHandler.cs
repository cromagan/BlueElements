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
    public class DatabaseSettingsEventHandler : System.EventArgs
    {


        public DatabaseSettingsEventHandler(ColumnItem ExecutingColumn, string Filenname, bool ReadOnly)
        {
            this.ExecutingColumn = ExecutingColumn;
            this.Filenname = Filenname;
            this.ReadOnly = ReadOnly;
            //this.PasswordSub = PasswordSub;
            //this.GenenerateLayout = GenLayout;
            //this.RenameColumnInLayout = RenameColumn;
        }

        public ColumnItem ExecutingColumn { get;  }
        public string Filenname { get; }

        public bool ReadOnly { get; set; }

        //public GetPassword PasswordSub { get; set; }
        //public GenerateLayout_Internal GenenerateLayout { get; set; }

        //public RenameColumnInLayout RenameColumnInLayout { get; set; }
    }
}
