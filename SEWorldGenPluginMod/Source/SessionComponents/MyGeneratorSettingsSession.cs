using Sandbox.ModAPI;
using SEWorldGenPluginMod.Source.Networking;
using SEWorldGenPluginMod.Source.ObjectBuilders;
using VRage.Game.Components;

namespace SEWorldGenPluginMod.Source.SessionComponents
{
    /// <summary>
    /// Session component holding generator settings of the plugin.
    /// </summary>
    [MySessionComponentDescriptor(MyUpdateOrder.NoUpdate)]
    public class MyGeneratorSettingsSession : MySessionComponentBase
    {
        /// <summary>
        /// Handler id for generator settings requests.
        /// </summary>
        private const ushort SETTINGS_REQUEST_ID = 2780;

        /// <summary>
        /// Static instance of this component.
        /// </summary>
        public static MyGeneratorSettingsSession Static;

        /// <summary>
        /// Generator settings of server, relevant for mod.
        /// </summary>
        public MyGeneratorSettings Settings { get; private set; }

        public override void BeforeStart()
        {
            if (MyAPIGateway.Multiplayer.IsServer) return;

            MyModLog.Log("Sending request to retreive settings from server");

            MyModNetUtil.SendPacketToServer(SETTINGS_REQUEST_ID, "GetSettings");
        }

        public override void LoadData()
        {
            MyModLog.Log("Enabling generator settings component: " + !MyAPIGateway.Multiplayer.IsServer);

            if (MyAPIGateway.Multiplayer.IsServer) return;

            Static = this;

            MyModNetUtil.RegisterMessageHandler(SETTINGS_REQUEST_ID, SettingsRequestHandler);
        }

        /// <summary>
        /// Handler for settings request responses
        /// </summary>
        /// <param name="senderId">Sender id</param>
        /// <param name="data">Packed data</param>
        private void SettingsRequestHandler(ulong senderId, byte[] data)
        {
            var settings = UnpackSettings(data);

            MyModLog.Log("Received settings from server." + (settings != null));

            if (settings == null) return;

            Settings = settings;
        }

        protected override void UnloadData()
        {
            MyModLog.Log("Enabling generator settings component");

           Static = null;

            if (MyAPIGateway.Multiplayer.IsServer) return;

            MyModNetUtil.UnregisterMessageHandlers(SETTINGS_REQUEST_ID);
        }

        /// <summary>
        /// Unpack settings packet
        /// </summary>
        /// <param name="data">Packed settings data</param>
        /// <returns>Unpacked settings</returns>
        private MyGeneratorSettings UnpackSettings(byte[] data)
        {
            return MyAPIGateway.Utilities.SerializeFromBinary<MyGeneratorSettings>(data);
        }
    }
}
