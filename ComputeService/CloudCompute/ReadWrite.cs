using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace CloudCompute
{
    // seralizeing/deserializeing object
    class ReadWrite
    {
        public void SerializeObject<T>(T serializableObject, string fileName)
        {
            if (serializableObject == null)
            {
                throw new ArgumentNullException("Argument serializable can't be null");
            }

            if (string.IsNullOrWhiteSpace(fileName))
            {
                throw new ArgumentNullException("Argument fileName can't be null or white space");
            }

            try
            {
                using (FileStream writer = new FileStream(fileName, FileMode.OpenOrCreate))
                {
                    DataContractSerializer ser = new DataContractSerializer(typeof(T));
                    ser.WriteObject(writer, serializableObject);
                }
            }
            catch (SerializationException ex)
            {
                Debug.WriteLine(ex.Message.ToString());
            }
        }

        public T DeSerializeObject<T>(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                throw new ArgumentNullException("Argument fileName can't be null or white space");
            }

            T objectOut = default(T);

            try
            {
                using (FileStream fs = new FileStream(fileName, FileMode.Open))
                {
                    XmlDictionaryReader reader = XmlDictionaryReader.CreateTextReader(fs, new XmlDictionaryReaderQuotas());
                    DataContractSerializer ser = new DataContractSerializer(typeof(T));
                    objectOut = (T)ser.ReadObject(reader, true);
                }
            }
            catch (SerializationException ex)
            {
                Debug.WriteLine(ex.Message.ToString());
            }

            return objectOut;
        }

        // starting data
        public void DataInput(ReadWrite readWrite)
        {
            var location = Environment.CurrentDirectory;
            string[] vs = location.Split(new string[] { "CloudCompute" }, StringSplitOptions.None);
            string exeLocation = String.Format(vs[0] + @"Client\bin\Debug\Client.exe");
            string dllLocation = String.Format(vs[0] + @"resources\read");
            string inputLocation = String.Format(vs[0] + @"resources\");
            var config = new Configuration() { InstaceCount = 2, Path = exeLocation, DllLocation = dllLocation, InputLocation = inputLocation };
            readWrite.SerializeObject(config, "ConfigFile.xml");
        }

    }
}
