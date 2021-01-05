using Sandbox.Game.World;
using SEWorldGenPlugin.Generator.ProceduralGen;
using VRage.Game.Components;

namespace SEWorldGenPlugin.Session
{
    /// <summary>
    /// Sessioncomponent that manages the tracking of players for the procedural generator
    /// </summary>
    [MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation, 501)]
    public class MyPlayerTrackerComponent : MySessionComponentBase
    {
        /// <summary>
        /// Runs before simulation update. If the plugin is enabled, will
        /// track every player that is currently online for the procedural generator.
        /// </summary>
        public override void UpdateBeforeSimulation()
        {
            if (!MySettingsSession.Static.Settings.Enabled) return;

            foreach(var player in MySession.Static.Players.GetAllPlayers())
            {
                var p = player;
                if(MySession.Static.Players.IsPlayerOnline(ref p))
                {
                    MyPlayer ent;
                    if(!MySession.Static.Players.TryGetPlayerById(player, out ent))continue;

                    ProceduralGenerator.Static.TrackEntity(ent.Character);
                }
            }
        }
    }
}
