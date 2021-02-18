using Sandbox;
using Sandbox.Graphics;
using Sandbox.Graphics.GUI;
using SEWorldGenPlugin.Utilities;
using System;
using VRage.Input;
using VRage.Utils;
using VRageMath;

namespace SEWorldGenPlugin.GUI.Controls
{
    /// <summary>
    /// A class that represents a ranged slider. A ranged slider has
    /// 2 thumbs, that can be slided independently and define a range
    /// of values instead of one fixed one.
    /// </summary>
    public class MyGuiControlRangedSlider : MyGuiControlBase
    {
        private class MyGuiSliderThumb
        {
            /// <summary>
            /// The current texture of this thumb
            /// </summary>
            private string m_currentTexture;

            /// <summary>
            /// The current value of this thumb
            /// </summary>
            private double m_currentValue;

            /// <summary>
            /// The current value of this thumb
            /// </summary>
            public double CurrentValue
            { 
                get 
                { 
                    return m_currentValue;
                }
                set 
                {
                    m_currentValue = value;                
                }
            }

            /// <summary>
            /// The current position relative to the slider of this thumb in Screen space coordinates
            /// </summary>
            public Vector2 CurrentPosition;

            /// <summary>
            /// Creates a new slider thumb with the given start value
            /// </summary>
            /// <param name="defaultValue">The value to set the slider to in the beginning</param>
            public MyGuiSliderThumb(double defaultValue)
            {
                m_currentTexture = MyGuiConstants.TEXTURE_SLIDER_THUMB_DEFAULT.Normal;
                m_currentValue = defaultValue;
                CurrentPosition = Vector2.Zero;
            }

            /// <summary>
            /// Sets this thumb to be focused
            /// </summary>
            public void ForcusThumb()
            {
                m_currentTexture = MyGuiConstants.TEXTURE_SLIDER_THUMB_DEFAULT.Focus;
            }

            /// <summary>
            /// Resets the appearance of this thumb to its default
            /// </summary>
            public void ResetThumbAppearance()
            {
                m_currentTexture = MyGuiConstants.TEXTURE_SLIDER_THUMB_DEFAULT.Normal;
            }

            /// <summary>
            /// Highlights this thumb
            /// </summary>
            public void HighlightThumb()
            {
                m_currentTexture = MyGuiConstants.TEXTURE_SLIDER_THUMB_DEFAULT.Highlight;
            }

            /// <summary>
            /// Returns the bounds of the thumb
            /// </summary>
            /// <param name="parentTopLeft">The parents top left position</param>
            /// <returns>The bounds for this thumb on the given parent</returns>
            public BoundingBox2 GetBounds(Vector2 parentTopLeft)
            {
                Vector2 size = MyGuiConstants.TEXTURE_SLIDER_THUMB_DEFAULT.SizeGui;
                BoundingBox2 bounds = new BoundingBox2(parentTopLeft + CurrentPosition - size / 2, parentTopLeft + CurrentPosition + size / 2);
                return bounds;
            }

            /// <summary>
            /// Draws the thumb to the screen
            /// </summary>
            /// <param name="parentPosition">Position of the parent sliders top left</param>
            /// <param name="colorMask">Color mask of parent slider</param>
            /// <param name="enabled">Enabled bool of parent slider</param>
            /// <param name="transitionAlpha">transition alpha of parent slider</param>
            public void Draw(Vector2 parentPosition, Vector4 colorMask, bool enabled, float transitionAlpha)
            {
                MyGuiManager.DrawSpriteBatch(m_currentTexture, parentPosition + CurrentPosition, MyGuiConstants.TEXTURE_SLIDER_THUMB_DEFAULT.SizeGui, MyGuiControlBase.ApplyColorMaskModifiers(colorMask, enabled, transitionAlpha), MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER);
            }

            /// <summary>
            /// Whether this thumb is highlighted
            /// </summary>
            /// <returns></returns>
            public bool IsHighlighted()
            {
                return m_currentTexture == MyGuiConstants.TEXTURE_SLIDER_THUMB_DEFAULT.Highlight;
            }

