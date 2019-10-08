using ProtoBuf;
using SEWorldGenPlugin.Generator;
using SEWorldGenPlugin.Networking.Attributes;
using SEWorldGenPlugin.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using VRage.Game.Components;

namespace SEWorldGenPlugin.Networking
{
    [MySessionComponentDescriptor(MyUpdateOrder.NoUpdate, priority: 1010)]
    public class PluginEventHandler : MySessionComponentBase
    {
        private const ushort HANDLER_ID = 2778;
        private static ProtoNull e = new ProtoNull();

        private MethodInfo m_unpackData = typeof(PluginEventHandler).GetMethod("UnpackData", BindingFlags.Instance | BindingFlags.NonPublic);

        public static PluginEventHandler Static;

        private Dictionary<ulong, MethodInfo> m_registeredMethods;

        public override void LoadData()
        {
            m_registeredMethods = new Dictionary<ulong, MethodInfo>();
            Static = this;
            NetUtil.RegisterMessageHandler(HANDLER_ID, MessageHandler);

            Register(typeof(SystemGenerator));
        }

        public void RaiseStaticEvent(Action action, ulong? receiver = null)
        {
            if (action.Method.CustomAttributes.Any(data => data.AttributeType == typeof(EventAttribute)))
            {
                ulong id = action.Method.GetCustomAttribute<EventAttribute>().Id;
                byte[] data = PackData(id, e, e, e);
                SendData(action.Method, data, receiver);
            }
        }

        public void RaiseStaticEvent<T>(Action<T> action, T arg1, ulong? receiver = null)
        {
            if (action.Method.CustomAttributes.Any(data => data.AttributeType == typeof(EventAttribute)))
            {
                ulong id = action.Method.GetCustomAttribute<EventAttribute>().Id;
                byte[] data = PackData(id, arg1, e, e);
                SendData(action.Method, data, receiver);
            }
        }

        public void RaiseStaticEvent<T1, T2>(Action<T1, T2> action, T1 arg1, T2 arg2, ulong? receiver = null)
        {
            if (action.Method.CustomAttributes.Any(data => data.AttributeType == typeof(EventAttribute)))
            {
                ulong id = action.Method.GetCustomAttribute<EventAttribute>().Id;
                byte[] data = PackData(id, arg1, arg2, e);
                SendData(action.Method, data, receiver);
            }
        }

        public void RaiseStaticEvent<T1, T2, T3>(Action<T1, T2, T3> action, T1 arg1, T2 arg2, T3 arg3, ulong? receiver = null)
        {
            if (action.Method.CustomAttributes.Any(data => data.AttributeType == typeof(EventAttribute)))
            {
                ulong id = action.Method.GetCustomAttribute<EventAttribute>().Id;
                byte[] data = PackData(id, arg1, arg2, arg3);
                SendData(action.Method, data, receiver);
            }
        }

        public void Register(Type type)
        {
            if (type.CustomAttributes.Where(data => data.AttributeType.Equals(typeof(EventOwnerAttribute))).Count() > 0)
            {
                foreach (var m in type.GetRuntimeMethods())
                {
                    if (m.CustomAttributes.Any(data => data.AttributeType == typeof(EventAttribute)))
                    {
                        m_registeredMethods.Add(m.GetCustomAttribute<EventAttribute>().Id, m);
                    }
                }
            }
        }

        protected override void UnloadData()
        {
            m_registeredMethods.Clear();
            m_registeredMethods = null;
            Static = null;
            NetUtil.UnregisterMessageHandlers(HANDLER_ID);
        }

        private void SendData(MethodInfo method, byte[] data, ulong? receiver = null)
        {
            foreach(var attr in method.CustomAttributes)
            {
                if(attr.AttributeType == typeof(ServerAttribute))
                {
                    NetUtil.SendPacketToServer(HANDLER_ID, data);
                }
                else if(attr.AttributeType == typeof(ClientAttribute))
                {
                    if (receiver.HasValue)
                    {
                        NetUtil.SendPacket(HANDLER_ID, data, receiver.Value);
                    }
                    else
                    {
                        NetUtil.SendPacketToClients(HANDLER_ID, data);
                    }
                }
                else if(attr.AttributeType == typeof(BroadcastAttribute))
                {
                    NetUtil.SendPacketToClients(HANDLER_ID, data);
                }
            }
        }

        private void MessageHandler(ulong sender, byte[] data)
        {
            ulong id = GetId(data);
            if(m_registeredMethods.TryGetValue(id, out MethodInfo info))
            {
                var parameters = info.GetParameters();
                if (parameters.Length > 3) return;
                Type[] args = new Type[3] { typeof(ProtoNull), typeof(ProtoNull), typeof(ProtoNull) };
                for(int i = 0; i < parameters.Length; i++)
                {
                    args[i] = parameters[i].ParameterType;
                }
                object[] paras = new object[5] { data, null, null, null, null };

                m_unpackData.MakeGenericMethod(args).Invoke(this, paras);
                List<object> objs = new List<object>();
                for(int i = 0; i < args.Length; i++)
                {
                    if (args[i] == typeof(ProtoNull)) continue;
                    objs.Add(paras[i + 2]);
                }

                info.Invoke(null, objs.ToArray());
            }
        }

        private byte[] PackData<T1, T2, T3>(ulong id, T1 arg1, T2 arg2, T3 arg3)
        {
            using (var ms = new MemoryStream())
            {
                Serializer.SerializeWithLengthPrefix(ms, id, PrefixStyle.Base128);
                Serializer.SerializeWithLengthPrefix(ms, arg1, PrefixStyle.Base128);
                Serializer.SerializeWithLengthPrefix(ms, arg2, PrefixStyle.Base128);
                Serializer.SerializeWithLengthPrefix(ms, arg3, PrefixStyle.Base128);
                return ms.ToArray();
            }
        }

        private ulong GetId(byte[] data)
        {
            using (var ms = new MemoryStream(data))
            {
                return Serializer.DeserializeWithLengthPrefix<ulong>(ms, PrefixStyle.Base128);
            }
        }

        private void UnpackData<T1, T2, T3>(byte[] data, out ulong id, out T1 arg1, out T2 arg2, out T3 arg3)
        {
            using(var ms = new MemoryStream(data))
            {
                id = Serializer.DeserializeWithLengthPrefix<ulong>(ms, PrefixStyle.Base128);
                arg1 = Serializer.DeserializeWithLengthPrefix<T1>(ms, PrefixStyle.Base128);
                arg2 = Serializer.DeserializeWithLengthPrefix<T2>(ms, PrefixStyle.Base128);
                arg3 = Serializer.DeserializeWithLengthPrefix<T3>(ms, PrefixStyle.Base128);

                return;
            }
        }

        [ProtoContract]
        public class ProtoNull
        {
        }
    }
}
