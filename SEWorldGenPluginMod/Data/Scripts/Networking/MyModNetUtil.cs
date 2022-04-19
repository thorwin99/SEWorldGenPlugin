using Sandbox.ModAPI;
using System;
using System.Collections.Generic;

namespace SEWorldGenPlugin.Utilities
{
    /// <summary>
    /// Utility class for network operation and plugin pingin. Can send and receive messages over network
    /// and ping a server to check if the plugin is on there.
    /// </summary>
    public class MyModNetUtil
    {
        /// <summary>
        /// Dictionary containing all actions to unregister a registered message handler.
        /// Needed, because this class encapsulates message handlers with its own retreive the sender id before calling the original message handler. 
        /// Thats why it stores actions to unregister those new message handlers.
        /// </summary>
        private static Dictionary<ushort, Action> unregActions;

        /// <summary>
        /// id of the current ping, essentially a ping counter
        /// </summary>
        private static int pingId;

        /// <summary>
        /// Creates a new net util instance
        /// </summary>
        static MyModNetUtil()
        {
            unregActions = new Dictionary<ushort, Action>();
            pingId = 0;
        }

        /// <summary>
        /// Registers a new message handler  with the given id.
        /// </summary>
        /// <param name="id">The handler ID</param>
        /// <param name="handler">The handler with 2 parameters, a ulong for the sender id and a string for the network message</param>
        /// Will encapsulate the handler into a new handler, which splits the byte[] message into
        /// a string message and a sender id, then calls the registered handler.
        public static void RegisterMessageHandler(ushort id, Action<ulong, string> handler)
        {
            Action<ushort, byte[], ulong, bool> newHandler = delegate (ushort s, byte[] data, ulong i, bool b)
            {
                ulong sender = NetDataToMsg(data, out string msg);
                handler(sender, msg);
            };
            MyAPIGateway.Multiplayer.RegisterSecureMessageHandler(id, newHandler);
            if (unregActions.TryGetValue(id, out Action value))
            {
                unregActions[id] += delegate
                {
                    MyAPIGateway.Multiplayer.UnregisterSecureMessageHandler(id, newHandler);
                };
            }
            else
            {
                unregActions[id] = delegate
                {
                    MyAPIGateway.Multiplayer.UnregisterSecureMessageHandler(id, newHandler);
                };
            }
        }

        /// <summary>
        /// Registers a new message handler with the given id.
        /// </summary>
        /// <param name="id">The handler ID</param>
        /// <param name="handler">The handler with 2 parameters, a ulong for the sender id and a byte[] for the network data</param>
        /// /// Will encapsulate the handler into a new handler, which splits the byte[] message into
        /// a byte[] data message and a sender id, then calls the registered handler.
        public static void RegisterMessageHandler(ushort id, Action<ulong, byte[]> handler)
        {
            Action<ushort, byte[], ulong, bool> newHandler = delegate (ushort s, byte[] data, ulong i, bool b)
            {
                ulong sender = ReadSenderId(data, out byte[] rawData);
                handler(sender, rawData);
            };
            MyAPIGateway.Multiplayer.RegisterSecureMessageHandler(id, newHandler);
            if (unregActions.TryGetValue(id, out Action value))
            {
                unregActions[id] += delegate
                {
                    MyAPIGateway.Multiplayer.UnregisterSecureMessageHandler(id, newHandler);
                };
            }
            else
            {
                unregActions[id] = delegate
                {
                    MyAPIGateway.Multiplayer.UnregisterSecureMessageHandler(id, newHandler);
                };
            }
        }

        /// <summary>
        /// Unregisteres a message handler with the given id, if it is registered.
        /// </summary>
        /// <param name="id">Id of the message handler</param>
        public static void UnregisterMessageHandlers(ushort id)
        {
            if(unregActions.TryGetValue(id, out Action value))
            {
                value();
                unregActions.Remove(id);
            }
        }

        /// <summary>
        /// Sends a string message to the server, which should get handled by the
        /// message handler with handlerId.
        /// </summary>
        /// <param name="handlerId">Id of the message handler that should handle this message</param>
        /// <param name="message">Message to send</param>
        public static void SendPacketToServer(ushort handlerId, string message)
        {
            ulong id = MyAPIGateway.Multiplayer.MyId;
            byte[] data = MsgToNetData(id, message);
            MyAPIGateway.Multiplayer.SendMessageToServer(handlerId, data, true);
        }

