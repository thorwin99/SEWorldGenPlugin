using Sandbox.Game.Multiplayer;
using SEWorldGenPlugin.Networking;
using SEWorldGenPlugin.Networking.Attributes;
using SEWorldGenPlugin.ObjectBuilders;
using SEWorldGenPlugin.Utilities;
using System;
using System.Collections.Generic;
using VRage.Utils;

namespace SEWorldGenPlugin.Generator
{
    public partial class SystemGenerator
    {
        private Dictionary<ulong, Action<bool, MySystemItem>> m_getCallbacks;
        private bool m_handshakeDone;
        private ulong m_currentIndex;

        private void InitNet()
        {
            m_getCallbacks = new Dictionary<ulong, Action<bool, MySystemItem>>();
        }

        private void LoadNet()
        {
            m_handshakeDone = false;
            m_currentIndex = 0;
        }

        private void UnloadNet()
        {
            m_handshakeDone = false;
            m_currentIndex = 0;
            m_getCallbacks = null;
        }

        public void GetObject(string name, Action<bool, MySystemItem> callback)
        {
            if (!m_handshakeDone)
            {
                NetUtil.PingServer(delegate
                {
                    m_handshakeDone = true;
                    m_getCallbacks.Add(++m_currentIndex, callback);
                    PluginEventHandler.Static.RaiseStaticEvent(SendGetServer, Sync.MyId, name, m_currentIndex, null);
                });
            }
            else
            {
                m_getCallbacks.Add(++m_currentIndex, callback);
                PluginEventHandler.Static.RaiseStaticEvent(SendGetServer, Sync.MyId, name, m_currentIndex, null);
            }
        }

        public void AddPlanet(MyPlanetItem planet)
        {
            if (!m_handshakeDone)
            {
                NetUtil.PingServer(delegate //Handshake needed to know, that server runs plugin
                {
                    m_handshakeDone = true;
                    PluginEventHandler.Static.RaiseStaticEvent(SendAddPlanet, planet, null);
                });

            }
            else
            {
                PluginEventHandler.Static.RaiseStaticEvent(SendAddPlanet, planet, null);
            }
        }

        public void AddRingToPlanet(string name, MyPlanetRingItem ring)
        {
            if (!m_handshakeDone)
            {
                NetUtil.PingServer(delegate //Handshake needed to check, if plugin is installed on the server, since it would crash the server to send data to it without it knowing what to do.
                {
                    m_handshakeDone = true;
                    PluginEventHandler.Static.RaiseStaticEvent(SendAddRingToPlanet, name, ring, null);
                });
            }
            else
            {
                PluginEventHandler.Static.RaiseStaticEvent(SendAddRingToPlanet, name, ring, null);
            }
        }

        [Event(100)]
        [Reliable]
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
                case SystemObjectType.PLANET:
                    PluginEventHandler.Static.RaiseStaticEvent(SendGetPlanetClient, success, (MyPlanetItem)item, callback, client);
                    break;
                case SystemObjectType.BELT:
                    PluginEventHandler.Static.RaiseStaticEvent(SendGetBeltClient, success, (MySystemBeltItem)item, callback, client);
                    break;
                case SystemObjectType.RING:
                    PluginEventHandler.Static.RaiseStaticEvent(SendGetRingClient, success, (MyPlanetRingItem)item, callback, client);
                    break;
                case SystemObjectType.MOON:
                    PluginEventHandler.Static.RaiseStaticEvent(SendGetMoonClient, success, (MyPlanetMoonItem)item, callback, client);
                    break;
                default:
                    break;
            }
        }

        [Event(101)]
        [Reliable]
        [Client]
        static void SendGetPlanetClient(bool success, MyPlanetItem item, ulong callback)
        {
            Static.m_getCallbacks[callback](success, item);
            Static.m_getCallbacks.Remove(callback);
        }

        [Event(102)]
        [Reliable]
        [Client]
        static void SendGetMoonClient(bool success, MyPlanetMoonItem item, ulong callback)
        {
            Static.m_getCallbacks[callback](success, item);
            Static.m_getCallbacks.Remove(callback);
        }

        [Event(103)]
        [Reliable]
        [Client]
        static void SendGetBeltClient(bool success, MySystemBeltItem item, ulong callback)
        {
            Static.m_getCallbacks[callback](success, item);
            Static.m_getCallbacks.Remove(callback);
        }

        [Event(104)]
        [Reliable]
        [Client]
        static void SendGetRingClient(bool success, MyPlanetRingItem item, ulong callback)
        {
            Static.m_getCallbacks[callback](success, item);
            Static.m_getCallbacks.Remove(callback);
        }

        [Event(105)]
        [Reliable]
        [Server]
        static void SendAddRingToPlanet(string planetName, MyPlanetRingItem ringBase)
        {
            if(Static.TryGetObject(planetName, out MySystemItem obj))
            {
                if (obj.Type == SystemObjectType.PLANET)
                {
                    MyPlanetItem planet = (MyPlanetItem)obj;
                    if(planet.PlanetRing == null)
                    {
                        ringBase.Center = planet.CenterPosition;
                        planet.PlanetRing = ringBase;
                    }
                }
            }
        }

        [Event(106)]
        [Reliable]
        [Server]
        static void SendAddPlanet(MyPlanetItem planet)
        {
            if(planet != null)
            {
                if (Static.TryGetObject(planet.DisplayName, out MySystemItem obj))
                {
                    if(obj.Type == SystemObjectType.PLANET)
                    {
                        if (((MyPlanetItem)obj).Generated) return;
                    }
                    else
                    {
                        return;
                    }
                    Static.m_objects.Remove(obj);
                }
                MyLog.Default.WriteLine("Adding Planet");
                Static.m_objects.Add(planet);
            }
        }
    }
}
