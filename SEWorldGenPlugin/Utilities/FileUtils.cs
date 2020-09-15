using Sandbox.Game.World;
using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using VRage.FileSystem;

namespace SEWorldGenPlugin.Utilities
{
    /*
     * Taken form Keen Github, since ModAPI is not available at Plugin init, but methods are needed 
     */

    /// <summary>
    /// This class manages file i/o of the plugin. The code is primarily taken from keens github page (https://github.com/KeenSoftwareHouse/SpaceEngineers)
    /// since they already implemented all those methods in their mod api. However, this api is not available at plugin initialization, so I needed to implement
    /// it again.
    /// </summary>
    public class FileUtils
    {
        /// <summary>
        /// Storage folder name in the world and SE data folder.
        /// </summary>
        private const string STORAGE_FOLDER = "Storage";

        /// <summary>
        /// Removes .dll from a file name if it needs to.
        /// </summary>
        /// <param name="name">Name of the dll file</param>
        /// <returns>DLL file name without extension.</returns>
        private static string StripDllExtIfNecessary(string name)
        {
            string ext = ".dll";
            if (name.EndsWith(ext, StringComparison.InvariantCultureIgnoreCase))
            {
                return name.Substring(0, name.Length - ext.Length);
            }
            return name;
        }

        /// <summary>
        /// Checks if a file with the given name exists in the world folder. The calling type is used
        /// to get the assembly name to get the sub directory in the storage folder, where files for the dll are stored.
        /// </summary>
        /// <param name="file">File name</param>
        /// <param name="callingType">Calling type</param>
        /// <returns></returns>
        public static bool FileExistsInWorldStorage(string file, Type callingType)
        {
            if (file.IndexOfAny(Path.GetInvalidFileNameChars()) != -1)
            {
                return false;
            }

            var path = Path.Combine(MySession.Static.CurrentPath, STORAGE_FOLDER, StripDllExtIfNecessary(callingType.Assembly.ManifestModule.ScopeName), file);

            return File.Exists(path);
        }

        /// <summary>
        /// Checks whether a file exists in a given paths storage folder or not. The path gets the storage folder and subfolder appended.
        /// The calling type is used to get the assembly name to get the sub directory in the storage folder, where files for the dll are stored.
        /// </summary>
        /// <param name="path">Path to the storage folder</param>
        /// <param name="file">Name of the file</param>
        /// <param name="callingType">Calling type</param>
        /// <returns>True if it exists</returns>
        public static bool FileExistsInPath(string path, string file, Type callingType)
        {
            if (file.IndexOfAny(Path.GetInvalidFileNameChars()) != -1)
            {
                return false;
            }

            var paths = Path.Combine(path, STORAGE_FOLDER, StripDllExtIfNecessary(callingType.Assembly.ManifestModule.ScopeName), file);
            return File.Exists(paths);
        }

        /// <summary>
        /// Checks if a file exists in the global storage of SE. This is located in the SE data folder.
        /// </summary>
        /// <param name="file">Name of the file</param>
        /// <returns>If it exists, true</returns>
        public static bool FileExistsInGlobalStorage(string file)
        {
            if (file.IndexOfAny(Path.GetInvalidFileNameChars()) != -1)
                return false;

            var path = Path.Combine(MyFileSystem.UserDataPath, STORAGE_FOLDER, file);
            return File.Exists(path);
        }

        /// <summary>
        /// Deletes a file in the storage folder of the current world. The calling type is used
        /// to get the assembly name to get the sub directory in the storage folder, where files for the dll are stored.
        /// </summary>
        /// <param name="file">File name</param>
        /// <param name="callingType">Calling type</param>
        public static void DeleteFileInWorldStorage(string file, Type callingType)
        {
            if (FileExistsInWorldStorage(file, callingType))
            {
                var path = Path.Combine(MySession.Static.CurrentPath, STORAGE_FOLDER, StripDllExtIfNecessary(callingType.Assembly.ManifestModule.ScopeName), file);
                File.Delete(path);
            }
        }

