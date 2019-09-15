using Sandbox.Game.Entities;
using Sandbox.Game.World;
using SEWorldGenPlugin.Generator.ProceduralGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage.Game.Components;

namespace SEWorldGenPlugin.Session
{
    [MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation, 501)]
    public class PlayerTracker : MySessionComponentBase
    {
        public override void UpdateBeforeSimulation()
        {
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
