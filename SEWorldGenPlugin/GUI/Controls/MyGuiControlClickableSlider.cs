using Sandbox;
using Sandbox.Graphics.GUI;
using System;
using VRage.Input;
using VRage.Utils;
using VRageMath;

namespace SEWorldGenPlugin.GUI.Controls
{
    /// <summary>
    /// A GUI Control element, that is a slider to input
    /// numerical values and when clicked while holding ctrl
    /// it will open an input box, to input an exact number.
    /// Also changes the label to always display the value of the slider.
    /// </summary>
    public class MyGuiControlClickableSlider : MyGuiControlSlider
    {

        /// <summary>
        /// Called after the label updated.
        /// </summary>
        public Action<MyGuiControlLabel> OnLabelUpdate;

        /// <summary>
        /// The new label to display the value of this slider
        /// </summary>
        MyGuiControlLabel m_label;

        /// <summary>
        /// The space between label and slider
        /// </summary>
        private float m_labelSpaceWidth;

        /// <summary>
        /// The prefix for the label
        /// </summary>
        private string m_labelPrefix;

        /// <summary>
        /// The suffix for the label
        /// </summary>
        private string m_labelSuffix;

        /// <summary>
        /// Creates a new clickable slider
        /// </summary>
        /// <param name="position">Position of slider</param>
        /// <param name="minValue">Minimum value of slider</param>
        /// <param name="maxValue">Maximum value of slider</param>
        /// <param name="width">Width of slider in screen space</param>
        /// <param name="defaultValue">Default value of slider</param>
        /// <param name="color">Color of slider</param>
        /// <param name="labelPrefix">Prefix for the value label</param>
        /// <param name="labelSuffix">Suffix for the value label</param>
        /// <param name="labelScale">Scale of the value label</param>
        /// <param name="labelSpaceWidth">Width of the value label</param>
        /// <param name="labelFont">Font of the value label</param>
        /// <param name="toolTip">Tooltip of this slider</param>
        /// <param name="visualStyle">VisualStyle</param>
        /// <param name="originAlign">Alignment of origin</param>
        /// <param name="intValue">Is int value</param>
        /// <param name="showLabel">Show the value label</param>
        public MyGuiControlClickableSlider(Vector2? position = null, float minValue = 0f, float maxValue = 1f, float width = 0.29f, float? defaultValue = null, Vector4? color = null, string labelPrefix = "", string labelSuffix = "", float labelScale = 0.8f, float labelSpaceWidth = 0f, string labelFont = "White", string toolTip = null, MyGuiControlSliderStyleEnum visualStyle = MyGuiControlSliderStyleEnum.Default, MyGuiDrawAlignEnum originAlign = MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, bool intValue = false, bool showLabel = false)
            : base(position, minValue, maxValue, width, defaultValue, color, null, 1, labelScale, labelSpaceWidth, labelFont, toolTip, visualStyle, originAlign, intValue, false)
        {
            m_labelSpaceWidth = labelSpaceWidth;
            m_labelPrefix = labelPrefix;
            m_labelSuffix = labelSuffix;
            m_label = new MyGuiControlLabel(null, null, "", null, labelScale, labelFont, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
            if (showLabel)
            {
                Elements.Add(m_label);
            }
            m_label.Position = new Vector2(GetPositionAbsoluteCenterLeft().X + Size.X + m_labelSpaceWidth, GetPositionAbsoluteCenterLeft().Y);

            ValueChanged += delegate (MyGuiControlSlider slider)
            {
                UpdateValueLabel();
            };

            UpdateValueLabel();
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
        /// Draws the control
        /// </summary>
        /// <param name="transitionAlpha"></param>
        /// <param name="backgroundTransitionAlpha"></param>
        public override void Draw(float transitionAlpha, float backgroundTransitionAlpha)
        {
            base.Draw(transitionAlpha, backgroundTransitionAlpha);
            UpdateValueLabel();
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

        /// <summary>
        /// Updates the label
        /// </summary>
        private void UpdateValueLabel()
        {
            if (IntValue)
            {
                m_label.Text = String.Format(m_labelPrefix + "{0:0}" + m_labelSuffix, (int)Value);
            }
            else
            {
                m_label.Text = String.Format(m_labelPrefix + "{0:0.00}" + m_labelSuffix, Value);
            }

            OnLabelUpdate?.Invoke(m_label);
        }
    }
}
