using Sandbox.Game.Multiplayer;
using SEWorldGenPlugin.Networking;
using SEWorldGenPlugin.Networking.Attributes;
using SEWorldGenPlugin.ObjectBuilders;
using SEWorldGenPlugin.Session;
using SEWorldGenPlugin.Utilities;
using System;
using System.Collections.Generic;

namespace SEWorldGenPlugin.Generator
{
    /// <summary>
    /// Networking part of the MySystemGenerator class
    /// </summary>
    public partial class MyStarSystemGenerator
    {
        /// <summary>
        /// Adds a new object to the system. If parentName is provided, it will be added as a child to that
        /// object, else it will be added as a child to the sun.
        /// </summary>
        /// <param name="systemObject">Object to add</param>
        /// <param name="parentId">Id of the parent object</param>
        /// <param name="callback">Callback to run when object was added</param>
        public void AddObjectToSystem(MySystemObject systemObject, Guid? parentId = null, Action<bool> callback = null)
        {
            PluginEventHandler.Static.RaiseStaticEvent(SendAddSystemObjectServer, systemObject, parentId == null ? Guid.Empty : parentId.Value);
        }

        /// <summary>
        /// Removes an object from the system.
        /// </summary>
        /// <param name="objectId">Id of the object to remove</param>
        /// <param name="callback">Callback to run, if the object was removed or not</param>
        public void RemoveObjectFromSystem(Guid objectId, Action<bool> callback = null)
        {
            PluginEventHandler.Static.RaiseStaticEvent(SendRemoveSystemObjectServer, objectId);
        }

        /// <summary>
        /// Gets the current star system as represented on the server
        /// </summary>
        /// <param name="callback">Callback to call, when star system was retreived</param>
        private void GetStarSystemFromServer()
        {
            MyPluginLog.Debug("Get star system");
            PluginEventHandler.Static.RaiseStaticEvent(SendGetStarSystemServer, Sync.MyId);
        }

        /// <summary>
        /// Renames an object in the star system.
        /// </summary>
        /// <param name="objectId">Id of the object to rename</param>
        /// <param name="newName">New name of the object</param>
        public void RenameObject(Guid objectId, string newName)
        {
            PluginEventHandler.Static.RaiseStaticEvent(SendRenameObjectServer, objectId, newName);
        }

        /// <summary>
        /// Server Event: Adds a new system object to the system on the server.
        /// </summary>
        /// <param name="obj">Object to add</param>
        /// <param name="parentId">Parent of the object to which it is a child</param>
        [Event(100)]
        [Server]
        private static void SendAddSystemObjectServer(MySystemObject obj, Guid parentId)
        {
            MyPluginLog.Log("Server: Add object " + obj.DisplayName + " to system");
            if (obj != null)
            {
                if (!Static.StarSystem.ObjectExists(obj.Id))
                {
                    var parent = Static.StarSystem.GetObjectById(parentId);
                    if (parent != null)
                    {
                        parent.ChildObjects.Add(obj);
                        obj.ParentId = parentId;
                        PluginEventHandler.Static.RaiseStaticEvent(BroadcastObjectAdded, obj);
                    }
                    else
                    {
                        Static.StarSystem.CenterObject.ChildObjects.Add(obj);
                        obj.ParentId = Static.StarSystem.CenterObject.Id;
                        PluginEventHandler.Static.RaiseStaticEvent(BroadcastObjectAdded, obj);
                    }

                    Static.AddAllPersistentGps();
                    return;
                }
            }
        }

        /// <summary>
        /// Broadcast Event: Adds object to local Star system to reflect server changes.
        /// </summary>
        /// <param name="obj">The object to add.</param>
        [Event(101)]
        [Broadcast]
        private static void BroadcastObjectAdded(MySystemObject obj)
        {
            if (Sync.IsServer) return;

            MyPluginLog.Log("Received update for Star System, adding object " + obj.Id + " to parent " + obj.ParentId);

            var parent = Static.StarSystem.GetObjectById(obj.ParentId);

            if(!Static.StarSystem.ObjectExists(obj.Id) && parent != null)
            {
                parent.ChildObjects.Add(obj);
            }
            else
            {
                MyPluginLog.Debug("The starsystem is out of sync with the server. Requesting full fetch of the system");
                Static.GetStarSystemFromServer();
            }
        }

