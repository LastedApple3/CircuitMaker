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
            this.menuStrip = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.newBoardToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openBoardToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.saveBoardToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveBoardAsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.editToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.undoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.redoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.copyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.cutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pasteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.editExternalAppearanceToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.insertToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.insertBuiltinComponentToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.insertBoardComponentToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip.SuspendLayout();

            this.tblWholePage = new System.Windows.Forms.TableLayoutPanel();
            this.builder = Builder.LoadBoard("Boards/SR-Nor-Latch.brd"); // change this for final <---------------------------------------------------------------------
            this.btnSimulate = new System.Windows.Forms.Button();
            this.tblSelector = new System.Windows.Forms.TableLayoutPanel();
            this.tblWholePage.SuspendLayout();

            this.SuspendLayout();
            // 
            // menuStrip
            // 
            this.menuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.editToolStripMenuItem,
            this.insertToolStripMenuItem});
            this.menuStrip.Location = new System.Drawing.Point(0, 0);
            this.menuStrip.Name = "menuStrip";
            this.menuStrip.Size = new System.Drawing.Size(800, 24);
            this.menuStrip.TabIndex = 0;
            this.menuStrip.Text = "menuStrip";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.newBoardToolStripMenuItem,
            this.openBoardToolStripMenuItem,
            this.toolStripSeparator2,
            this.saveBoardToolStripMenuItem,
            this.saveBoardAsToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // newBoardToolStripMenuItem
            // 
            this.newBoardToolStripMenuItem.Name = "newBoardToolStripMenuItem";
            this.newBoardToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.newBoardToolStripMenuItem.Text = "New Board";
            this.newBoardToolStripMenuItem.Click += new System.EventHandler(this.newBoardToolStripMenuItem_Click);
            // 
            // openBoardToolStripMenuItem
            // 
            this.openBoardToolStripMenuItem.Name = "openBoardToolStripMenuItem";
            this.openBoardToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.openBoardToolStripMenuItem.Text = "Open Board";
            this.openBoardToolStripMenuItem.Click += new System.EventHandler(this.openBoardToolStripMenuItem_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(177, 6);
            // 
            // saveBoardToolStripMenuItem
            // 
            this.saveBoardToolStripMenuItem.Name = "saveBoardToolStripMenuItem";
            this.saveBoardToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.saveBoardToolStripMenuItem.Text = "Save Board";
            this.saveBoardToolStripMenuItem.Click += new System.EventHandler(this.saveBoardToolStripMenuItem_Click);
            // 
            // saveBoardAsToolStripMenuItem
            // 
            this.saveBoardAsToolStripMenuItem.Name = "saveBoardAsToolStripMenuItem";
            this.saveBoardAsToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.saveBoardAsToolStripMenuItem.Text = "Save Board As";
            this.saveBoardAsToolStripMenuItem.Click += new System.EventHandler(this.saveBoardAsToolStripMenuItem_Click);
            // 
            // editToolStripMenuItem
            // 
            this.editToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.undoToolStripMenuItem,
            this.redoToolStripMenuItem,
            this.toolStripSeparator1,
            this.copyToolStripMenuItem,
            this.cutToolStripMenuItem,
            this.pasteToolStripMenuItem,
            this.toolStripSeparator3,
            this.editExternalAppearanceToolStripMenuItem});
            this.editToolStripMenuItem.Name = "editToolStripMenuItem";
            this.editToolStripMenuItem.Size = new System.Drawing.Size(39, 20);
            this.editToolStripMenuItem.Text = "Edit";
            // 
            // undoToolStripMenuItem
            // 
            this.undoToolStripMenuItem.Name = "undoToolStripMenuItem";
            this.undoToolStripMenuItem.Size = new System.Drawing.Size(205, 22);
            this.undoToolStripMenuItem.Text = "Undo";
            this.undoToolStripMenuItem.Click += new System.EventHandler(this.undoToolStripMenuItem_Click);
            // 
            // redoToolStripMenuItem
            // 
            this.redoToolStripMenuItem.Name = "redoToolStripMenuItem";
            this.redoToolStripMenuItem.Size = new System.Drawing.Size(205, 22);
            this.redoToolStripMenuItem.Text = "Redo";
            this.redoToolStripMenuItem.Click += new System.EventHandler(this.redoToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(202, 6);
            // 
            // copyToolStripMenuItem
            // 
            this.copyToolStripMenuItem.Name = "copyToolStripMenuItem";
            this.copyToolStripMenuItem.Size = new System.Drawing.Size(205, 22);
            this.copyToolStripMenuItem.Text = "Copy";
            this.copyToolStripMenuItem.Click += new System.EventHandler(this.copyToolStripMenuItem_Click);
            // 
            // cutToolStripMenuItem
            // 
            this.cutToolStripMenuItem.Name = "cutToolStripMenuItem";
            this.cutToolStripMenuItem.Size = new System.Drawing.Size(205, 22);
            this.cutToolStripMenuItem.Text = "Cut";
            this.cutToolStripMenuItem.Click += new System.EventHandler(this.cutToolStripMenuItem_Click);
            // 
            // pasteToolStripMenuItem
            // 
            this.pasteToolStripMenuItem.Name = "pasteToolStripMenuItem";
            this.pasteToolStripMenuItem.Size = new System.Drawing.Size(205, 22);
            this.pasteToolStripMenuItem.Text = "Paste";
            this.pasteToolStripMenuItem.Click += new System.EventHandler(this.pasteToolStripMenuItem_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(202, 6);
            // 
            // editExternalAppearanceToolStripMenuItem
            // 
            this.editExternalAppearanceToolStripMenuItem.Name = "editExternalAppearanceToolStripMenuItem";
            this.editExternalAppearanceToolStripMenuItem.Size = new System.Drawing.Size(205, 22);
            this.editExternalAppearanceToolStripMenuItem.Text = "Edit External Appearance";
            this.editExternalAppearanceToolStripMenuItem.Click += new System.EventHandler(this.editExternalAppearanceToolStripMenuItem_Click);
            // 
            // insertToolStripMenuItem
            // 
            this.insertToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.insertBuiltinComponentToolStripMenuItem,
            this.insertBoardComponentToolStripMenuItem});
            this.insertToolStripMenuItem.Name = "insertToolStripMenuItem";
            this.insertToolStripMenuItem.Size = new System.Drawing.Size(48, 20);
            this.insertToolStripMenuItem.Text = "Insert";
            // 
            // insertComponentToolStripMenuItem
            // 
            this.insertBuiltinComponentToolStripMenuItem.Name = "insertComponentToolStripMenuItem";
            this.insertBuiltinComponentToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.insertBuiltinComponentToolStripMenuItem.Text = "Insert Builtin Component";
            this.insertBuiltinComponentToolStripMenuItem.Click += new System.EventHandler(this.insertBuiltinComponentToolStripMenuItem_Click);
            // 
            // insertFromBoardToolStripMenuItem
            // 
            this.insertBoardComponentToolStripMenuItem.Name = "insertFromBoardToolStripMenuItem";
            this.insertBoardComponentToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.insertBoardComponentToolStripMenuItem.Text = "Insert Board Component";
            this.insertBoardComponentToolStripMenuItem.Click += new System.EventHandler(this.insertBoardComponentToolStripMenuItem_Click);
            // 
            // tblWholePage
            // 
            this.tblWholePage.ColumnCount = 2;
            this.tblWholePage.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tblWholePage.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            this.tblWholePage.Controls.Add(this.builder, 0, 0);
            this.tblWholePage.Controls.Add(this.btnSimulate, 1, 1);
            this.tblWholePage.Controls.Add(this.tblSelector, 0, 1);
            this.tblWholePage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tblWholePage.Location = new System.Drawing.Point(0, 24);
            this.tblWholePage.Name = "tblWholePage";
            this.tblWholePage.RowCount = 2;
            this.tblWholePage.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tblWholePage.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
            this.tblWholePage.Size = new System.Drawing.Size(800, 426);
            this.tblWholePage.TabIndex = 2;
            // 
            // builder
            // 
            this.tblWholePage.SetColumnSpan(this.builder, 2);
            this.builder.Dock = System.Windows.Forms.DockStyle.Fill;
            this.builder.Location = new System.Drawing.Point(3, 3);
            this.builder.Name = "builder";
            this.builder.Size = new System.Drawing.Size(794, 370);
            this.builder.TabIndex = 0;
            this.builder.SimulatingChange += BtnSimulate_UpdateText;
            // 
            // btnSimulate
            // 
            this.btnSimulate.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnSimulate.Location = new System.Drawing.Point(703, 379);
            this.btnSimulate.Name = "btnSimulate";
            this.btnSimulate.Size = new System.Drawing.Size(94, 44);
            this.btnSimulate.TabIndex = 1;
            this.btnSimulate.Text = "Start Simulation";
            this.btnSimulate.UseVisualStyleBackColor = true;
            this.btnSimulate.Click += BtnSimulate_Click;
            // 
            // tblSelector
            // 
            this.tblSelector.ColumnCount = 2;
            this.tblSelector.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tblSelector.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tblSelector.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.tblSelector.Location = new System.Drawing.Point(3, 379);
            this.tblSelector.Name = "tblSelector";
            this.tblSelector.RowCount = 1;
            this.tblSelector.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tblSelector.Size = new System.Drawing.Size(694, 44);
            this.tblSelector.TabIndex = 2;
            // 
            // GUIForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
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
        private System.Windows.Forms.MenuStrip menuStrip;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem newBoardToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openBoardToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveBoardToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveBoardAsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem editToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem undoToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem redoToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem copyToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem cutToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem pasteToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripMenuItem editExternalAppearanceToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem insertToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem insertBuiltinComponentToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem insertBoardComponentToolStripMenuItem;
        private System.Windows.Forms.TableLayoutPanel tblWholePage;
        private Builder builder;
        private System.Windows.Forms.Button btnSimulate;
        private System.Windows.Forms.TableLayoutPanel tblSelector;
    }
}