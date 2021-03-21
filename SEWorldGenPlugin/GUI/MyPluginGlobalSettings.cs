using Sandbox;
using Sandbox.Game.Screens;
using Sandbox.Graphics.GUI;
using SEWorldGenPlugin.GUI.Controls;
using System;
using System.Text.RegularExpressions;
using VRage.Utils;
using VRageMath;

namespace SEWorldGenPlugin.GUI
{
    /// <summary>
    /// Gui screen to edit the plugins global settings
    /// </summary>
    public class MyPluginGlobalSettings : MyGuiScreenBase
    {
        /// <summary>
        /// The size of this screen.
        /// </summary>
        private static readonly Vector2 SIZE = new Vector2(1024, 1120) / MyGuiConstants.GUI_OPTIMAL_SIZE;

        /// <summary>
        /// Padding for the child elements to the top left border of this screen
        /// </summary>
        private static readonly Vector2 PADDING = new Vector2(50) / MyGuiConstants.GUI_OPTIMAL_SIZE;

        /// <summary>
        /// The width of the center content section of the screen
        /// </summary>
        private static readonly float WIDTH = SIZE.X - 3 * PADDING.X;

        /// <summary>
        /// The width of the debug buttons added on this screen
        /// </summary>
        private static readonly float DBG_BTN_WIDTH = 0.15f;

        /// <summary>
        /// Regex to filter illegal xml characters
        /// </summary>
        private static string ILLEGAL_XML = "[&<>]";

        /// <summary>
        /// The margin between 2 child elements.
        /// </summary>
        private static readonly Vector2 CHILD_MARGINS_VERT = new Vector2(0, 32f) / MyGuiConstants.GUI_OPTIMAL_SIZE;

        private MyGuiControlTable m_moonDefsTable;
        private MyGuiControlTable m_gasGiantsDefsTable;
        private MyGuiControlTable m_sunDefsTable;
        private MyGuiControlTable m_mandatoryDefsTable;
        private MyGuiControlTable m_blacklistDefsTable;

        public MyPluginGlobalSettings() : base(new Vector2(0.5f, 0.5f), MyGuiConstants.SCREEN_BACKGROUND_COLOR, SIZE, false, null, MySandboxGame.Config.UIBkOpacity, MySandboxGame.Config.UIOpacity)
        {
            RecreateControls(true);

            LoadSettings();
        }

