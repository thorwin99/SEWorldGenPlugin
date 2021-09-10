using Sandbox.Game.Multiplayer;
using SEWorldGenPlugin.Generator.AsteroidObjects;
using SEWorldGenPlugin.Utilities;
using System;
using System.Collections.Generic;
using System.Reflection;
using VRage.Game;
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

            MyPluginLog.Log("Asteroid object manager loading data");

            Static = this;
            AsteroidObjectProviders = new Dictionary<string, MyAbstractAsteroidObjectProvider>();
            LoadAllAsteroidTypes();

            MyPluginLog.Log("Asteroid object manager loading data completed");
        }

        public override void Init(MyObjectBuilder_SessionComponent sessionComponent)
        {
            base.Init(sessionComponent);

            MyPluginLog.Log("Asteroid object manager initialization");

            if (!Sync.IsServer)
            {
                MyPluginLog.Log("Client is not the server, fetching asteroid provider data from server");

                foreach (var provider in AsteroidObjectProviders)
                {
                    provider.Value.FetchDataFromServer();
                }

                return;
            }

            MyPluginLog.Log("Asteroid object manager initialization complete");
        }

        public override void SaveData()
        {
            MyPluginLog.Log("Asteroid object manager saving");

            if (!MySettingsSession.Static.IsEnabled() || !Sync.IsServer)
            {
                MyPluginLog.Log("Plugin not enabled or client is not server, aborting");
                return;
            }

            foreach (var provider in AsteroidObjectProviders.Values)
            {
                MyPluginLog.Log("Saving asteroid provider " + provider.GetTypeName());
                provider.OnSave();
            }

            MyPluginLog.Log("Asteroid object manager saving complete");
        }

        protected override void UnloadData()
        {
            MyPluginLog.Log("Asteroid object manager unloading data");

            base.UnloadData();

            AsteroidObjectProviders.Clear();
            Static = null;

            MyPluginLog.Log("Asteroid object manager unloading data completed");
        }

        /// <summary>
        /// Loads all the asteroid object providers.
        /// </summary>
        private void LoadAllAsteroidTypes()
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                MyPluginLog.Debug("Registering asteroid object providers in: " + assembly.FullName);
                try
                {
                    foreach (var type in assembly.GetTypes())
                    {
                        if (type.IsSubclassOf(typeof(MyAbstractAsteroidObjectProvider)) && !type.IsGenericType)
                        {
                            MyPluginLog.Log("Registering provider " + type.Name);

                            MyAbstractAsteroidObjectProvider prov = Activator.CreateInstance(type) as MyAbstractAsteroidObjectProvider;
                            AsteroidObjectProviders.Add(prov.GetTypeName(), prov);
                        }
                    }
                }
                catch (ReflectionTypeLoadException)
                {
                    MyPluginLog.Log("Couldnt register asteroid object providers for assembly " + assembly.FullName, LogLevel.WARNING);
                }
            }
        }
    }
}
