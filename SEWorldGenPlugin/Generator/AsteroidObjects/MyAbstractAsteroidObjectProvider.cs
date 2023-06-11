using ProtoBuf;
using Sandbox.Game.Multiplayer;
using SEWorldGenPlugin.Generator.AsteroidObjectShapes;
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
        /// <param name="data">Optional data for the instance</param>
        /// <returns>An instance of a <see cref="MyStarSystemDesignerObjectMenu"/></return>
        public abstract MyStarSystemDesignerObjectMenu CreateStarSystemDesignerEditMenu(MySystemAsteroids instance, IMyAsteroidData data = null);

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
        /// <returns>True if file was loaded sucessfully, or it already was loaded</returns>
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
        /// Return a default instance of asteroid data for this provider.
        /// If <paramref name="parent"/> is not null, the data will be set with default
        /// values for that parent object, if it is null a completely default data will be returned
        /// This instance is not yet associated with ANY asteroid instance.
        /// </summary>
        /// <param name="parent">A parent for this instance data, that way default values can be set according to the parent.</param>
        /// <returns>A new default instance of data for this provider type.</returns>
        public abstract IMyAsteroidData GetDefaultData(MySystemObject parent);

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
        /// <param name="callback">Callback called on success or failure.</param>
        public abstract void AddInstance(MySystemAsteroids systemInstance, IMyAsteroidData instanceData, Action<bool> callback = null);

        /// <summary>
        /// Tries to remove the given asteroid instance from this provider.
        /// </summary>
        /// <param name="instanceId">The id of the instance.</param>
        /// <param name="callback">Callback called on success or failure.</param>
        /// <returns>True, if removed</returns>
        public void RemoveInstance(Guid instanceId, Action<bool> callback = null)
        {
            uint callbackId = PluginEventHandler.Static.AddNetworkCallback(callback);

            PluginEventHandler.Static.RaiseStaticEvent(EventRemoveInstanceServer, GetTypeName(), instanceId, callbackId, Sync.MyId);
        }

        /// <summary>
        /// Tries to remove the given asteroid instance from this provider.
        /// This is server only as it does not invoke the RPC event to remove the object.
        /// </summary>
        /// <param name="instanceId">The id of the instance.</param>
        /// <param name="callbackId">The id of the callback to call on success</param>
        /// <returns>True, if removed</returns>
        public void RemoveInstance(Guid instanceId, uint callbackId, ulong clientId)
        {
            if (!Sync.IsServer) return;

            EventRemoveInstanceServer(GetTypeName(), instanceId, callbackId, clientId);
        }

        /// <summary>
        /// Tires to set the data of the instance <paramref name="instanceId"/> to <paramref name="instanceData"/>.
        /// </summary>
        /// <param name="instanceId">Id of the instance to set the data of.</param>
        /// <param name="instanceData">Data to set.</param>
        /// <param name="callback">Callback called on success or failure.</param>
        public abstract void SetInstanceData(Guid instanceId, IMyAsteroidData instanceData, Action<bool> callback = null);

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
        /// Serializes the given data for network communication
        /// </summary>
        /// <param name="data">Data to serialize</param>
        /// <returns>The serialized data</returns>
        protected abstract MySerializedAsteroidData SerializeData(IMyAsteroidData data);

        /// <summary>
        /// Deserializes the given data from network communication
        /// </summary>
        /// <param name="data">Data to serialize</param>
        /// <returns>The deserialized data</returns>
        protected abstract IMyAsteroidData DeserializeData(MySerializedAsteroidData data);

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
                Dictionary<Guid, MySerializedAsteroidData> serializedData = new Dictionary<Guid, MySerializedAsteroidData>();
                foreach(var entry in prov.m_savedData)
                {
                    serializedData.Add(entry.Key, prov.SerializeData(entry.Value));
                }

                PluginEventHandler.Static.RaiseStaticEvent(EventFetchDataClient, provider, serializedData, requesterId);
            }
        }

        /// <summary>
        /// Client: Receives fetched data from server for provider <paramref name="provider"/>.
        /// </summary>
        /// <param name="provider">Provider the data is fetched from.</param>
        /// <param name="data">Data of the provider.</param>
        [Event(4001)]
        [Client]
        private static void EventFetchDataClient(string provider, Dictionary<Guid, MySerializedAsteroidData> data)
        {
            if (Sync.IsServer) return;

            MyPluginLog.Log("Received update for asteroid object provider " + provider + " with " + data.Count + " data entries");

            MyAbstractAsteroidObjectProvider prov;
            if (MyAsteroidObjectsManager.Static.AsteroidObjectProviders.TryGetValue(provider, out prov))
            {
                prov.m_savedData.Clear();
                foreach (var entry in data)
                {
                    prov.m_savedData.Add(entry.Key, prov.DeserializeData(entry.Value));
                }
            }
        }

        /// <summary>
        /// Server: Adds an instance of an asteroid object and its data to the system on the server.
        /// </summary>
        /// <param name="systemInstance">The asteroid object instance to add to the system.</param>
        /// <param name="instanceData">The data of the asteroid object.</param>
        /// <param name="callbackId">The id of the callback to call when action is completed.</param>
        /// <param name="senderId">The client that requested this event.</param>
        [Event(4002)]
        [Server]
        protected static void EventAddInstanceServer(MySystemAsteroids systemInstance, MySerializedAsteroidData instanceData, uint callbackId, ulong senderId)
        {
            MyPluginLog.Log("Server: Adding new asteroid object instance " + systemInstance.Id + " to system");

            Action<bool> callback = null;
            if (senderId == Sync.MyId)
                callback = PluginEventHandler.Static.GetNetworkCallback(callbackId);

            MyAbstractAsteroidObjectProvider prov;
            if (MyAsteroidObjectsManager.Static.AsteroidObjectProviders.TryGetValue(systemInstance.AsteroidTypeName, out prov))
            {
                var system = MyStarSystemGenerator.Static.StarSystem;
                if (system.Add(systemInstance))
                {
                    prov.m_savedData[systemInstance.Id] = prov.DeserializeData(instanceData);
                    PluginEventHandler.Static.RaiseStaticEvent(BroadcastInstanceAdded, systemInstance, instanceData, callbackId, senderId);
                    callback?.Invoke(true);

                    return;
                }
            }

            PluginEventHandler.Static.RaiseStaticEvent(SendNetActionFailed, callbackId, senderId);
            callback?.Invoke(false);
        }

        /// <summary>
        /// Broadcast: Notifies clients that an instance of an asteroid object has been added on the server.
        /// </summary>
        /// <param name="systemInstance">Instance that got added.</param>
        /// <param name="instanceData">Data of the instance.</param>
        /// <param name="callbackId">The id of the callback to call when action is completed.</param>
        /// <param name="senderId">The client that requested this event.</param>
        [Event(4003)]
        [Broadcast]
        private static void BroadcastInstanceAdded(MySystemAsteroids systemInstance, MySerializedAsteroidData instanceData, uint callbackId, ulong senderId)
        {
            if (Sync.IsServer) return;

            MyPluginLog.Log("Received update for asteroid objects, adding new instance " + systemInstance.Id + " to local system");

            Action<bool> callback = null;
            if (senderId == Sync.MyId)
                callback = PluginEventHandler.Static.GetNetworkCallback(callbackId);

            MyAbstractAsteroidObjectProvider prov;
            if (MyAsteroidObjectsManager.Static.AsteroidObjectProviders.TryGetValue(systemInstance.AsteroidTypeName, out prov))
            {
                var system = MyStarSystemGenerator.Static.StarSystem;
                if (system.Add(systemInstance))
                {
                    prov.m_savedData[systemInstance.Id] = prov.DeserializeData(instanceData);
                    callback?.Invoke(true);

                    return;
                }
                else
                {
                    MyPluginLog.Log("System data is out of sync, requesting full update.");
                    MyStarSystemGenerator.Static.GetStarSystemFromServer();
                    prov.FetchDataFromServer();

                    return;
                }
            }

            callback?.Invoke(false);
        }

        /// <summary>
        /// Server: Removes an instance of an asteroid object and its data from the system on the server.
        /// </summary>
        /// <param name="providerName">Name of the provider that the instance gets removed from.</param>
        /// <param name="instanceId">Id of the instance that should be removed.</param>
        /// <param name="callbackId">The id of the callback to call when action is completed.</param>
        /// <param name="senderId">The client that requested this event.</param>
        [Event(4004)]
        [Server]
        protected static void EventRemoveInstanceServer(string providerName, Guid instanceId, uint callbackId, ulong senderId)
        {
            MyPluginLog.Log("Server: Removing asteroid object instance " + instanceId + " from the system");

            Action<bool> callback = null;
            if (senderId == Sync.MyId)
                callback = PluginEventHandler.Static.GetNetworkCallback(callbackId);

            MyAbstractAsteroidObjectProvider prov;
            if (MyAsteroidObjectsManager.Static.AsteroidObjectProviders.TryGetValue(providerName, out prov))
            {
                var system = MyStarSystemGenerator.Static.StarSystem;
                if (system.Remove(instanceId))
                {
                    MyGPSManager.Static.RemovePersistentGps(instanceId);
                    prov.m_savedData.Remove(instanceId);
                    PluginEventHandler.Static.RaiseStaticEvent(BroadcastInstanceRemoved, providerName, instanceId, callbackId, senderId);

                    callback?.Invoke(true);

                    return;
                }
            }

            PluginEventHandler.Static.RaiseStaticEvent(SendNetActionFailed, callbackId, senderId);
            callback?.Invoke(false);
        }

        /// <summary>
        /// Broadcast: Notifies clients that an instance was removed from the server.
        /// </summary>
        /// <param name="providerName">Name of the provider the instance was removed from.</param>
        /// <param name="instanceId">The id of the instance.</param>
        /// <param name="callbackId">The id of the callback to call when action is completed.</param>
        /// <param name="senderId">The client that requested this event.</param>
        [Event(4005)]
        [Broadcast]
        private static void BroadcastInstanceRemoved(string providerName, Guid instanceId, uint callbackId, ulong senderId)
        {
            if (Sync.IsServer) return;

            MyPluginLog.Log("Received update for asteroid objects, removing instance " + instanceId + " from local system");

            Action<bool> callback = null;
            if (senderId == Sync.MyId)
                callback = PluginEventHandler.Static.GetNetworkCallback(callbackId);

            MyAbstractAsteroidObjectProvider prov;
            if (MyAsteroidObjectsManager.Static.AsteroidObjectProviders.TryGetValue(providerName, out prov))
            {
                var system = MyStarSystemGenerator.Static.StarSystem;
                if (system.Remove(instanceId))
                {
                    prov.m_savedData.Remove(instanceId);

                    callback?.Invoke(true);

                    return;
                }
                else
                {
                    MyPluginLog.Log("System data is out of sync, requesting full update.");
                    MyStarSystemGenerator.Static.GetStarSystemFromServer();
                    prov.FetchDataFromServer();

                    callback?.Invoke(false);

                    return;
                }
            }

            callback?.Invoke(false);
        }

        /// <summary>
        /// Server: Sets the data of an instance of an asteroid object.
        /// </summary>
        /// <param name="providerName">Provider name of the asteroid object</param>
        /// <param name="instanceId">Instance id of the asteroid object</param>
        /// <param name="instanceData">Data to set for the asteroid object</param>
        /// <param name="callbackId">The id of the callback to call when action is completed.</param>
        /// <param name="senderId">The client that requested this event.</param>
        [Event(4006)]
        [Server]
        protected static void EventSetInstanceData(string providerName, Guid instanceId, MySerializedAsteroidData instanceData, uint callbackId, ulong senderId)
        {
            MyPluginLog.Log("Server: Setting asteroid object instance " + instanceId + " data");

            Action<bool> callback = null;
            if (senderId == Sync.MyId)
                callback = PluginEventHandler.Static.GetNetworkCallback(callbackId);

            MyAbstractAsteroidObjectProvider prov;
            if (MyAsteroidObjectsManager.Static.AsteroidObjectProviders.TryGetValue(providerName, out prov))
            {
                var system = MyStarSystemGenerator.Static.StarSystem;
                if (system.Contains(instanceId))
                {
                    prov.m_savedData[instanceId] = prov.DeserializeData(instanceData);
                    PluginEventHandler.Static.RaiseStaticEvent(BroadcastInstanceDataSet, providerName, instanceId, instanceData, callbackId, senderId);

                    callback?.Invoke(true);

                    return;
                }
                else
                {
                    MyPluginLog.Log("Server: Instance does not exist.");
                }
            }

            PluginEventHandler.Static.RaiseStaticEvent(SendNetActionFailed, callbackId, senderId);
            callback?.Invoke(false);
        }

        /// <summary>
        /// Broadcast: Notifies clients that asteroid data was set.
        /// </summary>
        /// <param name="providerName">Provider name of the asteroid object</param>
        /// <param name="instanceId">Instance id of the asteroid object</param>
        /// <param name="instanceData">Data that got set for the asteroid object</param>
        /// <param name="callbackId">The id of the callback to call when action is completed.</param>
        /// <param name="senderId">The client that requested this event.</param>
        [Event(4007)]
        [Broadcast]
        private static void BroadcastInstanceDataSet(string providerName, Guid instanceId, MySerializedAsteroidData instanceData, uint callbackId, ulong senderId)
        {
            if (Sync.IsServer) return;

            MyPluginLog.Log("Received update for asteroid objects, setting instance data of " + instanceId);

            Action<bool> callback = null;
            if (senderId == Sync.MyId)
                callback = PluginEventHandler.Static.GetNetworkCallback(callbackId);

            MyAbstractAsteroidObjectProvider prov;
            if (MyAsteroidObjectsManager.Static.AsteroidObjectProviders.TryGetValue(providerName, out prov))
            {
                prov.m_savedData[instanceId] = prov.DeserializeData(instanceData);

                callback?.Invoke(true);
                return;
            }

            callback?.Invoke(false);
        }

        /// <summary>
        /// Network event when an action called from a client failed.
        /// Used to notify client about failure.
        /// </summary>
        /// <param name="callbackId">Id of the callback for that action.</param>
        [Event(4008)]
        [Client]
        private static void SendNetActionFailed(uint callbackId)
        {
            if (Sync.IsServer) return;

            Action<bool> callback = PluginEventHandler.Static.GetNetworkCallback(callbackId);

            callback?.Invoke(false);
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

            if (m_savedData.ContainsKey(asteroid.Id)) return true;

            var data = MyFileUtils.ReadXmlFileFromWorld<T>(GetFileName(asteroid));

            if (data == null) return false;

            m_savedData[asteroid.Id] = data;

            return true;
        }

        public override void AddInstance(MySystemAsteroids systemInstance, IMyAsteroidData instanceData, Action<bool> callback)
        {
            if (systemInstance == null || instanceData == null)
            {
                return;
            }

            if (!(instanceData is T))
            {
                MyPluginLog.Log("Client: Tried to add an instance with wrong type of data.", LogLevel.WARNING);
                return;
            }

            uint callbackId = PluginEventHandler.Static.AddNetworkCallback(callback);

            PluginEventHandler.Static.RaiseStaticEvent(EventAddInstanceServer, systemInstance, SerializeData(instanceData), callbackId, Sync.MyId);
        }

        public override void SetInstanceData(Guid instanceId, IMyAsteroidData instanceData, Action<bool> callback)
        {
            if (instanceData == null)
            {
                return;
            }

            if (!(instanceData is T))
            {
                MyPluginLog.Log("Client: Tried to add an instance with wrong type of data.", LogLevel.WARNING);
            }

            uint callbackId = PluginEventHandler.Static.AddNetworkCallback(callback);

            PluginEventHandler.Static.RaiseStaticEvent(EventSetInstanceData, GetTypeName(), instanceId, SerializeData(instanceData), callbackId, Sync.MyId);
        }

        protected override MySerializedAsteroidData SerializeData(IMyAsteroidData data)
        {
            if(data is T)
            {
                return new MySerializedAsteroidData(PluginEventHandler.Static.Serialize((T)data));
            }
            else
            {
                MyPluginLog.Log("Tried to serialize data of another provider. Aborting.", LogLevel.ERROR);
            }

            return null;
        }

        protected override IMyAsteroidData DeserializeData(MySerializedAsteroidData data)
        {
            try
            {
                T d = PluginEventHandler.Static.Deserialize<T>(data.Data);
                return d;
            }
            catch (Exception)
            {
                MyPluginLog.Log("Tried to deserialize data of another provider. Aborting.", LogLevel.ERROR);
            }
            return null;
        }
    }

    [Serializable]
    [ProtoContract]
    public class MySerializedAsteroidData
    {
        [ProtoMember(1)]
        public byte[] Data;

        public MySerializedAsteroidData()
        {
        }

        public MySerializedAsteroidData(byte[] data)
        {
            Data = data;
        }
    }
}
