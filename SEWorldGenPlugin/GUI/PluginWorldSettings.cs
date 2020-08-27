using Sandbox.Game.Localization;
using Sandbox.Game.Screens;
using Sandbox.Game.Screens.Helpers;
using Sandbox.Graphics.GUI;
using SEWorldGenPlugin.ObjectBuilders;
using SEWorldGenPlugin.Session;
using SEWorldGenPlugin.Utilities;
using System;
using System.IO;
using System.Text;
using VRage;
using VRage.FileSystem;
using VRage.Game;
using VRage.Utils;
using VRageMath;

namespace SEWorldGenPlugin.GUI
{
    public class PluginWorldSettings : MyGuiScreenWorldSettings
    {

        public static MyGuiScreenWorldSettings Static;
        public MyObjectBuilder_PluginSettings PluginSettings;

        private MyGuiControlCheckbox m_enablePlugin;
        private MyGuiControlLabel m_enablePluginLabel;
        private MyGuiControlButton m_pluginSettingsButton;

        private float MARGIN_TOP = 0.22f;

        private float MARGIN_BOTTOM = 50f / MyGuiConstants.GUI_OPTIMAL_SIZE.Y;

        private float MARGIN_LEFT_INFO = 29.5f / MyGuiConstants.GUI_OPTIMAL_SIZE.X;

        private float MARGIN_RIGHT = 81f / MyGuiConstants.GUI_OPTIMAL_SIZE.X;

        private float MARGIN_LEFT_LIST = 90f / MyGuiConstants.GUI_OPTIMAL_SIZE.X;

        internal PluginSettingsGui SettingsGui;

        public MyObjectBuilder_PluginSettings PlSettings;
        public bool UseGlobal
        {
            get;
            private set;
        }

        public PluginWorldSettings(bool displayTabScenario = true, bool displayTabWorkshop = true, bool displayTabCustom = true) : this(null, null, displayTabScenario, displayTabWorkshop, displayTabCustom)
        {
        }

        public PluginWorldSettings(MyObjectBuilder_Checkpoint checkpoint, string path, bool displayTabScenario = true, bool displayTabWorkshop = true, bool displayTabCustom = true, bool isCloudPath = false) : base(checkpoint, path, displayTabScenario, displayTabWorkshop, displayTabCustom, isCloudPath)
        {
            Static = this;
            m_isNewGame = (checkpoint == null);
            UseGlobal = false;
        }

