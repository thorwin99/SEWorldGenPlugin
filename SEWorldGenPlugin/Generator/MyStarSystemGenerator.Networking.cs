using Sandbox.Game.Entities;
using Sandbox.Game.Multiplayer;
using SEWorldGenPlugin.Generator.AsteroidObjects;
using SEWorldGenPlugin.Networking;
using SEWorldGenPlugin.Networking.Attributes;
using SEWorldGenPlugin.ObjectBuilders;
using SEWorldGenPlugin.Session;
using SEWorldGenPlugin.Utilities;
using System;
using VRageMath;

namespace SEWorldGenPlugin.Generator
{
    /// <summary>
    /// Networking part of the MySystemGenerator class
    /// </summary>
    public partial class MyStarSystemGenerator
    {
        /// <summary>
        /// Adds a new object to the system. If <paramref name="parentId"/> is provided, it will be added as a child to that
        /// object, else it will be added as a child to the sun.
        /// If no sun is present, an empty one will be created.
        /// </summary>
        /// <param name="systemObject">Object to add</param>
        /// <param name="parentId">Id of the parent object</param>
        /// <param name="callback">The success callback for this action</param>
        public void AddObjectToSystem(MySystemObject systemObject, Guid? parentId = null, Action<bool> callback = null)
        {
            uint callbackId = PluginEventHandler.Static.AddNetworkCallback(callback);
            Guid parent = Guid.Empty;
            if(StarSystem.CenterObject != null)
            {
                parent = parentId ?? StarSystem.CenterObject.Id;
            }
            else
            {
                parent = parentId ?? Guid.Empty;
            }

            PluginEventHandler.Static.RaiseStaticEvent(SendAddSystemObjectServer, systemObject, parent, callbackId, Sync.MyId);
        }

        /// <summary>
        /// Removes an object from the system.
        /// </summary>
        /// <param name="objectId">Id of the object to remove</param>
        /// <param name="callback">The success callback for this action</param>
        public void RemoveObjectFromSystem(Guid objectId, Action<bool> callback = null)
        {
            uint callbackId = PluginEventHandler.Static.AddNetworkCallback(callback);

            PluginEventHandler.Static.RaiseStaticEvent(SendRemoveSystemObjectServer, objectId, callbackId, Sync.MyId);
        }

        /// <summary>
        /// Gets the current star system as represented on the server
        /// </summary>
        public void GetStarSystemFromServer()
        {
            PluginEventHandler.Static.RaiseStaticEvent(SendGetStarSystemServer, Sync.MyId);
        }

        /// <summary>
        /// Renames an object in the star system.
        /// </summary>
        /// <param name="objectId">Id of the object to rename</param>
        /// <param name="newName">New name of the object</param>
        /// <param name="callback">The success callback for this action</param>
        public void RenameObject(Guid objectId, string newName, Action<bool> callback = null)
        {
            uint callbackId = PluginEventHandler.Static.AddNetworkCallback(callback);

            PluginEventHandler.Static.RaiseStaticEvent(SendRenameObjectServer, objectId, newName, callbackId, Sync.MyId);
        }

