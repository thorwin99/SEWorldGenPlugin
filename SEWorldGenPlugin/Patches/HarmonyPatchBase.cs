using HarmonyLib;
using SEWorldGenPlugin.Utilities;

namespace SEWorldGenPlugin.Patches
{
    /// <summary>
    /// base class for harmony patches
    /// </summary>
    public class HarmonyPatchBase
    {
        private string name;

        /// <summary>
        /// Base class for harmony patches
        /// </summary>
        /// <param name="patchName"></param>
        public HarmonyPatchBase(string patchName)
        {
            name = patchName;
        }

        /// <summary>
        /// Apply the patch
        /// </summary>
        public virtual void ApplyPatch(Harmony harmony)
        {
            MyPluginLog.Log("Applying patch '" + name + "'");
        }
    }
}
