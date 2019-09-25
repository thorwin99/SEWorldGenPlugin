using Sandbox;
using Sandbox.Game.Localization;
using Sandbox.Graphics.GUI;
using SEWorldGenPlugin.ObjectBuilders;
using System;
using VRage.Utils;
using VRageMath;
using SEWorldGenPlugin.Utilities;

namespace SEWorldGenPlugin.GUI
{
    public class PluginSettingsGui : MyGuiScreenBase
    {
        private MyGuiControlLabel m_objAmountLabel;
        private MyGuiControlLabel m_orbDistanceLabel;
        private MyGuiControlLabel m_useGlobalSettignsLabel;
        private MyGuiControlLabel m_sizeMultiplierLabel;
        private MyGuiControlLabel m_sizeCapLabel;
        private MyGuiControlLabel m_moonProbLabel;
        private MyGuiControlLabel m_ringWidthLabel;
        private MyGuiControlLabel m_ringProbLabel;
        private MyGuiControlLabel m_beltHeightLabel;
        private MyGuiControlLabel m_beltProbLabel;

        private MyGuiControlLabel m_objAmountValue;
        private MyGuiControlLabel m_orbDistanceValue;
        private MyGuiControlLabel m_sizeMultiplierValue;
        private MyGuiControlLabel m_sizeCapValue;
        private MyGuiControlLabel m_moonProbValue;
        private MyGuiControlLabel m_ringWidthValue;
        private MyGuiControlLabel m_ringProbValue;
        private MyGuiControlLabel m_beltHeightValue;
        private MyGuiControlLabel m_beltProbValue;

        private MyGuiControlCheckbox m_useGlobalCheck;
        private MyGuiControlSlider m_objAmountSlider;
        private MyGuiControlSlider m_orbDistanceSlider;
        private MyGuiControlSlider m_sizeMultiplierSlider;
        private MyGuiControlSlider m_sizeCapSlider;
        private MyGuiControlSlider m_moonProbSlider;
        private MyGuiControlSlider m_ringWidthSlider;
        private MyGuiControlSlider m_ringProbSlider;
        private MyGuiControlSlider m_beltHeightSlider;
        private MyGuiControlSlider m_beltProbSlider;

        private MyGuiControlButton m_okButton;

        private PluginWorldSettings m_parent;

        private bool m_isNewGame;

        public event Action OnOkButtonClicked;

        public PluginSettingsGui(PluginWorldSettings parent) : base(new Vector2(0.5f, 0.5f), MyGuiConstants.SCREEN_BACKGROUND_COLOR, CalcSize(), isTopMostScreen: false, null, MySandboxGame.Config.UIBkOpacity, MySandboxGame.Config.UIOpacity)
        {
            m_parent = parent;
            base.EnabledBackgroundFade = true;
            m_isNewGame = (parent.Checkpoint == null);
            RecreateControls(true);
        }

        public override void RecreateControls(bool constructor)
        {
            base.RecreateControls(constructor);
            BuildControls();
        }

