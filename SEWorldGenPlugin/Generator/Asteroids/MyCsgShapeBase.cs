using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage.Noise;
using VRageMath;

namespace SEWorldGenPlugin.Generator.Asteroids
{
    internal abstract class MyCsgShapeBase
    {
        protected bool m_enableModulation;

        protected float m_detailSize;

        protected MyCsgShapeBase()
        {
            m_enableModulation = true;
            m_detailSize = 6f;
        }

        internal abstract ContainmentType Contains(ref BoundingBox queryAabb, ref BoundingSphere querySphere, float lodVoxelSize);

        internal abstract float SignedDistance(ref Vector3 position, float lodVoxelSize, IMyModule macroModulator, IMyModule detailModulator);

        internal abstract float SignedDistanceUnchecked(ref Vector3 position, float lodVoxelSize, IMyModule macroModulator, IMyModule detailModulator);

        internal virtual void DebugDraw(ref MatrixD worldTranslation, Color color)
        {
        }

        internal abstract MyCsgShapeBase DeepCopy();

        internal abstract void ShrinkTo(float percentage);

        internal abstract Vector3 Center();

        internal virtual void ReleaseMaps()
        {
        }
    }
}
