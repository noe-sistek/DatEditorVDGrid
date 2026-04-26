using DatEditorVDGrid.Helpers;
using DatEditorVDGrid.Services;
using DatEditorVDGrid.UIHelpers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DatEditorVDGrid
{
    public partial class Form1 : Form
    {
        private void dgvColumns_CellToolTipTextNeeded(object sender, DataGridViewCellToolTipTextNeededEventArgs e)
        {
            if (e.RowIndex < 0) return;

            var colName = dgvColumns.Columns[e.ColumnIndex].Name;

            switch (colName)
            {
                case "CampoSQL":
                    e.ToolTipText = "Ejemplo: d.id, p.nombre";
                    break;

                case "EllipsisSqlSources":
                    e.ToolTipText = "SQL que se ejecuta en el buscador (popup)";
                    break;

                case "ColsSqlValidToken":
                    e.ToolTipText = "Consulta SQL usada para validar el valor";
                    break;

                case "ColsToAsignValuesToken":
                    e.ToolTipText = "Valores que se asignan automáticamente";
                    break;
            }
        }

        private void InitTooltips()
        {
            // Botones
            _tt.SetToolTip(btnAddRow, "Agrega una nueva fila al grid");
            _tt.SetToolTip(btnGenerate, "Genera el archivo .dat con la configuración actual");
            _tt.SetToolTip(btnValidate, "Valida que los campos requeridos estén completos");
            _tt.SetToolTip(btnLoadDat, "Carga un archivo .dat existente");
            _tt.SetToolTip(btnFillValues, "Completa valores por defecto en columnas vacías");

            // TextBoxes
            _tt.SetToolTip(txtWhereSql, "Cláusula WHERE del SQL");
            _tt.SetToolTip(txtOrderSql, "Cláusula ORDER BY");
            _tt.SetToolTip(txtTableToSave, "Tabla donde se guardarán los datos");
            _tt.SetToolTip(txtSqlIdentityKey, "Campo identity de la tabla");
            _tt.SetToolTip(txtCountableCol, "Columna usada como contador");
            _tt.SetToolTip(txtKeyField, "Campo clave primaria");
            _tt.SetToolTip(txtForeignKey, "Campo(s) clave foránea");
            _tt.SetToolTip(txtForeignSave, "Indica si se guarda la FK");
            _tt.SetToolTip(txtForeignAlias, "Alias de la tabla relacionada");

            // Checkboxes
            _tt.SetToolTip(chkProcedure, "Indica si el origen es un procedimiento almacenado");
            _tt.SetToolTip(chkEditable, "Permite edición global del grid");

            // RichTextBox
            _tt.SetToolTip(richSelect, "Sección FROM + JOIN del SQL");
            _tt.SetToolTip(richSalida, "Vista previa del archivo .dat generado");

            // Grid completo
            _tt.SetToolTip(dgvColumns, "Configuración de columnas del grid");
        }

        private void InitGridTooltips()
        {
            dgvColumns.Columns["Pos"].ToolTipText = "Posición de la columna";
            dgvColumns.Columns["CampoSQL"].ToolTipText = "Campo SQL (ej: d.id)";
            dgvColumns.Columns["Alias"].ToolTipText = "Alias del campo";
            dgvColumns.Columns["DataType"].ToolTipText = "Tipo: N=Numérico, C=Texto, A=Auto, D=Fecha, B=Booleano";

            dgvColumns.Columns["Guardar"].ToolTipText = "Indica si se guarda en la BD";
            dgvColumns.Columns["Requerido"].ToolTipText = "Campo obligatorio";
            dgvColumns.Columns["Editable"].ToolTipText = "Permite edición";

            dgvColumns.Columns["Header"].ToolTipText = "Texto visible en la columna";
            dgvColumns.Columns["Width"].ToolTipText = "Ancho de la columna";
            dgvColumns.Columns["Format"].ToolTipText = "Formato (ej: #,###.##)";

            dgvColumns.Columns["Ellipsis"].ToolTipText = "Activa botón de búsqueda (...)";
            dgvColumns.Columns["EllipsisSqlSources"].ToolTipText = "Consulta SQL del popup";
            dgvColumns.Columns["EllipsisColsWidths"].ToolTipText = "Anchos en popup";
            dgvColumns.Columns["EllipsisColsHeaders"].ToolTipText = "Encabezados en popup";
            dgvColumns.Columns["EllipsisColsToShowfrmSearch"].ToolTipText = "Columnas visibles en búsqueda";
            dgvColumns.Columns["EllipsisColsToAsign"].ToolTipText = "Campos que se asignan al seleccionar";
            dgvColumns.Columns["EllipsisColsToAlias"].ToolTipText = "Alias de campos asignados";

            dgvColumns.Columns["Validate"].ToolTipText = "Activa validación";
            dgvColumns.Columns["ColsSqlValidToken"].ToolTipText = "SQL de validación";
            dgvColumns.Columns["ColsToAsignValuesToken"].ToolTipText = "Valores a asignar en validación";

            dgvColumns.Columns["InsNewRowAfter"].ToolTipText = "Inserta nueva fila después";
            dgvColumns.Columns["SubTotal"].ToolTipText = "Incluye en subtotales";
        }
    }//Form
}//namespace