        /// <summary>
        /// Sends a string message to all clients, which should get handled by the
        /// message handler with handlerId.
        /// </summary>
        /// <param name="handlerId">Id of the message handler that should handle this message</param>
        /// <param name="message">Message to send</param>
        public static void SendPacketToClients(ushort handlerId, string message)
        {
            ulong id = MyAPIGateway.Multiplayer.MyId;
            byte[] data = MsgToNetData(id, message);
            MyAPIGateway.Multiplayer.SendMessageToOthers(handlerId, data, true);
        }

        /// <summary>
        /// Sends a string message to the client with the provided id. The message will be handled by
        /// the message handler that corresponds to the provided handler id.
        /// </summary>
        /// <param name="handlerId">Id of the message handler, which will handle the incoming message</param>
        /// <param name="message">The message</param>
        /// <param name="receiver">The id of the client, that receives this packet</param>
        public static void SendPacket(ushort handlerId, string message, ulong receiver)
        {
            ulong id = MyAPIGateway.Multiplayer.MyId;
            byte[] data = MsgToNetData(id, message);
            MyAPIGateway.Multiplayer.SendMessageTo(handlerId, data, receiver, true);
        }

        /// <summary>
        /// Sends a byte[] of data to the server, which should get handled by the
        /// message handler with handlerId.
        /// </summary>
        /// <param name="handlerId">Id of the message handler that should handle this message</param>
        /// <param name="data">Data to send</param>
        public static void SendPacketToServer(ushort handlerId, byte[] data)
        {
            ulong id = MyAPIGateway.Multiplayer.MyId;
            MyAPIGateway.Multiplayer.SendMessageToServer(handlerId, PrefixDataWithId(id, data), true);
        }

        /// <summary>
        /// Sends a byte[] of data to all clients, which should get handled by the
        /// message handler with handlerId.
        /// </summary>
        /// <param name="handlerId">Id of the message handler that should handle this message</param>
        /// <param name="data">Data to send</param>
        public static void SendPacketToClients(ushort handlerId, byte[] data)
        {
            ulong id = MyAPIGateway.Multiplayer.MyId;
            MyAPIGateway.Multiplayer.SendMessageToOthers(handlerId, PrefixDataWithId(id, data), true);
        }

        /// <summary>
        /// Sends a byte[] of data to the client corresponding to the provided receiver id. It should get handled by the
        /// message handler with handlerId.
        /// </summary>
        /// <param name="handlerId">Id of the message handler that should handle this message</param>
        /// <param name="data">Data to send</param>
        /// <param name="receiver">Id of the client that receives this message</param>
        public static void SendPacket(ushort handlerId, byte[] data, ulong receiver)
        {
            ulong id = MyAPIGateway.Multiplayer.MyId;
            MyAPIGateway.Multiplayer.SendMessageTo(handlerId, PrefixDataWithId(id, data), receiver, true);
        }

        /// <summary>
        /// Converts a string message to network data. For that, it will prefix the message with the sender id,
        /// and then append the string message as bytes.
        /// </summary>
        /// <param name="senderId">Sender Id</param>
        /// <param name="msg">String message</param>
        /// <returns>Byte[] ready to be send over net.</returns>
        public static byte[] MsgToNetData(ulong senderId, string msg)
        {
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

        /// <summary>
        /// Converts a network data to a string message and a sender id.
        /// </summary>
        /// <param name="data">The incoming data</param>
        /// <param name="msg">The output string message contained in data</param>
        /// <returns>The sender id of the sender</returns>
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

        /// <summary>
        /// Reads the sender id from the data and outputs the raw data without the sender id.
        /// Sender id is returned
        /// </summary>
        /// <param name="data">Incoming network data</param>
        /// <param name="rawData">Output raw data without sender id</param>
        /// <returns>Sender id of the sender</returns>
        private static ulong ReadSenderId(byte[] data, out byte[] rawData)
        {
            rawData = new byte[data.Length - 8];
            ulong id = 0;
            for (int i = 0; i < data.Length; i++)
            {
                if(i < 8)
                {
                    id = id | (ulong)((long)data[i] << ((7 - i) * 8));
                }
                else
                {
                    rawData[i - 8] = data[i];
                }
            }

            return id;
        }

        /// <summary>
        /// Prefixes raw network data with the sender id.
        /// </summary>
        /// <param name="id">Sender id</param>
        /// <param name="data">Raw data</param>
        /// <returns>Network data with sender id prefix</returns>
        private static byte[] PrefixDataWithId(ulong id, byte[] data)
        {
            byte[] newData = new byte[data.Length + 8];
            for(int i = 0; i < newData.Length; i++)
            {
                if(i < 8)
                {
                    newData[i] = (byte)((id & (ulong)0xFF00000000000000 >> i * 8) >> 56 - i * 8);
                }
                else
                {
                    newData[i] = data[i - 8];
                }
            }
            return newData;
        }
    }
}
