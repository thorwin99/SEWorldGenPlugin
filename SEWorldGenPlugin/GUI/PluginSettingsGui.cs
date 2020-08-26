using Sandbox;
using Sandbox.Game.Localization;
using Sandbox.Graphics.GUI;
using SEWorldGenPlugin.GUI.Controls;
using SEWorldGenPlugin.ObjectBuilders;
using SEWorldGenPlugin.Utilities;
using System;
using VRage.Utils;
using VRageMath;

namespace SEWorldGenPlugin.GUI
{
    public class PluginSettingsGui : MyGuiScreenBase
    {
        private MyGuiControlLabel m_useGlobalSettignsLabel;
        private MyGuiControlLabel m_useSemiRandomGenerationLabel;
        private MyGuiControlLabel m_useVanillaPlanetsLabel;
        private MyGuiControlLabel m_planetsOnlyOnceLabel;
        private MyGuiControlLabel m_moonsOnlyOnceLabel;
        private MyGuiControlLabel m_planetGpsLabel;
        private MyGuiControlLabel m_moonGpsLabel;
        private MyGuiControlLabel m_beltGpsLabel;
        private MyGuiControlLabel m_ringGpsLabel;
        private MyGuiControlLabel m_asteroidGeneratorLabel;
        private MyGuiControlLabel m_asteroidDensityLabel;
        private MyGuiControlLabel m_objAmountLabel;
        private MyGuiControlLabel m_orbDistanceLabel;
        private MyGuiControlLabel m_sizeMultiplierLabel;
        private MyGuiControlLabel m_sizeCapLabel;
        private MyGuiControlLabel m_moonProbLabel;
        private MyGuiControlLabel m_ringWidthLabel;
        private MyGuiControlLabel m_ringProbLabel;
        private MyGuiControlLabel m_beltHeightLabel;
        private MyGuiControlLabel m_beltProbLabel;
        private MyGuiControlLabel m_worldSizeLabel;

        private MyGuiControlLabel m_objAmountValue;
        private MyGuiControlLabel m_asteroidDensityValue;
        private MyGuiControlLabel m_orbDistanceValue;
        private MyGuiControlLabel m_sizeMultiplierValue;
        private MyGuiControlLabel m_sizeCapValue;
        private MyGuiControlLabel m_moonProbValue;
        private MyGuiControlLabel m_ringWidthValue;
        private MyGuiControlLabel m_ringProbValue;
        private MyGuiControlLabel m_beltHeightValue;
        private MyGuiControlLabel m_beltProbValue;
        private MyGuiControlLabel m_worldSizeValue;

        private MyGuiControlCheckbox m_useGlobalCheck;
        private MyGuiControlCheckbox m_useSemiRandomGenerationCheck;
        private MyGuiControlCheckbox m_useVanillaPlanetsCheck;
        private MyGuiControlCheckbox m_planetsOnlyOnceCheck;
        private MyGuiControlCheckbox m_moonsOnlyOnceCheck;
        private MyGuiControlCheckbox m_planetGpsCheck;
        private MyGuiControlCheckbox m_moonGpsCheck;
        private MyGuiControlCheckbox m_beltGpsCheck;
        private MyGuiControlCheckbox m_ringGpsCheck;
        private MyGuiControlCombobox m_asteroidGeneratorCombo;
        private MyGuiControlClickableSlider m_asteroidDensitySlider;
        private MyGuiControlClickableSlider m_objAmountSlider;
        private MyGuiControlClickableSlider m_orbDistanceSlider;
        private MyGuiControlClickableSlider m_sizeMultiplierSlider;
        private MyGuiControlClickableSlider m_sizeCapSlider;
        private MyGuiControlClickableSlider m_moonProbSlider;
        private MyGuiControlClickableSlider m_ringWidthSlider;
        private MyGuiControlClickableSlider m_ringProbSlider;
        private MyGuiControlClickableSlider m_beltHeightSlider;
        private MyGuiControlClickableSlider m_beltProbSlider;
        private MyGuiControlClickableSlider m_worldSizeSlider;

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
            float x2 = 0.209375018f;
            int mod = m_isNewGame ? 6 : 0;
            AddCaption("SEWorldGenPlugin Settings", null, new Vector2(0f, 0.003f));

