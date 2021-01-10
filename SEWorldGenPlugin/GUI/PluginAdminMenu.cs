using Sandbox.Definitions;
using Sandbox.Engine.Multiplayer;
using Sandbox.Engine.Utils;
using Sandbox.Game.Entities;
using Sandbox.Game.Gui;
using Sandbox.Game.Multiplayer;
using Sandbox.Game.World;
using Sandbox.Graphics.GUI;
using SEWorldGenPlugin.Draw;
using SEWorldGenPlugin.Generator;
using SEWorldGenPlugin.Generator.Asteroids;
using SEWorldGenPlugin.GUI.Controls;
using SEWorldGenPlugin.ObjectBuilders;
using SEWorldGenPlugin.Session;
using SEWorldGenPlugin.Utilities;
using System;
using System.Collections.Generic;
using System.Text;
using VRage.Game;
using VRage.Game.Entity;
using VRage.Game.SessionComponents;
using VRage.Library.Utils;
using VRage.Utils;
using VRageMath;

namespace SEWorldGenPlugin.GUI
{
    /// <summary>
    /// Replaces the vanilla admin menu to add plugin specific menus to it
    /// </summary>
    [PreloadRequired]
    public class PluginAdminMenu : MyGuiScreenAdminMenu
    {
        private static readonly Vector2 SCREEN_SIZE = new Vector2(0.4f, 1.2f);
        private static readonly float HIDDEN_PART_RIGHT = 0.04f;

        private static bool SNAP_CAMERA_TO_PLANET = true;

        private long m_currentKey;
        private long m_attachedEntity;

        //Elements for Ring Menu
        private MyGuiControlListbox m_planetListBox;
        private MyGuiControlLabel m_ringDistanceValue;
        private MyGuiControlLabel m_ringAngleXValue;
        private MyGuiControlLabel m_ringAngleYValue;
        private MyGuiControlLabel m_ringAngleZValue;
        private MyGuiControlLabel m_ringRoidSizeValue;
        private MyGuiControlLabel m_ringRoidSizeMaxValue;
        private MyGuiControlLabel m_ringWidthValue;

        private MyGuiControlClickableSlider m_ringDistanceSlider;
        private MyGuiControlClickableSlider m_ringAngleXSlider;
        private MyGuiControlClickableSlider m_ringAngleYSlider;
        private MyGuiControlClickableSlider m_ringAngleZSlider;
        private MyGuiControlClickableSlider m_ringRoidSizeSlider;
        private MyGuiControlClickableSlider m_ringRoidSizeMaxSlider;
        private MyGuiControlClickableSlider m_ringWidthSlider;

        private MyGuiControlButton m_addRingButton;
        private MyGuiControlButton m_removeRingButton;
        private MyGuiControlButton m_teleportToRingButton;

        private MyPlanetItem m_selectedPlanet;
        private bool m_newPlanet;

        //Elements for Planet Menu
        private MyGuiControlListbox m_planetDefListBox;
        private MyGuiControlLabel m_planetSizeValue;
        private MyGuiControlClickableSlider m_planetSizeSlider;
        private MyGuiControlButton m_spawnPlanetButton;

        private MyPlanetGeneratorDefinition m_selectedDefinition;

        private bool m_pluginInstalled;

        /// <summary>
        /// Initializes a new instance for the plugi nadmin menu
        /// </summary>
        public PluginAdminMenu() : base()
        {
            m_currentKey = 0L;
            m_pluginInstalled = false;
        }

        /// <summary>
        /// Rebuilds the UI of the menu, by replacing the combo box at the top, with
        /// a new one with new selectable options, and checks, if a plugin menu should be shown and when yes,
        /// which one, otherwise shows the corresponding vanilla menu
        /// </summary>
        /// <param name="constructor">Whether this is run from constructor</param>
        public override void RecreateControls(bool constructor)
        {
            base.RecreateControls(constructor);

            MyGuiControlCombobox modeCombo = GetCombo();

            int oldCount = modeCombo.GetItemsCount();

            if (MySession.Static.IsUserSpaceMaster(Sync.MyId) && MySession.Static.IsUserAdmin(Sync.MyId))
            {
                modeCombo.AddItem(oldCount, "SEWorldGenPlugin - Rings");
                modeCombo.AddItem(oldCount + 1, "SEWorldGenPlugin - Planets");
            }

            MyGuiControlCombobox newCombo = AddCombo();

            int count = modeCombo.GetItemsCount();

            for (int i = 0; i < count; i++)
            {
                var item = modeCombo.GetItemByIndex(i);
                newCombo.AddItem(item.Key, item.Value);
            }

            newCombo.Position = modeCombo.Position;
            newCombo.SelectItemByKey(m_currentKey);
            Controls[Controls.IndexOf(modeCombo)] = newCombo;
            Controls.Remove(modeCombo);

            newCombo.ItemSelected += delegate
            {
                m_currentKey = newCombo.GetSelectedKey();
                if (newCombo.GetSelectedKey() >= oldCount)
                {
                    RecreateControls(false);
                }
                else
                {
                    m_attachedEntity = 0L;
                    m_selectedPlanet = null;
                    modeCombo.SelectItemByKey(newCombo.GetSelectedKey());
                    RecreateControls(false);
                }
            };
            if(newCombo.GetSelectedKey() == oldCount)
            {
                ClearControls();
                CheckBuildPluginControls(BuildRingMenu);
            }
            else if(newCombo.GetSelectedKey() == oldCount + 1)
            {
                ClearControls();
                CheckBuildPluginControls(BuildPlanetMenu);
            }
        }

        /// <summary>
        /// Checks if the plugin is installed on the server and calls the
        /// creator callback, to build the gui.
        /// </summary>
        /// <param name="creator">GUI creator callback</param>
        private void CheckBuildPluginControls(Action creator)
        {
            BuildPluginMenuHeader();
            if (!m_pluginInstalled)
            {
                MyNetUtil.PingServer(delegate
                {
                    m_pluginInstalled = true;
                    creator?.Invoke();
                });
            }
            else {
                creator?.Invoke();
            }
        }

