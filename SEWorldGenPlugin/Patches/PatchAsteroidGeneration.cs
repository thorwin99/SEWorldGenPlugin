using HarmonyLib;
using Sandbox.Game.World.Generator;
using SEWorldGenPlugin.Session;
using VRage.Library.Utils;
using VRage.Noise;

namespace SEWorldGenPlugin.Patches
{
    /// <summary>
    /// Patch class to patch vanilla asteroid generation, so that encounter generation is still possible
    /// </summary>
    public class PatchAsteroidGeneration : HarmonyPatchBase
    {
        public PatchAsteroidGeneration() : base("Asteroid generator patch (encounter fix)")
        {
        }

        /// <summary>
        /// Prefix patch applied to <see cref="MyProceduralAsteroidCellGenerator.GenerateObjects(System.Collections.Generic.List{MyObjectSeed}, System.Collections.Generic.HashSet{VRage.Game.MyObjectSeedParams})"/>
        /// </summary>
        /// <param name="cell">Cell to generate objects for</param>
        /// <param name="objectSeed">Object to generate for cell</param>
        /// <param name="index"></param>
        /// <param name="random"></param>
        /// <param name="densityFunctionFilled"></param>
        /// <param name="densityFunctionRemoved"></param>
        /// <returns>True when original method should execute, false if skipped</returns>
        public static bool Prefix(MyProceduralCell cell, MyObjectSeed objectSeed, ref int index, MyRandom random, IMyModule densityFunctionFilled, IMyModule densityFunctionRemoved)
        {
            if (!MySettingsSession.Static.Settings.Enabled) return true;

            if (MySettingsSession.Static.Settings.GeneratorSettings.AsteroidGenerator != ObjectBuilders.AsteroidGenerationMethod.PLUGIN) return true;

            if(objectSeed.Params.Type == VRage.Game.MyObjectSeedType.Asteroid || objectSeed.Params.Type == VRage.Game.MyObjectSeedType.AsteroidCluster)
            {
                return false; // skip original method
            }

            return true;
        }

        public override void ApplyPatch(Harmony harmony)
        {
            base.ApplyPatch(harmony);

            var baseMethod = typeof(MyProceduralAsteroidCellGenerator).GetMethod("GenerateObject", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var prefix = typeof(PatchAsteroidGeneration).GetMethod("Prefix");

            harmony.Patch(baseMethod, prefix: new HarmonyMethod(prefix));
        }
    }
}
