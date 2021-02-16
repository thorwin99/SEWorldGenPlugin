using SEWorldGenPlugin.Generator.AsteroidObjects;
using SEWorldGenPlugin.Utilities;
using System;
using System.Collections.Generic;
using System.Reflection;
using VRage.Game.Components;

namespace SEWorldGenPlugin.Session
{
    /// <summary>
    /// This class handles loading and storing of all asteroid object types.
    /// </summary>
    [MySessionComponentDescriptor(MyUpdateOrder.NoUpdate, 1005)]
    public class MyAsteroidObjectsManager : MySessionComponentBase
    {
        /// <summary>
        /// Static instance of the asteroid object manager, so that only one exists
        /// </summary>
        public static MyAsteroidObjectsManager Static;

        /// <summary>
        /// All loaded asteroid object providers mapped to their type name
        /// </summary>
        public Dictionary<string, MyAbstractAsteroidObjectProvider> AsteroidObjectProviders;

        public override void LoadData()
        {
            base.LoadData();
            Static = this;
            AsteroidObjectProviders = new Dictionary<string, MyAbstractAsteroidObjectProvider>();
            LoadAllAsteroidTypes();
        }

        public override void SaveData()
        {
            if (!MySettingsSession.Static.IsEnabled()) return;

            foreach (var provider in AsteroidObjectProviders.Values)
            {
                provider.OnSave();
            }
        }

        protected override void UnloadData()
        {
            base.UnloadData();

            AsteroidObjectProviders.Clear();
            Static = null;
        }

        /// <summary>
        /// Loads all the asteroid object providers.
        /// </summary>
        private void LoadAllAsteroidTypes()
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                MyPluginLog.Log("Registering asteroid object providers in: " + assembly.FullName);
                try
                {
                    foreach (var type in assembly.GetTypes())
                    {
                        if (type.IsSubclassOf(typeof(MyAbstractAsteroidObjectProvider)))
                        {
                            MyPluginLog.Log("Registering provider " + type.Name);

                            MyAbstractAsteroidObjectProvider prov = Activator.CreateInstance(type) as MyAbstractAsteroidObjectProvider;
                            AsteroidObjectProviders.Add(prov.GetTypeName(), prov);
                        }
                    }
                }
                catch (ReflectionTypeLoadException)
                {
                    MyPluginLog.Log("Couldnt register asteroid object providers for assembly " + assembly.FullName, LogLevel.ERROR);
                }
            }
        }
    }
}
