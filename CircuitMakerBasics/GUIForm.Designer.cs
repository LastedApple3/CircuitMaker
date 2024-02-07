using System;
using System.Drawing;
using System.Windows.Forms;

namespace CircuitMaker.GUI
{
    partial class GUIForm
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
            this.menuStrip = new MenuStrip();
            this.fileToolStripMenuItem = new ToolStripMenuItem();
            this.newBoardToolStripMenuItem = new ToolStripMenuItem();
            this.openBoardToolStripMenuItem = new ToolStripMenuItem();
            this.toolStripSeparator2 = new ToolStripSeparator();
            this.saveBoardToolStripMenuItem = new ToolStripMenuItem();
            this.saveBoardAsToolStripMenuItem = new ToolStripMenuItem();
            this.editToolStripMenuItem = new ToolStripMenuItem();
            this.renameToolStripMenuItem = new ToolStripMenuItem();
            this.toolStripSeparator1 = new ToolStripSeparator();
            this.copyToolStripMenuItem = new ToolStripMenuItem();
            this.cutToolStripMenuItem = new ToolStripMenuItem();
            this.pasteToolStripMenuItem = new ToolStripMenuItem();
            this.toolStripSeparator3 = new ToolStripSeparator();
            this.editExternalAppearanceToolStripMenuItem = new ToolStripMenuItem();
            this.insertToolStripMenuItem = new ToolStripMenuItem();
            this.insertBuiltinComponentToolStripMenuItem = new ToolStripMenuItem();
            this.insertBoardComponentToolStripMenuItem = new ToolStripMenuItem();
            this.menuStrip.SuspendLayout();

            this.tblWholePage = new TableLayoutPanel();
            this.builder = Builder.NewBoard("untitled");
            this.btnSimulate = new Button();
            this.tblSelector = new TableLayoutPanel();
            this.tblWholePage.SuspendLayout();