        public override void RecreateControls(bool constructor)
        {
            base.RecreateControls(constructor);

            var caption = AddCaption("SEWorldGenPlugin global settings");
            caption.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_TOP;
            caption.Position = new Vector2(0, SIZE.Y / -2 + PADDING.Y);

            MyGuiControlButton OkButton = new MyGuiControlButton(null, VRage.Game.MyGuiControlButtonStyleEnum.Default, null, null, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_BOTTOM, "Saves the global settings file now and exits this menu", VRage.MyTexts.Get(MyCommonTexts.Ok), 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, OnOkButtonClicked);
            OkButton.Position = new Vector2(0, SIZE.Y / 2 - PADDING.Y);
            Controls.Add(OkButton);

            MyGuiControlSeparatorList separators = new MyGuiControlSeparatorList();
            separators.AddHorizontal(SIZE / -2 + PADDING + new Vector2(0, caption.Size.Y) + CHILD_MARGINS_VERT, SIZE.X - 2 * PADDING.X);
            separators.AddHorizontal(new Vector2(SIZE.X / -2 + PADDING.X, SIZE.Y / 2 - PADDING.Y - OkButton.Size.Y) - CHILD_MARGINS_VERT, SIZE.X - 2 * PADDING.X);
            Controls.Add(separators);

            MyGuiControlParentTableLayout parent = new MyGuiControlParentTableLayout(2, overflowColumns: true, minWidth: WIDTH);
            parent.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;

            var moonsLabel = new MyGuiControlLabel(null, null, "Moons");
            var gasGiantLabel = new MyGuiControlLabel(null, null, "Gas giants");
            var sunLabel = new MyGuiControlLabel(null, null, "Suns");
            var mandatoryLabel = new MyGuiControlLabel(null, null, "Mandatory planets and moons");
            var blacklistLabel = new MyGuiControlLabel(null, null, "Blacklisted planets and moons");

#region Tables

            ///Moons table
            m_moonDefsTable = new MyGuiControlTable();
            m_moonDefsTable.VisibleRowsCount = 8;
            m_moonDefsTable.Size = new Vector2(WIDTH, m_moonDefsTable.Size.Y);
            m_moonDefsTable.ColumnsCount = 1;
            m_moonDefsTable.SetCustomColumnWidths(new float[] { WIDTH });
            m_moonDefsTable.SetColumnName(0, new System.Text.StringBuilder("Subtype ID"));

            var addMoonBtn = MyPluginGuiHelper.CreateDebugButton(DBG_BTN_WIDTH, "Add Moon", delegate
            {
                OpenEnterIdDialog(m_moonDefsTable, delegate (string name)
                {
                    MySettings.Static.Settings.MoonDefinitions.Add(name);
                });
            });
            var remMoonBtn = MyPluginGuiHelper.CreateDebugButton(DBG_BTN_WIDTH, "Remove Moon", delegate 
            {
                MySettings.Static.Settings.MoonDefinitions.Remove(m_moonDefsTable.SelectedRow.UserData as string);
                m_moonDefsTable.RemoveSelectedRow();
            });

            remMoonBtn.Enabled = false;

            m_moonDefsTable.FocusChanged += delegate
            {
                remMoonBtn.Enabled = m_moonDefsTable.HasFocus && m_moonDefsTable.SelectedRow != null;
            };
            m_moonDefsTable.ItemSelected += delegate (MyGuiControlTable table, MyGuiControlTable.EventArgs args)
            {
                remMoonBtn.Enabled = m_moonDefsTable.HasFocus && m_moonDefsTable.SelectedRow != null;
            };

            parent.AddTableRow(moonsLabel);
            parent.AddTableRow(m_moonDefsTable);
            parent.AddTableRow(addMoonBtn, remMoonBtn);

            parent.AddTableSeparator();

            ///Gas giant table
            m_gasGiantsDefsTable = new MyGuiControlTable();
            m_gasGiantsDefsTable.VisibleRowsCount = 8;
            m_gasGiantsDefsTable.Size = new Vector2(WIDTH, m_gasGiantsDefsTable.Size.Y);
            m_gasGiantsDefsTable.ColumnsCount = 1;
            m_gasGiantsDefsTable.SetCustomColumnWidths(new float[] { WIDTH });
            m_gasGiantsDefsTable.SetColumnName(0, new System.Text.StringBuilder("Subtype ID"));

            MyGuiControlParentTableLayout gasGiantBtns = new MyGuiControlParentTableLayout(2, padding: new Vector2(0));
            var addGasGiantBtn = MyPluginGuiHelper.CreateDebugButton(DBG_BTN_WIDTH, "Add Gas Giant", delegate
            {
                OpenEnterIdDialog(m_gasGiantsDefsTable, delegate (string name)
                {
                    MySettings.Static.Settings.GasGiantDefinitions.Add(name);
                });
            });
            var remGasGiantBtn = MyPluginGuiHelper.CreateDebugButton(DBG_BTN_WIDTH, "Remove Gas Giant", delegate
            {
                MySettings.Static.Settings.GasGiantDefinitions.Remove(m_gasGiantsDefsTable.SelectedRow.UserData as string);
                m_gasGiantsDefsTable.RemoveSelectedRow();
            });

            remGasGiantBtn.Enabled = false;

            m_gasGiantsDefsTable.FocusChanged += delegate
            {
                remGasGiantBtn.Enabled = m_gasGiantsDefsTable.HasFocus && m_gasGiantsDefsTable.SelectedRow != null;
            };
            m_gasGiantsDefsTable.ItemSelected += delegate (MyGuiControlTable table, MyGuiControlTable.EventArgs args)
            {
                remGasGiantBtn.Enabled = table.SelectedRow != null;
            };

            parent.AddTableRow(gasGiantLabel);
            parent.AddTableRow(m_gasGiantsDefsTable);
            parent.AddTableRow(addGasGiantBtn, remGasGiantBtn);

            parent.AddTableSeparator();

            ///Suns table
            m_sunDefsTable = new MyGuiControlTable();
            m_sunDefsTable.VisibleRowsCount = 8;
            m_sunDefsTable.Size = new Vector2(WIDTH, m_sunDefsTable.Size.Y);
            m_sunDefsTable.ColumnsCount = 1;
            m_sunDefsTable.SetCustomColumnWidths(new float[] { WIDTH });
            m_sunDefsTable.SetColumnName(0, new System.Text.StringBuilder("Subtype ID"));

            var addSunBtn = MyPluginGuiHelper.CreateDebugButton(DBG_BTN_WIDTH, "Add Sun", delegate
            {
                OpenEnterIdDialog(m_sunDefsTable, delegate (string name)
                {
                    MySettings.Static.Settings.SunDefinitions.Add(name);
                });
            });
            var remSunBtn = MyPluginGuiHelper.CreateDebugButton(DBG_BTN_WIDTH, "Remove Sun", delegate
            {
                MySettings.Static.Settings.SunDefinitions.Remove(m_sunDefsTable.SelectedRow.UserData as string);
                m_sunDefsTable.RemoveSelectedRow();
            });

            remSunBtn.Enabled = false;

            m_sunDefsTable.FocusChanged += delegate
            {
                remSunBtn.Enabled = m_sunDefsTable.HasFocus && m_sunDefsTable.SelectedRow != null;
            };
            m_sunDefsTable.ItemSelected += delegate (MyGuiControlTable table, MyGuiControlTable.EventArgs args)
            {
                remSunBtn.Enabled = table.SelectedRow != null;
            };

            parent.AddTableRow(sunLabel);
            parent.AddTableRow(m_sunDefsTable);
            parent.AddTableRow(addSunBtn, remSunBtn);

            parent.AddTableSeparator();

            ///Mandatory table
            m_mandatoryDefsTable = new MyGuiControlTable();
            m_mandatoryDefsTable.VisibleRowsCount = 8;
            m_mandatoryDefsTable.Size = new Vector2(WIDTH, m_mandatoryDefsTable.Size.Y);
            m_mandatoryDefsTable.ColumnsCount = 1;
            m_mandatoryDefsTable.SetCustomColumnWidths(new float[] { WIDTH });
            m_mandatoryDefsTable.SetColumnName(0, new System.Text.StringBuilder("Subtype ID"));

            var addMandatoryBtn = MyPluginGuiHelper.CreateDebugButton(DBG_BTN_WIDTH, "Add Mandatory", delegate 
            { 
                OpenEnterIdDialog(m_mandatoryDefsTable, delegate(string name) 
                {
                    MySettings.Static.Settings.MandatoryPlanetDefinitions.Add(name);
                }); 
            });
            var remMandatoryBtn = MyPluginGuiHelper.CreateDebugButton(DBG_BTN_WIDTH, "Remove Mandatory", delegate
            {
                MySettings.Static.Settings.MandatoryPlanetDefinitions.Remove(m_mandatoryDefsTable.SelectedRow.UserData as string);
                m_mandatoryDefsTable.RemoveSelectedRow();
            });

            remMandatoryBtn.Enabled = false;

            m_mandatoryDefsTable.FocusChanged += delegate
            {
                remMandatoryBtn.Enabled = m_mandatoryDefsTable.HasFocus && m_mandatoryDefsTable.SelectedRow != null;
            };
            m_mandatoryDefsTable.ItemSelected += delegate (MyGuiControlTable table, MyGuiControlTable.EventArgs args)
            {
                remMandatoryBtn.Enabled = table.SelectedRow != null;
            };

            parent.AddTableRow(mandatoryLabel);
            parent.AddTableRow(m_mandatoryDefsTable);
            parent.AddTableRow(addMandatoryBtn, remMandatoryBtn);

            parent.AddTableSeparator();

            ///Blacklist table
            m_blacklistDefsTable = new MyGuiControlTable();
            m_blacklistDefsTable.VisibleRowsCount = 8;
            m_blacklistDefsTable.Size = new Vector2(WIDTH, m_blacklistDefsTable.Size.Y);
            m_blacklistDefsTable.ColumnsCount = 1;
            m_blacklistDefsTable.SetCustomColumnWidths(new float[] { WIDTH });
            m_blacklistDefsTable.SetColumnName(0, new System.Text.StringBuilder("Subtype ID"));

            var addBlacklistBtn = MyPluginGuiHelper.CreateDebugButton(DBG_BTN_WIDTH, "Add to Blacklist", delegate
            {
                OpenEnterIdDialog(m_blacklistDefsTable, delegate (string name)
                {
                    MySettings.Static.Settings.BlacklistedPlanetDefinitions.Add(name);
                });
            });
            var remBlacklistBtn = MyPluginGuiHelper.CreateDebugButton(DBG_BTN_WIDTH, "Remove from Blacklist", delegate
            {
                MySettings.Static.Settings.BlacklistedPlanetDefinitions.Remove(m_blacklistDefsTable.SelectedRow.UserData as string);
                m_blacklistDefsTable.RemoveSelectedRow();
            });

            remBlacklistBtn.Enabled = false;

            m_blacklistDefsTable.FocusChanged += delegate
            {
                remBlacklistBtn.Enabled = m_blacklistDefsTable.HasFocus && m_blacklistDefsTable.SelectedRow != null;
            };
            m_blacklistDefsTable.ItemSelected += delegate (MyGuiControlTable table, MyGuiControlTable.EventArgs args)
            {
                remBlacklistBtn.Enabled = table.SelectedRow != null;
            };

            parent.AddTableRow(blacklistLabel);
            parent.AddTableRow(m_blacklistDefsTable);
            parent.AddTableRow(addBlacklistBtn, remBlacklistBtn);

#endregion

            parent.ApplyRows();

            Vector2 start = SIZE / -2 + PADDING + new Vector2(0, caption.Size.Y) + CHILD_MARGINS_VERT * 2;
            Vector2 end = new Vector2(SIZE.X / 2 - PADDING.X, SIZE.Y / 2 - PADDING.Y - OkButton.Size.Y) - CHILD_MARGINS_VERT * 2;

            MyGuiControlScrollablePanel scrollPane = new MyGuiControlScrollablePanel(parent);
            scrollPane.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            scrollPane.ScrollbarVEnabled = true;
            scrollPane.Size = end - start;
            scrollPane.Position = start;

            Controls.Add(scrollPane);
        }

