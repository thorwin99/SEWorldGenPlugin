//using Sandbox.Engine.Utils;
//using Sandbox.Game.World.Generator;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using VRage.Game.Entity;
//using VRage.Utils;

//namespace SEWorldGenPlugin.Generator.ProceduralWorld
//{
//    static class MyEntityExtensionCustom
//    {
//        public static void CustomProceduralWorldGeneratorTrackEntity(this MyEntity thisEntity)
//        {
//            MyLog.Default.WriteLine("Tracking Entity for custom ProcGen");
//            if (!MyFakes.ENABLE_ASTEROID_FIELDS || !MyFakes.ENABLE_ASTEROIDS)
//            {
//                MyLog.Default.WriteLine("Asteroids disabled");
//                if (MyProceduralWorldGenerator.Static != null && MyProceduralWorldGenerator.Static is MyCustomProceduralWorldGenerator)
//                {
//                    MyLog.Default.WriteLine("Is Static");
//                    ((MyCustomProceduralWorldGenerator)MyProceduralWorldGenerator.Static).TrackEntity(thisEntity);
//                }
//            }
//        }
//    }
//}