        /// <summary>
        /// Builds the header for plugin specific menus,
        /// which essentially only shows, if the plugin is disabled on the server.
        /// </summary>
        private void BuildPluginMenuHeader()
        {
            Vector2 controlPadding = new Vector2(0.02f, 0.02f);
            float num2 = (SCREEN_SIZE.Y - 1f) / 2f;

            m_currentPosition = -m_size.Value / 2f + controlPadding;
            m_currentPosition.Y += num2 + MyGuiConstants.SCREEN_CAPTION_DELTA_Y + controlPadding.Y - 0.012f + 0.095f;
            m_currentPosition.X += 0.018f;

            if (!m_pluginInstalled)
            {
                MyGuiControlLabel errorLabel = new MyGuiControlLabel
                {
                    Position = new Vector2(0f, 0f),
                    OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER,
                    Text = "Plugin not installed or enabled on server",
                    ColorMask = Color.Red.ToVector4(),
                    TextScale = MyGuiConstants.DEFAULT_TEXT_SCALE

                };

                AddControl(errorLabel);

                return;
            }
        }

        /// <summary>
        /// Builds the menu for adding plugin planets. It creates a
        /// list of all available planets to place and sliders to configure it.
        /// </summary>
        private void BuildPlanetMenu()
        {
            ClearControls();
            Vector2 controlPadding = new Vector2(0.02f, 0.02f);
            float num = SCREEN_SIZE.X - HIDDEN_PART_RIGHT - controlPadding.X * 2f;
            float num2 = (SCREEN_SIZE.Y - 1f) / 2f;

            MyGuiControlLabel listBoxLabel = new MyGuiControlLabel
            {
                Position = new Vector2(-0.153f, -0.334f),
                OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP,
                Text = "List of Planet definitions"
            };

            MyGuiControlPanel listBoxLabelBg = new MyGuiControlPanel(new Vector2(listBoxLabel.PositionX - 0.0085f, listBoxLabel.Position.Y - 0.005f), new Vector2(0.2865f, 0.035f), null, null, null, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP)
            {
                BackgroundTexture = MyGuiConstants.TEXTURE_RECTANGLE_DARK_BORDER
            };

            Controls.Add(listBoxLabelBg);
            Controls.Add(listBoxLabel);

            m_currentPosition.Y += 0.020f;

            m_planetDefListBox = new MyGuiControlListbox(Vector2.Zero, VRage.Game.MyGuiControlListboxStyleEnum.Blueprints);
            m_planetDefListBox.Size = new Vector2(num, 0f);
            m_planetDefListBox.Enabled = true;
            m_planetDefListBox.VisibleRowsCount = 8;
            m_planetDefListBox.Position = m_planetDefListBox.Size / 2f + m_currentPosition;
            m_planetDefListBox.ItemClicked += PlanetDefListItemClicked;
            m_planetDefListBox.MultiSelect = false;

            MyGuiControlSeparatorList separator = new MyGuiControlSeparatorList();
            separator.AddHorizontal(new Vector2(0f, 0f) - new Vector2(m_size.Value.X * 0.83f / 2f, -0.00f), m_size.Value.X * 0.73f);
            Controls.Add(separator);

            m_currentPosition = m_planetDefListBox.GetPositionAbsoluteBottomLeft();
            m_currentPosition.Y += 0.045f;

            MyGuiControlLabel planetSizeLabel = new MyGuiControlLabel
            {
                Position = m_currentPosition + new Vector2(0.001f, 0f),
                OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP,
                Text = "PlanetSize"
            };
            Controls.Add(planetSizeLabel);

            m_planetSizeValue = new MyGuiControlLabel
            {
                Position = m_currentPosition + new Vector2(0.285f, 0f),
                OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP,
                Text = "0"
            };
            Controls.Add(m_planetSizeValue);

            m_currentPosition.Y += 0.025f;

            m_planetSizeSlider = new MyGuiControlClickableSlider(m_currentPosition + new Vector2(0.001f, 0f), 120f, (float)MySettingsSession.Static.Settings.GeneratorSettings.PlanetSettings.PlanetSizeCap, intValue: true, toolTip: MyPluginTexts.TOOLTIPS.ADMIN_PLANET_SIZE);
            m_planetSizeSlider.Size = new Vector2(0.285f, 1f);
            m_planetSizeSlider.DefaultValue = 1200f;
            m_planetSizeSlider.Value = m_planetSizeSlider.DefaultValue.Value;
            m_planetSizeSlider.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            m_planetSizeSlider.ValueChanged = (Action<MyGuiControlSlider>)Delegate.Combine(m_planetSizeSlider.ValueChanged, (Action<MyGuiControlSlider>)delegate (MyGuiControlSlider s)
            {
                m_planetSizeValue.Text = s.Value.ToString();
            });

            m_planetSizeValue.Text = m_planetSizeSlider.Value.ToString();

            Controls.Add(m_planetSizeSlider);

            m_currentPosition.Y += 0.055f + 0.035f;

            m_spawnPlanetButton = CreateDebugButton(0.284f, "Spawn planet", OnSpawnPlanetButton, true, MyPluginTexts.TOOLTIPS.ADMIN_ADD_RING_BUTTON);
            m_spawnPlanetButton.Enabled = false;

            Controls.Add(m_planetDefListBox);

            LoadPlanetDefinitions();
        }

