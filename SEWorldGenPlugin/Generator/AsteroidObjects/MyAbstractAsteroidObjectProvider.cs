using Sandbox.Game.Multiplayer;
using SEWorldGenPlugin.Draw;
using SEWorldGenPlugin.Generator.AsteroidObjectShapes;
using SEWorldGenPlugin.GUI.AdminMenu;
using SEWorldGenPlugin.GUI.AdminMenu.SubMenus.StarSystemDesigner;
using SEWorldGenPlugin.Networking;
using SEWorldGenPlugin.Networking.Attributes;
using SEWorldGenPlugin.ObjectBuilders;
using SEWorldGenPlugin.Session;
using SEWorldGenPlugin.Utilities;
using System;
using System.Collections.Generic;

namespace SEWorldGenPlugin.Generator.AsteroidObjects
{
    /// <summary>
    /// Abstract class that manages all instances for a custom asteroid object.
    /// This class should not be inherited from, instead use <see cref="MyAbstractAsteroidObjectProvider{T}"/>
    /// </summary>
    [EventOwner]
    public abstract class MyAbstractAsteroidObjectProvider
    {
        /// <summary>
        /// The currently saved asteroid data
        /// </summary>
        protected Dictionary<Guid, IMyAsteroidData> m_savedData;

        public MyAbstractAsteroidObjectProvider()
        {
            m_savedData = new Dictionary<Guid, IMyAsteroidData>();
        }

        /// <summary>
        /// Returns the name of the asteroid type provided by this
        /// </summary>
        /// <returns>The typename of the asteroid object</returns>
        public abstract string GetTypeName();

        /// <summary>
        /// Whether the asteroid object provided by this class can be
        /// generated automatically at system generation. If this returns
        /// false, GetAdminMenuCreator() should return a value, so that this
        /// object is even usable.
        /// </summary>
        /// <returns>True, if it can be generated at system generation</returns>
        public abstract bool IsInstanceGeneratable();

        /// <summary>
        /// Returns the shape for the given asteroid object
        /// </summary>
        /// <param name="instance">The system object instance for the corresponding asteroid</param>
        /// <returns>The asteroid shape for the asteroid object</returns>
        public abstract IMyAsteroidObjectShape GetAsteroidObjectShape(MySystemAsteroids instance);

        /// <summary>
        /// Returns a new Instance of the edit menu for the star system designer to edit an instance of this asteroid type for the given instance.
        /// </summary>
        /// <param name="instance">The edited or spawned instance.</param>
        /// <returns>An instance of a <see cref="MyStarSystemDesignerObjectMenu"/></returns>
        public abstract MyStarSystemDesignerObjectMenu CreateStarSystemDesignerEditMenu(MySystemAsteroids instance);

        /// <summary>
        /// Generates an instance of the asteroid object provided by this class.
        /// Returns the system object with corresponding display name and other values set.
        /// Saves the Instance in the provider for later use.
        /// This method is used by the SystemGenerator component.
        /// </summary>
        /// <param name="systemIndex">Index of the object in the system</param>
        /// <param name="systemParent">The parent of this object in the system</param>
        /// <param name="objectOrbitRadius">Radius of the orbit of this instance</param>
        /// <returns>The system asteroids object</returns>
        public abstract MySystemAsteroids GenerateInstance(int systemIndex, in MySystemObject systemParent, double objectOrbitRadius);

        /// <summary>
        /// Tries to load the given asteroid.
        /// </summary>
        /// <returns>True if file was loaded sucessfully</returns>
        public abstract bool TryLoadObject(MySystemAsteroids asteroid);

        /// <summary>
        /// Returns an <see cref="MyAbstractStarSystemDesignerRenderObject"/> to visually represent the Asteroid object in the world.
        /// </summary>
        /// <param name="instance">The asteroid object instance to draw, if it is from this provider type</param>
        /// <param name="data">The associated data of the asteroid object</param>
        /// <returns></returns>
        public abstract MyAbstractStarSystemDesignerRenderObject GetRenderObject(MySystemAsteroids instance, IMyAsteroidData data);

        /// <summary>
        /// Saves all the asteroid objects provided by this provider.
        /// </summary>
        public void OnSave()
        {
            if (!Sync.IsServer || MyStarSystemGenerator.Static.StarSystem == null) return;

            MyPluginLog.Log("Server: Saving asteroid objects.");

            foreach(var data in m_savedData)
            {
                var instance = MyStarSystemGenerator.Static.StarSystem.GetById(data.Key);
                if (instance == null) continue;
                MyFileUtils.WriteXmlFileToWorld(data.Value, GetFileName(instance as MySystemAsteroids));
            }

            MyPluginLog.Log("Server: Asteroid objects saved for provider " + GetTypeName());
        }

