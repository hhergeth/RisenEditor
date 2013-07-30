using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using GameLibrary;
using GameLibrary.Rendering;
using GameLibrary.Objekte;
using SlimDX;
using SlimDX.D3DCompiler;
using SlimDX.Direct3D11;

namespace RisenEditor.Code.Renderer
{
    public class Sorter : StdLibraryRenderer
    {
        System.Threading.Timer m_Timer;
        RenderCollection m_NodesStatic;//render these first
        RenderCollection m_NodesDynamic;//these contain eCPortalRoom children
        public Camera Camera { get; set; }
        bool m_up = false;
        object a_Main = new object(), a_Ren = new object();

        public static bool USE_PORTROOMS = false;

        public Sorter()
        {
            m_NodesStatic = new RenderCollection();
            m_NodesDynamic = new RenderCollection();
            Camera = ManagedWorld.NodeLibrary.Camera;
            DoUpdate();
            m_Timer = new Timer(OnTime, null, 0, 33);
        }

        public override void Dispose()
        {
            m_Timer.Dispose();
            base.Dispose();
        }

        private void OnTime(Object o)
        {
            DoUpdate();
        }

        //PREDICATE
        public void DoUpdate()
        {
            lock (a_Main)
            {
                if (m_up) return;
                m_up = true;
            }

            RenderCollection m_NS = new RenderCollection();
            RenderCollection m_ND = new RenderCollection();
            RenderInformation RI = Camera.CreateRenderInformation(true);
            IList<Node> N = ManagedWorld.NodeLibrary.OcTree.FindNodes(RI.BoundingFrustums[0]);

            if(USE_PORTROOMS)
                PortalRoomManager.TraverseTree(m_NS, m_ND, RI.BoundingFrustums[0]);
            foreach (Node n in N)
            {
                GraphicNode gn = n as GraphicNode;
                ILrentObject.ObjectTagger ta = n.Tag as ILrentObject.ObjectTagger;
                if (!n.Visible || gn == null || ta == null)
                    continue;
                if (USE_PORTROOMS && ta.PortalRoom != null)
                    continue;
                ILrentObject o = ta.Object;
                //if (o.Entity != null && !o.IsFreePoint && o.Entity.FadeOutRange < Vector3.Distance(RI.CameraPosition, o.Position))
                //    continue;
                
                if (o.File.IsLevelLrent)
                    m_NS.Add(gn);
                else m_ND.Add(gn);
            }
            m_NS.FinishCollecting();
            m_ND.FinishCollecting();

            lock (a_Ren)
            {
                m_NodesStatic = m_NS;
                m_NodesDynamic = m_ND;
            }
            m_up = false;
        }

        public override void RenderLibrary(RenderInformation RI, ObjektRenderer TR)
        {
            lock (a_Ren)
            {
                m_NodesStatic.Render(RI, TR);
                m_NodesDynamic.Render(RI, TR);
            }
        }

        public void SetInterval(int a_newInterval)
        {
            if (m_Timer != null)
            {
                m_Timer.Change(0, a_newInterval);
            }
        }
    }

    public class ObjRenderer : ObjektRenderer
    {
        public Sorter S;
        ShaderBytecode SB_V, SB_P;
        VertexShader VS;
        PixelShader PS;
        InputLayout IL;
        SlimDX.Direct3D11.Buffer cBuf;
        DataStream dS;
        SlimDX.Direct3D11.Buffer cBuf2;
        DataStream dS2;
        Matrix M;
        Form1 P;

        public ObjRenderer(Form1 F)
            : base(F.Device)
        {
            P = F;
            PortalRoomManager.Equals(null, null);
            S = new Sorter();
            string s0 = Environment.CurrentDirectory + "/resources/shaders/ambient_fast.fx";
            SB_V = ShaderBytecode.CompileFromFile(s0, "VS_STATIC", "vs_4_0", ManagedSettings.ShaderCompileFlags, EffectFlags.None);
            SB_P = ShaderBytecode.CompileFromFile(s0, "PS", "ps_4_0", ManagedSettings.ShaderCompileFlags, EffectFlags.None);
            VS = new VertexShader(F.Device.HadrwareDevice(), SB_V);
            PS = new PixelShader(F.Device.HadrwareDevice(), SB_P);
            IL = new InputLayout(Device.HadrwareDevice(), SB_V, StaticVertex.ies);
            BufferDescription desc = new BufferDescription
            {
                Usage = ResourceUsage.Default,
                SizeInBytes = 2 * 64,
                BindFlags = BindFlags.ConstantBuffer
            };
            cBuf = new SlimDX.Direct3D11.Buffer(Device.HadrwareDevice(), desc);
            dS = new DataStream(2 * 64, true, true);

            BufferDescription desc2 = new BufferDescription
            {
                Usage = ResourceUsage.Default,
                SizeInBytes = 64,
                BindFlags = BindFlags.ConstantBuffer
            };
            cBuf2 = new SlimDX.Direct3D11.Buffer(Device.HadrwareDevice(), desc2);
            dS2 = new DataStream(64, true, true);
        }

        public override void Dispose()
        {
           // E.Dispose();
            S.Dispose();
        }

        public override void DrawSubObjekt(RenderInformation RI, MeshPart M, GraphicNode GN)
        {
            Device.Context.VertexShader.SetConstantBuffer(cBuf, 0);
            Device.Context.PixelShader.SetConstantBuffer(cBuf2, 1); 
            if (M == null || M.Buffer == null || M.Buffer.VertexCount == 0) return;
            Device.InputAssembler.SetVertexBuffer(0, M.Buffer.Bindings[0]);
            if(M.Material.DiffuseTexture != null)
                Device.Context.PixelShader.SetShaderResource(M.Material.DiffuseTexture.ShaderResourceView, 0);
            else Device.Context.PixelShader.SetShaderResource(ShaderResourceTexture.WhiteTexture.ShaderResourceView, 0);
            Device.Context.Draw(M.Buffer.VertexCount, 0);
        }

        public void DrawList(MeshPart M, List<GraphicNode> gns)
        {

        }

        public override void PrepareForNode(RenderInformation RI, GraphicNode GN)
        {
            dS.Position = 0L;
            dS.Write<Matrix>(Matrix.Transpose(RI.WorldMatrix * M));
            dS.Write<Matrix>(Matrix.Transpose(RI.WorldMatrix));
            dS.Position = 0L;
            Device.Context.UpdateSubresource(new DataBox(0, 0, dS), cBuf, 0);

            dS2.Position = 0L;
            dS2.Write<Vector3>(RI.CameraPosition);
            dS2.Position = 0L;
            Device.Context.UpdateSubresource(new DataBox(0, 0, dS2), cBuf2, 0); 
        }

        public override void RenderAll()
        {
            RenderInformation RI = ManagedWorld.NodeLibrary.Camera.CreateRenderInformation(true);
            M = RI.ViewMatrix * RI.ProjectionMatrix;
            
            Device.InputAssembler.PrimitiveType = PrimitiveTopology.TriangleList;
            Device.InputAssembler.InputLayout = IL;
            Device.Context.PixelShader.Set(PS);
            Device.Context.VertexShader.Set(VS);
            Device.Context.GeometryShader.Set(null);

            S.Camera = ManagedWorld.NodeLibrary.Camera;
            P.BackBuffer.SetTarget(System.Drawing.Color.Black, 1.0f, ClearType.All);
            S.RenderLibrary(RI, this);
        }
    }
}
