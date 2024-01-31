using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CircuitMaker
{
    public partial class RenameBoardForm : Form
    {
        private string BoardName;

        public RenameBoardForm(string name)
        {
            BoardName = name;
            InitializeComponent();
        }

        private void RenameBoardForm_Load(object sender, EventArgs e)
        {
            txtName.Text = BoardName;
        }

        public string NewBoardName()
        {
            return txtName.Text;
        }
    }
}
