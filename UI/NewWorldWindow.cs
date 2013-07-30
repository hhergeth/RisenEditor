using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using RisenEditor.Code;
using RisenEditor.Code.RisenTypes;

namespace RisenEditor.UI
{
    public partial class NewWorldWindow : Form
    {
        Form1 P;

        public NewWorldWindow()
        {
            InitializeComponent();
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
        }

        public void Init(Form1 F)
        {
            P = F;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            LrentFile L = LrentFile.CreateNew(textBox1.Text + ".lrent", P.Device);
            SecFile S = SecFile.CreateNew(textBox1.Text + ".sec");
            WrlFile W = WrlFile.CreateNew("World.wrl");
           
            LrentFile _main = new LrentFile(FileManager.GetFile("SysDyn_{C3960F11-2A7F-42BA-B9F6-5D8BFA392E82}.lrent"), P.Device);

            W.AddSec("SysDyn_{C3960F11-2A7F-42BA-B9F6-5D8BFA392E82}.sec");
            W.AddSec("_Intern");

            _main.SaveFile();
            
            W.AddSec(S);
            S.AddLrent(L);

            L.SaveFile();
            S.SaveFile();
            W.SaveFile();

            this.P.loadSecs(FileManager.GetFiles(W.SecFiles.ToArray()));

            this.DialogResult = System.Windows.Forms.DialogResult.Yes;

            this.Close();
        }
    }
}
