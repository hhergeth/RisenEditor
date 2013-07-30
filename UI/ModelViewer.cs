using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GameLibrary;
using GameLibrary.Objekte;
using SlimDX;
using SlimDX.Direct3D11;
using System.IO;
using SlimDX.DXGI;
using GameLibrary.Rendering;

namespace RisenEditor.UI
{    
    public partial class ModelViewer : DeviceApplication
    {
        public GraphicNode GN;
        public Camera cam;
        public bool DrawAxis = true, DrawBBs = false;
        List<TreeNode> Mains = new List<TreeNode>();
        TSTRENDERER tst;
        Form1 DAParent;

        public ModelViewer(Form1 P)
            : base(P.Application)
        {
            this.DAParent = P;
            InitializeComponent();
            this.Show(P);
            P.FormClosing += new FormClosingEventHandler(P_FormClosing);
            tst = new TSTRENDERER(this);
            this.CreateBackbuffer(false, pictureBox1);
        }

        void P_FormClosing(object sender, FormClosingEventArgs e)
        {
            tst.Enabled = false;
            if (cam != null)
                cam.Dispose();
        }

        protected override void DestroyHandle()
        {
            DAParent.mv = null;
            if (GN != null)
                GN.Dispose();
            if (cam != null)
            cam.Dispose();
            base.Dispose(false);
        }

        public GraphicNode GNode
        {
            get
            {
                return GN;
            }

            set
            {
                if (value == null)
                    return;
                if (GN != null)
                {
                    GN.Visible = false;
                    GN.Dispose();
                }
                GN = value;
                if (cam == null)
                    cam = new _3d_Person_Cam(new Node(), 3, MathHelper.PiOver2, this);
                listView1.Items.Clear();
                //System.Windows.Forms.ImageList il = new ImageList();
                //this.listView1.SmallImageList = il;
                //this.listView1.LargeImageList = il;
                foreach (MeshPart mp in GN.Mesh.Parts)
                {
                    string s = string.Empty;
                    ShaderResourceTexture S = ShaderResourceTexture.WhiteTexture;
                    if (mp.Material.DiffuseTexture != null)
                    {
                        s = "Diffuse Texture";
                        S = mp.Material.DiffuseTexture;
                    }
                    if (mp.Material.NormalTexture != null)
                    {
                        s = "Normal Texture";
                        S = mp.Material.NormalTexture;
                    }
                    //Stream stream = new MemoryStream();
                    //SlimDX.Direct3D11.Texture2D.ToStream(S.Device.Context, S.Texture, ImageFileFormat.Dds, stream);
                    //Bitmap q = new Bitmap(stream);
                    //int index = il.Images.Count;
                    //il.Images.Add(q.GetThumbnailImage(100, 100, null, IntPtr.Zero), Color.Transparent);
                    ListViewItem lvi = new ListViewItem(s);
                    lvi.Tag = S;
                    listView1.Items.Add(lvi);
                }
            }
        }

        private void BTN_CLICK_LOAD(object sender, EventArgs e)
        {
            openFileDialog1.Reset();
            DialogResult dr = openFileDialog1.ShowDialog();
            if ((dr == System.Windows.Forms.DialogResult.Cancel) || (dr == System.Windows.Forms.DialogResult.No))
                return;
            if (GNode != null)
            {
                cam.Dispose();
                GNode.Dispose();
            }
            GNode = DAParent.Device.Content.LoadModelFromFile(openFileDialog1.FileName, true);
            GNode.Initialize(null, new Vector3(0), new Vector3(1), true);
            GNode.Visible = false;
            GNode.Rotate(RotationAngle.X_Axis, -90);
            //GNode.Rotate(RotationAngle.Z_Axis, 90);
            cam = new _3d_Person_Cam(GN, 3, MathHelper.PiOver2, this);
        }

        private Vector3 GetNewStartPosition()
        {
            Vector3 ta = ManagedWorld.NodeLibrary.Camera.TargetPosition_ABS - ManagedWorld.NodeLibrary.Camera.Position_ABS;
            ta.Normalize();
            ta = ManagedWorld.NodeLibrary.Camera.Position_ABS + ta * 100;
            return ta;
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            this.pictureBox1.Focus();
        }

        private void listView1_DoubleClick(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 0)
                return;
            ListViewItem q = listView1.SelectedItems[0];
            if (!(q.Tag is ShaderResourceTexture))
                return;
            DAParent.createImageViewer(q.Tag as ShaderResourceTexture);
        }
    }

    public class TSTRENDERER : ObjektRenderer
    {
        EffectWrapper E;
        ModelViewer MV;

        public TSTRENDERER(ModelViewer MV)
            : base(MV.Device)
        {
            this.CallFrequency = STDCallFrequencys.Always;
            this.MV = MV;
            D = MV.Device;
            E = new EffectWrapper("ambient.fx", MV.Device);
        }

        public override void RenderAll()
        {
            MV.BackBuffer.SetTarget(Color.CornflowerBlue, 1.0f, ClearType.DepthTarget | ClearType.RenderTarget);
            if (MV.GN != null && MV.cam != null)
            {
                RenderInformation RI = MV.cam.CreateRenderInformation(true);

                E.Variables["WVP"].Variable.AsMatrix().SetMatrix(MV.GN.ModelMatrix_ABS * RI.ViewMatrix * RI.ProjectionMatrix);
                if (MV.GN is AnimatedGraphicNode)
                    E.Variables["Bones"].SetVariable((MV.GN as AnimatedGraphicNode).BoneMatrices);
                foreach (MeshPart m in MV.GN.Mesh.Parts)
                {
                    E.Variables["DiffuseTexture"].SetVariable(m.Material.DiffuseTexture.ShaderResourceView);
                    if (MV.GN is AnimatedGraphicNode)
                        m.Buffer.DrawBufferIndexed(E.Techniques["Normal"].Passes["Animated"]);
                    else m.Buffer.DrawBufferIndexed(E.Techniques["Normal"].Passes["Static"]);
                }
            }
        }

        public override void DrawSubObjekt(RenderInformation RI, MeshPart GSO, GraphicNode GN)
        {

        }

        public override void PrepareForNode(RenderInformation RI, GraphicNode N)
        {

        }

        public override void Dispose()
        {
            
        }
    }
    
}
