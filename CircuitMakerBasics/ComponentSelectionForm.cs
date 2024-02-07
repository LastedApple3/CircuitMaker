using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CircuitMaker.Basics;

namespace CircuitMaker
{
    public partial class ComponentSelectionForm : Form
    {
        public ComponentSelectionForm()
        {
            InitializeComponent();

            lstComponents.Items.AddRange(ReadWriteImplementation.Constructors.Keys.Where(id => id != "BOARD").ToArray());
        }

        public IComponent GetComponent()
        {
            return ReadWriteImplementation.Constructors[(string)lstComponents.SelectedItem](ReadWriteImplementation.DefaultDetails[(string)lstComponents.SelectedItem]);
        }
    }
}
