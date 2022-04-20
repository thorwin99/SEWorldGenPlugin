using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using VRage;
using VRage.Game.Components;

namespace SEWorldGenPluginMod.Source.Networking
{
    /// <summary>
    /// Session component used for mod communication with plugin
    /// </summary>
    [MySessionComponentDescriptor(MyUpdateOrder.NoUpdate)]
    public class MyPositionLookupEventHandler : MySessionComponentBase
    {
        /// <summary>
        /// Handler id used for communication
        /// </summary>
        private const ushort HANDLER_ID = 2779;

        /// <summary>
        /// Static instance for this class
        /// </summary>
        public static MyPositionLookupEventHandler Static;

        /// <summary>
        /// Callback for lookup response
        /// </summary>
        private Action<List<float>> m_lookupCallback;

        public override void LoadData()
        {
            if (MyAPIGateway.Multiplayer.IsServer) return;

            Static = this;

            MyModNetUtil.RegisterMessageHandler(HANDLER_ID, LookupResponseHandler);
        }

        protected override void UnloadData()
        {
            if (MyAPIGateway.Multiplayer.IsServer) return;

            Static = null;

            MyModNetUtil.UnregisterMessageHandlers(HANDLER_ID);
        }

        /// <summary>
        /// Sets a lookup callback which is called, when lookup responses come from the server.
        /// </summary>
        /// <param name="lookupCallback">Callback to call when lookup responses arrive.</param>
        public void SetCallback(Action<List<float>> lookupCallback)
        {
            m_lookupCallback = lookupCallback;
        }

        /// <summary>
        /// Send request to server to check positions, whether they lie within an asteroid field.
        /// </summary>
        /// <param name="positions">List of positions</param>
        public void RequestPositionLookup(List<SerializableVector3D> positions)
        {
            MyModNetUtil.SendPacketToServer(HANDLER_ID, PackData(positions));
        }

        /// <summary>
        /// Handles position lookup response message.
        /// </summary>
        /// <param name="senderId">Id of sender</param>
        /// <param name="data">Data to unpack</param>
        private void LookupResponseHandler(ulong senderId, byte[] data)
        {
            if (m_lookupCallback != null)
            {
                m_lookupCallback(UnpackData(data));
            }
        }

        /// <summary>
        /// Unpacks byte array data into bit array data type
        /// </summary>
        /// <param name="data">Data to unpack</param>
        /// <returns>BitArray represented by data</returns>
        private List<float> UnpackData(byte[] data)
        {
            return MyAPIGateway.Utilities.SerializeFromBinary<List<float>>(data);
        }

        /// <summary>
        /// Packs positions into byte array
        /// </summary>
        /// <param name="positions">Positions to serialize to byte array</param>
        /// <returns>Byte[] representation of <paramref name="positions"/></returns>
        private byte[] PackData(List<SerializableVector3D> positions)
        {
            return MyAPIGateway.Utilities.SerializeToBinary(positions);
        }
    }
}
