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

            // SubTotals: track marked names for summary generation
            var markedSubTotalNames = new List<string>();
            int markedSubTotalCount = 0;

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
                    var fieldName = !string.IsNullOrWhiteSpace(alias) ? alias : DatOptions.StripTablePrefix(campo);
                    if (!string.IsNullOrEmpty(fieldName))
                        insNewRowSelected.Add(fieldName);
                }

                // SubTotal checkbox
                bool isSubTotal = Convert.ToBoolean(row.Cells["SubTotal"].Value ?? false);
                if (isSubTotal)
                {
                    // prefer alias, otherwise stripped campo
                    var name = !string.IsNullOrWhiteSpace(alias) ? alias : DatOptions.StripTablePrefix(campo);
                    if (!string.IsNullOrEmpty(name))
                    {
                        markedSubTotalNames.Add(name);
                        markedSubTotalCount++;
                    }
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
            var colComboSqlSourcesVal = DatOptions.PreserveOrRepeat(_options.MaskedProps, "ColComboSqlSources", '~', fieldCount);
            sb.AppendLine($"ColComboSqlSources={colComboSqlSourcesVal}");

            // ColComboEditables: preserved or comma-separated zeros
            var colComboEditablesVal = DatOptions.PreserveOrRepeat(_options.MaskedProps, "ColComboEditables", ',', fieldCount);
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
            else if (!string.IsNullOrWhiteSpace(_options.GlobalEllipsisColsToShow))
                ellipsisColsToShowJoined = _options.GlobalEllipsisColsToShow;
            sb.AppendLine($"EllipsisColsToShowfrmSearch={ellipsisColsToShowJoined}");

            // EllipsisColsToAsign (three keys)
            string ellipsisColsToAsignJoined = (ellipsisColsToAsign.Count > 0) ? string.Join("~", ellipsisColsToAsign) : _options.FullEllipsisColsToAsign ?? "";
            DatWriterHelper.WriteNumberedKeys(sb, "EllipsisColsToAsign", ellipsisColsToAsignJoined, 2);

            // EllipsisColsToAlias (single key)
            string ellipsisColsToAliasJoined = (ellipsisColsToAlias.Count > 0) ? string.Join("~", ellipsisColsToAlias) : _options.FullEllipsisColsToAlias ?? "";
            sb.AppendLine($"EllipsisColsToAlias={ellipsisColsToAliasJoined}");

            // Now emit the remaining ellipsis extra properties (preserve or generate appropriate zeros)
            sb.AppendLine($"EllipsisColInvokeColor={DatOptions.PreserveOrRepeat(_options.EllipsisExtraProps, "EllipsisColInvokeColor", ',', fieldCount)}");
            sb.AppendLine($"EllipsisColInvokeFont={DatOptions.PreserveOrRepeat(_options.EllipsisExtraProps, "EllipsisColInvokeFont", ',', fieldCount)}");
            sb.AppendLine($"EllipsisColsNewFields={DatOptions.PreserveOrRepeat(_options.EllipsisExtraProps, "EllipsisColsNewFields", '~', fieldCount)}");
            sb.AppendLine($"EllipsisColsNewCaption={DatOptions.PreserveOrRepeat(_options.EllipsisExtraProps, "EllipsisColsNewCaption", '~', fieldCount)}");
            sb.AppendLine($"EllipsisColsNewDataTypes={DatOptions.PreserveOrRepeat(_options.EllipsisExtraProps, "EllipsisColsNewDataTypes", '~', fieldCount)}");
            sb.AppendLine($"EllipsisColsNewTableNames={DatOptions.PreserveOrRepeat(_options.EllipsisExtraProps, "EllipsisColsNewTableNames", ',', fieldCount)}");
            sb.AppendLine($"EllipsisColInvokeOpen={DatOptions.PreserveOrRepeat(_options.EllipsisExtraProps, "EllipsisColInvokeOpen", ',', fieldCount)}");
            sb.AppendLine($"EllipsisColOpenFilters={DatOptions.PreserveOrRepeat(_options.EllipsisExtraProps, "EllipsisColOpenFilters", '~', fieldCount)}");
            sb.AppendLine($"EllipsisColInvokeBrowse={DatOptions.PreserveOrRepeat(_options.EllipsisExtraProps, "EllipsisColInvokeBrowse", ',', fieldCount)}");
            sb.AppendLine($"EllipsisColShellExec={DatOptions.PreserveOrRepeat(_options.EllipsisExtraProps, "EllipsisColShellExec", ',', fieldCount)}");

            // [Formatting]
            sb.AppendLine();
            sb.AppendLine("[Formatting]");

            // Headers (up to 4 keys)
            string headersJoined = (headers.Count > 0) ? string.Join(",", headers.Where(h => !string.IsNullOrWhiteSpace(h))) : "";
            DatWriterHelper.WriteNumberedKeys(sb, "Headers", headersJoined, 2);

            // Widths, Height, ResizeCols, ResizeRows, MoveCols, Editable, ColsToEdit, FormatStrings, etc.
            sb.AppendLine($"Widths={string.Join(",", widths)}");

            // Emit preserved formatting keys when available, otherwise blank (these are single-line keys not per-column)
            sb.AppendLine($"Height={(_options.FormattingProps.ContainsKey("Height") ? _options.FormattingProps["Height"] : "")}");
            sb.AppendLine($"ResizeCols={(_options.FormattingProps.ContainsKey("ResizeCols") ? _options.FormattingProps["ResizeCols"] : "")}");
            sb.AppendLine($"ResizeRows={(_options.FormattingProps.ContainsKey("ResizeRows") ? _options.FormattingProps["ResizeRows"] : "")}");
            sb.AppendLine($"MoveCols={(_options.FormattingProps.ContainsKey("MoveCols") ? _options.FormattingProps["MoveCols"] : "")}");

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
                    return DatOptions.StripTablePrefix((r.Cells["CampoSQL"].Value ?? "").ToString().Trim());
                })
                .Where(s => !string.IsNullOrEmpty(s))
                .ToList();

            string colsToEdit = colsToEditList.Count > 0 ? string.Join(",", colsToEditList) : "";
            sb.AppendLine($"ColsToEdit={colsToEdit}");

            // FormatStrings (use ~ for per-row format strings)
            sb.AppendLine($"FormatStrings={string.Join("~", formats)}");

            // Emit rest of formatting flags from preserved values
            sb.AppendLine($"ExplorerBar={(_options.FormattingProps.ContainsKey("ExplorerBar") ? _options.FormattingProps["ExplorerBar"] : "")}");
            sb.AppendLine($"AutoSearch={(_options.FormattingProps.ContainsKey("AutoSearch") ? _options.FormattingProps["AutoSearch"] : "")}");
            sb.AppendLine($"SelectFullRow={(_options.FormattingProps.ContainsKey("SelectFullRow") ? _options.FormattingProps["SelectFullRow"] : "")}");
            sb.AppendLine($"MergeCells={(_options.FormattingProps.ContainsKey("MergeCells") ? _options.FormattingProps["MergeCells"] : "")}");
            sb.AppendLine($"OutlineBar={(_options.FormattingProps.ContainsKey("OutlineBar") ? _options.FormattingProps["OutlineBar"] : "")}");
            sb.AppendLine($"BlankLinesOnEdit={(_options.FormattingProps.ContainsKey("BlankLinesOnEdit") ? _options.FormattingProps["BlankLinesOnEdit"] : "")}");
            sb.AppendLine($"FrozenRows={(_options.FormattingProps.ContainsKey("FrozenRows") ? _options.FormattingProps["FrozenRows"] : "")}");
            sb.AppendLine($"FrozenCols={(_options.FormattingProps.ContainsKey("FrozenCols") ? _options.FormattingProps["FrozenCols"] : "")}");
            sb.AppendLine($"WordWrap={(_options.FormattingProps.ContainsKey("WordWrap") ? _options.FormattingProps["WordWrap"] : "")}");

            // [SubTotals]
            sb.AppendLine();
            sb.AppendLine("[SubTotals]");

            // Totales = 1 if any SubTotal checkbox selected, otherwise 0 (but always emit)
            string totalesVal = (markedSubTotalCount > 0) ? "1" : "0";
            sb.AppendLine($"Totales={totalesVal}");

            // Preserve MultipleSubTotals if imported, otherwise emit preserved or blank
            string multipleSubTotalsVal = _options.SubTotalsProps.ContainsKey("MultipleSubTotals") ? _options.SubTotalsProps["MultipleSubTotals"] : "";
            sb.AppendLine($"MultipleSubTotals={multipleSubTotalsVal}");

            // SubTotalsGroups: emit a comma-separated list sized to fieldCount.
            // If one or more SubTotal checkboxes are checked -> "1,0,0,..."
            // Otherwise -> all zeros
            string subTotalsGroupsOut;
            if (markedSubTotalCount > 0)
            {
                // single leading 1, rest zeros
                subTotalsGroupsOut = "1" + (fieldCount > 1 ? "," + string.Join(",", Enumerable.Repeat("0", fieldCount - 1)) : "");
            }
            else
            {
                subTotalsGroupsOut = string.Join(",", Enumerable.Repeat("0", fieldCount));
            }
            sb.AppendLine($"SubTotalsGroups={subTotalsGroupsOut}");

            // SubTotalsSummary: if user selected marked fields, place their names as first chunk then pad with ~0
            string subTotalsSummaryOut;
            if (markedSubTotalCount > 0)
            {
                var first = markedSubTotalNames.Count > 0 ? string.Join(",", markedSubTotalNames) : "0";
                // create fieldCount tokens total: first token is the comma-list, remaining tokens are "0"
                subTotalsSummaryOut = first + string.Concat(Enumerable.Repeat("~0", Math.Max(0, fieldCount - 1)));
            }
            else
            {
                subTotalsSummaryOut = _options.SubTotalsProps.ContainsKey("SubTotalsSummary") ? _options.SubTotalsProps["SubTotalsSummary"] : string.Join("~", Enumerable.Repeat("0", fieldCount));
            }
            sb.AppendLine($"SubTotalsSummary={subTotalsSummaryOut}");

            // SubTotalBkColors and SubTotalCaptions: preserve or emit comma zeros of length fieldCount
            string bkcolorsOut = _options.SubTotalsProps.ContainsKey("SubTotalBkColors") ? _options.SubTotalsProps["SubTotalBkColors"] : string.Join(",", Enumerable.Repeat("0", fieldCount));
            sb.AppendLine($"SubTotalBkColors={bkcolorsOut}");

            string captionsOut = _options.SubTotalsProps.ContainsKey("SubTotalCaptions") ? _options.SubTotalsProps["SubTotalCaptions"] : string.Join(",", Enumerable.Repeat("0", fieldCount));
            sb.AppendLine($"SubTotalCaptions={captionsOut}");

            // SubTotalsEditable, DetailEditable preserve
            sb.AppendLine($"SubTotalsEditable={(_options.SubTotalsProps.ContainsKey("SubTotalsEditable") ? _options.SubTotalsProps["SubTotalsEditable"] : "")}");
            sb.AppendLine($"DetailEditable={(_options.SubTotalsProps.ContainsKey("DetailEditable") ? _options.SubTotalsProps["DetailEditable"] : "")}");

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
                var val = _options.BehaviorProps.ContainsKey(key) ? _options.BehaviorProps[key] : "";
                sb.AppendLine($"{key}={val}");
            }

            // InsNewRowAfterCols: prefer generated CSV from per-row checkboxes; if none selected, emit original value preserved
            string insJoined = insNewRowSelected.Count > 0 ? string.Join(",", insNewRowSelected) : (_options.BehaviorProps.ContainsKey("InsNewRowAfterCols") ? _options.BehaviorProps["InsNewRowAfterCols"] : "");
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

            // Color elements separated by commas and tildes with sequence colors, and bold headers
            RichTextHelper.ApplyDatSyntaxHighlighting(richSalida);
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

        private void btnFillValues_Click(object sender, EventArgs e)
        {
            // Columns to ensure default "0" values for ellipsis-related fields (per-row)
            var ellipsisCols = new[]
            {
                "EllipsisSqlSources",
                "EllipsisColsWidths",
                "EllipsisColsHeaders",
                "EllipsisColsToShowfrmSearch",
                "EllipsisColsToAsign",
                "EllipsisColsToAlias",
                // validate tokens
                "ColsSqlValidToken",
                "ColsToAsignValuesToken"
            };

            int filled = 0;

            foreach (DataGridViewRow row in dgvColumns.Rows)
            {
                if (row.IsNewRow)
                    continue;

                // Only fill rows that actually represent a field (CampoSQL present)
                var campoRaw = row.Cells["CampoSQL"]?.Value?.ToString();
                if (string.IsNullOrWhiteSpace(campoRaw))
                    continue;

                // 1) Fill Header with field name without table prefix when empty
                if (dgvColumns.Columns.Contains("Header"))
                {
                    var headerCell = row.Cells["Header"];
                    var headerVal = headerCell?.Value?.ToString();
                    if (string.IsNullOrWhiteSpace(headerVal))
                    {
                        headerCell.Value = DatOptions.StripTablePrefix(campoRaw);
                        filled++;
                    }
                }

                // 2) Ensure Width and Format default to "0" when empty
                if (dgvColumns.Columns.Contains("Width"))
                {
                    var widthCell = row.Cells["Width"];
                    if (string.IsNullOrWhiteSpace(widthCell?.Value?.ToString()))
                    {
                        widthCell.Value = "0";
                        filled++;
                    }
                }

                if (dgvColumns.Columns.Contains("Format"))
                {
                    var fmtCell = row.Cells["Format"];
                    if (string.IsNullOrWhiteSpace(fmtCell?.Value?.ToString()))
                    {
                        fmtCell.Value = "0";
                        filled++;
                    }
                }

                // 3) Per-row ellipsis/validate tokens -> set "0" when empty
                foreach (var colName in ellipsisCols)
                {
                    if (!dgvColumns.Columns.Contains(colName))
                        continue;

                    var cell = row.Cells[colName];
                    var raw = cell?.Value?.ToString();

                    if (string.IsNullOrWhiteSpace(raw))
                    {
                        cell.Value = "0";
                        filled++;
                    }
                }
            }

            MessageBox.Show($"Valores por defecto agregados: {filled}", "Auto-fill", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }//Form
}//namespace