            /// <summary>
            /// Whether this thumb is focused
            /// </summary>
            /// <returns></returns>
            public bool IsFocused()
            {
                return m_currentTexture == MyGuiConstants.TEXTURE_SLIDER_THUMB_DEFAULT.Focus;
            }
        }

        /// <summary>
        /// Action for changes of the value of this slider
        /// </summary>
        public Action<MyGuiControlRangedSlider> ValueChanged;

        /// <summary>
        /// Function to call when slider is clicked
        /// </summary>
        public Func<MyGuiControlRangedSlider, bool> SliderClicked;

        /// <summary>
        /// The current minimum range value
        /// </summary>
        public double CurrentMin => m_minThumb.CurrentValue;

        /// <summary>
        /// THe current maximum range value
        /// </summary>
        public double CurrentMax => m_maxThumb.CurrentValue;

        /// <summary>
        /// Label that shows the min value of this slider
        /// </summary>
        private MyGuiControlLabel m_minLabel;

        /// <summary>
        /// Label that shows the max value of this slider
        /// </summary>
        private MyGuiControlLabel m_maxLabel;

        /// <summary>
        /// Whether to show the label or not
        /// </summary>
        private bool m_showLabel;
        
        /// <summary>
        /// The space between labels and slider
        /// </summary>
        private float m_labelSpaceWidth;

        /// <summary>
        /// The min value of this slider
        /// </summary>
        private double m_minValue;

        /// <summary>
        /// The max value of this slider
        /// </summary>
        private double m_maxValue;

        /// <summary>
        /// The current texture for the rail
        /// </summary>
        private MyGuiCompositeTexture m_railTexture;

        /// <summary>
        /// The minimum thumb
        /// </summary>
        private MyGuiSliderThumb m_minThumb;

        /// <summary>
        /// The maximum thumb
        /// </summary>
        private MyGuiSliderThumb m_maxThumb;

        /// <summary>
        /// The currently focused thumb
        /// </summary>
        private MyGuiSliderThumb m_currentFocusThumb;

        /// <summary>
        /// Whether the slider should use ints or floats for values.
        /// </summary>
        private bool m_intMode;

        /// <summary>
        /// Whether logarithmic scaling is used instead of linear, to allow for very large ranges to set lower values.
        /// </summary>
        private bool m_logScale;

        /// <summary>
        /// Creates a new instance of a MyGuiControlRanged slider with given parameters
        /// </summary>
        /// <param name="position">The position of the slider in screenspace</param>
        /// <param name="width">The width of the slider in screenspace</param>
        /// <param name="minValue">The minimum value of the slider</param>
        /// <param name="maxValue">The maximum value of the slider</param>
        /// <param name="showLabel">Whether to show a label with the sliders values</param>
        /// <param name="labelSpaceWidth">The space between label and slider</param>
        /// <param name="labelScale">The size of the labels text</param>
        /// <param name="labelFont">The font of the label</param>
        /// <param name="toolTip">The tooltip for the slider</param>
        /// <param name="originAlign">The alignment of the slider</param>
        /// <param name="useLogScale">Whether to use logarithmic scaling of values on the slider to allow for wider ranges</param>
        /// <param name="defaultMax">Default max value</param>
        /// <param name="defaultMin">Default min value</param>
        /// <param name="intMode">If the slider should work in int mode</param>
        public MyGuiControlRangedSlider(double minValue, double maxValue, double defaultMin, double defaultMax, bool intMode = false, Vector2? position = null, float width = 0.29f, bool showLabel = true, float labelSpaceWidth = 0f, float labelScale = 0.8f, string labelFont = "White", string toolTip = null, MyGuiDrawAlignEnum originAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER, bool useLogScale = false) :
            base(position, null, null, toolTip, null, true, true, MyGuiControlHighlightType.WHEN_CURSOR_OVER_OR_FOCUS, originAlign)
        {
            m_showLabel = showLabel;
            m_minValue = Math.Min(minValue, maxValue);
            m_maxValue = Math.Max(maxValue, minValue);
            m_labelSpaceWidth = labelSpaceWidth;

            m_railTexture = MyGuiConstants.TEXTURE_SLIDER_RAIL;
            m_minThumb = new MyGuiSliderThumb(defaultMin);
            m_maxThumb = new MyGuiSliderThumb(defaultMax);

            m_intMode = intMode;

            m_logScale = useLogScale;

            m_minLabel = new MyGuiControlLabel(null, null, "", null, labelScale, labelFont, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_BOTTOM);
            m_maxLabel = new MyGuiControlLabel(null, null, "", null, labelScale, labelFont, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP);
            if (m_showLabel)
            {
                Elements.Add(m_minLabel);
                Elements.Add(m_maxLabel);
            }
            Size = new Vector2(width, Size.Y);
            UpdateLabels();
            RefreshInternals();
        }

