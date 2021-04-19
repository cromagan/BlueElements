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

using BlueBasics;
using BlueBasics.Enums;
using BlueControls.Forms;
using BlueDatabase;
using BlueDatabase.Enums;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;

namespace BlueControls {
    internal static class Dictionary {
        internal static object Lock_SpellChecking = new();


        private static Database _DictWords;

        public static bool IsSpellChecking;


        private static void Init() {
            _DictWords = Database.LoadResource(Assembly.GetAssembly(typeof(Skin)), "Deutsch.MDB", "Dictionary", true, false);
        }


        //Sub Release()

        //    If _DictWords IsNot Nothing AndAlso Not _DictWords.IsDisposed Then
        //        _DictWords.Releasex()
        //    End If


        //End Sub


        public static bool DictionaryRunning(bool TryToInit) {
            if (TryToInit && _DictWords == null) {
                Init();
            }
            return _DictWords != null;
        }


        public static bool IsWriteable() {
            if (_DictWords == null) { return false; }
            if (string.IsNullOrEmpty(_DictWords.Filename)) { return false; }
            return true;
        }


        public static bool IsWordOk(string Word) {
            if (!DictionaryRunning(true)) { return true; }
            if (string.IsNullOrEmpty(Word)) { return true; }
            if (Word.Length == 1) { return true; }
            if (Word.IsFormat(enDataFormat.Gleitkommazahl)) { return true; }
            if (Constants.Char_Numerals.Contains(Word.Substring(0, 1))) { return true; }// z.B. 00 oder 1b oder 2L


            if (Word != Word.ToLower() && Word != Word.ToUpper() && Word != Word.Substring(0, 1).ToUpper() + Word.Substring(1).ToLower()) { return false; }

            if (Word == Word.ToLower()) {
                // Wenn ein Wort klein geschrieben ist, mu0ß auch das kleingeschriebene in der Datenbank sein!
                return _DictWords.Row[new FilterItem(_DictWords.Column[0], enFilterType.Istgleich, Word)] != null;
            }


            return _DictWords.Row[Word] != null;
        }


        public static List<string> SimilarTo(string Word) {
            if (IsWordOk(Word)) { return null; }

            var l = new List<string>();


            foreach (var ThisRowItem in _DictWords.Row) {

                if (ThisRowItem != null) {
                    var w = ThisRowItem.CellFirstString();
                    var di = modAllgemein.LevenshteinDistance(Word.ToLower(), w.ToLower());

                    if (di < Word.Length / 2.0 || di < w.Length / 2.0) {
                        l.Add(di.ToString(Constants.Format_Integer5) + w);
                    }

                }
            }

            if (l.Count == 0) { return null; }

            l.Sort();
            var L2 = new List<string>();

            foreach (var Thisstring in l) {
                L2.Add(Thisstring.Substring(5));

                if (L2.Count == 10) { return L2; }

            }

            return L2;
        }


        public static void SpellCheckingAll(ExtText _ETXT, bool AllOK) {

            var Can = Monitor.TryEnter(Lock_SpellChecking);
            if (Can) { Monitor.Exit(Lock_SpellChecking); }


            if (!Can || IsSpellChecking) {
                MessageBox.Show("Die Rechtschreibprüfung steht<br>nicht zur Verfügung.", enImageCode.Information, "OK");
                return;
            }


            lock (Lock_SpellChecking) {


                var Pos = 0;
                var woEnd = -1;


                IsSpellChecking = true;


                do {
                    Pos = Math.Max(woEnd + 1, Pos + 1);

                    if (Pos >= _ETXT.Chars.Count) { break; }

                    var woStart = _ETXT.WordStart(Pos);

                    if (woStart > -1) {
                        woEnd = _ETXT.WordEnd(Pos);
                        var wort = _ETXT.Word(Pos);

                        if (!IsWordOk(wort)) {


                            var butt = 0;

                            if (wort.ToLower() != wort) {
                                butt = MessageBox.Show("<b>" + wort + "</b>", enImageCode.Stift, "'" + wort + "' aufnehmen", "'" + wort.ToLower() + "' aufnehmen", "Ignorieren", "Beenden");
                            } else {

                                if (AllOK) {
                                    butt = 1;
                                } else {
                                    butt = MessageBox.Show("<b>" + wort + "</b>", enImageCode.Stift, "'" + wort + "' aufnehmen", "Ignorieren", "Beenden") + 1;
                                    // Egal, das wort ist eh schon ein "ToLower", also kann das tolower es gar nicht mehr ändern
                                }
                            }

                            switch (butt) {
                                case 0:
                                    WordAdd(wort);
                                    break;
                                case 1:
                                    WordAdd(wort.ToLower());
                                    break;
                                case 2:
                                    //  woEnd = woStart - 1
                                    break;
                                case 3:
                                    IsSpellChecking = false;
                                    return;
                            }
                        }
                    }
                } while (true);


                IsSpellChecking = false;
            }
        }

        public static void WordAdd(string Wort) {

            if (_DictWords.Row[Wort] != null) { _DictWords.Row.Remove(_DictWords.Row[Wort]); }

            _DictWords.Row.Add(Wort);
        }
    }
}
