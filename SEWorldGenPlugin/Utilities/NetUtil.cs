using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using VRage.Utils;

namespace SEWorldGenPlugin.Utilities
{
    public class NetUtil
    {
        private static Dictionary<ushort, Action> unregActions;

        static NetUtil()
        {
            unregActions = new Dictionary<ushort, Action>();
        }

        public static void RegisterMessageHandler(ushort id, Action<ulong, string> handler)
        {
            Action<byte[]> newHandler = delegate (byte[] data)
            {
                ulong sender = NetDataToMsg(data, out string msg);
                handler(sender, msg);
            };
            MyAPIGateway.Multiplayer.RegisterMessageHandler(id, newHandler);
            if (unregActions.TryGetValue(id, out Action value))
            {
                unregActions[id] += delegate
                {
                    MyAPIGateway.Multiplayer.UnregisterMessageHandler(id, newHandler);
                };
            }
            else
            {
                unregActions[id] = delegate
                {
                    MyAPIGateway.Multiplayer.UnregisterMessageHandler(id, newHandler);
                };
            }
        }

        public static void UnregisterMessageHandlers(ushort id)
        {
            if(unregActions.TryGetValue(id, out Action value))
            {
                value();
                unregActions.Remove(id);
            }
        }

        public static void SendPacketToServer(ushort handlerId, string message)
        {
            ulong id = MyAPIGateway.Multiplayer.MyId;
            byte[] data = MsgToNetData(id, message);
            MyAPIGateway.Multiplayer.SendMessageToServer(handlerId, data, true);
        }

        public static void SendPacketToClients(ushort handlerId, string message)
        {
            ulong id = MyAPIGateway.Multiplayer.MyId;
            byte[] data = MsgToNetData(id, message);
            MyAPIGateway.Multiplayer.SendMessageToOthers(handlerId, data, true);
        }

        public static void SendPacket(ushort handlerId, string message, ulong receiver)
        {
            ulong id = MyAPIGateway.Multiplayer.MyId;
            byte[] data = MsgToNetData(id, message);
            MyAPIGateway.Multiplayer.SendMessageTo(handlerId, data, receiver, true);
        }

        public static byte[] MsgToNetData(ulong senderId, string msg)
        {
            MyLog.Default.WriteLine(senderId + "SENDER ID");
            char[] chars = msg.ToCharArray();
            byte[] bytes = new byte[8 + chars.Length * 2];
            for(int i = 0; i < bytes.Length; i++)
            {
                if(i < 8)
                {
                    bytes[i] = (byte)((senderId & (ulong)0xFF00000000000000 >> i * 8) >> 56 - i * 8);
                }
                else
                {
                    bytes[i] = (byte)((chars[(i - 8) / 2] & 0xFF00) >> 8);
                    bytes[i + 1] = (byte)((chars[(i - 8) / 2] & 0x00FF) >> 0);
                    i++;
                }
            }
            return bytes;
        }

        public static ulong NetDataToMsg(byte[] data, out string msg)
        {
            ulong id = 0;
            msg = "";
            for(int i = 0; i < data.Length; i++)
            {
                if(i < 8)
                {
                    id = id | (ulong)((long)data[i] << ((7 - i) * 8));
                }
                else
                {
                    msg += (char)((data[i] << 8) | data[i + 1]);
                    i++;
                }
            }
            return id;
        }
    }
}
