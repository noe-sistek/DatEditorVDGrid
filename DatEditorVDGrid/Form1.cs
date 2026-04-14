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

        public Form1()
        {
            InitializeComponent();
            this.Load += Form1_Load;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            dgvColumns.Columns.Clear();

            dgvColumns.AllowUserToAddRows = false;

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

            dgvColumns.Columns.Add("Header", "Header");
            dgvColumns.Columns.Add("Width", "Width");

            dgvColumns.Columns.Add(new DataGridViewCheckBoxColumn()
            {
                Name = "Editable",
                HeaderText = "Editable"
            });
            dgvColumns.Columns.Add("Format", "Formato");

            // NEW: EllipsisColsWidths, EllipsisColsHeaders, EllipsisColsToShowfrmSearch (text per row)
            dgvColumns.Columns.Add(new DataGridViewCheckBoxColumn()
            {
                Name = "Ellipsis",
                HeaderText = "Ellipsis"
            });
            dgvColumns.Columns.Add(new DataGridViewTextBoxColumn()
            {
                Name = "EllipsisColsWidths",
                HeaderText = "EllipsisColsWidths"
            });
            dgvColumns.Columns.Add(new DataGridViewTextBoxColumn()
            {
                Name = "EllipsisColsHeaders",
                HeaderText = "EllipsisColsHeaders"
            });
            dgvColumns.Columns.Add(new DataGridViewTextBoxColumn()
            {
                Name = "EllipsisColsToShowfrmSearch",
                HeaderText = "EllipsisColsToShowfrmSearch"
            });

            // EllipsisSqlSources per-row text column
            var ellipsisSourceCol = new DataGridViewTextBoxColumn()
            {
                Name = "EllipsisSqlSources",
                HeaderText = "EllipsisSqlSources"
            };
            dgvColumns.Columns.Add(ellipsisSourceCol);

            // Ensure edits commit immediately for checkbox handling
            dgvColumns.CurrentCellDirtyStateChanged += dgvColumns_CurrentCellDirtyStateChanged;
            dgvColumns.CellValueChanged += dgvColumns_CellValueChanged;
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

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
            var ellipsisList = new List<string>();
            var ellipsisSqlSources = new List<string>();
            var ellipsisColsWidths = new List<string>();
            var ellipsisColsHeaders = new List<string>();
            var ellipsisColsToShow = new List<string>();

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

                // EllipsisSqlSources per-row text cell (could be empty)
                string ellSrc = row.Cells["EllipsisSqlSources"].Value?.ToString() ?? "";

                // Additional per-row ellipsis properties
                string eWidth = row.Cells["EllipsisColsWidths"].Value?.ToString() ?? "";
                string eHeader = row.Cells["EllipsisColsHeaders"].Value?.ToString() ?? "";
                string eShow = row.Cells["EllipsisColsToShowfrmSearch"].Value?.ToString() ?? "";

                // si alias está vacío, no añadir espacio extra
                selectParts.Add(string.IsNullOrWhiteSpace(alias) ? $"{campo}" : $"{campo} {alias}");
                dataTypes.Add(tipo);
                fieldsToSave.Add(guardar ? "1" : "0");
                required.Add(req ? "1" : "0");
                headers.Add(header);
                widths.Add(width);
                formats.Add(format);
                ellipsisList.Add(ellipsis ? "1" : "0");
                ellipsisSqlSources.Add(ellSrc);
                ellipsisColsWidths.Add(eWidth);
                ellipsisColsHeaders.Add(eHeader);
                ellipsisColsToShow.Add(eShow);
            }

            // Trim trailing empty tokens to avoid generating many trailing '~'
            TrimTrailingEmpty(ellipsisSqlSources);
            TrimTrailingEmpty(ellipsisColsWidths);
            TrimTrailingEmpty(ellipsisColsHeaders);
            TrimTrailingEmpty(ellipsisColsToShow);

            // construir SELECT respetando que los campos no contengan coma adicional
            string selectClause = "SELECT " + string.Join(",", selectParts);
            string fromJoins = txtFromJoins.Text?.Trim() ?? "";
            string fullSql = string.IsNullOrWhiteSpace(fromJoins) ? selectClause : (selectClause + " " + fromJoins);

            var sb = new StringBuilder();

            // [Source] header requerido
            sb.AppendLine("[Source]");

            // SourceSql (respetando 255 chars por línea y sin cortar palabras/campos)
            AppendWrappedProperty(sb, "SourceSql", fullSql);

            // Procedimiento fijo
            sb.AppendLine("Procedure=0");

            // --- Now produce the keys in the exact order required by your example ---
            // WhereSql, OrderSql, TabletoSave (note: using the exact key name from your example),
            // DataTypes, FieldsToSave, SqlIdentityKey, CountableCol, KeyField,
            // ForeignKey, ForeignToSave, ForeignAlias, RequiredFields

            // Where & Order
            AppendWrappedProperty(sb, "WhereSql", txtWhereSql.Text);
            AppendWrappedProperty(sb, "OrderSql", txtOrderSql.Text);

            // TabletoSave (use the example's key name). Prefer txtTableToSave if provided.
            AppendWrappedProperty(sb, "TabletoSave", txtTableToSave.Text);

            // DataTypes / FieldsToSave
            AppendWrappedProperty(sb, "DataTypes", string.Join(",", dataTypes));
            AppendWrappedProperty(sb, "FieldsToSave", string.Join(",", fieldsToSave));

            // SqlIdentityKey: take from the dedicated textbox
            string sqlIdentity = txtSqlIdentityKey.Text?.Trim() ?? "";
            AppendWrappedProperty(sb, "SqlIdentityKey", sqlIdentity);

            // CountableCol: use txtCountableCol
            AppendWrappedProperty(sb, "CountableCol", txtCountableCol.Text?.Trim() ?? "");

            // KeyField
            AppendWrappedProperty(sb, "KeyField", txtKeyField.Text);

            // ForeignKey / ForeignToSave
            string foreignKeyStr = txtForeignKey.Text?.Trim() ?? "";
            string foreignToSaveStr = txtForeignSave.Text?.Trim() ?? "";
            AppendWrappedProperty(sb, "ForeignKey", foreignKeyStr);
            AppendWrappedProperty(sb, "ForeignToSave", foreignToSaveStr);

            // ForeignAlias: prefer explicit textbox; otherwise infer main alias or emit placeholders
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
            AppendWrappedProperty(sb, "ForeignAlias", foreignAliasStr);

            // RequiredFields
            AppendWrappedProperty(sb, "RequiredFields", string.Join(",", required));

            // --- End of Source section keys ---

            sb.AppendLine();
            sb.AppendLine("[Formatting]");

            // Headers: unir solo headers no vacíos para evitar comas extra
            var filteredHeaders = headers.Where(h => !string.IsNullOrWhiteSpace(h)).ToList();
            AppendWrappedProperty(sb, "Headers", filteredHeaders.Count > 0 ? string.Join(",", filteredHeaders) : "");

            // Widths: mantener todos los valores
            AppendWrappedProperty(sb, "Widths", string.Join(",", widths));

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

            string colsToEdit = string.Join(",", colsToEditList);
            AppendWrappedProperty(sb, "ColsToEdit", colsToEdit);

            // FormatStrings
            AppendWrappedProperty(sb, "FormatStrings", string.Join("~", formats));

            // Move all Ellipsis-related properties under [Ellipsiscols]
            sb.AppendLine();
            sb.AppendLine("[Ellipsiscols]");

            // EllipsisWhichOnes: keep comma-separated list
            AppendWrappedProperty(sb, "EllipsisWhichOnes", string.Join(",", ellipsisList));

            // EllipsisSqlSources: joined by ~ (one token per row)
            AppendWrappedProperty(sb, "EllipsisSqlSources", string.Join("~", ellipsisSqlSources));

            // Ellipsis cols properties: joined by ~ (top-level separator)
            AppendWrappedProperty(sb, "EllipsisColsWidths", string.Join("~", ellipsisColsWidths));
            AppendWrappedProperty(sb, "EllipsisColsHeaders", string.Join("~", ellipsisColsHeaders));

            // EllipsisColsToShowfrmSearch: prefer per-row tokens if any; otherwise use global value from .dat (if present)
            string ellipsisColsToShowValue = "";
            if (ellipsisColsToShow.Any(s => !string.IsNullOrWhiteSpace(s)))
            {
                ellipsisColsToShowValue = string.Join("~", ellipsisColsToShow);
            }
            else if (!string.IsNullOrWhiteSpace(_globalEllipsisColsToShow))
            {
                // use the original global CSV (no ~)
                ellipsisColsToShowValue = _globalEllipsisColsToShow;
            }
            AppendWrappedProperty(sb, "EllipsisColsToShowfrmSearch", ellipsisColsToShowValue);

            txtOutput.Text = sb.ToString();
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
                // alias validation removed per request
                bool guardar = Convert.ToBoolean(row.Cells["Guardar"].Value ?? false);

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

            // From+Joins se muestra en txtFromJoins (control para editar o mantener)
            txtFromJoins.Text = fromPart?.Trim() ?? "";

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
            if (ellipsisColsToShowTokens.Length == 1 && ellipsisColsToShowTokens[0].Contains(","))
            {
                _globalEllipsisColsToShow = ellipsisColsToShowTokens[0];
                // clear per-row tokens so we don't assign that big CSV into the first row
                ellipsisColsToShowTokens = new string[0];
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

                // Assign new ellipsis columns per-row tokens (top-level separator is ~)
                if (i < ellipsisColsWidthsTokens.Length)
                    dgvColumns.Rows[rowIndex].Cells["EllipsisColsWidths"].Value = ellipsisColsWidthsTokens[i];
                else
                    dgvColumns.Rows[rowIndex].Cells["EllipsisColsWidths"].Value = "";

                if (i < ellipsisColsHeadersTokens.Length)
                    dgvColumns.Rows[rowIndex].Cells["EllipsisColsHeaders"].Value = ellipsisColsHeadersTokens[i];
                else
                    dgvColumns.Rows[rowIndex].Cells["EllipsisColsHeaders"].Value = "";

                if (i < ellipsisColsToShowTokens.Length)
                    dgvColumns.Rows[rowIndex].Cells["EllipsisColsToShowfrmSearch"].Value = ellipsisColsToShowTokens[i];
                else
                    dgvColumns.Rows[rowIndex].Cells["EllipsisColsToShowfrmSearch"].Value = "";

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
            int chunkMax = Math.Max(1, maxLine - prefixLength);

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

        private void txtFromJoins_TextChanged(object sender, EventArgs e)
        {

            // Solo procesar si el último carácter es un espacio o si se borró texto
            if (txtFromJoins.Text.Length > 0 &&
                txtFromJoins.Text[txtFromJoins.Text.Length - 1] != ' ' && !txtFromJoins.Text.EndsWith("\n"))
            {
                return;
            }

            // Keywords SQL para FROM/JOINs
            string[] keywords = { "FROM", "JOIN", "LEFT", "RIGHT", "INNER", "OUTER", "ON", "AS" };
            Color joinColor = Color.Blue;
            Color keywordColor = Color.Green;

            // Guardar posición/selección actual
            int selStart = txtFromJoins.SelectionStart;
            int selLength = txtFromJoins.SelectionLength;

            // Resetear colores a negro
            txtFromJoins.SelectAll();
            txtFromJoins.SelectionColor = Color.Black;

            string text = txtFromJoins.Text ?? "";

            foreach (string kw in keywords)
            {
                string pattern = $@"\b{Regex.Escape(kw)}\b";
                foreach (Match m in Regex.Matches(text, pattern, RegexOptions.IgnoreCase))
                {
                    txtFromJoins.Select(m.Index, m.Length);
                    if (string.Equals(kw, "ON", StringComparison.OrdinalIgnoreCase) || string.Equals(kw, "AS", StringComparison.OrdinalIgnoreCase))
                        txtFromJoins.SelectionColor = keywordColor;
                    else
                        txtFromJoins.SelectionColor = joinColor;
                }
            }

            // Restaurar cursor/selección
            if (selStart >= 0 && selStart <= txtFromJoins.Text.Length)
            {
                txtFromJoins.SelectionStart = selStart;
                txtFromJoins.SelectionLength = selLength;
            }
            else
            {
                txtFromJoins.SelectionStart = txtFromJoins.Text.Length;
                txtFromJoins.SelectionLength = 0;
            }
            txtFromJoins.SelectionColor = Color.Black;
            txtFromJoins.DeselectAll();
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
        private void dgvColumns_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0)
                return;

            var colName = dgvColumns.Columns[e.ColumnIndex].Name;
            if (!string.Equals(colName, "Ellipsis", StringComparison.OrdinalIgnoreCase))
                return;

            var row = dgvColumns.Rows[e.RowIndex];
            bool isEllipsis = Convert.ToBoolean(row.Cells["Ellipsis"].Value ?? false);

            var campoObj = row.Cells["CampoSQL"].Value;
            string campo = campoObj?.ToString() ?? "";

            if (isEllipsis)
            {
                // put uppercase CampoSQL into EllipsisSqlSources so generated .dat has a hint
                row.Cells["EllipsisSqlSources"].Value = campo.ToUpperInvariant();
            }
            else
            {
                // clear the per-row EllipsisSqlSources if Ellipsis unchecked
                row.Cells["EllipsisSqlSources"].Value = "";
            }
        }
    }//Form
}//namespace