using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using GameLibrary;
using GameLibrary.Objekte;
using GameLibrary.IO;
using SlimDX;
using SlimDX.Direct3D11;
using RisenEditor.Code;
using RisenEditor.Code.Loader;
using RisenEditor.Code.Renderer;
using RisenEditor.Code.RisenTypes;

//http://www.file-upload.net/delete-3633117/lct4qe.html
//http://www.file-upload.net/delete-3794497/jsqktd.html
namespace RisenEditor
{
    public partial class Form1 : DeviceApplication
    {
        UserConsole m_Console;
        ILrentObject cN;
        long lastSave;
        App m_pApp;
        KeyManager m_pKeyManager;
        
        //Ui functions for PS's & loading & check if after ps modification refresh is needed, new model loading
        //tple files
        //rotate gizmo

        //I for inventory and other keys
        //prj files

        public unsafe Form1(App a_App)
            : base(a_App)
        {
            m_pApp = a_App;
            InitializeComponent();
            Icon = RisenEditor.Properties.Resources.RisenFont;
            m_pKeyManager = new KeyManager();
            base.CreateBackbuffer(false, pictureBox1);
            SystemLog.Append(LogImportance.System, "First settings were loaded.");

            PortalRoomManager.T();

            Camera c = new Camera(null, new Vector3(0), new Vector3(0, 0, 100), MathHelper.PiOver2, this);
            ManagedWorld.NodeLibrary.Camera = c;
            SystemLog.Append(LogImportance.System, "Second settings were loaded and rendering was started.");
            
            sceneControl1.Init(this);
            entityControl1.Init(this);
            
            m_Console = new UserConsole(this, a_App.m_pPost);
            
            ManagedWorld.NodeLibrary.OcTree.Build();
            ManagedWorld.NodeLibrary.Camera.PanSpeed = RisenWorld.PanSpeed;
        }

        public ILrentObject CurrNode
        {
            get
            {
                return cN;
            }

            set
            {
                cN = value;
                if (ObjectSelected != null)
                    this.ObjectSelected(CurrNode, null);
                lastSelection = Environment.TickCount;
                batchEntitiesToolStripMenuItem.Enabled = value != null && value.getSet<gCSkills_PS_Wrapper>() != null;
            }
        }

        public LrentFile CurrFile
        {
            get
            {
                return sceneControl1.getFile() ?? (CurrNode != null ? CurrNode.File : null);
            }
        }

        public eECoordinateSystem CoordSystem { get; private set; }

        private void pictureBox1_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.KeyCode == Keys.OemPipe)
            {
                m_Console.Open = !m_Console.Open;
                return;
            }
            if (m_Console.Open)
            {
                m_Console.ProcessInputKey(e.KeyCode, e.KeyData, e.Alt, e.Shift, e.Control);
                return;
            }

            if (e.KeyCode == Keys.F7)
            {
                CurrNode.SetWrappers.Add(new eCPortalRoom_PS_Wrapper(CurrNode));
            }

            if (e.KeyCode == Keys.F3)
            {
                base.Application.PauseRendering();
                if (Device.Rasterizer.FillMode == FillMode.Solid)
                    Device.Rasterizer.FillMode = FillMode.Wireframe;
                else Device.Rasterizer.FillMode = FillMode.Solid;
                base.Application.ResumeRendering();
            }
            else if (e.Control && e.KeyCode == Keys.S && sceneControl1.getEditableFile() != null)
            {
                if (Environment.TickCount - lastSave < 200)
                    return;
                GameLibrary.SystemLog.Append(LogImportance.Information, DateTime.Now.ToString() + " : " + "Quick Save");
                sceneControl1.getEditableFile().SaveFile();
                lastSave = Environment.TickCount;
                return;
            }

            m_pKeyManager.Process(e, this);

            if (CurrNode == null)
                return;

            if (e.KeyCode == Keys.Delete)
                BTN_CLICK_Del(null, null);

            if (e.Control)
            {
                Vector2 V = Vector2.Zero;
                float f = MathHelper.Pi / 10;
                if (e.KeyCode == Keys.NumPad8)
                    V = new Vector2(0, f);
                if (e.KeyCode == Keys.NumPad2)
                    V = new Vector2(0, -f);
                if (e.KeyCode == Keys.NumPad6)
                    V = new Vector2(1, f);
                if (e.KeyCode == Keys.NumPad4)
                    V = new Vector2(1, -f);
                if (e.KeyCode == Keys.NumPad9)
                    V = new Vector2(2, f);
                if (e.KeyCode == Keys.NumPad7)
                    V = new Vector2(2, -f);
                if (V.Y != 0)
                {
                    Quaternion Q = Quaternion.Identity;
                    Vector3 r = new Vector3();
                    r[(int)V.X] = 1.0f;
                    Q = Quaternion.RotationAxis(r, (float)V.Y);
                    CurrNode.Rotate(Q, CoordSystem);
                }
            }
            else
            {
                float f = 15;
                Vector3 V = Vector3.Zero;
                if (e.KeyCode == Keys.NumPad8)
                    V.Z -= f;
                if (e.KeyCode == Keys.NumPad2)
                    V.Z += f;
                if (e.KeyCode == Keys.NumPad6)
                    V.X += f;
                if (e.KeyCode == Keys.NumPad4)
                    V.X -= f;
                if (e.KeyCode == Keys.NumPad9)
                    V.Y += f;
                if (e.KeyCode == Keys.NumPad7)
                    V.Y -= f;
                if (V != Vector3.Zero)
                    CurrNode.Move(V, CoordSystem);
            }

            if (e.Control && ((e.KeyCode & Keys.C) == Keys.C))
            {
                BTN_CLICK_Copy(null, null);
                return;
            }
            if (e.Control && ((e.KeyCode & Keys.V) == Keys.V))
            {
                BTN_CLICK_Paste(null, null);
                return;
            }
            if (e.Control && ((e.KeyCode & Keys.G) == Keys.G) && CurrNode != null)
            {
                ILrentObject.MoveCameraTo(CurrNode);
                return;
            }
        }
    }
}
