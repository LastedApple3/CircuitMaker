using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CircuitMaker.GUI.Settings
{
    public partial class SettingsDialog : Form
    {
        Label[] labels;
        Control[] inputControls;

        public SettingsDialog(string name, ISettingDescription[] settingDescs)
        {
            InitializeComponent();

            Name = name;

            labels = new Label[settingDescs.Length];
            inputControls = new Control[settingDescs.Length];

            for (int i = 0; i < settingDescs.Length; i++) // look at FlowLayoutPanel
            {
                labels[i] = new Label();
                labels[i].Text = settingDescs[i].GetPrompt();
                labels[i].Location = new Point(50, 50 + (i * 50));

                inputControls[i] = settingDescs[i].GetInputControl();
                inputControls[i].Location = new Point(50, 70 + (i * 50));
            }
        }
    }

    public interface ISettingDescription
    {
        string GetPrompt();
        Control GetInputControl();
    }

    public abstract class SettingDescription<T> : ISettingDescription
    {
        protected string prompt;
        protected T defaultVal;
        protected Control inputControl;

        public string GetPrompt()
        {
            return prompt;
        }

        public SettingDescription(string prompt, T defaultValue)
        {
            this.prompt = prompt;
            defaultVal = defaultValue;
        }

        public abstract Control GetInputControl();
        public abstract T GetValue();
    }

    public abstract class TextBoxSettingDescription<T> : SettingDescription<T>
    {
        protected new TextBox inputControl;

        public TextBoxSettingDescription(string prompt, T defaultVal) : base(prompt, defaultVal) { }

        public override Control GetInputControl()
        {
            inputControl = new TextBox();

            inputControl.Text = defaultVal.ToString();
            inputControl.TextChanged += InputControl_TextChanged;

            return new TextBox();
        }

        private void InputControl_TextChanged(object sender, EventArgs e)
        {
            inputControl.Text = RestrictInput(inputControl.Text);
        }

        public abstract string RestrictInput(string current);

        protected string StripAllBut(string from, string chars)
        {
            return from.Where(chars.Contains).Select(chr => chr.ToString()).Prepend("").Aggregate((str1, str2) => str1 + str2);
        }
    }

    public class IntSettingDescription : TextBoxSettingDescription<int>
    {
        public IntSettingDescription(string prompt, int defaultVal) : base(prompt, defaultVal) { }

        public override string RestrictInput(string current)
        {
            string temp = StripAllBut(current, "-0123456789");
            bool pos = temp[0] != '-';
            temp = StripAllBut(temp, "0123456789");

            return (pos ? "-" : "") + temp;

            //return current.ToArray().Where("0123456789".Contains).Select(chr => chr.ToString()).Prepend("").Aggregate((str1, str2) => str1 + str2);
        }

        public override int GetValue()
        {
            return int.Parse(inputControl.Text);
        }
    }

    public class EnumSettingDescription<E> : SettingDescription<E> where E : Enum {
        protected new ComboBox inputControl;

        public EnumSettingDescription(string prompt) : base(prompt, default) { }

        public override Control GetInputControl()
        {
            inputControl = new ComboBox();

            inputControl.DropDownStyle = ComboBoxStyle.DropDownList;
            //inputControl.Items.AddRange(Enum.GetValues(typeof(E)).OfType<E>().Select(e => e.ToString()).ToArray());
            inputControl.Items.AddRange(Enum.GetValues(typeof(E)).OfType<E>().OfType<object>().ToArray());
            inputControl.SelectedItem = defaultVal;

            return inputControl;
        }

        public override E GetValue()
        {
            return (E)inputControl.SelectedItem;
        }
    }
}
