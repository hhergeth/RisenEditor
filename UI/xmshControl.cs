using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using RisenEditor.Code;
using GameLibrary.IO;
using GameLibrary;

namespace RisenEditor.UI
{
    public partial class xmshControl : UserControl
    {
        Form1 P;
        string m_pRoot = null;
        const string vK = "RK_XMSHCNTRL";
        const string vK2 = "RK_XMSHCNTRLROOT";

        public xmshControl()
        {
            InitializeComponent();

            string a = RegistryManager.Index[vK2];
            if (!string.IsNullOrEmpty(a))
            {
                m_pRoot = a;
                label1.Text = a;
            }

            m_pRoot = @"C:\Users\Hannes\Documents\Visual Studio 2010\Projects\WOW_Solution\WOW_App\bin\Release\Exported";
        }

        public void Initialize(Form1 P)
        {
            FileManager.NewFilesFound += new Action<List<GameLibrary.IO.EFile>>(FileManager_NewFilesFound);
            this.P = P;
            update();
        }

        void FileManager_NewFilesFound(List<GameLibrary.IO.EFile> obj)
        {
            update();
        }

        void update()
        {
            return;
            EFile[] F = FileManager.GetPhysicalFiles("*._xmsh");
            Action a = () =>
            {
                treeView1.Nodes.Clear();
                foreach (EFile e in F)
                {
                    EFile e0 = FileManager.GetFile(e.Path.Replace(e.Extension, "._xcom")), e1 = FileManager.GetFile(e.Path.Replace(e.Extension, "") + "_COL._xcom");
                    if (e0.IsOpenable || e1.IsOpenable)
                    {
                        TreeNode n = treeView1.Nodes.Add(e.Name);
                        n.Tag = new EFile[] { e, e0.IsOpenable ? e0 : e1 };
                    }
                }
            };
            if (this.IsHandleCreated)
                this.BeginInvoke(a);
            else a();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog A = new OpenFileDialog();
            string a = RegistryManager.Index[vK];
            if (!string.IsNullOrEmpty(a))
                A.FileName = a;
            DialogResult dr = A.ShowDialog();
            if (dr == DialogResult.OK)
            {
                RegistryManager.Index[vK] = A.FileName;
                LevelMeshConverter.Convert(P.Device, A.FileName, new SlimDX.Vector3((float)numericUpDown1.Value), null, m_pRoot);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog A = new FolderBrowserDialog();
            string a = RegistryManager.Index[vK2];
            if (!string.IsNullOrEmpty(a))
                A.SelectedPath = a;
            DialogResult dr = A.ShowDialog();
            if (dr == DialogResult.OK)
            {
                m_pRoot = RegistryManager.Index[vK2] = A.SelectedPath;
                label1.Text = A.SelectedPath;
            }
        }

        private void treeView1_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e2)
        {
            P.OnBufferClick = (e, p, n) =>
            {
                EFile[] n2 = e2.Node.Tag as EFile[];
                newEntityControl.addXMSH(n2[0].Path, n2[1].Path, P.Device, p, SlimDX.Quaternion.Identity, P.CurrFile);
                if((Control.ModifierKeys & Keys.Control) != Keys.Control)
                    P.OnBufferClick = null;
                return true;
            };
        }

        private void treeView1_MouseUp(object sender, MouseEventArgs e)
        {
            if(e.Button == MouseButtons.Right)
            {
                // Select the clicked node
                treeView1.SelectedNode = treeView1.GetNodeAt(e.X, e.Y);

                if(treeView1.SelectedNode != null)
                {
                    deleteToolStripMenuItem.Tag = treeView1.SelectedNode.Tag;
                    contextMenuStrip1.Show(treeView1, e.Location);
                }
            }
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            bool b = FileManager.NotifyOnPhysicalWatch;
            FileManager.NotifyOnPhysicalWatch = false;
            EFile[] n2 = deleteToolStripMenuItem.Tag as EFile[];
            foreach (EFile e2 in n2)
                System.IO.File.Delete(e2.Path);
            System.Threading.Thread.Sleep(100);
            FileManager.NotifyOnPhysicalWatch = b;
            update();
        }

        void convert(object F)
        {
            const float v_Scale = 65;
            P.startLoading();
            ManagedWorld.NodeLibrary.AddToOcTree = false;
            bool b = FileManager.NotifyOnPhysicalWatch;
            FileManager.NotifyOnPhysicalWatch = false;
            System.IO.FileInfo f0 = new System.IO.FileInfo((string)F);
            SlimDX.Design.Vector3Converter VC = new SlimDX.Design.Vector3Converter();
            SlimDX.Design.QuaternionConverter QC = new SlimDX.Design.QuaternionConverter();
            string[] T = System.IO.File.ReadAllLines((string)F, Encoding.ASCII);
            int l = 0;
            List<ILrentObject> objs = new List<ILrentObject>();
            while (l < T.Length)
            {
                if (T[l++].StartsWith("<Model>"))
                {
                    string n = T[l++];
                    SlimDX.Vector3 t = (SlimDX.Vector3)VC.ConvertFrom(T[l++]), t2 = new SlimDX.Vector3(-t.Z, t.Y, t.X) * v_Scale;
                    SlimDX.Quaternion o = (SlimDX.Quaternion)QC.ConvertFrom(T[l++]), o2 = new SlimDX.Quaternion(-o.Z, o.Y, o.X, o.W);
                    SlimDX.Vector3 s = (SlimDX.Vector3)VC.ConvertFrom(T[l++]);
                    string p = T[l++], x = p.ToLower().Replace(".obj", "._xmsh"), c = p.ToLower().Replace(".obj", "_COL._xcom"), pf = System.IO.Path.Combine(f0.DirectoryName, p);
                    LevelMeshConverter.Convert(P.Device, pf, v_Scale * s, null, null);
                    ILrentObject O = newEntityControl.addXMSH(x, c, P.Device, t2, o, P.CurrFile, false);
                    objs.Add(O);
                    if (l % 5 == 0)
                        P.setPercentage((int)(((float)l / (float)T.Length) * 100.0f));
                }
            }
            FileManager.NotifyOnPhysicalWatch = b;
            update();
            ManagedWorld.NodeLibrary.AddToOcTree = true;
            ManagedWorld.NodeLibrary.OcTree.Build();
            P.endLoading();
            RisenWorld.OnLoadingFinished();
            RisenWorld.OnEntityAdded(objs.ToArray());       
            
        }

        private void button3_Click(object sender, EventArgs e)
        {
            OpenFileDialog F = new OpenFileDialog();
            F.InitialDirectory = m_pRoot;
            if (F.ShowDialog() == DialogResult.OK)
                System.Threading.ThreadPool.QueueUserWorkItem(convert, F.FileName);
        }
    }
}