        /// <summary>
        /// Server Event: Adds a new system object to the system on the server.
        /// </summary>
        /// <param name="obj">Object to add</param>
        /// <param name="parentId">Parent of the object to which it is a child</param>
        /// <param name="callbackId">The id of the success callback on the client</param>
        /// <param name="senderId">The id of the client submitting this request</param>
        [Event(100)]
        [Server]
        private static void SendAddSystemObjectServer(MySystemObject obj, Guid parentId, uint callbackId, ulong senderId)
        {
            MyPluginLog.Log("Server: Add object " + obj.DisplayName + " to system");

            Action<bool> callback = null;

            if (senderId == Sync.MyId)
                callback = PluginEventHandler.Static.GetNetworkCallback(callbackId);

            if (obj != null)
            {
                if (!Static.StarSystem.Contains(obj.Id))
                {
                    var parent = Static.StarSystem.GetById(parentId);
                    if (parent != null)
                    {
                        parent.ChildObjects.Add(obj);
                        obj.ParentId = parentId;
                        PluginEventHandler.Static.RaiseStaticEvent(BroadcastObjectAdded, obj, callbackId, senderId);
                        callback?.Invoke(true);
                    }
                    else
                    {
                        if(Static.StarSystem.CenterObject != null)
                        {
                            Static.StarSystem.CenterObject.ChildObjects.Add(obj);
                            obj.ParentId = Static.StarSystem.CenterObject.Id;
                            PluginEventHandler.Static.RaiseStaticEvent(BroadcastObjectAdded, obj, callbackId, senderId);
                            callback?.Invoke(true);
                        }
                        else
                        {
                            if(obj.CenterPosition != Vector3D.Zero)
                            {
                                MySystemObject sun = new MySystemObject();
                                sun.DisplayName = "Center";
                                sun.Type = MySystemObjectType.EMPTY;
                                sun.ParentId = Guid.Empty;

                                Static.StarSystem.CenterObject = sun;

                                obj.ParentId = sun.Id;
                                Static.StarSystem.CenterObject.ChildObjects.Add(obj);

                                PluginEventHandler.Static.RaiseStaticEvent(BroadcastObjectAdded, obj, callbackId, senderId);
                                callback?.Invoke(true);
                            }
                            else
                            {
                                Static.StarSystem.CenterObject = obj;
                                obj.ParentId = Guid.Empty;
                                PluginEventHandler.Static.RaiseStaticEvent(BroadcastObjectAdded, obj, callbackId, senderId);
                                callback?.Invoke(true);
                            }
                        }
                    }
                    return;
                }
            }

            PluginEventHandler.Static.RaiseStaticEvent(SendNetActionFailed, callbackId, senderId);
            callback?.Invoke(false);
        }

        /// <summary>
        /// Broadcast Event: Adds object to local Star system to reflect server changes.
        /// </summary>
        /// <param name="obj">The object to add.</param>
        /// <param name="callbackId">The id of the success callback on the client</param>
        /// <param name="senderId">The id of the client submitting this request</param>
        [Event(101)]
        [Broadcast]
        private static void BroadcastObjectAdded(MySystemObject obj, uint callbackId, ulong senderId)
        {
            if (Sync.IsServer) return;

            MyPluginLog.Log("Received update for Star System, adding object " + obj.Id + " to parent " + obj.ParentId);
            Action<bool> callback = null;

            if (senderId == Sync.MyId)
                callback = PluginEventHandler.Static.GetNetworkCallback(callbackId);

            var parent = Static.StarSystem.GetById(obj.ParentId);

            if(!Static.StarSystem.Contains(obj.Id) && parent != null)
            {
                parent.ChildObjects.Add(obj);
                callback?.Invoke(true);
            }
            else if(obj.ParentId == Guid.Empty && Static.StarSystem.CenterObject == null)
            {
                if(obj.CenterPosition != Vector3D.Zero)
                {
                    MySystemObject sun = new MySystemObject();
                    sun.DisplayName = "Center";
                    sun.Type = MySystemObjectType.EMPTY;
                    sun.ParentId = Guid.Empty;

                    Static.StarSystem.CenterObject = sun;

                    obj.ParentId = sun.Id;
                    Static.StarSystem.CenterObject.ChildObjects.Add(obj);

                    callback?.Invoke(true);
                }
                else
                {
                    Static.StarSystem.CenterObject = obj;
                    callback?.Invoke(true);
                }
            }
            else
            {
                MyPluginLog.Log("The starsystem is out of sync with the server. Requesting full fetch of the system");
                Static.GetStarSystemFromServer();
                callback?.Invoke(false);
            }
        }