        /// <summary>
        /// Server Event: Removes a system object from the servers system data. Cant be the center object
        /// </summary>
        /// <param name="objectId">Id of the object</param>
        [Event(102)]
        [Server]
        private static void SendRemoveSystemObjectServer(Guid objectId)
        {
            MyPluginLog.Log("Server: Removing object " + objectId.ToString() + " from system");
            MySystemObject o = Static.StarSystem.GetObjectById(objectId);
            if (o != null && o.DisplayName != Static.StarSystem.CenterObject.DisplayName)
            {
                if(o.Type == MySystemObjectType.ASTEROIDS)
                {
                    var asteroids = o as MySystemAsteroids;
                    if (MyAsteroidObjectsManager.Static.AsteroidObjectProviders.ContainsKey(asteroids.AsteroidTypeName))
                    {
                        bool removed = MyAsteroidObjectsManager.Static.AsteroidObjectProviders[asteroids.AsteroidTypeName].RemoveInstance(asteroids);
                        if (removed)
                        {
                            MyGPSManager.Static.RemovePersistentGps(objectId);
                            if (Static.StarSystem.RemoveObject(objectId))
                            {
                                PluginEventHandler.Static.RaiseStaticEvent(BroadcastRemovedObject, objectId);
                            }
                            return;
                        }
                    }
                }
                else
                {
                    MyGPSManager.Static.RemovePersistentGps(objectId);
                    if (Static.StarSystem.RemoveObject(objectId))
                    {
                        PluginEventHandler.Static.RaiseStaticEvent(BroadcastRemovedObject, objectId);
                    }
                    return;
                }
            }
        }

        /// <summary>
        /// Broadcast Event: Notifies all clients that the object <paramref name="objectId"/> was removed.
        /// </summary>
        /// <param name="objectId">The object Id</param>
        [Event(103)]
        [Broadcast]
        private static void BroadcastRemovedObject(Guid objectId)
        {
            if (Sync.IsServer) return;

            MyPluginLog.Log("Received update for Star System, removing object " + objectId);

            if (Static.StarSystem.ObjectExists(objectId))
            {
                Static.StarSystem.RemoveObject(objectId);
            }
            else
            {
                MyPluginLog.Log("The starsystem is out of sync with the server. Requesting full fetch of the system");
                Static.GetStarSystemFromServer();
            }
        }

        /// <summary>
        /// Server Event: Retreives the whole star system
        /// </summary>
        /// <param name="callbackId">Id of the callback</param>
        /// <param name="clientId">Id of the client, that requested the system</param>
        [Event(104)]
        [Server]
        private static void SendGetStarSystemServer(ulong clientId)
        {
            MyPluginLog.Debug("Server: Get star system");
            PluginEventHandler.Static.RaiseStaticEvent(SendGetStarSystemClient, Static.StarSystem, clientId);
        }

        /// <summary>
        /// Client Event: Calls callback for GetStarSystem event.
        /// </summary>
        /// <param name="starSystem">Star system that was retreived</param>
        /// <param name="callbackId">Callback that will be called with the star system parameter</param>
        [Event(105)]
        [Client]
        private static void SendGetStarSystemClient(MyObjectBuilder_SystemData starSystem)
        {
            MyPluginLog.Debug("Client: Received star system");
            Static.StarSystem = starSystem;
        }

        /// <summary>
        /// Event to rename the object with id <paramref name="callbackId"/> to have the name <paramref name="newName"/>
        /// </summary>
        /// <param name="objectId">Id of the object</param>
        /// <param name="newName">New name of the object</param>
        /// <param name="callbackId">ID of the client callback for success of this action</param>
        /// <param name="clientId">ID of the client that called this event</param>
        [Event(106)]
        [Server]
        private static void SendRenameObjectServer(Guid objectId, string newName)
        {
            MyPluginLog.Log("Server: Renaming object " + objectId.ToString() + " to " + newName);
            if (Static.StarSystem.ObjectExists(objectId))
            {
                Static.StarSystem.GetObjectById(objectId).DisplayName = newName;
                PluginEventHandler.Static.RaiseStaticEvent(BroadcastRenamedObject, objectId, newName);
                return;
            }
        }

        /// <summary>
        /// Broadcast Event: Notifies all clients that the name of <paramref name="objectId"/> changed.
        /// </summary>
        /// <param name="objectId">The object Id</param>
        /// <param name="newName">The new Name</param>
        [Event(107)]
        [Broadcast]
        private static void BroadcastRenamedObject(Guid objectId, string newName)
        {
            if (Sync.IsServer) return;

            MyPluginLog.Log("Received update for Star System, renaming object " + objectId + " to " + newName);

            if (Static.StarSystem.ObjectExists(objectId))
            {
                Static.StarSystem.GetObjectById(objectId).DisplayName = newName;
            }
            else
            {
                MyPluginLog.Log("The starsystem is out of sync with the server. Requesting full fetch of the system");#
                Static.GetStarSystemFromServer();
            }
        }
    }
}
