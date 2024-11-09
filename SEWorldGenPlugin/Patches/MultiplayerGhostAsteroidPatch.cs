using HarmonyLib;
using Sandbox.Game.Multiplayer;
using Sandbox.Game.World;
using SEWorldGenPlugin.Session;
using VRage.Game;

namespace SEWorldGenPlugin.Patches
{
    /// <summary>
    /// Patch class to patch the client side procedural generation on a multiplayer server.
    /// If a player connects to a server, the client requests the server world settings. If the procedural density is not 0, the client will generate asteroids.
    /// </summary>
    public class MultiplayerGhostAsteroidPatch : HarmonyPatchBase
    {
        public MultiplayerGhostAsteroidPatch() : base("Multiplayer ghost asteroid fix")
        {
        }

        public static void Postfix(bool includeEntities, bool isClientRequest, ref MyObjectBuilder_World __result)
        {
            if (Sync.IsServer && isClientRequest && MySettingsSession.Static.Settings.GeneratorSettings.AsteroidGenerator == ObjectBuilders.AsteroidGenerationMethod.PLUGIN)
            {
                __result.Checkpoint.Settings.ProceduralDensity = 0;
            }
        }

        public override void ApplyPatch(Harmony harmony)
        {
            base.ApplyPatch(harmony);

            var baseMethod = typeof(MySession).GetMethod("GetWorld", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            var postfix = typeof(MultiplayerGhostAsteroidPatch).GetMethod("Postfix");

            harmony.Patch(baseMethod, postfix: new HarmonyMethod(postfix));
        }
    }
}