        /// <summary>
        /// Builds the menu to add asteroid rings to planets. It contains a list of all planets
        /// and shows the properties of the ring on a planet, and if none is present, you can edit these
        /// properties with sliders and add it to the planet.
        /// </summary>
        private void BuildRingMenu()
        {
            ClearControls();

            Vector2 controlPadding = new Vector2(0.02f, 0.02f);
            float num = SCREEN_SIZE.X - HIDDEN_PART_RIGHT - controlPadding.X * 2f;

            MyGuiControlLabel listBoxLabel = new MyGuiControlLabel
            {
                Position = new Vector2(-0.153f, -0.334f),
                OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP,
                Text = "List of Planets"
            };

            MyGuiControlPanel listBoxLabelBg = new MyGuiControlPanel(new Vector2(listBoxLabel.PositionX - 0.0085f, listBoxLabel.Position.Y - 0.005f), new Vector2(0.2865f, 0.035f), null, null, null, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP)
            {
                BackgroundTexture = MyGuiConstants.TEXTURE_RECTANGLE_DARK_BORDER
            };

            Controls.Add(listBoxLabelBg);
            Controls.Add(listBoxLabel);

            m_currentPosition.Y += 0.020f;

            m_planetListBox = new MyGuiControlListbox(Vector2.Zero, VRage.Game.MyGuiControlListboxStyleEnum.Blueprints);
            m_planetListBox.Size = new Vector2(num, 0f);
            m_planetListBox.Enabled = true;
            m_planetListBox.VisibleRowsCount = 7;
            m_planetListBox.Position = m_planetListBox.Size / 2f + m_currentPosition;
            m_planetListBox.ItemClicked += PlanetListItemClicked;
            m_planetListBox.MultiSelect = false;

            m_currentPosition = m_planetListBox.GetPositionAbsoluteBottomLeft();
            m_currentPosition.Y += 0.025f;

            MyGuiControlLabel snapLabel = new MyGuiControlLabel(null, null, "Snap camera to planet");
            snapLabel.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER;
            snapLabel.Position = m_currentPosition;

            Controls.Add(snapLabel);

            MyGuiControlCheckbox snapCamera = new MyGuiControlCheckbox();
            snapCamera.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER;
            snapCamera.IsCheckedChanged = snapCamera.IsCheckedChanged = (Action<MyGuiControlCheckbox>)Delegate.Combine(snapCamera.IsCheckedChanged, (Action<MyGuiControlCheckbox>)delegate (MyGuiControlCheckbox s)
            {
                SNAP_CAMERA_TO_PLANET = snapCamera.IsChecked;
            });

            snapCamera.IsChecked = SNAP_CAMERA_TO_PLANET;
            snapCamera.Position = snapLabel.Position + new Vector2(snapLabel.Size.X, 0f);
            Controls.Add(snapCamera);

            m_currentPosition.Y += 0.045f;

            MyGuiControlSeparatorList separator = new MyGuiControlSeparatorList();
            separator.AddHorizontal(new Vector2(0f, 0f) - new Vector2(m_size.Value.X * 0.83f / 2f, -0.09f), m_size.Value.X * 0.73f);
            Controls.Add(separator);

            MyGuiControlParent myGuiControlParent = new MyGuiControlParent
            {
                OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP,
                Position = Vector2.Zero,
                Size = new Vector2(0.32f, 0.56f)
            };

            MyGuiControlScrollablePanel m_optionsGroup = new MyGuiControlScrollablePanel(myGuiControlParent)
            {
                OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP,
                Position = m_currentPosition,
                Size = new Vector2(0.32f, 0.32f)
            };
            m_optionsGroup.ScrollbarVEnabled = true;
            m_optionsGroup.ScrollBarOffset = new Vector2(-0.01f, 0f);
            Controls.Add(m_optionsGroup);

            Vector2 vector = -myGuiControlParent.Size * 0.5f;

            MyGuiControlLabel ringDistLabel = new MyGuiControlLabel
            {
                Position = vector + new Vector2(0.001f, 0f),
                OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP,
                Text = "Ring distance"
            };
            myGuiControlParent.Controls.Add(ringDistLabel);

            m_ringDistanceValue = new MyGuiControlLabel
            {
                Position = vector + new Vector2(0.285f, 0f),
                OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP,
                Text = "0"
            };
            myGuiControlParent.Controls.Add(m_ringDistanceValue);

            vector.Y += 0.025f;

            m_ringDistanceSlider = new MyGuiControlClickableSlider(vector + new Vector2(0.001f, 0f), 5000f, 1000000f, intValue: true, toolTip: MyPluginTexts.TOOLTIPS.ADMIN_RING_DISTANCE, showLabel: false);//Make dynamic
            m_ringDistanceSlider.Size = new Vector2(0.285f, 1f);
            m_ringDistanceSlider.DefaultValue = 100000;
            m_ringDistanceSlider.Value = m_ringDistanceSlider.DefaultValue.Value;
            m_ringDistanceSlider.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            m_ringDistanceSlider.ValueChanged = (Action<MyGuiControlSlider>)Delegate.Combine(m_ringDistanceSlider.ValueChanged, (Action<MyGuiControlSlider>)delegate (MyGuiControlSlider s)
            {
                UpdateRingVisual();
                m_ringDistanceValue.Text = s.Value.ToString();
            });

            m_ringDistanceValue.Text = m_ringDistanceSlider.Value.ToString();

            myGuiControlParent.Controls.Add(m_ringDistanceSlider);

            vector.Y += 0.055f;

            MyGuiControlLabel ringWidthLabel = new MyGuiControlLabel
            {
                Position = vector + new Vector2(0.001f, 0f),
                OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP,
                Text = "Ring width"
            };
            myGuiControlParent.Controls.Add(ringWidthLabel);

            m_ringWidthValue = new MyGuiControlLabel
            {
                Position = vector + new Vector2(0.285f, 0f),
                OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP,
                Text = "0"
            };
            myGuiControlParent.Controls.Add(m_ringWidthValue);

            vector.Y += 0.025f;

            m_ringWidthSlider = new MyGuiControlClickableSlider(vector + new Vector2(0.001f, 0f), MySettingsSession.Static.Settings.GeneratorSettings.PlanetSettings.RingSettings.MinPlanetRingWidth, MySettingsSession.Static.Settings.GeneratorSettings.PlanetSettings.RingSettings.MaxPlanetRingWidth, intValue: true, toolTip: MyPluginTexts.TOOLTIPS.ADMIN_RING_WIDTH, showLabel: false);
            m_ringWidthSlider.Size = new Vector2(0.285f, 1f);
            m_ringWidthSlider.DefaultValue = (MySettingsSession.Static.Settings.GeneratorSettings.PlanetSettings.RingSettings.MinPlanetRingWidth + MySettingsSession.Static.Settings.GeneratorSettings.PlanetSettings.RingSettings.MaxPlanetRingWidth) / 2;
            m_ringWidthSlider.Value = m_ringWidthSlider.DefaultValue.Value;
            m_ringWidthSlider.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            m_ringWidthSlider.ValueChanged = (Action<MyGuiControlSlider>)Delegate.Combine(m_ringWidthSlider.ValueChanged, (Action<MyGuiControlSlider>)delegate (MyGuiControlSlider s)
            {
                UpdateRingVisual();
                m_ringWidthValue.Text = s.Value.ToString();
            });

            m_ringWidthValue.Text = m_ringWidthSlider.Value.ToString();

            myGuiControlParent.Controls.Add(m_ringWidthSlider);

            vector.Y += 0.055f;

            MyGuiControlLabel ringAngleXLabel = new MyGuiControlLabel
            {
                Position = vector + new Vector2(0.001f, 0f),
                OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP,
                Text = "Ring angle X"
            };
            myGuiControlParent.Controls.Add(ringAngleXLabel);

            m_ringAngleXValue = new MyGuiControlLabel
            {
                Position = vector + new Vector2(0.285f, 0f),
                OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP,
                Text = "0.00"
            };
            myGuiControlParent.Controls.Add(m_ringAngleXValue);

            vector.Y += 0.025f;

            m_ringAngleXSlider = new MyGuiControlClickableSlider(vector + new Vector2(0.001f, 0f), -90, 90, intValue: false, toolTip: MyPluginTexts.TOOLTIPS.ADMIN_RING_ANGLE);
            m_ringAngleXSlider.Size = new Vector2(0.285f, 1f);
            m_ringAngleXSlider.DefaultValue = 0;
            m_ringAngleXSlider.Value = m_ringAngleXSlider.DefaultValue.Value;
            m_ringAngleXSlider.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            m_ringAngleXSlider.ValueChanged = (Action<MyGuiControlSlider>)Delegate.Combine(m_ringAngleXSlider.ValueChanged, (Action<MyGuiControlSlider>)delegate (MyGuiControlSlider s)
            {
                UpdateRingVisual();
                m_ringAngleXValue.Text = String.Format("{0:0.00}", s.Value);
            });

            m_ringAngleXValue.Text = String.Format("{0:0.00}", m_ringAngleXSlider.Value);

            myGuiControlParent.Controls.Add(m_ringAngleXSlider);

            vector.Y += 0.055f;

            MyGuiControlLabel ringAngleYLabel = new MyGuiControlLabel
            {
                Position = vector + new Vector2(0.001f, 0f),
                OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP,
                Text = "Ring angle Y"
            };
            myGuiControlParent.Controls.Add(ringAngleYLabel);

            m_ringAngleYValue = new MyGuiControlLabel
            {
                Position = vector + new Vector2(0.285f, 0f),
                OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP,
                Text = "0.00"
            };
            myGuiControlParent.Controls.Add(m_ringAngleYValue);

            vector.Y += 0.025f;

            m_ringAngleYSlider = new MyGuiControlClickableSlider(vector + new Vector2(0.001f, 0f), -90, 90, intValue: false, toolTip: MyPluginTexts.TOOLTIPS.ADMIN_RING_ANGLE);
            m_ringAngleYSlider.Size = new Vector2(0.285f, 1f);
            m_ringAngleYSlider.DefaultValue = 0;
            m_ringAngleYSlider.Value = m_ringAngleYSlider.DefaultValue.Value;
            m_ringAngleYSlider.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            m_ringAngleYSlider.ValueChanged = (Action<MyGuiControlSlider>)Delegate.Combine(m_ringAngleYSlider.ValueChanged, (Action<MyGuiControlSlider>)delegate (MyGuiControlSlider s)
            {
                UpdateRingVisual();
                m_ringAngleYValue.Text = String.Format("{0:0.00}", s.Value);
            });

            m_ringAngleYValue.Text = String.Format("{0:0.00}", m_ringAngleYSlider.Value);

            myGuiControlParent.Controls.Add(m_ringAngleYSlider);

            vector.Y += 0.055f;

            MyGuiControlLabel ringAngleZLabel = new MyGuiControlLabel
            {
                Position = vector + new Vector2(0.001f, 0f),
                OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP,
                Text = "Ring angle Z"
            };
            myGuiControlParent.Controls.Add(ringAngleZLabel);

            m_ringAngleZValue = new MyGuiControlLabel
            {
                Position = vector + new Vector2(0.285f, 0f),
                OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP,
                Text = "0.00"
            };
            myGuiControlParent.Controls.Add(m_ringAngleZValue);

            vector.Y += 0.025f;

            m_ringAngleZSlider = new MyGuiControlClickableSlider(vector + new Vector2(0.001f, 0f), -90, 90, intValue: false, toolTip: MyPluginTexts.TOOLTIPS.ADMIN_RING_ANGLE);
            m_ringAngleZSlider.Size = new Vector2(0.285f, 1f);
            m_ringAngleZSlider.DefaultValue = 0;
            m_ringAngleZSlider.Value = m_ringAngleZSlider.DefaultValue.Value;
            m_ringAngleZSlider.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            m_ringAngleZSlider.ValueChanged = (Action<MyGuiControlSlider>)Delegate.Combine(m_ringAngleZSlider.ValueChanged, (Action<MyGuiControlSlider>)delegate (MyGuiControlSlider s)
            {
                UpdateRingVisual();
                m_ringAngleZValue.Text = String.Format("{0:0.00}", s.Value);
            });

            m_ringAngleZValue.Text = String.Format("{0:0.00}", m_ringAngleZSlider.Value);

            myGuiControlParent.Controls.Add(m_ringAngleZSlider);

            vector.Y += 0.055f;

            MyGuiControlLabel ringRoidSizeLabel = new MyGuiControlLabel
            {
                Position = vector + new Vector2(0.001f, 0f),
                OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP,
                Text = "Asteroid min size"
            };
            myGuiControlParent.Controls.Add(ringRoidSizeLabel);

            m_ringRoidSizeValue = new MyGuiControlLabel
            {
                Position = vector + new Vector2(0.285f, 0f),
                OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP,
                Text = "0"
            };
            myGuiControlParent.Controls.Add(m_ringRoidSizeValue);

            vector.Y += 0.025f;

            m_ringRoidSizeSlider = new MyGuiControlClickableSlider(vector + new Vector2(0.001f, 0f), 128, 1024, intValue: true, toolTip: MyPluginTexts.TOOLTIPS.ADMIN_RING_ROID_SIZE);
            m_ringRoidSizeSlider.Size = new Vector2(0.285f, 1f);
            m_ringRoidSizeSlider.DefaultValue = 128;
            m_ringRoidSizeSlider.Value = m_ringRoidSizeSlider.DefaultValue.Value;
            m_ringRoidSizeSlider.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            m_ringRoidSizeSlider.ValueChanged = (Action<MyGuiControlSlider>)Delegate.Combine(m_ringRoidSizeSlider.ValueChanged, (Action<MyGuiControlSlider>)delegate (MyGuiControlSlider s)
            {
                m_ringRoidSizeValue.Text = s.Value.ToString();
                if(s.Value > m_ringRoidSizeMaxSlider.Value)
                {
                    m_ringRoidSizeSlider.Value = m_ringRoidSizeMaxSlider.Value;
                }
            });

            m_ringRoidSizeValue.Text = m_ringRoidSizeSlider.Value.ToString();

            myGuiControlParent.Controls.Add(m_ringRoidSizeSlider);

            vector.Y += 0.055f;

            MyGuiControlLabel ringRoidSizeMaxLabel = new MyGuiControlLabel
            {
                Position = vector + new Vector2(0.001f, 0f),
                OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP,
                Text = "Asteroid max size"
            };
            myGuiControlParent.Controls.Add(ringRoidSizeMaxLabel);

            m_ringRoidSizeMaxValue = new MyGuiControlLabel
            {
                Position = vector + new Vector2(0.285f, 0f),
                OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP,
                Text = "0"
            };
            myGuiControlParent.Controls.Add(m_ringRoidSizeMaxValue);

            vector.Y += 0.025f;

            m_ringRoidSizeMaxSlider = new MyGuiControlClickableSlider(vector + new Vector2(0.001f, 0f), 128, 1024, intValue: true, toolTip: MyPluginTexts.TOOLTIPS.ADMIN_RING_ROID_SIZE_MAX);
            m_ringRoidSizeMaxSlider.Size = new Vector2(0.285f, 1f);
            m_ringRoidSizeMaxSlider.DefaultValue = 1028;
            m_ringRoidSizeMaxSlider.Value = m_ringRoidSizeSlider.DefaultValue.Value;
            m_ringRoidSizeMaxSlider.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            m_ringRoidSizeMaxSlider.ValueChanged = (Action<MyGuiControlSlider>)Delegate.Combine(m_ringRoidSizeMaxSlider.ValueChanged, (Action<MyGuiControlSlider>)delegate (MyGuiControlSlider s)
            {
                m_ringRoidSizeMaxValue.Text = s.Value.ToString();
                if(s.Value < m_ringRoidSizeSlider.Value)
                {
                    m_ringRoidSizeMaxSlider.Value = m_ringRoidSizeSlider.Value;
                }
            });

            m_ringRoidSizeMaxValue.Text = m_ringRoidSizeMaxSlider.Value.ToString();

            myGuiControlParent.Controls.Add(m_ringRoidSizeMaxSlider);

            m_optionsGroup.RefreshInternals();

            m_currentPosition.Y += m_optionsGroup.Size.Y;

            m_addRingButton = CreateDebugButton(0.284f, "Add ring to planet", OnAddRingToPlanetButton, true, MyPluginTexts.TOOLTIPS.ADMIN_ADD_RING_BUTTON);

            m_currentPosition.Y += 0.003f;

            m_removeRingButton = CreateDebugButton(0.284f, "Remove ring from planet", OnRemoveRingFromPlanetButton, true, MyPluginTexts.TOOLTIPS.ADMIN_REMOVE_RING_BUTTON);

            m_currentPosition.Y += 0.003f;

            m_teleportToRingButton = CreateDebugButton(0.284f, "Teleport to ring", OnTeleportToRingButton, true, MyPluginTexts.TOOLTIPS.ADMIN_TP_RING_BUTTON);
            m_teleportToRingButton.Enabled = false;

            Controls.Add(m_planetListBox);

            m_ringAngleXSlider.Enabled = false;
            m_ringAngleYSlider.Enabled = false;
            m_ringAngleZSlider.Enabled = false;
            m_ringDistanceSlider.Enabled = false;
            m_ringWidthSlider.Enabled = false;
            m_ringRoidSizeSlider.Enabled = false;
            m_ringRoidSizeMaxSlider.Enabled = false;
            m_addRingButton.Enabled = false;
            m_removeRingButton.Enabled = false;
            m_teleportToRingButton.Enabled = false;

            LoadPlanetsInWorld();
        }

