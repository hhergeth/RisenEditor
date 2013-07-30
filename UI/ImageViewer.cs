using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GameLibrary;
using SlimDX;
using SlimDX.Direct3D11;
using SlimDX.DXGI;
using GameLibrary.Rendering;
using RisenEditor.Code;
using RisenEditor.Code.Loader;
using RisenEditor.Code.Renderer;
using RisenEditor.Code.RisenTypes;

namespace RisenEditor.UI
{
    public partial class ImageViewer : DeviceApplication
    {
        ivRenderer renderer;
        public bool closed;
        XIMGLoader xLoader;
        API_Device D;

        public ImageViewer(Form1 a_Form, PostProcessingManager a_Manager)
            : base(a_Form.Application)
        {
            this.D = a_Form.Device;
            InitializeComponent();
            this.Show(a_Form);
            renderer = new ivRenderer(this);
            a_Manager.AddPostProcessor(renderer);
            CreateBackbuffer(false, pictureBox1);

            this.FormClosing += new FormClosingEventHandler(ImageViewer_FormClosing);
            xLoader = new XIMGLoader();
        }

        void ImageViewer_FormClosing(object sender, FormClosingEventArgs e)
        {
            closed = true;
        }

        private void ImageViewer_Resize(object sender, EventArgs e)
        {
            float pV = 1;
            if (renderer.srt != null)
                pV = (float)renderer.srt.Width / (float)renderer.srt.Height;
            if (pictureBox1.Width < pictureBox1.Height)
            {
                float nW = pV * (float)pictureBox1.Width;
                this.pictureBox1.Width = (int)nW;
            }
            else
            {
                float nH = pV * (float)pictureBox1.Height;
                this.pictureBox1.Height = (int)nH;
            }

            Device.Content.DisposeElement(BackBuffer.Texture);
            this.BackBuffer.Dispose();
            this.SwapChain.ResizeBuffers(0, Width, Height, this.BackBuffer.Format, SlimDX.DXGI.SwapChainFlags.None);
            Texture2D t = Texture2D.FromSwapChain<Texture2D>(this.SwapChain, 0);
            RenderTargetView rtv = Device.Content.CreateTargetView(t);
            Texture2DDescription de = t.Description;
            de.BindFlags = BindFlags.DepthStencil; de.Format = ManagedSettings.DepthBufferFormat; de.MipLevels = 1; de.OptionFlags = ResourceOptionFlags.None;
            Texture2D d = Device.Content.CreateTexture(de);
            DepthStencilView dsv = Device.Content.CreateDepthView(d);
            this.BackBuffer.Update(t, null, rtv, null, d, dsv);
        }

        public ShaderResourceTexture Texture
        {
            get
            {
                return renderer.srt;
            }

            set
            {
                renderer.srt = value;
                if (value != null)
                    this.propertyGrid1.SelectedObject = value.TextureDescription;
            }
        }

        private void ImageViewer_DragDrop(object sender, DragEventArgs e)
        {
            string O = (e.Data.GetData(System.Windows.Forms.DataFormats.FileDrop) as string[])[0];

            GameLibrary.IO.EFile f = FileManager.GetFile(O);
            this.Texture = new ShaderResourceTexture(xLoader.LoadTextureFromFile(f, this.Device), Device);
        }

        private void ImageViewer_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = e.AllowedEffect;
            string[] qs = e.Data.GetData(System.Windows.Forms.DataFormats.FileDrop) as string[];
            if (qs == null || qs.Length == 0)
                e.Effect = DragDropEffects.None;
            string s = qs[0];
            if (!s.Contains("_ximg", StringComparison.CurrentCultureIgnoreCase))
                e.Effect = DragDropEffects.None;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (renderer.srt == null)
                return;
            saveFileDialog1.DefaultExt = ".dds";
            DialogResult dr = saveFileDialog1.ShowDialog(this);
            if (dr == System.Windows.Forms.DialogResult.Abort || dr == System.Windows.Forms.DialogResult.Cancel || dr == System.Windows.Forms.DialogResult.No || dr == System.Windows.Forms.DialogResult.None)
                return;
            renderer.srt.ToFile(saveFileDialog1.FileName + ".dds", ImageFileFormat.Dds);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "Image Files(*.BMP;*.JPG;*.GIF)|*.BMP;*.JPG;*.GIF|All files (*.*)|*.*";
            DialogResult dr = openFileDialog1.ShowDialog();
            if (dr == System.Windows.Forms.DialogResult.Abort || dr == System.Windows.Forms.DialogResult.Cancel || dr == System.Windows.Forms.DialogResult.No || dr == System.Windows.Forms.DialogResult.None)
                return;
            string path = openFileDialog1.FileName;
            if (!System.IO.File.Exists(path))
                return;
            this.Texture = new ShaderResourceTexture(path, D);
        }   
    }

    class ivRenderer : PostProcessor
    {
        API_Device D;
        DeviceApplication DA;
        internal ShaderResourceTexture srt = null;

        internal ivRenderer(DeviceApplication da)
        {
            DA = da;
            D = da.Device;
        }

        public void DoProcess(RenderInformation RI)
        {
            DA.BackBuffer.SetTarget();
            ShaderResourceTexture s = srt;
            if (s == null)
                s = ShaderResourceTexture.WhiteTexture;
            FullScreenQuad.DrawTexture(s);
        }

        public bool Enabled
        {
            get
            {
                return true;
            }
            set { }
        }

        public API_Device Device
        {
            get
            {
                return D;
            }

            set
            {
                D = value;
            }
        }
    }
}