            MyGuiControlParent parent = new MyGuiControlParent(null, new Vector2(base.Size.Value.X - vector.X * 2f, 0.052f * (15 + mod)));
            parent.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;

            MyGuiControlSeparatorList myGuiControlSeparatorList = new MyGuiControlSeparatorList();
            myGuiControlSeparatorList.AddHorizontal(new Vector2(0f, 0f) - new Vector2(m_size.Value.X * 0.835f / 2f, m_size.Value.Y / 2f - 0.075f), m_size.Value.X * 0.835f);
            Controls.Add(myGuiControlSeparatorList);
            MyGuiControlSeparatorList myGuiControlSeparatorList2 = new MyGuiControlSeparatorList();
            myGuiControlSeparatorList2.AddHorizontal(new Vector2(0f, 0f) - new Vector2(m_size.Value.X * 0.835f / 2f, (0f - m_size.Value.Y) / 2f + 0.123f), m_size.Value.X * 0.835f);
            Controls.Add(myGuiControlSeparatorList2);

            m_useGlobalSettignsLabel = MakeLabel("Use global Config");
            m_useSemiRandomGenerationLabel = MakeLabel("Use all planets");
            m_useVanillaPlanetsLabel = MakeLabel("Use vanilla planets");
            m_planetsOnlyOnceLabel = MakeLabel("Generate Planets Once");
            m_moonsOnlyOnceLabel = MakeLabel("Generate Moons Once");
            m_planetGpsLabel = MakeLabel("Create GPS for Planets");
            m_moonGpsLabel = MakeLabel("Create GPS for Moons");
            m_beltGpsLabel = MakeLabel("Create GPS for Belts");
            m_ringGpsLabel = MakeLabel("Create GPS for Rings");
            m_asteroidGeneratorLabel = MakeLabel("Asteroid generator");
            m_asteroidDensityLabel = MakeLabel("Asteroid density");
            m_objAmountLabel = MakeLabel("Objects in System");
            m_orbDistanceLabel = MakeLabel("Average Orbit distance");
            m_sizeMultiplierLabel = MakeLabel("Planet size multiplier");
            m_sizeCapLabel = MakeLabel("Planet size cap");
            m_moonProbLabel = MakeLabel("Moon spawn probability");
            m_ringWidthLabel = MakeLabel("Average ring width");
            m_ringProbLabel = MakeLabel("Ring spawn probability");
            m_beltHeightLabel = MakeLabel("Average belt height");
            m_beltProbLabel = MakeLabel("Belt spawn probability");
            m_worldSizeLabel = MakeLabel("World Size");