        /// <summary>
        /// Updates the visible representation of the asteroid ring on the screen, which is currently viewed or edited
        /// using the ring menu
        /// </summary>
        private void UpdateRingVisual()
        {
            MyPluginDrawSession.Static.RemoveRenderObject(m_selectedPlanet.GetHashCode());

            MyAsteroidRingShape shape = MyAsteroidRingShape.CreateFromRingItem(GenerateRingItem());

            MyPluginDrawSession.Static.AddRenderObject(m_selectedPlanet.GetHashCode(), new RenderHollowCylinder(shape.worldMatrix, (float)shape.radius + shape.width, (float)shape.radius, shape.height, Color.LightGreen.ToVector4()));
        }

        /// <summary>
        /// Callback for button, to teleport the player to the currently selected planets ring
        /// </summary>
        /// <param name="button">Button that got pressed</param>
        private void OnTeleportToRingButton(MyGuiControlButton button)
        {
            if(MySession.Static.CameraController != MySession.Static.LocalCharacter)
            {
                CloseScreen();
                MyAsteroidRingShape shape = MyAsteroidRingShape.CreateFromRingItem(m_selectedPlanet.PlanetRing);

                MyMultiplayer.TeleportControlledEntity(shape.LocationInRing(0));
                m_attachedEntity = 0L;
                m_selectedPlanet = null;
                MyGuiScreenGamePlay.SetCameraController();
                
            }
        }

