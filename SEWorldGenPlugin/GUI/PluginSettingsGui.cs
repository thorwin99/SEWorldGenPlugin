using Sandbox;
using Sandbox.Graphics.GUI;
using SEWorldGenPlugin.ObjectBuilders;
using VRageMath;

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
        private MyGuiControlLabel m_beltHeightLabel;
        private MyGuiControlLabel m_beltProbLabel;

        //TODO: Add min max
        private MyGuiControlCheckbox m_useGlobalCheck;
        private MyGuiControlSlider m_objAmountSlider;
        private MyGuiControlSlider m_orbDistanceSlider;
        private MyGuiControlSlider m_sizeMultiplierSlider;
        private MyGuiControlSlider m_sizeCapSlider;
        private MyGuiControlSlider m_moonProbSlider;
        private MyGuiControlSlider m_rindgWidthSlider;
        private MyGuiControlSlider m_beltHeightSlider;
        private MyGuiControlSlider m_beltProbSlider;

        private PluginWorldSettings m_parent;

        private bool m_isNewGame;
        private bool m_isConfirmed;

        public PluginSettingsGui(PluginWorldSettings parent) : base(new Vector2(0.5f, 0.5f), MyGuiConstants.SCREEN_BACKGROUND_COLOR, CalcSize(), isTopMostScreen: false, null, MySandboxGame.Config.UIBkOpacity, MySandboxGame.Config.UIOpacity)
        {
            m_parent = parent;
            base.EnabledBackgroundFade = true;
            m_isNewGame = (parent.Checkpoint == null);
            m_isConfirmed = false;
            RecreateControls(true);
        }

        public override void RecreateControls(bool constructor)
        {
            base.RecreateControls(constructor);
            BuildControls();
            SetSettings(m_parent.PluginSettings);
            MyGuiControl
        }

        public void BuildControls()
        {

        }

        public void SetSettings(MyObjectBuilder_PluginSettings settings)
        {
        }

        public override string GetFriendlyName()
        {
            return "PluginSettingsGui";
        }

        public static Vector2 CalcSize()
        {
            return new Vector2(183f / 280f, 0.9398855f);
        }
    }
}