        /// <summary>
        /// Returns the associated asteroid data provided by this class for the given asteroid object instance,
        /// if the instance is of type of this provider.
        /// </summary>
        /// <param name="instanceId">Instance to get data for.</param>
        /// <returns>The data associated or null.</returns>
        public IMyAsteroidData GetInstanceData(Guid instanceId)
        {
            if (!m_savedData.ContainsKey(instanceId)) return null;
            return m_savedData[instanceId];
        }

        /// <summary>
        /// Loads all known data for this provider from the server.
        /// </summary>
        public void FetchDataFromServer()
        {
            PluginEventHandler.Static.RaiseStaticEvent(EventFetchDataServer, GetTypeName(), Sync.MyId);
        }

        /// <summary>
        /// Adds a new Instance of an Asteroid object to this provider. 
        /// </summary>
        /// <param name="systemInstance">Instance that gets added</param>
        /// <param name="instanceData">Data associated the instance</param>
        public abstract void AddInstance(MySystemAsteroids systemInstance, IMyAsteroidData instanceData);

        /// <summary>
        /// Tries to remove the given asteroid instance from this provider.
        /// </summary>
        /// <param name="instanceId">The id of the instance.</param>
        /// <returns>True, if removed</returns>
        public void RemoveInstance(Guid instanceId)
        {
            PluginEventHandler.Static.RaiseStaticEvent(EventRemoveInstanceServer, GetTypeName(), instanceId);
        }

        /// <summary>
        /// Tires to set the data of the instance <paramref name="instanceId"/> to <paramref name="instanceData"/>.
        /// </summary>
        /// <param name="instanceId">Id of the instance to set the data of.</param>
        /// <param name="instanceData">Data to set.</param>
        public abstract void SetInstanceData(Guid instanceId, IMyAsteroidData instanceData);

        /// <summary>
        /// Updates the <paramref name="instance"/>.
        /// Example: The instance got renamed, so the filename is now outdated.
        /// </summary>
        /// <param name="updatedInstace">Instance that got updated</param>
        /// <param name="oldName">The previous name of the instance</param>
        public void InstanceRenamed(MySystemAsteroids updatedInstace, string oldName)
        {
            if (!Sync.IsServer) return;

            if (m_savedData.ContainsKey(updatedInstace.Id))
            {
                MyFileUtils.DeleteFileInWorldStorage(GetFileName(oldName, updatedInstace.Id));
                MyFileUtils.WriteXmlFileToWorld(m_savedData[updatedInstace.Id], GetFileName(updatedInstace));
            }
        }

        /// <summary>
        /// Converts an asteroid object name to a file name.
        /// </summary>
        /// <param name="obj">The asteroid object.</param>
        /// <returns>The file name for the asteroid object.</returns>
        protected string GetFileName(MySystemAsteroids obj)
        {
            return GetFileName(obj.DisplayName, obj.Id);
        }

        /// <summary>
        /// Converts an asteroid object name to a file name.
        /// </summary>
        /// <param name="name">The asteroid objects display name.</param>
        /// <param name="id">The asteroid objects id</param>
        /// <returns>The file name for the asteroid object.</returns>
        protected string GetFileName(string name, Guid id)
        {
            return (name + id).Replace(" ", "_") + ".roid";//Needs to be updated on rename of an asteroid object due to dependency on the display name
        }

        /// <summary>
        /// Server: Fetches all asteroid data for the specified provider from server.
        /// </summary>
        /// <param name="provider">Provider to fetch data for.</param>
        /// <param name="requesterId">Id of the client that requested this.</param>
        [Event(4000)]
        [Server]
        protected static void EventFetchDataServer(string provider, ulong requesterId)
        {
            MyPluginLog.Log("Server: Client " + requesterId + " requested update of asteroid provider " + provider);

            MyAbstractAsteroidObjectProvider prov;
            if (MyAsteroidObjectsManager.Static.AsteroidObjectProviders.TryGetValue(provider, out prov))
            {
                PluginEventHandler.Static.RaiseStaticEvent(EventFetchDataClient, provider, prov.m_savedData, requesterId);
            }
        }

