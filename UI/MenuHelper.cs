using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;
using GameLibrary;
using GameLibrary.Objekte;
using GameLibrary.IO;
using SlimDX;
using RisenEditor.UI;
using System.Threading;
using System.Drawing;
using RisenEditor.Code;
using RisenEditor.Code.Loader;
using RisenEditor.Code.Renderer;
using RisenEditor.Code.RisenTypes;

namespace RisenEditor
{
    public partial class Form1
    {
        internal Form iv;
        internal ModelViewer mv;
        internal PakViewer pv;
        internal ImageViewer iw;
        internal TabViewer tf;
        internal int lastSelection = Environment.TickCount;
        internal bool m_Loading = false;
        private ILrentObject lastCopy = null;
        private Point lastDown = new Point(-1, -1);
        internal Func<MouseEventArgs, Vector3, Node, bool> OnBufferClick;

        public event EventHandler ObjectSelected;

        private void xInfoViewerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (iv != null)
            {
                iv.Close();
                iv.Dispose();
            }
            iv = new Form();
            System.Windows.Forms.Integration.ElementHost H = new System.Windows.Forms.Integration.ElementHost();
            H.Dock = DockStyle.Fill;
            H.Child = new xInfoElement();
            iv.Controls.Add(H);
            iv.Show(this);
        }

        private void BT_CLICK_NEW(object sender, EventArgs e)
        {
            DialogResult dr = MessageBox.Show("Would you really like to destroy the current world?", "Clear World", MessageBoxButtons.YesNo);
            if (dr == System.Windows.Forms.DialogResult.No)
                return;
            else if (dr == System.Windows.Forms.DialogResult.Yes)
            {
                m_pApp.PauseRendering();
                RisenWorld.Clear(this);
                m_pApp.ResumeRendering();
            }
            CurrNode = null;
        }

        //open file and open folder are DEPRECATED
        private void BT_CLICK_OPEN(object sender, EventArgs e)
        {
            if (m_Loading)
                return;
            openFileDialog1.InitialDirectory = RegistryManager.getLrentFolder();
            openFileDialog1.Multiselect = true;
            openFileDialog1.Title = "Select new world file.";
            openFileDialog1.CheckPathExists = true;
            openFileDialog1.CheckFileExists = true;
            openFileDialog1.AddExtension = true;
            openFileDialog1.Filter = "Lrent Files (*.lrent*)|*.lrent*|xml Files (*.xml)|*.xml|Sector Files (*.sec*)|*.sec|All Files|*.*";
            openFileDialog1.FilterIndex = 1;
            DialogResult er = openFileDialog1.ShowDialog(this);
            if ((er == System.Windows.Forms.DialogResult.Cancel) || (er == System.Windows.Forms.DialogResult.No))
                return;
            string pa = openFileDialog1.FileName;
            RegistryManager.setLrentFolder(new System.IO.FileInfo(openFileDialog1.FileName).Directory.FullName);
            if (pa.Contains(".xml"))
            {
                DialogResult dr = MessageBox.Show("Would you really like to open a new file, the current world will be lost!", "Open file", MessageBoxButtons.YesNo);
                if (dr == System.Windows.Forms.DialogResult.No)
                    return;
                startLoading();
                XML_FileHelper.LoadNodeLibrary(pa, this);
                endLoading();
            }
            else if (pa.Contains(".sec"))
                loadSecs(FileManager.GetFiles(openFileDialog1.FileNames));
            else loadLrents(FileManager.GetFiles(openFileDialog1.FileNames));
        }

        private void BTN_CLICK_OFD(object sender, EventArgs e)
        {
            if (m_Loading)
                return;
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.Description = "Select folders to load.";
            fbd.RootFolder = Environment.SpecialFolder.MyComputer;
            string s = @"C:\";
            if (RegistryManager.getFolderSelectorDirectory() != "")
                s = RegistryManager.getFolderSelectorDirectory();
            if (s.StartsWith(@"C:\Users"))
                fbd.RootFolder = Environment.SpecialFolder.Personal;
            fbd.SelectedPath = s;
            DialogResult er = fbd.ShowDialog();
            if ((er == System.Windows.Forms.DialogResult.Cancel) || (er == System.Windows.Forms.DialogResult.No))
                return;
            string pa = fbd.SelectedPath;
            List<EFile> folders = new List<EFile>();
            recAdd(pa, folders);
            if (folders.Count > 0)
            {
                loadLrents(folders.ToArray());   
            }
            RegistryManager.setFolderSelectorDirectory(fbd.SelectedPath);
        }

