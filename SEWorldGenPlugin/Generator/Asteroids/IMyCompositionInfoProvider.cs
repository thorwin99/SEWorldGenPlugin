using Sandbox.Game.World.Generator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage.Game;

/*
 * Code is taken from the Space Engineers code base. 
 */
namespace SEWorldGenPlugin.Generator.Asteroids
{
    internal interface IMyCompositionInfoProvider
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
