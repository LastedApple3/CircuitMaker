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
            this.saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveAsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.loadToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tblWholePage = new System.Windows.Forms.TableLayoutPanel();
            this.pnlBuilder = new System.Windows.Forms.Panel();
            this.btnSimulate = new System.Windows.Forms.Button();
            this.tblSelector = new System.Windows.Forms.TableLayoutPanel();
            this.menuStrip.SuspendLayout();
            this.tblWholePage.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip
            // 
            this.menuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem});
            this.menuStrip.Location = new System.Drawing.Point(0, 0);
            this.menuStrip.Name = "menuStrip";
            this.menuStrip.Size = new System.Drawing.Size(800, 24);
            this.menuStrip.TabIndex = 1;
            this.menuStrip.Text = "menuStrip2";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.saveToolStripMenuItem,
            this.saveAsToolStripMenuItem,
            this.loadToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // saveToolStripMenuItem
            // 
            this.saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            this.saveToolStripMenuItem.Size = new System.Drawing.Size(114, 22);
            this.saveToolStripMenuItem.Text = "Save";
            // 
            // saveAsToolStripMenuItem
            // 
            this.saveAsToolStripMenuItem.Name = "saveAsToolStripMenuItem";
            this.saveAsToolStripMenuItem.Size = new System.Drawing.Size(114, 22);
            this.saveAsToolStripMenuItem.Text = "Save As";
            // 
            // loadToolStripMenuItem
            // 
            this.loadToolStripMenuItem.Name = "loadToolStripMenuItem";
            this.loadToolStripMenuItem.Size = new System.Drawing.Size(114, 22);
            this.loadToolStripMenuItem.Text = "Load";
            // 
            // tblWholePage
            // 
            this.tblWholePage.ColumnCount = 2;
            this.tblWholePage.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tblWholePage.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            this.tblWholePage.Controls.Add(this.pnlBuilder, 0, 0);
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
            // pnlBuilder
            // 
            this.tblWholePage.SetColumnSpan(this.pnlBuilder, 2);
            this.pnlBuilder.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlBuilder.Location = new System.Drawing.Point(3, 3);
            this.pnlBuilder.Name = "pnlBuilder";
            this.pnlBuilder.Size = new System.Drawing.Size(794, 370);
            this.pnlBuilder.TabIndex = 0;
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
        private System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveAsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem loadToolStripMenuItem;
        private System.Windows.Forms.TableLayoutPanel tblWholePage;
        private System.Windows.Forms.Panel pnlBuilder;
        private System.Windows.Forms.Button btnSimulate;
        private System.Windows.Forms.TableLayoutPanel tblSelector;
    }
}