            this.SuspendLayout();
            // 
            // menuStrip
            // 
            this.menuStrip.Items.AddRange(new ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.editToolStripMenuItem,
            this.insertToolStripMenuItem});
            this.menuStrip.Location = new Point(0, 0);
            this.menuStrip.Name = "menuStrip";
            this.menuStrip.Size = new Size(800, 24);
            this.menuStrip.TabIndex = 0;
            this.menuStrip.Text = "menuStrip";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] {
            this.newBoardToolStripMenuItem,
            this.openBoardToolStripMenuItem,
            this.toolStripSeparator2,
            this.saveBoardToolStripMenuItem,
            this.saveBoardAsToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // newBoardToolStripMenuItem
            // 
            this.newBoardToolStripMenuItem.Name = "newBoardToolStripMenuItem";
            this.newBoardToolStripMenuItem.Size = new Size(180, 22);
            this.newBoardToolStripMenuItem.Text = "New Board";
            this.newBoardToolStripMenuItem.Click += new EventHandler(this.newBoardToolStripMenuItem_Click);
            // 
            // openBoardToolStripMenuItem
            // 
            this.openBoardToolStripMenuItem.Name = "openBoardToolStripMenuItem";
            this.openBoardToolStripMenuItem.Size = new Size(180, 22);
            this.openBoardToolStripMenuItem.Text = "Open Board";
            this.openBoardToolStripMenuItem.Click += new EventHandler(this.openBoardToolStripMenuItem_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new Size(177, 6);
            // 
            // saveBoardToolStripMenuItem
            // 
            this.saveBoardToolStripMenuItem.Name = "saveBoardToolStripMenuItem";
            this.saveBoardToolStripMenuItem.Size = new Size(180, 22);
            this.saveBoardToolStripMenuItem.Text = "Save Board";
            this.saveBoardToolStripMenuItem.Click += new EventHandler(this.saveBoardToolStripMenuItem_Click);
            // 
            // saveBoardAsToolStripMenuItem
            // 
            this.saveBoardAsToolStripMenuItem.Name = "saveBoardAsToolStripMenuItem";
            this.saveBoardAsToolStripMenuItem.Size = new Size(180, 22);
            this.saveBoardAsToolStripMenuItem.Text = "Save Board As";
            this.saveBoardAsToolStripMenuItem.Click += new EventHandler(this.saveBoardAsToolStripMenuItem_Click);
            // 
            // editToolStripMenuItem
            // 
            this.editToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] {
            this.renameToolStripMenuItem,
            this.toolStripSeparator1,
            this.copyToolStripMenuItem,
            this.cutToolStripMenuItem,
            this.pasteToolStripMenuItem,
            this.toolStripSeparator3,
            this.editExternalAppearanceToolStripMenuItem});
            this.editToolStripMenuItem.Name = "editToolStripMenuItem";
            this.editToolStripMenuItem.Size = new Size(39, 20);
            this.editToolStripMenuItem.Text = "Edit";
            //
            // renameToolStripMenuItem
            //
            this.renameToolStripMenuItem.Name = "renameToolStripMenuItem";
            this.renameToolStripMenuItem.Size = new Size(205, 22);
            this.renameToolStripMenuItem.Text = "Rename";
            this.renameToolStripMenuItem.Click += new EventHandler(this.renameToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new Size(202, 6);
            // 
            // copyToolStripMenuItem
            // 
            this.copyToolStripMenuItem.Name = "copyToolStripMenuItem";
            this.copyToolStripMenuItem.Size = new Size(205, 22);
            this.copyToolStripMenuItem.Text = "Copy";
            this.copyToolStripMenuItem.Click += new EventHandler(this.copyToolStripMenuItem_Click);
            // 
            // cutToolStripMenuItem
            // 
            this.cutToolStripMenuItem.Name = "cutToolStripMenuItem";
            this.cutToolStripMenuItem.Size = new Size(205, 22);
            this.cutToolStripMenuItem.Text = "Cut";
            this.cutToolStripMenuItem.Click += new EventHandler(this.cutToolStripMenuItem_Click);
            // 
            // pasteToolStripMenuItem
            // 
            this.pasteToolStripMenuItem.Name = "pasteToolStripMenuItem";
            this.pasteToolStripMenuItem.Size = new Size(205, 22);
            this.pasteToolStripMenuItem.Text = "Paste";
            this.pasteToolStripMenuItem.Click += new EventHandler(this.pasteToolStripMenuItem_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new Size(202, 6);
            // 
            // editExternalAppearanceToolStripMenuItem
            // 
            this.editExternalAppearanceToolStripMenuItem.Name = "editExternalAppearanceToolStripMenuItem";
            this.editExternalAppearanceToolStripMenuItem.Size = new Size(205, 22);
            this.editExternalAppearanceToolStripMenuItem.Text = "Edit External Appearance";
            this.editExternalAppearanceToolStripMenuItem.Click += new EventHandler(this.editExternalAppearanceToolStripMenuItem_Click);
            // 
            // insertToolStripMenuItem
            // 
            this.insertToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] {
            this.insertBuiltinComponentToolStripMenuItem,
            this.insertBoardComponentToolStripMenuItem});
            this.insertToolStripMenuItem.Name = "insertToolStripMenuItem";
            this.insertToolStripMenuItem.Size = new Size(48, 20);
            this.insertToolStripMenuItem.Text = "Insert";
            // 
            // insertComponentToolStripMenuItem
            // 
            this.insertBuiltinComponentToolStripMenuItem.Name = "insertComponentToolStripMenuItem";
            this.insertBuiltinComponentToolStripMenuItem.Size = new Size(180, 22);
            this.insertBuiltinComponentToolStripMenuItem.Text = "Insert Builtin Component";
            this.insertBuiltinComponentToolStripMenuItem.Click += new EventHandler(this.insertBuiltinComponentToolStripMenuItem_Click);
            // 
            // insertFromBoardToolStripMenuItem
            // 
            this.insertBoardComponentToolStripMenuItem.Name = "insertFromBoardToolStripMenuItem";
            this.insertBoardComponentToolStripMenuItem.Size = new Size(180, 22);
            this.insertBoardComponentToolStripMenuItem.Text = "Insert Board Component";
            this.insertBoardComponentToolStripMenuItem.Click += new EventHandler(this.insertBoardComponentToolStripMenuItem_Click);
            // 
            // tblWholePage
            // 
            this.tblWholePage.ColumnCount = 2;
            this.tblWholePage.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            this.tblWholePage.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100F));
            this.tblWholePage.Controls.Add(this.builder, 0, 0);
            this.tblWholePage.Controls.Add(this.btnSimulate, 1, 1);
            this.tblWholePage.Controls.Add(this.tblSelector, 0, 1);
            this.tblWholePage.Dock = DockStyle.Fill;
            this.tblWholePage.Location = new Point(0, 24);
            this.tblWholePage.Name = "tblWholePage";
            this.tblWholePage.RowCount = 2;
            this.tblWholePage.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            this.tblWholePage.RowStyles.Add(new RowStyle(SizeType.Absolute, 50F));
            this.tblWholePage.Size = new Size(800, 426);
            this.tblWholePage.TabIndex = 2;
            // 
            // builder
            // 
            this.tblWholePage.SetColumnSpan(this.builder, 2);
            this.builder.Dock = DockStyle.Fill;
            this.builder.Location = new Point(3, 3);
            this.builder.Name = "builder";
            this.builder.Size = new Size(794, 370);
            this.builder.TabIndex = 0;
            this.builder.SimulatingChange += BtnSimulate_UpdateText;
            // 
            // btnSimulate
            // 
            this.btnSimulate.Dock = DockStyle.Fill;
            this.btnSimulate.Location = new Point(703, 379);
            this.btnSimulate.Name = "btnSimulate";
            this.btnSimulate.Size = new Size(94, 44);
            this.btnSimulate.TabIndex = 1;
            this.btnSimulate.Text = "Start Simulation";
            this.btnSimulate.UseVisualStyleBackColor = true;
            this.btnSimulate.Click += BtnSimulate_Click;
            // 
            // tblSelector
            // 
            this.tblSelector.ColumnCount = 2;
            this.tblSelector.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            this.tblSelector.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            this.tblSelector.Dock = DockStyle.Bottom;
            this.tblSelector.Location = new Point(3, 379);
            this.tblSelector.Name = "tblSelector";
            this.tblSelector.RowCount = 1;
            this.tblSelector.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            this.tblSelector.Size = new Size(694, 44);
            this.tblSelector.TabIndex = 2;
            // 
            // GUIForm
            // 
            this.AutoScaleDimensions = new SizeF(6F, 13F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(800, 450);
            this.Controls.Add(this.tblWholePage);
            this.Controls.Add(this.menuStrip);
            this.Name = "GUIForm";
            this.Text = "GUIForm";
            this.menuStrip.ResumeLayout(false);
            this.menuStrip.PerformLayout();
            this.tblWholePage.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion
        private MenuStrip menuStrip;
        private ToolStripMenuItem fileToolStripMenuItem;
        private ToolStripMenuItem newBoardToolStripMenuItem;
        private ToolStripMenuItem openBoardToolStripMenuItem;
        private ToolStripMenuItem saveBoardToolStripMenuItem;
        private ToolStripMenuItem saveBoardAsToolStripMenuItem;
        private ToolStripMenuItem editToolStripMenuItem;
        private ToolStripMenuItem renameToolStripMenuItem;
        private ToolStripMenuItem copyToolStripMenuItem;
        private ToolStripMenuItem cutToolStripMenuItem;
        private ToolStripMenuItem pasteToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator2;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripSeparator toolStripSeparator3;
        private ToolStripMenuItem editExternalAppearanceToolStripMenuItem;
        private ToolStripMenuItem insertToolStripMenuItem;
        private ToolStripMenuItem insertBuiltinComponentToolStripMenuItem;
        private ToolStripMenuItem insertBoardComponentToolStripMenuItem;
        private TableLayoutPanel tblWholePage;
        private Builder builder;
        private Button btnSimulate;
        private TableLayoutPanel tblSelector;
    }
}