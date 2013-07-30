using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GameLibrary;
using GameLibrary.IO;
using RisenEditor.Code;
using RisenEditor.Code.Loader;
using RisenEditor.Code.Renderer;
using RisenEditor.Code.RisenTypes;

namespace RisenEditor.UI
{
    public partial class SceneControl : UserControl
    {
        const string regKey = "LastLrentSelected";
        Form1 P;
        string lastFile;
        Point lastDown;
        TreeNode _PhyNode;

        Bitmap getBit(Color c)
        {
            Bitmap B = new Bitmap(1, 1);
            B.SetPixel(0, 0, c);
            return B;
        }

        public SceneControl()
        {
            InitializeComponent();
            treeView1.ImageList = new ImageList();
            treeView1.ImageList.Images.Add(RisenEditor.Properties.Resources.lrentIcon);
            treeView1.ImageList.Images.Add(RisenEditor.Properties.Resources.secIcon);
            treeView1.ImageList.Images.Add(RisenEditor.Properties.Resources.wrlIcon);
            treeView1.ImageList.Images.Add(getBit(Color.Yellow));
            treeView1.ImageList.Images.Add(getBit(Color.Red));
            treeView1.SelectedImageIndex = 4;
            _PhyNode = treeView1.Nodes.Add("PHYSICAL_FILES");
            _PhyNode.ImageIndex = _ImgIndex(_PhyNode);
        }

        void addPhyiscalFIles(List<EFile> a_Files)
        {
            Action a = () =>
            {
                foreach (EFile f in a_Files)
                {//getNode(f.Name) == null && 
                    if ((f.Extension == ".lrent" || f.Extension == ".sec" || f.Extension == ".wrl"))
                    {
                        TreeNode n = _PhyNode.Nodes.Add(f.Name);
                        n.ImageIndex = _ImgIndex(n);
                    }
                }
            };
            if (this.IsHandleCreated)
                BeginInvoke(a);
            else a();
        }

        public void Init(Form1 F)
        {
            addPhyiscalFIles(FileManager.GetPhysicalFiles("").ToList());
            FileManager.NewFilesFound += addPhyiscalFIles;
            P = F;
            this.newEntityControl1.initialize(F);
            this.sceneViewControl1.Init(F);
            this.xmshControl1.Initialize(F);
            RisenWorld.LrentFileAdded += new FilesAdded(RisenWorld_OnFileAdded);
            EFile root = FileManager.GetRoot("common/projects");
            EFile w = root.GetChild("World");
            foreach (EFile e in w.Children)
                recAdd(e, null);

            string s = RegistryManager.Index[regKey];
            if (s != string.Empty)
                treeView1.SelectedNode = getNode(s); 
        }

        TreeNode getNode(string s)
        {
            Stack<TreeNode> v_Nodes = new Stack<TreeNode>();
            foreach (TreeNode t in treeView1.Nodes)
            {
                if (t.Text == s)
                    return t;
                if (t.Nodes.Count > 0)
                    v_Nodes.Push(t);
            }
            while (v_Nodes.Count != 0)
            {
                TreeNode t2 = v_Nodes.Pop();
                foreach (TreeNode t in t2.Nodes)
                {
                    if (t.Nodes.Count > 0)
                        v_Nodes.Push(t);
                    if (t.Text == s)
                        return t;
                }
            }
            return null;
        }

        public LrentFile getFile()
        {
            return sceneViewControl1.GetLrentFile();
        }

        public LrentFile getEditableFile()
        {
            return sceneViewControl1.GetEditable();
        }

        void RisenWorld_OnFileAdded(List<LrentFile> newFiles)
        {
            foreach (LrentFile f in newFiles)
            {
                TreeNode t = getNode(f.Name);
                if(t != null)
                    t.BackColor = Color.Beige;
            }
        }

        private void treeView1_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if(e.Node.Text.Contains(".lrent") || e.Node.Text.Contains(".wrl") || e.Node.Text.Contains(".sec"))
                addNodes(e.Node);
        }

