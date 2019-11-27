﻿using System;
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

    public sealed class clsSerializeableMultiUserFile<T> : clsMultiUserFile where T : ILastSavedBy
    {


        private string _dataOnDisk = string.Empty;
        private bool _FirstLoad = true;
        private string _OtherUser = string.Empty;


        public T obj = default(T);



        public clsSerializeableMultiUserFile() : base(false)
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



            var Serializer = new XmlSerializer(typeof(T), string.Empty);
            var MemoryStream = new MemoryStream();
            var TextWriter = new XmlTextWriter(MemoryStream, System.Text.Encoding.UTF8);
            TextWriter.Indentation = 4;
            TextWriter.IndentChar = ' ';
            TextWriter.Formatting = Formatting.Indented;

            //var XmlNamespace = new XmlSerializerNamespaces();
            //XmlNamespace.Add(string.Empty, string.Empty);


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







        //private static void SerializeObject<U>(string FileName, U DataObject)
        //{
        //    var Serializer = new XmlSerializer(typeof(U));
        //    var FileStream = new FileStream(FileName, FileMode.Create);
        //    var TextWriter = new XmlTextWriter(FileStream, System.Text.Encoding.UTF8);
        //    TextWriter.Indentation = 4;
        //    TextWriter.IndentChar = ' ';
        //    TextWriter.Formatting = Formatting.Indented;

        //    Serializer.Serialize(TextWriter, DataObject);

        //    TextWriter.Close();
        //    FileStream.Close();
        //    FileStream.Dispose();

        //}

        //private static string SerializeObject<U>(U DataObject)
        //{

        //    string Result = null;
        //    var Serializer = new XmlSerializer(typeof(U), string.Empty);
        //    var MemoryStream = new MemoryStream();
        //    var TextWriter = new XmlTextWriter(MemoryStream, System.Text.Encoding.UTF8);
        //    TextWriter.Indentation = 4;
        //    TextWriter.IndentChar = ' ';
        //    TextWriter.Formatting = Formatting.Indented;

        //    var XmlNamespace = new XmlSerializerNamespaces();
        //    XmlNamespace.Add(string.Empty, string.Empty);

        //    Serializer.Serialize(TextWriter, DataObject, XmlNamespace);

        //    Result = System.Text.Encoding.UTF8.GetString(MemoryStream.ToArray());

        //    TextWriter.Close();
        //    MemoryStream.Close();
        //    MemoryStream.Dispose();

        //    return Result;

        //}

        //public static U DeserializeObject<U>(string FileName)
        //{

        //    var Result = default(U);
        //    var Serializer = new XmlSerializer(typeof(U));
        //    var FileStream = new FileStream(FileName, FileMode.Open, FileAccess.Read);
        //    var TextReader = new XmlTextReader(FileStream);

        //    Result = (U)Serializer.Deserialize(TextReader);

        //    TextReader.Close();
        //    FileStream.Close();
        //    FileStream.Dispose();

        //    return Result;

        //}



        //private static U DeserializeString<U>(string XmlContent)
        //{

        //    var Result = default(U);


        //    while (!XmlContent.StartsWith("<"))
        //    {
        //        XmlContent = XmlContent.Substring(1);
        //    }

        //    var Serializer = new XmlSerializer(typeof(U));
        //    var StringReader = new StringReader(XmlContent);

        //    Result = (U)Serializer.Deserialize(StringReader);

        //    StringReader.Close();
        //    StringReader.Dispose();

        //    return Result;

        //}


        ////https://www.jerriepelser.com/blog/deserialize-different-json-object-same-class/
        //public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        //{
        //    object instance = Activator.CreateInstance(objectType);
        //    var props = objectType.GetTypeInfo().DeclaredProperties.ToList();

        //    JObject jo = JObject.Load(reader);
        //    foreach (JProperty jp in jo.Properties())
        //    {
        //        if (!_propertyMappings.TryGetValue(jp.Name, out var name))
        //            name = jp.Name;

        //        PropertyInfo prop = props.FirstOrDefault(pi =>
        //            pi.CanWrite && pi.GetCustomAttribute<JsonPropertyAttribute>().PropertyName == name);

        //        prop?.SetValue(instance, jp.Value.ToObject(prop.PropertyType, serializer));
        //    }

        //    return instance;
        //}




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
                    Forms.MessageBox.Show("<b><u>Achtung:</u></b><br>Sie und '<b>" + obj.LastSavedBy + "</b>' bearbeiten<br>gerade gleichzeitig '<b>" + obj.Beschreibung +
                       "</b>'.<br><br>Bitte koordinieren sie die Bearbeitung - <br>Daten werden <u>nicht</u> automatisch zusammengefügt.", BlueBasics.Enums.enImageCode.Warnung, "OK");
                }
            }
        }
    }
}
