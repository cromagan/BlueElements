using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System.Runtime.Serialization;
using BlueBasics.Interfaces;
using BlueBasics.EventArgs;
using BlueBasics.MultiUserFile;
using BlueBasics;

namespace BlueControls
{

    //https://social.msdn.microsoft.com/Forums/de-DE/68cdcd22-745e-49e8-8cad-dc7c3c7b8839/c-xml-datei-elemente-lesen-schreiben-ndern-erweitern
    ////https://www.jerriepelser.com/blog/deserialize-different-json-object-same-class/

    public sealed class clsSerializeableMultiUserFile<T> : clsMultiUserFile where T : ILastSavedBy
    {


        private string _dataOnDisk = string.Empty;
        private bool _FirstLoad = true;
        private string _OtherUser = string.Empty;


        public T obj = default(T);

        public event EventHandler MultipleUserDetected;


        public clsSerializeableMultiUserFile(bool readOnly, bool easymode) : base(readOnly, easymode)
        {
            obj = (T)Activator.CreateInstance(typeof(T));
        }


        protected override bool isSomethingDiscOperatingsBlocking()
        {
            return false;
        }

        protected override void CheckDataAfterReload()
        {

        }

        protected override bool IsFileAllowedToLoad(string fileName)
        {
            return true;
        }

        protected override void DoWorkInParallelBinSaverThread()
        {

        }

        protected override void DoWorkInSerialSavingThread()
        {

        }

        public override bool HasPendingChanges()
        {
            return _dataOnDisk != ToListOfByte(false).ToStringConvert();

        }

        protected override void ParseExternal(List<byte> bLoaded)
        {

            // https://stackoverflow.com/questions/2341566/deserializing-properties-into-a-pre-existing-object
            // https://stackoverflow.com/questions/2081612/net-determine-the-type-of-this-class-in-its-static-method

            var Result = default(T);
            var XmlContent = bLoaded.ToStringConvert(); // NICHT von UTF8 konvertieren, das macht der XML-Deserializer von alleine

            var _byteOrderMarkUtf8 = System.Text.Encoding.UTF8.GetString(System.Text.Encoding.UTF8.GetPreamble());
            if (XmlContent.StartsWith(_byteOrderMarkUtf8))
            {
                var lastIndexOfUtf8 = _byteOrderMarkUtf8.Length - 1;
                XmlContent = XmlContent.Remove(0, lastIndexOfUtf8);
            }



            while (!XmlContent.StartsWith("<"))
            {
                XmlContent = XmlContent.Substring(1);
            }


            // Bei Assistent für verwaltetes Debuggen "BindingFailure" :Fehler deaktiviren unw weiterlaufen lassen 
            // nn aufgrund der Sicherheitsebene ... Auf Pubilc setzern
            var Serializer = new XmlSerializer(typeof(T));
            var StringReader = new StringReader(XmlContent);

            Result = (T)Serializer.Deserialize(StringReader);

            StringReader.Close();
            StringReader.Dispose();

            var members = FormatterServices.GetSerializableMembers(typeof(T));
            FormatterServices.PopulateObjectMembers(obj, members, FormatterServices.GetObjectData(Result, members));
        }

        protected override void PrepeareDataForCheckingBeforeLoad()
        {

        }

        public override void RepairAfterParse()
        {

        }

        protected override List<byte> ToListOfByte(bool willSave)
        {

            if (willSave)
            {
                obj.LastSavedBy = modAllgemein.UserName();
            }


            // Bei Assistent für verwaltetes Debuggen "BindingFailure" :Fehler deaktiviren unw weiterlaufen lassen 
            // nn aufgrund der Sicherheitsebene ... Auf Pubilc setzern
            var Serializer = new XmlSerializer(typeof(T), string.Empty);
            var MemoryStream = new MemoryStream();
            var TextWriter = new XmlTextWriter(MemoryStream, System.Text.Encoding.UTF8);
            TextWriter.Indentation = 4;
            TextWriter.IndentChar = ' ';
            TextWriter.Formatting = Formatting.Indented;

            Serializer.Serialize(TextWriter, obj);

            var Result = System.Text.Encoding.UTF8.GetString(MemoryStream.ToArray()).ToByteList(); // NICHT nach UTF8 konvertieren, das machte der XML-Serializer bereits von alleine



            TextWriter.Close();
            MemoryStream.Close();
            MemoryStream.Dispose();


            //ParseExternal(Result);

            return Result;

        }

        protected override void StartBackgroundWorker()
        {

        }

        protected override bool IsBackgroundWorkerBusy()
        {
            return false;
        }

        protected override void CancelBackGroundWorker()
        {

        }

        protected override bool IsThereBackgroundWorkToDo(bool mustSave)
        {
            return false;
        }

        protected override void ThisIsOnDisk(List<byte> binaryData)
        {
            _dataOnDisk = binaryData.ToStringConvert();
        }







 



        protected override void OnLoaded(LoadedEventArgs e)
        {
            base.OnLoaded(e);

            if (_FirstLoad)
            {
                _FirstLoad = false;
                return;
            }

            if (obj.LastSavedBy != modAllgemein.UserName())
            {


                if (_OtherUser != obj.LastSavedBy)
                {
                    _OtherUser = obj.LastSavedBy;
                    OnMultipleUserDetected();
                    Forms.MessageBox.Show("<b><u>Achtung:</u></b><br>Sie und '<b>" + obj.LastSavedBy + "</b>' bearbeiten<br>gerade gleichzeitig '<b>" + obj.Beschreibung +
                       "</b>'.<br><br>Bitte koordinieren sie die Bearbeitung - <br>Daten werden <u>nicht</u> automatisch zusammengefügt.", BlueBasics.Enums.enImageCode.Warnung, "OK");
                }
            }
        }

        private void OnMultipleUserDetected()
        {
            MultipleUserDetected?.Invoke(this, System.EventArgs.Empty);
        }
    }
}
