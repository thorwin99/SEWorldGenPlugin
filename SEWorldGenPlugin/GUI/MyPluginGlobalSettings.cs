using Sandbox;
using Sandbox.Game.Screens;
using Sandbox.Graphics.GUI;
using SEWorldGenPlugin.GUI.Controls;
using SEWorldGenPlugin.ObjectBuilders;
using SEWorldGenPlugin.Utilities;
using System;
using System.Text;
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
        private MyGuiControlTable m_fixedPlanetSizeTable;
        private MyGuiControlTextbox m_planetNameBox;
        private MyGuiControlTextbox m_moonNameBox;
        private MyGuiControlTextbox m_beltNameBox;

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
            var fixedPlanetSizeLabel = new MyGuiControlLabel(null, null, "Fixed planet sizes");
            var planetNameLabel = new MyGuiControlLabel(null, null, "Planet name format");
            var moonNameLabel = new MyGuiControlLabel(null, null, "Moon name format");
            var beltNameLabel = new MyGuiControlLabel(null, null, "Belt name format");

            var enablePatchingLabel = new MyGuiControlLabel(null, null, "Enable Patching?");

            MyGuiControlCheckbox enablePatchingCheckbox = new MyGuiControlCheckbox(null, null, "Requires Restart. Enables patching for the plugin. Please read the wiki for more information about it.");
            enablePatchingCheckbox.IsChecked = MySettings.Static.Settings.EnablePatching;
            enablePatchingCheckbox.IsCheckedChanged += delegate
            {
                if (enablePatchingCheckbox.IsChecked)
                {
                    MyPluginGuiHelper.DisplayQuestion("Do you want to enable Patching?", "Warning", delegate (MyGuiScreenMessageBox.ResultEnum res)
                    {
                        if(res == MyGuiScreenMessageBox.ResultEnum.YES)
                        {
                            MySettings.Static.Settings.EnablePatching = true;
                        }
                        else
                        {
                            MySettings.Static.Settings.EnablePatching = false;
                            enablePatchingCheckbox.IsChecked = false;
                        }
                    });
                }
                else
                {
                    MySettings.Static.Settings.EnablePatching = false;
                    enablePatchingCheckbox.IsChecked = false;
                }
            };

            parent.AddTableRow(enablePatchingLabel, enablePatchingCheckbox);

            parent.AddTableSeparator();

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
                OpenEnterIdDialog(m_mandatoryDefsTable, delegate (string name)
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

            parent.AddTableSeparator();

            m_fixedPlanetSizeTable = new MyGuiControlTable();
            m_fixedPlanetSizeTable.VisibleRowsCount = 8;
            m_fixedPlanetSizeTable.Size = new Vector2(WIDTH, m_fixedPlanetSizeTable.Size.Y);
            m_fixedPlanetSizeTable.ColumnsCount = 2;
            m_fixedPlanetSizeTable.SetCustomColumnWidths(new float[] { WIDTH, WIDTH});
            m_fixedPlanetSizeTable.SetColumnName(0, new StringBuilder("Subtype ID"));
            m_fixedPlanetSizeTable.SetColumnName(1, new StringBuilder("Diameter"));

            var addFixedSizeBtn = MyPluginGuiHelper.CreateDebugButton(DBG_BTN_WIDTH, "Add", delegate
            {
                MyGuiScreenDialogText inputBox = new MyGuiScreenDialogText();
                inputBox.OnConfirmed += delegate (string text)
                {
                    if (Regex.Match(text, ILLEGAL_XML).Success)
                    {
                        MyPluginGuiHelper.DisplayError("The entered subtype id contains invalid characters (& < >).", "Error, invalid character");
                        return;
                    }

                    var prop = MySettings.Static.Settings.FixedPlanetSizes;

                    foreach(var planet in prop)
                    {
                        if(planet.SubtypeId == text)
                        {
                            MyPluginGuiHelper.DisplayError("The entered subtype id was already added.", "Error, already added");
                            return;
                        }
                    }

                    var def = new PlanetSizeDefinition(text, 120000);
                    var row = new MyGuiControlTable.Row(def);
                    row.AddCell(new MyGuiControlTable.Cell(text));
                    row.AddCell(new MyGuiControlTable.Cell("120000 m"));

                    m_fixedPlanetSizeTable.Add(row);
                    prop.Add(def);
                };
                MyGuiSandbox.AddScreen(inputBox);
            });
            var remFixedSizeBtn = MyPluginGuiHelper.CreateDebugButton(DBG_BTN_WIDTH, "Remove", delegate
            {
                if (m_fixedPlanetSizeTable.SelectedRow == null) return;

                PlanetSizeDefinition def = m_fixedPlanetSizeTable.SelectedRow.UserData as PlanetSizeDefinition;

                MySettings.Static.Settings.FixedPlanetSizes.Remove(def);

                m_fixedPlanetSizeTable.Remove(m_fixedPlanetSizeTable.SelectedRow);
            });
            remFixedSizeBtn.Enabled = false;

            m_fixedPlanetSizeTable.FocusChanged += delegate
            {
                remFixedSizeBtn.Enabled = m_fixedPlanetSizeTable.HasFocus && m_fixedPlanetSizeTable.SelectedRow != null;
            };
            m_fixedPlanetSizeTable.ItemSelected += delegate (MyGuiControlTable table, MyGuiControlTable.EventArgs args)
            {
                remFixedSizeBtn.Enabled = table.SelectedRow != null;
            };
            m_fixedPlanetSizeTable.ItemDoubleClicked += delegate (MyGuiControlTable table, MyGuiControlTable.EventArgs args)
            {
                PlanetSizeDefinition def = table.GetRow(args.RowIndex).UserData as PlanetSizeDefinition;

                MyGuiScreenDialogAmount inputBox = new MyGuiScreenDialogAmount(0, 2400000, MyCommonTexts.Blank, parseAsInteger: true, defaultAmount: (float)def.Diameter);

                inputBox.OnConfirmed += delegate (float diameter)
                {
                    def.Diameter = diameter;
                    table.Remove(table.GetRow(args.RowIndex));
                    var row = new MyGuiControlTable.Row(def);
                    row.AddCell(new MyGuiControlTable.Cell(def.SubtypeId));
                    row.AddCell(new MyGuiControlTable.Cell(def.Diameter.ToString() + " m"));

                    table.Add(row);
                };

                MyGuiSandbox.AddScreen(inputBox);
            };

            parent.AddTableRow(fixedPlanetSizeLabel);
            parent.AddTableRow(m_fixedPlanetSizeTable);
            parent.AddTableRow(addFixedSizeBtn, remFixedSizeBtn);

            #endregion

            parent.AddTableSeparator();

            #region NameFormats

            ///Planet name box
            m_planetNameBox = new MyGuiControlTextbox();
            m_planetNameBox.Size = new Vector2(WIDTH, m_planetNameBox.Size.Y);
            m_planetNameBox.TextChanged += delegate
            {
                StringBuilder s = new StringBuilder();
                m_planetNameBox.GetText(s);
                MySettings.Static.Settings.PlanetNameFormat = s.ToString();
            };

            MyGuiControlParentTableLayout formatButtons = new MyGuiControlParentTableLayout(3, padding: new Vector2(0));
            var planetNameButtons1 = new MyGuiControlButton[3];
            var planetNameButtons2 = new MyGuiControlButton[3];
            planetNameButtons1[0] = MyPluginGuiHelper.CreateDebugButton(DBG_BTN_WIDTH, "Add Object Number", delegate 
            { 
                MySettings.Static.Settings.PlanetNameFormat = AddNameProperty(m_planetNameBox, MyNamingUtils.PROP_OBJ_NUMBER); 
            });
            planetNameButtons1[1] = MyPluginGuiHelper.CreateDebugButton(DBG_BTN_WIDTH, "Add Roman number", delegate
            {
                MySettings.Static.Settings.PlanetNameFormat = AddNameProperty(m_planetNameBox, MyNamingUtils.PROP_OBJ_NUMBER_ROMAN);
            });
            planetNameButtons1[2] = MyPluginGuiHelper.CreateDebugButton(DBG_BTN_WIDTH, "Add Greek number", delegate
            {
                MySettings.Static.Settings.PlanetNameFormat = AddNameProperty(m_planetNameBox, MyNamingUtils.PROP_OBJ_NUMBER_GREEK);
            });
            planetNameButtons2[0] = MyPluginGuiHelper.CreateDebugButton(DBG_BTN_WIDTH, "Add lower letter", delegate
            {
                MySettings.Static.Settings.PlanetNameFormat = AddNameProperty(m_planetNameBox, MyNamingUtils.PROP_OBJ_LETTER_LOWER);
            });
            planetNameButtons2[1] = MyPluginGuiHelper.CreateDebugButton(DBG_BTN_WIDTH, "Add upper letter", delegate
            {
                MySettings.Static.Settings.PlanetNameFormat = AddNameProperty(m_planetNameBox, MyNamingUtils.PROP_OBJ_LETTER_UPPER);
            });
            planetNameButtons2[2] = MyPluginGuiHelper.CreateDebugButton(DBG_BTN_WIDTH, "Add id", delegate
            {
                MySettings.Static.Settings.PlanetNameFormat = AddNameProperty(m_planetNameBox, MyNamingUtils.PROP_OBJ_ID);
            });

            m_planetNameBox.TextChanged += delegate(MyGuiControlTextbox t)
            {
                var sb = new StringBuilder();
                t.GetText(sb);

                MySettings.Static.Settings.PlanetNameFormat = sb.ToString();
            };

            formatButtons.AddTableRow(planetNameButtons1);
            formatButtons.AddTableRow(planetNameButtons2);

            parent.AddTableRow(planetNameLabel);
            parent.AddTableRow(m_planetNameBox);
            parent.AddTableRow(formatButtons);

            parent.AddTableSeparator();

            ///Moon name box
            m_moonNameBox = new MyGuiControlTextbox();
            m_moonNameBox.Size = new Vector2(WIDTH, m_moonNameBox.Size.Y);
            m_moonNameBox.TextChanged += delegate
            {
                StringBuilder s = new StringBuilder();
                m_moonNameBox.GetText(s);
                MySettings.Static.Settings.MoonNameFormat = s.ToString();
            };

            MyGuiControlParentTableLayout formatButtonsMoon = new MyGuiControlParentTableLayout(3, padding: new Vector2(0));
            var moonNameButtons1 = new MyGuiControlButton[3];
            var moonNameButtons2 = new MyGuiControlButton[3];
            moonNameButtons1[0] = MyPluginGuiHelper.CreateDebugButton(DBG_BTN_WIDTH, "Add Object Number", delegate
            {
                MySettings.Static.Settings.MoonNameFormat = AddNameProperty(m_moonNameBox, MyNamingUtils.PROP_OBJ_NUMBER);
            });
            moonNameButtons1[1] = MyPluginGuiHelper.CreateDebugButton(DBG_BTN_WIDTH, "Add Roman number", delegate
            {
                MySettings.Static.Settings.MoonNameFormat = AddNameProperty(m_moonNameBox, MyNamingUtils.PROP_OBJ_NUMBER_ROMAN);
            });
            moonNameButtons1[2] = MyPluginGuiHelper.CreateDebugButton(DBG_BTN_WIDTH, "Add Greek number", delegate
            {
                MySettings.Static.Settings.MoonNameFormat = AddNameProperty(m_moonNameBox, MyNamingUtils.PROP_OBJ_NUMBER_GREEK);
            });
            moonNameButtons2[0] = MyPluginGuiHelper.CreateDebugButton(DBG_BTN_WIDTH, "Add lower letter", delegate
            {
                MySettings.Static.Settings.MoonNameFormat = AddNameProperty(m_moonNameBox, MyNamingUtils.PROP_OBJ_LETTER_LOWER);
            });
            moonNameButtons2[1] = MyPluginGuiHelper.CreateDebugButton(DBG_BTN_WIDTH, "Add upper letter", delegate
            {
                MySettings.Static.Settings.MoonNameFormat = AddNameProperty(m_moonNameBox, MyNamingUtils.PROP_OBJ_LETTER_UPPER);
            });
            moonNameButtons2[2] = MyPluginGuiHelper.CreateDebugButton(DBG_BTN_WIDTH, "Add id", delegate
            {
                MySettings.Static.Settings.MoonNameFormat = AddNameProperty(m_moonNameBox, MyNamingUtils.PROP_OBJ_ID);
            });
            var btnExtra = MyPluginGuiHelper.CreateDebugButton(DBG_BTN_WIDTH, "Add parent name", delegate
            {
                MySettings.Static.Settings.MoonNameFormat = AddNameProperty(m_moonNameBox, MyNamingUtils.PROP_OBJ_PARENT);
            });

            m_moonNameBox.TextChanged += delegate (MyGuiControlTextbox t)
            {
                var sb = new StringBuilder();
                t.GetText(sb);

                MySettings.Static.Settings.MoonNameFormat = sb.ToString();
            };

            formatButtonsMoon.AddTableRow(moonNameButtons1);
            formatButtonsMoon.AddTableRow(moonNameButtons2);
            formatButtonsMoon.AddTableRow(btnExtra);

            parent.AddTableRow(moonNameLabel);
            parent.AddTableRow(m_moonNameBox);
            parent.AddTableRow(formatButtonsMoon);

            parent.AddTableSeparator();

            ///Belt name box
            m_beltNameBox = new MyGuiControlTextbox();
            m_beltNameBox.Size = new Vector2(WIDTH, m_beltNameBox.Size.Y);
            m_beltNameBox.TextChanged += delegate
            {
                StringBuilder s = new StringBuilder();
                m_beltNameBox.GetText(s);
                MySettings.Static.Settings.BeltNameFormat = s.ToString();
            };

            MyGuiControlParentTableLayout formatButtonsBelt = new MyGuiControlParentTableLayout(3, padding: new Vector2(0));
            var beltNameButtons1 = new MyGuiControlButton[3];
            var beltNameButtons2 = new MyGuiControlButton[2];
            beltNameButtons1[0] = MyPluginGuiHelper.CreateDebugButton(DBG_BTN_WIDTH, "Add Object Number", delegate
            {
                MySettings.Static.Settings.BeltNameFormat = AddNameProperty(m_beltNameBox, MyNamingUtils.PROP_OBJ_NUMBER);
            });
            beltNameButtons1[1] = MyPluginGuiHelper.CreateDebugButton(DBG_BTN_WIDTH, "Add Roman number", delegate
            {
                MySettings.Static.Settings.BeltNameFormat = AddNameProperty(m_beltNameBox, MyNamingUtils.PROP_OBJ_NUMBER_ROMAN);
            });
            beltNameButtons1[2] = MyPluginGuiHelper.CreateDebugButton(DBG_BTN_WIDTH, "Add Greek number", delegate
            {
                MySettings.Static.Settings.BeltNameFormat = AddNameProperty(m_beltNameBox, MyNamingUtils.PROP_OBJ_NUMBER_GREEK);
            });
            beltNameButtons2[0] = MyPluginGuiHelper.CreateDebugButton(DBG_BTN_WIDTH, "Add lower letter", delegate
            {
                MySettings.Static.Settings.BeltNameFormat = AddNameProperty(m_beltNameBox, MyNamingUtils.PROP_OBJ_LETTER_LOWER);
            });
            beltNameButtons2[1] = MyPluginGuiHelper.CreateDebugButton(DBG_BTN_WIDTH, "Add upper letter", delegate
            {
                MySettings.Static.Settings.BeltNameFormat = AddNameProperty(m_beltNameBox, MyNamingUtils.PROP_OBJ_LETTER_UPPER);
            });

            m_beltNameBox.TextChanged += delegate (MyGuiControlTextbox t)
            {
                var sb = new StringBuilder();
                t.GetText(sb);

                MySettings.Static.Settings.BeltNameFormat = sb.ToString();
            };

            formatButtonsBelt.AddTableRow(beltNameButtons1);
            formatButtonsBelt.AddTableRow(beltNameButtons2);

            parent.AddTableRow(beltNameLabel);
            parent.AddTableRow(m_beltNameBox);
            parent.AddTableRow(formatButtonsBelt);
            #endregion

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
        /// Adds the property for naming formats to the textBox
        /// </summary>
        /// <param name="textBox">TextBox</param>
        /// <param name="property">Property</param>
        /// <returns>The resulting text in the text box</returns>
        private string AddNameProperty(MyGuiControlTextbox textBox, string property)
        {
            var sb = new StringBuilder();
            textBox.GetText(sb);
            sb.Append("[" + property + "]");
            textBox.SetText(sb);

            return sb.ToString();
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
            foreach(var def in MySettings.Static.Settings.FixedPlanetSizes)
            {
                var row = new MyGuiControlTable.Row(def);
                row.AddCell(new MyGuiControlTable.Cell(def.SubtypeId));
                row.AddCell(new MyGuiControlTable.Cell(def.Diameter.ToString() + " m"));

                m_fixedPlanetSizeTable.Add(row);
            }
            m_planetNameBox.SetText(new StringBuilder(MySettings.Static.Settings.PlanetNameFormat));
            m_moonNameBox.SetText(new StringBuilder(MySettings.Static.Settings.MoonNameFormat));
            m_beltNameBox.SetText(new StringBuilder(MySettings.Static.Settings.BeltNameFormat));
        }

        public override string GetFriendlyName()
        {
            return "SEWG Global Settings";
        }
    }
}
