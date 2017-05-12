using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace GuetSample
{
    public class SerializeHelper
    {
        public static bool Serialize(string path, object obj)
        {
            FileInfo fileInfo = new FileInfo(path);
            if (!fileInfo.Directory.Exists)
            {
                try
                {
                    fileInfo.Directory.Create();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                    return false;
                }
            }
            using (FileStream stream = fileInfo.Open(FileMode.OpenOrCreate))
            {
                BinaryFormatter bFormat = new BinaryFormatter();
                bFormat.Serialize(stream, obj);
            }
            return true;
        }

        public static bool Serialize(string dir, string fileName, object obj)
        {
            return Serialize(dir + "/" + fileName, obj);
        }

        public static bool SerializeDefault(string fileName, object obj)
        {
            string dir = AppDomain.CurrentDomain.BaseDirectory + "storeddata";
            return Serialize(dir, fileName, obj);
        }

        public static object Deserialize(string path)
        {
            if (!File.Exists(path)) return null;
            using (FileStream stream = new FileStream(path, FileMode.Open))
            {
                if (stream != null )
                {
                    BinaryFormatter bFormat = new BinaryFormatter();
                    return bFormat.Deserialize(stream);
                }
            }
            return null;
        }

        public static object Deserialize(string dir, string fileName)
        {
            string path = dir + "/" + fileName;
            return Deserialize(path);
        }

        public static object DeserializeDefault(string fileName)
        {
            string dir = AppDomain.CurrentDomain.BaseDirectory + "storeddata";
            if (!Directory.Exists(dir))
            {
                return null;
            }
            return Deserialize(dir, fileName);
        }
    }
}