        /// <summary>
        /// Server Event: Removes a system object from the servers system data. Cant be the center object
        /// </summary>
        /// <param name="objectId">Id of the object</param>
        /// <param name="callbackId">The id of the success callback on the client</param>
        /// <param name="senderId">The id of the client submitting this request</param>
        [Event(102)]
        [Server]
        private static void SendRemoveSystemObjectServer(Guid objectId, uint callbackId, ulong senderId)
        {
            MyPluginLog.Log("Server: Removing object " + objectId.ToString() + " from system");
            MySystemObject o = Static.StarSystem.GetById(objectId);

            Action<bool> callback = null;

            if (senderId == Sync.MyId)
                callback = PluginEventHandler.Static.GetNetworkCallback(callbackId);

            if (o != null && o.DisplayName != Static.StarSystem.CenterObject.DisplayName)
            {
                foreach(var child in o.GetAllChildren())
                {
                    if (child is MySystemPlanet)
                    {
                        MyEntities.Remove(MyEntities.GetEntityById((o as MySystemPlanet).EntityId));
                    }
                }
                if(o.Type == MySystemObjectType.ASTEROIDS)
                {
                    MySystemAsteroids roid = o as MySystemAsteroids;
                    MyAbstractAsteroidObjectProvider prov;
                    if(MyAsteroidObjectsManager.Static.AsteroidObjectProviders.TryGetValue(roid.AsteroidTypeName, out prov))
                    {
                        if(callback != null)
                        {
                            callbackId = PluginEventHandler.Static.AddNetworkCallback(callback);
                        }
                        prov.RemoveInstance(o.Id, callbackId, senderId);
                    }
                    return;
                }
                else
                {
                    MyGPSManager.Static.RemovePersistentGps(objectId);
                    if (Static.StarSystem.Remove(objectId))
                    {
                        if(o is MySystemPlanet)
                        {
                            MyEntities.Remove(MyEntities.GetEntityById((o as MySystemPlanet).EntityId));
                        }

                        PluginEventHandler.Static.RaiseStaticEvent(BroadcastRemovedObject, objectId, callbackId, senderId);
                        callback?.Invoke(true);
                    }
                    return;
                }
            }

            PluginEventHandler.Static.RaiseStaticEvent(SendNetActionFailed, callbackId, senderId);
            callback?.Invoke(false);
        }

        /// <summary>
        /// Broadcast Event: Notifies all clients that the object <paramref name="objectId"/> was removed.
        /// </summary>
        /// <param name="objectId">The object Id</param>
        /// <param name="callbackId">The id of the success callback on the client</param>
        /// <param name="senderId">The id of the client submitting this request</param>
        [Event(103)]
        [Broadcast]
        private static void BroadcastRemovedObject(Guid objectId, uint callbackId, ulong senderId)
        {
            if (Sync.IsServer) return;

            MyPluginLog.Log("Received update for Star System, removing object " + objectId);

            Action<bool> callback = null;

            if (senderId == Sync.MyId)
                callback = PluginEventHandler.Static.GetNetworkCallback(callbackId);

            if (Static.StarSystem.Contains(objectId))
            {
                Static.StarSystem.Remove(objectId);
                callback?.Invoke(true);
            }
            else
            {
                MyPluginLog.Log("The starsystem is out of sync with the server. Requesting full fetch of the system");
                Static.GetStarSystemFromServer();
                callback?.Invoke(false);
            }
        }

        /// <summary>
        /// Server Event: Retreives the whole star system
        /// </summary>
        /// <param name="clientId">Id of the client, that requested the system</param>
        [Event(104)]
        [Server]
        private static void SendGetStarSystemServer(ulong clientId)
        {
            MyPluginLog.Log("Server: Get star system for client " + clientId);
            PluginEventHandler.Static.RaiseStaticEvent(SendGetStarSystemClient, Static.StarSystem, clientId);
        }

