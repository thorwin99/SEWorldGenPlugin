using ProtoBuf;
using Sandbox.Game.Multiplayer;
using Sandbox.Graphics.GUI;
using SEWorldGenPlugin.Generator.AsteroidObjectShapes;
using SEWorldGenPlugin.GUI.AdminMenu;
using SEWorldGenPlugin.Networking;
using SEWorldGenPlugin.Networking.Attributes;
using SEWorldGenPlugin.ObjectBuilders;
using SEWorldGenPlugin.Utilities;
using System;
using System.Collections.Generic;
using VRageMath;

namespace SEWorldGenPlugin.Generator.AsteroidObjects.AsteroidSphere
{
    /// <summary>
    /// Provider class for a hollow sphere asteroid object
    /// </summary>
    [EventOwner]
    public class MyAsteroidSphereProvider : MyAbstractAsteroidObjectProvider
    {
        /// <summary>
        /// The type name, for the system asteroid object, this class provides
        /// </summary>
        public static readonly string TYPE_NAME = "AsteroidHollowSphere";

        /// <summary>
        /// Static instance of this provider, so that there is only one provider existent ever.
        /// </summary>
        public static MyAsteroidSphereProvider Static;

        /// <summary>
        /// The dictionary contains all currently loaded asteroid spheres
        /// </summary>
        private Dictionary<Guid, MyAsteroidSphereData> m_loadedSpheres;

        public MyAsteroidSphereProvider()
        {
            if(Static == null)
            {
                m_loadedSpheres = new Dictionary<Guid, MyAsteroidSphereData>();

                Static = this;
            }
            else
            {
                m_loadedSpheres = Static.m_loadedSpheres;

                Static = this;
            }
        }

        public override MySystemAsteroids GenerateInstance(int systemIndex, in MySystemObject systemParent, double objectOrbitRadius)
        {
            return null;
        }

        public override IMyAsteroidObjectShape GetAsteroidObjectShape(MySystemAsteroids instance)
        {
            if (m_loadedSpheres.ContainsKey(instance.Id))
                return MyAsteroidObjectShapeSphere.CreateFromAsteroidSphereData(m_loadedSpheres[instance.Id]);
            return null;
        }

        public override string GetTypeName()
        {
            return TYPE_NAME;
        }

        public override bool IsSystemGeneratable()
        {
            return false;
        }

        public override void OnSave()
        {
            foreach(var sphere in m_loadedSpheres)
            {
                if (MyStarSystemGenerator.Static.StarSystem == null) return;

                var instance = MyStarSystemGenerator.Static.StarSystem.GetObjectById(sphere.Key);
                if (instance == null) continue;
                MyFileUtils.WriteXmlFileToWorld(sphere.Value, GetFileName(instance.DisplayName), typeof(MyAsteroidSphereProvider));
            }
        }

        public override bool RemoveInstance(MySystemAsteroids systemInstance)
        {
            MyPluginLog.Debug("Removing instance from asteroid sphere provider");

            if (systemInstance.AsteroidTypeName != GetTypeName()) return false;
            if (!m_loadedSpheres.ContainsKey(systemInstance.Id)) return false;

            m_loadedSpheres.Remove(systemInstance.Id);

            MyFileUtils.DeleteFileInWorldStorage(GetFileName(systemInstance.DisplayName), typeof(MyAsteroidSphereProvider));

            MyPluginLog.Debug("Successfully removed instance");

            return true;
        }

        public override bool TryLoadObject(MySystemAsteroids asteroid)
        {
            if (m_loadedSpheres.ContainsKey(asteroid.Id)) return false;

            if (!MyFileUtils.FileExistsInWorldStorage(GetFileName(asteroid.DisplayName), typeof(MyAsteroidSphereProvider))) return false;

            var data = MyFileUtils.ReadXmlFileFromWorld<MyAsteroidSphereData>(GetFileName(asteroid.DisplayName), typeof(MyAsteroidSphereProvider));

            m_loadedSpheres.Add(asteroid.Id, data);

            return true;
        }

        protected override IMyAsteroidAdminMenuCreator CreateAdminMenuCreatorInstance()
        {
            return new MyAsteroidSphereAdminMenu();
        }

