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
//using CircuitMaker.Components;

namespace CircuitMaker.GUI.ExtApp
{
    partial class ExtAppEditorForm : Form
    {
        public ExtAppEditorForm(IBoardContainerComponent boardContainerComp, ColourScheme colourScheme)
        {
            extAppEditor1 = new ExtAppEditor(boardContainerComp, colourScheme);

            InitializeComponent();
        }
    }
}
