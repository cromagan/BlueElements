﻿using System;
using System.ComponentModel;
using BlueBasics;
using BlueBasics.Enums;
using BlueBasics.Interfaces;

namespace BlueControls.Classes_Editor
{
    [DefaultEvent("Changed")]
    internal abstract partial class AbstractClassEditor
    {


        public AbstractClassEditor()
        {
            InitializeComponent();
        }

        private IParseable _Object;
        private bool _Inited;
        private bool _IsFilling;
        private bool _IsDialog;
        private string _LastState = string.Empty;
        private bool _VisibleChanged_Done;

        public event EventHandler Changed;



        /// <summary>
        /// Das Objekt, das je mach Modus entweder geklont (Dialog-Modus) oder im Original (also Control in einer Form) bearbeitet wird.
        /// </summary>
        /// <returns></returns>
        [DefaultValue((IParseable)null)]
        internal IParseable ObjectWithDialog
        {
            get
            {

                return _Object;
            }
            set
            {
                if (_IsDialog)
                {
                    // TODO: ICloneable eleminieren und testen
                    _Object = (IParseable)((ICloneable)value).Clone();
                }
                else
                {
                    _Object = value;
                }

                ConvertObject(_Object);

                if (_Object != null)
                {
                    _LastState = _Object.ToString();
                    if (!_Inited)
                    {
                        _Inited = true;
                        PrepaireFormula();
                    }
                    _IsFilling = true;
                    EnabledAndFillFormula();
                    _IsFilling = false;
                }
                else
                {
                    _LastState = "";
                    _IsFilling = true;
                    DisableAndClearFormula();
                    _IsFilling = false;
                }

            }
        }


        public bool IsDialog
        {
            get
            {
                return _IsDialog;
            }
            set
            {
                if (_Object != null) { Develop.DebugPrint(enFehlerArt.Fehler, "Objekt bereits vorhanden"); }
                _IsDialog = value;
            }
        }


        public bool IsFilling()
        {
            return _IsFilling;
        }

        /// <summary>
        /// Sperrt die komplette Bearbeitung des Formulars und löscht alle Einträge.
        /// Typischerweiße, wenn das zu bearbeitende Objekt 'null' ist oder beim erstmaligen Initialiseren des Steuerelementes.
        /// </summary>
        protected abstract void DisableAndClearFormula();

        /// <summary>
        /// Erlaubt die Bearbeitung des Objektes und füllt den aktuellen Zustand in das Formular.
        /// </summary>
        protected abstract void EnabledAndFillFormula();

        /// <summary>
        /// Bereitet das Formular vor. Z.B. werden in den Auswahldialog-Boxen die voreingestellten Werte hineingeschrieben.
        /// Diese Routine wird aufgerufen, wenn das Object zum ersten Mal konvertiert wurde. Siehe "ConvertObject"
        /// </summary>
        protected abstract void PrepaireFormula();

        /// <summary>
        /// Das ObjectParsable wurde gesetzt und nun soll die ageleitete Klasse das Objekt für seinen Gebrauch richtig konvertieren.
        /// </summary>
        protected abstract void ConvertObject(object ThisObject);


        protected void OnChanged(IParseable Obj)
        {
            if (IsFilling()) { return; }

            var newstatse = Obj.ToString();

            if (newstatse == _LastState) { return; }

            _LastState = newstatse;

            Changed?.Invoke(this, System.EventArgs.Empty);
        }


        protected override void OnVisibleChanged(System.EventArgs e)
        {
            base.OnVisibleChanged(e);

            if (_VisibleChanged_Done) { return; }
            _VisibleChanged_Done = true;

            _IsFilling = true;
            if (_Object == null) { DisableAndClearFormula(); }
            _IsFilling = false;

        }
    }
}
