using Sandbox.Game.Gui;
using Sandbox.Graphics.GUI;
using System;
using System.Text;
using VRage.Game;
using VRageMath;

namespace SEWorldGenPlugin.GUI
{
    /// <summary>
    /// Helper class for creating various preset gui elements.
    /// </summary>
    public class MyPluginGuiHelper
    {

        /// <summary>
        /// Creates a debug style button. It is smaller than a nornmal button.
        /// </summary>
        /// <param name="usableWidth">Usable width of the button</param>
        /// <param name="text">Text of the button</param>
        /// <param name="onClick">On click event of the button</param>
        /// <param name="enabled">If it is enabled</param>
        /// <param name="tooltip">Tooltip for the button</param>
        /// <returns>The created button</returns>
        public static MyGuiControlButton CreateDebugButton(float usableWidth, string text, Action<MyGuiControlButton> onClick, bool enabled = true, string tooltip = null)
        {
            MyGuiControlButton myGuiControlButton = new MyGuiControlButton(null, VRage.Game.MyGuiControlButtonStyleEnum.Debug, onButtonClick: onClick);
            myGuiControlButton.VisualStyle = MyGuiControlButtonStyleEnum.Rectangular;
            myGuiControlButton.Size = new Vector2(usableWidth, myGuiControlButton.Size.Y);
            myGuiControlButton.Enabled = enabled;
            myGuiControlButton.Text = text;
            if (tooltip == null)
            {
                myGuiControlButton.SetToolTip(tooltip);
            }
            return myGuiControlButton;
        }

        /// <summary>
        /// Displays an error message box to the user and calls the given callback, when it is closed.
        /// </summary>
        /// <param name="error">The error message</param>
        /// <param name="caption">The caption of the box</param>
        /// <param name="callback">Callback to call when closed</param>
        public static void DisplayError(string error, string caption, Action callback = null)
        {
            MyGuiScreenMessageBox message = new MyGuiScreenMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.OK, new StringBuilder(error), new StringBuilder(caption), MyCommonTexts.Ok, MyCommonTexts.Cancel, MyCommonTexts.Yes, MyCommonTexts.No, null, 10000, MyGuiScreenMessageBox.ResultEnum.CANCEL, true, null, onClosing: callback); ;
            MyGuiSandbox.AddScreen(message);
        }
        /// <summary>
        /// Displays a message box to the user and calls the given callback, when it is closed.
        /// </summary>
        /// <param name="message">The message</param>
        /// <param name="caption">The caption of the box</param>
        /// <param name="callback">Callback to call when closed</param>
        public static void DisplayMessage(string message, string caption, Action callback = null)
        {
            MyGuiScreenMessageBox msg = new MyGuiScreenMessageBox(MyMessageBoxStyleEnum.Info, MyMessageBoxButtonsType.OK, new StringBuilder(message), new StringBuilder(caption), MyCommonTexts.Ok, MyCommonTexts.Cancel, MyCommonTexts.Yes, MyCommonTexts.No, null, 10000, MyGuiScreenMessageBox.ResultEnum.CANCEL, true, null, onClosing: callback); ;
            MyGuiSandbox.AddScreen(msg);
        }
    }
}