            m_useGlobalCheck = new MyGuiControlCheckbox();
            m_useSemiRandomGenerationCheck = new MyGuiControlCheckbox();
            m_useVanillaPlanetsCheck = new MyGuiControlCheckbox();
            m_planetsOnlyOnceCheck = new MyGuiControlCheckbox();
            m_moonsOnlyOnceCheck = new MyGuiControlCheckbox();
            m_planetGpsCheck = new MyGuiControlCheckbox();
            m_moonGpsCheck = new MyGuiControlCheckbox();
            m_beltGpsCheck = new MyGuiControlCheckbox();
            m_ringGpsCheck = new MyGuiControlCheckbox();
            m_asteroidGeneratorCombo = new MyGuiControlCombobox(null, new Vector2(x2, 0.04f));
            x2 += 0.05f;
            m_asteroidDensitySlider = new MyGuiControlClickableSlider(Vector2.Zero, 0.1f, 1f, x2, 0.6f, null, null, 0, 0.8f, 0.05f, "White", MyPluginTexts.TOOLTIPS.ROID_DENS_SLIDER, MyGuiControlSliderStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER, intValue: false);
            m_objAmountSlider = new MyGuiControlClickableSlider(Vector2.Zero, 0f, 100f, x2, 15f, null, null, 0, 0.8f, 0.05f, "White", MyPluginTexts.TOOLTIPS.SYS_OBJ_SLIDER, MyGuiControlSliderStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER, intValue: true);
            m_orbDistanceSlider = new MyGuiControlClickableSlider(Vector2.Zero, 500f, 100000f, x2, 50500f, null, null, 0, 0.8f, 0.05f, "White", MyPluginTexts.TOOLTIPS.ORB_DIST_SLIDER, MyGuiControlSliderStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER, intValue: true);
            m_sizeMultiplierSlider = new MyGuiControlClickableSlider(Vector2.Zero, 1f, 10f, x2, 2f, null, null, 0, 0.8f, 0.05f, "White", MyPluginTexts.TOOLTIPS.SIZE_MUL_SLIDER, MyGuiControlSliderStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER, intValue: true);
            m_sizeCapSlider = new MyGuiControlClickableSlider(Vector2.Zero, 120f, 2400f, x2, 1200f, null, null, 0, 0.8f, 0.05f, "White", MyPluginTexts.TOOLTIPS.SIZE_CAP_SLIDER, MyGuiControlSliderStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER, intValue: true);
            m_moonProbSlider = new MyGuiControlClickableSlider(Vector2.Zero, 0f, 1f, x2, 0.5f, null, null, 0, 0.8f, 0.05f, "White", MyPluginTexts.TOOLTIPS.MOON_PROB_SLIDER, MyGuiControlSliderStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER, intValue: false);
            m_ringWidthSlider = new MyGuiControlClickableSlider(Vector2.Zero, 10000f, 100000f, x2, 15000f, null, null, 0, 0.8f, 0.05f, "White", MyPluginTexts.TOOLTIPS.RING_WIDTH_SLIDER, MyGuiControlSliderStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER, intValue: true);
            m_ringProbSlider = new MyGuiControlClickableSlider(Vector2.Zero, 0f, 1f, x2, 0.5f, null, null, 0, 0.8f, 0.05f, "White", MyPluginTexts.TOOLTIPS.RING_PROB_SLIDER, MyGuiControlSliderStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER, intValue: false);
            m_beltHeightSlider = new MyGuiControlClickableSlider(Vector2.Zero, 4000f, 40000f, x2, 22000f, null, null, 0, 0.8f, 0.05f, "White", MyPluginTexts.TOOLTIPS.BELT_HEIGHT_SLIDER, MyGuiControlSliderStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER, intValue: true);
            m_beltProbSlider = new MyGuiControlClickableSlider(Vector2.Zero, 0f, 1f, x2, 0.4f, null, null, 0, 0.8f, 0.05f, "White", MyPluginTexts.TOOLTIPS.BELT_PROB_SLIDER, MyGuiControlSliderStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER, intValue: false);
            m_worldSizeSlider = new MyGuiControlClickableSlider(Vector2.Zero, -1f, 1000000 - 1, x2, -1, null, null, 0, 0.8f, 0.05f, "White", MyPluginTexts.TOOLTIPS.WORLD_SIZE_SLIDER, MyGuiControlSliderStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER, intValue: true);

