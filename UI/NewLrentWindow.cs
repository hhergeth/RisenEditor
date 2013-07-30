using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using RisenEditor.Code;
using GameLibrary.IO;
using GameLibrary;

namespace RisenEditor.UI
{
    public partial class NewLrentWindow : Form
    {
        public string newLrentName = string.Empty;
        public string newSecName = string.Empty;
        public bool AddToWRL = false;

        public NewLrentWindow()
        {
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            InitializeComponent();
            string newName = "NewLrent_";
            int i = 0;
            while (FileManager.GetFile(newName + i.ToString() + ".lrent").IsOpenable) i++;
            textBox1.Text = newName + i.ToString() + ".lrent";
            comboBox1.Text = "NewSec_" + i.ToString() + ".sec";

            EFile curr = FileManager.GetRoot("common/projects");
            curr = curr.GetChild("World");
            Stack<EFile> _files = new Stack<EFile>();
            _files.Push(curr);
            Dictionary<string, EFile> _found = new Dictionary<string, EFile>();
            while (_files.Count != 0)
            {
                curr = _files.Pop();
                foreach (EFile f in curr.Children)
                {
                    if (f.IsDirectory)
                        _files.Push(f);
                    else if (f.Extension == ".sec")
                        _found.Add(f.Name, f);
                }
            }
            EFile[] FF = FileManager.GetPhysicalFiles("*.sec");
            foreach (EFile ff in FF)
                if (!_found.ContainsKey(ff.Name))
                    _found.Add(ff.Name, ff);
            foreach (KeyValuePair<string, EFile> k in _found)
                comboBox1.Items.Add(k.Key);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            newLrentName = textBox1.Text;
            if (!newLrentName.Contains(".lrent"))
                newLrentName = newLrentName + ".lrent";
            newSecName = comboBox1.Text;
            AddToWRL = checkBox1.Checked && checkBox1.Enabled;
            this.DialogResult = System.Windows.Forms.DialogResult.Yes;
            this.Close();
        }

        private void comboBox1_TextChanged(object sender, EventArgs e)
        {
            checkBox1.Enabled = !FileManager.GetFile(comboBox1.Text).IsOpenable;
            checkBox1.Checked = checkBox1.Enabled;
        }
    }
}