        protected override void BuildControls()
        {
            base.BuildControls();

            Vector2 vector = -m_size.Value / 2f + new Vector2(m_isNewGame ? (MARGIN_LEFT_LIST + MyGuiConstants.LISTBOX_WIDTH + MARGIN_LEFT_INFO) : MARGIN_LEFT_LIST, m_isNewGame ? (MARGIN_TOP + 0.015f) : (MARGIN_TOP - 0.105f));
            Vector2 value = new Vector2(0f, 0.052f);
            Vector2 vector2 = m_size.Value / 2f - vector;
            vector2.X -= MARGIN_RIGHT + 0.005f;
            vector2.Y -= MARGIN_BOTTOM;
            Vector2 vector3 = vector2 * (m_isNewGame ? 0.339f : 0.329f);
            Vector2 vector4 = vector2 - vector3;

            m_enablePluginLabel = new MyGuiControlLabel(null, null, "Enable SEWorldGenPlugin");

            m_enablePlugin = new MyGuiControlCheckbox();
            m_enablePlugin.SetToolTip("Enable the SEWorldGenPlugin for this world");
            m_enablePlugin.IsCheckedChanged = (Action<MyGuiControlCheckbox>)Delegate.Combine(m_enablePlugin.IsCheckedChanged, (Action<MyGuiControlCheckbox>)delegate (MyGuiControlCheckbox s)
            {
                if (PlSettings != null)
                {
                    PlSettings.Enable = s.IsChecked;
                }
            });

            m_enablePlugin.IsChecked = MySettings.Static.Settings.Enable;

            m_pluginSettingsButton = new MyGuiControlButton(null, MyGuiControlButtonStyleEnum.Small, null, null, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, null, new StringBuilder("Plugin settings"), 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, OnSettingsClick);
            Controls.Add(m_enablePluginLabel);
            Controls.Add(m_enablePlugin);
            Controls.Add(m_pluginSettingsButton);

            m_enablePluginLabel.Position = vector + value * 5;
            m_enablePluginLabel.PositionY += MyGuiConstants.BACK_BUTTON_SIZE.Y * 2;
            m_enablePlugin.Position = GetControlPosition();
            m_enablePlugin.PositionY = m_enablePluginLabel.PositionY;
            m_enablePlugin.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER;
            m_pluginSettingsButton.Position = m_enablePlugin.Position;
            m_pluginSettingsButton.PositionX += m_enablePlugin.Size.X + 0.009f;
            m_pluginSettingsButton.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER;
            foreach(var c in Controls)
            {
                if(c is MyGuiControlButton)
                {
                    MyGuiControlButton b = (MyGuiControlButton)c;
                    if(b.Text == MyTexts.GetString(MyCommonTexts.Start) || b.Text == MyTexts.GetString(MyCommonTexts.Ok))
                    {
                        if (m_isNewGame)
                        {
                            b.ButtonClicked += delegate
                            {
                                if (!UseGlobal)
                                {
                                    MySettings.Static.SessionSettings = PlSettings;
                                    PluginLog.Log("Settings: " + PlSettings.ToString());
                                }
                                else
                                {
                                    MySettings.Static.SessionSettings = new MyObjectBuilder_PluginSettings();
                                    MySettings.Static.SessionSettings.Enable = m_enablePlugin.IsChecked;
                                    MySettings.Static.SessionSettings.GeneratorSettings = MySettings.Static.Settings.GeneratorSettings;
                                }
                            };
                        }
                        else
                        {
                            b.ButtonClicked += delegate
                            {
                                var name = Checkpoint.SessionName;
                                var path = Path.Combine(MyFileSystem.SavesPath, name.Replace(":", "-"));
                                FileUtils.WriteXmlFileToPath(PlSettings, path, SettingsSession.FILE_NAME, typeof(PluginWorldSettings));
                            };
                        }
                    }
                }
            }

            if (m_isNewGame)
            {
                PlSettings = new MyObjectBuilder_PluginSettings();
                PlSettings.GeneratorSettings.FirstPlanetCenter = MySettings.Static.Settings.GeneratorSettings.FirstPlanetCenter;
                PlSettings.GeneratorSettings.PlanetSettings.BlacklistedPlanets = MySettings.Static.Settings.GeneratorSettings.PlanetSettings.BlacklistedPlanets;
                PlSettings.GeneratorSettings.PlanetSettings.Moons = MySettings.Static.Settings.GeneratorSettings.PlanetSettings.Moons;
                PlSettings.GeneratorSettings.PlanetSettings.MandatoryPlanets = MySettings.Static.Settings.GeneratorSettings.PlanetSettings.MandatoryPlanets;
                PlSettings.GeneratorSettings.PlanetSettings.GasGiants = MySettings.Static.Settings.GeneratorSettings.PlanetSettings.GasGiants;
                //PlSettings.GeneratorSettings.PlanetSettings.PlanetNameFormat = MySettings.Static.Settings.GeneratorSettings.PlanetSettings.PlanetNameFormat;
                //PlSettings.GeneratorSettings.PlanetSettings.MoonNameFormat = MySettings.Static.Settings.GeneratorSettings.PlanetSettings.MoonNameFormat;
                //PlSettings.GeneratorSettings.BeltSettings.BeltNameFormat = MySettings.Static.Settings.GeneratorSettings.BeltSettings.BeltNameFormat;
            }
            else
            {
                LoadValues();
            }
        }

        private Vector2 GetControlPosition()
        {
            foreach(var c in Controls)
            {
                if (c is MyGuiControlCheckbox)
                    return c.Position;
            }
            return Vector2.Zero;
        }

        private void LoadValues()
        {
            var path = Path.Combine(MyFileSystem.SavesPath, Checkpoint.SessionName.Replace(":", "-"));
            if(FileUtils.FileExistsInPath(path, SettingsSession.FILE_NAME, typeof(PluginWorldSettings)))
            {
                PlSettings = FileUtils.ReadXmlFileFromPath<MyObjectBuilder_PluginSettings>(path, SettingsSession.FILE_NAME, typeof(PluginWorldSettings));
            }
            else
            {
                PlSettings = new MyObjectBuilder_PluginSettings();
            }
            m_enablePlugin.IsChecked = PlSettings.Enable;
        }

        private void OnSettingsClick(object sender)
        {
            SettingsGui = new PluginSettingsGui(this);
            SettingsGui.OnOkButtonClicked += Settings_OnOkButtonClick;
            SettingsGui.SetSettings(this.PlSettings, UseGlobal);
            MyGuiSandbox.AddScreen(SettingsGui);
        }

        private void Settings_OnOkButtonClick()
        {
            UseGlobal = SettingsGui.GetSettings(ref PlSettings);
        }

