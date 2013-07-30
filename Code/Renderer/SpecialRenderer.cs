using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GameLibrary;
using SlimDX;
using System.Drawing;
using GameLibrary.Rendering;
using GameLibrary.Objekte;
using SlimDX.Direct3D11;

namespace RisenEditor.Code.Renderer
{
    class GrassRendrer : GameLibrary.Rendering.ObjektRenderer
    {
        public static string Properties = "Grass";
        public static float MaxMeshDistance = 0.5f;
        public static float MaxBillboardDistance = 1;

        float farMesh, farBillboard;
        EffectWrapper E3;
        BlendState bsd0;
        DynamicMeshBuffer<StaticVertex> mBuffer;
        Form1 P;

        public GrassRendrer(Form1 F)
            : base(F.Device)
        {
            this.P = F;
            this.CallFrequency = STDCallFrequencys.Always;
            E3 = new EffectWrapper("Grass_Mesh.fx", D);
            BlendStateDescription bsd = new BlendStateDescription()
            {
                AlphaToCoverageEnable = true,
                IndependentBlendEnable = false,
            };
            RenderTargetBlendDescription rtbd = new RenderTargetBlendDescription()
            {
                BlendEnable = true,
                BlendOperation = BlendOperation.Add,
                BlendOperationAlpha = BlendOperation.Add,
                RenderTargetWriteMask = ColorWriteMaskFlags.All,
                DestinationBlend = BlendOption.InverseSourceAlpha,
                SourceBlend = BlendOption.SourceAlpha,
                SourceBlendAlpha = BlendOption.Zero,
                DestinationBlendAlpha = BlendOption.One
            };
            bsd.RenderTargets[0] = rtbd;
            bsd0 = BlendState.FromDescription(D.HadrwareDevice(), bsd);

            StaticVertex[] vs = new StaticVertex[100];
            mBuffer = new DynamicMeshBuffer<StaticVertex>(D, PrimitiveTopology.PointList);
            mBuffer.Write(vs);
        }

        public override void DrawSubObjekt(RenderInformation RI, MeshPart GSO, GraphicNode GN)
        {

        }

        public override void PrepareForNode(RenderInformation RI, GraphicNode N)
        {

        }

        public override void RenderAll()
        {
            if (ManagedWorld.NodeLibrary.Camera == null)
                return;
            
            farBillboard = ManagedSettings.FarDepth * MaxBillboardDistance; farBillboard = Math.Min(farBillboard, 10000);
            farMesh = ManagedSettings.FarDepth * MaxMeshDistance; farMesh = Math.Min(farMesh, 10000);
            RenderInformation RI = ManagedWorld.NodeLibrary.Camera.CreateRenderInformation(true);

            BlendState b0 = D.OutputMerger.BlendState;
            D.OutputMerger.BlendState = bsd0;
            P.BackBuffer.SetTarget();
            CameraNodeImplementor c = RI.CameraNode;
            BoundingFrustum bf = new BoundingFrustum(RI.ViewMatrix * Matrix.PerspectiveFovRH(c.FieldOfView, c.AspectRatio, 10, farBillboard));
            IList<Node> ns = VegetationManager.fetchNodes(bf);
            E3.Variables["VP"].SetVariable(RI.ViewMatrix * RI.ProjectionMatrix);
            E3.Variables["setAlpha"].SetVariable(false);
            foreach (Node n in ns)
            {
                if (!(n is VegetationMiniPartCollection))
                    continue;
                VegetationMiniPartCollection q = n as VegetationMiniPartCollection;
                if (q.p[q.p.Length - 1] == '2')
                    D.OutputMerger.BlendState = b0;
                else D.OutputMerger.BlendState = bsd0;
                E3.Variables["DiffuseTexture"].SetVariable(q.gps[0].mBody.Parts[0].Material.DiffuseTexture.ShaderResourceView);
                q.dInstanced(E3.Techniques[0].Passes[0]);

            }
            D.OutputMerger.BlendState = b0;
        }

        public override void Dispose()
        {
            mBuffer.Dispose();
        }
    }

    class NavRenderer : ObjektRenderer
    {
        private static bool b = false;
        public static bool DrawAllNav
        {
            get
            {
                return b;
            }
            set
            {
                if (value != b)
                    foreach (RisenNavStick s in sticks)
                        foreach (GraphicNode g in s.Nodes)
                            g.Visible = value;
                b = value;
            }
        }
        public static List<RisenNavStick> sticks = new List<RisenNavStick>();

        Form1 P;
        ThreadedLibraryRenderer tSorter;
        EffectWrapper E;
        MeshBuffer mBuffer;
        Vector4[] V = new Vector4[] { new Color4(Color.CornflowerBlue).ToVector4()};
        INavBase lastObj;

