using Sandbox.Engine.Multiplayer;
using Sandbox.Engine.Utils;
using Sandbox.Game.Entities;
using Sandbox.Game.Gui;
using Sandbox.Game.Multiplayer;
using Sandbox.Game.World;
using Sandbox.Graphics.GUI;
using Sandbox.ModAPI;
using SEWorldGenPlugin.Session;
using SEWorldGenPlugin.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage.Game;
using VRage.Game.Entity;
using VRage.Game.SessionComponents;
using VRage.Utils;
using VRageMath;

namespace SEWorldGenPlugin.GUI
{
    [PreloadRequired]
    public class PluginAdminMenu : MyGuiScreenAdminMenu
    {
        private static readonly Vector2 SCREEN_SIZE = new Vector2(0.4f, 1.2f);
        private static readonly float HIDDEN_PART_RIGHT = 0.04f;

        private long m_currentKey;
        private long m_attachedEntity;

        private MyGuiControlListbox m_planetListBox;
        private MyGuiControlLabel m_ringDistanceValue;
        private MyGuiControlLabel m_ringAngleValue;
        private MyGuiControlLabel m_ringRoidSizeValue;
        private MyGuiControlLabel m_ringWidthValue;

        private MyGuiControlSlider m_ringDistanceSlider;
        private MyGuiControlSlider m_ringAngleSlider;
        private MyGuiControlSlider m_ringRoidSizeSlider;
        private MyGuiControlSlider m_ringWidthSlider;

        public PluginAdminMenu() : base()
        {
            m_currentKey = 0L;
        }

        public override void RecreateControls(bool constructor)
        {
            base.RecreateControls(constructor);

            MyGuiControlCombobox modeCombo = GetCombo();

            if (MySession.Static.IsUserSpaceMaster(Sync.MyId) && MySession.Static.IsUserAdmin(Sync.MyId))
                modeCombo.AddItem(8L, "SEWorldGenPlugin");

            MyGuiControlCombobox newCombo = AddCombo();

            for(int i = 0; i < modeCombo.GetItemsCount(); i++)
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
                if (newCombo.GetSelectedKey() > 7)
                {
                    RecreateControls(false);
                }
                else
                {
                    m_attachedEntity = 0L;
                    modeCombo.SelectItemByKey(newCombo.GetSelectedKey());
                }
            };
            if(newCombo.GetSelectedKey() == 8)
            {
                ClearControls();
                BuildPluginControls();
            }
        }

