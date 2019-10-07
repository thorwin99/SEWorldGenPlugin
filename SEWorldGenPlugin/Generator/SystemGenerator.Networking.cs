using Sandbox.Engine.Multiplayer;
using Sandbox.Game.Multiplayer;
using SEWorldGenPlugin.ObjectBuilders;
using SEWorldGenPlugin.Session;
using SEWorldGenPlugin.Utilities;
using System;
using System.Collections.Generic;
using VRage;
using VRage.Network;
using VRage.Serialization;
using VRage.Utils;
using VRageMath;

namespace SEWorldGenPlugin.Generator
{
    public partial class SystemGenerator
    {
        private const ushort RING_HANDLER_ID = 2839;
        private const ushort PLANET_HANDLER_ID = 2876;
        private const ushort GET_HANDLER_ID = 2896;

        private Dictionary<ulong, Action<bool, MySystemItem>> m_getCallacks;
        private bool m_handshakeDone;
        private ulong m_currentIndex;

        private void InitNet()
        {
            m_getCallacks = new Dictionary<ulong, Action<bool, MySystemItem>>();

            MyMultiplayer.ReplicationLayer.RegisterFromAssembly(typeof(SystemGenerator).Assembly);

            NetUtil.RegisterMessageHandler(RING_HANDLER_ID, AddRingHandler);
            NetUtil.RegisterMessageHandler(PLANET_HANDLER_ID, AddPlanetPing);
            NetUtil.RegisterMessageHandler(GET_HANDLER_ID, GetObjectPing);
        }

        private void LoadNet()
        {
            m_handshakeDone = false;
            m_currentIndex = 0;
        }

        private void UnloadNet()
        {
            NetUtil.UnregisterMessageHandlers(RING_HANDLER_ID);
            NetUtil.UnregisterMessageHandlers(PLANET_HANDLER_ID);
            NetUtil.UnregisterMessageHandlers(GET_HANDLER_ID);
            m_handshakeDone = false;
            m_currentIndex = 0;
            m_getCallacks = null;
        }

        public void GetObject(string name, Action<bool, MySystemItem> callback)
        {
            if (!m_handshakeDone)
            {
                Action<ulong, string> handshake = delegate (ulong id, string msg)
                {
                    if (msg.Equals("PONG"))
                    {
                        m_handshakeDone = true;
                        m_getCallacks.Add(++m_currentIndex, callback);
                        MyMultiplayer.RaiseStaticEvent((IMyEventOwner s) => SendGetServer, Sync.MyId, name, m_currentIndex);
                    }
                };
                NetUtil.RegisterMessageHandler(GET_HANDLER_ID, handshake);
                NetUtil.SendPacketToServer(GET_HANDLER_ID, "PING");
            }
            else
            {
                m_getCallacks.Add(++m_currentIndex, callback);
                MyMultiplayer.RaiseStaticEvent((IMyEventOwner s) => SendGetServer, Sync.MyId, name, m_currentIndex);
            }
        }

        public void AddPlanet(MyPlanetItem planet)
        {
            if (!m_handshakeDone)
            {
                Action<ulong, string> handshake = delegate (ulong id, string msg)//Handshake needed to check, if plugin is installed on the server, since it would crash the server to send data to it without it knowing what to do.
                {
                    if (msg.Equals("PONG"))
                    {
                        m_handshakeDone = true;
                        MyMultiplayer.RaiseStaticEvent((IMyEventOwner s) => SendAddPlanet, planet);
                    }
                };
                NetUtil.RegisterMessageHandler(PLANET_HANDLER_ID, handshake);
                NetUtil.SendPacketToServer(PLANET_HANDLER_ID, "PING");
            }
            else
            {
                MyMultiplayer.RaiseStaticEvent((IMyEventOwner s) => SendAddPlanet, planet);
            }
        }