        public void BuildControls()
        {
            Vector2 vector = new Vector2(50f) / MyGuiConstants.GUI_OPTIMAL_SIZE;
            float x2 = 0.309375018f;
            AddCaption("SEWorldGenPlugin Settings", null, new Vector2(0f, 0.003f));
            MyGuiControlSeparatorList myGuiControlSeparatorList = new MyGuiControlSeparatorList();
            myGuiControlSeparatorList.AddHorizontal(new Vector2(0f, 0f) - new Vector2(m_size.Value.X * 0.835f / 2f, m_size.Value.Y / 2f - 0.075f), m_size.Value.X * 0.835f);
            Controls.Add(myGuiControlSeparatorList);
            MyGuiControlSeparatorList myGuiControlSeparatorList2 = new MyGuiControlSeparatorList();
            myGuiControlSeparatorList2.AddHorizontal(new Vector2(0f, 0f) - new Vector2(m_size.Value.X * 0.835f / 2f, (0f - m_size.Value.Y) / 2f + 0.123f), m_size.Value.X * 0.835f);
            Controls.Add(myGuiControlSeparatorList2);

            m_useGlobalSettignsLabel = MakeLabel("Use global Config");
            m_objAmountLabel = MakeLabel("Objects in System");
            m_orbDistanceLabel = MakeLabel("Average Orbit distance");
            m_sizeMultiplierLabel = MakeLabel("Planet size multiplier");
            m_sizeCapLabel = MakeLabel("Planet size cap");
            m_moonProbLabel = MakeLabel("Moon spawn probability");
            m_ringWidthLabel = MakeLabel("Average ring width");
            m_ringProbLabel = MakeLabel("Ring spawn probability");
            m_beltHeightLabel = MakeLabel("Average belt height");
            m_beltProbLabel = MakeLabel("Belt spawn probability");

            m_useGlobalCheck = new MyGuiControlCheckbox();
            m_objAmountSlider = new MyGuiControlSlider(Vector2.Zero, 0f, 100f, x2, 15f, null, null, 0, 0.8f, 0.05f, "White", MyPluginTexts.TOOLTIPS.SYS_OBJ_SLIDER, MyGuiControlSliderStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER, intValue: true);
            m_orbDistanceSlider = new MyGuiControlSlider(Vector2.Zero, 500f, 100000f, x2, 50500f, null, null, 0, 0.8f, 0.05f, "White", MyPluginTexts.TOOLTIPS.ORB_DIST_SLIDER, MyGuiControlSliderStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER, intValue: true);
            m_sizeMultiplierSlider = new MyGuiControlSlider(Vector2.Zero, 1f, 10f, x2, 2f, null, null, 0, 0.8f, 0.05f, "White", MyPluginTexts.TOOLTIPS.SIZE_MUL_SLIDER, MyGuiControlSliderStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER, intValue: true);
            m_sizeCapSlider = new MyGuiControlSlider(Vector2.Zero, 120f, 2400f, x2, 1200f, null, null, 0, 0.8f, 0.05f, "White", MyPluginTexts.TOOLTIPS.SIZE_CAP_SLIDER, MyGuiControlSliderStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER, intValue: true);
            m_moonProbSlider = new MyGuiControlSlider(Vector2.Zero, 0f, 1f, x2, 0.5f, null, null, 0, 0.8f, 0.05f, "White", MyPluginTexts.TOOLTIPS.MOON_PROB_SLIDER, MyGuiControlSliderStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER, intValue: false);
            m_ringWidthSlider = new MyGuiControlSlider(Vector2.Zero, 10000f, 100000f, x2, 15000f, null, null, 0, 0.8f, 0.05f, "White", MyPluginTexts.TOOLTIPS.RING_WIDTH_SLIDER, MyGuiControlSliderStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER, intValue: true);
            m_ringProbSlider = new MyGuiControlSlider(Vector2.Zero, 0f, 1f, x2, 0.5f, null, null, 0, 0.8f, 0.05f, "White", MyPluginTexts.TOOLTIPS.RING_PROB_SLIDER, MyGuiControlSliderStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER, intValue: false);
            m_beltHeightSlider = new MyGuiControlSlider(Vector2.Zero, 4000f, 40000f, x2, 22000f, null, null, 0, 0.8f, 0.05f, "White", MyPluginTexts.TOOLTIPS.BELT_HEIGHT_SLIDER, MyGuiControlSliderStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER, intValue: true);
            m_beltProbSlider = new MyGuiControlSlider(Vector2.Zero, 0f, 1f, x2, 0.4f, null, null, 0, 0.8f, 0.05f, "White", MyPluginTexts.TOOLTIPS.BELT_PROB_SLIDER, MyGuiControlSliderStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER, intValue: false);

            m_objAmountValue = MakeLabel(m_objAmountSlider.Value.ToString());
            m_objAmountValue.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_CENTER;
            m_orbDistanceValue = MakeLabel(m_orbDistanceSlider.Value.ToString());
            m_orbDistanceValue.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_CENTER;
            m_sizeMultiplierValue = MakeLabel(m_sizeMultiplierSlider.Value.ToString());
            m_sizeMultiplierValue.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_CENTER;
            m_sizeCapValue = MakeLabel(m_sizeCapSlider.Value.ToString());
            m_sizeCapValue.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_CENTER;
            m_moonProbValue = MakeLabel(String.Format("{0:0.00}", m_moonProbSlider.Value));
            m_moonProbValue.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_CENTER;
            m_ringWidthValue = MakeLabel(m_ringWidthSlider.Value.ToString());
            m_ringWidthValue.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_CENTER;
            m_ringProbValue = MakeLabel(m_ringProbSlider.Value.ToString());
            m_ringProbValue.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_CENTER;
            m_beltHeightValue = MakeLabel(m_beltHeightSlider.Value.ToString());
            m_beltHeightValue.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_CENTER;
            m_beltProbValue = MakeLabel(String.Format("{0:0.00}", m_beltProbSlider.Value));
            m_beltProbValue.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_CENTER;

            m_useGlobalCheck.SetToolTip(MyPluginTexts.TOOLTIPS.USE_GLOBAL_CHECK);
            m_useGlobalCheck.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER;

            m_objAmountSlider.ValueChanged = (Action<MyGuiControlSlider>)Delegate.Combine(m_objAmountSlider.ValueChanged, (Action<MyGuiControlSlider>)delegate (MyGuiControlSlider s)
            {
                m_objAmountValue.Text = s.Value.ToString();
            });
            m_orbDistanceSlider.ValueChanged = (Action<MyGuiControlSlider>)Delegate.Combine(m_orbDistanceSlider.ValueChanged, (Action<MyGuiControlSlider>)delegate (MyGuiControlSlider s)
            {
                m_orbDistanceValue.Text = s.Value.ToString();
            });
            m_sizeMultiplierSlider.ValueChanged = (Action<MyGuiControlSlider>)Delegate.Combine(m_sizeMultiplierSlider.ValueChanged, (Action<MyGuiControlSlider>)delegate (MyGuiControlSlider s)
            {
                m_sizeMultiplierValue.Text = s.Value.ToString();
            });
            m_sizeCapSlider.ValueChanged = (Action<MyGuiControlSlider>)Delegate.Combine(m_sizeCapSlider.ValueChanged, (Action<MyGuiControlSlider>)delegate (MyGuiControlSlider s)
            {
                m_sizeCapValue.Text = s.Value.ToString();
            });
            m_moonProbSlider.ValueChanged = (Action<MyGuiControlSlider>)Delegate.Combine(m_moonProbSlider.ValueChanged, (Action<MyGuiControlSlider>)delegate (MyGuiControlSlider s)
            {
                m_moonProbValue.Text = String.Format("{0:0.00}", s.Value);
            });
            m_ringWidthSlider.ValueChanged = (Action<MyGuiControlSlider>)Delegate.Combine(m_ringWidthSlider.ValueChanged, (Action<MyGuiControlSlider>)delegate (MyGuiControlSlider s)
            {
                m_ringWidthValue.Text = s.Value.ToString();
            });
            m_ringProbSlider.ValueChanged = (Action<MyGuiControlSlider>)Delegate.Combine(m_ringProbSlider.ValueChanged, (Action<MyGuiControlSlider>)delegate (MyGuiControlSlider s)
            {
                m_ringProbValue.Text = String.Format("{0:0.00}", s.Value);
            });
            m_beltHeightSlider.ValueChanged = (Action<MyGuiControlSlider>)Delegate.Combine(m_beltHeightSlider.ValueChanged, (Action<MyGuiControlSlider>)delegate (MyGuiControlSlider s)
            {
                m_beltHeightValue.Text = s.Value.ToString();
            });
            m_beltProbSlider.ValueChanged = (Action<MyGuiControlSlider>)Delegate.Combine(m_beltProbSlider.ValueChanged, (Action<MyGuiControlSlider>)delegate (MyGuiControlSlider s)
            {
                m_beltProbValue.Text = String.Format("{0:0.000}", s.Value); ;
            });
            m_useGlobalCheck.IsCheckedChanged = (Action<MyGuiControlCheckbox>)Delegate.Combine(m_useGlobalCheck.IsCheckedChanged, (Action<MyGuiControlCheckbox>)delegate (MyGuiControlCheckbox s)
            {
                m_objAmountSlider.Enabled = !s.IsChecked;
                m_orbDistanceSlider.Enabled = !s.IsChecked;
                m_sizeMultiplierSlider.Enabled = !s.IsChecked;
                m_sizeCapSlider.Enabled = !s.IsChecked;
                m_moonProbSlider.Enabled = !s.IsChecked;
                m_ringWidthSlider.Enabled = !s.IsChecked;
                m_ringProbSlider.Enabled = !s.IsChecked;
                m_beltHeightSlider.Enabled = !s.IsChecked;
                m_beltProbSlider.Enabled = !s.IsChecked;
            });

            Controls.Add(m_useGlobalSettignsLabel);
            Controls.Add(m_useGlobalCheck);

            Controls.Add(m_objAmountLabel);
            Controls.Add(m_objAmountSlider);
            Controls.Add(m_objAmountValue);

            Controls.Add(m_orbDistanceLabel);
            Controls.Add(m_orbDistanceSlider);
            Controls.Add(m_orbDistanceValue);

            Controls.Add(m_sizeMultiplierLabel);
            Controls.Add(m_sizeMultiplierSlider);
            Controls.Add(m_sizeMultiplierValue);

            Controls.Add(m_sizeCapLabel);
            Controls.Add(m_sizeCapSlider);
            Controls.Add(m_sizeCapValue);

            Controls.Add(m_moonProbLabel);
            Controls.Add(m_moonProbSlider);
            Controls.Add(m_moonProbValue);

            Controls.Add(m_ringWidthLabel);
            Controls.Add(m_ringWidthSlider);
            Controls.Add(m_ringWidthValue);

            Controls.Add(m_ringProbLabel);
            Controls.Add(m_ringProbSlider);
            Controls.Add(m_ringProbValue);

            Controls.Add(m_beltHeightLabel);
            Controls.Add(m_beltHeightSlider);
            Controls.Add(m_beltHeightValue);

            Controls.Add(m_beltProbLabel);
            Controls.Add(m_beltProbSlider);
            Controls.Add(m_beltProbValue);

            Vector2 start = (new Vector2(0f, 0f) - new Vector2(m_size.Value.X * 0.835f / 2f, m_size.Value.Y / 2f - 0.075f)) + (new Vector2(0f, m_useGlobalSettignsLabel.Size.Y));
            Vector2 offset = new Vector2(0f, 0.052f);
            Vector2 offset2 = new Vector2(m_orbDistanceLabel.Size.X * 1.5f, 0f);
            Vector2 offset3 = new Vector2(m_size.Value.X * 0.835f, 0f);

            m_useGlobalSettignsLabel.Position = start + offset * 0;
            m_useGlobalCheck.Position = m_useGlobalSettignsLabel.Position + offset2;

            m_objAmountLabel.Position = start + offset * 1;
            m_objAmountSlider.Position = m_objAmountLabel.Position + offset2;
            m_objAmountValue.Position = m_objAmountLabel.Position + offset3;

            m_orbDistanceLabel.Position = start + offset * 2;
            m_orbDistanceSlider.Position = m_orbDistanceLabel.Position + offset2;
            m_orbDistanceValue.Position = m_orbDistanceLabel.Position + offset3;

            m_sizeMultiplierLabel.Position = start + offset * 3;
            m_sizeMultiplierSlider.Position = m_sizeMultiplierLabel.Position + offset2;
            m_sizeMultiplierValue.Position = m_sizeMultiplierLabel.Position + offset3;

            m_sizeCapLabel.Position = start + offset * 4;
            m_sizeCapSlider.Position = m_sizeCapLabel.Position + offset2;
            m_sizeCapValue.Position = m_sizeCapLabel.Position + offset3;

            m_moonProbLabel.Position = start + offset * 5;
            m_moonProbSlider.Position = m_moonProbLabel.Position + offset2;
            m_moonProbValue.Position = m_moonProbLabel.Position + offset3;

            m_ringWidthLabel.Position = start + offset * 6;
            m_ringWidthSlider.Position = m_ringWidthLabel.Position + offset2;
            m_ringWidthValue.Position = m_ringWidthLabel.Position + offset3;

            m_ringProbLabel.Position = start + offset * 7;
            m_ringProbSlider.Position = m_ringProbLabel.Position + offset2;
            m_ringProbValue.Position = m_ringProbLabel.Position + offset3;

            m_beltHeightLabel.Position = start + offset * 8;
            m_beltHeightSlider.Position = m_beltHeightLabel.Position + offset2;
            m_beltHeightValue.Position = m_beltHeightLabel.Position + offset3;

            m_beltProbLabel.Position = start + offset * 9;
            m_beltProbSlider.Position = m_beltProbLabel.Position + offset2;
            m_beltProbValue.Position = m_beltProbLabel.Position + offset3;

            m_okButton = new MyGuiControlButton(null, VRage.Game.MyGuiControlButtonStyleEnum.Default, null, null, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, null, VRage.MyTexts.Get(MyCommonTexts.Ok), 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, OkButtonClicked);
            m_okButton.SetToolTip(VRage.MyTexts.GetString(MySpaceTexts.ToolTipOptionsSpace_Ok));
            m_okButton.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_BOTTOM;
            m_okButton.Position = (m_size.Value / 2f - vector) * new Vector2(0f, 1f) + new Vector2(0f - 25f, 0f) / MyGuiConstants.GUI_OPTIMAL_SIZE;

            Controls.Add(m_okButton);
        }