        private void BuildPluginControls()
        {
            Vector2 controlPadding = new Vector2(0.02f, 0.02f);
            float scale = 0.8f;
            float separatorSize = 0.01f;
            float num = SCREEN_SIZE.X - HIDDEN_PART_RIGHT - controlPadding.X * 2f;
            float num2 = (SCREEN_SIZE.Y - 1f) / 2f;

            m_currentPosition = -m_size.Value / 2f + controlPadding;
            m_currentPosition.Y += num2 + MyGuiConstants.SCREEN_CAPTION_DELTA_Y + controlPadding.Y - 0.012f + 0.095f;
            m_currentPosition.X += 0.018f;

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
            m_planetListBox.VisibleRowsCount = 10;
            m_planetListBox.Position = m_planetListBox.Size / 2f + m_currentPosition;
            m_planetListBox.ItemClicked += PlanetListItemClicked;
            m_planetListBox.MultiSelect = false;

            MyGuiControlSeparatorList separator = new MyGuiControlSeparatorList();
            separator.AddHorizontal(new Vector2(0f, 0f) - new Vector2(m_size.Value.X * 0.83f / 2f, -0.07f), m_size.Value.X * 0.73f);
            Controls.Add(separator);

            m_currentPosition = m_planetListBox.GetPositionAbsoluteBottomLeft();
            m_currentPosition.Y += 0.045f;

            MyGuiControlLabel ringDistLabel = new MyGuiControlLabel
            {
                Position = m_currentPosition + new Vector2(0.001f, 0f),
                OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP,
                Text = "Ring distance"
            };
            Controls.Add(ringDistLabel);

            m_ringDistanceValue = new MyGuiControlLabel
            {
                Position = m_currentPosition + new Vector2(0.285f, 0f),
                OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP,
                Text = "0"
            };
            Controls.Add(m_ringDistanceValue);

            m_currentPosition.Y += 0.035f;

            m_ringDistanceSlider = new MyGuiControlSlider(m_currentPosition + new Vector2(0.001f, 0f), 5000f, 1000000f, intValue: true, toolTip: MyPluginTexts.TOOLTIPS.ADMIN_RING_DISTANCE);//Make dynamic
            m_ringDistanceSlider.Size = new Vector2(0.285f, 1f);
            m_ringDistanceSlider.Value = 100000;
            m_ringDistanceSlider.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            m_ringDistanceSlider.ValueChanged = (Action<MyGuiControlSlider>)Delegate.Combine(m_ringDistanceSlider.ValueChanged, (Action<MyGuiControlSlider>)delegate (MyGuiControlSlider s)
            {
                m_ringDistanceValue.Text = s.Value.ToString();
            });

            m_ringDistanceValue.Text = m_ringDistanceSlider.Value.ToString();

            Controls.Add(m_ringDistanceSlider);

            m_currentPosition.Y += 0.045f;

            MyGuiControlLabel ringWidthLabel = new MyGuiControlLabel
            {
                Position = m_currentPosition + new Vector2(0.001f, 0f),
                OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP,
                Text = "Ring width"
            };
            Controls.Add(ringWidthLabel);

            m_ringWidthValue = new MyGuiControlLabel
            {
                Position = m_currentPosition + new Vector2(0.285f, 0f),
                OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP,
                Text = "0"
            };
            Controls.Add(m_ringWidthValue);

            m_currentPosition.Y += 0.035f;
            m_ringWidthSlider = new MyGuiControlSlider(m_currentPosition + new Vector2(0.001f, 0f), SettingsSession.Static.Settings.GeneratorSettings.PlanetSettings.RingSettings.MinPlanetRingWidth, SettingsSession.Static.Settings.GeneratorSettings.PlanetSettings.RingSettings.MaxPlanetRingWidth, intValue: true, toolTip: MyPluginTexts.TOOLTIPS.ADMIN_RING_WIDTH);
            m_ringWidthSlider.Size = new Vector2(0.285f, 1f);
            m_ringWidthSlider.Value = (SettingsSession.Static.Settings.GeneratorSettings.PlanetSettings.RingSettings.MinPlanetRingWidth + SettingsSession.Static.Settings.GeneratorSettings.PlanetSettings.RingSettings.MaxPlanetRingWidth) / 2;
            m_ringWidthSlider.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            m_ringWidthSlider.ValueChanged = (Action<MyGuiControlSlider>)Delegate.Combine(m_ringWidthSlider.ValueChanged, (Action<MyGuiControlSlider>)delegate (MyGuiControlSlider s)
            {
                m_ringWidthValue.Text = s.Value.ToString();
            });

            m_ringWidthValue.Text = m_ringWidthSlider.Value.ToString();

            Controls.Add(m_ringWidthSlider);

            m_currentPosition.Y += 0.045f;

            MyGuiControlLabel ringAngleLabel = new MyGuiControlLabel
            {
                Position = m_currentPosition + new Vector2(0.001f, 0f),
                OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP,
                Text = "Ring angle"
            };
            Controls.Add(ringAngleLabel);

            m_ringAngleValue = new MyGuiControlLabel
            {
                Position = m_currentPosition + new Vector2(0.285f, 0f),
                OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP,
                Text = "0.00"
            };
            Controls.Add(m_ringAngleValue);

            m_currentPosition.Y += 0.035f;
            m_ringAngleSlider = new MyGuiControlSlider(m_currentPosition + new Vector2(0.001f, 0f), -45, 45, intValue: false, toolTip: MyPluginTexts.TOOLTIPS.ADMIN_RING_ANGLE);
            m_ringAngleSlider.Size = new Vector2(0.285f, 1f);
            m_ringAngleSlider.Value = 0;
            m_ringAngleSlider.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            m_ringAngleSlider.ValueChanged = (Action<MyGuiControlSlider>)Delegate.Combine(m_ringAngleSlider.ValueChanged, (Action<MyGuiControlSlider>)delegate (MyGuiControlSlider s)
            {
                m_ringAngleValue.Text = String.Format("{0:0.00}", s.Value);
            });

            m_ringAngleValue.Text = String.Format("{0:0.00}", m_ringAngleSlider.Value);

            Controls.Add(m_ringAngleSlider);

            m_currentPosition.Y += 0.045f;

            MyGuiControlLabel ringRoidSizeLabel = new MyGuiControlLabel
            {
                Position = m_currentPosition + new Vector2(0.001f, 0f),
                OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP,
                Text = "Asteroid size"
            };
            Controls.Add(ringRoidSizeLabel);

            m_ringRoidSizeValue = new MyGuiControlLabel
            {
                Position = m_currentPosition + new Vector2(0.285f, 0f),
                OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP,
                Text = "0"
            };
            Controls.Add(m_ringRoidSizeValue);

            m_currentPosition.Y += 0.035f;
            m_ringRoidSizeSlider = new MyGuiControlSlider(m_currentPosition + new Vector2(0.001f, 0f), 128, 1028, intValue: true, toolTip: MyPluginTexts.TOOLTIPS.ADMIN_RING_ROID_SIZE);
            m_ringRoidSizeSlider.Size = new Vector2(0.285f, 1f);
            m_ringRoidSizeSlider.Value = 500;
            m_ringRoidSizeSlider.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            m_ringRoidSizeSlider.ValueChanged = (Action<MyGuiControlSlider>)Delegate.Combine(m_ringRoidSizeSlider.ValueChanged, (Action<MyGuiControlSlider>)delegate (MyGuiControlSlider s)
            {
                m_ringRoidSizeValue.Text = s.Value.ToString();
            });

            m_ringRoidSizeValue.Text = m_ringRoidSizeSlider.Value.ToString();

            Controls.Add(m_ringRoidSizeSlider);

            m_currentPosition.Y += 0.045f + 0.035f;

            MyGuiControlButton addRingButton = CreateDebugButton(0.284f, "Add ring to planet", OnAddRingToPlanetButton, true, MyPluginTexts.TOOLTIPS.ADMIN_ADD_RING_BUTTON);

            Controls.Add(m_planetListBox);

            FillList();
        }

        private void OnAddRingToPlanetButton(MyGuiControlButton button)
        {

        }

        private void PlanetListItemClicked(MyGuiControlListbox box)
        {
            if (box.SelectedItems.Count > 0)
            {
                MyEntityList.MyEntityListInfoItem myEntityListInfoItem = (MyEntityList.MyEntityListInfoItem)box.SelectedItems[box.SelectedItems.Count - 1].UserData;
                m_attachedEntity = myEntityListInfoItem.EntityId;
                if (!TryAttachCamera(myEntityListInfoItem.EntityId))
                {
                    MySession.Static.SetCameraController(MyCameraControllerEnum.Spectator, null, myEntityListInfoItem.Position + Vector3.One * 50f);
                }
            }
        }

        private void FillList()
        {
            List<MyEntityList.MyEntityListInfoItem> planets = MyEntityList.GetEntityList(MyEntityList.MyEntityTypeEnum.Planets);
            foreach(var item in planets)
            {
                m_planetListBox.Items.Add(new MyGuiControlListbox.Item(new StringBuilder(item.DisplayName), null, null, item));
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
