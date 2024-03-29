﻿using Sandbox.Game.World;
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
    public class MyFileUtils
    {
        /// <summary>
        /// Storage folder name in the world and SE data folder.
        /// </summary>
        private const string STORAGE_FOLDER = "Storage";

        /// <summary>
        /// Name of the folder for plugin specific files
        /// </summary>
        private const string PLUGIN_FOLDER = "SEWorldGenPlugin";

        /// <summary>
        /// Checks if a file with the given name exists in the world folder inside the plugins storage folder.
        /// </summary>
        /// <param name="file">File name</param>
        /// <returns></returns>
        public static bool FileExistsInWorldStorage(string file)
        {
            if (file.IndexOfAny(Path.GetInvalidFileNameChars()) != -1)
            {
                return false;
            }

            var path = Path.Combine(MySession.Static.CurrentPath, STORAGE_FOLDER, PLUGIN_FOLDER, file);

            return File.Exists(path);
        }

        /// <summary>
        /// Checks whether a file exists in a given paths storage folder or not. The path gets the storage folder and subfolder appended.
        /// The storage folder is the plugins storage folder.
        /// </summary>
        /// <param name="path">Path to the storage folder</param>
        /// <param name="file">Name of the file</param>
        /// <returns>True if it exists</returns>
        public static bool FileExistsInPath(string path, string file)
        {
            if (file.IndexOfAny(Path.GetInvalidFileNameChars()) != -1)
            {
                return false;
            }

            var paths = Path.Combine(path, STORAGE_FOLDER, PLUGIN_FOLDER, file);
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
        /// Deletes a file in the storage folder of the current world. The subdirectoy for the file in the storage folder
        /// is the plugins storage folder, SEWorldGenPlugin
        /// </summary>
        /// <param name="file">File name</param>
        public static void DeleteFileInWorldStorage(string file)
        {
            if (FileExistsInWorldStorage(file))
            {
                var path = Path.Combine(MySession.Static.CurrentPath, STORAGE_FOLDER, PLUGIN_FOLDER, file);
                File.Delete(path);
            }
        }

        /// <summary>
        /// Deletes a file at the paths storage folder. The subdirectoy for the file in the storage folder
        /// is the plugins storage folder, SEWorldGenPlugin
        /// </summary>
        /// <param name="path">Path to the storage folder</param>
        /// <param name="file">File name</param>
        public static void DeleteFileInPath(string path, string file)
        {
            if (FileExistsInPath(path, file))
            {
                var paths = Path.Combine(path, STORAGE_FOLDER, PLUGIN_FOLDER, file);
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
        /// Reads the file contents of a file in the current worlds storage folder. The subdirectoy for the file in the storage folder
        /// is the plugins storage folder, SEWorldGenPlugin
        /// </summary>
        /// <param name="file">File name</param>
        /// <returns>A TextReader of the opened file</returns>
        public static TextReader ReadFileInWorldStorage(string file)
        {
            if (file.IndexOfAny(Path.GetInvalidFileNameChars()) != -1)
            {
                throw new FileNotFoundException();
            }
            var path = Path.Combine(MySession.Static.CurrentPath, STORAGE_FOLDER, PLUGIN_FOLDER, file);
            var stream = MyFileSystem.OpenRead(path);
            if (stream != null)
            {
                return new StreamReader(stream);
            }
            throw new FileNotFoundException();
        }

        /// <summary>
        /// Reads a file in the storage folder at the given path. The subdirectoy for the file in the storage folder
        /// is the plugins storage folder, SEWorldGenPlugin
        /// </summary>
        /// <param name="path">Path to the storage folder</param>
        /// <param name="file">File name</param>
        /// <returns>Returns a TextReader of the read file</returns>
        public static TextReader ReadFileInPath(string path, string file)
        {
            if (file.IndexOfAny(Path.GetInvalidFileNameChars()) != -1)
            {
                throw new FileNotFoundException();
            }
            var paths = Path.Combine(path, STORAGE_FOLDER, PLUGIN_FOLDER, file);
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
        /// If the file does not exist, it will be created. 
        /// The subdirectoy for the file in the storage folder
        /// is the plugins storage folder, SEWorldGenPlugin
        /// </summary>
        /// <param name="file">File name</param>
        /// <exception cref="FileNotFoundException">When no file cannot be created</exception>
        /// <returns>A TextWriter, to write to the file</returns>
        public static TextWriter WriteFileInWorldStorage(string file)
        {
            if (file.IndexOfAny(Path.GetInvalidFileNameChars()) != -1)
            {
                throw new FileNotFoundException();
            }
            if (!MySession.Static.CurrentPath.ToLower().Contains(MyFileSystem.UserDataPath.ToLower()))
            {
                return null;
            }

            var path = Path.Combine(MySession.Static.CurrentPath, STORAGE_FOLDER, PLUGIN_FOLDER, file);
            var stream = MyFileSystem.OpenWrite(path);
            if (stream != null)
            {
                return new StreamWriter(stream);
            }
            throw new FileNotFoundException();
        }

        /// <summary>
        /// Writes to a file located in the storage folder at path. 
        /// The subdirectoy for the file in the storage folder
        /// is the plugins storage folder, SEWorldGenPlugin
        /// </summary>
        /// <param name="path">Path to the storage folder</param>
        /// <param name="file">File name</param>
        /// <returns>A TextWriter to write to the file</returns>
        public static TextWriter WriteFileInPath(string path, string file)
        {
            if (file.IndexOfAny(Path.GetInvalidFileNameChars()) != -1)
            {
                throw new FileNotFoundException();
            }

            var paths = Path.Combine(path, STORAGE_FOLDER, PLUGIN_FOLDER, file);
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
        /// Reads an xml file from the current worlds storage folder. 
        /// The subdirectoy for the file in the storage folder
        /// is the plugins storage folder, SEWorldGenPlugin
        /// </summary>
        /// <typeparam name="T">Type of the object represented in the xml file</typeparam>
        /// <param name="file">File name</param>
        /// <returns>Deserialized object of the xml in file</returns>
        public static T ReadXmlFileFromWorld<T>(string file)
        {
            if (FileExistsInWorldStorage(file))
            {
                try
                {
                    using (var reader = ReadFileInWorldStorage(file))
                    {
                        T saveFile = SerializeFromXml<T>(reader.ReadToEnd());
                        return saveFile;
                    }
                }
                catch (Exception e)
                {
                    MyPluginLog.Log("Couldnt load save file.", LogLevel.ERROR);
                    MyPluginLog.Log(e.Message + "\n" + e.StackTrace, LogLevel.ERROR);
                    DeleteFileInWorldStorage(file);
                    return default(T);
                }
            }
            return default(T);
        }

        /// <summary>
        /// Writes an object serialized to xml to the file. 
        /// The subdirectoy for the file in the storage folder
        /// is the plugins storage folder, SEWorldGenPlugin
        /// </summary>
        /// <typeparam name="T">Type of the object</typeparam>
        /// <param name="saveFile">Object to write to file as xml</param>
        /// <param name="file">File name</param>
        public static void WriteXmlFileToWorld<T>(T saveFile, string file)
        {
            DeleteFileInWorldStorage(file);
            string xml = SerializeToXml<T>(saveFile);
            using (var writer = WriteFileInWorldStorage(file))
            {
                if (writer == null) return;
                writer.Write(xml);
                writer.Close();
            }
        }

        /// <summary>
        /// Reads an xml file located at the storage folder at path from file. 
        /// The subdirectoy for the file in the storage folder
        /// is the plugins storage folder, SEWorldGenPlugin
        /// </summary>
        /// <typeparam name="T">Type of the object represented by the xml in the file</typeparam>
        /// <param name="path">Path to the storage folder</param>
        /// <param name="file">File name</param>
        /// <returns>Deserialized object of the xml in the file</returns>
        public static T ReadXmlFileFromPath<T>(string path, string file)
        {
            if (FileExistsInPath(path, file))
            {
                try
                {
                    using (var reader = ReadFileInPath(path, file))
                    {
                        T saveFile = SerializeFromXml<T>(reader.ReadToEnd());
                        return saveFile;
                    }
                }
                catch (Exception e)
                {
                    MyPluginLog.Log("Couldnt load save file.", LogLevel.ERROR);
                    MyPluginLog.Log(e.Message + "\n" + e.StackTrace, LogLevel.ERROR);
                    DeleteFileInPath(path, file);
                    return default(T);
                }
            }
            return default(T);
        }

        /// <summary>
        /// Writes a serialized object as xml to a file in the storage folder at the given path. 
        /// The subdirectoy for the file in the storage folder
        /// is the plugins storage folder, SEWorldGenPlugin
        /// </summary>
        /// <typeparam name="T">Type of the object to serialize</typeparam>
        /// <param name="saveFile">Object to serialize and write to the xml file</param>
        /// <param name="path">Path to the storage folder</param>
        /// <param name="file">File name</param>
        /// <param name="callingType">Calling type</param>
        public static void WriteXmlFileToPath<T>(T saveFile, string path, string file)
        {
            DeleteFileInPath(path, file);
            string xml = SerializeToXml<T>(saveFile);
            using (var writer = WriteFileInPath(path, file))
            {
                writer.Write(xml);
                writer.Close();
            }
        }
    }
}