using Sandbox.Game.Multiplayer;
using SEWorldGenPlugin.Networking;
using SEWorldGenPlugin.Networking.Attributes;
using SEWorldGenPlugin.ObjectBuilders;
using SEWorldGenPlugin.Utilities;
using System;
using System.Collections.Generic;

namespace SEWorldGenPlugin.Generator
{
    /// <summary>
    /// Networking part of the SystemGenerator class
    /// </summary>
    public partial class SystemGenerator
    {
        private Dictionary<ulong, Action<bool, MySystemItem>> m_getActionCallbacks;
        private Dictionary<ulong, Action<bool>> m_addActionsCallbacks;
        private ulong m_currentIndex;
        private ulong m_currentAddIndex;

        /// <summary>
        /// Initializes a list for callbacks, for networking actions.
        /// </summary>
        private void InitNet()
        {
            m_getActionCallbacks = new Dictionary<ulong, Action<bool, MySystemItem>>();
            m_addActionsCallbacks = new Dictionary<ulong, Action<bool>>();
        }

        /// <summary>
        /// Initializes internal variables
        /// </summary>
        private void LoadNet()
        {
            m_currentIndex = 0;
        }

        /// <summary>
        /// Unloads all internal variables
        /// </summary>
        private void UnloadNet()
        {
            m_currentIndex = 0;
            m_getActionCallbacks = null;
            m_addActionsCallbacks = null;
        }

        /// <summary>
        /// Gets an object of the system with given name  from the server and calls callback on it.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="callback"></param>
        public void GetObject(string name, Action<bool, MySystemItem> callback)
        {
            m_getActionCallbacks.Add(++m_currentIndex, callback);
            PluginEventHandler.Static.RaiseStaticEvent(SendGetServer, Sync.MyId, name, m_currentIndex, null);
        }

        /// <summary>
        /// Adds a planet to the system on the server.
        /// </summary>
        /// <param name="planet">Planet to add</param>
        public void AddPlanet(MyPlanetItem planet, Action<bool> callback = null)
        {
            m_addActionsCallbacks.Add(++m_currentAddIndex, callback);
            PluginEventHandler.Static.RaiseStaticEvent(SendAddPlanet, planet, m_currentAddIndex, Sync.MyId, null);
        }

        /// <summary>
        /// Adds a ring to a planet on the server.
        /// </summary>
        /// <param name="name">Name of the planet to add the ring to</param>
        /// <param name="ring">Ring data to add</param>
        public void AddRingToPlanet(string name, MyPlanetRingItem ring, Action<bool> callback = null)
        {
            m_addActionsCallbacks.Add(++m_currentAddIndex, callback);
            PluginEventHandler.Static.RaiseStaticEvent(SendAddRingToPlanet, name, ring, m_currentAddIndex, Sync.MyId, null);
        }

        /// <summary>
        /// Removes the asteroid ring from a planet on the server
        /// </summary>
        /// <param name="name">Name of the ring</param>
        public void RemoveRingFromPlanet(string name, Action<bool> callback = null)
        {
            m_addActionsCallbacks.Add(++m_currentAddIndex, callback);
            PluginEventHandler.Static.RaiseStaticEvent(SendRemoveRingFromPlanet, name, m_currentAddIndex, Sync.MyId, null);
        }

