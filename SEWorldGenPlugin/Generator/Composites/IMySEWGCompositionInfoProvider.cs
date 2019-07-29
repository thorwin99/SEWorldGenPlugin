using Sandbox.Game.World.Generator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage.Game;

namespace SEWorldGenPlugin.Generator.Composites
{
    internal interface IMySEWGCompositionInfoProvider
    {
        IMyCompositeDeposit[] Deposits
        {
            get;
        }

        IMyCompositeShape[] FilledShapes
        {
            get;
        }

        IMyCompositeShape[] RemovedShapes
        {
            get;
        }

        MyVoxelMaterialDefinition DefaultMaterial
        {
            get;
        }

        void Close();
    }
}