        /// <summary>
        /// Adds a new instance to this provider, if it doesnt exist
        /// </summary>
        /// <param name="instance">System object instance</param>
        /// <param name="data">Data for sphere</param>
        /// <param name="callback">Callback when action is complete</param>
        public void AddInstance(MySystemAsteroids instance, MyAsteroidSphereData data, Action<bool> callback)
        {
            if (data == null || instance == null)
            {
                callback?.Invoke(false);
            }
            MyStarSystemGenerator.Static.AddObjectToSystem(instance, instance.ParentId, delegate (bool success)
            {
                if (success)
                {
                    MyPluginLog.Debug("Successfully added asteroid object to system");

                    PluginEventHandler.Static.RaiseStaticEvent(AddInstanceServer, instance, data);
                    callback?.Invoke(true);
                }
                else
                {
                    callback?.Invoke(false);
                }
            });
        }

        public override object GetInstanceData(MySystemAsteroids instance)
        {
            if (m_loadedSpheres.ContainsKey(instance.Id))
            {
                return m_loadedSpheres[instance.Id];
            }

            return null;
        }

        public override void FetchDataFromServer()
        {
            PluginEventHandler.Static.RaiseStaticEvent(GetDataServer, Sync.MyId);
        }

        /// <summary>
        /// Server event: Adds instance
        /// </summary>
        /// <param name="instance">Instance of system object</param>
        /// <param name="data">Instance data</param>
        [Event(3001)]
        [Server]
        private static void AddInstanceServer(MySystemAsteroids instance, MyAsteroidSphereData data)
        {
            MyPluginLog.Log("Adding asteroid sphere instance on server");

            Static?.m_loadedSpheres.Add(instance.Id, data);
            PluginEventHandler.Static.RaiseStaticEvent(NotifyAddInstance, instance.Id, data);
        }

        /// <summary>
        /// Server event: Get all data saved in this provider for the requester client
        /// </summary>
        /// <param name="requesterId">Client that requested data</param>
        [Event(3002)]
        [Server]
        private static void GetDataServer(ulong requesterId)
        {
            MyPluginLog.Log("Retreiving all asteroid sphere data for client " + requesterId);

            PluginEventHandler.Static.RaiseStaticEvent(GetDataClient, Static.m_loadedSpheres, requesterId);
        }

        /// <summary>
        /// Client event: Called, when data retreived from server
        /// </summary>
        /// <param name="data">Data of this provider from server</param>
        [Event(3003)]
        [Client]
        private static void GetDataClient(Dictionary<Guid, MyAsteroidSphereData> data)
        {
            if (Sync.IsServer) return;

            MyPluginLog.Log("Received data from server for asteroid sphere provider with length " + data.Count);

            foreach (var pair in data)
            {
                if (Static.m_loadedSpheres.ContainsKey(pair.Key)) continue;

                MyPluginLog.Debug("Adding sphere to provider fetched from server " + pair.Key.ToString());

                Static.m_loadedSpheres.Add(pair.Key, pair.Value);
            }
        }

        /// <summary>
        /// Broadcast: Called, when instance got added on server
        /// </summary>
        /// <param name="id">Id of instance</param>
        /// <param name="data">Data of instance</param>
        [Event(3004)]
        [Broadcast]
        private static void NotifyAddInstance(Guid id, MyAsteroidSphereData data)
        {
            if (Sync.IsServer) return;

            MyPluginLog.Log("Got notified about a new Asteroid sphere instance");

            if (Static.m_loadedSpheres.ContainsKey(id)) return;

            Static.m_loadedSpheres.Add(id, data);
        }
    }

    /// <summary>
    /// Class for additional data to save an asteroid object hollow sphere
    /// </summary>
    [ProtoContract]
    [Serializable]
    public class MyAsteroidSphereData
    {
        /// <summary>
        /// The center of the hollow sphere asteroids object
        /// </summary>
        [ProtoMember(1)]
        public Vector3D Center;

        /// <summary>
        /// The inner radius for the hollow sphere of asteroids
        /// </summary>
        [ProtoMember(2)]
        public double InnerRadius;

        /// <summary>
        /// The outer radius for the hollow sphere asteroids
        /// </summary>
        [ProtoMember(3)]
        public double OuterRadius;
    }
}
