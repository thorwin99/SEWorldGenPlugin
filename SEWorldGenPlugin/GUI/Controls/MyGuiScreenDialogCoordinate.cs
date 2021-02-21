using Sandbox.Graphics.GUI;
using System;
using System.Text;
using VRageMath;

namespace SEWorldGenPlugin.GUI.Controls
{
    /// <summary>
    /// Creates a dialog box to enter a coordinate
    /// </summary>
    public class MyGuiScreenDialogCoordinate : MyGuiScreenBase
    {
        /// <summary>
        /// Padding for the dialog box elements
        /// </summary>
        private readonly Vector2 PADDING = 20f / MyGuiConstants.GUI_OPTIMAL_SIZE;

        /// <summary>
        /// Caption of the dialog box
        /// </summary>
        private string m_caption;

        // Gui elements
        private MyGuiControlButton m_okButton;
        private MyGuiControlButton m_cancelButton;
        private MyGuiControlTextbox m_xBox;
        private MyGuiControlTextbox m_yBox;
        private MyGuiControlTextbox m_zBox;
        private MyGuiControlLabel m_errorLabel;

        /// <summary>
        /// Callback, when coordinate input is confirmed
        /// </summary>
        public event Action<Vector3D> OnConfirmed;

        /// <summary>
        /// Creates a new dialog box to enter a coordinate into
        /// </summary>
        /// <param name="caption">Caption of the box. The caption is shown on the top</param>
        /// <param name="backgroundTransition"></param>
        /// <param name="guiTransition"></param>
        public MyGuiScreenDialogCoordinate(string caption, float backgroundTransition = 0f, float guiTransition = 0f) : base(new Vector2(0.5f, 0.5f), MyGuiConstants.SCREEN_BACKGROUND_COLOR, new Vector2(87f / 175f, 147f / 524f), isTopMostScreen: false, null, backgroundTransition, guiTransition)
        {
            CanHideOthers = false;
            EnabledBackgroundFade = false;

            m_caption = caption;

            RecreateControls(true);
        }

        /// <summary>
        /// Refresh the controls and regenerate gui elements
        /// </summary>
        /// <param name="constructor">Is run from constructor</param>
        public override void RecreateControls(bool constructor)
        {
            base.RecreateControls(constructor);

            MyGuiControlLabel caption = new MyGuiControlLabel(null, null, m_caption);

            caption.OriginAlign = VRage.Utils.MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_TOP;
            caption.Position = new Vector2(0, Size.Value.Y / -2 + PADDING.Y);

            Controls.Add(caption);

            MyGuiControlParentTableLayout table = new MyGuiControlParentTableLayout(6, true, PADDING);

            table.AddTableSeparator();

            m_xBox = new MyGuiControlTextbox(type: MyGuiControlTextboxType.DigitsOnly);
            m_yBox = new MyGuiControlTextbox(type: MyGuiControlTextboxType.DigitsOnly);
            m_zBox = new MyGuiControlTextbox(type: MyGuiControlTextboxType.DigitsOnly);

            m_xBox.Size = new Vector2(0.1f, m_xBox.Size.Y);
            m_yBox.Size = new Vector2(0.1f, m_yBox.Size.Y);
            m_zBox.Size = new Vector2(0.1f, m_zBox.Size.Y);

            m_xBox.SetText(new StringBuilder("0"));
            m_yBox.SetText(new StringBuilder("0"));
            m_zBox.SetText(new StringBuilder("0"));

            table.AddTableRow(new MyGuiControlLabel(text: "X:"), m_xBox, new MyGuiControlLabel(text: "Y:"), m_yBox, new MyGuiControlLabel(text: "Z:"), m_zBox);

            table.AddTableSeparator();

            table.ApplyRows();

            table.Position = new Vector2(0, caption.Position.Y + caption.Size.Y + PADDING.Y);
            table.OriginAlign = VRage.Utils.MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_TOP;

            Controls.Add(table);

            m_okButton = new MyGuiControlButton();
            m_okButton.Text = MyCommonTexts.Ok.ToString();
            m_okButton.OriginAlign = VRage.Utils.MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_BOTTOM;
            m_okButton.Position = new Vector2(-PADDING.X, Size.Value.Y / 2 - PADDING.Y);
            m_okButton.ButtonClicked += delegate
            {
                var xb = new StringBuilder();
                var yb = new StringBuilder();
                var zb = new StringBuilder();
                double x = 0;
                double y = 0;
                double z = 0;
                m_xBox.GetText(xb);
                m_yBox.GetText(yb);
                m_zBox.GetText(zb);
                if (double.TryParse(xb.ToString(), out x) && double.TryParse(yb.ToString(), out y) && double.TryParse(zb.ToString(), out z))
                {
                    OnConfirmed?.Invoke(new Vector3D(x, y, z));
                    CloseScreen();
                }
                else
                {
                    m_errorLabel.Visible = true;
                }
            };

            m_cancelButton = new MyGuiControlButton();
            m_cancelButton.Text = MyCommonTexts.Cancel.ToString();
            m_cancelButton.OriginAlign = VRage.Utils.MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_BOTTOM;
            m_cancelButton.Position = new Vector2(PADDING.X, Size.Value.Y / 2 - PADDING.Y);
            m_cancelButton.ButtonClicked += delegate
            {
                CloseScreen();
            };

            Controls.Add(m_okButton);
            Controls.Add(m_cancelButton);


            m_errorLabel = new MyGuiControlLabel(null, null, "Those are not valid coordinates.", font: "Red");
            m_errorLabel.OriginAlign = VRage.Utils.MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            m_errorLabel.Position = new Vector2(Size.Value.X / -2 + PADDING.X, m_xBox.Position.Y + m_xBox.Size.Y);
            m_errorLabel.Visible = false;

            Controls.Add(m_errorLabel);
        }

        public override string GetFriendlyName()
        {
            return "MyGuiScreenDialogCoordinate";
        }
    }
}
