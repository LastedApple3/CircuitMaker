namespace CircuitMaker
{
    partial class MenuTestForm
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
            this.cutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.copyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pasteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.editExternalAppearanceToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.insertToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.insertComponentToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.insertFromBoardToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip.SuspendLayout();
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
            this.cutToolStripMenuItem,
            this.copyToolStripMenuItem,
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
            // cutToolStripMenuItem
            // 
            this.cutToolStripMenuItem.Name = "cutToolStripMenuItem";
            this.cutToolStripMenuItem.Size = new System.Drawing.Size(205, 22);
            this.cutToolStripMenuItem.Text = "Cut";
            this.cutToolStripMenuItem.Click += new System.EventHandler(this.cutToolStripMenuItem_Click);
            // 
            // copyToolStripMenuItem
            // 
            this.copyToolStripMenuItem.Name = "copyToolStripMenuItem";
            this.copyToolStripMenuItem.Size = new System.Drawing.Size(205, 22);
            this.copyToolStripMenuItem.Text = "Copy";
            this.copyToolStripMenuItem.Click += new System.EventHandler(this.copyToolStripMenuItem_Click);
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
            this.insertComponentToolStripMenuItem,
            this.insertFromBoardToolStripMenuItem});
            this.insertToolStripMenuItem.Name = "insertToolStripMenuItem";
            this.insertToolStripMenuItem.Size = new System.Drawing.Size(48, 20);
            this.insertToolStripMenuItem.Text = "Insert";
            // 
            // insertComponentToolStripMenuItem
            // 
            this.insertComponentToolStripMenuItem.Name = "insertComponentToolStripMenuItem";
            this.insertComponentToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.insertComponentToolStripMenuItem.Text = "Insert Component";
            this.insertComponentToolStripMenuItem.Click += new System.EventHandler(this.insertComponentToolStripMenuItem_Click);
            // 
            // insertFromBoardToolStripMenuItem
            // 
            this.insertFromBoardToolStripMenuItem.Name = "insertFromBoardToolStripMenuItem";
            this.insertFromBoardToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.insertFromBoardToolStripMenuItem.Text = "Insert From Board";
            this.insertFromBoardToolStripMenuItem.Click += new System.EventHandler(this.insertFromBoardToolStripMenuItem_Click);
            // 
            // MenuTestForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.menuStrip);
            this.MainMenuStrip = this.menuStrip;
            this.Name = "MenuTestForm";
            this.Text = "MenuTestForm";
            this.menuStrip.ResumeLayout(false);
            this.menuStrip.PerformLayout();
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
        private System.Windows.Forms.ToolStripMenuItem cutToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem copyToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem pasteToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripMenuItem editExternalAppearanceToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem insertToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem insertComponentToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem insertFromBoardToolStripMenuItem;
    }
}