            m_asteroidDensityValue = MakeLabel(String.Format("{0:0.00}", m_asteroidDensitySlider.Value));
            m_asteroidDensityValue.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_CENTER;
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
            m_useSemiRandomGenerationCheck.SetToolTip(MyPluginTexts.TOOLTIPS.USE_SEMI_RAND_GEN_CHECK);
            m_useSemiRandomGenerationCheck.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER;
            m_useVanillaPlanetsCheck.SetToolTip(MyPluginTexts.TOOLTIPS.USE_VANILLA_PLANETS);
            m_useVanillaPlanetsCheck.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER;
            m_planetsOnlyOnceCheck.SetToolTip(MyPluginTexts.TOOLTIPS.PLANETS_ONLY_ONCE);
            m_planetsOnlyOnceCheck.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER;
            m_moonsOnlyOnceCheck.SetToolTip(MyPluginTexts.TOOLTIPS.MOONS_ONLY_ONCE);
            m_moonsOnlyOnceCheck.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER;
            m_planetGpsCheck.SetToolTip(MyPluginTexts.TOOLTIPS.PLANET_GPSL_CHECK);
            m_planetGpsCheck.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER;
            m_moonGpsCheck.SetToolTip(MyPluginTexts.TOOLTIPS.MOON_GPS_CHECK);
            m_moonGpsCheck.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER;
            m_beltGpsCheck.SetToolTip(MyPluginTexts.TOOLTIPS.BELT_GPS_CHECK);
            m_beltGpsCheck.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER;
            m_ringGpsCheck.SetToolTip(MyPluginTexts.TOOLTIPS.RING_GPS_CHECK);
            m_ringGpsCheck.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER;
            m_asteroidGeneratorCombo.SetToolTip(MyPluginTexts.TOOLTIPS.ROID_GEN_COMBO);
            m_asteroidGeneratorCombo.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER;
            m_worldSizeValue = MakeLabel(m_worldSizeSlider.Value.ToString());
            m_worldSizeValue.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_CENTER;

            m_asteroidGeneratorCombo.AddItem(0L, "Plugin");
            m_asteroidGeneratorCombo.AddItem(1L, "Vanilla");
            m_asteroidGeneratorCombo.AddItem(2L, "Mixed");
            m_asteroidGeneratorCombo.SelectItemByIndex(0);

            m_asteroidGeneratorCombo.ItemSelected += delegate
            {
                bool val = m_asteroidGeneratorCombo.GetSelectedKey() == 01L;

                m_beltGpsCheck.Enabled = !val;
                m_ringProbSlider.Enabled = !val;
                m_ringWidthSlider.Enabled = !val;
                m_beltHeightSlider.Enabled = !val;
                m_beltProbSlider.Enabled = !val;
                m_asteroidDensitySlider.Enabled = !val;
            };

