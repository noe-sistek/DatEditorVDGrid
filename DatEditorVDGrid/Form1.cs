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
        // Encapsulated preserved .dat state and helpers
        private readonly DatOptions _options = new DatOptions();

        // Syntax/coloring settings for SQL keywords and output separators
        private readonly string[] _sqlKeywords = { "SELECT", "FROM", "WHERE", "ORDER", "BY", "JOIN", "LEFT", "RIGHT", "INNER", "OUTER", "ON", "AS", "AND", "OR", "NOT", "LIKE", "IS", "NULL" };
        private readonly Color _joinColor = Color.Blue;
        private readonly Color _keywordColor = Color.Green;
        private readonly Color _separatorColor = Color.Orange;

        // guard to avoid recursive TextChanged events when we update RichTextBox.Text programmatically
        private bool _suppressRichSelectTextChanged = false;

        private ToolTip _tt;

        public Form1()
        {
            InitializeComponent();
            this.Load += Form1_Load;

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            dgvColumns.Columns.Clear();

            dgvColumns.AllowUserToAddRows = false;
            dgvColumns.ShowCellToolTips = true;

            // Core column order
            dgvColumns.Columns.Add("Pos", "Pos");
            dgvColumns.Columns.Add("CampoSQL", "Campo SQL");
            dgvColumns.Columns.Add("Alias", "Alias");

            var colType = new DataGridViewComboBoxColumn();
            colType.Name = "DataType";
            colType.HeaderText = "Tipo";
            colType.Items.AddRange("N", "C", "A", "D", "B");
            dgvColumns.Columns.Add(colType);

            dgvColumns.Columns.Add(new DataGridViewCheckBoxColumn()
            {
                Name = "Guardar",
                HeaderText = "Guardar"
            });

            dgvColumns.Columns.Add(new DataGridViewCheckBoxColumn()
            {
                Name = "Requerido",
                HeaderText = "Requerido"
            });


            dgvColumns.Columns.Add(new DataGridViewCheckBoxColumn()
            {
                Name = "Editable",
                HeaderText = "Editable"
            });

            dgvColumns.Columns.Add("Header", "Header");
            dgvColumns.Columns.Add("Width", "Width");

            dgvColumns.Columns.Add("Format", "Formato");

            // Ellipsis-related columns: ORDERED to match [Ellipsiscols] properties order
            // 1) Which ones -> checkbox per-row
            dgvColumns.Columns.Add(new DataGridViewCheckBoxColumn()
            {
                Name = "Ellipsis",
                HeaderText = "Ellipsis"
            });

            // 2) EllipsisSqlSources (per-row sources token)
            dgvColumns.Columns.Add(new DataGridViewTextBoxColumn()
            {
                Name = "EllipsisSqlSources",
                HeaderText = "EllipsisSqlSources"
            });

            // 3) EllipsisColsWidths
            dgvColumns.Columns.Add(new DataGridViewTextBoxColumn()
            {
                Name = "EllipsisColsWidths",
                HeaderText = "EllipsisColsWidths"
            });

            // 4) EllipsisColsHeaders
            dgvColumns.Columns.Add(new DataGridViewTextBoxColumn()
            {
                Name = "EllipsisColsHeaders",
                HeaderText = "EllipsisColsHeaders"
            });

            // 5) EllipsisColsToShowfrmSearch (per-row token; may be loaded from a global CSV too)
            dgvColumns.Columns.Add(new DataGridViewTextBoxColumn()
            {
                Name = "EllipsisColsToShowfrmSearch",
                HeaderText = "EllipsisColsToShowfrmSearch"
            });

            // 6) EllipsisColsToAsign (new) - per-row token
            dgvColumns.Columns.Add(new DataGridViewTextBoxColumn()
            {
                Name = "EllipsisColsToAsign",
                HeaderText = "EllipsisColsToAsign"
            });

            // 7) EllipsisColsToAlias (new) - per-row token
            dgvColumns.Columns.Add(new DataGridViewTextBoxColumn()
            {
                Name = "EllipsisColsToAlias",
                HeaderText = "EllipsisColsToAlias"
            });

            // Validate-related columns: allow per-row edit of validate tokens
            // 1) Validate flag (checkbox)
            dgvColumns.Columns.Add(new DataGridViewCheckBoxColumn()
            {
                Name = "Validate",
                HeaderText = "Validate"
            });

            // 2) ColsSqlValid per-row token (joined with ~ when writing)
            dgvColumns.Columns.Add(new DataGridViewTextBoxColumn()
            {
                Name = "ColsSqlValidToken",
                HeaderText = "ColsSqlValid"
            });

            // 3) ColsToAsignValues per-row token
            dgvColumns.Columns.Add(new DataGridViewTextBoxColumn()
            {
                Name = "ColsToAsignValuesToken",
                HeaderText = "ColsToAsignValues"
            });

            // Behavior: per-row flag to include this field in InsNewRowAfterCols
            dgvColumns.Columns.Add(new DataGridViewCheckBoxColumn()
            {
                Name = "InsNewRowAfter",
                HeaderText = "InsNewRowAfter"
            });

            // SubTotals: per-row checkbox to mark fields to be included as global totals
            dgvColumns.Columns.Add(new DataGridViewCheckBoxColumn()
            {
                Name = "SubTotal",
                HeaderText = "SubTotal"
            });

            // Ensure edits commit immediately for checkbox handling
            dgvColumns.CurrentCellDirtyStateChanged += (s, ev) =>
            {
                DataGridHelper.CommitEdit(dgvColumns);
            };
            dgvColumns.CellValueChanged += (s, ev) =>
            {
                if (ev.RowIndex < 0 || ev.ColumnIndex < 0)
                    return;

                var colName = dgvColumns.Columns[ev.ColumnIndex].Name;

                if (colName == "Ellipsis")
                {
                    DataGridHelper.HandleEllipsisChange(dgvColumns, ev.RowIndex);
                }
            };

            dgvColumns.Columns["Pos"].Frozen = true;
            dgvColumns.Columns["CampoSQL"].Frozen = true;
            dgvColumns.Columns["Pos"].DefaultCellStyle.BackColor = Color.White;
            dgvColumns.Columns["CampoSQL"].DefaultCellStyle.BackColor = Color.White;
            //TooltipHelper.AttachTooltips(this);
            // Inicializar ToolTip
            _tt = new ToolTip();
            _tt.AutoPopDelay = 5000;
            _tt.InitialDelay = 500;
            _tt.ReshowDelay = 200;
            _tt.ShowAlways = true;

            // Cargar tooltips
            InitTooltips();
            InitGridTooltips();

            // Evento para tooltips dinámicos en el grid
            dgvColumns.CellToolTipTextNeeded += dgvColumns_CellToolTipTextNeeded;
        }
        private void btnAddRow_Click(object sender, EventArgs e)
        {
            int rowIndex = dgvColumns.Rows.Add();
            dgvColumns.Rows[rowIndex].Cells["Pos"].Value = rowIndex + 1;

            // Leave CampoSQL empty so user fills it
            dgvColumns.Rows[rowIndex].Cells["CampoSQL"].Value = "";
        }

        private void richSelect_TextChanged(object sender, EventArgs e)
        {
            if (_suppressRichSelectTextChanged)
                return;

            // Only process when last character is space/newline or when text shortened
            if (richSelect.Text.Length > 0 &&
                richSelect.Text[richSelect.Text.Length - 1] != ' ' && !richSelect.Text.EndsWith("\n"))
            {
                return;
            }

            RichTextHelper.ApplySqlKeywordHighlight(
                richSelect,
                _sqlKeywords,
                _joinColor,
                _keywordColor,
                ref _suppressRichSelectTextChanged,
                true
            );
        }
        private void dgvColumns_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }
        private void tableLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void txtExport_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(richSalida.Text))
            {
                MessageBox.Show("No hay contenido para exportar.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = "DAT files (*.dat)|*.dat|All files (*.*)|*.*";
                sfd.Title = "Exportar a archivo DAT";
                sfd.DefaultExt = "dat";
                
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    System.IO.File.WriteAllText(sfd.FileName, richSalida.Text);
                    MessageBox.Show("Archivo exportado correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void btnQuery_Click(object sender, EventArgs e)
        {
            var filas = dgvColumns.Rows
                .Cast<DataGridViewRow>()
                .Where(r => !string.IsNullOrWhiteSpace(r.Cells["CampoSQL"].Value?.ToString()))
                .ToList();

            if (filas.Count == 0)
            {
                richSalida.Text = "-- No hay campos seleccionados para la consulta.";
                return;
            }

            var selectParts = filas.Select(row => {
                string campo = row.Cells["CampoSQL"].Value?.ToString().Trim();
                string alias = row.Cells["Alias"].Value?.ToString().Trim();
                return string.IsNullOrWhiteSpace(alias) ? campo : $"{campo} AS {alias}";
            });

            richSalida.Text = DatWriterHelper.BuildFormattedQuery(
                selectParts,
                richSelect.Text,
                txtWhereSql.Text,
                txtOrderSql.Text
            );

            // Aplicar resaltado de sintaxis SQL con colores vibrantes para fondo oscuro
            bool suppress = false;
            string[] allKeywords = _sqlKeywords.Concat(new[] { "GROUP", "HAVING" }).ToArray();

            RichTextHelper.ApplySqlKeywordHighlight(
                richSalida,
                allKeywords,
                RichTextHelper.VibrantColors[2], // Azul vibrante
                RichTextHelper.VibrantColors[3], // Amarillo vibrante
                ref suppress,
                true
            );
        }
    }//Form
}//namespace
