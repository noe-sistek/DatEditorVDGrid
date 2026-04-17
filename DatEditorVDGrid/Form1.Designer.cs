using System;
using System.Windows.Forms;

namespace DatEditorVDGrid
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle5 = new System.Windows.Forms.DataGridViewCellStyle();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.dgvColumns = new System.Windows.Forms.DataGridView();
            this.panel2 = new System.Windows.Forms.Panel();
            this.richSelect = new System.Windows.Forms.RichTextBox();
            this.txtForeignAlias = new System.Windows.Forms.TextBox();
            this.lblForeignAlias = new System.Windows.Forms.Label();
            this.txtCountableCol = new System.Windows.Forms.TextBox();
            this.lblCountable = new System.Windows.Forms.Label();
            this.txtSqlIdentityKey = new System.Windows.Forms.TextBox();
            this.lblSqlIdentityKey = new System.Windows.Forms.Label();
            this.chkProcedure = new System.Windows.Forms.CheckBox();
            this.lblFromJoins = new System.Windows.Forms.Label();
            this.chkEditable = new System.Windows.Forms.CheckBox();
            this.txtForeignSave = new System.Windows.Forms.TextBox();
            this.lblForeignSave = new System.Windows.Forms.Label();
            this.txtForeignKey = new System.Windows.Forms.TextBox();
            this.lblForeignKey = new System.Windows.Forms.Label();
            this.txtTableToSave = new System.Windows.Forms.TextBox();
            this.lblKeyField = new System.Windows.Forms.Label();
            this.lblTable = new System.Windows.Forms.Label();
            this.txtKeyField = new System.Windows.Forms.TextBox();
            this.txtOrderSql = new System.Windows.Forms.TextBox();
            this.lblOrder = new System.Windows.Forms.Label();
            this.lblWhere = new System.Windows.Forms.Label();
            this.txtWhereSql = new System.Windows.Forms.TextBox();
            this.richSalida = new System.Windows.Forms.RichTextBox();
            this.btnAddRow = new System.Windows.Forms.Button();
            this.btnGenerate = new System.Windows.Forms.Button();
            this.btnValidate = new System.Windows.Forms.Button();
            this.btnLoadDat = new System.Windows.Forms.Button();
            this.btnFillValues = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.lblStatus = new System.Windows.Forms.Label();
            this.tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvColumns)).BeginInit();
            this.panel2.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.dgvColumns, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.panel1, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.panel2, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.richSalida, 0, 3);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 4;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 150F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 400F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 250F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1228, 880);
            this.tableLayoutPanel1.TabIndex = 5;
            this.tableLayoutPanel1.Paint += new System.Windows.Forms.PaintEventHandler(this.tableLayoutPanel1_Paint);
            // 
            // dgvColumns
            // 
            this.dgvColumns.AllowUserToOrderColumns = true;
            this.dgvColumns.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewCellStyle5.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle5.BackColor = System.Drawing.SystemColors.ButtonFace;
            dataGridViewCellStyle5.Font = new System.Drawing.Font("Cascadia Mono", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle5.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle5.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle5.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle5.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvColumns.DefaultCellStyle = dataGridViewCellStyle5;
            this.dgvColumns.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvColumns.Location = new System.Drawing.Point(3, 153);
            this.dgvColumns.Name = "dgvColumns";
            this.dgvColumns.Size = new System.Drawing.Size(1222, 394);
            this.dgvColumns.TabIndex = 0;
            this.dgvColumns.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvColumns_CellContentClick);
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.richSelect);
            this.panel2.Controls.Add(this.txtForeignAlias);
            this.panel2.Controls.Add(this.lblForeignAlias);
            this.panel2.Controls.Add(this.txtCountableCol);
            this.panel2.Controls.Add(this.lblCountable);
            this.panel2.Controls.Add(this.txtSqlIdentityKey);
            this.panel2.Controls.Add(this.lblSqlIdentityKey);
            this.panel2.Controls.Add(this.chkProcedure);
            this.panel2.Controls.Add(this.lblFromJoins);
            this.panel2.Controls.Add(this.chkEditable);
            this.panel2.Controls.Add(this.txtForeignSave);
            this.panel2.Controls.Add(this.lblForeignSave);
            this.panel2.Controls.Add(this.txtForeignKey);
            this.panel2.Controls.Add(this.lblForeignKey);
            this.panel2.Controls.Add(this.txtTableToSave);
            this.panel2.Controls.Add(this.lblKeyField);
            this.panel2.Controls.Add(this.lblTable);
            this.panel2.Controls.Add(this.txtKeyField);
            this.panel2.Controls.Add(this.txtOrderSql);
            this.panel2.Controls.Add(this.lblOrder);
            this.panel2.Controls.Add(this.lblWhere);
            this.panel2.Controls.Add(this.txtWhereSql);
            this.panel2.Location = new System.Drawing.Point(3, 3);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(1111, 144);
            this.panel2.TabIndex = 6;
            // 
            // richSelect
            // 
            this.richSelect.BackColor = System.Drawing.SystemColors.ControlLight;
            this.richSelect.Location = new System.Drawing.Point(556, 28);
            this.richSelect.Name = "richSelect";
            this.richSelect.Size = new System.Drawing.Size(552, 112);
            this.richSelect.TabIndex = 24;
            this.richSelect.Text = "";
            // 
            // txtForeignAlias
            // 
            this.txtForeignAlias.Location = new System.Drawing.Point(272, 75);
            this.txtForeignAlias.Name = "txtForeignAlias";
            this.txtForeignAlias.Size = new System.Drawing.Size(100, 20);
            this.txtForeignAlias.TabIndex = 23;
            // 
            // lblForeignAlias
            // 
            this.lblForeignAlias.AutoSize = true;
            this.lblForeignAlias.Location = new System.Drawing.Point(202, 78);
            this.lblForeignAlias.Name = "lblForeignAlias";
            this.lblForeignAlias.Size = new System.Drawing.Size(64, 13);
            this.lblForeignAlias.TabIndex = 22;
            this.lblForeignAlias.Text = "ForeignAlias";
            // 
            // txtCountableCol
            // 
            this.txtCountableCol.Location = new System.Drawing.Point(86, 98);
            this.txtCountableCol.Name = "txtCountableCol";
            this.txtCountableCol.Size = new System.Drawing.Size(100, 20);
            this.txtCountableCol.TabIndex = 21;
            // 
            // lblCountable
            // 
            this.lblCountable.AutoSize = true;
            this.lblCountable.Location = new System.Drawing.Point(14, 101);
            this.lblCountable.Name = "lblCountable";
            this.lblCountable.Size = new System.Drawing.Size(73, 13);
            this.lblCountable.TabIndex = 20;
            this.lblCountable.Text = "Countable Col";
            // 
            // txtSqlIdentityKey
            // 
            this.txtSqlIdentityKey.Location = new System.Drawing.Point(86, 75);
            this.txtSqlIdentityKey.Name = "txtSqlIdentityKey";
            this.txtSqlIdentityKey.Size = new System.Drawing.Size(100, 20);
            this.txtSqlIdentityKey.TabIndex = 19;
            // 
            // lblSqlIdentityKey
            // 
            this.lblSqlIdentityKey.AutoSize = true;
            this.lblSqlIdentityKey.Location = new System.Drawing.Point(13, 78);
            this.lblSqlIdentityKey.Name = "lblSqlIdentityKey";
            this.lblSqlIdentityKey.Size = new System.Drawing.Size(74, 13);
            this.lblSqlIdentityKey.TabIndex = 18;
            this.lblSqlIdentityKey.Text = "SqlIdentityKey";
            // 
            // chkProcedure
            // 
            this.chkProcedure.AutoSize = true;
            this.chkProcedure.Location = new System.Drawing.Point(272, 100);
            this.chkProcedure.Name = "chkProcedure";
            this.chkProcedure.Size = new System.Drawing.Size(75, 17);
            this.chkProcedure.TabIndex = 17;
            this.chkProcedure.Text = "Procedure";
            this.chkProcedure.UseVisualStyleBackColor = true;
            // 
            // lblFromJoins
            // 
            this.lblFromJoins.AutoSize = true;
            this.lblFromJoins.Location = new System.Drawing.Point(553, 8);
            this.lblFromJoins.Name = "lblFromJoins";
            this.lblFromJoins.Size = new System.Drawing.Size(78, 13);
            this.lblFromJoins.TabIndex = 15;
            this.lblFromJoins.Text = "From and Joins";
            // 
            // chkEditable
            // 
            this.chkEditable.AutoSize = true;
            this.chkEditable.Location = new System.Drawing.Point(272, 121);
            this.chkEditable.Name = "chkEditable";
            this.chkEditable.Size = new System.Drawing.Size(64, 17);
            this.chkEditable.TabIndex = 13;
            this.chkEditable.Text = "Editable";
            this.chkEditable.UseVisualStyleBackColor = true;
            // 
            // txtForeignSave
            // 
            this.txtForeignSave.Location = new System.Drawing.Point(272, 52);
            this.txtForeignSave.Name = "txtForeignSave";
            this.txtForeignSave.Size = new System.Drawing.Size(100, 20);
            this.txtForeignSave.TabIndex = 11;
            // 
            // lblForeignSave
            // 
            this.lblForeignSave.AutoSize = true;
            this.lblForeignSave.Location = new System.Drawing.Point(199, 55);
            this.lblForeignSave.Name = "lblForeignSave";
            this.lblForeignSave.Size = new System.Drawing.Size(67, 13);
            this.lblForeignSave.TabIndex = 10;
            this.lblForeignSave.Text = "ForeignSave";
            // 
            // txtForeignKey
            // 
            this.txtForeignKey.Location = new System.Drawing.Point(272, 28);
            this.txtForeignKey.Name = "txtForeignKey";
            this.txtForeignKey.Size = new System.Drawing.Size(250, 20);
            this.txtForeignKey.TabIndex = 9;
            // 
            // lblForeignKey
            // 
            this.lblForeignKey.AutoSize = true;
            this.lblForeignKey.Location = new System.Drawing.Point(206, 31);
            this.lblForeignKey.Name = "lblForeignKey";
            this.lblForeignKey.Size = new System.Drawing.Size(60, 13);
            this.lblForeignKey.TabIndex = 8;
            this.lblForeignKey.Text = "ForeignKey";
            // 
            // txtTableToSave
            // 
            this.txtTableToSave.Location = new System.Drawing.Point(86, 52);
            this.txtTableToSave.Name = "txtTableToSave";
            this.txtTableToSave.Size = new System.Drawing.Size(100, 20);
            this.txtTableToSave.TabIndex = 7;
            // 
            // lblKeyField
            // 
            this.lblKeyField.AutoSize = true;
            this.lblKeyField.Location = new System.Drawing.Point(38, 123);
            this.lblKeyField.Name = "lblKeyField";
            this.lblKeyField.Size = new System.Drawing.Size(47, 13);
            this.lblKeyField.TabIndex = 6;
            this.lblKeyField.Text = "KeyField";
            // 
            // lblTable
            // 
            this.lblTable.AutoSize = true;
            this.lblTable.Location = new System.Drawing.Point(6, 55);
            this.lblTable.Name = "lblTable";
            this.lblTable.Size = new System.Drawing.Size(74, 13);
            this.lblTable.TabIndex = 5;
            this.lblTable.Text = "Table to Save";
            // 
            // txtKeyField
            // 
            this.txtKeyField.Location = new System.Drawing.Point(86, 120);
            this.txtKeyField.Name = "txtKeyField";
            this.txtKeyField.Size = new System.Drawing.Size(100, 20);
            this.txtKeyField.TabIndex = 4;
            // 
            // txtOrderSql
            // 
            this.txtOrderSql.Location = new System.Drawing.Point(86, 28);
            this.txtOrderSql.Name = "txtOrderSql";
            this.txtOrderSql.Size = new System.Drawing.Size(100, 20);
            this.txtOrderSql.TabIndex = 3;
            // 
            // lblOrder
            // 
            this.lblOrder.AutoSize = true;
            this.lblOrder.Location = new System.Drawing.Point(47, 31);
            this.lblOrder.Name = "lblOrder";
            this.lblOrder.Size = new System.Drawing.Size(33, 13);
            this.lblOrder.TabIndex = 2;
            this.lblOrder.Text = "Order";
            // 
            // lblWhere
            // 
            this.lblWhere.AutoSize = true;
            this.lblWhere.Location = new System.Drawing.Point(41, 8);
            this.lblWhere.Name = "lblWhere";
            this.lblWhere.Size = new System.Drawing.Size(39, 13);
            this.lblWhere.TabIndex = 1;
            this.lblWhere.Text = "Where";
            // 
            // txtWhereSql
            // 
            this.txtWhereSql.Location = new System.Drawing.Point(86, 5);
            this.txtWhereSql.Name = "txtWhereSql";
            this.txtWhereSql.Size = new System.Drawing.Size(436, 20);
            this.txtWhereSql.TabIndex = 0;
            // 
            // richSalida
            // 
            this.richSalida.BackColor = System.Drawing.SystemColors.Desktop;
            this.richSalida.Dock = System.Windows.Forms.DockStyle.Fill;
            this.richSalida.Font = new System.Drawing.Font("Cascadia Mono", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.richSalida.ForeColor = System.Drawing.SystemColors.Control;
            this.richSalida.Location = new System.Drawing.Point(3, 593);
            this.richSalida.Name = "richSalida";
            this.richSalida.Size = new System.Drawing.Size(1222, 284);
            this.richSalida.TabIndex = 7;
            this.richSalida.Text = "";
            this.richSalida.WordWrap = false;
            // 
            // btnAddRow
            // 
            this.btnAddRow.Location = new System.Drawing.Point(10, 8);
            this.btnAddRow.Name = "btnAddRow";
            this.btnAddRow.Size = new System.Drawing.Size(75, 23);
            this.btnAddRow.TabIndex = 1;
            this.btnAddRow.Text = "Agregar Fila";
            this.btnAddRow.UseVisualStyleBackColor = true;
            this.btnAddRow.Click += new System.EventHandler(this.btnAddRow_Click);
            // 
            // btnGenerate
            // 
            this.btnGenerate.Location = new System.Drawing.Point(91, 8);
            this.btnGenerate.Name = "btnGenerate";
            this.btnGenerate.Size = new System.Drawing.Size(75, 23);
            this.btnGenerate.TabIndex = 2;
            this.btnGenerate.Text = "Generar .dat";
            this.btnGenerate.UseVisualStyleBackColor = true;
            this.btnGenerate.Click += new System.EventHandler(this.btnGenerate_Click);
            // 
            // btnValidate
            // 
            this.btnValidate.Location = new System.Drawing.Point(172, 8);
            this.btnValidate.Name = "btnValidate";
            this.btnValidate.Size = new System.Drawing.Size(75, 23);
            this.btnValidate.TabIndex = 3;
            this.btnValidate.Text = "Validar";
            this.btnValidate.UseVisualStyleBackColor = true;
            this.btnValidate.Click += new System.EventHandler(this.btnValidate_Click);
            // 
            // btnLoadDat
            // 
            this.btnLoadDat.Location = new System.Drawing.Point(253, 8);
            this.btnLoadDat.Name = "btnLoadDat";
            this.btnLoadDat.Size = new System.Drawing.Size(75, 23);
            this.btnLoadDat.TabIndex = 4;
            this.btnLoadDat.Text = "Cargar .dat";
            this.btnLoadDat.UseVisualStyleBackColor = true;
            this.btnLoadDat.Click += new System.EventHandler(this.btnLoadDat_Click);
            // 
            // btnFillValues
            // 
            this.btnFillValues.Location = new System.Drawing.Point(334, 8);
            this.btnFillValues.Name = "btnFillValues";
            this.btnFillValues.Size = new System.Drawing.Size(75, 23);
            this.btnFillValues.TabIndex = 5;
            this.btnFillValues.Text = "LLENAR";
            this.btnFillValues.UseVisualStyleBackColor = true;
            this.btnFillValues.Click += new System.EventHandler(this.btnFillValues_Click);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.lblStatus);
            this.panel1.Controls.Add(this.btnFillValues);
            this.panel1.Controls.Add(this.btnLoadDat);
            this.panel1.Controls.Add(this.btnValidate);
            this.panel1.Controls.Add(this.btnGenerate);
            this.panel1.Controls.Add(this.btnAddRow);
            this.panel1.Location = new System.Drawing.Point(3, 553);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1053, 34);
            this.panel1.TabIndex = 5;
            // 
            // lblStatus
            // 
            this.lblStatus.AutoSize = true;
            this.lblStatus.Dock = System.Windows.Forms.DockStyle.Right;
            this.lblStatus.Location = new System.Drawing.Point(1053, 0);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(0, 13);
            this.lblStatus.TabIndex = 6;
            this.lblStatus.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1228, 880);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.tableLayoutPanel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvColumns)).EndInit();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

        }

        //private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        //{
        //    throw new NotImplementedException();
        //}

        #endregion
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.DataGridView dgvColumns;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Label lblWhere;
        private System.Windows.Forms.TextBox txtWhereSql;
        private System.Windows.Forms.TextBox txtKeyField;
        private System.Windows.Forms.TextBox txtOrderSql;
        private System.Windows.Forms.Label lblOrder;
        private System.Windows.Forms.TextBox txtTableToSave;
        private System.Windows.Forms.Label lblKeyField;
        private System.Windows.Forms.Label lblTable;
        private System.Windows.Forms.TextBox txtForeignKey;
        private System.Windows.Forms.Label lblForeignKey;
        private System.Windows.Forms.CheckBox chkEditable;
        private System.Windows.Forms.TextBox txtForeignSave;
        private System.Windows.Forms.Label lblForeignSave;
        private System.Windows.Forms.Label lblFromJoins;
        private System.Windows.Forms.Label lblSqlIdentityKey;
        private System.Windows.Forms.CheckBox chkProcedure;
        private System.Windows.Forms.TextBox txtCountableCol;
        private System.Windows.Forms.Label lblCountable;
        private System.Windows.Forms.TextBox txtSqlIdentityKey;
        private System.Windows.Forms.TextBox txtForeignAlias;
        private System.Windows.Forms.Label lblForeignAlias;
        private System.Windows.Forms.RichTextBox richSelect;
        private System.Windows.Forms.RichTextBox richSalida;
        private Panel panel1;
        private Button btnFillValues;
        private Button btnLoadDat;
        private Button btnValidate;
        private Button btnGenerate;
        private Button btnAddRow;
        private Label lblStatus;
    }
}

