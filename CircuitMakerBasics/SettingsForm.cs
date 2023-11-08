using System;
using System.Collections.Generic;
//using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CircuitMaker.Basics;

namespace CircuitMaker.GUI.Settings
{
    public partial class SettingsDialog : Form
    {
        Label[] labels;
        Control[] inputControls;

        TableLayoutPanel[] tbls;

        public SettingsDialog(string name, ISettingDescription[] settingDescs)
        {
            InitializeComponent();

            Name = name;

            labels = new Label[settingDescs.Length];
            inputControls = new Control[settingDescs.Length];
            tbls = new TableLayoutPanel[settingDescs.Length];

            AnchorStyles anchorStyle = AnchorStyles.Bottom | AnchorStyles.Left;

            for (int i = 0; i < settingDescs.Length; i++)
            {
                tbls[i] = new TableLayoutPanel();
                tbls[i].Name = $"tblSetting{i}";
                tbls[i].AutoSize = true;
                tbls[i].AutoSizeMode = AutoSizeMode.GrowAndShrink;
                tbls[i].ColumnCount = 1;
                tbls[i].ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
                tbls[i].RowCount = 2;
                tbls[i].RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
                tbls[i].RowStyles.Add(new RowStyle(SizeType.Percent, 50F));

                labels[i] = new Label();
                labels[i].Name = $"lblSetting{i}";
                labels[i].Text = settingDescs[i].GetPrompt();
                labels[i].Anchor = anchorStyle;

                inputControls[i] = settingDescs[i].GetInputControl();
                inputControls[i].Name = $"inpSetting{i}";

                tbls[i].Controls.Add(labels[i], 0, 0);
                tbls[i].Controls.Add(inputControls[i], 0, 1);

                flpSettings.Controls.Add(tbls[i]);
            }
        }
    }

    public interface ISettingDescription
    {
        string GetPrompt();
        Control GetInputControl();
    }

    interface ISettingsComponent : IComponent
    {
        ISettingDescription[] GetSettingDescriptions();
        void ApplySettings();
    }

    static class SettingsComponentExtensions
    {
        public static void OpenSettings<T>(this T comp) where T : ISettingsComponent
        {
            SettingsDialog settingsDialog = new SettingsDialog($"{comp.GetComponentID()} Settings", comp.GetSettingDescriptions());
            settingsDialog.ShowDialog();

            comp.ApplySettings();
        }
    }

    public abstract class SettingDescription<T> : ISettingDescription
    {
        protected string prompt;
        protected T defaultVal;
        //protected Control inputControl;

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
        protected TextBox inputControl;

        public TextBoxSettingDescription(string prompt, T defaultVal) : base(prompt, defaultVal) { }

        public override Control GetInputControl()
        {
            inputControl = new TextBox();

            inputControl.Text = defaultVal.ToString();
            inputControl.KeyPress += InputControl_KeyPress;

            return inputControl;
        }

        private void InputControl_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\b') { return; }

            if (!AllowInput(inputControl.Text, e.KeyChar, inputControl.SelectionStart))
            {
                e.Handled = true;
            }
        }

        public abstract bool AllowInput(string current, char newChar, int caretIdx);

        protected string StripAllBut(string from, string chars)
        {
            return from.Where(chars.Contains).Select(chr => chr.ToString()).Prepend("").Aggregate((str1, str2) => str1 + str2);
        }
    }

    public class SignedIntSettingDescription : TextBoxSettingDescription<int>
    {
        public SignedIntSettingDescription(string prompt, int defaultVal = 0) : base(prompt, defaultVal) { }

        public override bool AllowInput(string current, char newChar, int caretIdx)
        {
            if (newChar == '-' && caretIdx == 0 && !current.Contains('-'))
            {
                return true;
            }

            if ("0123456789".Contains(newChar))
            {
                return true;
            }

            return false;
        }

        public override int GetValue()
        {
            return int.Parse(inputControl.Text);
        }
    }

    public class PositiveIntSettingDescription : TextBoxSettingDescription<int>
    {
        public PositiveIntSettingDescription(string prompt, int defaultVal = 0) : base(prompt, defaultVal) { }

        public override bool AllowInput(string current, char newChar, int caretIdx)
        {
            if ("0123456789".Contains(newChar))
            {
                return true;
            }

            return false;
        }

        public override int GetValue()
        {
            return int.Parse(inputControl.Text);
        }
    }

    public class NameSettingDescription : TextBoxSettingDescription<string>
    {
        public NameSettingDescription(string prompt, string defaultVal) : base(prompt, defaultVal) { }

        public override bool AllowInput(string current, char newChar, int caretIdx)
        {
            return ('A' <= newChar && newChar <= 'Z') || ('a' <= newChar && newChar <= 'z');
        }

        public override string GetValue()
        {
            return inputControl.Text;
        }
    }

    public class EnumSettingDescription<E> : SettingDescription<E> where E : Enum {
        protected ComboBox inputControl;

        public EnumSettingDescription(string prompt, E defaultVal = default) : base(prompt, defaultVal) { }

        public override Control GetInputControl()
        {
            inputControl = new ComboBox();

            inputControl.DropDownStyle = ComboBoxStyle.DropDownList;
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
