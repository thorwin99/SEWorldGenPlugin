﻿using Sandbox.Engine.Utils;
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
        OBJECT_SYSTEM,
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

            if(m_systemRenderObjects.ContainsKey(m_focusedObject))
                m_systemRenderObjects[m_focusedObject].SetFocus(false);

            m_focusedObject = obj;

            m_systemRenderObjects[obj].SetFocus(true);

            var renderer = m_systemRenderObjects[obj];
            double renderSize = 0;

            if (FocusZoom == ZoomLevel.ORBIT)
            {
                MySystemObject parent = MyStarSystemGenerator.Static.StarSystem.GetById(renderer.RenderObject.ParentId);

                if(parent != null)
                {
                    renderSize = CalculateObjectSystemSize(m_systemRenderObjects[parent.Id]);
                    SetCameraTarget(parent.CenterPosition, renderSize * 2f);
                    return;
                }
                else
                {
                    renderSize = CalculateObjectSystemSize(renderer);

                    SetCameraTarget(renderer.RenderObject.CenterPosition, renderSize * 2.5f);
                }
            }
            else if (FocusZoom == ZoomLevel.OBJECT_SYSTEM)
            {
                renderSize = CalculateObjectSystemSize(renderer);

                SetCameraTarget(renderer.RenderObject.CenterPosition, renderSize * 2f);
            }
            else
            {
                renderSize = renderer.GetObjectSize();

                SetCameraTarget(renderer.RenderObject.CenterPosition, renderSize * 1.5f);
            }
        }

        /// <summary>
        /// Calculates the size of the object system
        /// </summary>
        private double CalculateObjectSystemSize(MyAbstractStarSystemDesignerRenderObject renderObject)
        {
            double radius = 0;

            foreach(var child in renderObject.RenderObject.ChildObjects)
            {
                double childRad = Vector3D.Distance(renderObject.RenderObject.CenterPosition, child.CenterPosition);

                if (m_systemRenderObjects.ContainsKey(child.Id))
                {
                    childRad += m_systemRenderObjects[child.Id].GetObjectSize();
                }

                radius = Math.Max(childRad, radius);
            }

            radius += renderObject.GetObjectSize();

            return radius;
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
            MySpectatorCameraController.Static.Position = targetPos + (new Vector3D(0.4f, 0, 0.6f)) * distance;
            MySpectatorCameraController.Static.SetTarget(targetPos, new Vector3D(-0.6f, 0, -0.4f));

            if(FocusZoom == ZoomLevel.ORBIT)
                MySpectatorCameraController.Static.Position += Vector3D.UnitX * distance * 0.2f;
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
