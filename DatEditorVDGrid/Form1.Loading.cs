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
            _options.Reset();

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
                _options.FullEllipsisColsToAsign = DatService.GetFullProperty(data, "EllipsisColsToAsign");
            if (data.ContainsKey("EllipsisColsToAlias"))
                _options.FullEllipsisColsToAlias = DatService.GetFullProperty(data, "EllipsisColsToAlias");

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
                    _options.BehaviorProps[k] = DatService.GetFullProperty(data, k) ?? "";
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
                    _options.FormattingProps[k] = DatService.GetFullProperty(data, k) ?? "";
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
                    _options.MaskedProps[k] = DatService.GetFullProperty(data, k) ?? "";
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
                    _options.EllipsisExtraProps[k] = DatService.GetFullProperty(data, k) ?? "";
            }

            // Preserve SubTotals keys
            string[] subTotalsKeys = new[]
            {
        "Totales",
        "MultipleSubTotals",
        "SubTotalsGroups",
        "SubTotalsSummary",
        "SubTotalBkColors",
        "SubTotalCaptions",
        "SubTotalsEditable",
        "DetailEditable"
    };
            foreach (var k in subTotalsKeys)
            {
                if (data.ContainsKey(k))
                    _options.SubTotalsProps[k] = DatService.GetFullProperty(data, k) ?? "";
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
                _options.GlobalEllipsisColsToShow = ellipsisColsToShowTokens[0];
                // try to split by commas and assign each token to the corresponding row's cell
                var csvTokens = _options.GlobalEllipsisColsToShow.Split(new[] { ',' }, StringSplitOptions.None);
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
                _options.FullEllipsisColsToAsign = fullEllipsisColsToAsign;
            if (!string.IsNullOrEmpty(fullEllipsisColsToAlias))
                _options.FullEllipsisColsToAlias = fullEllipsisColsToAlias;

            // If we had a single global CSV for EllipsisColsToShowfrmSearch, create csvTokens now so we can assign per-row
            string[] ellipsisColsToShowGlobalCsvTokens = null;
            if (!string.IsNullOrEmpty(_options.GlobalEllipsisColsToShow))
                ellipsisColsToShowGlobalCsvTokens = _options.GlobalEllipsisColsToShow.Split(new[] { ',' }, StringSplitOptions.None);

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
            var fullInsNewRowAfter = _options.BehaviorProps.ContainsKey("InsNewRowAfterCols") ? _options.BehaviorProps["InsNewRowAfterCols"] : "";
            var insNewRowTokens = string.IsNullOrWhiteSpace(fullInsNewRowAfter)
                ? new string[0]
                : fullInsNewRowAfter.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(t => t.Trim()).ToArray();

            // NEW: parse SubTotalsSummary first token (comma list) and use it to mark checkboxes.
            var fullSubTotalsSummary = _options.SubTotalsProps.ContainsKey("SubTotalsSummary") ? _options.SubTotalsProps["SubTotalsSummary"] : "";
            var subTotalsSummaryTokens = string.IsNullOrEmpty(fullSubTotalsSummary)
                ? new string[0]
                : fullSubTotalsSummary.Split(new[] { '~' }, StringSplitOptions.None);

            var subTotalsNameSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (subTotalsSummaryTokens.Length > 0 && !string.IsNullOrWhiteSpace(subTotalsSummaryTokens[0]) && subTotalsSummaryTokens[0] != "0")
            {
                foreach (var n in subTotalsSummaryTokens[0].Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    var trimmed = n.Trim();
                    if (!string.IsNullOrEmpty(trimmed))
                        subTotalsNameSet.Add(trimmed);
                }
            }

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
                    string.Equals(t, DatOptions.StripTablePrefix(campo), StringComparison.OrdinalIgnoreCase));
                dgvColumns.Rows[rowIndex].Cells["InsNewRowAfter"].Value = insMark;

                // Mark SubTotal checkbox ONLY if this field is listed in SubTotalsSummary first token
                bool subTotalMark = false;
                if (subTotalsNameSet.Count > 0)
                {
                    var stripped = DatOptions.StripTablePrefix(campo);
                    if (!string.IsNullOrEmpty(alias) && subTotalsNameSet.Contains(alias))
                        subTotalMark = true;
                    else if (!string.IsNullOrEmpty(stripped) && subTotalsNameSet.Contains(stripped))
                        subTotalMark = true;
                }
                dgvColumns.Rows[rowIndex].Cells["SubTotal"].Value = subTotalMark;

                // If this column name (alias or campo without table prefix) is listed in ColsToEdit => mark Editable
                bool shouldEdit = colsToEditTokens.Any(token =>
                    string.Equals(token, alias, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(token, DatOptions.StripTablePrefix(campo), StringComparison.OrdinalIgnoreCase));

                dgvColumns.Rows[rowIndex].Cells["Editable"].Value = shouldEdit;
            }

            // Adjust selected columns to fit their content so they become narrower where possible
            var columnsToFit = new[]
            {
        "Pos",
        "DataType", // "Tipo" header -> column name is DataType
        "Guardar",
        "Requerido",
        "Editable",
        "Width",
        "Ellipsis",
        "Validate",
        "InsNewRowAfter",
        "SubTotal"
    };

            foreach (var colName in columnsToFit)
            {
                if (!dgvColumns.Columns.Contains(colName))
                    continue;

                var col = dgvColumns.Columns[colName];

                // Resize to fit all cells (including header)
                dgvColumns.AutoResizeColumn(col.Index, DataGridViewAutoSizeColumnMode.AllCells);

                // Add a small padding so content doesn't touch cell border, then lock autosize so user can still manually resize
                int padding = 6;
                col.Width = Math.Max(col.MinimumWidth, col.Width + padding);
                col.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            }
            // set status text and resize label to fit filename
            lblStatus.Text = "ArchivoImportado: " + ofd.SafeFileName;

            // Preferible: calcular ancho exacto y limitarlo al ancho disponible del formulario
            var measured = TextRenderer.MeasureText(lblStatus.Text, lblStatus.Font);
            int paddinglbl = 2;
            int maxAllowed = Math.Max(100, this.ClientSize.Width - lblStatus.Left - paddinglbl);
            lblStatus.AutoSize = false;
            lblStatus.Width = Math.Min(measured.Width + paddinglbl, maxAllowed);

            // Si no cabe, usar elipsis para indicar recorte
            lblStatus.AutoEllipsis = true;
            lblStatus.Refresh();
            MessageBox.Show("DAT cargado correctamente");
        }

    }//Form
}//namespace
