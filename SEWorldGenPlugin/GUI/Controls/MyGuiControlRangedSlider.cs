using Sandbox;
using Sandbox.Graphics;
using Sandbox.Graphics.GUI;
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
            private float m_currentValue;

            /// <summary>
            /// The current value of this thumb
            /// </summary>
            public float CurrentValue
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
            /// The current position in screen space of this thumb
            /// </summary>
            public Vector2 CurrentPosition;

            /// <summary>
            /// Creates a new slider thumb with the given start value
            /// </summary>
            /// <param name="defaultValue">The value to set the slider to in the beginning</param>
            public MyGuiSliderThumb(float defaultValue)
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
            /// <returns></returns>
            public BoundingBox2 GetBounds()
            {
                Vector2 size = MyGuiConstants.TEXTURE_SLIDER_THUMB_DEFAULT.NormalSize;
                BoundingBox2 bounds = new BoundingBox2(CurrentPosition - size / 2, CurrentPosition + size / 2);
                return bounds;
            }

            /// <summary>
            /// Draws the thumb to the screen
            /// </summary>
            /// <param name="colorMask">Color mask of parent slider</param>
            /// <param name="enabled">Enabled bool of parent slider</param>
            /// <param name="transitionAlpha">transition alpha of parent slider</param>
            public void Draw(Vector4 colorMask, bool enabled, float transitionAlpha)
            {
                MyGuiManager.DrawSpriteBatch(m_currentTexture, CurrentPosition, MyGuiConstants.TEXTURE_SLIDER_THUMB_DEFAULT.SizeGui, MyGuiControlBase.ApplyColorMaskModifiers(colorMask, enabled, transitionAlpha), MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER);
            }

            /// <summary>
            /// Whether this thumb is highlighted
            /// </summary>
            /// <returns></returns>
            public bool IsHighlighted()
            {
                return m_currentTexture == MyGuiConstants.TEXTURE_SLIDER_THUMB_DEFAULT.Highlight;
            }
        }

        /// <summary>
        /// Action for changes of the value of this slider
        /// </summary>
        public Action<MyGuiControlRangedSlider> ValueChanged;

        public Func<MyGuiControlRangedSlider, bool> SliderClicked;

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
        private float m_minValue;

        /// <summary>
        /// The max value of this slider
        /// </summary>
        private float m_maxValue;

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
        public MyGuiControlRangedSlider(float minValue, float maxValue, float defaultMin, float defaultMax, Vector2? position = null, float width = 0.29f, bool showLabel = true, float labelSpaceWidth = 0f, float labelScale = 0.8f, string labelFont = "White", string toolTip = null, MyGuiDrawAlignEnum originAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER) :
            base(position, null, null, toolTip, null, true, true, MyGuiControlHighlightType.WHEN_CURSOR_OVER_OR_FOCUS, originAlign)
        {
            m_showLabel = showLabel;
            m_minValue = Math.Min(minValue, maxValue);
            m_maxValue = Math.Max(maxValue, minValue);
            m_labelSpaceWidth = labelSpaceWidth;

            m_railTexture = MyGuiConstants.TEXTURE_SLIDER_RAIL;
            m_minThumb = new MyGuiSliderThumb(defaultMin);
            m_maxThumb = new MyGuiSliderThumb(defaultMax);

            m_minLabel = new MyGuiControlLabel(null, null, "", null, labelScale, labelFont, MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_BOTTOM);
            m_maxLabel = new MyGuiControlLabel(null, null, "", null, labelScale, labelFont, MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP);
            if (m_showLabel)
            {
                Elements.Add(m_minLabel);
                Elements.Add(m_maxLabel);
            }
            Size = new Vector2(width, base.Size.Y);
            UpdateLabels();
        }

        public override MyGuiControlBase HandleInput()
        {
            var ret =  base.HandleInput();
            if (ret != null) return ret;
            if (!Enabled) return null;

            bool controlCaptured = true;
            if(IsMouseOver && MyInput.Static.IsNewPrimaryButtonPressed() && !OnSliderClicked())
            {
                controlCaptured = true;
                HandleClickOnThumb();
            }
            if (MyInput.Static.IsNewPrimaryButtonReleased())
            {
                controlCaptured = false;
            }
            if (IsMouseOver)
            {
                if (controlCaptured)
                {
                    var mousePos = MyGuiManager.MouseCursorPosition;
                    float startX = GetPositionAbsoluteTopLeft().X + MyGuiConstants.SLIDER_INSIDE_OFFSET_X;
                    float endX = GetPositionAbsoluteTopRight().X - MyGuiConstants.SLIDER_INSIDE_OFFSET_X;
                    float centerY = GetPositionAbsoluteTopLeft().Y + Size.Y / 2;
                    float minRatio = (m_minThumb.CurrentValue - m_minValue) / (m_maxValue - m_minValue);
                    float maxRatio = (m_maxThumb.CurrentValue - m_minValue) / (m_maxValue - m_minValue);

                    Vector2 minPos = new Vector2(MathHelper.Lerp(startX, endX, minRatio), centerY);
                    Vector2 maxPos = new Vector2(MathHelper.Lerp(startX, endX, maxRatio), centerY);
                    m_minThumb.CurrentPosition = minPos;
                    m_maxThumb.CurrentPosition = maxPos;

                    if (m_minThumb.GetBounds().Contains(mousePos) == ContainmentType.Contains)
                    {
                        m_currentFocusThumb = m_minThumb;
                        m_minThumb.ForcusThumb();
                        m_maxThumb.ResetThumbAppearance();
                    }
                    else if(m_maxThumb.GetBounds().Contains(mousePos) == ContainmentType.Contains)
                    {
                        m_currentFocusThumb = m_maxThumb;
                        m_maxThumb.ForcusThumb();
                        m_minThumb.ResetThumbAppearance();
                    }

                    if(m_currentFocusThumb != null)
                    {
                        float mouseRatio = (mousePos.X - startX) / (endX - startX);
                        float mouseVal = (m_maxValue - m_minValue) * mouseRatio + m_minValue;
                        if(m_currentFocusThumb == m_minThumb)
                        {
                            m_currentFocusThumb.CurrentValue = MathHelper.Clamp(mouseVal, m_minValue, m_maxThumb.CurrentValue);
                        }
                        else
                        {
                            m_currentFocusThumb.CurrentValue = MathHelper.Clamp(mouseVal, m_minThumb.CurrentValue, m_maxValue);
                        }

                        float currentRatio = (m_currentFocusThumb.CurrentValue - m_minValue) / m_maxValue - m_minValue;
                        m_currentFocusThumb.CurrentPosition = new Vector2(startX + (endX - startX) * currentRatio, centerY);
                    }
                }
                else
                {
                    m_currentFocusThumb = null;
                    m_minThumb.ResetThumbAppearance();
                    m_maxThumb.ResetThumbAppearance();
                }
            }

            return ret;
        }

        /// <summary>
        /// Handles a ctrl click on one of the thumbs to open a direct input window.
        /// </summary>
        private void HandleClickOnThumb()
        {
            if (!MyInput.Static.IsAnyCtrlKeyPressed() || !MyInput.Static.IsNewPrimaryButtonPressed()) return;

            float min = 0;
            float max = 0;
            float current = 0;
            MyGuiSliderThumb clickedThumb = null;

            if (m_minThumb.IsHighlighted())
            {
                min = m_minValue;
                max = m_maxThumb.CurrentValue;
                current = m_minThumb.CurrentValue;
                clickedThumb = m_minThumb;
            }
            if (m_maxThumb.IsHighlighted())
            {
                min = m_minThumb.CurrentValue;
                max = m_maxValue;
                current = m_maxThumb.CurrentValue;
                clickedThumb = m_minThumb;
            }
            if (clickedThumb == null) return;

            MyGuiScreenDialogAmount dialog = new MyGuiScreenDialogAmount(min, max, MyCommonTexts.DialogAmount_SetValueCaption, parseAsInteger: false, defaultAmount: current, backgroundTransition: MySandboxGame.Config.UIBkOpacity, guiTransition: MySandboxGame.Config.UIOpacity);
            dialog.OnConfirmed += delegate (float value)
            {
                clickedThumb.CurrentValue = value;
                RefreshInternals();
            };
        }

        /// <summary>
        /// Updates the labels for this slider
        /// </summary>
        protected void UpdateLabels()
        {
            m_minLabel.Text = String.Format("Min: {0:0.00}", m_minValue);
            m_maxLabel.Text = String.Format("Max: {0:0.00}", m_maxValue);
            RefreshInternals();
        }

        /// <summary>
        /// Refreshes textures and sizes
        /// </summary>
        private void RefreshInternals()
        {
            if (HasHighlight)
            {
                m_railTexture = MyGuiConstants.TEXTURE_SLIDER_RAIL_HIGHLIGHT;
                if(m_maxThumb.GetBounds().Contains(MyGuiManager.MouseCursorPosition) == ContainmentType.Contains)
                {
                    m_maxThumb.HighlightThumb();
                }
                if (m_minThumb.GetBounds().Contains(MyGuiManager.MouseCursorPosition) == ContainmentType.Contains)
                {
                    m_maxThumb.HighlightThumb();
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

            float startX = GetPositionAbsoluteTopLeft().X + MyGuiConstants.SLIDER_INSIDE_OFFSET_X;
            float endX = GetPositionAbsoluteTopRight().X - MyGuiConstants.SLIDER_INSIDE_OFFSET_X;
            float centerY = GetPositionAbsoluteTopLeft().Y + Size.Y / 2;
            float minRatio = (m_minThumb.CurrentValue - m_minValue) / (m_maxValue - m_minValue);
            float maxRatio = (m_maxThumb.CurrentValue - m_minValue) / (m_maxValue - m_minValue);

            m_minThumb.CurrentPosition = new Vector2(MathHelper.Lerp(startX, endX, minRatio), centerY);
            m_maxThumb.CurrentPosition = new Vector2(MathHelper.Lerp(startX, endX, maxRatio), centerY);
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
            m_railTexture.Draw(GetPositionAbsoluteTopLeft(), Size - new Vector2(m_labelSpaceWidth, 0f), MyGuiControlBase.ApplyColorMaskModifiers(ColorMask, Enabled, transitionAlpha), 1);
            DrawThumbs(transitionAlpha);
            if (m_showLabel)
            {
                m_minLabel.Draw(transitionAlpha, backgroundTransitionAlpha);
                m_maxLabel.Draw(transitionAlpha, backgroundTransitionAlpha);
            }
        }

        /// <summary>
        /// Draws the both thumbs of the slider
        /// </summary>
        /// <param name="transitionAlpha">transition alpha</param>
        private void DrawThumbs(float transitionAlpha)
        {
            float centerY = GetPositionAbsoluteTopLeft().Y + Size.Y / 2;
            float start = GetPositionAbsoluteTopLeft().X + MyGuiConstants.SLIDER_INSIDE_OFFSET_X;
            float end = GetPositionAbsoluteTopLeft().X + (Size.X - (MyGuiConstants.SLIDER_INSIDE_OFFSET_X + m_labelSpaceWidth));

            m_minThumb.Draw(ColorMask, Enabled, transitionAlpha);
            m_maxThumb.Draw(ColorMask, Enabled, transitionAlpha);
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