        /// <summary>
        /// Closes the screen and unloads data
        /// </summary>
        /// <returns>If the screen was closed</returns>
        public override bool CloseScreen(bool isUnloading = false)
        {
            if(m_selectedPlanet != null)
                MyPluginDrawSession.Static.RemoveRenderObject(m_selectedPlanet.GetHashCode());
            return base.CloseScreen(isUnloading);
        }

        /// <summary>
        /// Callback for button, to add a ring to the current selected planet
        /// </summary>
        /// <param name="button">Button that got clicked</param>
        private void OnAddRingToPlanetButton(MyGuiControlButton button)
        {
            if (m_planetListBox.SelectedItems.Count > 0)
            {
                var item = GenerateRingItem();
                if (m_newPlanet)
                {
                    item.Center = m_selectedPlanet.CenterPosition;
                    m_selectedPlanet.PlanetRing = item;
                    SystemGenerator.Static.AddPlanet(m_selectedPlanet);
                }
                else
                {
                    SystemGenerator.Static.AddRingToPlanet(m_selectedPlanet.DisplayName, item, delegate (bool success)
                    {
                        LoadPlanetsInWorld(m_selectedPlanet.DisplayName);
                        PlanetListItemClicked(m_planetListBox);
                    });
                }
                LoadPlanetsInWorld(m_selectedPlanet.DisplayName);
                PlanetListItemClicked(m_planetListBox);
            }
        }

