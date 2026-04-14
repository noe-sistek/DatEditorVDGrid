using System;
using System.Collections.Generic;
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
        // Holds a global EllipsisColsToShowfrmSearch value when the .dat provides a single long CSV
        private string _globalEllipsisColsToShow = "";

        // preserve full loaded values for EllipsisColsToAsign / EllipsisColsToAlias so we can
        // write them back unchanged (split into numbered lines respecting 255 chars).
        private string _fullEllipsisColsToAsign = "";
        private string _fullEllipsisColsToAlias = "";

        // Syntax/coloring settings for SQL keywords and output separators
        private readonly string[] _sqlKeywords = { "FROM", "JOIN", "LEFT", "RIGHT", "INNER", "OUTER", "ON", "AS" };
        private readonly Color _joinColor = Color.Blue;
        private readonly Color _keywordColor = Color.Green;
        private readonly Color _separatorColor = Color.Orange;

        // guard to avoid recursive TextChanged events when we update RichTextBox.Text programmatically
        private bool _suppressRichSelectTextChanged = false;

        public Form1()
        {
            InitializeComponent();
            this.Load += Form1_Load;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            dgvColumns.Columns.Clear();

            dgvColumns.AllowUserToAddRows = false;

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


            // Ensure edits commit immediately for checkbox handling
            dgvColumns.CurrentCellDirtyStateChanged += dgvColumns_CurrentCellDirtyStateChanged;
            dgvColumns.CellValueChanged += dgvColumns_CellValueChanged;
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        { }

        private void btnAddRow_Click(object sender, EventArgs e)
        {
            int rowIndex = dgvColumns.Rows.Add();
            dgvColumns.Rows[rowIndex].Cells["Pos"].Value = rowIndex + 1;

            // Leave CampoSQL empty so user fills it
            dgvColumns.Rows[rowIndex].Cells["CampoSQL"].Value = "";
        }

        private void btnGenerate_Click(object sender, EventArgs e)
        {
            // Only include rows with a non-empty CampoSQL (avoid generating many empty tokens)
            var filas = dgvColumns.Rows
                .Cast<DataGridViewRow>()
                .Where(r => !string.IsNullOrWhiteSpace(r.Cells["CampoSQL"].Value?.ToString()))
                .ToList();

            var selectParts = new List<string>();
            var dataTypes = new List<string>();
            var fieldsToSave = new List<string>();
            var required = new List<string>();
            var headers = new List<string>();
            var widths = new List<string>();
            var formats = new List<string>();

            // Build ellipsis per-row collections from the grid; if none present, we'll fallback to _full...
            var ellipsisWhichOnes = new List<string>();
            var ellipsisSqlSources = new List<string>();
            var ellipsisColsWidths = new List<string>();
            var ellipsisColsHeaders = new List<string>();
            var ellipsisColsToShow = new List<string>();
            var ellipsisColsToAsign = new List<string>();
            var ellipsisColsToAlias = new List<string>();

            foreach (var row in filas)
            {
                string campo = row.Cells["CampoSQL"].Value?.ToString();
                string alias = row.Cells["Alias"].Value?.ToString();
                string tipo = row.Cells["DataType"].Value?.ToString() ?? "C";

                bool guardar = Convert.ToBoolean(row.Cells["Guardar"].Value ?? false);
                bool req = Convert.ToBoolean(row.Cells["Requerido"].Value ?? false);

                string header = row.Cells["Header"].Value?.ToString() ?? "";
                string width = row.Cells["Width"].Value?.ToString() ?? "0";

                string format = row.Cells["Format"].Value?.ToString() ?? "0";
                bool ellipsis = Convert.ToBoolean(row.Cells["Ellipsis"].Value ?? false);

                // per-row ellipsis cells (may be empty)
                string ellSrc = row.Cells["EllipsisSqlSources"].Value?.ToString() ?? "";
                string eWidth = row.Cells["EllipsisColsWidths"].Value?.ToString() ?? "";
                string eHeader = row.Cells["EllipsisColsHeaders"].Value?.ToString() ?? "";
                string eShow = row.Cells["EllipsisColsToShowfrmSearch"].Value?.ToString() ?? "";
                string eAsign = row.Cells["EllipsisColsToAsign"].Value?.ToString() ?? "";
                string eAlias = row.Cells["EllipsisColsToAlias"].Value?.ToString() ?? "";

                // select parts
                selectParts.Add(string.IsNullOrWhiteSpace(alias) ? $"{campo}" : $"{campo} {alias}");
                dataTypes.Add(tipo);
                fieldsToSave.Add(guardar ? "1" : "0");
                required.Add(req ? "1" : "0");
                headers.Add(header);
                widths.Add(width);
                formats.Add(format);

                // collect ellipsis tokens per row
                ellipsisWhichOnes.Add(ellipsis ? "1" : "0");
                ellipsisSqlSources.Add(ellSrc);
                ellipsisColsWidths.Add(eWidth);
                ellipsisColsHeaders.Add(eHeader);
                ellipsisColsToShow.Add(eShow);
                ellipsisColsToAsign.Add(eAsign);
                ellipsisColsToAlias.Add(eAlias);
            }

            // Trim trailing empty tokens for per-row lists (only for things using ~ separators)
            TrimTrailingEmpty(ellipsisSqlSources);
            TrimTrailingEmpty(ellipsisColsWidths);
            TrimTrailingEmpty(ellipsisColsHeaders);
            TrimTrailingEmpty(ellipsisColsToShow);
            TrimTrailingEmpty(ellipsisColsToAsign);
            TrimTrailingEmpty(ellipsisColsToAlias);

            // construir SELECT respetando que los campos no contengan coma adicional
            string selectClause = "SELECT " + string.Join(",", selectParts);
            // use richSelect (independent) for FROM/JOIN content
            string fromJoins = richSelect.Text?.Trim() ?? "";
            string fullSql = string.IsNullOrWhiteSpace(fromJoins) ? selectClause : (selectClause + " " + fromJoins);

            var sb = new StringBuilder();

            // --- Build full file with all base keys and sections in the exact order requested ---
            // [Source]
            sb.AppendLine("[Source]");

            // SourceSql..SourceSql7 (always present, empty if not set)
            WriteNumberedKeys(sb, "SourceSql", fullSql, 2);

            // Procedure (leave blank if not configured)
            string procedureVal = chkProcedure.Checked ? "1" : "";
            sb.AppendLine($"Procedure={procedureVal}");

            // Where & Order & TabletoSave
            // Use single-line keys (empty if no config)
            sb.AppendLine($"WhereSql={txtWhereSql.Text?.Trim() ?? ""}");
            sb.AppendLine($"OrderSql={txtOrderSql.Text?.Trim() ?? ""}");
            sb.AppendLine($"TabletoSave={txtTableToSave.Text?.Trim() ?? ""}");

            // DataTypes / FieldsToSave
            string dataTypesVal = (dataTypes.Count > 0) ? string.Join(",", dataTypes) : "";
            string fieldsToSaveVal = (fieldsToSave.Count > 0) ? string.Join(",", fieldsToSave) : "";
            sb.AppendLine($"DataTypes={dataTypesVal}");
            sb.AppendLine($"FieldsToSave={fieldsToSaveVal}");

            // SqlIdentityKey, CountableCol, KeyField
            sb.AppendLine($"SqlIdentityKey={txtSqlIdentityKey.Text?.Trim() ?? ""}");
            sb.AppendLine($"CountableCol={txtCountableCol.Text?.Trim() ?? ""}");
            sb.AppendLine($"KeyField={txtKeyField.Text?.Trim() ?? ""}");

            // ForeignKey / ForeignToSave / ForeignAlias
            string foreignKeyStr = txtForeignKey.Text?.Trim() ?? "";
            string foreignToSaveStr = txtForeignSave.Text?.Trim() ?? "";
            sb.AppendLine($"ForeignKey={foreignKeyStr}");
            sb.AppendLine($"ForeignToSave={foreignToSaveStr}");

            string foreignAliasStr = txtForeignAlias.Text?.Trim() ?? "";
            if (string.IsNullOrEmpty(foreignAliasStr) && !string.IsNullOrEmpty(foreignKeyStr))
            {
                string inferredAlias = ExtractMainTableAliasFromFromPart(fromJoins);
                var fkTokens = foreignKeyStr.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => s.Trim())
                    .ToArray();
                if (!string.IsNullOrEmpty(inferredAlias))
                {
                    foreignAliasStr = string.Join(",", fkTokens.Select(_ => inferredAlias));
                }
                else
                {
                    foreignAliasStr = string.Join(",", fkTokens.Select(_ => ""));
                }
            }
            sb.AppendLine($"ForeignAlias={foreignAliasStr}");

            // RequiredFields
            string requiredVal = (required.Count > 0) ? string.Join(",", required) : "";
            sb.AppendLine($"RequiredFields={requiredVal}");

            // [MaskedCols]
            sb.AppendLine();
            sb.AppendLine("[MaskedCols]");
            sb.AppendLine($"ColComboSqlSources=");
            sb.AppendLine($"ColComboEditables=");

            // [Ellipsiscols]
            sb.AppendLine();
            sb.AppendLine("[Ellipsiscols]");

            // EllipsisWhichOnes (comma list)
            string ellipsisWhichVal = (ellipsisWhichOnes.Count > 0) ? string.Join(",", ellipsisWhichOnes) : "";
            sb.AppendLine($"EllipsisWhichOnes={ellipsisWhichVal}");

            // EllipsisSqlSources..3 (always present up to 6 keys)
            // Use per-row ellipsisSqlSources joined by '~' OR per-row tokens; prefer per-row tokens processed above
            string ellipsisSqlSourcesJoined = (ellipsisSqlSources.Count > 0) ? string.Join("~", ellipsisSqlSources) : "";
            // If none per-row but there may be a global _globalEllipsisColsToShow we won't use it here.
            WriteNumberedKeys(sb, "EllipsisSqlSources", ellipsisSqlSourcesJoined, 2);

            // EllipsisColsWidths (single key with possible ~-joined tokens)
            string ellipsisColsWidthsJoined = (ellipsisColsWidths.Count > 0) ? string.Join("~", ellipsisColsWidths) : "";
            sb.AppendLine($"EllipsisColsWidths={ellipsisColsWidthsJoined}");

            // EllipsisColsHeaders and additional two numbered header keys (Headers, Headers2, Headers3)
            // We'll join per-row values with '~' and then distribute across the available header keys if needed.
            string ellipsisColsHeadersJoined = (ellipsisColsHeaders.Count > 0) ? string.Join("~", ellipsisColsHeaders) : "";
            WriteNumberedKeys(sb, "EllipsisColsHeaders", ellipsisColsHeadersJoined, 2);

            // EllipsisColsToShowfrmSearch (single key)
            string ellipsisColsToShowJoined = "";
            if (ellipsisColsToShow.Any(s => !string.IsNullOrWhiteSpace(s)))
                ellipsisColsToShowJoined = string.Join("~", ellipsisColsToShow);
            else if (!string.IsNullOrWhiteSpace(_globalEllipsisColsToShow))
                ellipsisColsToShowJoined = _globalEllipsisColsToShow;
            sb.AppendLine($"EllipsisColsToShowfrmSearch={ellipsisColsToShowJoined}");

            // EllipsisColsToAsign (three keys)
            string ellipsisColsToAsignJoined = (ellipsisColsToAsign.Count > 0) ? string.Join("~", ellipsisColsToAsign) : _fullEllipsisColsToAsign ?? "";
            WriteNumberedKeys(sb, "EllipsisColsToAsign", ellipsisColsToAsignJoined, 2);

            // EllipsisColsToAlias (single key)
            string ellipsisColsToAliasJoined = (ellipsisColsToAlias.Count > 0) ? string.Join("~", ellipsisColsToAlias) : _fullEllipsisColsToAlias ?? "";
            sb.AppendLine($"EllipsisColsToAlias={ellipsisColsToAliasJoined}");

            // Additional Ellipsis display/invoke keys (empty if not configured)
            sb.AppendLine($"EllipsisColInvokeColor=");
            sb.AppendLine($"EllipsisColInvokeFont=");
            sb.AppendLine($"EllipsisColsNewFields=");
            sb.AppendLine($"EllipsisColsNewCaption=");
            sb.AppendLine($"EllipsisColsNewDataTypes=");
            sb.AppendLine($"EllipsisColsNewTableNames=");
            sb.AppendLine($"EllipsisColInvokeOpen=");
            sb.AppendLine($"EllipsisColOpenFilters=");
            sb.AppendLine($"EllipsisColInvokeBrowse=");
            sb.AppendLine($"EllipsisColShellExec=");

            // [Formatting]
            sb.AppendLine();
            sb.AppendLine("[Formatting]");

            // Headers (up to 4 keys)
            string headersJoined = (headers.Count > 0) ? string.Join(",", headers.Where(h => !string.IsNullOrWhiteSpace(h))) : "";
            WriteNumberedKeys(sb, "Headers", headersJoined, 2);

            // Widths, Height, ResizeCols, ResizeRows, MoveCols, Editable, ColsToEdit, FormatStrings, etc.
            sb.AppendLine($"Widths={string.Join(",", widths)}");
            sb.AppendLine($"Height=");
            sb.AppendLine($"ResizeCols=");
            sb.AppendLine($"ResizeRows=");
            sb.AppendLine($"MoveCols=");
            sb.AppendLine($"Editable={(chkEditable.Checked ? "1" : "0")}");

            // Build ColsToEdit: use Alias when present, otherwise CampoSQL; ignore empty values
            var colsToEditList = filas
                .Where(r => Convert.ToBoolean(r.Cells["Editable"].Value ?? false))
                .Select(r =>
                {
                    var alias = (r.Cells["Alias"].Value ?? "").ToString().Trim();
                    if (!string.IsNullOrEmpty(alias))
                        return alias;
                    return (r.Cells["CampoSQL"].Value ?? "").ToString().Trim();
                })
                .Where(s => !string.IsNullOrEmpty(s))
                .ToList();

            string colsToEdit = colsToEditList.Count > 0 ? string.Join(",", colsToEditList) : "";
            sb.AppendLine($"ColsToEdit={colsToEdit}");

            // FormatStrings (use ~ for per-row format strings)
            sb.AppendLine($"FormatStrings={string.Join("~", formats)}");

            // More formatting flags
            sb.AppendLine($"ExplorerBar=");
            sb.AppendLine($"AutoSearch=");
            sb.AppendLine($"SelectFullRow=");
            sb.AppendLine($"MergeCells=");
            sb.AppendLine($"OutlineBar=");
            sb.AppendLine($"BlankLinesOnEdit=");
            sb.AppendLine($"FrozenRows=");
            sb.AppendLine($"FrozenCols=");
            sb.AppendLine($"WordWrap=");

            // [SubTotals]
            sb.AppendLine();
            sb.AppendLine("[SubTotals]");
            sb.AppendLine($"Totales=");
            sb.AppendLine($"MultipleSubTotals=");
            sb.AppendLine($"SubTotalsGroups=");
            sb.AppendLine($"SubTotalsSummary=");
            sb.AppendLine($"SubTotalBkColors=");
            sb.AppendLine($"SubTotalCaptions=");
            sb.AppendLine($"SubTotalsEditable=");
            sb.AppendLine($"DetailEditable=");

            // [Behavior]
            sb.AppendLine();
            sb.AppendLine("[Behavior]");
            sb.AppendLine($"UpperCaseConvert=");
            sb.AppendLine($"AllowEnterToTab=");
            sb.AppendLine($"EnterActionNextCol=");
            sb.AppendLine($"EnterctionNextRow=");
            sb.AppendLine($"GoLastRowOnLoad=");
            sb.AppendLine($"ShowColToolTips=");
            sb.AppendLine($"DoubleClickAction=");
            sb.AppendLine($"ReturnToCallerForm=");
            sb.AppendLine($"CallOtherForm=");
            sb.AppendLine($"FormToCall=");
            sb.AppendLine($"FieldToSend=");
            sb.AppendLine($"NewRowColor=");
            sb.AppendLine($"ModRowColor=");
            sb.AppendLine($"DelRowColor=");
            sb.AppendLine($"AddAutomatically=");
            sb.AppendLine($"BKColorByRow=");
            sb.AppendLine($"FKColorNegativeValues=");
            sb.AppendLine($"InsNewRowAfterCols=");

            // [Validate]
            sb.AppendLine();
            sb.AppendLine("[Validate]");
            sb.AppendLine($"ColsSqlValid=");
            sb.AppendLine($"ColsToAsignValues=");

            // show generated .dat inside richSalida only
            richSalida.Text = sb.ToString();

            // Only color separator characters (commas and tildes). Do not change background/overall forecolor here.
            HighlightSeparators(richSalida, new[] { ',', '~' }, _separatorColor);
        }

        private void btnValidate_Click(object sender, EventArgs e)
        {
            var errores = new List<string>();

            var filas = dgvColumns.Rows
                .Cast<DataGridViewRow>()
                .Where(r => r.Cells["CampoSQL"].Value != null)
                .ToList();

            foreach (var row in filas)
            {
                string campo = row.Cells["CampoSQL"].Value?.ToString();
                if (string.IsNullOrWhiteSpace(campo))
                    errores.Add("CampoSQL vacío");
            }

            if (errores.Count == 0)
                MessageBox.Show("Todo OK", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Information);
            else
                MessageBox.Show(string.Join("\n", errores), "Errores", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private Dictionary<string, string> ParseDatFile(string path)
        {
            var dict = new Dictionary<string, string>();
            var lines = System.IO.File.ReadAllLines(path);

            string currentSection = "";

            foreach (var line in lines)
            {
                if (line.StartsWith("["))
                {
                    currentSection = line.Trim();
                    continue;
                }

                if (string.IsNullOrWhiteSpace(line) || !line.Contains("="))
                    continue;

                var parts = line.Split(new[] { '=' }, 2);
                string key = parts[0].Trim();
                string value = parts[1].Trim();

                dict[key] = value;
            }

            return dict;
        }

        private void btnLoadDat_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "DAT files (*.dat)|*.dat";

            if (ofd.ShowDialog() != DialogResult.OK)
                return;

            var data = ParseDatFile(ofd.FileName);

            dgvColumns.Rows.Clear();

            if (!data.ContainsKey("SourceSql"))
            {
                MessageBox.Show("No se encontró SourceSql");
                return;
            }

            // reset global ellipsis-per-popup holder
            _globalEllipsisColsToShow = "";
            _fullEllipsisColsToAsign = "";
            _fullEllipsisColsToAlias = "";

            var fullSource = GetFullProperty(data, "SourceSql");

            // separar SELECT vs FROM+JOINS
            SplitSelectFrom(fullSource, out string selectOnly, out string fromPart);

            // si el .dat trae TabletoSave o TableToSave, preferirla; si no, extraer la tabla principal desde FROM
            string tableVal = "";
            if (data.ContainsKey("TabletoSave"))
                tableVal = GetFullProperty(data, "TabletoSave");
            else if (data.ContainsKey("TableToSave"))
                tableVal = GetFullProperty(data, "TableToSave");

            if (!string.IsNullOrWhiteSpace(tableVal))
                txtTableToSave.Text = tableVal;
            else
                txtTableToSave.Text = ExtractMainTableFromFromPart(fromPart);

            // completar controles de configuración desde .dat
            txtWhereSql.Text = data.ContainsKey("WhereSql") ? GetFullProperty(data, "WhereSql") : "";
            txtOrderSql.Text = data.ContainsKey("OrderSql") ? GetFullProperty(data, "OrderSql") : "";
            // KeyField
            txtKeyField.Text = data.ContainsKey("KeyField") ? GetFullProperty(data, "KeyField") : "";
            // ForeignKey / ForeignToSave
            txtForeignKey.Text = data.ContainsKey("ForeignKey") ? GetFullProperty(data, "ForeignKey") : "";
            txtForeignSave.Text = data.ContainsKey("ForeignToSave") ? GetFullProperty(data, "ForeignToSave") : "";
            // Procedure flag (if provided)
            if (data.ContainsKey("Procedure"))
                chkProcedure.Checked = GetFullProperty(data, "Procedure").Trim() != "0";
            // SqlIdentityKey, CountableCol, ForeignAlias
            txtSqlIdentityKey.Text = data.ContainsKey("SqlIdentityKey") ? GetFullProperty(data, "SqlIdentityKey") : "";
            txtCountableCol.Text = data.ContainsKey("CountableCol") ? GetFullProperty(data, "CountableCol") : "";
            txtForeignAlias.Text = data.ContainsKey("ForeignAlias") ? GetFullProperty(data, "ForeignAlias") : "";
            chkEditable.Checked = data.ContainsKey("Editable") && GetFullProperty(data, "Editable").Trim() == "1";

            // NEW: load EllipsisColsToAsign / EllipsisColsToAlias full values for later re-output
            if (data.ContainsKey("EllipsisColsToAsign"))
                _fullEllipsisColsToAsign = GetFullProperty(data, "EllipsisColsToAsign");
            if (data.ContainsKey("EllipsisColsToAlias"))
                _fullEllipsisColsToAlias = GetFullProperty(data, "EllipsisColsToAlias");

            // From+Joins se muestra en richSelect (control independiente)
            try
            {
                _suppressRichSelectTextChanged = true;
                richSelect.Text = fromPart?.Trim() ?? "";
                // ensure reserved words appear uppercased + colored from the start
                ApplySqlKeywordHighlight(richSelect, uppercaseKeywords: true);
            }
            finally
            {
                _suppressRichSelectTextChanged = false;
            }

            // reconstruir lista de campos del SELECT sin el prefijo SELECT
            var sourceBody = selectOnly ?? fullSource;
            if (sourceBody.TrimStart().StartsWith("SELECT", StringComparison.OrdinalIgnoreCase))
            {
                sourceBody = sourceBody.TrimStart().Substring(6);
            }
            sourceBody = sourceBody.Trim();

            var campos = SplitSqlColumns(sourceBody);
            var dataTypes = data.ContainsKey("DataTypes") ? GetFullProperty(data, "DataTypes").Split(',') : new string[0];
            var save = data.ContainsKey("FieldsToSave") ? GetFullProperty(data, "FieldsToSave").Split(',') : new string[0];
            var req = data.ContainsKey("RequiredFields") ? GetFullProperty(data, "RequiredFields").Split(',') : new string[0];
            var headers = data.ContainsKey("Headers") ? GetFullProperty(data, "Headers").Split(',') : new string[0];
            var widths = data.ContainsKey("Widths") ? GetFullProperty(data, "Widths").Split(',') : new string[0];
            var formats = data.ContainsKey("FormatStrings") ? GetFullProperty(data, "FormatStrings").Split('~') : new string[0];
            var ellipsis = data.ContainsKey("EllipsisWhichOnes") ? GetFullProperty(data, "EllipsisWhichOnes").Split(',') : new string[0];

            // NEW: load EllipsisSqlSources and ColsToEdit combined properties and new ellipsis columns
            var fullEllipsisSources = data.ContainsKey("EllipsisSqlSources") ? GetFullProperty(data, "EllipsisSqlSources") : "";
            var ellipsisTokens = string.IsNullOrEmpty(fullEllipsisSources)
                ? new string[0]
                : fullEllipsisSources.Split(new[] { '~' }, StringSplitOptions.None);

            var fullColsToEdit = data.ContainsKey("ColsToEdit") ? GetFullProperty(data, "ColsToEdit") : "";
            var colsToEditTokens = string.IsNullOrWhiteSpace(fullColsToEdit)
                ? new string[0]
                : fullColsToEdit.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => s.Trim())
                    .ToArray();

            var fullEllipsisColsWidths = data.ContainsKey("EllipsisColsWidths") ? GetFullProperty(data, "EllipsisColsWidths") : "";
            var ellipsisColsWidthsTokens = string.IsNullOrEmpty(fullEllipsisColsWidths)
                ? new string[0]
                : fullEllipsisColsWidths.Split(new[] { '~' }, StringSplitOptions.None);

            var fullEllipsisColsHeaders = data.ContainsKey("EllipsisColsHeaders") ? GetFullProperty(data, "EllipsisColsHeaders") : "";
            var ellipsisColsHeadersTokens = string.IsNullOrEmpty(fullEllipsisColsHeaders)
                ? new string[0]
                : fullEllipsisColsHeaders.Split(new[] { '~' }, StringSplitOptions.None);

            var fullEllipsisColsToShow = data.ContainsKey("EllipsisColsToShowfrmSearch") ? GetFullProperty(data, "EllipsisColsToShowfrmSearch") : "";
            var ellipsisColsToShowTokens = string.IsNullOrEmpty(fullEllipsisColsToShow)
                ? new string[0]
                : fullEllipsisColsToShow.Split(new[] { '~' }, StringSplitOptions.None);

            // If there's only one token and it contains commas, treat it as the global CSV for the popup
            // -> ALSO populate per-row column so values are visible/editable in the grid.
            if (ellipsisColsToShowTokens.Length == 1 && ellipsisColsToShowTokens[0].Contains(","))
            {
                _globalEllipsisColsToShow = ellipsisColsToShowTokens[0];
                // try to split by commas and assign each token to the corresponding row's cell
                var csvTokens = _globalEllipsisColsToShow.Split(new[] { ',' }, StringSplitOptions.None);
                // We'll still clear ellipsisColsToShowTokens so the per-row ~-joined value will be taken from cells
                ellipsisColsToShowTokens = new string[0];
            }

            // Load EllipsisColsToAsign / EllipsisColsToAlias tokens (full strings preserved already)
            var fullEllipsisColsToAsign = data.ContainsKey("EllipsisColsToAsign") ? GetFullProperty(data, "EllipsisColsToAsign") : "";
            var ellipsisColsToAsignTokens = string.IsNullOrEmpty(fullEllipsisColsToAsign)
                ? new string[0]
                : fullEllipsisColsToAsign.Split(new[] { '~' }, StringSplitOptions.None);

            var fullEllipsisColsToAlias = data.ContainsKey("EllipsisColsToAlias") ? GetFullProperty(data, "EllipsisColsToAlias") : "";
            var ellipsisColsToAliasTokens = string.IsNullOrEmpty(fullEllipsisColsToAlias)
                ? new string[0]
                : fullEllipsisColsToAlias.Split(new[] { '~' }, StringSplitOptions.None);

            // preserve full original strings for fallback
            if (!string.IsNullOrEmpty(fullEllipsisColsToAsign))
                _fullEllipsisColsToAsign = fullEllipsisColsToAsign;
            if (!string.IsNullOrEmpty(fullEllipsisColsToAlias))
                _fullEllipsisColsToAlias = fullEllipsisColsToAlias;

            // If we had a single global CSV for EllipsisColsToShowfrmSearch, create csvTokens now so we can assign per-row
            string[] ellipsisColsToShowGlobalCsvTokens = null;
            if (!string.IsNullOrEmpty(_globalEllipsisColsToShow))
                ellipsisColsToShowGlobalCsvTokens = _globalEllipsisColsToShow.Split(new[] { ',' }, StringSplitOptions.None);

            for (int i = 0; i < campos.Count; i++)
            {
                var parts = campos[i].Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                string campo = parts[0];
                string alias = parts.Length > 1 ? parts[1] : "";

                int rowIndex = dgvColumns.Rows.Add();

                dgvColumns.Rows[rowIndex].Cells["Pos"].Value = i + 1;

                // Do NOT force CampoSQL to uppercase; show as in the source
                dgvColumns.Rows[rowIndex].Cells["CampoSQL"].Value = campo;
                dgvColumns.Rows[rowIndex].Cells["Alias"].Value = alias;

                if (i < dataTypes.Length)
                    dgvColumns.Rows[rowIndex].Cells["DataType"].Value = dataTypes[i];

                if (i < save.Length)
                    dgvColumns.Rows[rowIndex].Cells["Guardar"].Value = save[i] == "1";

                if (i < req.Length)
                    dgvColumns.Rows[rowIndex].Cells["Requerido"].Value = req[i] == "1";

                if (i < headers.Length)
                    dgvColumns.Rows[rowIndex].Cells["Header"].Value = headers[i];

                if (i < widths.Length)
                    dgvColumns.Rows[rowIndex].Cells["Width"].Value = widths[i];

                if (i < formats.Length)
                    dgvColumns.Rows[rowIndex].Cells["Format"].Value = formats[i];

                if (i < ellipsis.Length)
                    dgvColumns.Rows[rowIndex].Cells["Ellipsis"].Value = ellipsis[i] == "1";

                // Assign EllipsisSqlSources token if present for this row
                if (i < ellipsisTokens.Length)
                    dgvColumns.Rows[rowIndex].Cells["EllipsisSqlSources"].Value = ellipsisTokens[i];
                else
                    dgvColumns.Rows[rowIndex].Cells["EllipsisSqlSources"].Value = "";

                // Assign EllipsisColsWidths token if present
                if (i < ellipsisColsWidthsTokens.Length)
                    dgvColumns.Rows[rowIndex].Cells["EllipsisColsWidths"].Value = ellipsisColsWidthsTokens[i];
                else
                    dgvColumns.Rows[rowIndex].Cells["EllipsisColsWidths"].Value = "";

                // Assign EllipsisColsHeaders token if present
                if (i < ellipsisColsHeadersTokens.Length)
                    dgvColumns.Rows[rowIndex].Cells["EllipsisColsHeaders"].Value = ellipsisColsHeadersTokens[i];
                else
                    dgvColumns.Rows[rowIndex].Cells["EllipsisColsHeaders"].Value = "";

                // Assign EllipsisColsToShowfrmSearch:
                if (ellipsisColsToShowTokens.Length > 0)
                {
                    // there were per-row ~ tokens in the .dat; assign
                    if (i < ellipsisColsToShowTokens.Length)
                        dgvColumns.Rows[rowIndex].Cells["EllipsisColsToShowfrmSearch"].Value = ellipsisColsToShowTokens[i];
                    else
                        dgvColumns.Rows[rowIndex].Cells["EllipsisColsToShowfrmSearch"].Value = "";
                }
                else if (ellipsisColsToShowGlobalCsvTokens != null)
                {
                    // distribute global CSV tokens to rows if possible
                    if (i < ellipsisColsToShowGlobalCsvTokens.Length)
                        dgvColumns.Rows[rowIndex].Cells["EllipsisColsToShowfrmSearch"].Value = ellipsisColsToShowGlobalCsvTokens[i];
                    else
                        dgvColumns.Rows[rowIndex].Cells["EllipsisColsToShowfrmSearch"].Value = "";
                }
                else
                {
                    dgvColumns.Rows[rowIndex].Cells["EllipsisColsToShowfrmSearch"].Value = "";
                }

                // Assign EllipsisColsToAsign per-row token if present in the .dat
                if (i < ellipsisColsToAsignTokens.Length)
                    dgvColumns.Rows[rowIndex].Cells["EllipsisColsToAsign"].Value = ellipsisColsToAsignTokens[i];
                else
                    dgvColumns.Rows[rowIndex].Cells["EllipsisColsToAsign"].Value = "";

                // Assign EllipsisColsToAlias per-row token if present in the .dat
                if (i < ellipsisColsToAliasTokens.Length)
                    dgvColumns.Rows[rowIndex].Cells["EllipsisColsToAlias"].Value = ellipsisColsToAliasTokens[i];
                else
                    dgvColumns.Rows[rowIndex].Cells["EllipsisColsToAlias"].Value = "";

                // If this column name (alias or campo) is listed in ColsToEdit => mark Editable
                bool shouldEdit = colsToEditTokens.Any(token =>
                    string.Equals(token, alias, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(token, campo, StringComparison.OrdinalIgnoreCase));

                dgvColumns.Rows[rowIndex].Cells["Editable"].Value = shouldEdit;
            }

            MessageBox.Show("DAT cargado correctamente");
        }

        private string GetFullProperty(Dictionary<string, string> data, string baseKey)
        {
            var parts = new List<string>();

            int i = 1;

            while (true)
            {
                string key = (i == 1) ? baseKey : $"{baseKey}{i}";

                if (!data.ContainsKey(key))
                    break;

                parts.Add(data[key]);
                i++;
            }

            return string.Join(" ", parts);
        }

        private List<string> SplitSqlColumns(string input)
        {
            var result = new List<string>();
            int parenLevel = 0;
            var current = new StringBuilder();

            foreach (char c in input)
            {
                if (c == '(') parenLevel++;
                if (c == ')') parenLevel--;

                if (c == ',' && parenLevel == 0)
                {
                    result.Add(current.ToString());
                    current.Clear();
                }
                else
                {
                    current.Append(c);
                }
            }

            if (current.Length > 0)
                result.Add(current.ToString());

            return result;
        }

        // Trim trailing empty or whitespace-only tokens from a list in-place.
        private void TrimTrailingEmpty(List<string> list)
        {
            for (int k = list.Count - 1; k >= 0; k--)
            {
                if (string.IsNullOrWhiteSpace(list[k]))
                    list.RemoveAt(k);
                else
                    break;
            }
        }

        // Divide una cadena en trozos procurando no cortar palabras ni campos:
        // - Primero intenta partir en la última coma dentro del límite.
        // - Si no hay coma, busca el último espacio dentro del límite.
        // - Si no hay donde partir, usa el límite máximo.
        // El parámetro maxChunkLength se refiere a la longitud máxima permitida para cada trozo
        // (excluyendo la longitud de la cabecera "Key=" que se maneja en AppendWrappedProperty).
        private List<string> SplitSqlIntoLines(string sql, int maxChunkLength = 255)
        {
            var result = new List<string>();

            sql = (sql ?? "").Trim();

            while (sql.Length > maxChunkLength)
            {
                int searchIndex = Math.Min(maxChunkLength - 1, sql.Length - 1);

                int commaPos = sql.LastIndexOf(',', searchIndex);
                if (commaPos > 0)
                {
                    // incluir la coma en el chunk; la longitud será commaPos+1 <= maxChunkLength
                    int len = commaPos + 1;
                    var chunk = sql.Substring(0, len).Trim();
                    result.Add(chunk);
                    sql = sql.Substring(len).Trim();
                    continue;
                }

                int spacePos = sql.LastIndexOf(' ', searchIndex);
                if (spacePos > 0)
                {
                    // no incluir el espacio final en el chunk
                    int len = spacePos;
                    var chunk = sql.Substring(0, len).Trim();
                    result.Add(chunk);
                    sql = sql.Substring(len).Trim();
                    continue;
                }

                // no encontramos coma ni espacio, partir en el límite
                int take = Math.Min(maxChunkLength, sql.Length);
                var forced = sql.Substring(0, take).Trim();
                result.Add(forced);
                sql = sql.Substring(take).Trim();
            }

            if (!string.IsNullOrWhiteSpace(sql))
                result.Add(sql);

            return result;
        }

        // Construye líneas numeradas (Key, Key2, Key3...) asegurando que cada línea (incluyendo "Key=") no supere 255 chars.
        private void AppendWrappedProperty(StringBuilder sb, string key, string value)
        {
            // Si null -> tratar como vacío
            value = value ?? "";

            int maxLine = 255;
            int prefixLength = key.Length + 1; // key + '='
            int chunkMax = System.Math.Max(1, maxLine - prefixLength);

            var chunks = SplitSqlIntoLines(value, chunkMax);

            if (chunks.Count == 0)
            {
                sb.AppendLine($"{key}=");
                return;
            }

            for (int i = 0; i < chunks.Count; i++)
            {
                if (i == 0)
                    sb.AppendLine($"{key}={chunks[i]}");
                else
                    sb.AppendLine($"{key}{i + 1}={chunks[i]}");
            }
        }

        // Helper: compute wrapped chunks for a given key respecting the 255 chars limit per full line (key + '=' + chunk).
        private List<string> GetWrappedChunks(string key, string value)
        {
            value = value ?? "";
            int maxLine = 255;
            int prefixLength = key.Length + 1; // key + '='
            int chunkMax = System.Math.Max(1, maxLine - prefixLength);
            return SplitSqlIntoLines(value, chunkMax);
        }

        // Helper: write numbered keys (Key, Key2, Key3...) using wrapped chunks distribution.
        // Writes at least totalSlots entries but will expand if the wrapped chunks exceed that number
        // so imported .dat content is not truncated.
        private void WriteNumberedKeys(StringBuilder sb, string baseKey, string value, int totalSlots)
        {
            var chunks = GetWrappedChunks(baseKey, value);
            int count = Math.Max(totalSlots, chunks.Count);
            for (int i = 0; i < count; i++)
            {
                string key = (i == 0) ? baseKey : $"{baseKey}{i + 1}";
                string val = (i < chunks.Count) ? chunks[i] : "";
                sb.AppendLine($"{key}={val}");
            }
        }

        private void SplitSelectFrom(string fullSql, out string selectPart, out string fromPart)
        {
            selectPart = fullSql;
            fromPart = "";

            var lower = (fullSql ?? "").ToLower();
            int idx = lower.IndexOf(" from ");
            if (idx >= 0)
            {
                selectPart = fullSql.Substring(0, idx);
                fromPart = fullSql.Substring(idx + 1).Trim(); // includes "from ..." (starting at 'from')
            }
        }

        // Extrae el nombre de la tabla principal desde la parte que comienza con "from".
        // Si no puede identificar, devuelve cadena vacía.
        private string ExtractMainTableFromFromPart(string fromPart)
        {
            var lower = (fromPart ?? "").ToLower();
            int idxFrom = lower.IndexOf("from");
            string tail = (idxFrom >= 0) ? fromPart.Substring(idxFrom + 4).Trim() : (fromPart ?? "").Trim();

            // cortar antes de WHERE/ORDER/GROUP/HAVING/LIMIT
            string[] terminators = new[] { " where ", " order ", " group ", " having ", " limit " };
            int endIdx = -1;
            foreach (var t in terminators)
            {
                int p = tail.ToLower().IndexOf(t);
                if (p >= 0)
                {
                    if (endIdx < 0 || p < endIdx)
                    {
                        endIdx = p;
                    }
                }
            }

            if (endIdx >= 0)
                tail = tail.Substring(0, endIdx).Trim();

            // Si hay joins, la primera palabra suele ser la tabla principal
            // Tomar la primera token (antes de espacios, comas o alias)
            var tokens = tail.Split(new[] { ' ', '\t', '\r', '\n', ',' }, StringSplitOptions.RemoveEmptyEntries);
            if (tokens.Length == 0)
                return "";

            // tokens[0] es la tabla principal (puede ser schema.table)
            return tokens[0];
        }

        // Extrae el alias principal (si existe) junto a la tabla en FROM.
        // Por ejemplo: "from TalRecepcionDet d inner join ..." -> devuelve "d"
        private string ExtractMainTableAliasFromFromPart(string fromPart)
        {
            if (string.IsNullOrWhiteSpace(fromPart))
                return "";

            var lower = fromPart.ToLower();
            int idxFrom = lower.IndexOf("from");
            string tail = (idxFrom >= 0) ? fromPart.Substring(idxFrom + 4).Trim() : fromPart.Trim();

            // cortar antes de WHERE/ORDER/GROUP/HAVING/LIMIT
            string[] terminators = new[] { " where ", " order ", " group ", " having ", " limit " };
            int endIdx = -1;
            foreach (var t in terminators)
            {
                int p = tail.ToLower().IndexOf(t);
                if (p >= 0)
                {
                    if (endIdx < 0 || p < endIdx) endIdx = p;
                }
            }
            if (endIdx >= 0) tail = tail.Substring(0, endIdx).Trim();

            var tokens = tail.Split(new[] { ' ', '\t', '\r', '\n', ',' }, StringSplitOptions.RemoveEmptyEntries);
            if (tokens.Length < 2)
                return ""; // no alias token present

            // tokens[0] = table, tokens[1] might be alias unless it's a join keyword
            var candidate = tokens[1];
            string[] reserved = new[] { "inner", "left", "right", "full", "outer", "join", "on", "as" };
            if (reserved.Contains(candidate.ToLower()))
                return "";

            return candidate;
        }

        // New: handle changes in richSelect (uppercase reserved words and color them when a word completes)
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

            ApplySqlKeywordHighlight(richSelect, uppercaseKeywords: true);
        }

        // Apply uppercase (optional) and color highlighting for SQL keywords inside a RichTextBox.
        // Preserves caret/selection.
        private void ApplySqlKeywordHighlight(RichTextBox rtb, bool uppercaseKeywords)
        {
            if (rtb == null) return;

            int origSelStart = rtb.SelectionStart;
            int origSelLen = rtb.SelectionLength;

            string originalText = rtb.Text ?? "";
            string processedText = originalText;

            if (uppercaseKeywords)
            {
                // Replace occurrences of keywords with their uppercase form (length unchanged)
                foreach (var kw in _sqlKeywords)
                {
                    processedText = Regex.Replace(processedText, $@"\b{Regex.Escape(kw)}\b", kw, RegexOptions.IgnoreCase);
                }
            }

            // Only assign Text if changed to avoid recursive TextChanged churn
            bool textChanged = !string.Equals(originalText, processedText, System.StringComparison.Ordinal);
            if (textChanged)
            {
                _suppressRichSelectTextChanged = true;
                try
                {
                    rtb.Text = processedText;
                }
                finally
                {
                    _suppressRichSelectTextChanged = false;
                }
            }

            // Do not force a specific global text color; use control ForeColor for non-keywords
            Color normalColor = rtb.ForeColor;

            string textToScan = rtb.Text ?? "";

            // Color each keyword occurrence; leave other text colors as-is (we set keyword color directly)
            foreach (var kw in _sqlKeywords)
            {
                string pattern = $@"\b{Regex.Escape(kw)}\b";
                foreach (Match m in Regex.Matches(textToScan, pattern, RegexOptions.IgnoreCase))
                {
                    rtb.Select(m.Index, m.Length);
                    if (string.Equals(kw, "ON", System.StringComparison.OrdinalIgnoreCase) || string.Equals(kw, "AS", System.StringComparison.OrdinalIgnoreCase))
                        rtb.SelectionColor = _keywordColor;
                    else
                        rtb.SelectionColor = _joinColor;
                }
            }

            // Restore original selection and selection color
            if (origSelStart >= 0 && origSelStart <= rtb.Text.Length)
            {
                rtb.SelectionStart = origSelStart;
                rtb.SelectionLength = origSelLen;
            }
            else
            {
                rtb.SelectionStart = rtb.Text.Length;
                rtb.SelectionLength = 0;
            }
            rtb.SelectionColor = normalColor;
            rtb.DeselectAll();
        }

        // Highlight separators (commas and tildes) inside generated .dat shown in richSalida.
        // IMPORTANT: do not change background/overall forecolor of the RichTextBox here.
        // Only color separator characters so the user can choose overall colors in designer/properties.
        private void HighlightSeparators(RichTextBox rtb, char[] separators, Color sepColor)
        {
            if (rtb == null) return;

            int origSelStart = rtb.SelectionStart;
            int origSelLen = rtb.SelectionLength;

            string text = rtb.Text ?? "";
            for (int i = 0; i < text.Length; i++)
            {
                if (separators.Contains(text[i]))
                {
                    rtb.Select(i, 1);
                    rtb.SelectionColor = sepColor;
                }
            }

            // Restore selection and selection color to match current ForeColor
            if (origSelStart >= 0 && origSelStart <= rtb.Text.Length)
            {
                rtb.SelectionStart = origSelStart;
                rtb.SelectionLength = origSelLen;
            }
            else
            {
                rtb.SelectionStart = rtb.Text.Length;
                rtb.SelectionLength = 0;
            }
            rtb.SelectionColor = rtb.ForeColor;
            rtb.DeselectAll();
        }

        // Commit checkbox edits immediately so CellValueChanged fires.
        private void dgvColumns_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            if (dgvColumns.IsCurrentCellDirty)
            {
                dgvColumns.CommitEdit(DataGridViewDataErrorContexts.Commit);
            }
        }

        // When the Ellipsis checkbox changes, set/clear the EllipsisSqlSources cell with the uppercase CampoSQL.
        // DO NOT clear per-row EllipsisColsToShowfrmSearch/ToAsign/ToAlias values.
        private void dgvColumns_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0)
                return;

            var colName = dgvColumns.Columns[e.ColumnIndex].Name;
            if (!string.Equals(colName, "Ellipsis", System.StringComparison.OrdinalIgnoreCase))
                return;

            var row = dgvColumns.Rows[e.RowIndex];
            bool isEllipsis = Convert.ToBoolean(row.Cells["Ellipsis"].Value ?? false);

            var campoObj = row.Cells["CampoSQL"].Value;
            string campo = campoObj?.ToString() ?? "";

            if (isEllipsis)
            {
                // put uppercase CampoSQL into EllipsisSqlSources so generated .dat has a hint
                // do not touch EllipsisColsToShowfrmSearch, EllipsisColsToAsign or EllipsisColsToAlias
                row.Cells["EllipsisSqlSources"].Value = campo.ToUpperInvariant();
            }
            else
            {
                // if unchecked, clear ellipsis sql source but keep other per-row tokens intact
                row.Cells["EllipsisSqlSources"].Value = "";
            }
        }
    }//Form
}//namespace