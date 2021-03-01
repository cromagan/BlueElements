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


using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;



namespace BlueScript {
    public class Script {


        string _ScriptText = string.Empty;

        public IEnumerable<Method> Comands;

        public readonly List<Variable> Variablen;

        public static IEnumerable<T> GetEnumerableOfType<T>(params object[] constructorArgs) where T : class {
            var objects = new List<T>();
            foreach (var type in
                Assembly.GetAssembly(typeof(T)).GetTypes()
                .Where(myType => myType.IsClass && !myType.IsAbstract && myType.IsSubclassOf(typeof(T)))) {
                objects.Add((T)Activator.CreateInstance(type, constructorArgs));
            }
            //objects.Sort();
            return objects;
        }


        public Script(List<Variable> variablen) {


            Comands = GetEnumerableOfType<Method>();

            Variablen = variablen;
        }

        public string ScriptText {
            get {
                return _ScriptText;
            }
            set {

                if (_ScriptText == value) { return; }
                //_parsed = false;
                _ScriptText = value;
            }
        }




        public (string error, int pos) Parse() {

            var pos = -1;


            do {
                pos++;


                foreach (var thisC in Comands) {

                    (var continuepos, var error, var abbruch, var betweentext) = thisC.CanDo(_ScriptText, pos);

                    if (abbruch) { return (error, continuepos); }

                    if (string.IsNullOrEmpty(error)) {
                        (var error2, var pos2) = thisC.DoIt(betweentext, Variablen);

                        if (!string.IsNullOrEmpty(error2)) { return (error2, pos + pos2); }

                    }


                }




            } while (true);





        }


    }
}
