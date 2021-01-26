using Sandbox.Game.World;
using SEWorldGenPlugin.Draw;
using SEWorldGenPlugin.ObjectBuilders;
using System;
using VRage.Game;
using VRage.Game.Components;
using VRage.Input;
using VRageMath;

namespace SEWorldGenPlugin.Session
{
    /// <summary>
    /// Session component that is used to hold and paste a MySystemItem
    /// into the world. It will update a visual for the item, which currently only works with planets.
    /// </summary>
    [MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation)]
    public class MyPluginItemsClipboard : MySessionComponentBase
    {
        /// <summary>
        /// Singleton instance of this session component
        /// </summary>
        public static MyPluginItemsClipboard Static;

        /// <summary>
        /// Currently copied item
        /// </summary>
        private MySystemObject m_copiedItem = null;

        /// <summary>
        /// Callpack for the paste event of the currently copied item
        /// </summary>
        private Action<MySystemObject, Vector3D> m_callback;

        /// <summary>
        /// If the clipboard is currently active
        /// </summary>
        private bool m_isActive = false;

        /// <summary>
        /// Distance of the to paste object to the camera
        /// </summary>
        private float m_distanceToCam;

        /// <summary>
        /// Current position of the to be pasted item
        /// </summary>
        private Vector3D m_currentPos;

        /// <summary>
        /// Initializes this session component and its singleton instance
        /// </summary>
        /// <param name="sessionComponent"></param>
        public override void Init(MyObjectBuilder_SessionComponent sessionComponent)
        {
            base.Init(sessionComponent);
            Static = this;
        }

        /// <summary>
        /// Activates this clipboard with the given MySystemItem and sets the callback for the paste event
        /// </summary>
        /// <param name="item">Item to copy and paste</param>
        /// <param name="callback">Callback to call, when the item gets pasted</param>
        /// <param name="distanceToCam">The distance the object has to the camera</param>
        public void Activate(MySystemObject item, Action<MySystemObject, Vector3D> callback, float distanceToCam)
        {
            m_copiedItem = item;
            m_callback = callback;
            m_isActive = true;
            m_distanceToCam = distanceToCam;
        }

        /// <summary>
        /// Deactivates the currently used clipboard
        /// </summary>
        public void Deactivate()
        {
            m_copiedItem = null;
            m_callback = null;
            m_isActive = false;
            m_distanceToCam = 0;
        }

        /// <summary>
        /// Handles player input. When the escape key is pressed, the copy and paste is cancelled
        /// and all cleared. When you press the left mouse button, the item is pasted and the callback is called, if the clipboard is active.
        /// </summary>
        public override void HandleInput()
        {
            base.HandleInput();
            if (MyInput.Static.IsNewKeyPressed(MyKeys.Escape))
            {
                if (m_isActive)
                {
                    MyPluginDrawSession.Static.RemoveRenderObject(m_copiedItem.GetHashCode());
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
                    MyPluginDrawSession.Static.RemoveRenderObject(m_copiedItem.GetHashCode());
                    m_callback?.Invoke(m_copiedItem, m_currentPos);
                    m_copiedItem = null;
                    m_callback = null;
                    m_distanceToCam = 0;
                    m_isActive = false;
                }
            }
        }

        /// <summary>
        /// Runs before simulation update. If the clipboard is active,
        /// draws the copied item, incase of a planet, into the world as a sphere and updates its position.
        /// </summary>
        public override void UpdateBeforeSimulation()
        {
            if (m_isActive)
            {
                MyPluginDrawSession.Static.RemoveRenderObject(m_copiedItem.GetHashCode());

                MatrixD wm = GetPasteMatrix();

                Vector3D posGlobal = wm.Forward * m_distanceToCam;

                m_currentPos = wm.Translation + posGlobal;

                if(m_copiedItem.GetType() == typeof(MySystemPlanet))
                    MyPluginDrawSession.Static.AddRenderObject(m_copiedItem.GetHashCode(), new RenderSphere(m_currentPos, (float)((MySystemPlanet)m_copiedItem).Diameter / 2, Color.LightGreen));
            }
        }

        /// <summary>
        /// Gets the paste world matrix used to calculate the position of the object in relation to the camera.
        /// </summary>
        /// <returns>The head matrix or world matrix of the player.</returns>
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