        public override MyGuiControlBase HandleInput()
        {
            var ret =  base.HandleInput();
            if (ret != null) return ret;
            if (!Enabled) return null;

            bool controlCaptured = false;
            bool mouseDown = MyInput.Static.IsNewPrimaryButtonPressed();

            if (IsMouseOver && mouseDown && !OnSliderClicked())
            {
                controlCaptured = true;
                if (HandleClickOnThumb())
                {
                    return ret;
                }
            }
            if (m_currentFocusThumb != null)
            {
                controlCaptured = true;
            }
            if (MyInput.Static.IsNewPrimaryButtonReleased())
            {
                controlCaptured = false;
            }
            if (IsMouseOver || true)
            {
                if (controlCaptured)
                {
                    var mousePos = MyGuiManager.MouseCursorPosition;
                    float startX = MyGuiConstants.SLIDER_INSIDE_OFFSET_X;
                    float endX = Size.X - MyGuiConstants.SLIDER_INSIDE_OFFSET_X;
                    float centerY = Size.Y / 2;

                    if(m_currentFocusThumb == null && m_maxThumb.GetBounds(GetPositionAbsoluteTopLeft()).Contains(mousePos) != ContainmentType.Disjoint)
                    {
                        m_currentFocusThumb = m_maxThumb;
                        m_maxThumb.ForcusThumb();
                        m_minThumb.ResetThumbAppearance();
                    }
                    else if (m_currentFocusThumb == null && m_minThumb.GetBounds(GetPositionAbsoluteTopLeft()).Contains(mousePos) != ContainmentType.Disjoint)
                    {
                        m_currentFocusThumb = m_minThumb;
                        m_minThumb.ForcusThumb();
                        m_maxThumb.ResetThumbAppearance();
                    }

                    if (m_currentFocusThumb != null)
                    {
                        float mouseRatio = (mousePos.X - GetPositionAbsoluteTopLeft().X) / (GetPositionAbsoluteTopRight().X - GetPositionAbsoluteTopLeft().X);

                        if (m_logScale)
                        {
                            mouseRatio = (float)Math.Pow(mouseRatio, 3);
                        }

                        double mouseVal = (m_maxValue - m_minValue) * mouseRatio + m_minValue;

                        if(m_currentFocusThumb == m_minThumb)
                        {
                            m_currentFocusThumb.CurrentValue = MathHelper.Clamp(mouseVal, m_minValue, m_maxThumb.CurrentValue);
                            if (m_intMode)
                                m_currentFocusThumb.CurrentValue = (int)m_currentFocusThumb.CurrentValue;
                        }
                        else
                        {
                            m_currentFocusThumb.CurrentValue = MathHelper.Clamp(mouseVal, m_minThumb.CurrentValue, m_maxValue);
                            if (m_intMode)
                                m_currentFocusThumb.CurrentValue = (int)m_currentFocusThumb.CurrentValue;
                        }

                        float currentRatio = (float)((m_currentFocusThumb.CurrentValue - m_minValue) / (m_maxValue - m_minValue));

                        if (m_logScale)
                        {
                            currentRatio = (float)Math.Pow(currentRatio, 1f / 3f);
                        }

                        m_currentFocusThumb.CurrentPosition = new Vector2(startX + (endX - startX) * currentRatio, centerY);
                    }
                }
                else
                {
                    m_currentFocusThumb = null;
                    RefreshInternals();
                }
            }

            return ret;
        }

