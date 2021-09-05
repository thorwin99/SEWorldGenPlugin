using Sandbox.Graphics.GUI;
using System;
using System.Collections.Generic;
using VRageMath;

namespace SEWorldGenPlugin.GUI.Controls
{

    /// <summary>
    /// Gui dialog to display a combobox selection
    /// </summary>
    public class MyGuiScreenDialogCombobox : MyGuiScreenBase
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
        private MyGuiControlCombobox m_comboBox;

        /// <summary>
        /// The tooltip of the combobox
        /// </summary>
        private string m_tooltip;

        /// <summary>
        /// The entries for the combobox
        /// </summary>
        private List<string> m_entries;

        /// <summary>
        /// event called when the input was confirmed
        /// </summary>
        public event Action<long, string> OnConfirm;

        /// <summary>
        /// Creates a new dialog box to select from a combobox with the given entries
        /// </summary>
        /// <param name="caption">Caption of the box. The caption is shown on the top</param>
        /// <param name="entries">Entries of the combobox</param>
        /// <param name="toolTip">The tooltip of the combobox</param>
        /// <param name="backgroundTransition"></param>
        /// <param name="guiTransition"></param>
        public MyGuiScreenDialogCombobox(string caption, List<string> entries, string toolTip, float backgroundTransition = 0f, float guiTransition = 0f) : base(new Vector2(0.5f, 0.5f), MyGuiConstants.SCREEN_BACKGROUND_COLOR, new Vector2(87f / 175f, 147f / 524f), isTopMostScreen: false, null, backgroundTransition, guiTransition)
        {
            CanHideOthers = false;
            EnabledBackgroundFade = false;

            m_caption = caption;
            m_tooltip = toolTip;
            m_entries = entries;

            RecreateControls(true);
        }

        public override void RecreateControls(bool constructor)
        {
            base.RecreateControls(constructor);

            MyGuiControlLabel caption = new MyGuiControlLabel(null, null, m_caption);

            caption.OriginAlign = VRage.Utils.MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_TOP;
            caption.Position = new Vector2(0, Size.Value.Y / -2 + PADDING.Y);

            Controls.Add(caption);

            MyGuiControlParentTableLayout table = new MyGuiControlParentTableLayout(1, false, PADDING);

            table.AddTableSeparator();

            m_comboBox = new MyGuiControlCombobox();
            m_comboBox.SetToolTip(new MyToolTips(m_tooltip));

            int key = 0;
            foreach(var entry in m_entries)
            {
                m_comboBox.AddItem(key, entry);
                key++;
            }

            m_comboBox.SelectItemByKey(0);

            table.AddTableRow(m_comboBox);

            table.AddTableSeparator();

            table.Position = new Vector2(0, caption.Position.Y + caption.Size.Y + PADDING.Y);
            table.OriginAlign = VRage.Utils.MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_TOP;

            Controls.Add(table);

            m_okButton = new MyGuiControlButton();
            m_okButton.Text = MyCommonTexts.Ok.ToString();
            m_okButton.OriginAlign = VRage.Utils.MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_BOTTOM;
            m_okButton.Position = new Vector2(-PADDING.X, Size.Value.Y / 2 - PADDING.Y);
            m_okButton.ButtonClicked += delegate
            {
                OnConfirm?.Invoke(m_comboBox.GetSelectedKey(), m_comboBox.GetSelectedValue().ToString());
                CloseScreen();
            };

            m_cancelButton = new MyGuiControlButton();
            m_cancelButton.Text = MyCommonTexts.Cancel.ToString();
            m_cancelButton.OriginAlign = VRage.Utils.MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_BOTTOM;
            m_cancelButton.Position = new Vector2(PADDING.X, Size.Value.Y / 2 - PADDING.Y);
            m_cancelButton.ButtonClicked += delegate {
                CloseScreen();
            };

            Controls.Add(m_okButton);
            Controls.Add(m_cancelButton);
        }

        public override string GetFriendlyName()
        {
            return "MyGuiScreenDialogCombobox";
        }
    }
}
