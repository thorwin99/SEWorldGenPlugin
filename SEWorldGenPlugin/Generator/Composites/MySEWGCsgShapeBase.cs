using Sandbox.Engine.Utils;
using VRage.Game;
using VRage.Noise;
using VRageMath;
using VRageRender;

namespace SEWorldGenPlugin.Generator.Composites
{
    internal abstract class MySEWGCsgShapeBase
    {
        protected bool m_enableModulation;

        protected float m_detailSize;

        protected MySEWGCsgShapeBase()
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

        internal abstract MySEWGCsgShapeBase DeepCopy();

        internal abstract void ShrinkTo(float percentage);

        internal abstract Vector3 Center();

        internal virtual void ReleaseMaps()
        {
        }
    }
}