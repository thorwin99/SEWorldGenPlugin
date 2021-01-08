using Sandbox.Game.Multiplayer;
using SEWorldGenPlugin.Networking;
using SEWorldGenPlugin.Networking.Attributes;
using SEWorldGenPlugin.ObjectBuilders;
using System;
using System.Collections.Generic;

namespace SEWorldGenPlugin.Generator
{
    /// <summary>
    /// Networking part of the MySystemGenerator class
    /// </summary>
    public partial class MyStarSystemGenerator
    {
        private Dictionary<ulong, Action<bool, MySystemObject>> m_getActionCallbacks;
        private Dictionary<ulong, Action<bool>> m_addActionsCallbacks;
        private ulong m_currentGetIndex;
        private ulong m_currentAddIndex;

        private void LoadNetworking()
        {
            m_getActionCallbacks = new Dictionary<ulong, Action<bool, MySystemObject>>();
            m_addActionsCallbacks = new Dictionary<ulong, Action<bool>>();
            m_currentAddIndex = 0;
            m_currentGetIndex = 0;
        }

        private void UnloadNetworking()
        {
            m_getActionCallbacks.Clear();
            m_addActionsCallbacks.Clear();
        }

        /// <summary>
        /// Retreives a system object by name from the server.
        /// </summary>
        /// <param name="displayName">Name of the object.</param>
        /// <param name="callback">Callback to run, when object is retreived.</param>
        public void GetSystemObjectByName(string displayName, Action<bool, MySystemObject> callback)
        {
            m_getActionCallbacks.Add(++m_currentGetIndex, callback);
            PluginEventHandler.Static.RaiseStaticEvent(SendGetObjectServer, Sync.MyId, displayName, m_currentGetIndex);
        }

        /// <summary>
        /// Adds a new object to the system. If parentName is provided, it will be added as a child to that
        /// object, else it will be added as a child to the sun.
        /// </summary>
        /// <param name="systemObject">Object to add</param>
        /// <param name="parentName">Name of the parent object</param>
        /// <param name="callback">Callback to run when object was added</param>
        public void AddObjectToSystem(MySystemObject systemObject, string parentName, Action<bool> callback)
        {
            if (systemObject.Type == MySystemObjectType.MOON) return;
            m_addActionsCallbacks.Add(++m_currentAddIndex, callback);
            PluginEventHandler.Static.RaiseStaticEvent(SendAddSystemObjectServer, systemObject, parentName, m_currentGetIndex, Sync.MyId);
        }

        /// <summary>
        /// Server Event: Tries to get the system object with given name and send it back to client
        /// </summary>
        /// <param name="clientId">The client, that requests the object</param>
        /// <param name="objectName">The name of the object</param>
        /// <param name="callbackId">The callback, that should get called on the client</param>
        [Event(100)]
        [Server]
        private static void SendGetObjectServer(ulong clientId, string objectName, ulong callbackId)
        {
            bool success = true;
            if (Static != null)
            {
                MySystemObject res = Static.StarSystem.FindObjectByName(objectName);
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
        /// <param name="obj"></param>
        /// <param name="callbackId"></param>
        /// <param name="clientId"></param>
        [Event(102)]
        [Server]
        private static void SendAddSystemObjectServer(MySystemObject obj, string parentName, ulong callbackId, ulong clientId)
        {
            if(obj != null)
            {
                if (!Static.StarSystem.ObjectExists(obj.DisplayName))
                {
                    var parent = Static.StarSystem.FindObjectByName(parentName);
                    if (parent != null)
                    {
                        parent.ChildObjects.Add(obj);
                        PluginEventHandler.Static.RaiseStaticEvent(SendAddSystemObjectClient, true, callbackId, clientId);
                    }
                    else
                    {
                        Static.StarSystem.CenterObject.ChildObjects.Add(obj);
                        PluginEventHandler.Static.RaiseStaticEvent(SendAddSystemObjectClient, true, callbackId, clientId);
                    }
                }
            }
            PluginEventHandler.Static.RaiseStaticEvent(SendAddSystemObjectClient, false, callbackId, clientId);
        }

        /// <summary>
        /// Client Event: Callback if the addition of an item was successfull.
        /// </summary>
        /// <param name="success">If the addition of an item was successfull</param>
        /// <param name="callbackId">Id of the callback to run</param>
        [Event(103)]
        [Server]
        private static void SendAddSystemObjectClient(bool success, ulong callbackId)
        {
            if(Static.m_addActionsCallbacks.ContainsKey(callbackId))
            {
                Static.m_addActionsCallbacks[callbackId](success);
                Static.m_addActionsCallbacks.Remove(callbackId);
            }
        }
    }
}
