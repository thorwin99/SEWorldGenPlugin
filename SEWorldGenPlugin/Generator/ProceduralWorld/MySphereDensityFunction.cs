using Sandbox.Game.World.Generator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage.Noise;
using VRageMath;

namespace SEWorldGenPlugin.Generator.ProceduralWorld
{
    internal class MySphereDensityFunction : IMyAsteroidFieldDensityFunction, IMyModule
    {
        private Vector3D m_center;

        private BoundingSphereD m_sphereMax;

        private double m_innerRadius;

        private double m_outerRadius;

        private double m_middleRadius;

        private double m_halfFalloff;

        public MySphereDensityFunction(Vector3D center, double radius, double additionalFalloff)
        {
            m_center = center;
            m_sphereMax = new BoundingSphereD(center, radius + additionalFalloff);
            m_innerRadius = radius;
            m_halfFalloff = additionalFalloff / 2.0;
            m_middleRadius = radius + m_halfFalloff;
            m_outerRadius = radius + additionalFalloff;
        }

        public bool ExistsInCell(ref BoundingBoxD bbox)
        {
            return m_sphereMax.Contains(bbox) != ContainmentType.Disjoint;
        }

        public double GetValue(double x)
        {
            throw new NotImplementedException();
        }

        public double GetValue(double x, double y)
        {
            throw new NotImplementedException();
        }

        public double GetValue(double x, double y, double z)
        {
            double num = Vector3D.Distance(m_center, new Vector3D(x, y, z));
            if (num > m_outerRadius)
            {
                return 1.0;
            }
            if (num < m_innerRadius)
            {
                return -1.0;
            }
            if (num > m_middleRadius)
            {
                return (m_middleRadius - num) / (0.0 - m_halfFalloff);
            }
            return (num - m_middleRadius) / m_halfFalloff;
        }
    }
}