        /// <summary>
        /// Handles a ctrl click on one of the thumbs to open a direct input window.
        /// </summary>
        private bool HandleClickOnThumb()
        {
            if (!MyInput.Static.IsAnyCtrlKeyPressed() || !MyInput.Static.IsNewPrimaryButtonPressed()) return false;

            double min = 0;
            double max = 0;
            double current = 0;
            MyGuiSliderThumb clickedThumb = null;

            if (m_minThumb.IsHighlighted() || m_minThumb.IsFocused())
            {
                min = m_minValue;
                max = m_maxThumb.CurrentValue;
                current = m_minThumb.CurrentValue;
                clickedThumb = m_minThumb;
            }
            if (m_maxThumb.IsHighlighted() || m_maxThumb.IsFocused())
            {
                min = m_minThumb.CurrentValue;
                max = m_maxValue;
                current = m_maxThumb.CurrentValue;
                clickedThumb = m_maxThumb;
            }
            if (clickedThumb == null) return false;

            MyGuiScreenDialogAmount dialog = new MyGuiScreenDialogAmount((float)min, (float)max, MyCommonTexts.DialogAmount_SetValueCaption, parseAsInteger: m_intMode, defaultAmount: (float)current, backgroundTransition: MySandboxGame.Config.UIBkOpacity, guiTransition: MySandboxGame.Config.UIOpacity);
            dialog.OnConfirmed += delegate (float value)
            {
                clickedThumb.CurrentValue = value;
                RefreshInternals();
            };

            MyGuiSandbox.AddScreen(dialog);

            return true;
        }

        /// <summary>
        /// Updates the labels for this slider
        /// </summary>
        protected void UpdateLabels()
        {
            if (m_intMode)
            {
                m_minLabel.Text = String.Format("Min: {0:0}", m_minThumb.CurrentValue);
                m_maxLabel.Text = String.Format("Max: {0:0}", m_maxThumb.CurrentValue);
            }
            else
            {
                m_minLabel.Text = String.Format("Min: {0:0.00}", m_minThumb.CurrentValue);
                m_maxLabel.Text = String.Format("Max: {0:0.00}", m_maxThumb.CurrentValue);
            }
        }

