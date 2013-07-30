using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GameLibrary;
using GameLibrary.IO;
using RisenEditor.Code;
using RisenEditor.Code.Loader;
using RisenEditor.Code.Renderer;
using RisenEditor.Code.RisenTypes;
using System.Threading;

namespace RisenEditor.UI
{
    public partial class PakViewer : Form
    {
        Form1 DA;

        public PakViewer(Form1 P)
        {
            DA = P;
            InitializeComponent();
            this.openFileDialog1.RestoreDirectory = true;
            this.Show(P);
        }

        private void öffnenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string s = RegistryManager.getPakFolder();
            if (s == string.Empty)
                s = FileManager.g_pGamepath;
            openFileDialog1.Filter = "Pak files (*.Pak)|*.Pak|All files (*.*)|*.*";
            openFileDialog1.InitialDirectory = s;
            this.openFileDialog1.ShowDialog();
        }

        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            if (e.Cancel)
                return;
            string fname = openFileDialog1.FileName;
            GameLibrary.IO.EFile file = FileManager.GetFile(fname);
            if (!file.IsOpenable)
                return;
            appendPak(FileManager.GetRoot(file.Parent.Name + @"\" + file.Name), null);
            RegistryManager.setPakFolder(new System.IO.FileInfo(openFileDialog1.FileName).Directory.FullName);
        }

        private void appendPak(EFile a_File, TreeNode parentTN)
        {
            TreeNode v_N = null;
            if (parentTN == null)
                v_N = treeView1.Nodes.Add(a_File.Name);
            else v_N = parentTN.Nodes.Add(a_File.Name);
            v_N.Tag = a_File;
            if (a_File.IsDirectory)
                foreach (EFile f in a_File.Children)
                    appendPak(f, v_N);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            TreeNode t1 = treeView1.SelectedNode;
            if (t1 == null)
                return;
            string bpath = new System.IO.FileInfo(openFileDialog1.FileName).Directory.FullName;
            DialogResult dr = folderBrowserDialog1.ShowDialog(this);
            if (dr == System.Windows.Forms.DialogResult.Abort || dr == System.Windows.Forms.DialogResult.Cancel || dr == System.Windows.Forms.DialogResult.No)
                return;
            System.IO.Directory.CreateDirectory(bpath);
            bpath = folderBrowserDialog1.SelectedPath + @"/";
            exportObject(t1.Tag as RPakFile, bpath);
        }

        private void exportObject(RPakFile o, string path)
        {
            if (!o.IsDirectory)
            {
                System.IO.Stream S = o.Open(System.IO.FileAccess.Read);
                S.WriteToFile(path + "/" + o.Name);
                o.Close();
            }
            else
            {
                System.IO.Directory.CreateDirectory(path + "/" + o.Name);
                foreach (RPakFile p in o.Children)
                    exportObject(p, path + "/" + o.Name);
            }
        }

        private void treeView1_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            EFile _F = FileManager.GetFile(e.Node.Text);
            if (!_F.IsDirectory && _F.IsOpenable)
            {
                if (_F.Name.EndsWith("._xmac") || _F.Name.EndsWith("._xmsh"))
                {
                    GameLibrary.Objekte.GraphicNode gn = DA.Device.Content.LoadModelFromFile(_F.Name, false);
                    DA.createModelViewer(gn);
                }
                else if (_F.Name.EndsWith("._ximg"))
                {
                    ShaderResourceTexture s = new ShaderResourceTexture(_F.Name, DA.Device);
                    DA.createImageViewer(s);
                }
                else if (_F.Name.EndsWith(".lrent"))
                {

                }
                else if (_F.Name.EndsWith(".sec"))
                {

                }
            }
        }

        private void treeView1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.C && treeView1.SelectedNode != null)
            {
                //DA.BeginInvoke((ThreadStart)delegate()
                //{
                    Clipboard.SetText(treeView1.SelectedNode.Text);
                //});
            }
        }
    }
}