        /// <summary>
        /// Server Event: Gets an object from the system, and sends it to the client, and passes the callback id through
        /// </summary>
        /// <param name="client">Client that requested the object</param>
        /// <param name="name">Name of the object</param>
        /// <param name="callback">Callback id of the callback to call on the client</param>
        [Event(100)]
        [Server]
        static void SendGetServer(ulong client, string name, ulong callback)
        {
            bool success = Static.TryGetObject(name, out MySystemItem item);
            if (item == null)
            {
                item = new MyPlanetItem();
            }
            switch (item.Type)
            {
                case LegacySystemObjectType.PLANET:
                    PluginEventHandler.Static.RaiseStaticEvent(SendGetPlanetClient, success, (MyPlanetItem)item, callback, client);
                    break;
                case LegacySystemObjectType.BELT:
                    PluginEventHandler.Static.RaiseStaticEvent(SendGetBeltClient, success, (MySystemBeltItem)item, callback, client);
                    break;
                case LegacySystemObjectType.RING:
                    PluginEventHandler.Static.RaiseStaticEvent(SendGetRingClient, success, (MyPlanetRingItem)item, callback, client);
                    break;
                case LegacySystemObjectType.MOON:
                    PluginEventHandler.Static.RaiseStaticEvent(SendGetMoonClient, success, (MyPlanetMoonItem)item, callback, client);
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Client Event: Sends a Planet item to the client
        /// </summary>
        /// <param name="success">Whether or not the planet is valid</param>
        /// <param name="item">The planet item</param>
        /// <param name="callback">Callback id of the callback to call</param>
        [Event(101)]
        [Client]
        static void SendGetPlanetClient(bool success, MyPlanetItem item, ulong callback)
        {
            Static.m_getActionCallbacks[callback](success, item);
            Static.m_getActionCallbacks.Remove(callback);
        }

        /// <summary>
        /// Client Event: Sends a Moon item to the client
        /// </summary>
        /// <param name="success">Whether or not the moon is valid</param>
        /// <param name="item">The moon item</param>
        /// <param name="callback">Callback id of the callback to call</param>
        [Event(102)]
        [Client]
        static void SendGetMoonClient(bool success, MyPlanetMoonItem item, ulong callback)
        {
            Static.m_getActionCallbacks[callback](success, item);
            Static.m_getActionCallbacks.Remove(callback);
        }

        /// <summary>
        /// Client Event: Sends a Belt item to the client
        /// </summary>
        /// <param name="success">Whether or not the belt is valid</param>
        /// <param name="item">The belt item</param>
        /// <param name="callback">Callback id of the callback to call</param>
        [Event(103)]
        [Client]
        static void SendGetBeltClient(bool success, MySystemBeltItem item, ulong callback)
        {
            Static.m_getActionCallbacks[callback](success, item);
            Static.m_getActionCallbacks.Remove(callback);
        }

        /// <summary>
        /// Client Event: Sends a ring item to the client
        /// </summary>
        /// <param name="success">Whether or not the ring is valid</param>
        /// <param name="item">The ring item</param>
        /// <param name="callback">Callback id of the callback to call</param>
        [Event(104)]
        [Client]
        static void SendGetRingClient(bool success, MyPlanetRingItem item, ulong callback)
        {
            Static.m_getActionCallbacks[callback](success, item);
            Static.m_getActionCallbacks.Remove(callback);
        }

        /// <summary>
        /// Server Event: Adds a ring to a planet on the server
        /// </summary>
        /// <param name="planetName">Name of the planet</param>
        /// <param name="ringBase">Ring item to add to the planet</param>
        [Event(105)]
        [Server]
        static void SendAddRingToPlanet(string planetName, MyPlanetRingItem ringBase, ulong callback, ulong client)
        {
            if(Static.TryGetObject(planetName, out MySystemItem obj))
            {
                if (obj.Type == LegacySystemObjectType.PLANET)
                {
                    MyPlanetItem planet = (MyPlanetItem)obj;
                    if(planet.PlanetRing == null)
                    {
                        ringBase.Center = planet.CenterPosition;
                        planet.PlanetRing = ringBase;

                        PluginEventHandler.Static.RaiseStaticEvent(SendAddCallbackClient, true, callback, client);
                        return;
                    }
                }
            }
            PluginEventHandler.Static.RaiseStaticEvent(SendAddCallbackClient, false, callback, client);
        }

        /// <summary>
        /// Server Event: Adds a planet to the solar system on the server
        /// </summary>
        /// <param name="planet">Planet item to add</param>
        [Event(106)]
        [Server]
        static void SendAddPlanet(MyPlanetItem planet, ulong callback, ulong client)
        {
            if(planet != null)
            {
                if (Static.TryGetObject(planet.DisplayName, out MySystemItem obj))
                {
                    if(obj.Type == LegacySystemObjectType.PLANET)
                    {
                        if (((MyPlanetItem)obj).Generated) return;
                    }
                    else
                    {
                        return;
                    }
                    Static.Objects.Remove(obj);
                }
                Static.Objects.Add(planet);

                PluginEventHandler.Static.RaiseStaticEvent(SendAddCallbackClient, true, callback, client);
                return;
            }
            PluginEventHandler.Static.RaiseStaticEvent(SendAddCallbackClient, false, callback, client);
        }

        /// <summary>
        /// Server Event: Removes a planet with given name from the solar system on the server
        /// </summary>
        /// <param name="planetName">Name of the planet</param>
        /// <param name="planetName">Name of the planet</param>
        [Event(107)]
        [Server]
        static void SendRemoveRingFromPlanet(string planetName, ulong callback, ulong client)
        {
            if (Static.TryGetObject(planetName, out MySystemItem obj))
            {
                if (obj.Type == LegacySystemObjectType.PLANET)
                {
                    MyPlanetItem planet = (MyPlanetItem)obj;
                    if (planet.PlanetRing != null)
                    {
                        planet.PlanetRing = null;
                        PluginEventHandler.Static.RaiseStaticEvent(SendAddCallbackClient, true, callback, client);
                        return;
                    }
                }
            }
            PluginEventHandler.Static.RaiseStaticEvent(SendAddCallbackClient, false, callback, client);
        }

        /// <summary>
        /// Client Event: Sends a boolean to the client to check if the add event was successfull.
        /// </summary>
        /// <param name="success">Whether or not the planet is valid</param>
        /// <param name="item">The planet item</param>
        /// <param name="callback">Callback id of the callback to call</param>
        [Event(108)]
        [Client]
        static void SendAddCallbackClient(bool success, ulong callback)
        {
            if(Static.m_addActionsCallbacks[callback] == null)
            {
                Static.m_addActionsCallbacks.Remove(callback);
                return;
            }
            Static.m_addActionsCallbacks[callback](success);
            Static.m_addActionsCallbacks.Remove(callback);
        }
    }
}