        /// <summary>
        /// Callback for button, to remove an asteroid ring from a planet
        /// </summary>
        /// <param name="button">Button that got clicked</param>
        private void OnRemoveRingFromPlanetButton(MyGuiControlButton button)
        {
            if (m_planetListBox.SelectedItems.Count > 0)
            {
                if (m_newPlanet)
                {
                    m_selectedPlanet.PlanetRing = null;
                    SystemGenerator.Static.AddPlanet(m_selectedPlanet);
                }
                else
                {
                    SystemGenerator.Static.RemoveRingFromPlanet(m_selectedPlanet.DisplayName);
                }
                LoadPlanetsInWorld(m_selectedPlanet.DisplayName);
            }
        }

        /// <summary>
        /// Generates an MyPlanetRingItem with the current slider values in the ring menu
        /// </summary>
        /// <returns>The generated MyPlanetRingItem</returns>
        private MyPlanetRingItem GenerateRingItem()
        {
            if (m_selectedPlanet.PlanetRing != null) return m_selectedPlanet.PlanetRing;
            MyPlanetRingItem item = new MyPlanetRingItem();
            item.Type = LegacySystemObjectType.RING;
            item.DisplayName = "";
            item.AngleDegrees = m_ringAngleZSlider.Value;
            item.AngleDegreesX = m_ringAngleXSlider.Value;
            item.AngleDegreesY = m_ringAngleYSlider.Value;
            item.Radius = m_ringDistanceSlider.Value + m_selectedPlanet.Size / 2f;
            item.Width = (int)m_ringWidthSlider.Value;
            item.Height = item.Width / 10;
            item.RoidSize = (int)m_ringRoidSizeSlider.Value;
            item.RoidSizeMax = (int)m_ringRoidSizeMaxSlider.Value;
            item.Center = m_selectedPlanet.CenterPosition;

            return item;
        }

