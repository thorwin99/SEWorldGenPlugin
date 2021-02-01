using Sandbox.Graphics.GUI;
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

        public MyGuiControlParentTableLayout(int columns) : base()
        {
            m_tableRows = new List<MyGuiControlBase[]>();
            m_columnsMax = columns;
            m_columnWidths = new float[m_columnsMax];
        }

        /// <summary>
        /// Adds a new row of controls to the table layouts
        /// row queue. The first column of the row is the first element in the
        /// array. Use ApplyRows to build the layout. All controls
        /// in the row should have already defined its sizes.
        /// </summary>
        /// <param name="rowControls"></param>
        /// <returns>True, when the row is </returns>
        public bool AddTableRow(MyGuiControlBase[] rowControls)
        {
            if (rowControls.Length > m_columnsMax) return false;
            if (rowControls == null) return false;

            float rowHeight = 0;

            for(int i = 0; i < rowControls.Length; i++)
            {
                if(rowControls[i].Size.X > m_columnWidths[i])
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
                tableWidth += columnWidth;
            }

            tableWidth = Math.Min(MaxSize.X, tableWidth);

            Size = new Vector2(tableWidth, m_tableHeight - MARGIN_ROWS);

            Vector2 currentRowTopLeft = new Vector2(Size.X / -2, Size.Y / -2);

            foreach(var row in m_tableRows)
            {
                float currentColumnOffset = 0;
                float rowHeight = 0;
                foreach(var col in row)
                {
                    rowHeight = Math.Max(rowHeight, col.Size.Y);
                }

                for(int i = 0; i < row.Length; i++)
                {
                    var control = row[i];
                    control.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER;
                    control.Position = currentRowTopLeft + new Vector2(currentColumnOffset, rowHeight / 2);
                    Controls.Add(control);
                    currentColumnOffset += m_columnWidths[i] + MARGIN_COLUMNS;
                }

                currentRowTopLeft += new Vector2(0, rowHeight + MARGIN_ROWS);
            }
        }
    }
}
