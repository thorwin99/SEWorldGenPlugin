using SEWorldGenPlugin.Draw;
using System.Collections.Generic;
using VRage.Game;
using VRage.Game.Components;
using VRage.Utils;

namespace SEWorldGenPlugin.Session
{
    [MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation)]
    public class PluginDrawSession : MySessionComponentBase
    {
        public static PluginDrawSession Static;

        public override void Init(MyObjectBuilder_SessionComponent sessionComponent)
        {
            base.Init(sessionComponent);

            Static = this;
        }

        private Dictionary<int, IRenderObject> m_toRender = new Dictionary<int, IRenderObject>();

        public bool AddRenderObject(int id, IRenderObject render)
        {
            if (m_toRender.ContainsKey(id)) return false;
            m_toRender.Add(id, render);
            return true;
        }

        public void RemoveRenderObject(int id)
        {
            m_toRender.Remove(id);
        }

        public override void Draw()
        {
            foreach(IRenderObject obj in m_toRender.Values)
            {
                obj.Draw();
            }
            base.Draw();
        }
    }
}
