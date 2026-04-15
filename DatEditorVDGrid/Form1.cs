using DatEditorVDGrid.Helpers;
using DatEditorVDGrid.Services;
using DatEditorVDGrid.UIHelpers;
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

        // Preserve behavior, formatting, masked and ellipsis extra properties loaded from the .dat so we can re-emit them unchanged
        private readonly Dictionary<string, string> _behaviorProps = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, string> _formattingProps = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, string> _maskedProps = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, string> _ellipsisExtraProps = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

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
        }


        private void btnAddRow_Click(object sender, EventArgs e)
        {
            int rowIndex = dgvColumns.Rows.Add();
            dgvColumns.Rows[rowIndex].Cells["Pos"].Value = rowIndex + 1;

            // Leave CampoSQL empty so user fills it
            dgvColumns.Rows[rowIndex].Cells["CampoSQL"].Value = "";
        }

        // Helper: strip table alias prefix if present (e.g. "d.Cantidad" => "Cantidad")
        private static string StripTablePrefix(string name)
        {
            if (string.IsNullOrEmpty(name))
                return name;
            var idx = name.LastIndexOf('.');
            if (idx >= 0 && idx < name.Length - 1)
                return name.Substring(idx + 1);
            return name;
        }

        // Helper: return preserved value if present otherwise create a repeated "0" token list of length count using sep
        private static string PreserveOrRepeat(Dictionary<string, string> dict, string key, char sep, int count)
        {
            if (dict != null && dict.TryGetValue(key, out var val) && !string.IsNullOrEmpty(val))
                return val;
            if (count <= 0)
                return "";
            var token = "0";
            return string.Join(sep.ToString(), Enumerable.Repeat(token, count));
        }

        private void btnGenerate_Click(object sender, EventArgs e)
        {
            // Only include rows with a non-empty CampoSQL (avoid generating many empty tokens)
            var filas = dgvColumns.Rows
                .Cast<DataGridViewRow>()
                .Where(r => !string.IsNullOrWhiteSpace(r.Cells["CampoSQL"].Value?.ToString()))
                .ToList();

            int fieldCount = filas.Count;

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

            // Validate per-row tokens
            var validateFlags = new List<string>();
            var colsSqlValidTokens = new List<string>();
            var colsToAsignTokens = new List<string>();

            // Behavior InsNewRowAfter selection
            var insNewRowSelected = new List<string>();

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

                // Validate tokens (new per-row values)
                bool validate = Convert.ToBoolean(row.Cells["Validate"].Value ?? false);
                string colSqlValid = row.Cells["ColsSqlValidToken"].Value?.ToString() ?? "";
                string colAsignVal = row.Cells["ColsToAsignValuesToken"].Value?.ToString() ?? "";

                // InsNewRowAfter checkbox
                bool insNewAfter = Convert.ToBoolean(row.Cells["InsNewRowAfter"].Value ?? false);
                if (insNewAfter)
                {
                    // prefer alias when present otherwise campo name WITHOUT table prefix
                    var fieldName = !string.IsNullOrWhiteSpace(alias) ? alias : StripTablePrefix(campo);
                    if (!string.IsNullOrEmpty(fieldName))
                        insNewRowSelected.Add(fieldName);
                }

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

                // collect validate tokens per row
                validateFlags.Add(validate ? "1" : "0");
                colsSqlValidTokens.Add(string.IsNullOrEmpty(colSqlValid) ? "0" : colSqlValid);
                colsToAsignTokens.Add(string.IsNullOrEmpty(colAsignVal) ? "0" : colAsignVal);
            }

            // Trim trailing empty tokens for per-row lists (only for things using ~ separators)
            DatWriterHelper.TrimTrailingEmpty(ellipsisSqlSources);
            DatWriterHelper.TrimTrailingEmpty(ellipsisColsWidths);
            DatWriterHelper.TrimTrailingEmpty(ellipsisColsHeaders);
            DatWriterHelper.TrimTrailingEmpty(ellipsisColsToShow);
            DatWriterHelper.TrimTrailingEmpty(ellipsisColsToAsign);
            DatWriterHelper.TrimTrailingEmpty(ellipsisColsToAlias);

            // Also trim trailing "0" placeholders from validate token lists so output is compact
            DatWriterHelper.TrimTrailingEmpty(colsSqlValidTokens);
            DatWriterHelper.TrimTrailingEmpty(colsToAsignTokens);

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
            DatWriterHelper.WriteNumberedKeys(sb, "SourceSql", fullSql, 2);

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
                string inferredAlias = SqlParserService.ExtractMainTableAliasFromFromPart(fromJoins);
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

            // ColComboSqlSources: use preserved value or generate "~" separated zeros count = fieldCount
            var colComboSqlSourcesVal = PreserveOrRepeat(_maskedProps, "ColComboSqlSources", '~', fieldCount);
            sb.AppendLine($"ColComboSqlSources={colComboSqlSourcesVal}");

            // ColComboEditables: preserved or comma-separated zeros
            var colComboEditablesVal = PreserveOrRepeat(_maskedProps, "ColComboEditables", ',', fieldCount);
            sb.AppendLine($"ColComboEditables={colComboEditablesVal}");

            // [Ellipsiscols]
            sb.AppendLine();
            sb.AppendLine("[Ellipsiscols]");

            // EllipsisWhichOnes (comma list)
            string ellipsisWhichVal = (ellipsisWhichOnes.Count > 0) ? string.Join(",", ellipsisWhichOnes) : "";
            sb.AppendLine($"EllipsisWhichOnes={ellipsisWhichVal}");

            // EllipsisSqlSources..3 (always present up to 6 keys)
            string ellipsisSqlSourcesJoined = (ellipsisSqlSources.Count > 0) ? string.Join("~", ellipsisSqlSources) : "";
            DatWriterHelper.WriteNumberedKeys(sb, "EllipsisSqlSources", ellipsisSqlSourcesJoined, 2);

            // EllipsisColsWidths (single key with possible ~-joined tokens)
            string ellipsisColsWidthsJoined = (ellipsisColsWidths.Count > 0) ? string.Join("~", ellipsisColsWidths) : "";
            sb.AppendLine($"EllipsisColsWidths={ellipsisColsWidthsJoined}");

            // EllipsisColsHeaders and numbered header keys
            string ellipsisColsHeadersJoined = (ellipsisColsHeaders.Count > 0) ? string.Join("~", ellipsisColsHeaders) : "";
            DatWriterHelper.WriteNumberedKeys(sb, "EllipsisColsHeaders", ellipsisColsHeadersJoined, 2);

            // EllipsisColsToShowfrmSearch (single key)
            string ellipsisColsToShowJoined = "";
            if (ellipsisColsToShow.Any(s => !string.IsNullOrWhiteSpace(s)))
                ellipsisColsToShowJoined = string.Join("~", ellipsisColsToShow);
            else if (!string.IsNullOrWhiteSpace(_globalEllipsisColsToShow))
                ellipsisColsToShowJoined = _globalEllipsisColsToShow;
            sb.AppendLine($"EllipsisColsToShowfrmSearch={ellipsisColsToShowJoined}");

            // EllipsisColsToAsign (three keys)
            string ellipsisColsToAsignJoined = (ellipsisColsToAsign.Count > 0) ? string.Join("~", ellipsisColsToAsign) : _fullEllipsisColsToAsign ?? "";
            DatWriterHelper.WriteNumberedKeys(sb, "EllipsisColsToAsign", ellipsisColsToAsignJoined, 2);

            // EllipsisColsToAlias (single key)
            string ellipsisColsToAliasJoined = (ellipsisColsToAlias.Count > 0) ? string.Join("~", ellipsisColsToAlias) : _fullEllipsisColsToAlias ?? "";
            sb.AppendLine($"EllipsisColsToAlias={ellipsisColsToAliasJoined}");

            // Now emit the remaining ellipsis extra properties (preserve or generate appropriate zeros)
            // Keys and their separators based on production examples:
            // - EllipsisColInvokeColor -> comma
            // - EllipsisColInvokeFont -> comma
            // - EllipsisColsNewFields -> ~
            // - EllipsisColsNewCaption -> ~
            // - EllipsisColsNewDataTypes -> ~
            // - EllipsisColsNewTableNames -> comma
            // - EllipsisColInvokeOpen -> comma
            // - EllipsisColOpenFilters -> ~
            // - EllipsisColInvokeBrowse -> comma
            // - EllipsisColShellExec -> comma

            sb.AppendLine($"EllipsisColInvokeColor={PreserveOrRepeat(_ellipsisExtraProps, "EllipsisColInvokeColor", ',', fieldCount)}");
            sb.AppendLine($"EllipsisColInvokeFont={PreserveOrRepeat(_ellipsisExtraProps, "EllipsisColInvokeFont", ',', fieldCount)}");
            sb.AppendLine($"EllipsisColsNewFields={PreserveOrRepeat(_ellipsisExtraProps, "EllipsisColsNewFields", '~', fieldCount)}");
            sb.AppendLine($"EllipsisColsNewCaption={PreserveOrRepeat(_ellipsisExtraProps, "EllipsisColsNewCaption", '~', fieldCount)}");
            sb.AppendLine($"EllipsisColsNewDataTypes={PreserveOrRepeat(_ellipsisExtraProps, "EllipsisColsNewDataTypes", '~', fieldCount)}");
            sb.AppendLine($"EllipsisColsNewTableNames={PreserveOrRepeat(_ellipsisExtraProps, "EllipsisColsNewTableNames", ',', fieldCount)}");
            sb.AppendLine($"EllipsisColInvokeOpen={PreserveOrRepeat(_ellipsisExtraProps, "EllipsisColInvokeOpen", ',', fieldCount)}");
            sb.AppendLine($"EllipsisColOpenFilters={PreserveOrRepeat(_ellipsisExtraProps, "EllipsisColOpenFilters", '~', fieldCount)}");
            sb.AppendLine($"EllipsisColInvokeBrowse={PreserveOrRepeat(_ellipsisExtraProps, "EllipsisColInvokeBrowse", ',', fieldCount)}");
            sb.AppendLine($"EllipsisColShellExec={PreserveOrRepeat(_ellipsisExtraProps, "EllipsisColShellExec", ',', fieldCount)}");

            // Additional Ellipsis display/invoke keys (other keys kept empty for now if not preserved)
            //sb.AppendLine($"EllipsisColInvokeColor2=");
            //sb.AppendLine($"EllipsisColInvokeFont2=");
            //sb.AppendLine($"EllipsisColsNewFields2=");
            //sb.AppendLine($"EllipsisColsNewCaption2=");
            //sb.AppendLine($"EllipsisColsNewDataTypes2=");
            //sb.AppendLine($"EllipsisColsNewTableNames2=");
            //sb.AppendLine($"EllipsisColInvokeOpen2=");
            //sb.AppendLine($"EllipsisColOpenFilters2=");
            //sb.AppendLine($"EllipsisColInvokeBrowse2=");
            //sb.AppendLine($"EllipsisColShellExec2=");

            // [Formatting]
            sb.AppendLine();
            sb.AppendLine("[Formatting]");

            // Headers (up to 4 keys)
            string headersJoined = (headers.Count > 0) ? string.Join(",", headers.Where(h => !string.IsNullOrWhiteSpace(h))) : "";
            DatWriterHelper.WriteNumberedKeys(sb, "Headers", headersJoined, 2);

            // Widths, Height, ResizeCols, ResizeRows, MoveCols, Editable, ColsToEdit, FormatStrings, etc.
            sb.AppendLine($"Widths={string.Join(",", widths)}");

            // Emit preserved formatting keys when available, otherwise blank (these are single-line keys not per-column)
            sb.AppendLine($"Height={(_formattingProps.ContainsKey("Height") ? _formattingProps["Height"] : "")}");
            sb.AppendLine($"ResizeCols={(_formattingProps.ContainsKey("ResizeCols") ? _formattingProps["ResizeCols"] : "")}");
            sb.AppendLine($"ResizeRows={(_formattingProps.ContainsKey("ResizeRows") ? _formattingProps["ResizeRows"] : "")}");
            sb.AppendLine($"MoveCols={(_formattingProps.ContainsKey("MoveCols") ? _formattingProps["MoveCols"] : "")}");

            // Keep UI-driven Editable (chkEditable) — this is already set on load from the .dat
            sb.AppendLine($"Editable={(chkEditable.Checked ? "1" : "0")}");

            // Build ColsToEdit: use Alias when present, otherwise CampoSQL stripped of table prefix; ignore empty values
            var colsToEditList = filas
                .Where(r => Convert.ToBoolean(r.Cells["Editable"].Value ?? false))
                .Select(r =>
                {
                    var alias = (r.Cells["Alias"].Value ?? "").ToString().Trim();
                    if (!string.IsNullOrEmpty(alias))
                        return alias;
                    return StripTablePrefix((r.Cells["CampoSQL"].Value ?? "").ToString().Trim());
                })
                .Where(s => !string.IsNullOrEmpty(s))
                .ToList();

            string colsToEdit = colsToEditList.Count > 0 ? string.Join(",", colsToEditList) : "";
            sb.AppendLine($"ColsToEdit={colsToEdit}");

            // FormatStrings (use ~ for per-row format strings)
            sb.AppendLine($"FormatStrings={string.Join("~", formats)}");

            // Emit rest of formatting flags from preserved values
            sb.AppendLine($"ExplorerBar={(_formattingProps.ContainsKey("ExplorerBar") ? _formattingProps["ExplorerBar"] : "")}");
            sb.AppendLine($"AutoSearch={(_formattingProps.ContainsKey("AutoSearch") ? _formattingProps["AutoSearch"] : "")}");
            sb.AppendLine($"SelectFullRow={(_formattingProps.ContainsKey("SelectFullRow") ? _formattingProps["SelectFullRow"] : "")}");
            sb.AppendLine($"MergeCells={(_formattingProps.ContainsKey("MergeCells") ? _formattingProps["MergeCells"] : "")}");
            sb.AppendLine($"OutlineBar={(_formattingProps.ContainsKey("OutlineBar") ? _formattingProps["OutlineBar"] : "")}");
            sb.AppendLine($"BlankLinesOnEdit={(_formattingProps.ContainsKey("BlankLinesOnEdit") ? _formattingProps["BlankLinesOnEdit"] : "")}");
            sb.AppendLine($"FrozenRows={(_formattingProps.ContainsKey("FrozenRows") ? _formattingProps["FrozenRows"] : "")}");
            sb.AppendLine($"FrozenCols={(_formattingProps.ContainsKey("FrozenCols") ? _formattingProps["FrozenCols"] : "")}");
            sb.AppendLine($"WordWrap={(_formattingProps.ContainsKey("WordWrap") ? _formattingProps["WordWrap"] : "")}");

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

            // Known behavior keys we preserve when present in the imported .dat.
            string[] behaviorKeys = new[]
            {
                "UpperCaseConvert",
                "AllowEnterToTab",
                "EnterActionNextCol",
                "EnterctionNextRow",
                "GoLastRowOnLoad",
                "ShowColToolTips",
                "DoubleClickAction",
                "ReturnToCallerForm",
                "CallOtherForm",
                "FormToCall",
                "FieldToSend",
                "NewRowColor",
                "ModRowColor",
                "DelRowColor",
                "AddAutomatically",
                "BKColorByRow",
                "FKColorNegativeValues"
                // InsNewRowAfterCols handled below (we want to override it with selections when present)
            };

            // Emit preserved behavior keys in the same order; if not present keep empty
            foreach (var key in behaviorKeys)
            {
                var val = _behaviorProps.ContainsKey(key) ? _behaviorProps[key] : "";
                sb.AppendLine($"{key}={val}");
            }

            // InsNewRowAfterCols: prefer generated CSV from per-row checkboxes; if none selected, emit original value preserved
            string insJoined = insNewRowSelected.Count > 0 ? string.Join(",", insNewRowSelected) : (_behaviorProps.ContainsKey("InsNewRowAfterCols") ? _behaviorProps["InsNewRowAfterCols"] : "");
            sb.AppendLine($"InsNewRowAfterCols={insJoined}");

            // [Validate]
            sb.AppendLine();
            sb.AppendLine("[Validate]");

            // Write ColsSqlValid across numbered keys if needed (use 3 slots to match production examples)
            string colsSqlValidJoined = (colsSqlValidTokens.Count > 0) ? string.Join("~", colsSqlValidTokens) : "";
            DatWriterHelper.WriteNumberedKeys(sb, "ColsSqlValid", colsSqlValidJoined, 3);

            // Write ColsToAsignValues across numbered keys (use 2 slots)
            string colsToAsignJoined = (colsToAsignTokens.Count > 0) ? string.Join("~", colsToAsignTokens) : "";
            DatWriterHelper.WriteNumberedKeys(sb, "ColsToAsignValues", colsToAsignJoined, 2);

            // show generated .dat inside richSalida only
            richSalida.Text = sb.ToString();

            // Only color separator characters (commas and tildes). Do not change background/overall forecolor here.
            RichTextHelper.HighlightSeparators(
                richSalida,
                new[] { ',', '~' },
                _separatorColor
            );
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

        private void btnLoadDat_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "DAT files (*.dat)|*.dat";

            if (ofd.ShowDialog() != DialogResult.OK)
                return;

            var data = DatService.ParseDatFile(ofd.FileName);

            dgvColumns.Rows.Clear();

            if (!data.ContainsKey("SourceSql"))
            {
                MessageBox.Show("No se encontró SourceSql");
                return;
            }

            // reset holders
            _globalEllipsisColsToShow = "";
            _fullEllipsisColsToAsign = "";
            _fullEllipsisColsToAlias = "";
            _behaviorProps.Clear();
            _formattingProps.Clear();
            _maskedProps.Clear();
            _ellipsisExtraProps.Clear();

            var fullSource = DatService.GetFullProperty(data, "SourceSql");

            // separar SELECT vs FROM+JOINS
            SqlParserService.SplitSelectFrom(fullSource, out string selectOnly, out string fromPart);

            // si el .dat trae TabletoSave o TableToSave, preferirla; si no, extraer la tabla principal desde FROM
            string tableVal = "";
            if (data.ContainsKey("TabletoSave"))
                tableVal = DatService.GetFullProperty(data, "TabletoSave");
            else if (data.ContainsKey("TableToSave"))
                tableVal = DatService.GetFullProperty(data, "TableToSave");

            if (!string.IsNullOrWhiteSpace(tableVal))
                txtTableToSave.Text = tableVal;
            else
                txtTableToSave.Text = SqlParserService.ExtractMainTableFromFromPart(fromPart);

            // completar controles de configuración desde .dat
            txtWhereSql.Text = data.ContainsKey("WhereSql") ? DatService.GetFullProperty(data, "WhereSql") : "";
            txtOrderSql.Text = data.ContainsKey("OrderSql") ? DatService.GetFullProperty(data, "OrderSql") : "";
            // KeyField
            txtKeyField.Text = data.ContainsKey("KeyField") ? DatService.GetFullProperty(data, "KeyField") : "";
            // ForeignKey / ForeignToSave
            txtForeignKey.Text = data.ContainsKey("ForeignKey") ? DatService.GetFullProperty(data, "ForeignKey") : "";
            txtForeignSave.Text = data.ContainsKey("ForeignToSave") ? DatService.GetFullProperty(data, "ForeignToSave") : "";
            // Procedure flag (if provided)
            if (data.ContainsKey("Procedure"))
                chkProcedure.Checked = DatService.GetFullProperty(data, "Procedure").Trim() != "0";
            // SqlIdentityKey, CountableCol, ForeignAlias
            txtSqlIdentityKey.Text = data.ContainsKey("SqlIdentityKey") ? DatService.GetFullProperty(data, "SqlIdentityKey") : "";
            txtCountableCol.Text = data.ContainsKey("CountableCol") ? DatService.GetFullProperty(data, "CountableCol") : "";
            txtForeignAlias.Text = data.ContainsKey("ForeignAlias") ? DatService.GetFullProperty(data, "ForeignAlias") : "";
            chkEditable.Checked = data.ContainsKey("Editable") && DatService.GetFullProperty(data, "Editable").Trim() == "1";

            // NEW: load EllipsisColsToAsign / EllipsisColsToAlias full values for later re-output
            if (data.ContainsKey("EllipsisColsToAsign"))
                _fullEllipsisColsToAsign = DatService.GetFullProperty(data, "EllipsisColsToAsign");
            if (data.ContainsKey("EllipsisColsToAlias"))
                _fullEllipsisColsToAlias = DatService.GetFullProperty(data, "EllipsisColsToAlias");

            // Preserve Behavior keys found in the imported .dat so we can write them back unchanged later
            string[] behaviorKeysToPreserve = new[]
            {
                "UpperCaseConvert",
                "AllowEnterToTab",
                "EnterActionNextCol",
                "EnterctionNextRow",
                "GoLastRowOnLoad",
                "ShowColToolTips",
                "DoubleClickAction",
                "ReturnToCallerForm",
                "CallOtherForm",
                "FormToCall",
                "FieldToSend",
                "NewRowColor",
                "ModRowColor",
                "DelRowColor",
                "AddAutomatically",
                "BKColorByRow",
                "FKColorNegativeValues",
                "InsNewRowAfterCols"
            };
            foreach (var k in behaviorKeysToPreserve)
            {
                if (data.ContainsKey(k))
                    _behaviorProps[k] = DatService.GetFullProperty(data, k) ?? "";
            }

            // Preserve Formatting keys that are NOT per-column (we already handle Headers/Widths/ColsToEdit/FormatStrings from grid)
            string[] formattingKeysToPreserve = new[]
            {
                "Height",
                "ResizeCols",
                "ResizeRows",
                "MoveCols",
                "ExplorerBar",
                "AutoSearch",
                "SelectFullRow",
                "MergeCells",
                "OutlineBar",
                "BlankLinesOnEdit",
                "FrozenRows",
                "FrozenCols",
                "WordWrap"
            };
            foreach (var k in formattingKeysToPreserve)
            {
                if (data.ContainsKey(k))
                    _formattingProps[k] = DatService.GetFullProperty(data, k) ?? "";
            }

            // Preserve MaskedCols keys
            string[] maskedKeys = new[]
            {
                "ColComboSqlSources",
                "ColComboEditables"
            };
            foreach (var k in maskedKeys)
            {
                if (data.ContainsKey(k))
                    _maskedProps[k] = DatService.GetFullProperty(data, k) ?? "";
            }

            // Preserve ellipsis extra properties
            string[] ellipsisExtraKeys = new[]
            {
                "EllipsisColInvokeColor",
                "EllipsisColInvokeFont",
                "EllipsisColsNewFields",
                "EllipsisColsNewCaption",
                "EllipsisColsNewDataTypes",
                "EllipsisColsNewTableNames",
                "EllipsisColInvokeOpen",
                "EllipsisColOpenFilters",
                "EllipsisColInvokeBrowse",
                "EllipsisColShellExec"
            };
            foreach (var k in ellipsisExtraKeys)
            {
                if (data.ContainsKey(k))
                    _ellipsisExtraProps[k] = DatService.GetFullProperty(data, k) ?? "";
            }

            // From+Joins se muestra en richSelect (control independiente)
            try
            {
                _suppressRichSelectTextChanged = true;
                richSelect.Text = fromPart?.Trim() ?? "";
                // ensure reserved words appear uppercased + colored from the start
                RichTextHelper.ApplySqlKeywordHighlight(
                    richSelect,
                    _sqlKeywords,
                    _joinColor,
                    _keywordColor,
                    ref _suppressRichSelectTextChanged,
                    true
                );
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

            var campos = SqlParserService.SplitSqlColumns(sourceBody);
            var dataTypes = data.ContainsKey("DataTypes") ? DatService.GetFullProperty(data, "DataTypes").Split(',') : new string[0];
            var save = data.ContainsKey("FieldsToSave") ? DatService.GetFullProperty(data, "FieldsToSave").Split(',') : new string[0];
            var req = data.ContainsKey("RequiredFields") ? DatService.GetFullProperty(data, "RequiredFields").Split(',') : new string[0];
            var headers = data.ContainsKey("Headers") ? DatService.GetFullProperty(data, "Headers").Split(',') : new string[0];
            var widths = data.ContainsKey("Widths") ? DatService.GetFullProperty(data, "Widths").Split(',') : new string[0];
            var formats = data.ContainsKey("FormatStrings") ? DatService.GetFullProperty(data, "FormatStrings").Split('~') : new string[0];
            var ellipsis = data.ContainsKey("EllipsisWhichOnes") ? DatService.GetFullProperty(data, "EllipsisWhichOnes").Split(',') : new string[0];

            // NEW: load EllipsisSqlSources and ColsToEdit combined properties and new ellipsis columns
            var fullEllipsisSources = data.ContainsKey("EllipsisSqlSources") ? DatService.GetFullProperty(data, "EllipsisSqlSources") : "";
            var ellipsisTokens = string.IsNullOrEmpty(fullEllipsisSources)
                ? new string[0]
                : fullEllipsisSources.Split(new[] { '~' }, StringSplitOptions.None);

            var fullColsToEdit = data.ContainsKey("ColsToEdit") ? DatService.GetFullProperty(data, "ColsToEdit") : "";
            var colsToEditTokens = string.IsNullOrWhiteSpace(fullColsToEdit)
                ? new string[0]
                : fullColsToEdit.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => s.Trim())
                    .ToArray();

            var fullEllipsisColsWidths = data.ContainsKey("EllipsisColsWidths") ? DatService.GetFullProperty(data, "EllipsisColsWidths") : "";
            var ellipsisColsWidthsTokens = string.IsNullOrEmpty(fullEllipsisColsWidths)
                ? new string[0]
                : fullEllipsisColsWidths.Split(new[] { '~' }, StringSplitOptions.None);

            var fullEllipsisColsHeaders = data.ContainsKey("EllipsisColsHeaders") ? DatService.GetFullProperty(data, "EllipsisColsHeaders") : "";
            var ellipsisColsHeadersTokens = string.IsNullOrEmpty(fullEllipsisColsHeaders)
                ? new string[0]
                : fullEllipsisColsHeaders.Split(new[] { '~' }, StringSplitOptions.None);

            var fullEllipsisColsToShow = data.ContainsKey("EllipsisColsToShowfrmSearch") ? DatService.GetFullProperty(data, "EllipsisColsToShowfrmSearch") : "";
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
            var fullEllipsisColsToAsign = data.ContainsKey("EllipsisColsToAsign") ? DatService.GetFullProperty(data, "EllipsisColsToAsign") : "";
            var ellipsisColsToAsignTokens = string.IsNullOrEmpty(fullEllipsisColsToAsign)
                ? new string[0]
                : fullEllipsisColsToAsign.Split(new[] { '~' }, StringSplitOptions.None);

            var fullEllipsisColsToAlias = data.ContainsKey("EllipsisColsToAlias") ? DatService.GetFullProperty(data, "EllipsisColsToAlias") : "";
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

            // NEW: load Validate properties
            var fullColsSqlValid = data.ContainsKey("ColsSqlValid") ? DatService.GetFullProperty(data, "ColsSqlValid") : "";
            var colsSqlValidTokens = string.IsNullOrEmpty(fullColsSqlValid)
                ? new string[0]
                : fullColsSqlValid.Split(new[] { '~' }, StringSplitOptions.None);

            var fullColsToAsignValues = data.ContainsKey("ColsToAsignValues") ? DatService.GetFullProperty(data, "ColsToAsignValues") : "";
            var colsToAsignValuesTokens = string.IsNullOrEmpty(fullColsToAsignValues)
                ? new string[0]
                : fullColsToAsignValues.Split(new[] { '~' }, StringSplitOptions.None);

            // NEW: load InsNewRowAfterCols tokens (behavior) to mark per-row checkbox
            var fullInsNewRowAfter = _behaviorProps.ContainsKey("InsNewRowAfterCols") ? _behaviorProps["InsNewRowAfterCols"] : "";
            var insNewRowTokens = string.IsNullOrWhiteSpace(fullInsNewRowAfter)
                ? new string[0]
                : fullInsNewRowAfter.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(t => t.Trim()).ToArray();

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

                // Assign Validate tokens if present
                var tokSql = i < colsSqlValidTokens.Length ? colsSqlValidTokens[i] : "";
                var tokAsign = i < colsToAsignValuesTokens.Length ? colsToAsignValuesTokens[i] : "";

                dgvColumns.Rows[rowIndex].Cells["ColsSqlValidToken"].Value = tokSql;
                dgvColumns.Rows[rowIndex].Cells["ColsToAsignValuesToken"].Value = tokAsign;

                // Mark Validate checkbox if either token is non-empty and not "0"
                bool shouldValidate = (!(string.IsNullOrEmpty(tokSql) || tokSql == "0") || !(string.IsNullOrEmpty(tokAsign) || tokAsign == "0"));
                dgvColumns.Rows[rowIndex].Cells["Validate"].Value = shouldValidate;

                // Mark InsNewRowAfter checkbox if this field is present in imported InsNewRowAfterCols
                bool insMark = insNewRowTokens.Any(t =>
                    string.Equals(t, alias, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(t, StripTablePrefix(campo), StringComparison.OrdinalIgnoreCase));
                dgvColumns.Rows[rowIndex].Cells["InsNewRowAfter"].Value = insMark;

                // If this column name (alias or campo without table prefix) is listed in ColsToEdit => mark Editable
                bool shouldEdit = colsToEditTokens.Any(token =>
                    string.Equals(token, alias, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(token, StripTablePrefix(campo), StringComparison.OrdinalIgnoreCase));

                dgvColumns.Rows[rowIndex].Cells["Editable"].Value = shouldEdit;
            }

            MessageBox.Show("DAT cargado correctamente");
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
    }//Form
}//namespace