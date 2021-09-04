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
    /// An enum indicating which zoom level the star system designer currently is on.
    /// </summary>
    public enum ZoomLevel
    {
        ORBIT,
        OBJECT
    }

    /// <summary>
    /// This class is used to render the star system in the star system designer and controls
    /// the camera placement of the spectator camera for the star system designer
    /// </summary>
    public class MyStarSystemDesignerRenderer : IRenderObject
    {
        /// <summary>
        /// A dictionary of all render objects for the system objects
        /// </summary>
        private Dictionary<Guid, MyAbstractStarSystemDesignerRenderObject> m_systemRenderObjects;

        /// <summary>
        /// The target position of the camera to look at
        /// </summary>
        private Vector3D m_targetPos;

        /// <summary>
        /// The id of the currently focused object
        /// </summary>
        private Guid m_focusedObject;

        /// <summary>
        /// The current zoom level of the spectator cam
        /// </summary>
        public ZoomLevel FocusZoom
        {
            get;
            private set;
        }

        public MyStarSystemDesignerRenderer()
        {
            m_systemRenderObjects = new Dictionary<Guid, MyAbstractStarSystemDesignerRenderObject>();
            MySession.Static.SetCameraController(MyCameraControllerEnum.Spectator);
            SetCameraTarget(Vector3D.Zero, 1000);
        }

        public void FocusObject(Guid obj)
        {
            if (!m_systemRenderObjects.ContainsKey(obj)) return;
            m_focusedObject = obj;
            var renderer = m_systemRenderObjects[obj];
            double renderSize = renderer.GetObjectRenderSize(FocusZoom);
            SetCameraTarget(renderer.RenderObject.CenterPosition, renderSize * 1.5f);
        }

        /// <summary>
        /// Zooms onto the current selected object
        /// </summary>
        public void ZoomInOnObject()
        {
            if(FocusZoom != ZoomLevel.OBJECT)
            {
                FocusZoom++;
                FocusObject(m_focusedObject);
            }
        }

        /// <summary>
        /// Zooms out of the current selected object
        /// </summary>
        public void ZoomOutOfObject()
        {
            if (FocusZoom != ZoomLevel.ORBIT)
            {
                FocusZoom--;
                FocusObject(m_focusedObject);
            }
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
        private void SetCameraTarget(Vector3D targetPos, double distance)
        {
            m_targetPos = targetPos;
            MySpectatorCameraController.Static.Position = targetPos + Vector3D.UnitZ * distance;
            MySpectatorCameraController.Static.SetTarget(targetPos, Vector3D.UnitX);
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
