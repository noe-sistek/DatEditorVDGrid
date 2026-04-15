using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DatEditorVDGrid.UIHelpers
{
    internal class DataGridHelper
    {
        public static void CommitEdit(DataGridView dgv)
        {
            if (dgv.IsCurrentCellDirty)
            {
                dgv.CommitEdit(DataGridViewDataErrorContexts.Commit);
            }
        }

        public static void HandleEllipsisChange(DataGridView dgv, int rowIndex)
        {
            if (rowIndex < 0) return;

            var row = dgv.Rows[rowIndex];

            bool isEllipsis = Convert.ToBoolean(row.Cells["Ellipsis"].Value ?? false);

            var campoObj = row.Cells["CampoSQL"].Value;
            string campo = campoObj?.ToString() ?? "";

            if (isEllipsis)
            {
                row.Cells["EllipsisSqlSources"].Value = campo.ToUpperInvariant();
            }
            else
            {
                row.Cells["EllipsisSqlSources"].Value = "";
            }
        }
    }
}
