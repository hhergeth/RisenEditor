using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace RisenEditor.UI
{
    public partial class NewTabForm : Form
    {
        public string TabName;
        public string TabPrefix;

        public NewTabForm()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            TabName = textBox2.Text;
            TabPrefix = textBox1.Text;
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            Close();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            button1.Enabled = textBox1.Text.Length > 0 && textBox2.Text.Length > 0;
        }
    }
}
