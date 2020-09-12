using Sandbox.Game.Localization;
using Sandbox.Game.Screens;
using Sandbox.Game.Screens.Helpers;
using Sandbox.Graphics.GUI;
using System.Text;
using VRage;

namespace SEWorldGenPlugin.GUI
{
    /// <summary>
    /// Replacement for the new game screen to add plugin specific elements
    /// </summary>
    public class PluginGuiScreenNewGame : MyGuiScreenNewGame
    {
        public PluginGuiScreenNewGame() : base()
        {

        }

        public PluginGuiScreenNewGame(bool displayTabScenario = true, bool displayTabWorkshop = true, bool displayTabCustom = true) : base(displayTabScenario, displayTabWorkshop, displayTabCustom)
        {

        }

        /// <summary>
        /// Builds the gui and replaces the top buttons to change the type of world (custom, workshop, newgame)
        /// to open the plugins own new game screen, workshop game screen and campaign screen
        /// </summary>
        /// <param name="constructor">Whether or not it was called from constructor</param>
        public override void RecreateControls(bool constructor)
        {
            base.RecreateControls(constructor);

            MyGuiControlScreenSwitchPanel p = null;
            MyGuiControlButton custom = null;
            MyGuiControlButton newGame = null;
            MyGuiControlButton workshop = null;

            foreach (var c in Controls)
            {
                if(c is MyGuiControlScreenSwitchPanel)
                {
                    p = (MyGuiControlScreenSwitchPanel)c;
                    break;
                }
            }
            foreach(var c in p.Controls)
            {
                if(c is MyGuiControlButton)
                {
                    if (((MyGuiControlButton)c).Text == MyTexts.GetString(MyCommonTexts.ScreenCaptionCustomWorld))
                        custom = (MyGuiControlButton)c;
                    if (((MyGuiControlButton)c).Text == MyTexts.GetString(MyCommonTexts.ScreenCaptionNewGame))
                        newGame = (MyGuiControlButton)c;
                    if (((MyGuiControlButton)c).Text == MyTexts.GetString(MyCommonTexts.ScreenCaptionWorkshop))
                        workshop = (MyGuiControlButton)c;
                }
            }
            if(custom != null)
            {
                p.Controls.Remove(custom);
                MyGuiControlButton newButton = new MyGuiControlButton(custom.Position, custom.VisualStyle, custom.Size, custom.ColorMask, custom.OriginAlign, MyTexts.GetString(MySpaceTexts.ToolTipNewGame_CustomGame), new StringBuilder(custom.Text), custom.TextScale, custom.TextAlignment, custom.HighlightType, OnCustomWorldButtonClick, custom.CueEnum);
                p.Controls.Add(newButton);
            }
            if(newGame != null)
            {
                p.Controls.Remove(newGame);
                MyGuiControlButton newButton = new MyGuiControlButton(newGame.Position, newGame.VisualStyle, newGame.Size, newGame.ColorMask, newGame.OriginAlign, MyTexts.GetString(MySpaceTexts.ToolTipNewGame_Campaign), new StringBuilder(newGame.Text), newGame.TextScale, newGame.TextAlignment, newGame.HighlightType, OnCampaignButtonClick, newGame.CueEnum);
                p.Controls.Add(newButton);
                FocusedControl = newButton;
                newButton.HighlightType = MyGuiControlHighlightType.FORCED;
                newButton.HasHighlight = true;
                newButton.Selected = true;
                newButton.Name = "CampaignButton";
            }
            if (workshop != null)
            {
                p.Controls.Remove(workshop);
                MyGuiControlButton newButton = new MyGuiControlButton(workshop.Position, workshop.VisualStyle, workshop.Size, workshop.ColorMask, workshop.OriginAlign, MyTexts.GetString(MySpaceTexts.ToolTipNewGame_WorkshopContent), new StringBuilder(workshop.Text), workshop.TextScale, workshop.TextAlignment, workshop.HighlightType, OnWorkshopButtonClick, workshop.CueEnum);
                p.Controls.Add(newButton);
            }
        }

        /// <summary>
        /// Button callback for the custom world button, to open the custom world screen
        /// </summary>
        /// <param name="myGuiControlButton">Clicked button</param>
        private void OnCustomWorldButtonClick(MyGuiControlButton myGuiControlButton)
        {
            MyGuiScreenBase screenWithFocus = MyScreenManager.GetScreenWithFocus();
            if (!(screenWithFocus is MyGuiScreenWorldSettings))
            {
                SeamlesslyChangeScreen(screenWithFocus, new PluginWorldSettings());
            }
        }

        /// <summary>
        /// Button callback for the campaing button, to change to the campaign screen
        /// </summary>
        /// <param name="myGuiControlButton">Clicked button</param>
        private void OnCampaignButtonClick(MyGuiControlButton myGuiControlButton)
        {
            MyGuiScreenBase screenWithFocus = MyScreenManager.GetScreenWithFocus();
            if (!(screenWithFocus is MyGuiScreenNewGame))
            {
                SeamlesslyChangeScreen(screenWithFocus, new PluginGuiScreenNewGame());
            }
        }

        /// <summary>
        /// Button callback for the workshop world button, to open the workshop world screen
        /// </summary>
        /// <param name="myGuiControlButton">Clicked button</param>
        private void OnWorkshopButtonClick(MyGuiControlButton myGuiControlButton)
        {
            MyGuiScreenBase screenWithFocus = MyScreenManager.GetScreenWithFocus();
            if (!(screenWithFocus is MyGuiScreenNewWorkshopGame))
            {
                SeamlesslyChangeScreen(screenWithFocus, new PluginGuiScreenWorkshopGame());
            }
        }

        /// <summary>
        /// Seamlessly changes between two screens
        /// </summary>
        /// <param name="focusedScreen">Currently focused screen</param>
        /// <param name="exchangedFor">New screen</param>
        private static void SeamlesslyChangeScreen(MyGuiScreenBase focusedScreen, MyGuiScreenBase exchangedFor)
        {
            focusedScreen.SkipTransition = true;
            focusedScreen.CloseScreen();
            exchangedFor.SkipTransition = true;
            MyScreenManager.AddScreenNow(exchangedFor);
        }
    }
}
