using HarmonyLib;
using Sandbox.Game.World.Generator;
using SEWorldGenPlugin.Generator.ProceduralGeneration;
using SEWorldGenPlugin.Utilities;
using System.Collections.Generic;
using VRageMath;

namespace SEWorldGenPlugin.Patches
{
    [HarmonyPatch(typeof(MyProceduralWorldGenerator), "OverlapAllAsteroidSeedsInSphere")]
    public static class Patch_OverlapAllAsteroidSeedsInSphere
    {
        public static void Postfix(BoundingSphereD area, List<MyObjectSeed> list)
        {
            MyProceduralGeneratorComponent.Static.GetCellSeedsInBounds(area, list);
        }
    }
}
