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
using BlueBasics.Interfaces;
using System;
using System.ComponentModel;

namespace BlueControls.Classes_Editor
{
    [DefaultEvent("Changed")]
    internal partial class AbstractClassEditor<T> : BlueControls.Controls.GroupBox where T : IParseable
    {

        public AbstractClassEditor() : base()
        {
            InitializeComponent();
        }

        private T _Item;
        private string _LastState = string.Empty;
        private bool _VisibleChanged_Done;

        public event EventHandler Changed;



        /// <summary>
        /// Das Objekt, das im Original bearbeitet wird.
        /// </summary>
        /// <returns></returns>
        [DefaultValue(null)]
        internal T Item
        {
            get => _Item;
            set
            {
                _Item = value;

                if (_Item != null)
                {
                    _LastState = _Item.ToString();
                    if (!Inited)
                    {
                        Inited = true;
                        PrepaireFormula();
                    }
                    IsFilling = true;
                    EnabledAndFillFormula();
                    IsFilling = false;
                }
                else
                {
                    _LastState = string.Empty;
                    IsFilling = true;
                    DisableAndClearFormula();
                    IsFilling = false;
                }
            }
        }



        public bool IsFilling { get; private set; }
        public bool Inited { get; private set; }

        /// <summary>
        /// Sperrt die komplette Bearbeitung des Formulars und löscht alle Einträge.
        /// Typischerweiße, wenn das zu bearbeitende Objekt 'null' ist oder beim erstmaligen Initialiseren des Steuerelementes.
        /// </summary>
        protected virtual void DisableAndClearFormula()
        {
            Develop.DebugPrint_RoutineMussUeberschriebenWerden();
        }

        /// <summary>
        /// Erlaubt die Bearbeitung des Objektes und füllt den aktuellen Zustand in das Formular.
        /// </summary>
        protected virtual void EnabledAndFillFormula()
        {
            Develop.DebugPrint_RoutineMussUeberschriebenWerden();
        }

        /// <summary>
        /// Bereitet das Formular vor. Z.B. werden in den Auswahldialog-Boxen die voreingestellten Werte hineingeschrieben.
        /// Diese Routine wird aufgerufen, wenn das Item zum ersten Mal empfangen wurde.
        /// </summary>
        protected virtual void PrepaireFormula()
        {
            Develop.DebugPrint_RoutineMussUeberschriebenWerden();
        }


        protected void OnChanged(T Obj)
        {
            if (IsFilling) { return; }

            var newstatse = Obj.ToString();

            if (newstatse == _LastState) { return; }

            _LastState = newstatse;

            Changed?.Invoke(this, System.EventArgs.Empty);
        }


        protected override void OnVisibleChanged(System.EventArgs e)
        {
            base.OnVisibleChanged(e);

            // Damit das Formular nach der Anzeige erstmal deaktiviert ist.


            if (_VisibleChanged_Done) { return; }
            _VisibleChanged_Done = true;


            if (_Item == null)
            {
                IsFilling = true;
                DisableAndClearFormula();
                IsFilling = false;
            }
        }
    }
}
