using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage.Noise;
using VRageMath;
using VRageRender;

/*
 * Code is taken from the Space Engineers code base, since this class is not viewable for mods but is needed to generate Asteroids/Planets
 */
namespace SEWorldGenPlugin.Generator.Asteroids
{
    internal class MyCsgCapsule : MyCsgShapeBase
    {
        private Vector3 m_pointA;

        private Vector3 m_pointB;

        private float m_radius;

        private float m_halfDeviation;

        private float m_deviationFrequency;

        private float m_detailFrequency;

        private float m_potentialHalfDeviation;

        public MyCsgCapsule(Vector3 pointA, Vector3 pointB, float radius, float halfDeviation, float deviationFrequency, float detailFrequency)
        {
            m_pointA = pointA;
            m_pointB = pointB;
            m_radius = radius;
            m_halfDeviation = halfDeviation;
            m_deviationFrequency = deviationFrequency;
            m_detailFrequency = detailFrequency;
            if (deviationFrequency == 0f)
            {
                m_enableModulation = false;
            }
            m_potentialHalfDeviation = m_halfDeviation + m_detailSize;
        }

        internal override ContainmentType Contains(ref BoundingBox queryAabb, ref BoundingSphere querySphere, float lodVoxelSize)
        {
            Vector3 v = m_pointB - m_pointA;
            float max = v.Normalize();
            float value = (querySphere.Center - m_pointA).Dot(ref v);
            value = MathHelper.Clamp(value, 0f, max);
            Vector3 value2 = m_pointA + value * v;
            float num = (querySphere.Center - value2).Length() - m_radius;
            float num2 = m_potentialHalfDeviation + lodVoxelSize + querySphere.Radius;
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
            Vector3 value = position - m_pointA;
            Vector3 v = m_pointB - m_pointA;
            float scaleFactor = MathHelper.Clamp(value.Dot(ref v) / v.LengthSquared(), 0f, 1f);
            float signedDistance = (value - v * scaleFactor).Length() - m_radius;
            float num = m_potentialHalfDeviation + lodVoxelSize;
            if (signedDistance > num)
            {
                return 1f;
            }
            if (signedDistance < 0f - num)
            {
                return -1f;
            }
            return SignedDistanceInternal(ref position, lodVoxelSize, macroModulator, detailModulator, ref signedDistance);
        }

        private float SignedDistanceInternal(ref Vector3 position, float lodVoxelSize, IMyModule macroModulator, IMyModule detailModulator, ref float signedDistance)
        {
            if (m_enableModulation)
            {
                float num = (float)macroModulator.GetValue(position.X * m_deviationFrequency, position.Y * m_deviationFrequency, position.Z * m_deviationFrequency);
                signedDistance -= num * m_halfDeviation;
            }
            if (m_enableModulation && 0f - m_detailSize < signedDistance && signedDistance < m_detailSize)
            {
                signedDistance += m_detailSize * (float)detailModulator.GetValue(position.X * m_detailFrequency, position.Y * m_detailFrequency, position.Z * m_detailFrequency);
            }
            return signedDistance / lodVoxelSize;
        }

        internal override float SignedDistanceUnchecked(ref Vector3 position, float lodVoxelSize, IMyModule macroModulator, IMyModule detailModulator)
        {
            Vector3 value = position - m_pointA;
            Vector3 v = m_pointB - m_pointA;
            float scaleFactor = MathHelper.Clamp(value.Dot(ref v) / v.LengthSquared(), 0f, 1f);
            float signedDistance = (value - v * scaleFactor).Length() - m_radius;
            return SignedDistanceInternal(ref position, lodVoxelSize, macroModulator, detailModulator, ref signedDistance);
        }

        internal override void DebugDraw(ref MatrixD worldTranslation, Color color)
        {
            Vector3D p = Vector3D.Transform(m_pointA, worldTranslation);
            Vector3D p2 = Vector3D.Transform(m_pointB, worldTranslation);
            MyRenderProxy.DebugDrawCapsule(p, p2, m_radius, color, depthRead: true, shaded: true);
        }

        internal override MyCsgShapeBase DeepCopy()
        {
            float detailFrequency = m_detailFrequency;
            float deviationFrequency = m_deviationFrequency;
            float halfDeviation = m_halfDeviation;
            return new MyCsgCapsule(m_pointA, m_pointB, m_radius, halfDeviation, deviationFrequency, detailFrequency);
        }

        internal override void ShrinkTo(float percentage)
        {
            m_radius *= percentage;
            m_halfDeviation *= percentage;
        }

        internal override Vector3 Center()
        {
            return (m_pointA + m_pointB) / 2f;
        }
    }
}
