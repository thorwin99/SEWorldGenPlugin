using Sandbox.Graphics.GUI;
using SEWorldGenPlugin.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using VRage.Utils;
using VRageMath;

namespace SEWorldGenPlugin.GUI.Controls
{
    /// <summary>
    /// A parent gui element, that organizes its children in
    /// columns and rows.
    /// </summary>
    public class MyGuiControlParentTableLayout : MyGuiControlParent
    {
        /// <summary>
        /// Margin between rows
        /// </summary>
        private readonly float MARGIN_ROWS = (25f / MyGuiConstants.GUI_OPTIMAL_SIZE.Y);

        /// <summary>
        /// Margin between columns
        /// </summary>
        private readonly float MARGIN_COLUMNS = (20f / MyGuiConstants.GUI_OPTIMAL_SIZE.Y);

        /// <summary>
        /// The table rows in order of addition
        /// </summary>
        private List<MyGuiControlBase[]> m_tableRows;

        /// <summary>
        /// The max amount of columns allowed in the table
        /// </summary>
        private int m_columnsMax;

        /// <summary>
        /// An array that contains each columns current widths based on added rows
        /// </summary>
        private float[] m_columnWidths;

        /// <summary>
        /// The height of the whole table, based on all currently added rows
        /// </summary>
        private float m_tableHeight;

        /// <summary>
        /// Wether column elements can overflow to the next one
        /// </summary>
        private bool m_overflowColumns;

        /// <summary>
        /// Padding for the table. It is the top left offset for the elements and the bottom padding of the table
        /// </summary>
        private Vector2 m_padding;

        /// <summary>
        /// The amount of controls a column is allowed to have in this table layout
        /// </summary>
        public int MaxColumns
        {
            get
            {
                return m_columnsMax;
            }
            private set
            {
                m_columnsMax = value;
            }
        }

        /// <summary>
        /// Creates a new instance. If minWidth is larger then the width of the table when rows are applied, the last column
        /// will fill the space and separator will get adjusted accordingly.
        /// </summary>
        /// <param name="columns">The amount of columns</param>
        /// <param name="overflowColumns">If a column can overflow to the next, if it is the last one of a row but not the last of the table.</param>
        /// <param name="padding">The padding of the Table from the top left corner</param>
        /// <param name="minWidth">The minimum width the table should have.</param>
        /// <param name="maxWidth">The maximum width the table should have. -1 to disable</param>
        public MyGuiControlParentTableLayout(int columns, bool overflowColumns = false, Vector2? padding = null, float minWidth = 0, float maxWidth = -1) : base()
        {
            m_tableRows = new List<MyGuiControlBase[]>();
            m_columnsMax = columns;
            m_columnWidths = new float[m_columnsMax];
            m_overflowColumns = overflowColumns;

            if(padding == null)
            {
                m_padding = new Vector2(MARGIN_COLUMNS, MARGIN_ROWS);
            }
            else
            {
                m_padding = padding.Value;
            }

            m_tableHeight = 0;

            MinSize = new Vector2(minWidth, MinSize.Y);
            if(maxWidth >= 0)
            {
                MaxSize = new Vector2(maxWidth, MaxSize.Y);
            }
        }

        /// <summary>
        /// Adds a new row of controls to the table layouts
        /// row queue. The first column of the row is the first element in the
        /// array. Use ApplyRows to build the layout. All controls
        /// in the row should have already defined its sizes.
        /// </summary>
        /// <param name="rowControls"></param>
        /// <returns>True, when the row is </returns>
        public bool AddTableRow(params MyGuiControlBase[] rowControls)
        {
            if (rowControls.Length > m_columnsMax) return false;
            if (rowControls == null) return false;

            float rowHeight = 0;

            for(int i = 0; i < rowControls.Length; i++)
            {
                if(rowControls[i].Size.X > m_columnWidths[i] && !(i == rowControls.Length - 1 && rowControls.Length < m_columnsMax && m_overflowColumns))
                {
                    m_columnWidths[i] = rowControls[i].Size.X;
                }

                if(rowControls[i].Size.Y > rowHeight)
                {
                    rowHeight = rowControls[i].Size.Y;
                }
            }

            m_tableHeight += rowHeight + MARGIN_ROWS;

            m_tableRows.Add(rowControls);
            return true;
        }

        /// <summary>
        /// Adds a separator into the table at after the last added row.
        /// </summary>
        /// <returns></returns>
        public void AddTableSeparator()
        {
            m_tableRows.Add(null);
            m_tableHeight += MARGIN_ROWS;
        }

        /// <summary>
        /// This applies all rows that are currently added and
        /// sets the size of this parent and the positions of the children.
        /// Only run, when all rows were added.
        /// </summary>
        public void ApplyRows()
        {
            Controls.Clear();

            float tableWidth = 0;

            foreach(var columnWidth in m_columnWidths)
            {
                tableWidth += columnWidth + MARGIN_COLUMNS;
            }

            if(MinSize.X > tableWidth)
            {
                tableWidth = MinSize.X;
            }

            Size = new Vector2(tableWidth, m_tableHeight - MARGIN_ROWS + m_padding.Y);

            Vector2 currentRowTopLeft = new Vector2(Size.X / -2 + m_padding.X, Size.Y / -2 + m_padding.Y);

            foreach(var row in m_tableRows)
            {
                if(row == null)
                {
                    MyGuiControlSeparatorList sep = new MyGuiControlSeparatorList();
                    sep.AddHorizontal(currentRowTopLeft, tableWidth - MARGIN_COLUMNS);

                    Controls.Add(sep);

                    currentRowTopLeft += new Vector2(0, MARGIN_ROWS);

                    continue;
                }

                float currentColumnOffset = 0;
                float rowHeight = 0;
                foreach(var col in row)
                {
                    if (col == null) continue;
                    rowHeight = Math.Max(rowHeight, col.Size.Y);
                }

                for(int i = 0; i < row.Length; i++)
                {
                    var control = row[i];

                    if(control != null)
                    {
                        control.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER;
                        control.Position = currentRowTopLeft + new Vector2(currentColumnOffset, rowHeight / 2);
                        Controls.Add(control);
                    }
                    currentColumnOffset += m_columnWidths[i] + MARGIN_COLUMNS;
                }

                currentRowTopLeft += new Vector2(0, rowHeight + MARGIN_ROWS);
            }
        }

        /// <summary>
        /// Recalculates the table size
        /// </summary>
        public void RecalculateSize()
        {
            m_tableHeight = 0;

            foreach (var row in m_tableRows)
            {
                float rowHeight = 0;
                if (row != null)
                {
                    foreach (var col in row)
                    {
                        if (col.Size.Y > rowHeight)
                            rowHeight = col.Size.Y;
                    }
                }
                m_tableHeight += rowHeight + MARGIN_ROWS;
            }

            float tableWidth = 0;
            foreach (var columnWidth in m_columnWidths)
            {
                tableWidth += columnWidth + MARGIN_COLUMNS;
            }

            Size = new Vector2(tableWidth, m_tableHeight - MARGIN_ROWS + m_padding.Y);
        }

        /// <summary>
        /// Clears the table
        /// </summary>
        public void ClearTable()
        {
            Controls.Clear();
            if(m_tableRows != null)
                m_tableRows.Clear();

            if (m_columnWidths != null)
                m_columnWidths = new float[m_columnsMax];

            m_tableHeight = MARGIN_ROWS;
        }
    }
}
