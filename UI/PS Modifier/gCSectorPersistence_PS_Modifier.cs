using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using RisenEditor.Code;
using RisenEditor.Code.RisenTypes;

namespace RisenEditor.UI.PS_Modifier
{
    public partial class gCSectorPersistence_PS_Modifier : PS_Modifier
    {
        public gCSectorPersistence_PS_Modifier()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            gCSectorPersistence_PS S = P.CurrNode.getAccessor("gCSectorPersistence_PS").Query<gCSectorPersistence_PS>();
            S.Clear();
            WrlFile W = new WrlFile(FileManager.GetFile("world.wrl"));
            List<string> S2 = W.SecFiles;
            foreach (string s in S2)
                S.AddSector(s);

        }
    }
}
