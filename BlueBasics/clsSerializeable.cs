using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System.Runtime.Serialization;
using System.Reflection;

namespace BlueBasics
{

    //https://social.msdn.microsoft.com/Forums/de-DE/68cdcd22-745e-49e8-8cad-dc7c3c7b8839/c-xml-datei-elemente-lesen-schreiben-ndern-erweitern

    public class clsSerializeable<T> : clsMultiUserFile
    {


        public T obj = default(T);


        public clsSerializeable() : base(false)
        {
            obj = (T)Activator.CreateInstance(typeof(T));
        }

        protected override bool SomethingBlocking()
        {
            return false;
        }

        protected override void CheckDataAfterReload()
        {

        }

        protected override void CheckFileWillBeLoadedErrors(string fileName)
        {

        }

        protected override void DoWorkInParallelBinSaverThread()
        {

        }

        protected override void DoWorkInSerialSavingThread()
        {

        }

        public override bool HasPendingChanges()
        {
            throw new NotImplementedException();
        }

        protected override void ParseExternal(List<byte> bLoaded)
        {

            //        https://stackoverflow.com/questions/2341566/deserializing-properties-into-a-pre-existing-object

            //  var tx = this.GetType();
            var x = DeserializeString<T>(bLoaded.ToStringConvertUTF8());


            // var t = MethodBase.GetCurrentMethod().DeclaringType; // https://stackoverflow.com/questions/2081612/net-determine-the-type-of-this-class-in-its-static-method
            var members = FormatterServices.GetSerializableMembers(typeof(T)); // typeof(clsSerializeable)
            FormatterServices.PopulateObjectMembers(obj, members, FormatterServices.GetObjectData(x, members));

            // Object state is back
            //  Console.WriteLine("{0}, {1}", book.Title, book.Author);

        }

        protected override void PrepeareDataForCheckingBeforeLoad()
        {

        }

        public override void RepairAfterParse()
        {

        }

        protected override List<byte> ToListOfByte()
        {
            throw new NotImplementedException();
        }

        protected override void StartBackgroundWorker()
        {

        }

        protected override bool IsBackgroundWorkerBusy()
        {
            return false;
        }

        protected override void CancelBackGroundWork()
        {

        }

        protected override bool IsThereBackgroundWorkToDo(bool mustSave)
        {
            return false;
        }


        private static void SerializeObject<U>(string FileName, U DataObject)
        {
            var Serializer = new XmlSerializer(typeof(U));
            var FileStream = new FileStream(FileName, FileMode.Create);
            var TextWriter = new XmlTextWriter(FileStream, System.Text.Encoding.UTF8);
            TextWriter.Indentation = 4;
            TextWriter.IndentChar = ' ';
            TextWriter.Formatting = Formatting.Indented;

            Serializer.Serialize(TextWriter, DataObject);

            TextWriter.Close();
            FileStream.Close();
            FileStream.Dispose();

        }

        private static string SerializeObject<U>(U DataObject)
        {

            string Result = null;
            var Serializer = new XmlSerializer(typeof(U), string.Empty);
            var MemoryStream = new MemoryStream();
            var TextWriter = new XmlTextWriter(MemoryStream, System.Text.Encoding.UTF8);
            TextWriter.Indentation = 4;
            TextWriter.IndentChar = ' ';
            TextWriter.Formatting = Formatting.Indented;

            var XmlNamespace = new XmlSerializerNamespaces();
            XmlNamespace.Add(string.Empty, string.Empty);

            Serializer.Serialize(TextWriter, DataObject, XmlNamespace);

            Result = System.Text.Encoding.UTF8.GetString(MemoryStream.ToArray());

            TextWriter.Close();
            MemoryStream.Close();
            MemoryStream.Dispose();

            return Result;

        }

        public static U DeserializeObject<U>(string FileName)
        {

            var Result = default(U);
            var Serializer = new XmlSerializer(typeof(U));
            var FileStream = new FileStream(FileName, FileMode.Open, FileAccess.Read);
            var TextReader = new XmlTextReader(FileStream);

            Result = (U)Serializer.Deserialize(TextReader);

            TextReader.Close();
            FileStream.Close();
            FileStream.Dispose();

            return Result;

        }



        private static U DeserializeString<U>(string XmlContent)
        {

            var Result = default(U);


            while (!XmlContent.StartsWith("<"))
            {
                XmlContent = XmlContent.Substring(1);
            }

            var Serializer = new XmlSerializer(typeof(U));
            var StringReader = new StringReader(XmlContent);

            Result = (U)Serializer.Deserialize(StringReader);

            StringReader.Close();
            StringReader.Dispose();

            return Result;

        }


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


    }
}