        /// <summary>
        /// Client Event: Receives star system from server
        /// </summary>
        /// <param name="starSystem">Star system that was retreived</param>
        [Event(105)]
        [Client]
        private static void SendGetStarSystemClient(MyObjectBuilder_SystemData starSystem)
        {
            MyPluginLog.Log("Client: Received star system");
            Static.StarSystem = starSystem;
        }

        /// <summary>
        /// Event to rename the object with id <paramref name="callbackId"/> to have the name <paramref name="newName"/>
        /// </summary>
        /// <param name="objectId">Id of the object</param>
        /// <param name="newName">New name of the object</param>
        /// <param name="callbackId">The id of the success callback on the client</param>
        /// <param name="senderId">The id of the client submitting this request</param>
        [Event(106)]
        [Server]
        private static void SendRenameObjectServer(Guid objectId, string newName, uint callbackId, ulong senderId)
        {
            MyPluginLog.Log("Server: Renaming object " + objectId.ToString() + " to " + newName);

            Action<bool> callback = null;
            if (senderId == Sync.MyId)
                callback = PluginEventHandler.Static.GetNetworkCallback(callbackId);

            if (Static.StarSystem.Contains(objectId))
            {
                MySystemObject obj = Static.StarSystem.GetById(objectId);
                string oldName = obj.DisplayName + "";
                obj.DisplayName = newName;

                if(obj.Type == MySystemObjectType.ASTEROIDS)
                {
                    MySystemAsteroids roid = obj as MySystemAsteroids;
                    MyAbstractAsteroidObjectProvider prov;
                    if(MyAsteroidObjectsManager.Static.AsteroidObjectProviders.TryGetValue(roid.AsteroidTypeName, out prov))
                    {
                        prov.InstanceRenamed(roid, oldName);
                    }
                    else
                    {
                        MyPluginLog.Log("Asteroid object provider " + roid.AsteroidTypeName + " does not exist. Renaming anyway...");
                    }
                }

                PluginEventHandler.Static.RaiseStaticEvent(BroadcastRenamedObject, objectId, newName, callbackId, senderId);
                callback?.Invoke(true);
                return;
            }

            PluginEventHandler.Static.RaiseStaticEvent(SendNetActionFailed, callbackId, senderId);
            callback?.Invoke(false);
        }

        /// <summary>
        /// Broadcast Event: Notifies all clients that the name of <paramref name="objectId"/> changed.
        /// </summary>
        /// <param name="objectId">The object Id</param>
        /// <param name="newName">The new Name</param>
        /// <param name="callbackId">The id of the success callback on the client</param>
        /// <param name="senderId">The id of the client submitting this request</param>
        [Event(107)]
        [Broadcast]
        private static void BroadcastRenamedObject(Guid objectId, string newName, uint callbackId, ulong senderId)
        {
            if (Sync.IsServer) return;

            MyPluginLog.Log("Received update for Star System, renaming object " + objectId + " to " + newName);

            Action<bool> callback = null;
            if (senderId == Sync.MyId)
                callback = PluginEventHandler.Static.GetNetworkCallback(callbackId);

            if (Static.StarSystem.Contains(objectId))
            {
                Static.StarSystem.GetById(objectId).DisplayName = newName;

                callback?.Invoke(true);
            }
            else
            {
                MyPluginLog.Log("The starsystem is out of sync with the server. Requesting full fetch of the system");
                Static.GetStarSystemFromServer();

                callback?.Invoke(false);
            }
        }

        /// <summary>
        /// Network event when an action called from a client failed.
        /// Used to notify client about failure.
        /// </summary>
        /// <param name="callbackId">Id of the callback for that action.</param>
        [Event(199)]
        [Client]
        private static void SendNetActionFailed(uint callbackId)
        {
            if (Sync.IsServer) return;

            Action<bool> callback = null;
            callback = PluginEventHandler.Static.GetNetworkCallback(callbackId);

            callback?.Invoke(false);
        }
    }
}