        /// <summary>
        /// Client: Receives fetched data from server for provider <paramref name="provider"/>.
        /// </summary>
        /// <param name="provider">Provider the data is fetched from.</param>
        /// <param name="data">Data of the provider.</param>
        [Event(4001)]
        [Client]
        private static void EventFetchDataClient(string provider, Dictionary<Guid, IMyAsteroidData> data)
        {
            if (Sync.IsServer) return;

            MyPluginLog.Log("Received update for asteroid object provider " + provider + " with " + data.Count + " data entries");

            MyAbstractAsteroidObjectProvider prov;
            if (MyAsteroidObjectsManager.Static.AsteroidObjectProviders.TryGetValue(provider, out prov))
            {
                prov.m_savedData.Clear();
                foreach (var entry in data)
                {
                    prov.m_savedData.Add(entry.Key, entry.Value);
                }
            }
        }

        /// <summary>
        /// Server: Adds an instance of an asteroid object and its data to the system on the server.
        /// </summary>
        /// <param name="systemInstance">The asteroid object instance to add to the system.</param>
        /// <param name="instanceData">The data of the asteroid object.</param>
        [Event(4002)]
        [Server]
        protected static void EventAddInstanceServer(MySystemAsteroids systemInstance, IMyAsteroidData instanceData)
        {
            MyPluginLog.Log("Server: Adding new asteroid object instance " + systemInstance.Id + " to system");

            MyAbstractAsteroidObjectProvider prov;
            if (MyAsteroidObjectsManager.Static.AsteroidObjectProviders.TryGetValue(systemInstance.AsteroidTypeName, out prov))
            {
                var system = MyStarSystemGenerator.Static.StarSystem;
                if (system.Add(systemInstance))
                {
                    prov.m_savedData[systemInstance.Id] = instanceData;
                    PluginEventHandler.Static.RaiseStaticEvent(BroadcastInstanceAdded, systemInstance, instanceData);
                }
            }
        }

        /// <summary>
        /// Broadcast: Notifies clients that an instance of an asteroid object has been added on the server.
        /// </summary>
        /// <param name="systemInstance">Instance that got added.</param>
        /// <param name="instanceData">Data of the instance.</param>
        [Event(4003)]
        [Broadcast]
        private static void BroadcastInstanceAdded(MySystemAsteroids systemInstance, IMyAsteroidData instanceData)
        {
            if (Sync.IsServer) return;

            MyPluginLog.Log("Received update for asteroid objects, adding new instance " + systemInstance.Id + " to local system");

            MyAbstractAsteroidObjectProvider prov;
            if (MyAsteroidObjectsManager.Static.AsteroidObjectProviders.TryGetValue(systemInstance.AsteroidTypeName, out prov))
            {
                var system = MyStarSystemGenerator.Static.StarSystem;
                if (system.Add(systemInstance))
                {
                    prov.m_savedData[systemInstance.Id] = instanceData;
                }
                else
                {
                    MyPluginLog.Log("System data is out of sync, requesting full update.");
                    MyStarSystemGenerator.Static.GetStarSystemFromServer();
                    prov.FetchDataFromServer();
                }
            }
        }

        /// <summary>
        /// Server: Removes an instance of an asteroid object and its data from the system on the server.
        /// </summary>
        /// <param name="providerName">Name of the provider that the instance gets removed from.</param>
        /// <param name="instanceId">Id of the instance that should be removed.</param>
        [Event(4004)]
        [Server]
        protected static void EventRemoveInstanceServer(string providerName, Guid instanceId)
        {
            MyPluginLog.Log("Server: Removing asteroid object instance " + instanceId + " from the system");

            MyAbstractAsteroidObjectProvider prov;
            if (MyAsteroidObjectsManager.Static.AsteroidObjectProviders.TryGetValue(providerName, out prov))
            {
                var system = MyStarSystemGenerator.Static.StarSystem;
                if (system.Remove(instanceId))
                {
                    MyGPSManager.Static.RemovePersistentGps(instanceId);
                    prov.m_savedData.Remove(instanceId);
                    PluginEventHandler.Static.RaiseStaticEvent(BroadcastInstanceRemoved, providerName, instanceId);
                }
            }
        }

