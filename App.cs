using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GameLibrary;
using GameLibrary.Rendering;
using RisenEditor.Code;
using RisenEditor.Code.Renderer;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using GameLibrary.Objekte;
using GameLibrary.IO;
using SlimDX;
using SlimDX.Direct3D11;
using RisenEditor.Code.Loader;
using RisenEditor.Code.RisenTypes;

namespace RisenEditor
{
    public class App : D3DApplication
    {
        public ObjektRendererManager m_pRenderer;
        public PostProcessingManager m_pPost;
        Form1 m_pWindow;

        public App()
        {
            ManagedSettings.FarDepth *= 10;
            GameLibrary.Objekte.SpotLight.ShowNodes = false;
            GraphicNode.FASTCPY = true;
            SystemLog.Initialize();
            FeatureLevel fLevel = SlimDX.Direct3D11.Device.GetSupportedFeatureLevel(new SlimDX.DXGI.Factory().GetAdapter(0));
            if (fLevel == FeatureLevel.Level_9_1 || fLevel == FeatureLevel.Level_9_2 || fLevel == FeatureLevel.Level_9_3)
            {
                MessageBox.Show("Sorry but DirectX 9 is no longer supported.");
                SystemLog.Append(LogImportance.System, "Closed because no DirectX 10 supported.");
                throw new Exception();
            }
            base.FeatureLevel = fLevel;
            ManagedSettings.ShaderCompileFlags = SlimDX.D3DCompiler.ShaderFlags.OptimizationLevel3;
            base.Flags = DeviceCreationFlags.None;
#if (DEBUG)
            base.Flags = DeviceCreationFlags.Debug;
            ManagedSettings.ShaderCompileFlags = SlimDX.D3DCompiler.ShaderFlags.Debug | SlimDX.D3DCompiler.ShaderFlags.SkipOptimization;
#endif
        }

        public override GameLibrary.IO.EFile GetFileHandle(string a_FileName, FileType a_FileType)
        {
            return FileManager.getHandle(a_FileName, a_FileType);
        }

        public override void Initialize()
        {
            ResourceManager.InitResourceManager();
            GameLibrary.IO.IniFile inif = new GameLibrary.IO.IniFile("Resources/Settings.ini");
            Camera.ReverseYAxis = bool.Parse(inif.IniReadValue("General", "ReverseMouseYAxis"));
            string f = inif.IniReadValue("General", "RisenMainPath");
            string s = null;
            if (f != "" && f != "INVALID" && System.IO.Directory.Exists(f))
                s = f;
            FileManager.InitFileManager(this, s);
            m_pRenderer = new ObjektRendererManager();
            m_pPost = new PostProcessingManager();
            m_pWindow = new Form1(this);
            m_pPost.AddPostProcessor(new InformationDrawer(m_pWindow));
            m_pPost.AddPostProcessor(new NodeDisplayerPostProc(m_pWindow));
            m_pPost.AddPostProcessor(new GizmoComponent(m_pWindow));
            m_pPost.AddPostProcessor(new IconDrawer(m_pWindow));
            //Core.ObjektRendererManager.RegisterRenderer(new AmbientOutputRenderer(Core.Device));
            //Core.ObjektRendererManager.GetPostProcessor<AmbientOutputRenderer>().Sorter = new Sorter();
            m_pRenderer.RegisterRenderer(new ObjRenderer(m_pWindow));
            m_pRenderer.RegisterRenderer(new GrassRendrer(m_pWindow));
            m_pRenderer.RegisterRenderer(new NavRenderer(m_pWindow));
            //base.Initialize();
            m_pWindow.Show();
        }

        public override void Draw()
        {
            m_pRenderer.UseAllRenderer();
            m_pPost.UseAllPostProcessors();
        }

        public override void Free()
        {
        }
    }
}
