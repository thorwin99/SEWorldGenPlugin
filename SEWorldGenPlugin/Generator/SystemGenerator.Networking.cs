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

        public struct PlanetInfo
        {
            [Serialize(MyObjectFlags.Nullable)]
            public SystemObjectType Type;
            public string DisplayName;
            public Vector3D CenterPos;
            public Vector3D OffsetPos;
            public float Size;
            public bool Generated;
            public string DefName;
            public RingInfo Ring;
        }

        public struct RingInfo
        {
            [Serialize(MyObjectFlags.Nullable)]
            public SystemObjectType Type;
            public string DisplayName;
            public long Radius;
            public int Width;
            public int Height;
            public float AngleDegrees;
            public int RoidSize;
            public Vector3D Center;
        }

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
            MyLog.Default.WriteLine("Getting Object, requesting server");
            if (!m_handshakeDone)
            {
                Action<ulong, string> handshake = delegate (ulong id, string msg)
                {
                    if (msg.Equals("PONG"))
                    {
                        MyLog.Default.WriteLine("Got Pong");
                        m_handshakeDone = true;
                        m_getCallacks.Add(m_currentIndex++, callback);
                        MyMultiplayer.RaiseStaticEvent((IMyEventOwner s) => SendGetServer, Sync.MyId, name, m_currentIndex);
                    }
                };
                NetUtil.RegisterMessageHandler(GET_HANDLER_ID, handshake);
                NetUtil.SendPacketToServer(GET_HANDLER_ID, "PING");
                MyLog.Default.WriteLine("Sending Ping");
            }
            else
            {
                m_getCallacks.Add(m_currentIndex++, callback);
                MyMultiplayer.RaiseStaticEvent((IMyEventOwner s) => SendGetServer, Sync.MyId, name, m_currentIndex);
            }
        }

        public void AddPlanet(MyPlanetItem planet)
        {
            if (!m_handshakeDone)
            {
                Action<ulong, string> handshake = delegate (ulong id, string msg)
                {
                    if (msg.Equals("PING"))
                    {
                        NetUtil.SendPacket(PLANET_HANDLER_ID, "PONG", id);
                    }
                    else if (msg.Equals("PONG"))
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

        public void AddRingToPlanet(string name)
        {
            if (Static == null) return;

            NetUtil.SendPacketToServer(RING_HANDLER_ID, "GENRINGSERV" + name);
            MyLog.Default.WriteLine("Send data to server");
        }

        [Event]
        [Reliable]
        [Server]
        static void SendGetServer(ulong client, string name, ulong callback)
        {
            MyLog.Default.WriteLine("Sending answer to client to get object");
            bool success = Static.TryGetObject(name, out MySystemItem item);
            MyMultiplayer.RaiseStaticEvent((IMyEventOwner s) => SendGetClient, success, /*Static.PlanetItemToInfo((MyPlanetItem)item),*/ callback, targetEndpoint: new EndpointId(client));
        }

        [Event]
        [Reliable]
        [Client]
        static void SendGetClient(bool success,/* PlanetInfo info, */ulong callback)
        {
            //MyPlanetItem item = Static.PlanetInfoToItem(info);
            MyLog.Default.WriteLine("Got answer from server, object got: " + success + "  ");
            //Static.m_getCallacks[callback](success, (MySystemItem)item);
            //Static.m_getCallacks.Remove(callback);
        }

        [Event]
        [Reliable]
        [Server]
        static void SendAddRingToPlanet(string name)
        {
            if (!SettingsSession.Static.Settings.Enable) return;
        }

        [Event]
        [Reliable]
        [Server]
        static void SendAddPlanet(MyPlanetItem planet)
        {
            if (!SettingsSession.Static.Settings.Enable) return;
        }

        private void GetObjectPing(ulong sender, string message)
        {
            if (message.Equals("PING"))
            {
                MyLog.Default.WriteLine("Got Ping");
                NetUtil.SendPacket(GET_HANDLER_ID, "PONG", sender);
                MyLog.Default.WriteLine("Sending Pong");
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
            MyLog.Default.WriteLine("Got message from " + sender + " with message " + message);
            if (message.Contains("GENRINGSERV"))
            {
                NetUtil.SendPacket(RING_HANDLER_ID, "GENRINGCLIENT" + message.Substring(("GENRINGSERV").Length), sender);
            }
            else if (message.Contains("GENRINGCLIENT"))
            {
                string name = message.Substring("GENRINGCLIENT".Length);
                m_handshakeDone = true;
                MyMultiplayer.RaiseStaticEvent((IMyEventOwner s) => SendAddRingToPlanet, name);
            }
        }

        private PlanetInfo PlanetItemToInfo(MyPlanetItem item)
        {
            PlanetInfo info = new PlanetInfo
            {
                CenterPos = item == null ? Vector3D.Zero : new Vector3D(item.CenterPosition.X, item.CenterPosition.Y, item.CenterPosition.Z),
                DefName = item == null ? "" : item.DefName,
                DisplayName = item == null ? "" : item.DisplayName,
                Generated = item == null ? false : item.Generated,
                OffsetPos = item == null ? Vector3D.Zero : new Vector3D(item.OffsetPosition.X, item.OffsetPosition.Y, item.OffsetPosition.Z),
                Ring = RingItemToInfo(item == null? null : item.PlanetRing),
                Size = item == null ? -1 : item.Size,
                Type = item == null ? SystemObjectType.PLANET : item.Type
            };
            return info;
        }

        private RingInfo RingItemToInfo(MyPlanetRingItem item)
        {
            RingInfo info = new RingInfo
            {
                Center = (item == null ? Vector3D.Zero : new Vector3D(item.Center.X, item.Center.Y, item.Center.Z)),
                AngleDegrees = item == null ? 0 : item.AngleDegrees,
                DisplayName = item == null ? "" : item.DisplayName,
                Height = item == null ? -1 : item.Height,
                Radius = item == null ? -1 : item.Radius,
                RoidSize = item == null ? -1 : item.RoidSize,
                Type = item == null ? SystemObjectType.RING : item.Type,
                Width = item == null ? -1 : item.Width
            };
            return info;
        }

        private MyPlanetItem PlanetInfoToItem(PlanetInfo info)
        {
            if (info.Size == -1) return null;
            MyPlanetItem item = new MyPlanetItem();
            item.Type = info.Type;
            item.DisplayName = info.DisplayName;
            item.DefName = info.DefName;
            item.Generated = info.Generated;
            item.CenterPosition = info.CenterPos;
            item.OffsetPosition = info.OffsetPos;
            item.PlanetMoons = new MyPlanetMoonItem[0];
            item.Size = info.Size;
            item.PlanetRing = RingInfoToItem(info.Ring);
            return item;
        }

        private MyPlanetRingItem RingInfoToItem(RingInfo info)
        {
            if (info.Height == -1) return null;
            MyPlanetRingItem item = new MyPlanetRingItem();
            item.Type = info.Type;
            item.DisplayName = info.DisplayName;
            item.AngleDegrees = info.AngleDegrees;
            item.Center = info.Center;
            item.Height = info.Height;
            item.Width = info.Width;
            item.Radius = info.Radius;
            item.RoidSize = info.RoidSize;
            return item;
        }
    }
}
