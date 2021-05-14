using HarmonyLib;
using Sandbox.Game.World.Generator;
using SEWorldGenPlugin.Generator.ProceduralGeneration;
using SEWorldGenPlugin.Utilities;
using System.Collections.Generic;
using VRageMath;

namespace SEWorldGenPlugin.Patches
{
    /// <summary>
    /// Patch class to patch the bug with jumping inside asteroid rings not considering asteroids for collision
    /// </summary>
    public class PatchOverlapAllAsteroidSeedsInSphere : HarmonyPatchBase
    {
        public PatchOverlapAllAsteroidSeedsInSphere() : base("Asteroids jumpdrive fix")
        {
        }

        public static void Postfix(BoundingSphereD area, List<MyObjectSeed> list)
        {
            MyProceduralGeneratorComponent.Static.GetCellSeedsInBounds(area, list);
        }

        public override void ApplyPatch(Harmony harmony)
        {
            base.ApplyPatch(harmony);

            var baseMethod = typeof(MyProceduralWorldGenerator).GetMethod("OverlapAllAsteroidSeedsInSphere");
            var postfix = typeof(PatchOverlapAllAsteroidSeedsInSphere).GetMethod("Postfix");

            harmony.Patch(baseMethod, postfix: new HarmonyMethod(postfix));
        }
    }
}
