using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEWorldGenPlugin.Utilities
{
    using System.IO;
    using System.Xml;
    using System.Xml.Serialization;
    using VRage.FileSystem;

    namespace SEWorldGenPlugin.Utilities
    {
        public class FileUtils
        {
            private const string STORAGE_FOLDER = "Storage";

            public static bool FileExistsInGlobalStorage(string file)
            {
                if (file.IndexOfAny(Path.GetInvalidFileNameChars()) != -1)
                    return false;

                var path = Path.Combine(MyFileSystem.UserDataPath, STORAGE_FOLDER, file);
                return File.Exists(path);
            }

            public static void DeleteFileInGlobalStorage(string file)
            {
                if (FileExistsInGlobalStorage(file))
                {
                    var path = Path.Combine(MyFileSystem.UserDataPath, STORAGE_FOLDER, file);
                    File.Delete(path);
                }
            }

            public static TextReader ReadFileInGlobalStorage(string file)
            {
                if (file.IndexOfAny(Path.GetInvalidFileNameChars()) != -1)
                    throw new FileNotFoundException();

                var path = Path.Combine(MyFileSystem.UserDataPath, STORAGE_FOLDER, file);
                var stream = MyFileSystem.OpenRead(path);
                if (stream != null)
                {
                    return new StreamReader(stream);
                }
                throw new FileNotFoundException();
            }

            public static TextWriter WriteFileInGlobalStorage(string file)
            {
                if (file.IndexOfAny(Path.GetInvalidFileNameChars()) != -1)
                    throw new FileNotFoundException();

                var path = Path.Combine(MyFileSystem.UserDataPath, STORAGE_FOLDER, file);
                var stream = MyFileSystem.OpenWrite(path);
                if (stream != null)
                {
                    return new StreamWriter(stream);
                }
                throw new FileNotFoundException();
            }

            public static string SerializeToXml<T>(T objToSerialize)
            {
                System.Xml.Serialization.XmlSerializer x = new System.Xml.Serialization.XmlSerializer(objToSerialize.GetType());
                StringWriter textWriter = new StringWriter();
                x.Serialize(textWriter, objToSerialize);
                return textWriter.ToString();
            }

            public static T SerializeFromXml<T>(string xml)
            {
                if (string.IsNullOrEmpty(xml))
                {
                    return default(T);
                }

                XmlSerializer serializer = new XmlSerializer(typeof(T));

                using (StringReader textReader = new StringReader(xml))
                {
                    using (XmlReader xmlReader = XmlReader.Create(textReader))
                    {
                        return (T)serializer.Deserialize(xmlReader);
                    }
                }
            }

        }
    }
}
