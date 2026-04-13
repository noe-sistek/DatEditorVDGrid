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
            this.btnAddRow = new System.Windows.Forms.Button();
            this.btnGenerate = new System.Windows.Forms.Button();
            this.btnValidate = new System.Windows.Forms.Button();
            this.txtOutput = new System.Windows.Forms.TextBox();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.dgvColumns = new System.Windows.Forms.DataGridView();
            this.panel1 = new System.Windows.Forms.Panel();
            this.btnLoadDat = new System.Windows.Forms.Button();
            this.panel2 = new System.Windows.Forms.Panel();
            this.lblFromJoins = new System.Windows.Forms.Label();
            this.chkEditable = new System.Windows.Forms.CheckBox();
            this.txtForeignSave = new System.Windows.Forms.TextBox();
            this.lblForeignSave = new System.Windows.Forms.Label();
            this.txtForeign = new System.Windows.Forms.TextBox();
            this.lblForeignKey = new System.Windows.Forms.Label();
            this.txtTable = new System.Windows.Forms.TextBox();
            this.lblKeyField = new System.Windows.Forms.Label();
            this.lblTable = new System.Windows.Forms.Label();
            this.txtKey = new System.Windows.Forms.TextBox();
            this.txtOrder = new System.Windows.Forms.TextBox();
            this.lblOrder = new System.Windows.Forms.Label();
            this.lblWhere = new System.Windows.Forms.Label();
            this.txtWhere = new System.Windows.Forms.TextBox();
            this.txtFromJoins = new System.Windows.Forms.RichTextBox();
            this.tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvColumns)).BeginInit();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.SuspendLayout();
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
            // txtOutput
            // 
            this.txtOutput.BackColor = System.Drawing.SystemColors.WindowText;
            this.txtOutput.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtOutput.Font = new System.Drawing.Font("Cascadia Mono", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtOutput.ForeColor = System.Drawing.Color.Azure;
            this.txtOutput.Location = new System.Drawing.Point(3, 593);
            this.txtOutput.Multiline = true;
            this.txtOutput.Name = "txtOutput";
            this.txtOutput.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtOutput.Size = new System.Drawing.Size(1065, 284);
            this.txtOutput.TabIndex = 4;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.dgvColumns, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.txtOutput, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.panel1, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.panel2, 0, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 4;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 150F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 400F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 250F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1071, 880);
            this.tableLayoutPanel1.TabIndex = 5;
            // 
            // dgvColumns
            // 
            this.dgvColumns.AllowUserToOrderColumns = true;
            this.dgvColumns.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvColumns.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvColumns.Location = new System.Drawing.Point(3, 153);
            this.dgvColumns.Name = "dgvColumns";
            this.dgvColumns.Size = new System.Drawing.Size(1065, 394);
            this.dgvColumns.TabIndex = 0;
            this.dgvColumns.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridView1_CellContentClick);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.btnLoadDat);
            this.panel1.Controls.Add(this.btnValidate);
            this.panel1.Controls.Add(this.btnGenerate);
            this.panel1.Controls.Add(this.btnAddRow);
            this.panel1.Location = new System.Drawing.Point(3, 553);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(356, 34);
            this.panel1.TabIndex = 5;
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
            // panel2
            // 
            this.panel2.Controls.Add(this.txtFromJoins);
            this.panel2.Controls.Add(this.lblFromJoins);
            this.panel2.Controls.Add(this.chkEditable);
            this.panel2.Controls.Add(this.txtForeignSave);
            this.panel2.Controls.Add(this.lblForeignSave);
            this.panel2.Controls.Add(this.txtForeign);
            this.panel2.Controls.Add(this.lblForeignKey);
            this.panel2.Controls.Add(this.txtTable);
            this.panel2.Controls.Add(this.lblKeyField);
            this.panel2.Controls.Add(this.lblTable);
            this.panel2.Controls.Add(this.txtKey);
            this.panel2.Controls.Add(this.txtOrder);
            this.panel2.Controls.Add(this.lblOrder);
            this.panel2.Controls.Add(this.lblWhere);
            this.panel2.Controls.Add(this.txtWhere);
            this.panel2.Location = new System.Drawing.Point(3, 3);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(1003, 144);
            this.panel2.TabIndex = 6;
            // 
            // lblFromJoins
            // 
            this.lblFromJoins.AutoSize = true;
            this.lblFromJoins.Location = new System.Drawing.Point(9, 28);
            this.lblFromJoins.Name = "lblFromJoins";
            this.lblFromJoins.Size = new System.Drawing.Size(78, 13);
            this.lblFromJoins.TabIndex = 15;
            this.lblFromJoins.Text = "From and Joins";
            // 
            // chkEditable
            // 
            this.chkEditable.AutoSize = true;
            this.chkEditable.Location = new System.Drawing.Point(927, 74);
            this.chkEditable.Name = "chkEditable";
            this.chkEditable.Size = new System.Drawing.Size(64, 17);
            this.chkEditable.TabIndex = 13;
            this.chkEditable.Text = "Editable";
            this.chkEditable.UseVisualStyleBackColor = true;
            // 
            // txtForeignSave
            // 
            this.txtForeignSave.Location = new System.Drawing.Point(891, 5);
            this.txtForeignSave.Name = "txtForeignSave";
            this.txtForeignSave.Size = new System.Drawing.Size(100, 20);
            this.txtForeignSave.TabIndex = 11;
            // 
            // lblForeignSave
            // 
            this.lblForeignSave.AutoSize = true;
            this.lblForeignSave.Location = new System.Drawing.Point(818, 8);
            this.lblForeignSave.Name = "lblForeignSave";
            this.lblForeignSave.Size = new System.Drawing.Size(67, 13);
            this.lblForeignSave.TabIndex = 10;
            this.lblForeignSave.Text = "ForeignSave";
            // 
            // txtForeign
            // 
            this.txtForeign.Location = new System.Drawing.Point(708, 5);
            this.txtForeign.Name = "txtForeign";
            this.txtForeign.Size = new System.Drawing.Size(100, 20);
            this.txtForeign.TabIndex = 9;
            // 
            // lblForeignKey
            // 
            this.lblForeignKey.AutoSize = true;
            this.lblForeignKey.Location = new System.Drawing.Point(642, 8);
            this.lblForeignKey.Name = "lblForeignKey";
            this.lblForeignKey.Size = new System.Drawing.Size(60, 13);
            this.lblForeignKey.TabIndex = 8;
            this.lblForeignKey.Text = "ForeignKey";
            // 
            // txtTable
            // 
            this.txtTable.Location = new System.Drawing.Point(382, 5);
            this.txtTable.Name = "txtTable";
            this.txtTable.Size = new System.Drawing.Size(100, 20);
            this.txtTable.TabIndex = 7;
            // 
            // lblKeyField
            // 
            this.lblKeyField.AutoSize = true;
            this.lblKeyField.Location = new System.Drawing.Point(488, 8);
            this.lblKeyField.Name = "lblKeyField";
            this.lblKeyField.Size = new System.Drawing.Size(47, 13);
            this.lblKeyField.TabIndex = 6;
            this.lblKeyField.Text = "KeyField";
            // 
            // lblTable
            // 
            this.lblTable.AutoSize = true;
            this.lblTable.Location = new System.Drawing.Point(302, 8);
            this.lblTable.Name = "lblTable";
            this.lblTable.Size = new System.Drawing.Size(74, 13);
            this.lblTable.TabIndex = 5;
            this.lblTable.Text = "Table to Save";
            // 
            // txtKey
            // 
            this.txtKey.Location = new System.Drawing.Point(536, 5);
            this.txtKey.Name = "txtKey";
            this.txtKey.Size = new System.Drawing.Size(100, 20);
            this.txtKey.TabIndex = 4;
            // 
            // txtOrder
            // 
            this.txtOrder.Location = new System.Drawing.Point(196, 5);
            this.txtOrder.Name = "txtOrder";
            this.txtOrder.Size = new System.Drawing.Size(100, 20);
            this.txtOrder.TabIndex = 3;
            // 
            // lblOrder
            // 
            this.lblOrder.AutoSize = true;
            this.lblOrder.Location = new System.Drawing.Point(157, 8);
            this.lblOrder.Name = "lblOrder";
            this.lblOrder.Size = new System.Drawing.Size(33, 13);
            this.lblOrder.TabIndex = 2;
            this.lblOrder.Text = "Order";
            // 
            // lblWhere
            // 
            this.lblWhere.AutoSize = true;
            this.lblWhere.Location = new System.Drawing.Point(6, 8);
            this.lblWhere.Name = "lblWhere";
            this.lblWhere.Size = new System.Drawing.Size(39, 13);
            this.lblWhere.TabIndex = 1;
            this.lblWhere.Text = "Where";
            // 
            // txtWhere
            // 
            this.txtWhere.Location = new System.Drawing.Point(51, 5);
            this.txtWhere.Name = "txtWhere";
            this.txtWhere.Size = new System.Drawing.Size(100, 20);
            this.txtWhere.TabIndex = 0;
            // 
            // txtFromJoins
            // 
            this.txtFromJoins.Font = new System.Drawing.Font("Cascadia Mono", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtFromJoins.Location = new System.Drawing.Point(9, 44);
            this.txtFromJoins.Name = "txtFromJoins";
            this.txtFromJoins.Size = new System.Drawing.Size(799, 97);
            this.txtFromJoins.TabIndex = 16;
            this.txtFromJoins.Text = "";
            this.txtFromJoins.TextChanged += new System.EventHandler(this.txtFromJoins_TextChanged);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1071, 880);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvColumns)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Button btnAddRow;
        private System.Windows.Forms.Button btnGenerate;
        private System.Windows.Forms.Button btnValidate;
        private System.Windows.Forms.TextBox txtOutput;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.DataGridView dgvColumns;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btnLoadDat;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Label lblWhere;
        private System.Windows.Forms.TextBox txtWhere;
        private System.Windows.Forms.TextBox txtKey;
        private System.Windows.Forms.TextBox txtOrder;
        private System.Windows.Forms.Label lblOrder;
        private System.Windows.Forms.TextBox txtTable;
        private System.Windows.Forms.Label lblKeyField;
        private System.Windows.Forms.Label lblTable;
        private System.Windows.Forms.TextBox txtForeign;
        private System.Windows.Forms.Label lblForeignKey;
        private System.Windows.Forms.CheckBox chkEditable;
        private System.Windows.Forms.TextBox txtForeignSave;
        private System.Windows.Forms.Label lblForeignSave;
        private System.Windows.Forms.Label lblFromJoins;
        private System.Windows.Forms.RichTextBox txtFromJoins;
    }
}

