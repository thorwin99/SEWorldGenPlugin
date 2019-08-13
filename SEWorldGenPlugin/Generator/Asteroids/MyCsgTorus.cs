
using VRage.Noise;
using VRageMath;
using VRageRender;

namespace SEWorldGenPlugin.Generator.Asteroids
{
    internal sealed class MyCsgTorus : MyCsgShapeBase
    {
        private Vector3 m_translation;

        private Quaternion m_invRotation;

        private float m_primaryRadius;

        private float m_secondaryRadius;

        private float m_secondaryHalfDeviation;

        private float m_deviationFrequency;

        private float m_detailFrequency;

        private float m_potentialHalfDeviation;

        internal MyCsgTorus(Vector3 translation, Quaternion invRotation, float primaryRadius, float secondaryRadius, float secondaryHalfDeviation, float deviationFrequency, float detailFrequency)
        {
            m_translation = translation;
            m_invRotation = invRotation;
            m_primaryRadius = primaryRadius;
            m_secondaryRadius = secondaryRadius;
            m_deviationFrequency = deviationFrequency;
            m_detailFrequency = detailFrequency;
            m_potentialHalfDeviation = m_secondaryHalfDeviation + m_detailSize;
            if (m_detailFrequency == 0f)
            {
                m_enableModulation = false;
            }
        }

        internal override ContainmentType Contains(ref BoundingBox queryAabb, ref BoundingSphere querySphere, float lodVoxelSize)
        {
            BoundingSphere boundingSphere = querySphere;
            boundingSphere.Center -= m_translation;
            Vector3.Transform(ref boundingSphere.Center, ref m_invRotation, out boundingSphere.Center);
            boundingSphere.Radius += lodVoxelSize;
            float num = new Vector2(new Vector2(boundingSphere.Center.X, boundingSphere.Center.Z).Length() - m_primaryRadius, boundingSphere.Center.Y).Length() - m_secondaryRadius;
            float num2 = m_potentialHalfDeviation + lodVoxelSize + boundingSphere.Radius;
            if (num > num2)
            {
                return ContainmentType.Disjoint;
            }
            if (num < 0f - num2)
            {
                return ContainmentType.Contains;
            }
            return ContainmentType.Intersects;
        }

        internal override float SignedDistance(ref Vector3 position, float lodVoxelSize, IMyModule macroModulator, IMyModule detailModulator)
        {
            Vector3 value = position - m_translation;
            Vector3.Transform(ref value, ref m_invRotation, out value);
            float signedDistance = new Vector2(new Vector2(value.X, value.Z).Length() - m_primaryRadius, value.Y).Length() - m_secondaryRadius;
            float num = m_potentialHalfDeviation + lodVoxelSize;
            if (signedDistance > num)
            {
                return 1f;
            }
            if (signedDistance < 0f - num)
            {
                return -1f;
            }
            return SignedDistanceInternal(lodVoxelSize, macroModulator, detailModulator, ref value, ref signedDistance);
        }

        private float SignedDistanceInternal(float lodVoxelSize, IMyModule macroModulator, IMyModule detailModulator, ref Vector3 localPosition, ref float signedDistance)
        {
            if (m_enableModulation)
            {
                float scaleFactor = 0.5f * m_deviationFrequency;
                Vector3 vector = localPosition * scaleFactor;
                float num = (float)macroModulator.GetValue(vector.X, vector.Y, vector.Z);
                signedDistance -= num * m_secondaryHalfDeviation;
            }
            if (m_enableModulation && 0f - m_detailSize < signedDistance && signedDistance < m_detailSize)
            {
                float scaleFactor2 = 0.5f * m_detailFrequency;
                Vector3 vector2 = localPosition * scaleFactor2;
                signedDistance += m_detailSize * (float)detailModulator.GetValue(vector2.X, vector2.Y, vector2.Z);
            }
            return signedDistance / lodVoxelSize;
        }

        internal override float SignedDistanceUnchecked(ref VRageMath.Vector3 position, float lodVoxelSize, IMyModule macroModulator, IMyModule detailModulator)
        {
            Vector3 value = position - m_translation;
            Vector3.Transform(ref value, ref m_invRotation, out value);
            float signedDistance = new VRageMath.Vector2(new Vector2(value.X, value.Z).Length() - m_primaryRadius, value.Y).Length() - m_secondaryRadius;
            return SignedDistanceInternal(lodVoxelSize, macroModulator, detailModulator, ref value, ref signedDistance);
        }

        internal override void DebugDraw(ref MatrixD worldTranslation, Color color)
        {
            MatrixD matrix = MatrixD.CreateTranslation(m_translation) * worldTranslation;
            float num = (m_primaryRadius + m_secondaryRadius) * 2f;
            float num2 = m_secondaryRadius * 2f;
            MatrixD matrix2 = MatrixD.CreateScale(num, num2, num);
            MatrixD.CreateFromQuaternion(ref m_invRotation, out MatrixD result);
            MatrixD.Transpose(ref result, out result);
            MyRenderProxy.DebugDrawCylinder(matrix2 * result * matrix, color.ToVector3(), 0.5f, depthRead: true, smooth: false);
        }

        internal override MyCsgShapeBase DeepCopy()
        {
            return new MyCsgTorus(m_translation, m_invRotation, m_primaryRadius, m_secondaryRadius, m_secondaryHalfDeviation, m_deviationFrequency, m_detailFrequency);
        }

        internal override void ShrinkTo(float percentage)
        {
            m_secondaryRadius *= percentage;
            m_secondaryHalfDeviation *= percentage;
        }

        internal override Vector3 Center()
        {
            return m_translation;
        }
    }
}
