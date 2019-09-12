using VRage.Noise;
using VRageMath;
using VRageRender;

/*
 * Code is taken from the Space Engineers code base, since this class is not viewable for mods but is needed to generate Asteroids/Planets
 */
namespace SEWorldGenPlugin.Generator.Asteroids
{
    internal class MyCsgSphere : MyCsgShapeBase
    {
        private Vector3 m_translation;

        private float m_radius;

        private float m_halfDeviation;

        private float m_deviationFrequency;

        private float m_detailFrequency;

        private float m_outerRadius;

        private float m_innerRadius;

        public MyCsgSphere(Vector3 translation, float radius, float halfDeviation = 0f, float deviationFrequency = 0f, float detailFrequency = 0f)
        {
            m_translation = translation;
            m_radius = radius;
            m_halfDeviation = halfDeviation;
            m_deviationFrequency = deviationFrequency;
            m_detailFrequency = detailFrequency;
            if (m_halfDeviation == 0f && m_deviationFrequency == 0f && detailFrequency == 0f)
            {
                m_enableModulation = false;
                m_detailSize = 0f;
            }
            ComputeDerivedProperties();
        }

        internal override ContainmentType Contains(ref BoundingBox queryAabb, ref BoundingSphere querySphere, float lodVoxelSize)
        {
            BoundingSphere boundingSphere = new BoundingSphere(m_translation, m_outerRadius + lodVoxelSize);
            boundingSphere.Contains(ref queryAabb, out ContainmentType result);
            if (result == ContainmentType.Disjoint)
            {
                return ContainmentType.Disjoint;
            }
            boundingSphere.Radius = m_innerRadius - lodVoxelSize;
            boundingSphere.Contains(ref queryAabb, out ContainmentType result2);
            if (result2 == ContainmentType.Contains)
            {
                return ContainmentType.Contains;
            }
            return ContainmentType.Intersects;
        }

        internal override float SignedDistance(ref Vector3 position, float lodVoxelSize, IMyModule macroModulator, IMyModule detailModulator)
        {
            Vector3 localPosition = position - m_translation;
            float num = localPosition.Length();
            if (m_innerRadius - lodVoxelSize > num)
            {
                return -1f;
            }
            if (m_outerRadius + lodVoxelSize < num)
            {
                return 1f;
            }
            return SignedDistanceInternal(lodVoxelSize, macroModulator, detailModulator, ref localPosition, num);
        }

        private float SignedDistanceInternal(float lodVoxelSize, IMyModule macroModulator, IMyModule detailModulator, ref Vector3 localPosition, float distance)
        {
            float num2;
            if (m_enableModulation)
            {
                float num = 0f;
                if (distance != 0f)
                {
                    num = 1f / distance;
                }
                float scaleFactor = m_deviationFrequency * m_radius * num;
                Vector3 vector = localPosition * scaleFactor;
                num2 = (float)macroModulator.GetValue(vector.X, vector.Y, vector.Z);
            }
            else
            {
                num2 = 0f;
            }
            float num3 = distance - m_radius - num2 * m_halfDeviation;
            if (m_enableModulation && 0f - m_detailSize < num3 && num3 < m_detailSize)
            {
                float scaleFactor2 = m_detailFrequency * m_radius / ((distance == 0f) ? 1f : distance);
                Vector3 vector2 = localPosition * scaleFactor2;
                num3 += m_detailSize * (float)detailModulator.GetValue(vector2.X, vector2.Y, vector2.Z);
            }
            return num3 / lodVoxelSize;
        }

        internal override float SignedDistanceUnchecked(ref Vector3 position, float lodVoxelSize, IMyModule macroModulator, IMyModule detailModulator)
        {
            Vector3 localPosition = position - m_translation;
            float distance = localPosition.Length();
            return SignedDistanceInternal(lodVoxelSize, macroModulator, detailModulator, ref localPosition, distance);
        }

        internal override void DebugDraw(ref MatrixD worldTranslation, Color color)
        {
            MyRenderProxy.DebugDrawSphere(Vector3D.Transform(m_translation, worldTranslation), m_radius, color.ToVector3(), 0.5f);
        }

        internal override MyCsgShapeBase DeepCopy()
        {
            return new MyCsgSphere(m_translation, m_radius, m_halfDeviation, m_deviationFrequency, m_detailFrequency);
        }

        internal override void ShrinkTo(float percentage)
        {
            m_radius *= percentage;
            m_halfDeviation *= percentage;
            ComputeDerivedProperties();
        }

        private void ComputeDerivedProperties()
        {
            m_outerRadius = m_radius + m_halfDeviation + m_detailSize;
            m_innerRadius = m_radius - m_halfDeviation - m_detailSize;
        }

        internal override Vector3 Center()
        {
            return m_translation;
        }
    }
}