        /// <summary>
        /// Deletes a file at the paths storage folder. The calling type is used
        /// to get the assembly name to get the sub directory in the storage folder, where files for the dll are stored.
        /// </summary>
        /// <param name="path">Path to the storage folder</param>
        /// <param name="file">File name</param>
        /// <param name="callingType">Calling type</param>
        public static void DeleteFileInPath(string path, string file, Type callingType)
        {
            if (FileExistsInPath(path, file, callingType))
            {
                var paths = Path.Combine(path, STORAGE_FOLDER, StripDllExtIfNecessary(callingType.Assembly.ManifestModule.ScopeName), file);
                File.Delete(paths);
            }
        }

        /// <summary>
        /// Deletes a file in the global storage folder of SE. It is located in the SE data folder
        /// </summary>
        /// <param name="file">File name</param>
        public static void DeleteFileInGlobalStorage(string file)
        {
            if (FileExistsInGlobalStorage(file))
            {
                var path = Path.Combine(MyFileSystem.UserDataPath, STORAGE_FOLDER, file);
                File.Delete(path);
            }
        }

        /// <summary>
        /// Reads the file contents of a file in the current worlds storage folder. The calling type is used
        /// to get the assembly name to get the sub directory in the storage folder, where files for the dll are stored.
        /// </summary>
        /// <param name="file">File name</param>
        /// <param name="callingType">Calling type</param>
        /// <returns>A TextReader of the opened file</returns>
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

        /// <summary>
        /// Reads a file in the storage folder at the given path. The calling type is used
        /// to get the assembly name to get the sub directory in the storage folder, where files for the dll are stored.
        /// The calling type is used to get the assembly name to get the sub directory in the storage folder, where files for the dll are stored.
        /// </summary>
        /// <param name="path">Path to the storage folder</param>
        /// <param name="file">File name</param>
        /// <param name="callingType">Dalling type</param>
        /// <returns>Returns a TextReader of the read file</returns>
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

        /// <summary>
        /// Reads a file in the storage folder located at the SE data folder.
        /// </summary>
        /// <param name="file">Name of the file</param>
        /// <returns>A TextReader to read the file</returns>
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

        /// <summary>
        /// Writes to a file in the current worlds storage folder. 
        /// If the file does not exist, it will be created. The calling type is used
        /// to get the assembly name to get the sub directory in the storage folder, 
        /// where files for the dll are stored. 
        /// </summary>
        /// <param name="file">File name</param>
        /// <param name="callingType">Calling type</param>
        /// <returns>A TextWriter, to write to the file</returns>
        public static TextWriter WriteFileInWorldStorage(string file, Type callingType)
        {
            if (file.IndexOfAny(Path.GetInvalidFileNameChars()) != -1)
            {
                throw new FileNotFoundException();
            }
            if (!MySession.Static.CurrentPath.ToLower().Contains(MyFileSystem.UserDataPath.ToLower()))
            {
                return null;
            }

            var path = Path.Combine(MySession.Static.CurrentPath, STORAGE_FOLDER, StripDllExtIfNecessary(callingType.Assembly.ManifestModule.ScopeName), file);
            var stream = MyFileSystem.OpenWrite(path);
            if (stream != null)
            {
                return new StreamWriter(stream);
            }
            throw new FileNotFoundException();
        }

        /// <summary>
        /// Writes to a file located in the storage folder at path. The calling type is used
        /// to get the assembly name to get the sub directory in the storage folder, where files for the dll are stored.
        /// </summary>
        /// <param name="path">Path to the storage folder</param>
        /// <param name="file">File name</param>
        /// <param name="callingType">Calling type</param>
        /// <returns>A TextWriter to write to the file</returns>
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

        /// <summary>
        /// Writes to a file in the global storage folder of SE located at SE data folder.
        /// </summary>
        /// <param name="file">Name of the file</param>
        /// <returns>A TextWriter to write to the file</returns>
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

        /// <summary>
        /// Serializes an Object to XML and returns the xml string.
        /// </summary>
        /// <typeparam name="T">Type of the object to serialize</typeparam>
        /// <param name="objToSerialize">The object to serialize to xml</param>
        /// <returns>An xml string of the serialized object</returns>
        public static string SerializeToXml<T>(T objToSerialize)
        {
            XmlSerializer x = new XmlSerializer(objToSerialize.GetType());
            StringWriter textWriter = new StringWriter();
            x.Serialize(textWriter, objToSerialize);
            return textWriter.ToString();
        }