        public void AddRingToPlanet(string name, MyPlanetRingItem ring)
        {
            if (!m_handshakeDone)
            {
                Action<ulong, string> handshake = delegate (ulong id, string msg)//Handshake needed to check, if plugin is installed on the server, since it would crash the server to send data to it without it knowing what to do.
                {
                    if (msg.Equals("PONG"))
                    {
                        m_handshakeDone = true;
                        MyMultiplayer.RaiseStaticEvent((IMyEventOwner s) => SendAddRingToPlanet, name, ring);
                    }
                };
                NetUtil.RegisterMessageHandler(RING_HANDLER_ID, handshake);
                NetUtil.SendPacketToServer(RING_HANDLER_ID, "PING");
            }
            else
            {
                MyMultiplayer.RaiseStaticEvent((IMyEventOwner s) => SendAddRingToPlanet, name, ring);
            }
        }

        [Event]
        [Reliable]
        [Server]
        static void SendGetServer(ulong client, string name, ulong callback)
        {
            bool success = Static.TryGetObject(name, out MySystemItem item);

            switch (item.Type)
            {
                case SystemObjectType.PLANET:
                    MyMultiplayer.RaiseStaticEvent((IMyEventOwner s) => SendGetPlanetClient, success, (MyPlanetItem)item, callback, targetEndpoint: new EndpointId(client));
                    break;
                case SystemObjectType.BELT:
                    MyMultiplayer.RaiseStaticEvent((IMyEventOwner s) => SendGetBeltClient, success, (MySystemBeltItem)item, callback, targetEndpoint: new EndpointId(client));
                    break;
                case SystemObjectType.RING:
                    MyMultiplayer.RaiseStaticEvent((IMyEventOwner s) => SendGetRingClient, success, (MyPlanetRingItem)item, callback, targetEndpoint: new EndpointId(client));
                    break;
                case SystemObjectType.MOON:
                    MyMultiplayer.RaiseStaticEvent((IMyEventOwner s) => SendGetMoonClient, success, (MyPlanetMoonItem)item, callback, targetEndpoint: new EndpointId(client));
                    break;
                default:
                    break;
            }
        }

        [Event]
        [Reliable]
        [Client]
        static void SendGetPlanetClient(bool success, MyPlanetItem item, ulong callback)
        {
            Static.m_getCallacks[callback](success, item);
            Static.m_getCallacks.Remove(callback);
        }

        [Event]
        [Reliable]
        [Client]
        static void SendGetMoonClient(bool success, MyPlanetMoonItem item, ulong callback)
        {
            Static.m_getCallacks[callback](success, item);
            Static.m_getCallacks.Remove(callback);
        }

        [Event]
        [Reliable]
        [Client]
        static void SendGetBeltClient(bool success, MySystemBeltItem item, ulong callback)
        {
            Static.m_getCallacks[callback](success, item);
            Static.m_getCallacks.Remove(callback);
        }

        [Event]
        [Reliable]
        [Client]
        static void SendGetRingClient(bool success, MyPlanetRingItem item, ulong callback)
        {
            Static.m_getCallacks[callback](success, item);
            Static.m_getCallacks.Remove(callback);
        }

        [Event]
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

        [Event]
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
                Static.m_objects.Add(planet);
            }
        }


        //Refactor into single method, that parses the handler id from message

        private void GetObjectPing(ulong sender, string message)
        {
            if (message.Equals("PING"))
            {
                NetUtil.SendPacket(GET_HANDLER_ID, "PONG", sender);
            }
        }

        private void AddPlanetPing(ulong sender, string message)
        {
            if (message.Equals("PING"))
            {
                NetUtil.SendPacket(PLANET_HANDLER_ID, "PONG", sender);
            }
        }

        private void AddRingHandler(ulong sender, string message)
        {
            if (message.Equals("PING"))
            {
                NetUtil.SendPacket(RING_HANDLER_ID, "PONG", sender);
            }
        }
    }
}
