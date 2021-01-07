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
        /// Adds a new primary system object to the system on the server. Cant be moon or ring.
        /// </summary>
        /// <param name="systemObject">Object to add</param>
        /// <param name="callback">Callback to run when object was added</param>
        public void AddObjectToSystem(MySystemObject systemObject, Action<bool> callback)
        {
            if (systemObject.Type == MySystemObjectType.MOON || systemObject.Type == MySystemObjectType.RING) return;
            m_addActionsCallbacks.Add(++m_currentAddIndex, callback);
        }

        [Event(100)]
        [Server]
        private static void SendGetObjectServer(ulong clientId, string objectName, ulong callbackId)
        {

        }
    }
}
