using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DatEditorVDGrid
{
    public partial class Form1 : Form
    {
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
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void btnAddRow_Click(object sender, EventArgs e)
        {
            int rowIndex = dgvColumns.Rows.Add();
            dgvColumns.Rows[rowIndex].Cells["Pos"].Value = rowIndex + 1;
        }

        private void btnGenerate_Click(object sender, EventArgs e)
        {
            var filas = dgvColumns.Rows
                .Cast<DataGridViewRow>()
                .Where(r => r.Cells["CampoSQL"].Value != null)
                .ToList();

            var selectParts = new List<string>();
            var dataTypes = new List<string>();
            var fieldsToSave = new List<string>();
            var required = new List<string>();
            var headers = new List<string>();
            var widths = new List<string>();

            foreach (var row in filas)
            {
                string campo = row.Cells["CampoSQL"].Value?.ToString();
                string alias = row.Cells["Alias"].Value?.ToString();
                string tipo = row.Cells["DataType"].Value?.ToString() ?? "C";

                bool guardar = Convert.ToBoolean(row.Cells["Guardar"].Value ?? false);
                bool req = Convert.ToBoolean(row.Cells["Requerido"].Value ?? false);

                string header = row.Cells["Header"].Value?.ToString() ?? "";
                string width = row.Cells["Width"].Value?.ToString() ?? "0";

                // si alias está vacío, no añadir espacio extra
                selectParts.Add(string.IsNullOrWhiteSpace(alias) ? $"{campo}" : $"{campo} {alias}");
                dataTypes.Add(tipo);
                fieldsToSave.Add(guardar ? "1" : "0");
                required.Add(req ? "1" : "0");
                headers.Add(header);
                widths.Add(width);
            }

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

            // WhereSql, OrderSql, TableToSave y demás propiedades simples (también respetan 255)
            AppendWrappedProperty(sb, "WhereSql", txtWhere.Text);
            AppendWrappedProperty(sb, "OrderSql", txtOrder.Text);
            AppendWrappedProperty(sb, "TableToSave", txtTable.Text);

            // DataTypes, FieldsToSave, RequiredFields
            AppendWrappedProperty(sb, "DataTypes", string.Join(",", dataTypes));
            AppendWrappedProperty(sb, "FieldsToSave", string.Join(",", fieldsToSave));
            AppendWrappedProperty(sb, "RequiredFields", string.Join(",", required));

            // Llaves y foreigns
            AppendWrappedProperty(sb, "KeyField", txtKey.Text);
            AppendWrappedProperty(sb, "ForeignKey", txtForeign.Text);
            AppendWrappedProperty(sb, "ForeignToSave", txtForeignSave.Text);

            sb.AppendLine();
            sb.AppendLine("[Formatting]");

            // Headers: unir solo headers no vacíos para evitar comas extra
            var filteredHeaders = headers.Where(h => !string.IsNullOrWhiteSpace(h)).ToList();
            AppendWrappedProperty(sb, "Headers", filteredHeaders.Count > 0 ? string.Join(",", filteredHeaders) : "");

            // Widths: mantener todos los valores
            AppendWrappedProperty(sb, "Widths", string.Join(",", widths));

            sb.AppendLine($"Editable={(chkEditable.Checked ? "1" : "0")}");

            string colsToEdit = string.Join(",", filas
                .Where(r => Convert.ToBoolean(r.Cells["Editable"].Value ?? false))
                .Select(r => r.Cells["Alias"].Value?.ToString())
            );
            AppendWrappedProperty(sb, "ColsToEdit", colsToEdit);

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

            var fullSource = GetFullSourceSql(data);

            // separar SELECT vs FROM+JOINS
            SplitSelectFrom(fullSource, out string selectOnly, out string fromPart);

            // si el .dat ya trae TableToSave, usarlo; si no, extraer la tabla principal desde FROM
            if (data.ContainsKey("TableToSave"))
            {
                txtTable.Text = data["TableToSave"];
            }
            else
            {
                txtTable.Text = ExtractMainTableFromFromPart(fromPart);
            }

            // completar controles de configuración desde .dat
            txtWhere.Text = data.ContainsKey("WhereSql") ? data["WhereSql"] : "";
            txtOrder.Text = data.ContainsKey("OrderSql") ? data["OrderSql"] : "";
            txtKey.Text = data.ContainsKey("KeyField") ? data["KeyField"] : "";
            txtForeign.Text = data.ContainsKey("ForeignKey") ? data["ForeignKey"] : "";
            txtForeignSave.Text = data.ContainsKey("ForeignToSave") ? data["ForeignToSave"] : "";
            chkEditable.Checked = data.ContainsKey("Editable") && data["Editable"] == "1";

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
            var dataTypes = data.ContainsKey("DataTypes") ? data["DataTypes"].Split(',') : new string[0];
            var save = data.ContainsKey("FieldsToSave") ? data["FieldsToSave"].Split(',') : new string[0];
            var req = data.ContainsKey("RequiredFields") ? data["RequiredFields"].Split(',') : new string[0];
            var headers = data.ContainsKey("Headers") ? data["Headers"].Split(',') : new string[0];
            var widths = data.ContainsKey("Widths") ? data["Widths"].Split(',') : new string[0];

            for (int i = 0; i < campos.Count; i++)
            {
                var parts = campos[i].Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                string campo = parts[0];
                string alias = parts.Length > 1 ? parts[1] : "";

                int rowIndex = dgvColumns.Rows.Add();

                dgvColumns.Rows[rowIndex].Cells["Pos"].Value = i + 1;
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
            }

            MessageBox.Show("DAT cargado correctamente");
        }
        private string GetFullSourceSql(Dictionary<string, string> data)
        {
            var parts = new List<string>();

            int i = 1;

            while (true)
            {
                string key = (i == 1) ? "SourceSql" : $"SourceSql{i}";

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

        // Divide una cadena en trozos procurando no cortar palabras ni campos:
        // - Primero intenta partir en la última coma dentro del límite.
        // - Si no hay coma, busca el último espacio dentro del límite.
        // - Si no hay donde partir, usa el límite máximo.
        // El parámetro maxChunkLength se refiere a la longitud máxima permitida para cada trozo
        // (excluyendo la longitud de la cabecera "Key=" que se maneja en AppendWrappedProperty).
        private List<string> SplitSqlIntoLines(string sql, int maxChunkLength = 255)
        {
            var result = new List<string>();

            //if (string.IsNullOrEmpty(sql))
            //    return result;

            sql = sql.Trim();

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

            var lower = fullSql.ToLower();
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
            //if (string.IsNullOrWhiteSpace(fromPart))
            //    return "";

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
    }//Form
}//namespace