        public override void RecreateControls(bool constructor)
        {
            base.RecreateControls(constructor);

            MyGuiControlScreenSwitchPanel p = null;
            MyGuiControlButton custom = null;
            MyGuiControlButton newGame = null;
            MyGuiControlButton workshop = null;

            foreach (var c in Controls)
            {
                if (c is MyGuiControlScreenSwitchPanel)
                {
                    p = (MyGuiControlScreenSwitchPanel)c;
                    break;
                }
            }
            if (p == null) return;
            foreach (var c in p.Controls)
            {
                if (c is MyGuiControlButton)
                {
                    if (((MyGuiControlButton)c).Text == MyTexts.GetString(MyCommonTexts.ScreenCaptionCustomWorld))
                        custom = (MyGuiControlButton)c;
                    if (((MyGuiControlButton)c).Text == MyTexts.GetString(MyCommonTexts.ScreenCaptionNewGame))
                        newGame = (MyGuiControlButton)c;
                    if (((MyGuiControlButton)c).Text == MyTexts.GetString(MyCommonTexts.ScreenCaptionWorkshop))
                        workshop = (MyGuiControlButton)c;
                }
            }
            if (custom != null)
            {
                p.Controls.Remove(custom);
                MyGuiControlButton newButton = new MyGuiControlButton(custom.Position, custom.VisualStyle, custom.Size, custom.ColorMask, custom.OriginAlign, MyTexts.GetString(MySpaceTexts.ToolTipNewGame_CustomGame), new StringBuilder(custom.Text), custom.TextScale, custom.TextAlignment, custom.HighlightType, OnCustomWorldButtonClick, custom.CueEnum);
                p.Controls.Add(newButton);
                FocusedControl = newButton;
                newButton.HighlightType = MyGuiControlHighlightType.FORCED;
                newButton.HasHighlight = true;
                newButton.Selected = true;
            }
            if (newGame != null)
            {
                p.Controls.Remove(newGame);
                MyGuiControlButton newButton = new MyGuiControlButton(newGame.Position, newGame.VisualStyle, newGame.Size, newGame.ColorMask, newGame.OriginAlign, MyTexts.GetString(MySpaceTexts.ToolTipNewGame_Campaign), new StringBuilder(newGame.Text), newGame.TextScale, newGame.TextAlignment, newGame.HighlightType, OnCampaignButtonClick, newGame.CueEnum);
                p.Controls.Add(newButton);
            }
            if (workshop != null)
            {
                p.Controls.Remove(workshop);
                MyGuiControlButton newButton = new MyGuiControlButton(workshop.Position, workshop.VisualStyle, workshop.Size, workshop.ColorMask, workshop.OriginAlign, MyTexts.GetString(MySpaceTexts.ToolTipNewGame_WorkshopContent), new StringBuilder(workshop.Text), workshop.TextScale, workshop.TextAlignment, workshop.HighlightType, OnWorkshopButtonClick, workshop.CueEnum);
                p.Controls.Add(newButton);
            }
        }

        private void OnCustomWorldButtonClick(MyGuiControlButton myGuiControlButton)
        {
            MyGuiScreenBase screenWithFocus = MyScreenManager.GetScreenWithFocus();
            if (!(screenWithFocus is MyGuiScreenWorldSettings))
            {
                SeamlesslyChangeScreen(screenWithFocus, new PluginWorldSettings());
            }
        }

        private void OnCampaignButtonClick(MyGuiControlButton myGuiControlButton)
        {
            MyGuiScreenBase screenWithFocus = MyScreenManager.GetScreenWithFocus();
            if (!(screenWithFocus is MyGuiScreenNewGame))
            {
                SeamlesslyChangeScreen(screenWithFocus, new PluginGuiScreenNewGame());
            }
        }

        private void OnWorkshopButtonClick(MyGuiControlButton myGuiControlButton)
        {
            MyGuiScreenBase screenWithFocus = MyScreenManager.GetScreenWithFocus();
            if (!(screenWithFocus is MyGuiScreenNewWorkshopGame))
            {
                SeamlesslyChangeScreen(screenWithFocus, new PluginGuiScreenWorkshopGame());
            }
        }

        private static void SeamlesslyChangeScreen(MyGuiScreenBase focusedScreen, MyGuiScreenBase exchangedFor)
        {
            focusedScreen.SkipTransition = true;
            focusedScreen.CloseScreen();
            exchangedFor.SkipTransition = true;
            MyScreenManager.AddScreenNow(exchangedFor);
        }
    }
}