        public void SetSettings(MyObjectBuilder_PluginSettings settings, bool useGlobal)
        {
            MyLog.Default.WriteLine(FileUtils.SerializeToXml(settings) + "  " + useGlobal);

            m_useGlobalCheck.IsChecked = useGlobal;

            if (useGlobal || settings == null) return;

            m_objAmountSlider.Value = (settings.GeneratorSettings.MinObjectsInSystem + settings.GeneratorSettings.MaxObjectsInSystem) / 2;

            m_orbDistanceSlider.Value = (settings.GeneratorSettings.MinOrbitDistance / 1000 + settings.GeneratorSettings.MaxOrbitDistance / 1000) / 2;

            m_sizeMultiplierSlider.Value = settings.GeneratorSettings.PlanetSettings.SizeMultiplier;

            m_sizeCapSlider.Value = (float)settings.GeneratorSettings.PlanetSettings.PlanetSizeCap / 1000;

            m_moonProbSlider.Value = settings.GeneratorSettings.PlanetSettings.MoonProbability;

            m_ringWidthSlider.Value = (settings.GeneratorSettings.PlanetSettings.RingSettings.MinPlanetRingWidth + settings.GeneratorSettings.PlanetSettings.RingSettings.MaxPlanetRingWidth) / 2;

            m_ringProbSlider.Value = settings.GeneratorSettings.PlanetSettings.RingSettings.PlanetRingProbability;

            m_beltHeightSlider.Value = (settings.GeneratorSettings.BeltSettings.MinBeltHeight + settings.GeneratorSettings.BeltSettings.MaxBeltHeight) / 2;
        }