        private void BTN_CLICK_SVE(object sender, EventArgs e)
        {
            FileManager.SaveTabFiles();
        }

        private void BTN_CLICK_Save(object sender, EventArgs e)
        {
            RisenWorld.Save();
        }

        private void BTN_CLICK_EXT(object sender, EventArgs e)
        {
            Environment.Exit(Environment.ExitCode);
        }

        private void BTN_CLICK_MV(object sender, EventArgs e)
        {
            if (mv != null)
            {
                mv.Close();
                mv.Dispose();
            }
            mv = new ModelViewer(this);
        }

        private void BTN_CLICK_PV(object sender, EventArgs e)
        {
            pv = new PakViewer(this);
        }

        private void BTN_CLICK_IW(object sender, EventArgs e)
        {
            if (iw != null)
            {
                iw.Close();
                iw.Dispose();
            }
            iw = new ImageViewer(this, m_pApp.m_pPost);
        }

        private void BTN_CLICK_Copy(object sender, EventArgs e)
        {
            if (CurrNode == null || !BackBufferControl.Focused || CurrNode is RisenNavStick)
                return;
            //Clipboard.SetText(CurrNode.ToString());
            lastCopy = CurrNode;
        }

        private void BTN_CLICK_Paste(object sender, EventArgs e)
        {
            if (!BackBufferControl.Focused || lastCopy == null || CurrNode is RisenNavStick)
                return;
            ILrentObject ro = lastCopy;
            Vector3 x = GetNewStartPosition(CurrNode != null ? CurrNode.Position : ro.Position);      
            ILrentObject rot = ro.Clone(GetCopyFile(ro.File), Device) as ILrentObject;
            rot.Position = x;
        }

        private void BTN_CLICK_Del(object sender, EventArgs e)
        {
            if (CurrNode != null)
            {
                CurrNode.Delete();
                CurrNode = null;
            }
        }

        private void BTN_CLICK_TAB(object sender, EventArgs e)
        {
            if (tf != null)
            {
                tf.Close();
                tf.Dispose();
            }
            tf = new TabViewer();
            tf.Show(this);
        }

