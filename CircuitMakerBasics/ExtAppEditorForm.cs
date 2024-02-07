using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CircuitMaker.Basics;

namespace CircuitMaker.GUI.ExtApp
{
    partial class ExtAppEditorForm : Form
    {
        public ExtAppEditorForm(IBoardContainerComponent boardContainerComp, ColourScheme colourScheme)
        {
            extAppEditor = new ExtAppEditor(boardContainerComp, colourScheme);

            DoubleBuffered = true;

            InitializeComponent();
        }

        public void SaveChanges()
        {
            extAppEditor.SaveChanges();
        }

        public void ResetChanges()
        {
            extAppEditor.ResetChanges();
        }
    }
}
