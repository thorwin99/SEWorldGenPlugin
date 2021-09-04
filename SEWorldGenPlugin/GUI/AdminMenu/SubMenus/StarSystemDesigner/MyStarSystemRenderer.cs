using Sandbox.Engine.Utils;
using Sandbox.Game.World;
using SEWorldGenPlugin.Draw;
using SEWorldGenPlugin.Generator;
using SEWorldGenPlugin.ObjectBuilders;
using SEWorldGenPlugin.Utilities;
using System;
using System.Collections.Generic;
using VRage.Game;
using VRageMath;

namespace SEWorldGenPlugin.GUI.AdminMenu.SubMenus.StarSystemDesigner
{
    /// <summary>
    /// This class is used to render the star system in the star system designer
    /// </summary>
    public class MyStarSystemRenderer : IRenderObject
    {
        /// <summary>
        /// A dictionary of all render objects for the system objects
        /// </summary>
        private Dictionary<Guid, MyAbstractStarSystemDesignerRenderObject> m_systemRenderObjects;

        /// <summary>
        /// The target position of the camera to look at
        /// </summary>
        private Vector3D m_targetPos;

        public MyStarSystemRenderer()
        {
            m_systemRenderObjects = new Dictionary<Guid, MyAbstractStarSystemDesignerRenderObject>();
            MySession.Static.SetCameraController(MyCameraControllerEnum.Spectator);
            SetCameraTarget(Vector3D.Zero, 1000);
        }

        /// <summary>
        /// Adds a new object to the list of render objects to render.
        /// The <paramref name="id"/> is the ID of the system object while the
        /// <paramref name="obj"/> is a render object representing the system object.
        /// Only one render object can be added for a given system object.
        /// </summary>
        /// <param name="id">Id of the system object</param>
        /// <param name="obj">Render object to visualize the system object</param>
        public void AddObject(Guid id, MyAbstractStarSystemDesignerRenderObject obj)
        {
            if (m_systemRenderObjects.ContainsKey(id)) return;
            m_systemRenderObjects.Add(id, obj);
        }

        /// <summary>
        /// Removes an object from the list of render objects to render.
        /// The <paramref name="id"/> is the ID of the system object to remove
        /// </summary>
        /// <param name="id">Id of the system object</param>
        public void RemoveObject(Guid id)
        {
            if (!m_systemRenderObjects.ContainsKey(id)) return;
            m_systemRenderObjects.Remove(id);
        }

        /// <summary>
        /// Updates an existing object in the list of render objects to render.
        /// The <paramref name="id"/> is the ID of the system object while the
        /// </summary>
        /// <param name="id">Id of the system object</param>
        /// <param name="obj">Render object to visualize the system object</param>
        public void UpdateRenderObject(Guid id, MyAbstractStarSystemDesignerRenderObject obj)
        {
            if (!m_systemRenderObjects.ContainsKey(id)) return;
            m_systemRenderObjects[id] = obj;
        }

        /// <summary>
        /// Clears all system objects from the render list.
        /// </summary>
        public void ClearRenderList()
        {
            m_systemRenderObjects.Clear();
        }

        /// <summary>
        /// Sets the given object as the camera target.
        /// </summary>
        /// <param name="distance">Distance to the target</param>
        /// <param name="targetPos">Target position for the camera to look at</param>
        public void SetCameraTarget(Vector3D targetPos, float distance)
        {
            m_targetPos = targetPos;
            MySpectatorCameraController.Static.Position = targetPos + (new Vector3D(0.2f * distance, 0.6f * distance, 0.2f * distance));
            MySpectatorCameraController.Static.Target = targetPos;
        }

        /// <summary>
        /// Changes the camera distance to the current target.
        /// </summary>
        /// <param name="newDistance">The new distance to the target</param>
        public void ChangeCameraDistance(float newDistance)
        {
            MySpectatorCameraController.Static.Position = m_targetPos + (new Vector3D(0.2f * newDistance, 0.6f * newDistance, 0.2f * newDistance));
            MySpectatorCameraController.Static.Target = m_targetPos;
        }

        public void Draw()
        {
            foreach(var entry in m_systemRenderObjects)
            {
                entry.Value.Draw();
            }
        }
    }
}
