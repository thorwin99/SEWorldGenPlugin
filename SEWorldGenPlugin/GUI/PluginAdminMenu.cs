using Sandbox.Definitions;
using Sandbox.Engine.Multiplayer;
using Sandbox.Engine.Utils;
using Sandbox.Engine.Voxels;
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
using VRage.Game.Voxels;
using VRage.Library.Utils;
using VRage.Utils;
using VRageMath;

namespace SEWorldGenPlugin.GUI
{
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

        public PluginAdminMenu() : base()
        {
            m_currentKey = 0L;
            m_pluginInstalled = false;
        }

        public override void RecreateControls(bool constructor)
        {
            base.RecreateControls(constructor);

            MyGuiControlCombobox modeCombo = GetCombo();

            if (MySession.Static.IsUserSpaceMaster(Sync.MyId) && MySession.Static.IsUserAdmin(Sync.MyId))
            {
                modeCombo.AddItem(9L, "SEWorldGenPlugin - Rings");
                modeCombo.AddItem(10L, "SEWorldGenPlugin - Planets");
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
                if (newCombo.GetSelectedKey() > 8)
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
            if(newCombo.GetSelectedKey() == 9)
            {
                ClearControls();
                CheckBuildPluginControls(BuildRingMenu);
            }
            else if(newCombo.GetSelectedKey() == 10)
            {
                ClearControls();
                CheckBuildPluginControls(BuildPlanetMenu);
            }
        }

        private void CheckBuildPluginControls(Action creator)
        {
            if (!m_pluginInstalled)
            {
                NetUtil.PingServer(delegate
                {
                    m_pluginInstalled = true;
                    //RecreateControls(false);
                });
            }
            creator?.Invoke();
        }

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

        private void BuildPlanetMenu()
        {
            ClearControls();
            Vector2 controlPadding = new Vector2(0.02f, 0.02f);
            float num = SCREEN_SIZE.X - HIDDEN_PART_RIGHT - controlPadding.X * 2f;
            float num2 = (SCREEN_SIZE.Y - 1f) / 2f;

            BuildPluginMenuHeader();

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

            m_planetSizeSlider = new MyGuiControlClickableSlider(m_currentPosition + new Vector2(0.001f, 0f), 120f, 2400f, intValue: true, toolTip: MyPluginTexts.TOOLTIPS.ADMIN_PLANET_SIZE);
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

        private void BuildRingMenu()
        {
            ClearControls();

            Vector2 controlPadding = new Vector2(0.02f, 0.02f);
            float num = SCREEN_SIZE.X - HIDDEN_PART_RIGHT - controlPadding.X * 2f;

            BuildPluginMenuHeader();

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

            m_ringWidthSlider = new MyGuiControlClickableSlider(vector + new Vector2(0.001f, 0f), SettingsSession.Static.Settings.GeneratorSettings.PlanetSettings.RingSettings.MinPlanetRingWidth, SettingsSession.Static.Settings.GeneratorSettings.PlanetSettings.RingSettings.MaxPlanetRingWidth, intValue: true, toolTip: MyPluginTexts.TOOLTIPS.ADMIN_RING_WIDTH, showLabel: false);
            m_ringWidthSlider.Size = new Vector2(0.285f, 1f);
            m_ringWidthSlider.DefaultValue = (SettingsSession.Static.Settings.GeneratorSettings.PlanetSettings.RingSettings.MinPlanetRingWidth + SettingsSession.Static.Settings.GeneratorSettings.PlanetSettings.RingSettings.MaxPlanetRingWidth) / 2;
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

            /*MyGuiControlSeparatorList separator2 = new MyGuiControlSeparatorList();
            separator2.AddHorizontal(new Vector2(0f, 0f) - new Vector2(m_size.Value.X * 0.83f / 2f, -0.00f), m_size.Value.X * 0.73f);
            Controls.Add(separator2);*/

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

        private void UpdateRingVisual()
        {
            PluginDrawSession.Static.RemoveRenderObject(m_selectedPlanet.GetHashCode());

            AsteroidRingShape shape = AsteroidRingShape.CreateFromRingItem(GenerateRingItem());

            PluginDrawSession.Static.AddRenderObject(m_selectedPlanet.GetHashCode(), new RenderHollowCylinder(shape.worldMatrix, (float)shape.radius + shape.width, (float)shape.radius, shape.height, Color.LightGreen.ToVector4()));
        }

        private void OnTeleportToRingButton(MyGuiControlButton button)
        {
            if(MySession.Static.CameraController != MySession.Static.LocalCharacter)
            {
                CloseScreen();
                AsteroidRingShape shape = AsteroidRingShape.CreateFromRingItem(m_selectedPlanet.PlanetRing);

                MyMultiplayer.TeleportControlledEntity(shape.LocationInRing(0));
                m_attachedEntity = 0L;
                m_selectedPlanet = null;
                MyGuiScreenGamePlay.SetCameraController();
                
            }
        }

        public override bool CloseScreen()
        {
            if(m_selectedPlanet != null)
                PluginDrawSession.Static.RemoveRenderObject(m_selectedPlanet.GetHashCode());
            return base.CloseScreen();
        }

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
                    SystemGenerator.Static.AddRingToPlanet(m_selectedPlanet.DisplayName, item);
                }
                PlanetListItemClicked(m_planetListBox);
            }
        }
        
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
                PlanetListItemClicked(m_planetListBox);
            }
        }

        private MyPlanetRingItem GenerateRingItem()
        {
            if (m_selectedPlanet.PlanetRing != null) return m_selectedPlanet.PlanetRing;
            MyPlanetRingItem item = new MyPlanetRingItem();
            item.Type = SystemObjectType.RING;
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

        private void PlanetListItemClicked(MyGuiControlListbox box)
        {
            if (box.SelectedItems.Count > 0)
            {
                MyEntityList.MyEntityListInfoItem myEntityListInfoItem = (MyEntityList.MyEntityListInfoItem)box.SelectedItems[box.SelectedItems.Count - 1].UserData;
                m_attachedEntity = myEntityListInfoItem.EntityId;
                if (SNAP_CAMERA_TO_PLANET && !TryAttachCamera(myEntityListInfoItem.EntityId))
                {
                    MySession.Static.SetCameraController(MyCameraControllerEnum.Spectator, null, myEntityListInfoItem.Position + Vector3.One * 50f);
                }

                MyPlanet planetEntity = (MyPlanet)MyEntities.GetEntityById(myEntityListInfoItem.EntityId);

                string name = "";
                if (myEntityListInfoItem.DisplayName.StartsWith("Planet"))
                {
                    name = myEntityListInfoItem.DisplayName.Replace("_", " ").Split('-')[0].Trim();
                }
                else
                {
                    name = myEntityListInfoItem.DisplayName.Replace("_", " ");
                }

                if(m_selectedPlanet != null)
                {
                    PluginDrawSession.Static.RemoveRenderObject(m_selectedPlanet.GetHashCode());
                }

                SystemGenerator.Static.GetObject(name, delegate (bool success, MySystemItem obj)
                {
                    if (success)
                    {
                        m_selectedPlanet = (MyPlanetItem)obj;
                        m_newPlanet = false;
                    }
                    else
                    {
                        m_selectedPlanet = new MyPlanetItem()
                        {
                            DisplayName = myEntityListInfoItem.DisplayName.Replace("_", " "),
                            CenterPosition = planetEntity.PositionComp.GetPosition(),
                            DefName = ((MyObjectBuilder_Planet)planetEntity.GetObjectBuilder()).Name,
                            Generated = true,
                            OffsetPosition = planetEntity.PositionLeftBottomCorner,
                            PlanetMoons = new MyPlanetMoonItem[0],
                            PlanetRing = null,
                            Size = planetEntity.AverageRadius * 2,
                            Type = SystemObjectType.PLANET
                        };
                        m_newPlanet = true;
                    }

                    m_ringDistanceSlider.MinValue = planetEntity.AverageRadius * 2 * 0.75f - planetEntity.AverageRadius;
                    m_ringDistanceSlider.MinValue -= m_ringDistanceSlider.MinValue % 1000;
                    m_ringDistanceSlider.MaxValue = planetEntity.AverageRadius * 2 * 2 - planetEntity.AverageRadius;
                    m_ringDistanceSlider.MaxValue -= m_ringDistanceSlider.MaxValue % 1000;
                    m_ringDistanceSlider.Value = planetEntity.AverageRadius * 2 * 1.25f - planetEntity.AverageRadius;
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
                });
            }
        }

        private void LoadPlanetsInWorld()
        {
            List<MyEntityList.MyEntityListInfoItem> planets = MyEntityList.GetEntityList(MyEntityList.MyEntityTypeEnum.Planets);
            foreach(var item in planets)
            {
                string name = item.DisplayName.Replace("_", " ");

                if (name.StartsWith("Moon ")) continue;

                m_planetListBox.Items.Add(new MyGuiControlListbox.Item(new StringBuilder(name), null, null, item));
            }
        }

        private void LoadPlanetDefinitions()
        {
            var definitions = SystemGenerator.Static.PlanetDefinitions;
            foreach (var item in definitions)
            {
                string name = item.Id.SubtypeName.ToString();

                m_planetDefListBox.Items.Add(new MyGuiControlListbox.Item(new StringBuilder(name), null, null, item));
            }
        }

        private void PlanetDefListItemClicked(MyGuiControlListbox box)
        {
            if (box.SelectedItems.Count > 0)
            {                
                m_selectedDefinition = (MyPlanetGeneratorDefinition)box.SelectedItems[box.SelectedItems.Count - 1].UserData;
                m_spawnPlanetButton.Enabled = true;
            }
        }

        private void OnSpawnPlanetButton(MyGuiControlButton button)
        {
            if (m_planetDefListBox.SelectedItems.Count > 0)
            {
                float size = m_planetSizeSlider.Value * 1000;
                MyPlanetItem planet = new MyPlanetItem()
                {
                    OffsetPosition = Vector3D.Zero,
                    DefName = m_selectedDefinition.Id.SubtypeId.ToString(),
                    DisplayName = m_selectedDefinition.Id.SubtypeId.ToString() + "_" + size + "_" + MyRandom.Instance.Next(),
                    Generated = false,
                    PlanetMoons = new MyPlanetMoonItem[0],
                    PlanetRing = null,
                    Size = size,
                    Type = SystemObjectType.PLANET,
                    CenterPosition = Vector3D.Zero
                };

                PluginItemsClipboard.Static.Activate(planet, SpawnPlanet, size);

                CloseScreenNow();
            }
        }

        private void SpawnPlanet(MySystemItem planet, Vector3D position)
        {
            if(planet.Type == SystemObjectType.PLANET)
            {
                MyPlanetItem p = (MyPlanetItem)planet;
                p.CenterPosition = position;

                SystemGenerator.Static.AddPlanet(p);
            }
        }

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
