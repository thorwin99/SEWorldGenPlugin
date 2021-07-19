using SEWorldGenPlugin.http;
using SEWorldGenPlugin.Utilities;
using VRage.Game;
using VRage.Game.Components;

namespace SEWorldGenPlugin.Session
{
    /// <summary>
    /// Session component to compare server and client side of the plugin (e.g. version)
    /// </summary>
    [MySessionComponentDescriptor(MyUpdateOrder.NoUpdate, 1005)]
    public class MyPluginSession : MySessionComponentBase
    {
        /// <summary>
        /// Static instance of this session component
        /// </summary>
        public static MyPluginSession Static;

        /// <summary>
        /// Whether the client and server version of the plugin match up.
        /// </summary>
        public bool ServerVersionMatch
        {
            get;
            private set;
        }

        public MyPluginSession()
        {
            ServerVersionMatch = false;
        }

        public override void LoadData()
        {
            base.LoadData();
            Static = this;
        }

        public override void Init(MyObjectBuilder_SessionComponent sessionComponent)
        {
            MyPluginLog.Debug("Checking server version");
            VersionCheck.Static.CompareVersionWithServer((res) =>
            {
                ServerVersionMatch = res;
                if (res)
                {
                    MyPluginLog.Debug("Server and client version match up");
                }
                else
                {
                    MyPluginLog.Debug("Server and client version dont match up", LogLevel.WARNING);
                }
            });
        }
    }
}
