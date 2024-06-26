﻿using System;
using System.Windows.Forms;

namespace AgOpenGPS
{
    partial class FormFilePickerForExport
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
            this.components = new System.ComponentModel.Container();
            this.lvLines = new System.Windows.Forms.ListView();
            this.chName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.chDistance = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.chArea = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.btnByDistance = new System.Windows.Forms.Button();
            this.btnExportLv = new System.Windows.Forms.Button();
            this.btnDeleteAB = new System.Windows.Forms.Button();
            this.btnDeleteField = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.cbChooseFiletype = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.lbFarmer = new System.Windows.Forms.Label();
            this.lbFarm = new System.Windows.Forms.Label();
            this.txtFarmer = new System.Windows.Forms.TextBox();
            this.txtFarm = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // lvLines
            // 
            this.lvLines.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lvLines.BackColor = System.Drawing.SystemColors.InactiveCaption;
            this.lvLines.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.chName,
            this.chDistance,
            this.chArea});
            this.lvLines.Font = new System.Drawing.Font("Tahoma", 21.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lvLines.FullRowSelect = true;
            this.lvLines.GridLines = true;
            this.lvLines.HideSelection = false;
            this.lvLines.ImeMode = System.Windows.Forms.ImeMode.Off;
            this.lvLines.Location = new System.Drawing.Point(5, 12);
            this.lvLines.MultiSelect = false;
            this.lvLines.Name = "lvLines";
            this.lvLines.Size = new System.Drawing.Size(996, 459);
            this.lvLines.Sorting = System.Windows.Forms.SortOrder.Ascending;
            this.lvLines.TabIndex = 86;
            this.lvLines.UseCompatibleStateImageBehavior = false;
            this.lvLines.View = System.Windows.Forms.View.Details;
            // 
            // chName
            // 
            this.chName.Text = "Field Name";
            this.chName.Width = 680;
            // 
            // chDistance
            // 
            this.chDistance.Text = "Distance";
            this.chDistance.Width = 140;
            // 
            // chArea
            // 
            this.chArea.Text = "Area";
            this.chArea.Width = 140;
            // 
            // timer1
            // 
            this.timer1.Interval = 300;
            // 
            // btnByDistance
            // 
            this.btnByDistance.BackColor = System.Drawing.Color.Transparent;
            this.btnByDistance.Font = new System.Drawing.Font("Tahoma", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnByDistance.Image = global::AgOpenGPS.Properties.Resources.Sort;
            this.btnByDistance.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnByDistance.Location = new System.Drawing.Point(163, 558);
            this.btnByDistance.Margin = new System.Windows.Forms.Padding(5, 4, 5, 4);
            this.btnByDistance.Name = "btnByDistance";
            this.btnByDistance.Size = new System.Drawing.Size(147, 63);
            this.btnByDistance.TabIndex = 93;
            this.btnByDistance.Text = "Sort";
            this.btnByDistance.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnByDistance.UseVisualStyleBackColor = false;
            this.btnByDistance.Click += new System.EventHandler(this.btnByDistance_Click);
            // 
            // btnExportLv
            // 
            this.btnExportLv.BackColor = System.Drawing.Color.Transparent;
            this.btnExportLv.DialogResult = System.Windows.Forms.DialogResult.Yes;
            this.btnExportLv.Font = new System.Drawing.Font("Tahoma", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnExportLv.Image = global::AgOpenGPS.Properties.Resources.FileOpen;
            this.btnExportLv.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnExportLv.Location = new System.Drawing.Point(697, 558);
            this.btnExportLv.Margin = new System.Windows.Forms.Padding(5, 4, 5, 4);
            this.btnExportLv.Name = "btnExportLv";
            this.btnExportLv.Size = new System.Drawing.Size(261, 63);
            this.btnExportLv.TabIndex = 92;
            this.btnExportLv.Text = "Export";
            this.btnExportLv.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnExportLv.UseVisualStyleBackColor = false;
            this.btnExportLv.Click += new System.EventHandler(this.btnExportLv_Click);
            // 
            // btnDeleteAB
            // 
            this.btnDeleteAB.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnDeleteAB.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold);
            this.btnDeleteAB.ForeColor = System.Drawing.SystemColors.ControlText;
            this.btnDeleteAB.Image = global::AgOpenGPS.Properties.Resources.Cancel64;
            this.btnDeleteAB.Location = new System.Drawing.Point(367, 558);
            this.btnDeleteAB.Name = "btnDeleteAB";
            this.btnDeleteAB.Size = new System.Drawing.Size(71, 63);
            this.btnDeleteAB.TabIndex = 91;
            this.btnDeleteAB.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.btnDeleteAB.Click += new System.EventHandler(this.btnDeleteAB_Click);
            // 
            // btnDeleteField
            // 
            this.btnDeleteField.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold);
            this.btnDeleteField.ForeColor = System.Drawing.SystemColors.ControlText;
            this.btnDeleteField.Image = global::AgOpenGPS.Properties.Resources.skull;
            this.btnDeleteField.Location = new System.Drawing.Point(46, 558);
            this.btnDeleteField.Name = "btnDeleteField";
            this.btnDeleteField.Size = new System.Drawing.Size(71, 63);
            this.btnDeleteField.TabIndex = 94;
            this.btnDeleteField.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.btnDeleteField.Click += new System.EventHandler(this.btnDeleteField_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Tahoma", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(43, 539);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(74, 16);
            this.label1.TabIndex = 95;
            this.label1.Text = "Delete Field";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Tahoma", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(380, 539);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(45, 16);
            this.label2.TabIndex = 96;
            this.label2.Text = "Cancel";
            // 
            // cbChooseFiletype
            // 
            this.cbChooseFiletype.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbChooseFiletype.Font = new System.Drawing.Font("Tahoma", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cbChooseFiletype.FormattingEnabled = true;
            this.cbChooseFiletype.ItemHeight = 29;
            this.cbChooseFiletype.Items.AddRange(new object[] {
            "KML",
            "Shapefile",
            "Geopackage",
            "GeoJSON",
            "ISOXML"});
            this.cbChooseFiletype.Location = new System.Drawing.Point(482, 572);
            this.cbChooseFiletype.Name = "cbChooseFiletype";
            this.cbChooseFiletype.Size = new System.Drawing.Size(172, 37);
            this.cbChooseFiletype.TabIndex = 97;
            this.cbChooseFiletype.TextChanged += new System.EventHandler(this.cbChooseFiletype_TextChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Tahoma", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(517, 553);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(97, 16);
            this.label3.TabIndex = 98;
            this.label3.Text = "Choose Filetype";
            // 
            // lbFarmer
            // 
            this.lbFarmer.AutoSize = true;
            this.lbFarmer.Font = new System.Drawing.Font("Tahoma", 16F);
            this.lbFarmer.Location = new System.Drawing.Point(362, 505);
            this.lbFarmer.Name = "lbFarmer";
            this.lbFarmer.Size = new System.Drawing.Size(89, 27);
            this.lbFarmer.TabIndex = 99;
            this.lbFarmer.Text = "Farmer:";
            this.lbFarmer.Visible = false;
            // 
            // lbFarm
            // 
            this.lbFarm.AutoSize = true;
            this.lbFarm.Font = new System.Drawing.Font("Tahoma", 16F);
            this.lbFarm.Location = new System.Drawing.Point(692, 501);
            this.lbFarm.Name = "lbFarm";
            this.lbFarm.Size = new System.Drawing.Size(69, 27);
            this.lbFarm.TabIndex = 100;
            this.lbFarm.Text = "Farm:";
            this.lbFarm.Visible = false;
            // 
            // txtFarmer
            // 
            this.txtFarmer.Font = new System.Drawing.Font("Tahoma", 16F);
            this.txtFarmer.Location = new System.Drawing.Point(482, 502);
            this.txtFarmer.Name = "txtFarmer";
            this.txtFarmer.Size = new System.Drawing.Size(172, 33);
            this.txtFarmer.TabIndex = 101;
            this.txtFarmer.Text = "AgOpenFarmer";
            this.txtFarmer.Visible = false;
            this.txtFarmer.Enter += new System.EventHandler(this.txtFarmer_Enter);
            this.txtFarmer.Leave += new System.EventHandler(this.txtFarmer_Leave);
            // 
            // txtFarm
            // 
            this.txtFarm.Font = new System.Drawing.Font("Tahoma", 16F);
            this.txtFarm.Location = new System.Drawing.Point(776, 499);
            this.txtFarm.Name = "txtFarm";
            this.txtFarm.Size = new System.Drawing.Size(182, 33);
            this.txtFarm.TabIndex = 102;
            this.txtFarm.Text = "AgOpenFarm";
            this.txtFarm.Visible = false;
            this.txtFarm.Enter += new System.EventHandler(this.txtFarm_Enter);
            this.txtFarm.Leave += new System.EventHandler(this.txtFarm_Leave);
            // 
            // FormFilePickerForExport
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 23F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1001, 634);
            this.ControlBox = false;
            this.Controls.Add(this.txtFarm);
            this.Controls.Add(this.txtFarmer);
            this.Controls.Add(this.lbFarm);
            this.Controls.Add(this.lbFarmer);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.cbChooseFiletype);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnDeleteField);
            this.Controls.Add(this.btnByDistance);
            this.Controls.Add(this.btnExportLv);
            this.Controls.Add(this.btnDeleteAB);
            this.Controls.Add(this.lvLines);
            this.Font = new System.Drawing.Font("Tahoma", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Margin = new System.Windows.Forms.Padding(5);
            this.Name = "FormFilePickerForExport";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "FormFilePicker";
            this.Load += new System.EventHandler(this.FormFilePicker_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListView lvLines;
        private System.Windows.Forms.ColumnHeader chName;
        private System.Windows.Forms.ColumnHeader chDistance;
        private System.Windows.Forms.Button btnByDistance;
        private System.Windows.Forms.Button btnExportLv;
        private System.Windows.Forms.Button btnDeleteAB;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.ColumnHeader chArea;
        private System.Windows.Forms.Button btnDeleteField;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private FolderBrowserDialog folderBrowserDialog1;
        private ComboBox cbChooseFiletype;
        private Label label3;
        private Label lbFarmer;
        private Label lbFarm;
        private TextBox txtFarmer;
        private TextBox txtFarm;
    }
}