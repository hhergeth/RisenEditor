using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using RisenEditor.Code.RisenTypes;

namespace RisenEditor.UI
{
    public partial class NewInfoForm : Form
    {
        public string InfoName;
        public string InfoOwner;
        public gEInfoType InfoType;
        public gEInfoCondType InfoCndType;

        public NewInfoForm()
        {
            InitializeComponent();
            foreach(object a in Enum.GetValues(typeof(gEInfoType)))
                comboBox1.Items.Add(a);
            comboBox1.SelectedItem = gEInfoType.gEInfoType_Info;
            foreach(object a in Enum.GetValues(typeof(gEInfoCondType)))
                comboBox2.Items.Add(a);
            comboBox2.SelectedItem = gEInfoCondType.gEInfoCondType_General;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            InfoName = textBox1.Text;
            InfoOwner = textBox2.Text;
            InfoType = (gEInfoType)comboBox1.SelectedItem;
            InfoCndType = (gEInfoCondType)comboBox2.SelectedItem;
            DialogResult = System.Windows.Forms.DialogResult.OK;
            Close();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            button1.Enabled = textBox1.Text.Length > 0 && textBox2.Text.Length > 0;
        }
    }
}
