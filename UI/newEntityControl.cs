using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using RisenEditor.Code;
using RisenEditor.Code.Loader;
using RisenEditor.Code.Renderer;
using RisenEditor.Code.RisenTypes;
using GameLibrary;
using GameLibrary.IO;
using SlimDX;

namespace RisenEditor.UI
{
    public partial class newEntityControl : UserControl
    {
        Form1 P;

        public static int z_maxcounter = 12;
        List<Vector3> z_points;
        LrentFile z_File;
        long lastDown;

        public newEntityControl()
        {
            z_points = new List<Vector3>();
            InitializeComponent();
            z_maxcounter = (int)numericUpDown1.Value;
        }

        public void initialize(Form1 F)
        {
            P = F;
            textBox2.Text = "DonCamp_01_L01_COL._xcom";
            textBox1.Text = "DonCamp_01_L01._xmsh";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            z_maxcounter = Math.Max(z_maxcounter, 3);
            z_File = P.CurrFile;
            if (z_File == null) return;
            P.OnBufferClick = handleMouseDown0;
            z_points.Clear();
            label1.Text = "Please click three times to specify the location of the new zone.";
            label1.Visible = true;
        }

        bool handleMouseDown0(MouseEventArgs a, Vector3 v, GameLibrary.Objekte.Node q)
        {
            BackBufferControl_MouseDown(null, a);
            //P.OnBufferClick = null;
            return false;
        }

