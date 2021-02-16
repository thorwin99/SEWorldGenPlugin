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
    /// <summary>
    /// The plugins networking handler. Can execute events on clients and the server and send the
    /// parameters aswell as receive them. Is a singleton class, since there should always
    /// only exist one instance of this object.
    /// </summary>
    [MySessionComponentDescriptor(MyUpdateOrder.NoUpdate, priority: 1010)]
    public class PluginEventHandler : MySessionComponentBase
    {
        private const ushort HANDLER_ID = 2778;
        private static ProtoNull e = new ProtoNull();

        /// <summary>
        /// The method to unpack the send data. Needs to be a Method Info, to dynamically set
        /// the parameter types of UnpackData.
        /// </summary>
        private MethodInfo m_unpackData = typeof(PluginEventHandler).GetMethod("UnpackData", BindingFlags.Instance | BindingFlags.NonPublic);

        public static PluginEventHandler Static;

        private Dictionary<ulong, MethodInfo> m_registeredMethods;

        /// <summary>
        /// Initializes the network message handler, by registering it in SpaceEngineers and
        /// registering all networking methods using the networking attributes.
        /// </summary>
        public override void LoadData()
        {
            m_registeredMethods = new Dictionary<ulong, MethodInfo>();
            Static = this;
            MyNetUtil.RegisterMessageHandler(HANDLER_ID, MessageHandler);

            RegisterAll();
        }

        /// <summary>
        /// Raises a static event to be executed. It executes it on receiver, if it is not
        /// marked as broadcast or server.
        /// </summary>
        /// <param name="action">The method to execute over network on the receiver</param>
        /// <param name="receiver">Optional receiver, if the method is marked as client</param>
        public void RaiseStaticEvent(Action action, ulong? receiver = null)
        {
            if (action.Method.CustomAttributes.Any(data => data.AttributeType == typeof(EventAttribute)))
            {
                ulong id = action.Method.GetCustomAttribute<EventAttribute>().Id;
                byte[] data = PackData(id, e, e, e);
                SendData(action.Method, data, receiver);
            }
        }

        /// <summary>
        /// Raises a static event, using arg1 as a parameter, to be executed over network. It executes it on receiver, if it is not
        /// marked as broadcast or server.
        /// </summary>
        /// <typeparam name="T">The type of the first argument of the method that gets executed.</typeparam>
        /// <param name="action">The method to execute over network on the receiver</param>
        /// <param name="arg1">The first method parameter</param>
        /// <param name="receiver">Optional receiver, if the method is marked as client</param>
        public void RaiseStaticEvent<T>(Action<T> action, T arg1, ulong? receiver = null)
        {
            if (action.Method.CustomAttributes.Any(data => data.AttributeType == typeof(EventAttribute)))
            {
                ulong id = action.Method.GetCustomAttribute<EventAttribute>().Id;
                byte[] data = PackData(id, arg1, e, e);
                SendData(action.Method, data, receiver);
            }
        }

        /// <summary>
        /// Raises a static event, using arg1 and arg2 as a parameters, to be executed over network. It executes it on receiver, if it is not
        /// marked as broadcast or server.
        /// </summary>
        /// <typeparam name="T1">The type of the first argument of the method that gets executed.</typeparam>
        /// <typeparam name="T2">The type of the second argument of the method that gets executed.</typeparam>
        /// <param name="action">The method to execute over network on the receiver</param>
        /// <param name="arg1">The first method parameter</param>
        /// <param name="arg2">The second method parameter</param>
        /// <param name="receiver">Optional receiver, if the method is marked as client</param>
        public void RaiseStaticEvent<T1, T2>(Action<T1, T2> action, T1 arg1, T2 arg2, ulong? receiver = null)
        {
            if (action.Method.CustomAttributes.Any(data => data.AttributeType == typeof(EventAttribute)))
            {
                ulong id = action.Method.GetCustomAttribute<EventAttribute>().Id;
                byte[] data = PackData(id, arg1, arg2, e);
                SendData(action.Method, data, receiver);
            }
        }

        /// <summary>
        /// Raises a static event, using arg1, arg2 and arg3 as a parameters, to be executed over network. It executes it on receiver, if it is not
        /// marked as broadcast or server.
        /// </summary>
        /// <typeparam name="T1">The type of the first argument of the method that gets executed.</typeparam>
        /// <typeparam name="T2">The type of the second argument of the method that gets executed.</typeparam>
        /// <typeparam name="T3">The type of the third argument of the method that gets executed.</typeparam>
        /// <param name="action">The method to execute over network on the receiver</param>
        /// <param name="arg1">The first method parameter</param>
        /// <param name="arg2">The second method parameter</param>
        /// <param name="arg3">The third method parameter</param>
        /// <param name="receiver">Optional receiver, if the method is marked as client</param>
        public void RaiseStaticEvent<T1, T2, T3>(Action<T1, T2, T3> action, T1 arg1, T2 arg2, T3 arg3, ulong? receiver = null)
        {
            if (action.Method.CustomAttributes.Any(data => data.AttributeType == typeof(EventAttribute)))
            {
                ulong id = action.Method.GetCustomAttribute<EventAttribute>().Id;
                byte[] data = PackData(id, arg1, arg2, arg3);
                SendData(action.Method, data, receiver);
            }
        }

        /// <summary>
        /// Raises a static event, using arg1, arg2 and arg3 as a parameters, to be executed over network. It executes it on receiver, if it is not
        /// marked as broadcast or server.
        /// </summary>
        /// <typeparam name="T1">The type of the first argument of the method that gets executed.</typeparam>
        /// <typeparam name="T2">The type of the second argument of the method that gets executed.</typeparam>
        /// <typeparam name="T3">The type of the third argument of the method that gets executed.</typeparam>
        /// <typeparam name="T4">The type of the fourth argument of the method that gets executed.</typeparam>
        /// <param name="action">The method to execute over network on the receiver</param>
        /// <param name="arg1">The first method parameter</param>
        /// <param name="arg2">The second method parameter</param>
        /// <param name="arg3">The third method parameter</param>
        /// <param name="arg4">The fourth method parameter</param>
        /// <param name="receiver">Optional receiver, if the method is marked as client</param>
        public void RaiseStaticEvent<T1, T2, T3, T4>(Action<T1, T2, T3, T4> action, T1 arg1, T2 arg2, T3 arg3, T4 arg4, ulong? receiver = null)
        {
            if (action.Method.CustomAttributes.Any(data => data.AttributeType == typeof(EventAttribute)))
            {
                ulong id = action.Method.GetCustomAttribute<EventAttribute>().Id;
                byte[] data = PackData(id, arg1, arg2, arg3, arg4);
                SendData(action.Method, data, receiver);
            }
        }

        /// <summary>
        /// Registers all types with the EventOwnerAttribute.
        /// </summary>
        public void RegisterAll()
        {
            foreach(var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                MyPluginLog.Debug("Registering: " + assembly.FullName);
                try
                {
                    foreach (var type in assembly.GetTypes())
                    {
                        if (type.GetCustomAttributes(typeof(EventOwnerAttribute), true).Length > 0)
                        {
                            MyPluginLog.Log("Registering type " + type.Name + " in PluginEventHandler.");
                            Register(type);
                        }
                    }
                }catch(ReflectionTypeLoadException)
                {
                    MyPluginLog.Log("Couldnt register Types for assembly " + assembly.FullName, LogLevel.ERROR);
                }                
            }
        }

        /// <summary>
        /// Registers all methods in of the given class type with the EventOwner attribute, that are marked as events,
        /// in the PluginHandler.
        /// </summary>
        /// <param name="type">The class type to register</param>
        public void Register(Type type)
        {
            if (type.CustomAttributes.Where(data => data.AttributeType.Equals(typeof(EventOwnerAttribute))).Count() > 0)
            {
                foreach (var m in type.GetRuntimeMethods())
                {
                    if (m.CustomAttributes.Any(data => data.AttributeType == typeof(EventAttribute)))
                    {
                        MyPluginLog.Log("Registering method " + m.Name + " in type " + type.Name);
                        ulong id = m.GetCustomAttribute<EventAttribute>().Id;
                        if (m_registeredMethods.ContainsKey(id))
                        {
                            MyPluginLog.Log("A networking event with id " + id + " already exists. Wont register method " + m.GetBaseDefinition().Name + " from type " + type.Name + ".", LogLevel.ERROR);
                        }
                        m_registeredMethods.Add(m.GetCustomAttribute<EventAttribute>().Id, m);
                    }
                }
            }
        }

        /// <summary>
        /// Unloads all data used by this session component
        /// </summary>
        protected override void UnloadData()
        {
            m_registeredMethods.Clear();
            m_registeredMethods = null;
            Static = null;
            MyNetUtil.UnregisterMessageHandlers(HANDLER_ID);
        }

        /// <summary>
        /// Sends the packed data to the receiver to be executed.
        /// </summary>
        /// <param name="method">The method to execute on the receiver corresponding to the same event id used in the packed data</param>
        /// <param name="data">The packed data of the method to execute</param>
        /// <param name="receiver">The optional receiver, if the method is a client method</param>
        private void SendData(MethodInfo method, byte[] data, ulong? receiver = null)
        {
            foreach(var attr in method.CustomAttributes)
            {
                if(attr.AttributeType == typeof(ServerAttribute))
                {
                    MyNetUtil.SendPacketToServer(HANDLER_ID, data);
                }
                else if(attr.AttributeType == typeof(ClientAttribute))
                {
                    if (receiver.HasValue)
                    {
                        MyNetUtil.SendPacket(HANDLER_ID, data, receiver.Value);
                    }
                    else
                    {
                        MyNetUtil.SendPacketToClients(HANDLER_ID, data);
                    }
                }
                else if(attr.AttributeType == typeof(BroadcastAttribute))
                {
                    MyNetUtil.SendPacketToClients(HANDLER_ID, data);
                }
            }
        }

        /// <summary>
        /// The message handler, that handles the received packages to this handler.
        /// It receives the packed data and the sender id, unpacks it and executes it.
        /// To unpack it, it creates a new generic method of UnpackData with the argument types
        /// of the method, that gets executed. This way it will return the correct types.
        /// </summary>
        /// <param name="sender">The data sender</param>
        /// <param name="data">The packed data for the method to execute</param>
        private void MessageHandler(ulong sender, byte[] data)
        {
            ulong id = GetId(data);
            if(m_registeredMethods.TryGetValue(id, out MethodInfo info))
            {
                var parameters = info.GetParameters();
                if (parameters.Length > 4) return;
                Type[] args = new Type[4] { typeof(ProtoNull), typeof(ProtoNull), typeof(ProtoNull), typeof(ProtoNull) };
                for(int i = 0; i < parameters.Length; i++)
                {
                    args[i] = parameters[i].ParameterType;
                    if(args[i] == null)
                    {
                        args[i] = typeof(ProtoNull);
                    }
                }
                object[] paras = new object[6] { data, null, null, null, null, null};

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

        /// <summary>
        /// Packs the data for an event method into a serialized byte[].
        /// The byte array starts with the methods event id and is followed by
        /// the arguments of the method.
        /// </summary>
        /// <typeparam name="T1">The type of the first method parameter</typeparam>
        /// <typeparam name="T2">The type of the second method parameter</typeparam>
        /// <typeparam name="T3">The type of the third method parameter</typeparam>
        /// <param name="id">The methods event id</param>
        /// <param name="arg1">The methods first parameter</param>
        /// <param name="arg2">The methods second parameter</param>
        /// <param name="arg3">The methods third parameter</param>
        /// <returns>The byte[] of the packed data for the method.</returns>
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

        /// <summary>
        /// Packs the data for an event method into a serialized byte[].
        /// The byte array starts with the methods event id and is followed by
        /// the arguments of the method.
        /// </summary>
        /// <typeparam name="T1">The type of the first method parameter</typeparam>
        /// <typeparam name="T2">The type of the second method parameter</typeparam>
        /// <typeparam name="T3">The type of the third method parameter</typeparam>
        /// <typeparam name="T4">The type of the fourth method parameter</typeparam>
        /// <param name="id">The methods event id</param>
        /// <param name="arg1">The methods first parameter</param>
        /// <param name="arg2">The methods second parameter</param>
        /// <param name="arg3">The methods third parameter</param>
        /// <param name="arg4">The methods fourth parameter</param>
        /// <returns>The byte[] of the packed data for the method.</returns>
        private byte[] PackData<T1, T2, T3, T4>(ulong id, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
            using (var ms = new MemoryStream())
            {
                Serializer.SerializeWithLengthPrefix(ms, id, PrefixStyle.Base128);
                Serializer.SerializeWithLengthPrefix(ms, arg1, PrefixStyle.Base128);
                Serializer.SerializeWithLengthPrefix(ms, arg2, PrefixStyle.Base128);
                Serializer.SerializeWithLengthPrefix(ms, arg3, PrefixStyle.Base128);
                Serializer.SerializeWithLengthPrefix(ms, arg4, PrefixStyle.Base128);
                return ms.ToArray();
            }
        }

        /// <summary>
        /// Retreives the methods event id from the packed data of a method
        /// </summary>
        /// <param name="data">The packed data for the method</param>
        /// <returns>The methods event id</returns>
        private ulong GetId(byte[] data)
        {
            using (var ms = new MemoryStream(data))
            {
                return Serializer.DeserializeWithLengthPrefix<ulong>(ms, PrefixStyle.Base128);
            }
        }

        /// <summary>
        /// It unpacks a packed data byte array for a method.
        /// </summary>
        /// <typeparam name="T1">The type of the first method parameter</typeparam>
        /// <typeparam name="T2">The type of the second method parameter</typeparam>
        /// <typeparam name="T3">The type of the third method parameter</typeparam>
        /// <typeparam name="T4">The type of the fourth method parameter</typeparam>
        /// <param name="data">The packed data of the method</param>
        /// <param name="id">Output value for the methods event id.</param>
        /// <param name="arg1">Output value for the first method parameter.</param>
        /// <param name="arg2">Output value for the second method parameter.</param>
        /// <param name="arg3">Output value for the third method parameter.</param>
        /// <param name="arg4">Output value for the fourth method parameter.</param>
        private void UnpackData<T1, T2, T3, T4>(byte[] data, out ulong id, out T1 arg1, out T2 arg2, out T3 arg3, out T4 arg4)
        {
            using (var ms = new MemoryStream(data))
            {
                id = Serializer.DeserializeWithLengthPrefix<ulong>(ms, PrefixStyle.Base128);
                arg1 = Serializer.DeserializeWithLengthPrefix<T1>(ms, PrefixStyle.Base128);
                arg2 = Serializer.DeserializeWithLengthPrefix<T2>(ms, PrefixStyle.Base128);
                arg3 = Serializer.DeserializeWithLengthPrefix<T3>(ms, PrefixStyle.Base128);
                arg4 = Serializer.DeserializeWithLengthPrefix<T4>(ms, PrefixStyle.Base128);

                return;
            }
        }

        /// <summary>
        /// Serializable class representing a null value.
        /// </summary>
        [ProtoContract]
        public class ProtoNull
        {
        }
    }
}