        private void treeView1_KeyDown(object sender, KeyEventArgs e)
        {
            TreeNode n = treeView1.SelectedNode;
            if (n == null)
                return;
            if (e.KeyCode == Keys.Enter)
                addNodes(n);
            else if (e.KeyCode == Keys.C && e.Control)
                Clipboard.SetText(n.Text);
        }

        void recAdd(EFile e, TreeNode n)
        {
            if (e.Extension == ".wrldatasc")
                return;
            TreeNode N = null;
            if (n == null)
            {
                N = new TreeNode(e.Name);
                treeView1.Nodes.Add(N);
            }
            else N = n.Nodes.Add(e.Name);
            N.ImageIndex = _ImgIndex(N);
            if (e.IsDirectory)
                foreach (EFile c in e.Children)
                    recAdd(c, N);
        }

        void addNodes(TreeNode n2)
        {
            if (n2.Text.ToLower().Contains(".wrl"))
            {
                WrlFile F = new WrlFile(FileManager.GetFile(n2.Text));
                List<EFile> _f = new List<EFile>();
                foreach (string s in F.SecFiles)
                    _f.Add(FileManager.GetFile(s));
                P.loadSecs(_f.ToArray());
            }
            else if (n2.Text.ToLower().Contains(".sec"))
            {
                P.loadSecs(FileManager.GetFile(n2.Text));
            }
            else if (!n2.Text.ToLower().Contains(".lrent"))
            {
                List<EFile> _files = new List<EFile>();
                foreach (TreeNode n in n2.Nodes)
                {
                    if (!n.Text.Contains(".lrent"))
                        continue;
                    EFile f = FileManager.GetFile(n.Text);
                    _files.Add(f);
                }
                if (_files.Count > 0)
                    P.loadLrents(_files.ToArray());
            }
            else
            {
                EFile e2 = FileManager.GetFile(n2.Text);
                P.loadLrents(e2);
            }
        }

        int _ImgIndex(TreeNode n)
        {
            if (n.Text.EndsWith("lrent"))
                return 0;
            else if (n.Text.EndsWith("sec"))
                return 1;
            else if (n.Text.EndsWith("wrl"))
                return 2;
            return 3;
        }

        private void treeView1_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button != System.Windows.Forms.MouseButtons.Right)
                return;
            TreeNode t = treeView1.GetNodeAt(e.X, e.Y);
            lastDown = e.Location;
            if (t.Text.Contains(".sec"))
            {
                lastFile = t.Text;
                contextMenuStrip1.Show(treeView1, e.Location);
            }
            else if (t.Text.Contains(".wrl"))
            {
                lastFile = t.Text;
                contextMenuStrip2.Show(treeView1, e.Location);
            }
        }

        private void toolStripTextBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && lastFile != null)
            {
                if (lastFile.Contains(".sec"))
                {
                    SecFile S = new SecFile(FileManager.GetFile(lastFile));
                    string s = toolStripTextBox1.Text;
                    if (!s.EndsWith(".lrent"))
                        s = s + ".lrent";
                    LrentFile L = null;
                    if (FileManager.GetFile(s).IsOpenable)
                        L = new LrentFile(FileManager.GetFile(s), P.Device);
                    else L = LrentFile.CreateNew(s, P.Device);
                    S.AddLrent(L);
                    S.SaveFile();
                    contextMenuStrip3.Close();
                }
                else if (lastFile.Contains(".wrl"))
                {
                    WrlFile W = new WrlFile(FileManager.GetFile(lastFile));
                    string s = toolStripTextBox1.Text;
                    if (!s.EndsWith(".sec"))
                        s = s + ".sec";
                    SecFile S = null;
                    if (FileManager.GetFile(s).IsOpenable)
                        S = new SecFile(FileManager.GetFile(s));
                    else S = SecFile.CreateNew(s);
                    W.AddSec(S);
                    W.SaveFile();
                    contextMenuStrip3.Close();
                }
            }
        }

        private void addFile(object sender, EventArgs e)
        {
            contextMenuStrip3.Show(treeView1, lastDown);
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            RegistryManager.Index[regKey] = e.Node.Text;
        }
    }
}