        void BackBufferControl_MouseDown(object sender, MouseEventArgs e)
        {
            if (Environment.TickCount - lastDown < 100)
                return;
            lastDown = Environment.TickCount;
            if (z_points.Count >= z_maxcounter || e.Button != System.Windows.Forms.MouseButtons.Left) return;
            z_points.Add(P.GetNewStartPosition(ManagedWorld.NodeLibrary.Camera.Position_ABS));
            if (z_points.Count == z_maxcounter)
            {
                P.OnBufferClick = null;
                label1.Visible = false;
                Vector3 pos = Vector3.Zero;
                for (int i = 0; i < z_maxcounter; i++)
                    pos += z_points[i];
                pos /= (float)z_maxcounter;
                eCFile F = new eCFile(FileManager.GetFile("resources/emptyZone.bin"));
                eCDynamicEntity E = new eCDynamicEntity(F);
                E.GUID.Value = Guid.NewGuid();
                E.SetRotation_ST(Quaternion.Identity);
                E.SetPosition_ST(Vector3.Zero);
                bTObjArray<SlimDX.Vector3> Points = E[0].Properties["Point"].Object as bTObjArray<SlimDX.Vector3>;
                Points.Clear();
                float mR = 0.0f;
                for (int i = 0; i < z_maxcounter; i++)
                {
                    Points.Add( GENOMEMath.toGENOME(z_points[i] - pos));
                    mR = Math.Max(mR, Points[i].Length());
                }
                E[0].Properties["Radius"].Object = mR;
                E[0].Properties["RadiusOffset"].Object = new Vector3(0.0f);
                ILrentObject v_NewObj = new ILrentObject(z_File, E);
                v_NewObj.Matrix = Matrix.Translation(pos);
                v_NewObj.LoadModels(P.Device);
                v_NewObj.File.addObject(v_NewObj);
                RisenWorld.OnEntityAdded(v_NewObj);
                Code.Renderer.NavRenderer.DrawAllNav = true;
                v_NewObj.Position = v_NewObj.Position;
                v_NewObj.getSet<gIZone>().CalculateCenter();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            z_maxcounter = Math.Max(z_maxcounter, 2);
            z_File = P.CurrFile;
            if (z_File == null) return;
            P.OnBufferClick = handleMouseDown1;
            z_points.Clear();
            label1.Text = "Please click twice to specify the location of the new path.";
            label1.Visible = true;
        }

        void BackBufferControl_MouseDown2(object sender, MouseEventArgs e)
        {
            if (Environment.TickCount - lastDown < 100)
                return;
            lastDown = Environment.TickCount;
            if (z_points.Count >= z_maxcounter || e.Button != System.Windows.Forms.MouseButtons.Left) return;
            z_points.Add(P.GetNewStartPosition(ManagedWorld.NodeLibrary.Camera.Position_ABS));
            if (z_points.Count == z_maxcounter)
            {
                P.OnBufferClick = null;
                label1.Visible = false;
                Vector3 pos = Vector3.Zero;
                for (int i = 0; i < z_maxcounter; i++)
                    pos += z_points[i];
                pos /= (float)z_maxcounter;
                eCFile F = new eCFile(FileManager.GetFile("resources/emptyPath.bin"));
                eCDynamicEntity E = new eCDynamicEntity(F);
                E.GUID.Value = Guid.NewGuid();
                E.SetRotation_ST(Quaternion.Identity);
                E.SetPosition_ST(Vector3.Zero);
                bTObjArray<SlimDX.Vector3> Points = E[0].Properties["Point"].Object as bTObjArray<SlimDX.Vector3>;
                bTObjArray<float> Radii = E[0].Properties["Radius"].Object as bTObjArray<float>;
                Points.Clear();
                Radii.Clear();
                float mR = 0.0f;
                for (int i = 0; i < z_maxcounter; i++)
                {
                    Points.Add(GENOMEMath.toGENOME(z_points[i] - pos));
                    mR = Math.Max(mR, Points[i].Length());
                    Radii.Add(75.0f);
                }
                ILrentObject v_NewObj = new ILrentObject(z_File, E);
                v_NewObj.Matrix = Matrix.Translation(pos);
                v_NewObj.LoadModels(P.Device);
                v_NewObj.File.addObject(v_NewObj);
                RisenWorld.OnEntityAdded(v_NewObj);
                Code.Renderer.NavRenderer.DrawAllNav = true;
                v_NewObj.Position = v_NewObj.Position;
            }
        }

        bool handleMouseDown1(MouseEventArgs a, Vector3 v, GameLibrary.Objekte.Node q)
        {
            BackBufferControl_MouseDown2(null, a);
            //P.OnBufferClick = null;
            return false;
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            z_maxcounter = (int)(sender as NumericUpDown).Value;
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            if (sender == textBox1)
            {
                textBox2.Text = (sender as TextBox).Text.Replace("._xmsh", "") + "_COL._xcom";
            }
            //string s = (sender as TextBox).Text;
            //if (sender == textBox1)
            //    textBox1.Text = s.Substring(0, s.IndexOf(".")) + "._xmsh";
            //if (sender == textBox2)
            //    textBox2.Text = s.Substring(0, s.IndexOf(".")) + "._xcom";
            button3.Enabled = FileManager.GetFile(textBox1.Text).IsOpenable && FileManager.GetFile(textBox2.Text).IsOpenable;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            z_File = P.CurrFile;
            if (z_File == null)
                return;
            P.OnBufferClick = handleMouseDown2;
            label1.Text = "Please specify a location for the new entity.";
            label1.Visible = true;
        }

        void BackBufferControl_MouseDown3(object sender, MouseEventArgs e)
        {
            label1.Visible = false;

            Vector3 pos = P.GetNewStartPosition(ManagedWorld.NodeLibrary.Camera.Position_ABS);
            addXMSH(textBox1.Text, textBox2.Text, P.Device, pos, Quaternion.Identity, z_File);
        }

        public static ILrentObject addXMSH(string xmsh, string xcol, API_Device D, Vector3 pos, Quaternion rot, LrentFile z_File, bool a_Notify = true)
        {
            eCFile F = new eCFile(FileManager.GetFile("resources/emptyLevelEntity.bin"));
            eCDynamicEntity E = new eCDynamicEntity(F);
            E.GUID.Value = Guid.NewGuid();

            EFile e0 = FileManager.GetFile(xmsh);

            //string x = FileManager.mapFilename(xmsh), x2 = FileManager.mapFilename(xcol);
            string x = "#G3:/" + xmsh, x2 = "#G3:/" + xcol;
            E.Name.pString = e0.Name.Replace(e0.Extension, "");//xmsh.Replace("._xmsh", "") //@"#G3:/data/raw/meshes/World//" +
            (E["eCMesh_PS"].Properties["MeshFileName"].Object as bCString).pString = x;
            E.Query<eCCollisionShape_PS>()[0].SetXColMesh(x2);//DonCamp_01_L01_COL._xcom
            E.Query<eCCollisionShape_PS>()[1].SetXColMesh(x2);

            ILrentObject O = new ILrentObject(z_File, E);
            O.LoadModels(D);

            E.ApplyBoundingVolume(GENOMEMath.toGENOME(O.Nodes[0].BoundingBox));

            O.File.UpdateContextBox();
            O.File.addObject(O);
            O.Position = pos;
            O.Rotation = rot;

            if(a_Notify)
                RisenWorld.OnEntityAdded(O);

            return O;
        }

        bool handleMouseDown2(MouseEventArgs a, Vector3 v, GameLibrary.Objekte.Node n)
        {
            BackBufferControl_MouseDown3(null, a);
            P.OnBufferClick = null;
            return false;
        }
    }
}
