using SEWorldGenPlugin.Draw;
using System.Collections.Generic;
using VRage.Game;
using VRage.Game.Components;

namespace SEWorldGenPlugin.Session
{
    /// <summary>
    /// Session component, that manages the drawing of objects rendered by the plugin.
    /// </summary>
    [MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation)]
    public class MyPluginDrawSession : MySessionComponentBase
    {
        /// <summary>
        /// Singleton instance of this session component
        /// </summary>
        public static MyPluginDrawSession Static;

        /// <summary>
        /// Initializes this component and sets the singleton instance
        /// </summary>
        /// <param name="sessionComponent"></param>
        public override void Init(MyObjectBuilder_SessionComponent sessionComponent)
        {
            base.Init(sessionComponent);

            Static = this;
        }

        /// <summary>
        /// Dictionary containing all currently rendered objects. The key is an id to identify the 
        /// object, while the value is the rendered object itself.
        /// </summary>
        private Dictionary<int, IRenderObject> m_toRender = new Dictionary<int, IRenderObject>();

        /// <summary>
        /// List of objects to render, essentially the same as m_toRender, but used to thread
        /// safe remove and add objects to render
        /// </summary>
        private List<IRenderObject> m_renderObjects = new List<IRenderObject>();

        /// <summary>
        /// Adds a new IRenderObject that should be rendered.
        /// </summary>
        /// <param name="id">Id of the rendered object</param>
        /// <param name="render">Rendered object</param>
        /// <returns></returns>
        public bool AddRenderObject(int id, IRenderObject render)
        {
            if (m_toRender.ContainsKey(id)) return false;

            m_toRender.Add(id, render);

            m_renderObjects.Add(render);
            return true;
        }

        /// <summary>
        /// Removes a rendered object from the managed objects list.
        /// </summary>
        /// <param name="id">Id of the object</param>
        public void RemoveRenderObject(int id)
        {
            if (!m_toRender.ContainsKey(id)) return;

            m_renderObjects.Remove(m_toRender[id]);

            m_toRender.Remove(id);
        }

        /// <summary>
        /// Draws all render objects into the world.
        /// </summary>
        public override void Draw()
        {
            foreach (IRenderObject obj in m_renderObjects.ToArray())
            {
                if(obj != null)
                    obj.Draw();
            }
            base.Draw();
        }
    }
}
