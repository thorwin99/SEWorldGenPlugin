using Sandbox;
using Sandbox.Graphics.GUI;
using VRage.Input;
using VRage.Utils;
using VRageMath;

namespace SEWorldGenPlugin.GUI.Controls
{
    /// <summary>
    /// A GUI Control element, that is a slider to input
    /// numerical values and when clicked while holding ctrl
    /// it will open an input box, to input an exact number
    /// </summary>
    public class MyGuiControlClickableSlider : MyGuiControlSlider
    {
        public MyGuiControlClickableSlider(Vector2? position = null, float minValue = 0f, float maxValue = 1f, float width = 0.29f, float? defaultValue = null, Vector4? color = null, string labelText = null, int labelDecimalPlaces = 1, float labelScale = 0.8f, float labelSpaceWidth = 0f, string labelFont = "White", string toolTip = null, MyGuiControlSliderStyleEnum visualStyle = MyGuiControlSliderStyleEnum.Default, MyGuiDrawAlignEnum originAlign = MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, bool intValue = false, bool showLabel = false)
            : base(position, minValue, maxValue, width, defaultValue, color, labelText, labelDecimalPlaces, labelScale, labelSpaceWidth, labelFont, toolTip, visualStyle, originAlign, intValue, showLabel)
        {

        }

        /// <summary>
        /// Event handler for when the slider is clicked.
        /// Checks if ctrl is pressed simultaniously and opens the input field.
        /// </summary>
        /// <returns>If the click was sucessful</returns>
        protected override bool OnSliderClicked()
        {
            if (MyInput.Static.IsAnyCtrlKeyPressed())
            {
                MyGuiScreenDialogAmount dialog = new MyGuiScreenDialogAmount(this.MinValue, this.MaxValue, MyCommonTexts.DialogAmount_SetValueCaption, parseAsInteger: IntValue, defaultAmount: Value, backgroundTransition: MySandboxGame.Config.UIBkOpacity, guiTransition: MySandboxGame.Config.UIOpacity);
                dialog.OnConfirmed += ConfirmValue;
                MyGuiSandbox.AddScreen(dialog);
                return true;
            }
            return base.OnSliderClicked();
        }

        /// <summary>
        /// Sets the value of the slider to the one returned by the input box
        /// </summary>
        /// <param name="value">Value to set</param>
        private void ConfirmValue(float value)
        {
            Value = value;
            ValueChanged(this);
        }
    }
}
