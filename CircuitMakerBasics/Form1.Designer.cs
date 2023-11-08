namespace CircuitMaker
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
            this.components = new System.ComponentModel.Container();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.item1ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.item2ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.item3compoundToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.item31ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.item32ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.item4ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.label1 = new System.Windows.Forms.Label();
            this.contextMenuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.item1ToolStripMenuItem,
            this.item2ToolStripMenuItem,
            this.item3compoundToolStripMenuItem,
            this.item4ToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(181, 114);
            // 
            // item1ToolStripMenuItem
            // 
            this.item1ToolStripMenuItem.Name = "item1ToolStripMenuItem";
            this.item1ToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.item1ToolStripMenuItem.Text = "item1";
            this.item1ToolStripMenuItem.Click += new System.EventHandler(this.item1ToolStripMenuItem_Click);
            // 
            // item2ToolStripMenuItem
            // 
            this.item2ToolStripMenuItem.Name = "item2ToolStripMenuItem";
            this.item2ToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.item2ToolStripMenuItem.Text = "item2";
            // 
            // item3compoundToolStripMenuItem
            // 
            this.item3compoundToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.item31ToolStripMenuItem,
            this.item32ToolStripMenuItem});
            this.item3compoundToolStripMenuItem.Name = "item3compoundToolStripMenuItem";
            this.item3compoundToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.item3compoundToolStripMenuItem.Text = "item3 (compound)";
            // 
            // item31ToolStripMenuItem
            // 
            this.item31ToolStripMenuItem.Name = "item31ToolStripMenuItem";
            this.item31ToolStripMenuItem.Size = new System.Drawing.Size(113, 22);
            this.item31ToolStripMenuItem.Text = "item3.1";
            // 
            // item32ToolStripMenuItem
            // 
            this.item32ToolStripMenuItem.Name = "item32ToolStripMenuItem";
            this.item32ToolStripMenuItem.Size = new System.Drawing.Size(113, 22);
            this.item32ToolStripMenuItem.Text = "item3.2";
            // 
            // item4ToolStripMenuItem
            // 
            this.item4ToolStripMenuItem.Name = "item4ToolStripMenuItem";
            this.item4ToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.item4ToolStripMenuItem.Text = "item4";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(412, 169);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(35, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "label1";
            this.label1.Click += new System.EventHandler(this.label1_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.label1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.contextMenuStrip1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem item1ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem item2ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem item3compoundToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem item31ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem item32ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem item4ToolStripMenuItem;
        private System.Windows.Forms.Label label1;
    }
}