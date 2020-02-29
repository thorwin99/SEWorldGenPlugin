using Sandbox.Game.World;
using SEWorldGenPlugin.Draw;
using SEWorldGenPlugin.ObjectBuilders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage.Game;
using VRage.Game.Components;
using VRage.Input;
using VRageMath;

namespace SEWorldGenPlugin.Session
{
    [MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation)]
    public class PluginItemsClipboard : MySessionComponentBase
    {
        public static PluginItemsClipboard Static;

        private MySystemItem m_copiedItem = null;
        private Action<MySystemItem, Vector3D> m_callback;
        private bool m_isActive = false;
        private float m_distanceToCam;
        private Vector3D m_currentPos;

        public override void Init(MyObjectBuilder_SessionComponent sessionComponent)
        {
            base.Init(sessionComponent);
            Static = this;
        }

        public void Activate(MySystemItem item, Action<MySystemItem, Vector3D> callback, float distanceToCam)
        {
            m_copiedItem = item;
            m_callback = callback;
            m_isActive = true;
            m_distanceToCam = distanceToCam;
        }

        public override void HandleInput()
        {
            base.HandleInput();
            if (MyInput.Static.IsNewKeyPressed(MyKeys.Escape))
            {
                if (m_isActive)
                {
                    PluginDrawSession.Static.RemoveRenderObject(m_copiedItem.GetHashCode());
                    m_copiedItem = null;
                    m_callback = null;
                    m_distanceToCam = 0;
                    m_isActive = false;
                }
                
            }
            if (MyInput.Static.IsNewLeftMousePressed())
            {
                if (m_isActive)
                {
                    PluginDrawSession.Static.RemoveRenderObject(m_copiedItem.GetHashCode());
                    m_callback?.Invoke(m_copiedItem, m_currentPos);
                    m_copiedItem = null;
                    m_callback = null;
                    m_distanceToCam = 0;
                    m_isActive = false;
                }
            }
        }

        public override void UpdateBeforeSimulation()
        {
            if (m_isActive)
            {
                PluginDrawSession.Static.RemoveRenderObject(m_copiedItem.GetHashCode());
                MatrixD wm = GetPasteMatrix();

                Vector3D posGlobal = wm.Forward * m_distanceToCam;

                m_currentPos = wm.Translation + posGlobal;

                PluginDrawSession.Static.AddRenderObject(m_copiedItem.GetHashCode(), new RenderSphere(m_currentPos, ((MyPlanetItem)m_copiedItem).Size / 2, Color.LightGreen));
            }
        }

        private static MatrixD GetPasteMatrix()
        {
            if (MySession.Static.ControlledEntity != null &&
                (MySession.Static.GetCameraControllerEnum() == MyCameraControllerEnum.Entity || MySession.Static.GetCameraControllerEnum() == MyCameraControllerEnum.ThirdPersonSpectator))
            {
                return MySession.Static.ControlledEntity.GetHeadMatrix(true);
            }
            else
            {
                return MySector.MainCamera.WorldMatrix;
            }
        }
    }
}