        /// <summary>
        /// Deserializes an xml string to an object
        /// </summary>
        /// <typeparam name="T">Type of the object, the string gets serialized to</typeparam>
        /// <param name="xml">XML string of the serialized object</param>
        /// <returns>The deserialized object</returns>
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

        /// <summary>
        /// Reads an xml file from the current worlds storage folder. The calling type is used
        /// to get the assembly name to get the sub directory in the storage folder, where files for the dll are stored.
        /// </summary>
        /// <typeparam name="T">Type of the object represented in the xml file</typeparam>
        /// <param name="file">File name</param>
        /// <param name="callingType">Calling type</param>
        /// <returns>Deserialized object of the xml in file</returns>
        public static T ReadXmlFileFromWorld<T>(string file, Type callingType)
        {
            if (FileExistsInWorldStorage(file, callingType))
            {
                try
                {
                    using (var reader = ReadFileInWorldStorage(file, callingType))
                    {
                        T saveFile = SerializeFromXml<T>(reader.ReadToEnd());
                        return saveFile;
                    }
                }
                catch (Exception e)
                {
                    PluginLog.Log("Couldnt load save file.", LogLevel.ERROR);
                    PluginLog.Log(e.Message + "\n" + e.StackTrace, LogLevel.ERROR);
                    DeleteFileInWorldStorage(file, callingType);
                    return default(T);
                }
            }
            return default(T);
        }

        /// <summary>
        /// Writes an object serialized to xml to the file. The calling type is used
        /// to get the assembly name to get the sub directory in the storage folder, where files for the dll are stored.
        /// </summary>
        /// <typeparam name="T">Type of the object</typeparam>
        /// <param name="saveFile">Object to write to file as xml</param>
        /// <param name="file">File name</param>
        /// <param name="callingType">Calling type</param>
        public static void WriteXmlFileToWorld<T>(T saveFile, string file, Type callingType)
        {
            DeleteFileInWorldStorage(file, callingType);
            string xml = SerializeToXml<T>(saveFile);
            using (var writer = WriteFileInWorldStorage(file, callingType))
            {
                if (writer == null) return;
                writer.Write(xml);
                writer.Close();
            }
        }

        /// <summary>
        /// Reads an xml file located at the storage folder at path from file. The calling type is used
        /// to get the assembly name to get the sub directory in the storage folder, where files for the dll are stored.
        /// </summary>
        /// <typeparam name="T">Type of the object represented by the xml in the file</typeparam>
        /// <param name="path">Path to the storage folder</param>
        /// <param name="file">File name</param>
        /// <param name="callingType">Calling type</param>
        /// <returns>Deserialized object of the xml in the file</returns>
        public static T ReadXmlFileFromPath<T>(string path, string file, Type callingType)
        {
            if (FileExistsInPath(path, file, callingType))
            {
                try
                {
                    using (var reader = ReadFileInPath(path, file, callingType))
                    {
                        T saveFile = SerializeFromXml<T>(reader.ReadToEnd());
                        return saveFile;
                    }
                }
                catch (Exception e)
                {
                    PluginLog.Log("Couldnt load save file.", LogLevel.ERROR);
                    PluginLog.Log(e.Message + "\n" + e.StackTrace, LogLevel.ERROR);
                    DeleteFileInPath(path, file, callingType);
                    return default(T);
                }
            }
            return default(T);
        }

        /// <summary>
        /// Writes a serialized object as xml to a file in the storage folder at the given path. The calling type is used
        /// to get the assembly name to get the sub directory in the storage folder, where files for the dll are stored.
        /// </summary>
        /// <typeparam name="T">Type of the object to serialize</typeparam>
        /// <param name="saveFile">Object to serialize and write to the xml file</param>
        /// <param name="path">Path to the storage folder</param>
        /// <param name="file">File name</param>
        /// <param name="callingType">Calling type</param>
        public static void WriteXmlFileToPath<T>(T saveFile, string path, string file, Type callingType)
        {
            DeleteFileInPath(path, file, callingType);
            string xml = SerializeToXml<T>(saveFile);
            using (var writer = WriteFileInPath(path, file, callingType))
            {
                writer.Write(xml);
                writer.Close();
            }
        }
    }
}