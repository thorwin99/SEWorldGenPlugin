using SEWorldGenPlugin.http;
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

        public override void Init(MyObjectBuilder_SessionComponent sessionComponent)
        {
            VersionCheck.Static.CompareVersionWithServer((res) =>
            {
                ServerVersionMatch = res;
            });
        }
    }
}