        /// <summary>
        /// Opens the dialog to add a subtype id to the given table
        /// </summary>
        /// <param name="table">Table to add item to</param>
        /// <param name="addAction">Action, which should add the id to the settings file</param>
        private void OpenEnterIdDialog(MyGuiControlTable table, Action<string> addAction)
        {
            MyGuiScreenDialogText inputBox = new MyGuiScreenDialogText();
            inputBox.OnConfirmed += delegate(string text)
            {
                if(Regex.Match(text, ILLEGAL_XML).Success)
                {
                    MyPluginGuiHelper.DisplayError("The entered subtype id contains invalid characters (& < >).", "Error, invalid character");
                    return;
                }

                var row = new MyGuiControlTable.Row(text);
                row.AddCell(new MyGuiControlTable.Cell(text));

                table.Add(row);
                addAction(text);
            };
            MyGuiSandbox.AddScreen(inputBox);
        }

        /// <summary>
        /// Called, when ok button is clicked
        /// </summary>
        /// <param name="btn">The button that called this</param>
        private void OnOkButtonClicked(MyGuiControlButton btn)
        {
            MySettings.Static.SaveSettings();
            this.CloseScreen();
        }

        /// <summary>
        /// Loads the global settings and reflects them in gui
        /// </summary>
        private void LoadSettings()
        {
            foreach(var moon in MySettings.Static.Settings.MoonDefinitions)
            {
                var row = new MyGuiControlTable.Row(moon);
                row.AddCell(new MyGuiControlTable.Cell(moon));

                m_moonDefsTable.Add(row);
            }
            foreach (var gasGiant in MySettings.Static.Settings.GasGiantDefinitions)
            {
                var row = new MyGuiControlTable.Row(gasGiant);
                row.AddCell(new MyGuiControlTable.Cell(gasGiant));

                m_gasGiantsDefsTable.Add(row);
            }
            foreach (var sun in MySettings.Static.Settings.SunDefinitions)
            {
                var row = new MyGuiControlTable.Row(sun);
                row.AddCell(new MyGuiControlTable.Cell(sun));

                m_sunDefsTable.Add(row);
            }
            foreach (var mandatory in MySettings.Static.Settings.MandatoryPlanetDefinitions)
            {
                var row = new MyGuiControlTable.Row(mandatory);
                row.AddCell(new MyGuiControlTable.Cell(mandatory));

                m_mandatoryDefsTable.Add(row);
            }
            foreach (var blacklisted in MySettings.Static.Settings.BlacklistedPlanetDefinitions)
            {
                var row = new MyGuiControlTable.Row(blacklisted);
                row.AddCell(new MyGuiControlTable.Cell(blacklisted));

                m_blacklistDefsTable.Add(row);
            }
        }

        public override string GetFriendlyName()
        {
            return "SEWG Global Settings";
        }
    }
}