            m_asteroidDensitySlider.ValueChanged = (Action<MyGuiControlSlider>)Delegate.Combine(m_asteroidDensitySlider.ValueChanged, (Action<MyGuiControlSlider>)delegate (MyGuiControlSlider s)
            {
                m_asteroidDensityValue.Text = String.Format("{0:0.00}", s.Value);
            });
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
                m_beltProbValue.Text = String.Format("{0:0.000}", s.Value);
            });
            m_worldSizeSlider.ValueChanged = (Action<MyGuiControlSlider>)Delegate.Combine(m_worldSizeSlider.ValueChanged, (Action<MyGuiControlSlider>)delegate (MyGuiControlSlider s)
            {
                if(s.Value <= 0)
                {
                    m_worldSizeValue.Text = "Infinite";
                }
                else
                {
                    m_worldSizeValue.Text = s.Value.ToString();
                }
            });
            m_useGlobalCheck.IsCheckedChanged = (Action<MyGuiControlCheckbox>)Delegate.Combine(m_useGlobalCheck.IsCheckedChanged, (Action<MyGuiControlCheckbox>)delegate (MyGuiControlCheckbox s)
            {
                m_useSemiRandomGenerationCheck.Enabled = !s.IsChecked;
                m_useVanillaPlanetsCheck.Enabled = !s.IsChecked;
                m_planetsOnlyOnceCheck.Enabled = !s.IsChecked;
                m_moonsOnlyOnceCheck.Enabled = !s.IsChecked;
                m_objAmountSlider.Enabled = !s.IsChecked;
                m_asteroidDensitySlider.Enabled = !s.IsChecked;
                m_asteroidGeneratorCombo.Enabled = !s.IsChecked;
                m_orbDistanceSlider.Enabled = !s.IsChecked;
                m_sizeMultiplierSlider.Enabled = !s.IsChecked;
                m_sizeCapSlider.Enabled = !s.IsChecked;
                m_moonProbSlider.Enabled = !s.IsChecked;
                m_ringWidthSlider.Enabled = !s.IsChecked;
                m_ringProbSlider.Enabled = !s.IsChecked;
                m_beltHeightSlider.Enabled = !s.IsChecked;
                m_beltProbSlider.Enabled = !s.IsChecked;
                m_planetGpsCheck.Enabled = !s.IsChecked;
                m_moonGpsCheck.Enabled = !s.IsChecked;
                m_beltGpsCheck.Enabled = !s.IsChecked;
                m_ringGpsCheck.Enabled = !s.IsChecked;
                m_worldSizeSlider.Enabled = !s.IsChecked && m_isNewGame;
            });

            parent.Controls.Add(m_useGlobalSettignsLabel);
            parent.Controls.Add(m_useGlobalCheck);

            if (m_isNewGame)
            {
                parent.Controls.Add(m_useSemiRandomGenerationLabel);
                parent.Controls.Add(m_useSemiRandomGenerationCheck);

                parent.Controls.Add(m_useVanillaPlanetsLabel);
                parent.Controls.Add(m_useVanillaPlanetsCheck);

                parent.Controls.Add(m_planetsOnlyOnceLabel);
                parent.Controls.Add(m_planetsOnlyOnceCheck);

                parent.Controls.Add(m_moonsOnlyOnceLabel);
                parent.Controls.Add(m_moonsOnlyOnceCheck);

                parent.Controls.Add(m_planetGpsLabel);
                parent.Controls.Add(m_planetGpsCheck);

                parent.Controls.Add(m_moonGpsLabel);
                parent.Controls.Add(m_moonGpsCheck);
            }

            parent.Controls.Add(m_beltGpsLabel);
            parent.Controls.Add(m_beltGpsCheck);

            parent.Controls.Add(m_ringGpsLabel);
            parent.Controls.Add(m_ringGpsCheck);

            parent.Controls.Add(m_asteroidGeneratorLabel);
            parent.Controls.Add(m_asteroidGeneratorCombo);

            parent.Controls.Add(m_asteroidDensityLabel);
            parent.Controls.Add(m_asteroidDensitySlider);
            parent.Controls.Add(m_asteroidDensityValue);

            parent.Controls.Add(m_objAmountLabel);
            parent.Controls.Add(m_objAmountSlider);
            parent.Controls.Add(m_objAmountValue);

            parent.Controls.Add(m_orbDistanceLabel);
            parent.Controls.Add(m_orbDistanceSlider);
            parent.Controls.Add(m_orbDistanceValue);

            parent.Controls.Add(m_sizeMultiplierLabel);
            parent.Controls.Add(m_sizeMultiplierSlider);
            parent.Controls.Add(m_sizeMultiplierValue);

            parent.Controls.Add(m_sizeCapLabel);
            parent.Controls.Add(m_sizeCapSlider);
            parent.Controls.Add(m_sizeCapValue);
            
            parent.Controls.Add(m_moonProbLabel);
            parent.Controls.Add(m_moonProbSlider);
            parent.Controls.Add(m_moonProbValue);
            
            parent.Controls.Add(m_ringWidthLabel);
            parent.Controls.Add(m_ringWidthSlider);
            parent.Controls.Add(m_ringWidthValue);
            
            parent.Controls.Add(m_ringProbLabel);
            parent.Controls.Add(m_ringProbSlider);
            parent.Controls.Add(m_ringProbValue);
            
            parent.Controls.Add(m_beltHeightLabel);
            parent.Controls.Add(m_beltHeightSlider);
            parent.Controls.Add(m_beltHeightValue);
            
            parent.Controls.Add(m_beltProbLabel);
            parent.Controls.Add(m_beltProbSlider);
            parent.Controls.Add(m_beltProbValue);

            parent.Controls.Add(m_worldSizeLabel);
            parent.Controls.Add(m_worldSizeSlider);
            parent.Controls.Add(m_worldSizeValue);

            //Vector2 start = (new Vector2(0f, (!m_isNewGame) ? 0.052f : 0.026f) - new Vector2(m_size.Value.X * 0.835f / 2f, m_size.Value.Y / 2f - 0.075f)) + (new Vector2(0f, m_useGlobalSettignsLabel.Size.Y));
            Vector2 start = Vector2.Zero - new Vector2(parent.Size.X / 2f, parent.Size.Y / 2f - 0.028f);
            Vector2 offset = new Vector2(0f, 0.050f);//0.028f
            Vector2 offset2 = new Vector2(m_orbDistanceLabel.Size.X * 1.5f, 0f);
            Vector2 offset3 = new Vector2(0.4973214f, 0f);
            int m = 0;

            m_useGlobalSettignsLabel.Position = start + offset * m++;
            m_useGlobalCheck.Position = m_useGlobalSettignsLabel.Position + offset2;

            if (m_isNewGame)
            {
                m_useSemiRandomGenerationLabel.Position = start + offset * m++;
                m_useSemiRandomGenerationCheck.Position = m_useSemiRandomGenerationLabel.Position + offset2;

                m_useVanillaPlanetsLabel.Position = start + offset * m++;
                m_useVanillaPlanetsCheck.Position = m_useVanillaPlanetsLabel.Position + offset2;

                m_planetsOnlyOnceLabel.Position = start + offset * m++;
                m_planetsOnlyOnceCheck.Position = m_planetsOnlyOnceLabel.Position + offset2;

                m_moonsOnlyOnceLabel.Position = start + offset * m++;
                m_moonsOnlyOnceCheck.Position = m_moonsOnlyOnceLabel.Position + offset2;

                m_planetGpsLabel.Position = start + offset * m++;
                m_planetGpsCheck.Position = m_planetGpsLabel.Position + offset2;

                m_moonGpsLabel.Position = start + offset * m++;
                m_moonGpsCheck.Position = m_moonGpsLabel.Position + offset2;
            }
            
            m_beltGpsLabel.Position = start + offset * m++;
            m_beltGpsCheck.Position = m_beltGpsLabel.Position + offset2;

            m_ringGpsLabel.Position = start + offset * m++;
            m_ringGpsCheck.Position = m_ringGpsLabel.Position + offset2;

            m_asteroidGeneratorLabel.Position = start + offset * m++;
            m_asteroidGeneratorCombo.Position = m_asteroidGeneratorLabel.Position + offset2;

            m_asteroidDensityLabel.Position = start + offset * m++;
            m_asteroidDensitySlider.Position = m_asteroidDensityLabel.Position + offset2;
            m_asteroidDensityValue.Position = m_asteroidDensityLabel.Position + offset3;

            m_objAmountLabel.Position = start + offset * m++;
            m_objAmountSlider.Position = m_objAmountLabel.Position + offset2;
            m_objAmountValue.Position = m_objAmountLabel.Position + offset3;

            m_orbDistanceLabel.Position = start + offset * m++;
            m_orbDistanceSlider.Position = m_orbDistanceLabel.Position + offset2;
            m_orbDistanceValue.Position = m_orbDistanceLabel.Position + offset3;

            m_sizeMultiplierLabel.Position = start + offset * m++;
            m_sizeMultiplierSlider.Position = m_sizeMultiplierLabel.Position + offset2;
            m_sizeMultiplierValue.Position = m_sizeMultiplierLabel.Position + offset3;

            m_sizeCapLabel.Position = start + offset * m++;
            m_sizeCapSlider.Position = m_sizeCapLabel.Position + offset2;
            m_sizeCapValue.Position = m_sizeCapLabel.Position + offset3;

            m_moonProbLabel.Position = start + offset * m++;
            m_moonProbSlider.Position = m_moonProbLabel.Position + offset2;
            m_moonProbValue.Position = m_moonProbLabel.Position + offset3;

            m_ringWidthLabel.Position = start + offset * m++;
            m_ringWidthSlider.Position = m_ringWidthLabel.Position + offset2;
            m_ringWidthValue.Position = m_ringWidthLabel.Position + offset3;

            m_ringProbLabel.Position = start + offset * m++;
            m_ringProbSlider.Position = m_ringProbLabel.Position + offset2;
            m_ringProbValue.Position = m_ringProbLabel.Position + offset3;

            m_beltHeightLabel.Position = start + offset * m++;
            m_beltHeightSlider.Position = m_beltHeightLabel.Position + offset2;
            m_beltHeightValue.Position = m_beltHeightLabel.Position + offset3;

            m_beltProbLabel.Position = start + offset * m++;
            m_beltProbSlider.Position = m_beltProbLabel.Position + offset2;
            m_beltProbValue.Position = m_beltProbLabel.Position + offset3;

            m_worldSizeLabel.Position = start + offset * m++;
            m_worldSizeSlider.Position = m_worldSizeLabel.Position + offset2;
            m_worldSizeValue.Position = m_worldSizeLabel.Position + offset3;

            m_okButton = new MyGuiControlButton(null, VRage.Game.MyGuiControlButtonStyleEnum.Default, null, null, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, null, VRage.MyTexts.Get(MyCommonTexts.Ok), 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, OkButtonClicked);
            m_okButton.SetToolTip(VRage.MyTexts.GetString(MySpaceTexts.ToolTipOptionsSpace_Ok));
            m_okButton.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_BOTTOM;
            m_okButton.Position = (m_size.Value / 2f - vector) * new Vector2(0f, 1f) + new Vector2(0f - 25f, 0f) / MyGuiConstants.GUI_OPTIMAL_SIZE;

            MyGuiControlScrollablePanel scrollPane = new MyGuiControlScrollablePanel(parent);
            scrollPane.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            scrollPane.ScrollbarVEnabled = true;
            scrollPane.Size = new Vector2(base.Size.Value.X - vector.X * 2f - 0.035f, 0.74f);
            scrollPane.Position = new Vector2(-0.27f, -0.394f);

            if (!m_isNewGame)
            {
                m_worldSizeSlider.Enabled = false;
            }

            Controls.Add(m_okButton);
            Controls.Add(scrollPane);
        }

        public void SetSettings(MyObjectBuilder_PluginSettings settings, bool useGlobal)
        {
            m_useGlobalCheck.IsChecked = useGlobal;

            if (useGlobal || settings == null) return;

            m_useSemiRandomGenerationCheck.IsChecked = settings.GeneratorSettings.SemiRandomizedGeneration;

            m_useVanillaPlanetsCheck.IsChecked = settings.GeneratorSettings.UseVanillaPlanets;

            m_planetsOnlyOnceCheck.IsChecked = settings.GeneratorSettings.PlanetsOnlyOnce;

            m_moonsOnlyOnceCheck.IsChecked = settings.GeneratorSettings.MoonsOnlyOnce;

            m_planetGpsCheck.IsChecked = settings.GeneratorSettings.PlanetSettings.ShowPlanetGPS;

            m_moonGpsCheck.IsChecked = settings.GeneratorSettings.PlanetSettings.ShowMoonGPS;

            m_beltGpsCheck.IsChecked = settings.GeneratorSettings.BeltSettings.ShowBeltGPS;

            m_ringGpsCheck.IsChecked = settings.GeneratorSettings.PlanetSettings.RingSettings.ShowRingGPS;

            m_asteroidGeneratorCombo.SelectItemByKey((int)settings.GeneratorSettings.AsteroidGenerator);

            m_asteroidDensitySlider.Value = settings.GeneratorSettings.AsteroidDensity;

            m_objAmountSlider.Value = (settings.GeneratorSettings.MinObjectsInSystem + settings.GeneratorSettings.MaxObjectsInSystem) / 2;

            m_orbDistanceSlider.Value = (settings.GeneratorSettings.MinOrbitDistance / 1000 + settings.GeneratorSettings.MaxOrbitDistance / 1000) / 2;

            m_sizeMultiplierSlider.Value = settings.GeneratorSettings.PlanetSettings.SizeMultiplier;

            m_sizeCapSlider.Value = (float)settings.GeneratorSettings.PlanetSettings.PlanetSizeCap / 1000;

            m_moonProbSlider.Value = settings.GeneratorSettings.PlanetSettings.MoonProbability;

            m_ringWidthSlider.Value = (settings.GeneratorSettings.PlanetSettings.RingSettings.MinPlanetRingWidth + settings.GeneratorSettings.PlanetSettings.RingSettings.MaxPlanetRingWidth) / 2;

            m_ringProbSlider.Value = settings.GeneratorSettings.PlanetSettings.RingSettings.PlanetRingProbability;

            m_beltHeightSlider.Value = (settings.GeneratorSettings.BeltSettings.MinBeltHeight + settings.GeneratorSettings.BeltSettings.MaxBeltHeight) / 2;
        
            m_worldSizeSlider.Value = settings.GeneratorSettings.WorldSize / 1000;
        }

        public bool GetSettings(ref MyObjectBuilder_PluginSettings settings)
        {
            if (settings == null)
                settings = new MyObjectBuilder_PluginSettings();

            settings.GeneratorSettings.SemiRandomizedGeneration = m_useSemiRandomGenerationCheck.IsChecked;
            settings.GeneratorSettings.UseVanillaPlanets = m_useVanillaPlanetsCheck.IsChecked;
            settings.GeneratorSettings.PlanetsOnlyOnce = m_planetsOnlyOnceCheck.IsChecked;
            settings.GeneratorSettings.MoonsOnlyOnce = m_moonsOnlyOnceCheck.IsChecked;

            settings.GeneratorSettings.MinObjectsInSystem = (int)m_objAmountSlider.Value;
            settings.GeneratorSettings.MaxObjectsInSystem = (int)m_objAmountSlider.Value;

            settings.GeneratorSettings.MinOrbitDistance = (int)m_orbDistanceSlider.Value * 1000 / 10;
            settings.GeneratorSettings.MaxOrbitDistance = (int)(m_orbDistanceSlider.Value * 1000 * 2 - settings.GeneratorSettings.MinOrbitDistance);

            settings.GeneratorSettings.AsteroidGenerator = (AsteroidGenerator)m_asteroidGeneratorCombo.GetSelectedKey();
            settings.GeneratorSettings.AsteroidDensity = m_asteroidDensitySlider.Value;

            settings.GeneratorSettings.PlanetSettings.SizeMultiplier = (int)m_sizeMultiplierSlider.Value;
            settings.GeneratorSettings.PlanetSettings.PlanetSizeCap = (int)m_sizeCapSlider.Value * 1000;
            settings.GeneratorSettings.PlanetSettings.MoonProbability = m_moonProbSlider.Value;
            settings.GeneratorSettings.PlanetSettings.ShowPlanetGPS = m_planetGpsCheck.IsChecked;
            settings.GeneratorSettings.PlanetSettings.ShowMoonGPS = m_moonGpsCheck.IsChecked;

            settings.GeneratorSettings.PlanetSettings.RingSettings.MinPlanetRingWidth = (int)m_ringWidthSlider.Value / 10;
            settings.GeneratorSettings.PlanetSettings.RingSettings.MaxPlanetRingWidth = (int)m_ringWidthSlider.Value * 2 - settings.GeneratorSettings.PlanetSettings.RingSettings.MinPlanetRingWidth;
            settings.GeneratorSettings.PlanetSettings.RingSettings.PlanetRingProbability = m_ringProbSlider.Value;
            settings.GeneratorSettings.PlanetSettings.RingSettings.ShowRingGPS = m_ringGpsCheck.IsChecked;

            settings.GeneratorSettings.BeltSettings.MinBeltHeight = (int)m_beltHeightSlider.Value / 10;
            settings.GeneratorSettings.BeltSettings.MaxBeltHeight = (int)m_beltHeightSlider.Value * 2 - settings.GeneratorSettings.BeltSettings.MinBeltHeight;
            settings.GeneratorSettings.BeltSettings.BeltProbability = m_beltProbSlider.Value;
            settings.GeneratorSettings.BeltSettings.ShowBeltGPS = m_beltGpsCheck.IsChecked;

            settings.GeneratorSettings.WorldSize = (long)m_worldSizeSlider.Value * 1000;

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
