﻿using HarmonyLib;
using SEWorldGenPlugin.Utilities;
using System.IO;
using VRage.Utils;

namespace SEWorldGenPlugin.Patches
{
    /// <summary>
    /// Class to patch the <see cref="MyUtils.CopyDirectory"/> function to also copy sub directories.
    /// </summary>
    public class CopyDirectoryPatch : HarmonyPatchBase
    {
        public CopyDirectoryPatch() : base("Copy directory patch.")
        {
        }

        public static void Postfix(string source, string destination)
        {
            if(Directory.Exists(source))
            {
                foreach (string text in Directory.GetDirectories(source))
                {
                    string fileName = Path.GetFileName(text);
                    string destFileName = Path.Combine(destination, fileName);

                    Directory.CreateDirectory(destFileName);
                    MyUtils.CopyDirectory(text, destFileName);
                }
            }
        }

        public override void ApplyPatch(Harmony harmony)
        {
            base.ApplyPatch(harmony);

            var baseMethod = typeof(MyUtils).GetMethod("CopyDirectory");
            var postfix = typeof(CopyDirectoryPatch).GetMethod("Postfix");

            harmony.Patch(baseMethod, postfix: new HarmonyMethod(postfix));
        }
    }
}
