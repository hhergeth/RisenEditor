using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using GameLibrary;
using RisenEditor.Code;
using RisenEditor.Code.Loader;
using RisenEditor.Code.Renderer;
using RisenEditor.Code.RisenTypes;

namespace RisenEditor.UI
{
    public partial class subSceneControl : UserControl
    {
        filterControl fC;
        PoperContainer popup;
        Form1 PF;
        List<TreeNode> objs;
        Dictionary<LrentFile, TreeNode> groups;
        TreeNode lSelection = null;
        LrentFile editableFile = null;

        public subSceneControl()
        {
            InitializeComponent();
            fC = new filterControl();
            popup = new PoperContainer(fC);
            fC.UpdateEvent += new EventHandler(fC_UpdateEvent);
            RisenWorld.WorldCleared += new WorldCleared(RisenWorld_OnClear);
            RisenWorld.LrentFileAdded += new FilesAdded(RisenWorld_OnFileAdded);
            RisenWorld.EntityAdded += new EntityAdded(RisenWorld_EntityAdded);
            RisenWorld.EntityDeleted += new EntityDeleted(RisenWorld_EntityDeleted);
            objs = new List<TreeNode>();
            groups = new Dictionary<LrentFile, TreeNode>();
        }

        void RisenWorld_EntityDeleted(ILrentObject a_Entity)
        {
            DeleteObject(a_Entity);
        }
        void RisenWorld_EntityAdded(ICollection<ILrentObject> a_Entity)
        {
            Action A = () =>
            {
                treeView1.BeginUpdate();
                foreach (ILrentObject e in a_Entity)
                    AddObject(e);
                treeView1.EndUpdate();
            };
            if (IsHandleCreated)
                BeginInvoke(A);
            else A();
        }
        public void Init(Form1 F)
        {
            PF = F;
            F.ObjectSelected += new EventHandler(F_ObjectSelected);
        }
        void AddObject(ILrentObject e)
        {
            TreeNode t = groups[e.File].Nodes.Add(e.Name);
            t.Tag = e;
            if(updateNode(e))
                objs.Add(t);
        }
        void DeleteObject(ILrentObject e)
        {
            TreeNode t = null;
            for (int i = 0; i < objs.Count; i++)
                if (objs[i].Tag == e)
                {
                    t = objs[i];
                    objs.RemoveAt(i);
                }
            if (lSelection != null && lSelection.Tag == e)
                lSelection = null;
            t.Remove();
        }

        void F_ObjectSelected(object sender, EventArgs e)
        {
            if (lSelection != null)
                lSelection.BackColor = Color.FromName("Window");
            for(int i = 0; i < treeView1.Nodes.Count; i++)
                if (treeView1.Nodes[i].Tag as ILrentObject == PF.CurrNode)
                {
                    lSelection = treeView1.Nodes[i];
                    lSelection.BackColor = Color.Red;
                    break;
                }
        }

        void RisenWorld_OnFileAdded(List<LrentFile> f)
        {
            if (IsHandleCreated)
                this.Invoke(new appendNodesDelegate(aNodes), f);
            else aNodes(f);
        }
        void RisenWorld_OnClear()
        {
            if (IsHandleCreated)
                treeView1.BeginInvoke((ThreadStart)delegate()
                {
                    this.treeView1.Nodes.Clear();
                    objs.Clear();
                    groups.Clear();
                });
            else
            {
                this.treeView1.Nodes.Clear();
                objs.Clear();
                groups.Clear();
            }
        }
        private void aNodes(List<LrentFile> o)
        {
            treeView1.BeginUpdate();
            foreach (LrentFile l in o)
            {
                TreeNode n = treeView1.Nodes.Add(l.Name);
                n.Tag = l;
                groups.Add(l, n);
                foreach (ILrentObject e in l)
                {
                    AddObject(e);
                }
            }
            treeView1.EndUpdate();
        }
        delegate void appendNodesDelegate(List<LrentFile> o);

        void fC_UpdateEvent(object sender, EventArgs e)
        {
            List<bool> B = new List<bool>();
            foreach (KeyValuePair<LrentFile, TreeNode> k in groups)
                B.Add(k.Value.IsExpanded);
            treeView1.Nodes.Clear();
            foreach (KeyValuePair<LrentFile, TreeNode> k in groups)
            {
                k.Value.Nodes.Clear();
                treeView1.Nodes.Add(k.Value);
            }
            foreach (TreeNode i in objs)
                if (i.Tag is ILrentObject && updateNode(i.Tag as ILrentObject))
                    groups[(i.Tag as ILrentObject).File].Nodes.Add(i);
            int i2 = 0;
            foreach (KeyValuePair<LrentFile, TreeNode> k in groups)
            {
                if (B[i2])
                    k.Value.Expand();
                i2++;
            }
            if (groups.Count > 0)
                groups.ElementAt(0).Value.Checked = true;
        }

        bool updateNode(ILrentObject o)
        {
            if (fC.pSFilter != null && o.getAccessor(fC.pSFilter) == null)
                return false;
            if (fC.filterNamePart != null && !o.Name.Contains(fC.filterNamePart))
                return false;
            return true;
        }

        private void listView1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                TreeNode t = treeView1.GetNodeAt(e.X, e.Y);
                if (t != null && t.Tag is LrentFile)
                {
                    treeView1.SelectedNode = t;
                    if(t.Tag == editableFile)
                        contextMenuStrip2.Show(treeView1, e.Location);//unmark
                    else contextMenuStrip1.Show(treeView1, e.Location);//mark
                }
                else
                {
                    popup.Show(this);
                }
            }
        }

        private void treeView1_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Node.Tag as ILrentObject == null)
                return;
            ILrentObject o = e.Node.Tag as ILrentObject;
            PF.CurrNode = o;
            ILrentObject.MoveCameraTo(o);
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if ((e.Node.Tag as ILrentObject) == null)
                return;
            PF.CurrNode = (e.Node.Tag as ILrentObject);
        }

        public LrentFile GetLrentFile()
        {
            if (editableFile != null)
                return editableFile;
            Func<LrentFile> F = () =>
            {
                if (treeView1.SelectedNode == null)
                    return null;
                if (treeView1.SelectedNode.Tag is ILrentObject)
                    return (treeView1.SelectedNode.Tag as ILrentObject).File;
                if (treeView1.SelectedNode.Tag is LrentFile)
                    return treeView1.SelectedNode.Tag as LrentFile;
                return null;
            };
            if (Handle != IntPtr.Zero)
                return (LrentFile)Invoke(F);
            else return F();
        }

        private void treeView1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                if (treeView1.SelectedNode != null && treeView1.SelectedNode.Tag is ILrentObject)
                {
                    (treeView1.SelectedNode.Tag as ILrentObject).Delete();
                }
            }
        }

        private void markAsEditableToolStripMenuItem_Click(object sender, EventArgs e)
        {
            editableFile = treeView1.SelectedNode.Tag as LrentFile;
        }

        public LrentFile GetEditable()
        {
            return editableFile;
        }

        private void saveFile(object sender, EventArgs e)
        {
            (treeView1.SelectedNode.Tag as LrentFile).SaveFile();
        }

        private void unmarkToolStripMenuItem_Click(object sender, EventArgs e)
        {
            editableFile = null;
        }
    }
}
