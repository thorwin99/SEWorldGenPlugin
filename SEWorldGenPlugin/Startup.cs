using Sandbox.Graphics.GUI;
using SpaceEngineers.Game;
using Sandbox.Game.Screens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage;
using VRage.Plugins;
using VRage.Utils;
using Sandbox.Game;
using Sandbox.Game.Entities;
using VRage.Game.Entity;
using SEWorldGenPlugin.Generator.ProceduralWorld;

namespace SEWorldGenPlugin
{
    public class Startup : IPlugin
    {
        public void Dispose()
        {
        }

        public void Init(object gameInstance)
        {
            //MyEntity.MyProceduralWorldGeneratorTrackEntityExtCallback += MyEntityExtensionCustom.CustomProceduralWorldGeneratorTrackEntity;
        }

        public void Update()
        {
        }
    }
}
