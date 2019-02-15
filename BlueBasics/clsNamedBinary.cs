using System;
using System.Drawing;
using System.Drawing.Imaging;
using BlueBasics.Enums;
using BlueBasics.Interfaces;
using static BlueBasics.FileOperations;

namespace BlueBasics
{
    public sealed class clsNamedBinary : ICanBeEmpty, IParseable
    {
        #region  Variablen-Deklarationen 

        private string _binary;
        private Bitmap _picture;
        private string _name;

        #endregion

        #region  Event-Deklarationen + Delegaten 
        public event EventHandler Changed;
        #endregion

        #region  Construktor + Initialize 

        public clsNamedBinary(string CodeToParse)
        {
            Parse(CodeToParse);
        }

        public clsNamedBinary(string vName, Bitmap BMP)
        {
            Initialize();
            _name = vName;
            _picture = BMP;
        }

        public clsNamedBinary()
        {
            Initialize();
        }


        public clsNamedBinary(string vName, string Bins)
        {
            Initialize();
            _name = vName;
            _binary = Bins;
        }

        public void Initialize()
        {
            _name = string.Empty;
            _binary = string.Empty;
            _picture = null;
        }

        #endregion

        #region  Properties 
        public bool IsParsing { get; private set; }

        public string Binary
        {
            get
            {
                return _binary;
            }

            set
            {
                if (_binary == value) { return; }
                _binary = value;
                OnChanged();
            }
        }

        public Bitmap Picture
        {
            get
            {
                return _picture;
            }

            set
            {
                if (_picture == value) { return; }
                _picture = value;
                OnChanged();
            }
        }

        public string Name
        {
            get
            {
                return _name;
            }

            set
            {
                if (_name == value) { return; }
                _name = value;
                OnChanged();
            }
        }
        #endregion


        public void LoadFromFile(string FileName)
        {


            if (!FileExists(FileName))
            {
                Develop.DebugPrint(enFehlerArt.Fehler, "Datei Existiert nicht");
                return;
            }

            _binary = string.Empty;
            _picture = null;

            if (FileName.FileType() == enFileFormat.Image)
            {
                _picture = (Bitmap)modAllgemein.Image_FromFile(FileName);

            }
            else
            {

                _binary = modConverter.FileToString(FileName);

            }

            _name = FileName.FileNameWithSuffix();
        }



        public bool IsNullOrEmpty()
        {
            if (_picture == null && string.IsNullOrEmpty(_binary)) { return true; }
            return false;
        }


        public void Parse(string StringToParse)
        {
            IsParsing = true;
            Initialize();

            if (string.IsNullOrEmpty(StringToParse) || StringToParse.Length < 10)
            {
                IsParsing = false;
                return;
            }

            if (!StringToParse.StartsWith("{"))
            {
                ParseOldFormat(StringToParse);
                IsParsing = false;
                return;
            }


            foreach (var pair in StringToParse.GetAllTags())
            {
                switch (pair.Key)
                {
                    case "name":
                        _name = pair.Value.FromNonCritical();
                        break;
                    case "png":
                        _picture = modConverter.StringToBitmap(pair.Value.FromNonCritical());
                        break;
                    case "bin":
                        _binary = pair.Value.FromNonCritical();
                        break;
                    default:
                        Develop.DebugPrint(enFehlerArt.Fehler, "Tag unbekannt: " + pair.Key);
                        break;
                }
            }
            IsParsing = false;
        }

        private void ParseOldFormat(string Value)
        {

            Develop.DebugPrint_NichtImplementiert();   // Todo: Diese Routine löschen
            if (Value.Substring(1, 3) == "PNG")
            {
                _picture = modConverter.StringToBitmap(Value);
                return;
            }

            switch (Value.Substring(0, 9))
            {
                case "NamedPNG#":
                {
                    var x = Value.IndexOf("\r");
                    _name = Value.Substring(9, x - 9);
                    _picture = modConverter.StringToBitmap(Value.Substring(x + 1));
                    return;
                }
                case "NamedBIN#":
                {
                    var x = Value.IndexOf("\r");
                    _name = Value.Substring(9, x - 9);
                    _binary = Value.Substring(x + 1);
                    return;
                }
                default:
                    Develop.DebugPrint(enFehlerArt.Fehler, "Kann nicht geparsed werden: " + Value.Substring(0, 20));
                    break;
            }
        }


        public override string ToString()
        {

            var t = "{Name=" + _name.ToNonCritical() + ", ";


            if (_picture != null)
            {
                t = t + "PNG=" + modConverter.BitmapToString(_picture, ImageFormat.Png).ToNonCritical() + ", ";
            }

            if (!string.IsNullOrEmpty(_binary))
            {
                t = t + "BIN=" + _binary.ToNonCritical() + ", ";
            }

            return t.TrimEnd(", ") + "}";
        }


        public void OnChanged()
        {
            if (IsParsing) { Develop.DebugPrint(enFehlerArt.Warnung, "Falscher Parsing Zugriff!"); return; }
            Changed?.Invoke(this, System.EventArgs.Empty);
        }

    }

}