        /// <summary>
        /// Broadcast: Notifies clients that an instance was removed from the server.
        /// </summary>
        /// <param name="providerName">Name of the provider the instance was removed from.</param>
        /// <param name="instanceId">The id of the instance.</param>
        [Event(4005)]
        [Broadcast]
        private static void BroadcastInstanceRemoved(string providerName, Guid instanceId)
        {
            if (Sync.IsServer) return;

            MyPluginLog.Log("Received update for asteroid objects, removing instance " + instanceId + " from local system");

            MyAbstractAsteroidObjectProvider prov;
            if (MyAsteroidObjectsManager.Static.AsteroidObjectProviders.TryGetValue(providerName, out prov))
            {
                var system = MyStarSystemGenerator.Static.StarSystem;
                if (system.Remove(instanceId))
                {
                    prov.m_savedData.Remove(instanceId);
                }
                else
                {
                    MyPluginLog.Log("System data is out of sync, requesting full update.");
                    MyStarSystemGenerator.Static.GetStarSystemFromServer();
                    prov.FetchDataFromServer();
                }
            }
        }

        /// <summary>
        /// Server: Sets the data of an instance of an asteroid object.
        /// </summary>
        /// <param name="providerName">Provider name of the asteroid object</param>
        /// <param name="instanceId">Instance id of the asteroid object</param>
        /// <param name="instanceData">Data to set for the asteroid object</param>
        [Event(4006)]
        [Server]
        protected static void EventSetInstanceData(string providerName, Guid instanceId, IMyAsteroidData instanceData)
        {
            MyPluginLog.Log("Server: Removing asteroid object instance " + instanceId + " from the system");

            MyAbstractAsteroidObjectProvider prov;
            if (MyAsteroidObjectsManager.Static.AsteroidObjectProviders.TryGetValue(providerName, out prov))
            {
                var system = MyStarSystemGenerator.Static.StarSystem;
                if (system.Contains(instanceId))
                {
                    prov.m_savedData[instanceId] = instanceData;
                    PluginEventHandler.Static.RaiseStaticEvent(BroadcastInstanceDataSet, providerName, instanceId, instanceData);
                }
                else
                {
                    MyPluginLog.Log("Server: Instance does not exist.");
                }
            }
        }

        /// <summary>
        /// Broadcast: Notifies clients that asteroid data was set.
        /// </summary>
        /// <param name="providerName">Provider name of the asteroid object</param>
        /// <param name="instanceId">Instance id of the asteroid object</param>
        /// <param name="instanceData">Data that got set for the asteroid object</param>
        [Event(4007)]
        [Broadcast]
        private static void BroadcastInstanceDataSet(string providerName, Guid instanceId, IMyAsteroidData instanceData)
        {
            if (Sync.IsServer) return;

            MyPluginLog.Log("Received update for asteroid objects, setting instance data o" + instanceId);

            MyAbstractAsteroidObjectProvider prov;
            if (MyAsteroidObjectsManager.Static.AsteroidObjectProviders.TryGetValue(providerName, out prov))
            {
                prov.m_savedData[instanceId] = instanceData;
            }
        }
    }

    /// <summary>
    /// An IMyAsteroidData extension of MyAbstractAsteroidObjectProvider to implement type specific.
    /// </summary>
    /// <typeparam name="T">The data type used by asteroids from this provider. Has to inherit <see cref="IMyAsteroidData"/></typeparam>
    public abstract class MyAbstractAsteroidObjectProvider<T> : MyAbstractAsteroidObjectProvider where T : IMyAsteroidData
    {
        public override bool TryLoadObject(MySystemAsteroids asteroid)
        {
            if (!Sync.IsServer) return false;

            if (m_savedData.ContainsKey(asteroid.Id)) return false;

            var data = MyFileUtils.ReadXmlFileFromWorld<T>(GetFileName(asteroid));

            if (data == null) return false;

            m_savedData[asteroid.Id] = data;

            return true;
        }

        public override void AddInstance(MySystemAsteroids systemInstance, IMyAsteroidData instanceData)
        {
            if (systemInstance == null || instanceData == null)
            {
                return;
            }

            if (!(instanceData is T))
            {
                MyPluginLog.Log("Client: Tried to add an instance with wrong type of data.");
                return;
            }

            PluginEventHandler.Static.RaiseStaticEvent(EventAddInstanceServer, systemInstance, instanceData);
        }

        public override void SetInstanceData(Guid instanceId, IMyAsteroidData instanceData)
        {
            if (instanceData == null)
            {
                return;
            }

            if (!(instanceData is T))
            {
                MyPluginLog.Log("Client: Tried to add an instance with wrong type of data.");
            }

            PluginEventHandler.Static.RaiseStaticEvent(EventSetInstanceData, GetTypeName(), instanceId, instanceData);
        }
    }
}
