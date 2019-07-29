using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage.Game;
using VRageMath;

namespace SEWorldGenPlugin.Generator.Asteroids
{
    /**
     *
     * Code from Space engineers github:
     * https://github.com/KeenSoftwareHouse/SpaceEngineers/
     *
    */
    class MyCompositeShapeOreDeposit
    {
        public readonly MyCsgShapeBase Shape;
        protected readonly MyVoxelMaterialDefinition m_material;

        virtual public void DebugDraw(ref Vector3D translation, ref Color materialColor)
        {
            Shape.DebugDraw(ref translation, materialColor);
            VRageRender.MyRenderProxy.DebugDrawText3D(Shape.Center() + translation, m_material.Id.SubtypeName, Color.White, 1f, false);
        }

        public MyCompositeShapeOreDeposit(MyCsgShapeBase shape, MyVoxelMaterialDefinition material)
        {
            System.Diagnostics.Debug.Assert(material != null, "Shape must have material");
            Shape = shape;
            m_material = material;
        }

        public virtual MyVoxelMaterialDefinition GetMaterialForPosition(ref Vector3 pos, float lodSize)
        {
            return m_material;
        }
    }
}