        /// <summary>
        /// Refreshes textures and sizes
        /// </summary>
        private void RefreshInternals()
        {
            if (HasHighlight)
            {
                m_railTexture = MyGuiConstants.TEXTURE_SLIDER_RAIL_HIGHLIGHT;
                if(m_maxThumb.GetBounds(GetPositionAbsoluteTopLeft()).Contains(MyGuiManager.MouseCursorPosition) != ContainmentType.Disjoint)
                {
                    m_maxThumb.HighlightThumb();
                }
                else
                {
                    m_maxThumb.ResetThumbAppearance();
                }
                if (m_minThumb.GetBounds(GetPositionAbsoluteTopLeft()).Contains(MyGuiManager.MouseCursorPosition) != ContainmentType.Disjoint)
                {
                    m_minThumb.HighlightThumb();
                }
                else
                {
                    m_minThumb.ResetThumbAppearance();
                }
            }
            else if (HasFocus)
            {
                m_railTexture = MyGuiConstants.TEXTURE_SLIDER_RAIL_FOCUS;
            }
            else
            {
                m_railTexture = MyGuiConstants.TEXTURE_SLIDER_RAIL;
                m_minThumb.ResetThumbAppearance();
                m_maxThumb.ResetThumbAppearance();
            }

            MinSize = new Vector2(m_railTexture.MinSizeGui.X + m_labelSpaceWidth, Math.Max(m_railTexture.MinSizeGui.Y, m_minLabel.Size.Y + m_maxLabel.Size.Y));
            MaxSize = new Vector2(m_railTexture.MaxSizeGui.X + m_labelSpaceWidth, Math.Max(m_railTexture.MaxSizeGui.Y, m_minLabel.Size.Y + m_maxLabel.Size.Y));
            m_minLabel.Position = new Vector2(Size.X * 0.5f, 0f);
            m_maxLabel.Position = new Vector2(Size.X * 0.5f, 0f);

            float startX = MyGuiConstants.SLIDER_INSIDE_OFFSET_X;
            float endX = Size.X - MyGuiConstants.SLIDER_INSIDE_OFFSET_X;
            float centerY = Size.Y / 2;
            float minRatio = (float)((m_minThumb.CurrentValue - m_minValue) / (m_maxValue - m_minValue));
            float maxRatio = (float)((m_maxThumb.CurrentValue - m_minValue) / (m_maxValue - m_minValue));

            if (m_logScale)
            {
                minRatio = (float)Math.Pow(minRatio, 1f / 3f);
                maxRatio = (float)Math.Pow(maxRatio, 1f / 3f);
            }

            m_minThumb.CurrentPosition = new Vector2(MathHelper.Lerp(startX, endX, minRatio), centerY);
            m_maxThumb.CurrentPosition = new Vector2(MathHelper.Lerp(startX, endX, maxRatio), centerY);
        }

        /// <summary>
        /// Sets the range to both values, if they fit in the min and max range for the slider.
        /// </summary>
        /// <param name="v1">First value</param>
        /// <param name="v2">Second value</param>
        public void SetValues(double v1, double v2)
        {
            m_minThumb.CurrentValue = Math.Max(Math.Min(v1, v2), m_minValue);
            m_maxThumb.CurrentValue = Math.Min(Math.Max(v1, v2), m_maxValue);

            RefreshInternals();
        }

        /// <summary>
        /// Sets the min and max range to the given values. The current values will be reset
        /// to v1 and v2
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        public void SetRange(double v1, double v2)
        {
            m_minValue = Math.Min(v1, v2);
            m_maxValue = Math.Max(v1, v2);

            SetValues(v1, v2);
        }

        private bool OnSliderClicked()
        {
            if(SliderClicked != null)
            {
                return SliderClicked(this);
            }

            return false;
        }

        public override void Draw(float transitionAlpha, float backgroundTransitionAlpha)
        {
            base.Draw(transitionAlpha, backgroundTransitionAlpha);
            m_railTexture.Draw(GetPositionAbsoluteTopLeft(), Size - new Vector2(m_labelSpaceWidth, 0f), ApplyColorMaskModifiers(ColorMask, Enabled, transitionAlpha), 1);
            DrawThumbs(transitionAlpha);
            if (m_showLabel)
            {
                m_minLabel.Draw(transitionAlpha, backgroundTransitionAlpha);
                m_maxLabel.Draw(transitionAlpha, backgroundTransitionAlpha);
            }

            UpdateLabels();
        }

        /// <summary>
        /// Draws the both thumbs of the slider
        /// </summary>
        /// <param name="transitionAlpha">transition alpha</param>
        private void DrawThumbs(float transitionAlpha)
        {
            m_minThumb.Draw(GetPositionAbsoluteTopLeft(), ColorMask, Enabled, transitionAlpha);
            m_maxThumb.Draw(GetPositionAbsoluteTopLeft(), ColorMask, Enabled, transitionAlpha);
        }

        protected override void OnHasHighlightChanged()
        {
            base.OnHasHighlightChanged();
            RefreshInternals();
        }

        protected override void OnSizeChanged()
        {
            base.OnSizeChanged();
            RefreshInternals();
        }

        public override void OnFocusChanged(MyGuiControlBase control, bool focus)
        {
            base.OnFocusChanged(control, focus);
            RefreshInternals();
        }
    }
}