        /// <summary>
        /// Callback, for when a planet in the planet list is clicked
        /// Loads all slider values, if a ring is present, if not it initializes them
        /// with respective limits to the planet
        /// </summary>
        /// <param name="box">Listbox that got clicked</param>
        private void PlanetListItemClicked(MyGuiControlListbox box)
        {
            if (box.SelectedItems.Count > 0)
            {
                var data = (Tuple<MySystemItem, MyEntityList.MyEntityListInfoItem>)box.SelectedItems[box.SelectedItems.Count - 1].UserData;
                MyEntityList.MyEntityListInfoItem myEntityListInfoItem = data.Item2;
                MyPlanetItem planet = (MyPlanetItem)data.Item1;
                MyPlanet entityPlanet = (MyPlanet)MyEntities.GetEntityById(myEntityListInfoItem.EntityId);

                m_attachedEntity = myEntityListInfoItem.EntityId;
                if (SNAP_CAMERA_TO_PLANET && !TryAttachCamera(myEntityListInfoItem.EntityId))
                {
                    MySession.Static.SetCameraController(MyCameraControllerEnum.Spectator, null, myEntityListInfoItem.Position + Vector3.One * 50f);
                }

                if(m_selectedPlanet != null)
                {
                    MyPluginDrawSession.Static.RemoveRenderObject(m_selectedPlanet.GetHashCode());
                }

                m_selectedPlanet = planet;
                m_newPlanet = false;

                m_ringDistanceSlider.MinValue = entityPlanet.AverageRadius * 2 * 0.75f - entityPlanet.AverageRadius;
                m_ringDistanceSlider.MinValue -= m_ringDistanceSlider.MinValue % 1000;
                m_ringDistanceSlider.MaxValue = entityPlanet.AverageRadius * 2 * 2 - entityPlanet.AverageRadius;
                m_ringDistanceSlider.MaxValue -= m_ringDistanceSlider.MaxValue % 1000;
                m_ringDistanceSlider.Value = entityPlanet.AverageRadius * 2 * 1.25f - entityPlanet.AverageRadius;
                m_ringDistanceSlider.Value -= m_ringDistanceSlider.Value % 1000;

                bool hasRing = m_selectedPlanet.PlanetRing != null;

                m_ringAngleXSlider.Enabled = !hasRing;
                m_ringAngleYSlider.Enabled = !hasRing;
                m_ringAngleZSlider.Enabled = !hasRing;
                m_ringDistanceSlider.Enabled = !hasRing;
                m_ringWidthSlider.Enabled = !hasRing;
                m_ringRoidSizeSlider.Enabled = !hasRing;
                m_ringRoidSizeMaxSlider.Enabled = !hasRing;
                m_addRingButton.Enabled = !hasRing;
                m_removeRingButton.Enabled = hasRing;
                m_teleportToRingButton.Enabled = hasRing;

                if (hasRing)
                {
                    m_ringDistanceSlider.Value = (float)m_selectedPlanet.PlanetRing.Radius - m_selectedPlanet.Size / 2;
                    m_ringWidthSlider.Value = m_selectedPlanet.PlanetRing.Width;
                    m_ringAngleZSlider.Value = m_selectedPlanet.PlanetRing.AngleDegrees;
                    m_ringAngleYSlider.Value = m_selectedPlanet.PlanetRing.AngleDegreesY;
                    m_ringAngleXSlider.Value = m_selectedPlanet.PlanetRing.AngleDegreesX;
                    m_ringRoidSizeSlider.Value = m_selectedPlanet.PlanetRing.RoidSize;
                    m_ringRoidSizeMaxSlider.Value = m_selectedPlanet.PlanetRing.RoidSizeMax;
                }
                else
                {
                    m_ringWidthSlider.Value = m_ringWidthSlider.DefaultValue.Value;
                    m_ringAngleXSlider.Value = m_ringAngleXSlider.DefaultValue.Value;
                    m_ringAngleYSlider.Value = m_ringAngleXSlider.DefaultValue.Value;
                    m_ringAngleZSlider.Value = m_ringAngleXSlider.DefaultValue.Value;
                    m_ringRoidSizeSlider.Value = m_ringRoidSizeSlider.MinValue;
                    m_ringRoidSizeMaxSlider.Value = m_ringRoidSizeMaxSlider.MaxValue;
                    m_ringRoidSizeSlider.Value = m_ringRoidSizeSlider.DefaultValue.Value;
                    m_ringRoidSizeMaxSlider.Value = m_ringRoidSizeMaxSlider.DefaultValue.Value;
                }

                m_ringRoidSizeValue.Text = m_ringRoidSizeSlider.Value.ToString();
                m_ringAngleXValue.Text = String.Format("{0:0.00}", m_ringAngleXSlider.Value);
                m_ringAngleYValue.Text = String.Format("{0:0.00}", m_ringAngleYSlider.Value);
                m_ringAngleZValue.Text = String.Format("{0:0.00}", m_ringAngleZSlider.Value);
                m_ringWidthValue.Text = m_ringWidthSlider.Value.ToString();
                m_ringDistanceValue.Text = m_ringDistanceSlider.Value.ToString();

                UpdateRingVisual();
            }
        }

        /// <summary>
        /// Loads all planets that are in the world
        /// </summary>
        private void LoadPlanetsInWorld(string selectPlanet = null)
        {
            m_planetListBox.Items.Clear();
            List<MyEntityList.MyEntityListInfoItem> planets = MyEntityList.GetEntityList(MyEntityList.MyEntityTypeEnum.Planets);
            foreach(var item in planets)
            {
                string name = item.DisplayName.Substring(0, item.DisplayName.LastIndexOf("-")).Replace("_", " ").Trim();

                SystemGenerator.Static.GetObject(name, delegate (bool success, MySystemItem obj)
                {
                    if (obj.Type == LegacySystemObjectType.MOON) return;
                    if(obj.Type == LegacySystemObjectType.PLANET && success)
                    {
                        m_planetListBox.Items.Add(new MyGuiControlListbox.Item(new StringBuilder(name), null, null, Tuple.Create(obj, item)));
                        if(selectPlanet != null && name.Equals(selectPlanet))
                        {
                            m_planetListBox.SelectSingleItem(m_planetListBox.Items[m_planetListBox.Items.Count - 1]);
                            PlanetListItemClicked(m_planetListBox);
                        }
                    }
                    else
                    {
                        MyPlanet e = MyEntities.GetEntityById(item.EntityId) as MyPlanet;
                        MyPlanetItem p = new MyPlanetItem()
                        {
                            OffsetPosition = e.PositionLeftBottomCorner,
                            DefName = item.DisplayName,
                            DisplayName = item.DisplayName.Replace("_", " "),
                            Generated = true,
                            PlanetMoons = new MyPlanetMoonItem[0],
                            PlanetRing = null,
                            Size = e.MaximumRadius * 2,
                            Type = LegacySystemObjectType.PLANET,
                            CenterPosition = e.PositionComp.GetPosition()
                        };

                        SystemGenerator.Static.AddPlanet(p, delegate (bool s)
                        {
                            if (s)
                            {
                                m_planetListBox.Items.Add(new MyGuiControlListbox.Item(new StringBuilder(name), null, null, Tuple.Create(obj, item)));
                                if (selectPlanet != null && name.Equals(selectPlanet))
                                {
                                    m_planetListBox.SelectSingleItem(m_planetListBox.Items[m_planetListBox.Items.Count - 1]);
                                    PlanetListItemClicked(m_planetListBox);
                                }
                            }
                        });
                    }
                });
            }
        }

        /// <summary>
        /// Loads all available planetary definitions
        /// </summary>
        private void LoadPlanetDefinitions()
        {
            var definitions = SystemGenerator.Static.PlanetDefinitions;
            foreach (var item in definitions)
            {
                string name = item.Id.SubtypeName.ToString();

                m_planetDefListBox.Items.Add(new MyGuiControlListbox.Item(new StringBuilder(name), null, null, item));
            }
        }

