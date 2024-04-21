using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CircuitMaker.Basics;
using CircuitMaker.Components;
using CircuitMaker.GUI;
using CircuitMaker.GUI.Settings;
using CircuitMaker.GUI.ExtApp;
using System.IO;
using System.Runtime.CompilerServices;
using System.Data.SqlClient;
using System.Security.Cryptography.X509Certificates;
using System.Xml.Linq;

namespace CircuitMaker
{
internal class Program
{
    [STAThread]
    static void Main(string[] args)
    {
        ComponentRegisterer.RegisterComponents();

        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new GUIForm());
    }
}

}