        private void toolStripComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            ToolStripComboBox b = sender as ToolStripComboBox;
            this.CoordSystem = (eECoordinateSystem)b.SelectedIndex;
        }

        public void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            lastDown = new Point(-1, -1);
            pictureBox1.Focus();
            if (e.Button != MouseButtons.Left)
                return;
            Vector3 pos;
            Node q = ManagedWorld.PickObject(ManagedWorld.NodeLibrary.Camera, e.X, e.Y, Application, BackBuffer.DepthTexture, out pos);
            if (OnBufferClick != null)
                if (!OnBufferClick(e, pos, q))
                    return;
            m_pApp.m_pPost.GetPostProcessor<GizmoComponent>().enabledDragging = true;
            m_pApp.m_pPost.GetPostProcessor<GizmoComponent>().BackBufferControl_MouseMove(sender, e);
            if (GizmoComponent.dragging)
                return;
            if (q == null || q.Tag == null || !(q.Tag is ILrentObject.ObjectTagger))
            {
                if((ModifierKeys & Keys.Control) == 0)
                    CurrNode = null;
                lastDown = e.Location;
                return;
            }
            if ((ModifierKeys & Keys.Control) != Keys.Control)
            {//single selection
                ILrentObject a = (q.Tag as ILrentObject.ObjectTagger).Object;
                if (a is RisenNavStick)
                {
                    if ((a as RisenNavStick).P == CurrNode)
                        CurrNode = a;
                }
                else CurrNode = a;
            }
            else
            {//multi selection
                ILrentObject Q = (q.Tag as ILrentObject.ObjectTagger).Object;
                if (!(CurrNode is LrentObjectCollection && (CurrNode as LrentObjectCollection).Objects.Contains(Q)))
                {
                    LrentObjectCollection qa = LrentObjectCollection.CreateOrFuse(CurrNode);
                    qa.Objects.Add(Q);
                    CurrNode = qa;
                }
                else if (CurrNode is LrentObjectCollection)
                    (CurrNode as LrentObjectCollection).Objects.Remove(Q);
            }
            m_pApp.m_pPost.GetPostProcessor<GizmoComponent>().enabledDragging = Environment.TickCount - lastSelection > 100;
        }

        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            if (lastDown == new Point(-1, -1) || e.Button != System.Windows.Forms.MouseButtons.Left)
                return;
            Vector2 lU1 = new Vector2(lastDown.X, lastDown.Y);
            Vector2 rD1 = new Vector2(e.X, e.Y);
            Vector2 lU = Vector2.Minimize(lU1, rD1);
            Vector2 rD = Vector2.Maximize(lU1, rD1);
            if ((lU - rD).Length() < 3)
                return;
            
            Vector3[] P = new Vector3[8];
            Camera v_C = ManagedWorld.NodeLibrary.Camera;
            for (int i = 0; i < 2; i++)
            {
                P[i * 4 + 0] = v_C.Unproject(lU.X, lU.Y, i == 0 ? ManagedSettings.NearDepth : ManagedSettings.FarDepth, BackBuffer);
                P[i * 4 + 1] = v_C.Unproject(rD.X, lU.Y, i == 0 ? ManagedSettings.NearDepth : ManagedSettings.FarDepth, BackBuffer);
                P[i * 4 + 2] = v_C.Unproject(rD.X, rD.Y, i == 0 ? ManagedSettings.NearDepth : ManagedSettings.FarDepth, BackBuffer);
                P[i * 4 + 3] = v_C.Unproject(lU.X, rD.Y, i == 0 ? ManagedSettings.NearDepth : ManagedSettings.FarDepth, BackBuffer);
            }
            Plane[] P2 = new Plane[4];
            Vector3[] P3 = new Vector3[] { Vector3.Lerp(P[0], P[3], 0.5f), Vector3.Lerp(P[1], P[2], 0.5f), Vector3.Lerp(P[0], P[1], 0.5f), Vector3.Lerp(P[3], P[2], 0.5f)};
            P2[0] = new Plane(P[7], P[3], P[0]);
            P2[1] = new Plane(P[1], P[2], P[6]);
            P2[2] = new Plane(P[0], P[1], P[5]);
            P2[3] = new Plane(P[6], P[2], P[3]);
            BoundingFrustum BF = new BoundingFrustum(P2);//not great
            IList<Node> N = ManagedWorld.NodeLibrary.OcTree.FindNodes(v_C.CreateRenderInformation(true).BoundingFrustums[0]);
            List<ILrentObject> O = new List<ILrentObject>(N.Count / 2);
            for (int i = 0; i < N.Count; i++)
            {
                if (N[i].Tag == null || !(N[i].Tag is ILrentObject.ObjectTagger) || O.Contains((N[i].Tag as ILrentObject.ObjectTagger).Object))
                    continue;
                if(BF.Contains(N[i].BoundingBox_ABS) != ContainmentType.Disjoint)
                O.Add((N[i].Tag as ILrentObject.ObjectTagger).Object);
            }
            if(O.Count > 0)
                CurrNode = new LrentObjectCollection(O.ToArray());
            /*
            Bitmap BB = new Bitmap(BackBuffer.Width, BackBuffer.Height);
            Graphics G = Graphics.FromImage(BB);
            G.Clear(Color.White);
            Pen PQQ = new Pen(new SolidBrush(Color.Red));
            G.DrawRectangle(PQQ, lU.X, lU.Y, rD.X - lU.X, rD.Y - lU.Y);
            G.DrawLine(new Pen(new SolidBrush(Color.Green)), v_C.Project(P3[0], BackBuffer), v_C.Project(P3[0] + P2[0].Normal * 10, BackBuffer));
            G.DrawLine(new Pen(new SolidBrush(Color.Blue)), v_C.Project(P3[1], BackBuffer), v_C.Project(P3[1] + P2[1].Normal * 10, BackBuffer));
            G.DrawLine(new Pen(new SolidBrush(Color.Black)), v_C.Project(P3[2], BackBuffer), v_C.Project(P3[2] + P2[2].Normal * 10, BackBuffer));
            G.DrawLine(new Pen(new SolidBrush(Color.Yellow)), v_C.Project(P3[3], BackBuffer), v_C.Project(P3[3] + P2[3].Normal * 10, BackBuffer));
            G.Flush();
            BB.Save("1.jpg");*/
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            NavRenderer.DrawAllNav = !NavRenderer.DrawAllNav;
            if (NavRenderer.DrawAllNav)
                toolStripButton3.Image = RisenEditor.Properties.Resources.shown_11;
            else toolStripButton3.Image = RisenEditor.Properties.Resources.shown_01;
        }

        private void BTN_CLICK_NL(object sender, EventArgs e)
        {
            NewLrentWindow l = new NewLrentWindow();
            DialogResult dR = l.ShowDialog(this);
            if (dR == System.Windows.Forms.DialogResult.Yes)
            {
                LrentFile LN = LrentFile.CreateNew(l.newLrentName, Device);
                LN.SaveFile();
                RisenWorld.AddLrents(new List<LrentFile>() { LN });
                if (!FileManager.GetFile(l.newSecName).IsOpenable)
                {
                    SecFile SC = SecFile.CreateNew(l.newSecName);
                    SC.AddLrent(LN);
                    SC.SaveFile();
                    if (l.AddToWRL)
                    {
                        WrlFile _W = new WrlFile(FileManager.GetFile("world.wrl"));
                        _W.AddSec(l.newSecName);
                    }
                }
                else
                {
                    SecFile SC = new SecFile(FileManager.GetFile(l.newSecName));
                    SC.AddLrent(LN);
                    SC.SaveFile();
                }
            }
        }

        private void BTN_CLICK_NW(object sender, EventArgs e)
        {
            NewWorldWindow F = new NewWorldWindow();
            F.Init(this);
            DialogResult dR = F.ShowDialog(this);
            if (dR == System.Windows.Forms.DialogResult.Yes)
            {

            }
        }

        private void batchEntitiesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (LrentFile f in RisenWorld.Files)
                    foreach (ILrentObject f2 in f)
                        if (f2.Name == CurrNode.Name && f2.getSet<gCSkills_PS_Wrapper>() != null)
                            f2.getSet<gCSkills_PS_Wrapper>().CopyFrom(CurrNode.getSet<gCSkills_PS_Wrapper>());
        }

        #region Helper
        private void recAdd(string s, List<EFile> S)
        {
            DirectoryInfo[] dis = new DirectoryInfo(s).GetDirectories();
            FileInfo[] fis = new DirectoryInfo(s).GetFiles("*.lrent");
            foreach (DirectoryInfo q1 in dis)
                recAdd(q1.FullName, S);
            foreach (FileInfo q2 in fis)
                S.Add(FileManager.GetFile(q2.FullName));
        }

        internal void createModelViewer(GraphicNode gn)
        {
            if (mv != null)
            {
                mv.GNode = gn;
                if (mv.WindowState == FormWindowState.Minimized)
                    mv.WindowState = FormWindowState.Normal;
            }
            else
            {
                mv = new ModelViewer(this);
                mv.GNode = gn;
            }
        }

        internal void createImageViewer(ShaderResourceTexture srt)
        {
            if ((iw != null) && (!iw.closed))
            {
                iw.Texture = srt;
                if (iw.WindowState == FormWindowState.Minimized)
                    iw.WindowState = FormWindowState.Normal;
            }
            else
            {
                iw = new ImageViewer(this, m_pApp.m_pPost);
                iw.Texture = srt;
            }
        }

        internal void startLoading()
        {
            if (IsHandleCreated)
                this.BeginInvoke((ThreadStart)delegate()
                {
                    m_Loading = true;
                    toolStripProgressBar1.Visible = true;
                    toolStripStatusLabel1.Visible = true;
                });
            else
            {
                m_Loading = true;
                toolStripProgressBar1.Visible = true;
                toolStripStatusLabel1.Visible = true;
            }
        }

        internal void endLoading()
        {
            if (IsHandleCreated)
                this.BeginInvoke((ThreadStart)delegate()
                {
                    m_Loading = false;
                    toolStripProgressBar1.Visible = false;
                    toolStripStatusLabel1.Visible = false;
                });
            else
            {
                m_Loading = false;
                toolStripProgressBar1.Visible = false;
                toolStripStatusLabel1.Visible = false;
            }
        }

        internal void setPercentage(int i)
        {
            if (IsHandleCreated)
                this.BeginInvoke((ThreadStart)delegate()
                {
                    toolStripProgressBar1.Value = i;
                });
            else
            {
                toolStripProgressBar1.Value = i;
            }
        }

        internal void loadLrents(params EFile[] f)
        {
            List<EFile> _ValidFiles = new List<EFile>();
            foreach (EFile _f in f)
            {
                bool b = true;
                foreach(LrentFile F in RisenWorld.Files)
                    if (F.Name == _f.Name)
                    {
                        b = false;
                        break;
                    }
                if (b)
                    _ValidFiles.Add(_f);
            }

            if (m_Loading || _ValidFiles.Count == 0) return;
            LrentImporter imp = new LrentImporter();
            startLoading();
            imp.Read(this, _ValidFiles.ToArray());
        }

        internal void loadSecs(params EFile[] f)
        {
            List<EFile> _Files = new List<EFile>();
            foreach(EFile f2 in f)
                secReader.Read(f2, this, _Files);
            loadLrents(_Files.ToArray());
        }

        private string v3Tos(Vector3 v)
        {
            string x = v.X.ToString().Replace(",", ".");
            string y = v.Y.ToString().Replace(",", ".");
            string z = v.Z.ToString().Replace(",", ".");
            return x + " " + y + " " + z;
        }

        internal Vector3 GetNewStartPosition(Vector3 cPos)
        {
            Vector3 ta = ManagedWorld.NodeLibrary.Camera.TargetPosition_ABS - ManagedWorld.NodeLibrary.Camera.Position_ABS;
            ta.Normalize();
            ta = ManagedWorld.NodeLibrary.Camera.Position_ABS + ta * 100;
            if (BackBufferControl.ClientRectangle.Contains(Cursor.Position))
            {
                Point v_P = BackBufferControl.PointToClient(Cursor.Position);
                Vector3 x2 = ManagedWorld.GetWorldPosByXY(v_P.X, v_P.Y, ManagedWorld.NodeLibrary.Camera, BackBuffer.DepthTexture, Application);
                float f1 = Vector3.Distance(cPos, ManagedWorld.NodeLibrary.Camera.Position_ABS);
                if (x2.X == -1.0f && x2.Y == -1.0f && x2.Z == -1.0f)
                {
                    Vector3 q0 = ManagedWorld.NodeLibrary.Camera.Unproject(Cursor.Position.X, Cursor.Position.Y, ManagedSettings.FarDepth, BackBuffer);
                    Vector3 d = Vector3.Normalize(q0 - ManagedWorld.NodeLibrary.Camera.Position_ABS);
                    ta = ManagedWorld.NodeLibrary.Camera.Position_ABS + d * f1;
                }
                else ta = x2;
            }
            return ta;
        }

        internal LrentFile GetCopyFile(LrentFile Original)
        {
            LrentFile f = (sceneControl1.getFile() != null) ? sceneControl1.getFile() : null;
            if (f != null)
                return f;
            else return Original;
        }
        #endregion

        #region KeyHelper
        internal void UI_Open_Inventory()
        {
            entityControl1.ActivateModifier<UI.PS_Modifier.gCInventory_PS_Modifier>();
        }
        internal void SEL_All()
        {
            List<ILrentObject> O = new List<ILrentObject>();
            SystemLog.Append(LogImportance.Information, "Select all does not work yet.");
        }
        #endregion
    }
}
