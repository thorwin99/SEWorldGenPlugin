using Sandbox;
using Sandbox.Graphics.GUI;
using VRage.Input;
using VRage.Utils;
using VRageMath;

namespace SEWorldGenPlugin.GUI.Controls
{
    public class MyGuiControlClickableSlider : MyGuiControlSlider
    {
        public MyGuiControlClickableSlider(Vector2? position = null, float minValue = 0f, float maxValue = 1f, float width = 0.29f, float? defaultValue = null, Vector4? color = null, string labelText = null, int labelDecimalPlaces = 1, float labelScale = 0.8f, float labelSpaceWidth = 0f, string labelFont = "White", string toolTip = null, MyGuiControlSliderStyleEnum visualStyle = MyGuiControlSliderStyleEnum.Default, MyGuiDrawAlignEnum originAlign = MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, bool intValue = false, bool showLabel = false)
            : base(position, minValue, maxValue, width, defaultValue, color, labelText, labelDecimalPlaces, labelScale, labelSpaceWidth, labelFont, toolTip, visualStyle, originAlign, intValue, showLabel)
        {

        }

        protected override bool OnSliderClicked()
        {
            if (MyInput.Static.IsAnyCtrlKeyPressed())
            {
                MyGuiScreenDialogAmount dialog = new MyGuiScreenDialogAmount(this.MinValue, this.MaxValue, MyCommonTexts.DialogAmount_SetValueCaption, parseAsInteger: IntValue, defaultAmount: Value, backgroundTransition: MySandboxGame.Config.UIBkOpacity, guiTransition: MySandboxGame.Config.UIOpacity);
                dialog.OnConfirmed += m_confirmValue;
                MyGuiSandbox.AddScreen(dialog);
                return true;
            }
            return base.OnSliderClicked();
        }

        private void m_confirmValue(float value)
        {
            Value = value;
            ValueChanged(this);
        }
    }
}
