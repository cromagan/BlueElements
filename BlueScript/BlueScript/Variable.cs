﻿#region BlueElements - a collection of useful tools, database and controls
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static BlueBasics.modAllgemein;
using static BlueBasics.Extensions;
using BlueBasics;
using Skript.Enums;

namespace BlueScript {
    public class Variable {


        public override string ToString() {


            switch (Type) {

                case enVariableDataType.String:
                    return Name + ": {string} " + ValueString;

                case enVariableDataType.Number:
                    return Name + ": {numeral} " + ValueString;

                case enVariableDataType.Date:
                    return Name + ": {date} " + ValueString;

                case enVariableDataType.Bool:
                    return Name + ": {bool} " + ValueString;

                case enVariableDataType.List:
                    return Name + ": {list} " + ValueString;

                default:
                    return Name + ": {unknown} " + ValueString;

            }

        }

        public Variable(string name) {

            if (!IsValidName(name)) {
                Develop.DebugPrint(BlueBasics.Enums.enFehlerArt.Fehler, "Ungültiger Variablenname: " + name);
            }
            Name = name.ToLower();
        }


        public string ValueForReplace {
            get {

                switch (Type) {

                    case enVariableDataType.String:
                        return "\"" + ValueString + "\"";

                    case enVariableDataType.Number:
                        return ValueString;

                    default:
                        Develop.DebugPrint_NichtImplementiert();
                        return ValueString;

                }
            }
        }


        public string Name { get; private set; }
        public string ValueString { get; set; }

        public enVariableDataType Type { get; set; }

        public static bool IsValidName(string v) {

            v = v.ToLower();

            var vo = v;
            v = v.ReduceToChars(Constants.Char_az + "_" + Constants.Char_Numerals);


            if (v != vo) { return false; }

            if (string.IsNullOrEmpty(v)) { return false; }

            return true;


        }
    }


    public static class VariableExtensions {


        public static Variable Get(this List<Variable> vars, string name) {

            foreach (var thisv in vars) {
                if (thisv.Name.ToUpper() == name.ToUpper()) {
                    return thisv;
                }

            }

            return null;
        }

        public static List<string> AllNames(this List<Variable> vars) {

            var l = new List<string>();
            foreach (var thisvar in vars) {
                l.Add(thisvar.Name);
            }

            return l;
        }

    }


}





