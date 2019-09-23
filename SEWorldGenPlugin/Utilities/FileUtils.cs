using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEWorldGenPlugin.Utilities
{
    using Sandbox.Game.World;
    using System.IO;
    using System.Xml;
    using System.Xml.Serialization;
    using VRage.FileSystem;
    using VRage.Utils;

    namespace SEWorldGenPlugin.Utilities
    {
        /*
         * Taken form Keen Github, since ModAPI is not available at Plugin init, but methods are needed 
         */
        public class FileUtils
        {
            private const string STORAGE_FOLDER = "Storage";

            private static string StripDllExtIfNecessary(string name)
            {
                string ext = ".dll";
                if (name.EndsWith(ext, StringComparison.InvariantCultureIgnoreCase))
                {
                    return name.Substring(0, name.Length - ext.Length);
                }
                return name;
            }

            public static bool FileExistsInWorldStorage(string file, Type callingType)
            {
                if (file.IndexOfAny(Path.GetInvalidFileNameChars()) != -1)
                {
                    return false;
                }

                var path = Path.Combine(MySession.Static.CurrentPath, STORAGE_FOLDER, StripDllExtIfNecessary(callingType.Assembly.ManifestModule.ScopeName), file);

                return File.Exists(path);
            }

            public static bool FileExistsInPath(string path, string file, Type callingType)
            {
                if (file.IndexOfAny(Path.GetInvalidFileNameChars()) != -1)
                {
                    return false;
                }

                var paths = Path.Combine(path, STORAGE_FOLDER, StripDllExtIfNecessary(callingType.Assembly.ManifestModule.ScopeName), file);
                MyLog.Default.WriteLine(paths);
                return File.Exists(paths);
            }

            public static bool FileExistsInGlobalStorage(string file)
            {
                if (file.IndexOfAny(Path.GetInvalidFileNameChars()) != -1)
                    return false;

                var path = Path.Combine(MyFileSystem.UserDataPath, STORAGE_FOLDER, file);
                return File.Exists(path);
            }

            public static void DeleteFileInWorldStorage(string file, Type callingType)
            {
                if (FileExistsInWorldStorage(file, callingType))
                {
                    var path = Path.Combine(MySession.Static.CurrentPath, STORAGE_FOLDER, StripDllExtIfNecessary(callingType.Assembly.ManifestModule.ScopeName), file);
                    File.Delete(path);
                }
            }

            public static void DeleteFileInPath(string path, string file, Type callingType)
            {
                if (FileExistsInPath(path, file, callingType))
                {
                    var paths = Path.Combine(path, STORAGE_FOLDER, StripDllExtIfNecessary(callingType.Assembly.ManifestModule.ScopeName), file);
                    File.Delete(paths);
                }
            }

            public static void DeleteFileInGlobalStorage(string file)
            {
                if (FileExistsInGlobalStorage(file))
                {
                    var path = Path.Combine(MyFileSystem.UserDataPath, STORAGE_FOLDER, file);
                    File.Delete(path);
                }
            }

            public static TextReader ReadFileInWorldStorage(string file, Type callingType)
            {
                if (file.IndexOfAny(Path.GetInvalidFileNameChars()) != -1)
                {
                    throw new FileNotFoundException();
                }
                var path = Path.Combine(MySession.Static.CurrentPath, STORAGE_FOLDER, StripDllExtIfNecessary(callingType.Assembly.ManifestModule.ScopeName), file);
                var stream = MyFileSystem.OpenRead(path);
                if (stream != null)
                {
                    return new StreamReader(stream);
                }
                throw new FileNotFoundException();
            }

            public static TextReader ReadFileInPath(string path, string file, Type callingType)
            {
                if (file.IndexOfAny(Path.GetInvalidFileNameChars()) != -1)
                {
                    throw new FileNotFoundException();
                }
                var paths = Path.Combine(path, STORAGE_FOLDER, StripDllExtIfNecessary(callingType.Assembly.ManifestModule.ScopeName), file);
                var stream = MyFileSystem.OpenRead(paths);
                if (stream != null)
                {
                    return new StreamReader(stream);
                }
                throw new FileNotFoundException();
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

            public static TextWriter WriteFileInWorldStorage(string file, Type callingType)
            {
                if (file.IndexOfAny(Path.GetInvalidFileNameChars()) != -1)
                {
                    throw new FileNotFoundException();
                }

                var path = Path.Combine(MySession.Static.CurrentPath, STORAGE_FOLDER, StripDllExtIfNecessary(callingType.Assembly.ManifestModule.ScopeName), file);
                var stream = MyFileSystem.OpenWrite(path);
                if (stream != null)
                {
                    return new StreamWriter(stream);
                }
                throw new FileNotFoundException();
            }

            public static TextWriter WriteFileInPath(string path, string file, Type callingType)
            {
                if (file.IndexOfAny(Path.GetInvalidFileNameChars()) != -1)
                {
                    throw new FileNotFoundException();
                }

                var paths = Path.Combine(path, STORAGE_FOLDER, StripDllExtIfNecessary(callingType.Assembly.ManifestModule.ScopeName), file);
                var stream = MyFileSystem.OpenWrite(paths);
                if (stream != null)
                {
                    return new StreamWriter(stream);
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

            public static T ReadXmlFileFromWorld<T>(string file, Type callingType)
            {
                if(FileExistsInWorldStorage(file, callingType))
                {
                    try
                    {
                        using(var reader = ReadFileInWorldStorage(file, callingType))
                        {
                            T saveFile = SerializeFromXml<T>(reader.ReadToEnd());
                            return saveFile;
                        }
                    }
                    catch(Exception e)
                    {
                        MyLog.Default.Error("Couldnt load save file.");
                        MyLog.Default.Error(e.Message + "\n" + e.StackTrace);
                        DeleteFileInWorldStorage(file, callingType);
                        return default(T);
                    }
                }
                return default(T);
            }

            public static void WriteXmlFileToWorld<T>(T saveFile, string file, Type callingType)
            {
                DeleteFileInWorldStorage(file, callingType);
                string xml = SerializeToXml<T>(saveFile);
                using(var writer = WriteFileInWorldStorage(file, callingType))
                {
                    writer.Write(xml);
                    writer.Close();
                }
            }

            public static T ReadXmlFileFromPath<T>(string path, string file, Type callingType)
            {
                MyLog.Default.WriteLine(path + " PAth");
                if(FileExistsInPath(path, file, callingType))
                {
                    try
                    {
                        using(var reader = ReadFileInPath(path, file, callingType))
                        {
                            T saveFile = SerializeFromXml<T>(reader.ReadToEnd());
                            return saveFile;
                        }
                    }
                    catch(Exception e)
                    {
                        MyLog.Default.Error("Couldnt load save file.");
                        MyLog.Default.Error(e.Message + "\n" + e.StackTrace);
                        DeleteFileInPath(path, file, callingType);
                        return default(T);
                    }
                }
                return default(T);
            }

            public static void WriteXmlFileToPath<T>(T saveFile, string path, string file, Type callingType)
            {
                DeleteFileInPath(path, file, callingType);
                string xml = SerializeToXml<T>(saveFile);
                using(var writer = WriteFileInPath(path, file, callingType))
                {
                    writer.Write(xml);
                    writer.Close();
                }
            }
        }
    }
}
