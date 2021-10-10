using Sandbox.Engine.Multiplayer;
using Sandbox.Game.Gui;
using Sandbox.Game.World;
using Sandbox.Graphics.GUI;
using SEWorldGenPlugin.Generator.AsteroidObjects.AsteroidRing;
using SEWorldGenPlugin.Generator.AsteroidObjectShapes;
using SEWorldGenPlugin.GUI;
using SEWorldGenPlugin.GUI.AdminMenu;
using SEWorldGenPlugin.GUI.AdminMenu.SubMenus.StarSystemDesigner;
using SEWorldGenPlugin.GUI.Controls;
using SEWorldGenPlugin.ObjectBuilders;
using SEWorldGenPlugin.Session;
using SEWorldGenPlugin.Utilities;
using System;
using VRageMath;

namespace SEWorldGenPlugin.Generator.AsteroidObjects.AsteroidCluster
{
    /// <summary>
    /// Star system designer menu to create asteroid clusters.
    /// </summary>
    public class MyStarSystemDesigneAsteroidClusterMenu : MyStarSystemDesignerOrbitMenu
    {
        /// <summary>
        /// Data associated with the edited instance
        /// </summary>
        public MyAsteroidClusterData Data
        {
            get;
            private set;
        }

        /// <summary>
        /// The slider used to set the size of the cluster
        /// </summary>
        private MyGuiControlClickableSlider m_sizeSlider;

        /// <summary>
        /// Button used to teleport to the ring
        /// </summary>
        private MyGuiControlButton m_teleportButton;

        public MyStarSystemDesigneAsteroidClusterMenu(MySystemAsteroids instance, MyAsteroidClusterData data) : base(instance)
        {
            Data = data;
        }

        public override void RecreateControls(MyGuiControlParentTableLayout controlTable, float maxWidth, bool isEditing = false)
        {
            base.RecreateControls(controlTable, maxWidth, isEditing);

            var genSettings = MySettingsSession.Static.Settings.GeneratorSettings;

            m_sizeSlider = new MyGuiControlClickableSlider(null, 0, Math.Max((int)genSettings.MinMaxOrbitDistance.Min / 20, genSettings.PlanetSettings.PlanetSizeCap / 2) / 1000, maxWidth - 0.1f, (float)Data.Size / 1000f, labelSuffix: " km", showLabel: true);
            m_sizeSlider.ValueChanged += delegate (MyGuiControlSlider s)
            {
                Data.Size = s.Value * 1000f;
                ChangedObject();
            };

            controlTable.AddTableRow(m_sizeSlider);

            m_teleportButton = MyPluginGuiHelper.CreateDebugButton("Teleport to cluster", OnTeleport, true, "Teleports you into the cluster");
            m_teleportButton.Enabled = isEditing;
            m_teleportButton.Size = new Vector2(maxWidth, m_teleportButton.Size.Y);

            controlTable.AddTableRow(m_teleportButton);
        }

        /// <summary>
        /// Teleports the player into the ring
        /// </summary>
        /// <param name="btn"></param>
        private void OnTeleport(MyGuiControlButton btn)
        {
            MyPluginLog.Debug("Teleporting player to " + m_object.DisplayName);

            if (MySession.Static.CameraController != MySession.Static.LocalCharacter || true)
            {
                if (MyAsteroidObjectsManager.Static.AsteroidObjectProviders.TryGetValue((m_object as MySystemAsteroids).AsteroidTypeName, out var prov))
                {
                    IMyAsteroidObjectShape shape = prov.GetAsteroidObjectShape(m_object as MySystemAsteroids);
                    if (shape == null)
                    {
                        MyPluginGuiHelper.DisplayError("Cant teleport to the asteroid cluster. It does not exist", "Error");
                        return;
                    }

                    MyAdminMenuExtension.Static.CloseScreenNow();

                    MyMultiplayer.TeleportControlledEntity(shape.GetPointInShape());
                    MyGuiScreenGamePlay.SetCameraController();
                }
                else
                {
                    MyPluginGuiHelper.DisplayError("Unknown asteroid type", "Error");
                    return;
                }
            }
        }
    }
}
