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

            double multiplier = distance / 10000000f;

            double size = m_data.Size * multiplier;

            if (multiplier > 300)
            {
                size = m_data.Size * 300;
            }
            else if (size < m_data.Size / 2f)
            {
                size = (float)(m_data.Size / 2f);
            }

            m_render.Radius = (float)size;
        }
    }
}
