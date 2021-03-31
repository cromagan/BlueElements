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

namespace Skript.Enums
{

    public enum enVariableDataType
    {

        NotDefinedYet = 0,
        Bool = 1,
        Number = 2,
        String = 4,
        Date = 8,
        List = 16,

        /// <summary>
        /// Nur für Attribute
        /// </summary>
        Integer = 256,

        Number_or_String = Number | String,

        String_or_List = String | List,

        Bool_Number_or_String = Bool | Number | String,
        Bool_Number_String_or_List = Bool | Number | String | List,

        Variable = 1024,

        Error = 2048,
        Null = 4096,


        //VariableBool = 128,
        //VariableNum = 256,
        //VariableString = 512,
        VariableList = Variable | List,
        VariableString = Variable | String,

        VariableListOrString = Variable | String | List,
        VariableListOrStringNumBool = Variable | String | List | Number | Bool,
        VariableStringNum = Variable | String | List | Number,
        VariableNumStrListDateBool = Variable | String | List | Number | Bool | Date,
        VariableAny = Variable | String | List | Number | Bool | Date | Error | NotDefinedYet
    }

}