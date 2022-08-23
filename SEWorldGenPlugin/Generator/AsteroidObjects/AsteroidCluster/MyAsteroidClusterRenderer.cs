using Sandbox.Engine.Utils;
using SEWorldGenPlugin.Draw;
using SEWorldGenPlugin.GUI.AdminMenu.SubMenus.StarSystemDesigner;
using SEWorldGenPlugin.ObjectBuilders;
using VRageMath;

namespace SEWorldGenPlugin.Generator.AsteroidObjects.AsteroidCluster
{
    /// <summary>
    /// Class to visualize Asteroid clusters in the star system designer
    /// </summary>
    public class MyAsteroidClusterRenderer : MyOrbitRenderObject
    {
        /// <summary>
        /// Data associated with the cluster
        /// </summary>
        private MyAsteroidClusterData m_data;

        /// <summary>
        /// Max radius of an asteroid cluster. Only used for visual clamping of visualization sphere, so larger asteroid clusters dont appear as gigantic spheres.
        /// </summary>
        private const double MAX_RAD = 240000;

        /// <summary>
        /// Render object for the cluster
        /// </summary>
        private RenderSphere m_render;

        public MyAsteroidClusterRenderer(MySystemAsteroids instance, MyAsteroidClusterData data) : base(instance)
        {
            m_data = data;
            m_render = new RenderSphere(instance.CenterPosition, (float)data.Size, Color.Brown);
        }

        public override void Draw()
        {
            base.Draw();

            m_render.Position = RenderObject.CenterPosition;
            m_render.Radius = (float)m_data.Size;
            m_render.Color = IsFocused ? Color.LightBlue : Color.Brown;

            UpdateClusterSphereSize();

            m_render.Draw();
        }

        public override double GetObjectSize()
        {
            return m_data.Size;
        }

        /// <summary>
        /// Updates the render sphere size to be appropriatly sized
        /// </summary>
        private void UpdateClusterSphereSize()
        {
            var specPos = MySpectatorCameraController.Static.Position;
            double distance = Vector3D.Distance(RenderObject.CenterPosition, specPos);

            double multiplier = distance / 20000000f;

            double size = m_data.Size * multiplier;

            double radius = m_data.Size > MAX_RAD ? MAX_RAD : m_data.Size;

            if (size > radius * 1000)
            {
                size = radius * 1000;
            }
            else if (size < m_data.Size)
            {
                size = (float)(m_data.Size);
            }

            m_render.Radius = (float)size;
        }
    }
}
