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
        /// Callback dictionaries and indexes, to track callbacks
        private Dictionary<ulong, Action<bool, MySystemObject>> m_getActionCallbacks;
        private Dictionary<ulong, Action<bool>> m_simpleActionsCallbacks;
        private Dictionary<ulong, Action<MyObjectBuilder_SystemData>> m_getSystemCallbacks;
        private ulong m_currentGetIndex;
        private ulong m_currentSimpleIndex;
        private ulong m_getSystemIndex;

        /// <summary>
        /// Initializes networking for the system generator
        /// </summary>
        private void LoadNetworking()
        {
            m_getActionCallbacks = new Dictionary<ulong, Action<bool, MySystemObject>>();
            m_simpleActionsCallbacks = new Dictionary<ulong, Action<bool>>();
            m_getSystemCallbacks = new Dictionary<ulong, Action<MyObjectBuilder_SystemData>>();
            m_currentSimpleIndex = 0;
            m_currentGetIndex = 0;
            m_getSystemIndex = 0;
        }

        /// <summary>
        /// Unloads networking data
        /// </summary>
        private void UnloadNetworking()
        {
            m_getActionCallbacks.Clear();
            m_simpleActionsCallbacks.Clear();
            m_getSystemCallbacks.Clear();

            m_currentGetIndex = 0;
            m_currentSimpleIndex = 0;
            m_getSystemIndex = 0;
        }

        /// <summary>
        /// Retreives a system object by name from the server.
        /// </summary>
        /// <param name="id">Id of the object.</param>
        /// <param name="callback">Callback to run, when object is retreived.</param>
        public void GetSystemObjectById(Guid id, Action<bool, MySystemObject> callback)
        {
            m_getActionCallbacks.Add(++m_currentGetIndex, callback);
            PluginEventHandler.Static.RaiseStaticEvent(SendGetObjectServer, Sync.MyId, id, m_currentGetIndex);
        }

        /// <summary>
        /// Adds a new object to the system. If parentName is provided, it will be added as a child to that
        /// object, else it will be added as a child to the sun.
        /// </summary>
        /// <param name="systemObject">Object to add</param>
        /// <param name="parentId">Id of the parent object</param>
        /// <param name="callback">Callback to run when object was added</param>
        public void AddObjectToSystem(MySystemObject systemObject, Guid? parentId = null, Action<bool> callback = null)
        {
            if (systemObject.Type == MySystemObjectType.MOON) return;
            m_simpleActionsCallbacks.Add(++m_currentSimpleIndex, callback);
            PluginEventHandler.Static.RaiseStaticEvent(SendAddSystemObjectServer, systemObject, parentId == null ? Guid.Empty : parentId.Value, m_currentSimpleIndex, Sync.MyId);
        }

        /// <summary>
        /// Removes an object from the system.
        /// </summary>
        /// <param name="objectId">Id of the object to remove</param>
        /// <param name="callback">Callback to run, if the object was removed or not</param>
        public void RemoveObjectFromSystem(Guid objectId, Action<bool> callback = null)
        {
            m_simpleActionsCallbacks.Add(++m_currentSimpleIndex, callback);
            PluginEventHandler.Static.RaiseStaticEvent(SendRemoveSystemObjectServer, objectId, m_currentSimpleIndex, Sync.MyId);
        }

        /// <summary>
        /// Gets the current star system as represented on the server
        /// </summary>
        /// <param name="callback">Callback to call, when star system was retreived</param>
        public void GetStarSystem(Action<MyObjectBuilder_SystemData> callback)
        {
            MyPluginLog.Debug("Get star system");
            m_getSystemCallbacks.Add(++m_getSystemIndex, callback);
            PluginEventHandler.Static.RaiseStaticEvent(SendGetStarSystemServer, m_getSystemIndex, Sync.MyId);
        }

        /// <summary>
        /// Server Event: Tries to get the system object with given name and send it back to client
        /// </summary>
        /// <param name="clientId">The client, that requests the object</param>
        /// <param name="id">The id of the object</param>
        /// <param name="callbackId">The callback, that should get called on the client</param>
        [Event(100)]
        [Server]
        private static void SendGetObjectServer(ulong clientId, Guid id, ulong callbackId)
        {
            bool success = true;
            if (Static != null)
            {
                MySystemObject res = Static.StarSystem.GetObjectById(id);
                if(res == null)
                {
                    res = new MySystemObject();
                    success = false;
                }
                PluginEventHandler.Static.RaiseStaticEvent(SendGetObjectClient, success, res, callbackId, clientId);
            }
            else
            {
                PluginEventHandler.Static.RaiseStaticEvent(SendGetObjectClient, false, new MySystemObject(), callbackId, clientId);
            }
        }

        /// <summary>
        /// Client Event: Sends a system object to the client
        /// </summary>
        /// <param name="success">If getting the object was successfult</param>
        /// <param name="obj">The object to send</param>
        /// <param name="callbackId">The callback to call on client</param>
        [Event(101)]
        [Client]
        private static void SendGetObjectClient(bool success, MySystemObject obj, ulong callbackId)
        {
            if (Static.m_getActionCallbacks.ContainsKey(callbackId))
            {
                Static.m_getActionCallbacks[callbackId](success, obj);
                Static.m_getActionCallbacks.Remove(callbackId);
            }
        }

        /// <summary>
        /// Server Event: Adds a new system object to the system on the server.
        /// </summary>
        /// <param name="obj">Object to add</param>
        /// <param name="parentId">Parent of the object to which it is a child</param>
        /// <param name="callbackId">Id of the callback to run</param>
        /// <param name="clientId">Id of the client, that requested this</param>
        [Event(102)]
        [Server]
        private static void SendAddSystemObjectServer(MySystemObject obj, Guid parentId, ulong callbackId, ulong clientId)
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
                        PluginEventHandler.Static.RaiseStaticEvent(SendSimpleActionCallbackClient, true, callbackId, clientId);
                    }
                    else
                    {
                        Static.StarSystem.CenterObject.ChildObjects.Add(obj);
                        obj.ParentId = Static.StarSystem.CenterObject.Id;
                        PluginEventHandler.Static.RaiseStaticEvent(SendSimpleActionCallbackClient, true, callbackId, clientId);
                    }

                    Static.AddAllPersistentGps();
                }
            }
            PluginEventHandler.Static.RaiseStaticEvent(SendSimpleActionCallbackClient, false, callbackId, clientId);
        }

        /// <summary>
        /// Client Event: Callback, if a simple action was executed sucessfully 
        /// </summary>
        /// <param name="success">If the action was sucessfull</param>
        /// <param name="callbackId">Id of the callback to run</param>
        [Event(103)]
        [Server]
        private static void SendSimpleActionCallbackClient(bool success, ulong callbackId)
        {
            MyPluginLog.Log("Client: Getting simple callback with success=" + success + " from server");
            if (Static.m_simpleActionsCallbacks.ContainsKey(callbackId))
            {
                Static.m_simpleActionsCallbacks[callbackId](success);
                Static.m_simpleActionsCallbacks.Remove(callbackId);
            }
        }

        /// <summary>
        /// Server Event: Removes a system object from the servers system data. Cant be the center object
        /// </summary>
        /// <param name="objectName">Name of the object</param>
        /// <param name="callbackId">Id of the callback</param>
        /// <param name="clientId">Id of the client, that send this request</param>
        [Event(104)]
        [Server]
        private static void SendRemoveSystemObjectServer(Guid objectId, ulong callbackId, ulong clientId)
        {
            MyPluginLog.Log("Server: Removing object " + objectId.ToString() + " from system");
            MySystemObject o = Static.StarSystem.GetObjectById(objectId);
            if (o != null && o.DisplayName != Static.StarSystem.CenterObject.DisplayName)
            {
                MySystemObject parent = Static.StarSystem.GetObjectById(o.Id);
                if (parent != null)
                {
                    if(o.Type == MySystemObjectType.ASTEROIDS)
                    {
                        var asteroids = o as MySystemAsteroids;
                        if (MyAsteroidObjectsManager.Static.AsteroidObjectProviders.ContainsKey(asteroids.AsteroidTypeName))
                        {
                            bool removed = MyAsteroidObjectsManager.Static.AsteroidObjectProviders[asteroids.AsteroidTypeName].RemoveInstance(asteroids);
                            if (removed)
                            {
                                parent.ChildObjects.Remove(o);
                                PluginEventHandler.Static.RaiseStaticEvent(SendSimpleActionCallbackClient, true, callbackId, clientId);
                                return;
                            }
                        }
                    }
                    else
                    {
                        parent.ChildObjects.Remove(o);
                        PluginEventHandler.Static.RaiseStaticEvent(SendSimpleActionCallbackClient, true, callbackId, clientId);
                        return;
                    }
                }
            }
            PluginEventHandler.Static.RaiseStaticEvent(SendSimpleActionCallbackClient, false, callbackId, clientId);
        }

        /// <summary>
        /// Server Event: Retreives the whole star system
        /// </summary>
        /// <param name="callbackId">Id of the callback</param>
        /// <param name="clientId">Id of the client, that requested the system</param>
        [Event(105)]
        [Server]
        private static void SendGetStarSystemServer(ulong callbackId, ulong clientId)
        {
            MyPluginLog.Debug("Server: Get star system");
            PluginEventHandler.Static.RaiseStaticEvent(SendGetStarSystemClient, Static.StarSystem, callbackId, clientId);
        }

        /// <summary>
        /// Client Event: Calls callback for GetStarSystem event.
        /// </summary>
        /// <param name="starSystem">Star system that was retreived</param>
        /// <param name="callbackId">Callback that will be called with the star system parameter</param>
        [Event(106)]
        [Client]
        private static void SendGetStarSystemClient(MyObjectBuilder_SystemData starSystem, ulong callbackId)
        {
            MyPluginLog.Debug("Client: Received star system");
            if (Static.m_getSystemCallbacks.ContainsKey(callbackId))
            {
                Static.m_getSystemCallbacks[callbackId](starSystem);
                Static.m_getSystemCallbacks.Remove(callbackId);
            }
        }
    }
}