        public bool GetSettings(ref MyObjectBuilder_PluginSettings settings)
        {
            if (settings == null)
                settings = new MyObjectBuilder_PluginSettings();

            settings.GeneratorSettings.MinObjectsInSystem = (int)m_objAmountSlider.Value;
            settings.GeneratorSettings.MaxObjectsInSystem = (int)m_objAmountSlider.Value;

            settings.GeneratorSettings.MinOrbitDistance = (int)m_orbDistanceSlider.Value * 1000 / 10;
            settings.GeneratorSettings.MaxOrbitDistance = (int)(m_orbDistanceSlider.Value * 1000 * 2 - settings.GeneratorSettings.MinOrbitDistance);

            settings.GeneratorSettings.PlanetSettings.SizeMultiplier = (int)m_sizeMultiplierSlider.Value;
            settings.GeneratorSettings.PlanetSettings.PlanetSizeCap = (int)m_sizeCapSlider.Value * 1000;
            settings.GeneratorSettings.PlanetSettings.MoonProbability = m_moonProbSlider.Value;

            settings.GeneratorSettings.PlanetSettings.RingSettings.MinPlanetRingWidth = (int)m_ringWidthSlider.Value / 10;
            settings.GeneratorSettings.PlanetSettings.RingSettings.MaxPlanetRingWidth = (int)m_ringWidthSlider.Value * 2 - settings.GeneratorSettings.PlanetSettings.RingSettings.MinPlanetRingWidth;
            settings.GeneratorSettings.PlanetSettings.RingSettings.PlanetRingProbability = m_ringProbSlider.Value;

            settings.GeneratorSettings.BeltSettings.MinBeltHeight = (int)m_beltHeightSlider.Value / 10;
            settings.GeneratorSettings.BeltSettings.MaxBeltHeight = (int)m_beltHeightSlider.Value * 2 - settings.GeneratorSettings.BeltSettings.MinBeltHeight;
            settings.GeneratorSettings.BeltSettings.BeltProbability = m_beltProbSlider.Value;

            return m_useGlobalCheck.IsChecked;
        }

        public override string GetFriendlyName()
        {
            return "PluginSettingsGui";
        }

        public static Vector2 CalcSize()
        {
            return new Vector2(183f / 280f, 0.9398855f);
        }

        private MyGuiControlLabel MakeLabel(string text)
        {
            return new MyGuiControlLabel(null, null, text);
        }

        private void OkButtonClicked(object sender)
        {
            this.OnOkButtonClicked?.Invoke();
            CloseScreen();
        }
    }
}