        public NavRenderer(Form1 F)
            : base(F.Device)
        {
            P = F;
            P.ObjectSelected += new EventHandler(P_ObjectSelected);
            RisenWorld.WorldCleared += new WorldCleared(RisenWorld_OnClear);
            RisenWorld.LrentFileAdded += new FilesAdded(RisenWorld_OnFileAdded);
            RisenWorld.LoadingFinished += new LoadingFinished(RisenWorld_LoadingFinished);
            RisenWorld.EntityAdded += new EntityAdded(RisenWorld_EntityAdded);

            tSorter = new ThreadedLibraryRenderer();
            tSorter.Predicate = ComparePredicate;
            E = new EffectWrapper("gizmo.fx", D);
            GizmoComponent.colorVertex[] V = new GizmoComponent.colorVertex[100];
            for(int i = 0; i < V.Length; i++)
                V[i] = new GizmoComponent.colorVertex(i,0);
            mBuffer = new MeshBuffer(V, V[0], F.Device, PrimitiveTopology.TriangleList);     
        }

        void RisenWorld_EntityAdded(ICollection<ILrentObject> a_Entitys)
        {
            foreach (var a_Entity in a_Entitys)
            {
                if (a_Entity.getSet<INavBase>() != null)
                {
                    List<RisenNavStick> L = a_Entity.getSet<INavBase>().getSticks();
                    foreach (RisenNavStick n in L)
                        foreach (GraphicNode gn in n.Nodes)
                            gn.Visible = DrawAllNav;
                    sticks.AddRange(L);
                }
            }
        }

        void RisenWorld_LoadingFinished()
        {
            foreach (RisenNavStick s in sticks)
                foreach (GraphicNode g in s.Nodes)
                    g.Visible = DrawAllNav;         
        }

        void RisenWorld_OnFileAdded(List<LrentFile> F)
        {
            foreach (LrentFile f in F)
                foreach (ILrentObject o in f)
                    if (o.getSet<INavBase>() != null)
                        sticks.AddRange(o.getSet<INavBase>().getSticks());
        }

        void RisenWorld_OnClear()
        {
            sticks.Clear();
        }

        void P_ObjectSelected(object sender, EventArgs e)
        {
            if (lastObj != null)
            {
                lastObj.setVisibilty(false);
                lastObj = null;
            }
            if (DrawAllNav || P.CurrNode == null) return;
            ILrentObject o = P.CurrNode;
            if (o is RisenNavStick)
                o = (o as RisenNavStick).P;
            if (o.getSet<gIColl>() != null)
            {
                lastObj = o.getSet<gIColl>();
                lastObj.setVisibilty(true);
            }
            if (o != null && o.Nodes.Count > 0 && ComparePredicate(o.Nodes[0]))
            {
                lastObj = o.getSet<INavBase>();
                lastObj.setVisibilty(true);
            }
        }

        bool ComparePredicate(GraphicNode GN)
        {
            if (!(GN.Tag is ILrentObject.ObjectTagger)) return false;
            INavBase b = (GN.Tag as ILrentObject.ObjectTagger).Object.getSet<INavBase>();
            return b != null && !(b is gIColl);
        }

        public override void Dispose()
        {
            tSorter.Dispose();
            mBuffer.Dispose();
            E.Dispose();
        }

        public override void DrawSubObjekt(RenderInformation RI, MeshPart M2, GraphicNode GN)
        {
            ILrentObject O = (GN.Tag as ILrentObject.ObjectTagger).Object;
            Vector4[] buffer = null;
            if (O.getSet<gIPath>() != null)
                buffer = O.getSet<gIPath>().getBuffer();
            else buffer = O.getSet<gIZone>().getBuffer();

            if (buffer == null || buffer.Length > 100) return;

            E.Variables["ColorA"].SetVariable(V);
            E.Variables["WVP"].SetVariable(O.Matrix * RI.ViewMatrix * RI.ProjectionMatrix);
            E.Variables["PointA"].SetVariable(buffer);
            INavBase b = (GN.Tag as ILrentObject.ObjectTagger).Object.getSet<INavBase>();
            mBuffer.DrawBufferNonIndexed(E.Techniques[0].Passes[0], buffer.Length, b is gIPath ? PrimitiveTopology.TriangleList : PrimitiveTopology.LineList);
        }

        public override void PrepareForNode(RenderInformation RI, GraphicNode GN)
        {
            
        }

        public override void RenderAll()
        {
            if (DrawAllNav)
            {
                RenderInformation RI = ManagedWorld.NodeLibrary.Camera.CreateRenderInformation(true);
                tSorter.RenderLibrary(RI, this);
            }
            else if (lastObj != null && !(lastObj is gIColl))
            {
                RenderInformation RI = ManagedWorld.NodeLibrary.Camera.CreateRenderInformation(true);
                DrawSubObjekt(RI, null, lastObj.Object.Nodes[0]);
            }
        }
    }
}
