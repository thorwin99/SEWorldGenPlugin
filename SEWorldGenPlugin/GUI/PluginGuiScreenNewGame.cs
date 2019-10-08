using Sandbox.Game.Localization;
using Sandbox.Game.Screens;
using Sandbox.Game.Screens.Helpers;
using Sandbox.Graphics.GUI;
using System.Text;
using VRage;

namespace SEWorldGenPlugin.GUI
{
    public class PluginGuiScreenNewGame : MyGuiScreenNewGame
    {
        public PluginGuiScreenNewGame() : base()
        {

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