        /// <summary>
        /// Callback for click in the Planet definition list.
        /// Selects the respective definition
        /// </summary>
        /// <param name="box">Clicked box</param>
        private void PlanetDefListItemClicked(MyGuiControlListbox box)
        {
            if (box.SelectedItems.Count > 0)
            {                
                m_selectedDefinition = (MyPlanetGeneratorDefinition)box.SelectedItems[box.SelectedItems.Count - 1].UserData;
                m_spawnPlanetButton.Enabled = true;
            }
        }

        /// <summary>
        /// Callback for spawn planet button. Spawns a planet of the current selected definition
        /// and the entered parameters in the planet menu. Closes the menu and switches to planet placement
        /// </summary>
        /// <param name="button">Clicked button</param>
        private void OnSpawnPlanetButton(MyGuiControlButton button)
        {
            if (m_planetDefListBox.SelectedItems.Count > 0)
            {
                float size = m_planetSizeSlider.Value * 1000;
                MyPlanetItem planet = new MyPlanetItem()
                {
                    OffsetPosition = Vector3D.Zero,
                    DefName = m_selectedDefinition.Id.SubtypeId.ToString(),
                    DisplayName = m_selectedDefinition.Id.SubtypeId.ToString() + " " + size + " " + MyRandom.Instance.Next(),
                    Generated = false,
                    PlanetMoons = new MyPlanetMoonItem[0],
                    PlanetRing = null,
                    Size = size,
                    Type = LegacySystemObjectType.PLANET,
                    CenterPosition = Vector3D.Zero
                };

                MyPluginItemsClipboard.Static.Activate(planet, SpawnPlanet, size);

                CloseScreenNow();
            }
        }

        /// <summary>
        /// Spawns a planet in the system
        /// </summary>
        /// <param name="planet">Planet to spawn</param>
        /// <param name="position">Position to spawn at</param>
        private void SpawnPlanet(MySystemItem planet, Vector3D position)
        {
            if(planet.Type == LegacySystemObjectType.PLANET)
            {
                MyPlanetItem p = (MyPlanetItem)planet;
                p.CenterPosition = position;

                SystemGenerator.Static.AddPlanet(p);
            }
        }

        /// <summary>
        /// Clears all controls, except those before the combo box to select the current menu from.
        /// Used to rebuild the whole admin menu
        /// </summary>
        private void ClearControls()
        {
            List<MyGuiControlBase> keep = new List<MyGuiControlBase>();
            foreach(var c in Controls)
            {
                keep.Add(c);
                if (c is MyGuiControlCombobox) break;
            }
            Controls.Clear();
            foreach(var c in keep)
            {
                Controls.Add(c);
            }
            
        }

        /// <summary>
        /// Gets the admin menu combo box from the list of controls
        /// </summary>
        /// <returns>The combo box or null if it does not exist</returns>
        private MyGuiControlCombobox GetCombo()
        {
            foreach(var c in Controls)
            {
                if(c is MyGuiControlCombobox)
                {
                    return (MyGuiControlCombobox)c;
                }
            }
            return null;
        }

        /// <summary>
        /// Creates a smaller button called a debug button
        /// </summary>
        /// <param name="usableWidth">Usable width of the button</param>
        /// <param name="text">Text of the button</param>
        /// <param name="onClick">OnClick callback</param>
        /// <param name="enabled">Whether it is enabled</param>
        /// <param name="tooltip">Tooltip of the button</param>
        /// <param name="increaseSpacing">Increased spacing or not</param>
        /// <param name="addToControls"><Whether it should be added to controls/param>
        /// <returns></returns>
        private MyGuiControlButton CreateDebugButton(float usableWidth, string text, Action<MyGuiControlButton> onClick, bool enabled = true, string tooltip = null, bool increaseSpacing = true, bool addToControls = true)
        {
            MyGuiControlButton myGuiControlButton = AddButton(new StringBuilder(text), onClick, null, null, null, increaseSpacing, addToControls);
            myGuiControlButton.VisualStyle = MyGuiControlButtonStyleEnum.Rectangular;
            myGuiControlButton.TextScale = m_scale;
            myGuiControlButton.Size = new Vector2(usableWidth, myGuiControlButton.Size.Y);
            myGuiControlButton.Position += new Vector2((0f - HIDDEN_PART_RIGHT) / 2f, 0f);
            myGuiControlButton.Enabled = enabled;
            if (tooltip == null)
            {
                myGuiControlButton.SetToolTip(tooltip);
            }
            return myGuiControlButton;
        }

        /// <summary>
        /// Tries to attach the spectator camera to the entity with the given id, so the camera
        /// will look at it.
        /// </summary>
        /// <param name="entityId">Id of the entity</param>
        /// <returns>If it was sucessful or not</returns>
        private static bool TryAttachCamera(long entityId)
        {
            if (MyEntities.TryGetEntityById(entityId, out MyEntity entity))
            {
                BoundingSphereD worldVolume = entity.PositionComp.WorldVolume;
                MySession.Static.SetCameraController(MyCameraControllerEnum.Spectator);
                MySpectatorCameraController.Static.Position = worldVolume.Center + Math.Max((float)worldVolume.Radius, 1f) * Vector3.One;
                MySpectatorCameraController.Static.Target = worldVolume.Center;
                MySessionComponentAnimationSystem.Static.EntitySelectedForDebug = entity;
                return true;
            }
            return false;
        }

        /// <summary>
        /// OnUpdate call for the gui.
        /// </summary>
        /// <param name="hasFocus">If the window has focus</param>
        /// <returns></returns>
        public override bool Update(bool hasFocus)
        {
            if(m_currentKey == 8)
            {
                bool ret = base.Update(hasFocus);
                TryAttachCamera(m_attachedEntity);
                return ret;
            }
            return base.Update(hasFocus);
        }
    }
}
