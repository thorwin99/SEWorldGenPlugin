using SEWorldGenPlugin.Draw;
using SEWorldGenPlugin.Generator;
using SEWorldGenPlugin.ObjectBuilders;
using SEWorldGenPlugin.Session;
using System;
using VRageMath;
using VRageRender;

namespace SEWorldGenPlugin.GUI.AdminMenu.SubMenus.StarSystemDesigner
{
    /// <summary>
    /// A class simply used to implement the <see cref="MyAbstractStarSystemDesignerRenderObject"/> for
    /// objects that wont be rendered, but still can be focused, such as the system center
    /// </summary>
    public class MyEmptyObjectRenderObject : MyAbstractStarSystemDesignerRenderObject
    {
        /// <summary>
        /// The circle for orbit visualization
        /// </summary>
        private RenderCircle m_orbitRender;

        /// <summary>
        /// The parent object the orbit goes around
        /// </summary>
        private MySystemObject m_parent;

        public MyEmptyObjectRenderObject(MySystemObject obj) : base(obj)
        {
            m_parent = MyStarSystemGenerator.Static.StarSystem.GetById(obj.ParentId);
            m_orbitRender = new RenderCircle(CalculateWorldMatrix(), (float)CalculateRadius(), Color.Orange.ToVector4());
        }

        public override void Draw()
        {
            Color color = IsFocused ? Color.Green : Color.LightBlue;
            MyRenderProxy.DebugDrawText3D(RenderObject.CenterPosition, RenderObject.DisplayName, color, 0.25f, true, align: VRage.Utils.MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER);

            m_orbitRender.Radius = (float)CalculateRadius();
            m_orbitRender.WorldMatrix = CalculateWorldMatrix();
            m_orbitRender.Draw();
        }

        public override double GetObjectSize()
        {
            return MySettingsSession.Static.Settings.GeneratorSettings.PlanetSettings.PlanetSizeCap;
        }

        /// <summary>
        /// Calculates the radius of this orbit
        /// </summary>
        /// <returns></returns>
        private double CalculateRadius()
        {
            if (m_parent == null)
            {
                return Vector3D.Distance(Vector3D.Zero, RenderObject.CenterPosition);
            }
            else
            {
                return Vector3D.Distance(m_parent.CenterPosition, RenderObject.CenterPosition);
            }
        }

        /// <summary>
        /// Calculates a world matrix for this orbit
        /// </summary>
        /// <returns>The world matrix</returns>
        private MatrixD CalculateWorldMatrix()
        {
            Vector3D center = Vector3D.Zero;
            if (m_parent != null)
            {
                center = m_parent.CenterPosition;
            }

            Vector3D forward = Vector3D.Subtract(RenderObject.CenterPosition, center);
            Vector3D fn = Vector3D.Normalize(forward);
            double radius = CalculateRadius();

            double elevation = Math.Asin(forward.Z / radius);
            double orbitRad = Math.Acos(forward.X / Math.Cos(elevation) / radius);

            if (forward.Y < 0)
            {
                orbitRad = Math.PI * 2 - orbitRad;
            }

            MatrixD my = MatrixD.CreateRotationY(elevation);
            MatrixD mz = MatrixD.CreateRotationZ(orbitRad);

            Vector3D up = new Vector3D(0, 0, 1);
            up = Vector3D.Rotate(up, my * mz);

            return MatrixD.CreateWorld(center, fn, up);
        }
    }
}
