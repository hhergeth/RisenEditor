using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GameLibrary;
using GameLibrary.Objekte;
using SlimDX;
using SlimDX.Direct3D11;

namespace RisenEditor.Code.Renderer
{
    public class GrassNode : GraphicNodeCollection
    {
        public GrassNode(List<GraphicNode> gs, API_Device D, ILrentObject a_Obj)
            : base(gs, true)
        {
            this.D = D;
            ManagedWorld.NodeLibrary.RemoveNode(this);
        }

        public GrassNode()
        {

        }

        public override void DeSerialize(GameLibrary.IO.BinaryStream bs, DeviceApplication DA)
        {
            base.DeSerialize(bs, DA);
        }
    }

    public class VegetationMiniPart
    {
        public Mesh mBody;
        public Vector3 vPosition;
        public Vector3 vSize;
        public Quaternion qOrientation;
        public Matrix mWorld;
        public string sPath;
        public BoundingBox bb;

        public VegetationMiniPart(GrassNode gn, int i)
        {
            mBody = gn[i];
            vPosition = gn.Positions[i];
            vSize = gn.Sizes[i];
            qOrientation = gn.Orientations[i];
            mWorld = gn.Matrices[i];
            sPath = gn.Paths[i];
            bb = mBody.BoundingBox.Transform(mWorld);
        }

        public VegetationMiniPart(GraphicNode gn, int i)
        {
            mBody = new Mesh(gn.BoundingBox, gn.Mesh.CullMode, gn.Mesh.Parts[i]);
            vPosition = gn.Position_ABS;
            vSize = gn.Size_ABS;
            qOrientation = gn.Orientation_ABS;
            mWorld = gn.ModelMatrix_ABS;
            sPath = gn.Path + i.ToString();
            bb = mBody.BoundingBox.Transform(mWorld);
        }
    }

    public class VegetationMiniPartCollection : Node
    {
        public GraphicNode qTST;
        API_Device D;
        public VegetationMiniPart[] gps;
        public string p;
        ShaderResourceBuffer sBuffer;
        VertexBufferBinding vb;
        private static InputElement[] ies = new InputElement[]
                                         {
                                            new InputElement("POSITION", 0, SlimDX.DXGI.Format.R32G32B32A32_Float, 0, 0),
                                            new InputElement("TEXCOORD", 0, SlimDX.DXGI.Format.R32G32_Float, 16, 0),
                                            new InputElement("NORMAL", 0, SlimDX.DXGI.Format.R32G32B32_Float, 24, 0),
                                            new InputElement("TANGENT", 0, SlimDX.DXGI.Format.R32G32B32_Float, 36, 0),
                                            new InputElement("TEXCOORD", 1, SlimDX.DXGI.Format.R32G32B32_Float, 0, 1, InputClassification.PerInstanceData, 1),
                                            new InputElement("TEXCOORD", 2, SlimDX.DXGI.Format.R32G32B32A32_Float, 12, 1, InputClassification.PerInstanceData, 1),
                                         };
        static InputLayout il;

        public VegetationMiniPartCollection(ICollection<VegetationMiniPart> ps)
        {
            ManagedWorld.NodeLibrary.RemoveNode(this);
            gps = ps.ToArray<VegetationMiniPart>();
            p = gps[0].sPath;
            D = gps[0].mBody.Device;
            //List<Vector3> ms = new List<Vector3>();
            //List<Quaternion> qs = new List<Quaternion>();
            List<byte> bs = new List<byte>();
            int stride = 3 * 4 + 4 * 4;//float3 + float4
            BoundingBox abs = new BoundingBox(new Vector3(1E10f), new Vector3(-1E10f));
            foreach (VegetationMiniPart gp in gps)
            {
                abs = abs.Extend(gp.bb);
                //ms.Add(gp.vPosition);
                //qs.Add(gp.qOrientation);
                bs.AddRange(gp.vPosition.GetBytes());
                bs.AddRange(gp.qOrientation.GetBytes());
            }
            this.BoundingBox = abs;
            DataStream ds = new DataStream(bs.ToArray(), true, true);
            sBuffer = new ShaderResourceBuffer(ds, SlimDX.Direct3D11.BindFlags.VertexBuffer, SlimDX.Direct3D11.CpuAccessFlags.None, SlimDX.Direct3D11.ResourceUsage.Default, SlimDX.Direct3D11.ResourceOptionFlags.None, stride, D);
            ds.Dispose();
            vb = new SlimDX.Direct3D11.VertexBufferBinding(sBuffer.Buffer, sBuffer.Description.StructureByteStride, 0);
            qTST = new GraphicNode("", null, D); qTST.BoundingBox = this.BoundingBox; qTST.ParentNode = this;
        }

        public void dInstanced(Effect_Pass E)
        {
            if (il == null)
                il = new InputLayout(D.HadrwareDevice(), E.Pass.Description.Signature, ies);
            //setting mesh buffer
            MeshBuffer m = gps[0].mBody.Parts[0].Buffer;
            if(m.IndexBuffer != null)
                this.D.InputAssembler.SetIndexBuffer(0, m.IndexFormat, m.IndexBuffer.Buffer);
            this.D.InputAssembler.SetVertexBuffer(0, m.Bindings);
            this.D.InputAssembler.PrimitiveType = m.PrimitiveType;
            this.D.InputAssembler.InputLayout = il;
            //setting matrix buffer
            this.D.InputAssembler.SetVertexBuffer(1, vb);
            //drawing
            E.Activate();
            if(m.IndexBuffer != null)
                gps[0].mBody.Device.Context.DrawInstanced(gps[0].mBody.Parts[0].Buffer.VertexCount, gps.Length, 0, 0);
            //ui
            GameLibrary.Rendering.DrawFrame.DrawCalls++;
            GameLibrary.Rendering.DrawFrame.TriDrawn += gps.Length * gps[0].mBody.Parts[0].Buffer.VertexCount / 3;
        }
    }

    public static class VegetationManager
    {
        public static List<VegetationMiniPartCollection> lrentPatches = new List<VegetationMiniPartCollection>();
        public static List<OcTree.Oc> trees = new List<OcTree.Oc>();
        public static OcTree.Oc qT;
        public static bool easy = false;
        public static int mDepth = 3;//1 is way to low, 2 or 3 should be perfect

        public static void addLrent(List<GrassNode> gns, API_Device D)
        {
            Dictionary<string, List<VegetationMiniPart>> ps = new Dictionary<string, List<VegetationMiniPart>>();
            for (int i = 0; i < gns.Count; i++)
            {
                for (int j = 0; j < gns[i].Count; j++)
                {
                    VegetationMiniPart p = new VegetationMiniPart(gns[i], j);
                    if (ps.ContainsKey(p.sPath))
                        ps[p.sPath].Add(p);
                    else
                    {
                        List<VegetationMiniPart> psa = new List<VegetationMiniPart>();
                        psa.Add(p);
                        ps.Add(p.sPath, psa);
                    }
                }
            }
            Build(D, ps);
            foreach (GrassNode gn in gns)
                gn.Dispose();
        }

        public static void addLrent(List<GraphicNode> gns, API_Device D)
        {
            Dictionary<string, List<VegetationMiniPart>> ps = new Dictionary<string, List<VegetationMiniPart>>();
            for (int i = 0; i < gns.Count; i++)
            {
                for (int j = 0; j < gns[i].Mesh.Parts.Length; j++)
                {
                    VegetationMiniPart p = new VegetationMiniPart(gns[i], j);
                    if (ps.ContainsKey(p.sPath))
                        ps[p.sPath].Add(p);
                    else
                    {
                        List<VegetationMiniPart> psa = new List<VegetationMiniPart>();
                        psa.Add(p);
                        ps.Add(p.sPath, psa);
                    }
                }
            }
            Build(D, ps);
            foreach (GraphicNode gn in gns)
                gn.Dispose();
        }

        private static void Build(API_Device D, Dictionary<string, List<VegetationMiniPart>> ps)
        {
            BoundingBox bb = new BoundingBox(new Vector3(1E10f), new Vector3(-1E10f));
            foreach (KeyValuePair<string, List<VegetationMiniPart>> kvp in ps)
            {
                if (easy)
                {
                    VegetationMiniPartCollection gp = new VegetationMiniPartCollection(kvp.Value);
                    lrentPatches.Add(gp);
                    bb = bb.Extend(gp.BoundingBox);
                }
                else
                {
                    BoundingBox BB2 = new BoundingBox(new Vector3(1E10f), new Vector3(-1E10f));
                    foreach (VegetationMiniPart gp in kvp.Value)
                        BB2 = BB2.Extend(gp.bb);
                    bb = bb.Extend(BB2);
                    mDepth = 3;
                    float l = (bb.Maximum - bb.Minimum).Length();
                    if (l < 5000)
                        mDepth = 0;
                    bTree(kvp.Value, lrentPatches, 0, BB2);
                }
            }
            List<Node> qs = lrentPatches.Cast<Node>().ToList();
            trees.Add(new OcTree.Oc(bb, qs, 0, null));
            /*
            if (trees.Count == 0)
                return;
            bb = trees[0].ContainerBox;
            IList<Node> ns = new List<Node>();
            foreach (OcTree.Oc on in trees)
            {
                bb = bb.Extend(on.ContainerBox);
                on.Find(ref ns, ref bb);
            }
            qT = new OcTree.Oc(bb, ns, 0, trees[0].Device);*/
        }

        private static void bTree(List<VegetationMiniPart> cList, List<VegetationMiniPartCollection> mLists, int depth, BoundingBox BB)
        {
            if (depth == mDepth)
            {
                mLists.Add(new VegetationMiniPartCollection(cList));
                return;
            }
            Vector3 min = BB.Minimum;
            Vector3 max = BB.Maximum;
            Vector3 size = (max - min) * .5f;
            Vector3 sizeX = new Vector3(size.X, 0f, 0f);
            Vector3 sizeY = new Vector3(0f, size.Y, 0f);
            Vector3 sizeZ = new Vector3(0f, 0f, size.Z);
            List<BoundingBox> BBs = new List<BoundingBox>()
            {
                new BoundingBox(min, min + size),
                new BoundingBox(min + sizeZ, min + sizeZ + size),
                new BoundingBox(min + sizeX, min + sizeX + size),
                new BoundingBox(min + sizeX + sizeZ, min + sizeX + sizeZ + size),
                new BoundingBox(min + sizeY, min + sizeY + size),
                new BoundingBox(min + sizeZ + sizeY, min + sizeZ + sizeY + size),
                new BoundingBox(min + sizeX + sizeY, min + sizeX + sizeY + size),
                new BoundingBox(min + size, max),
            };
            List<List<VegetationMiniPart>> bbParts = new List<List<VegetationMiniPart>>();
            for (int i = 0; i < 8; i++)
                bbParts.Add(new List<VegetationMiniPart>());
            for (int i = cList.Count - 1; i >= 0; i--)
            {
                for (int j = 0; j < 8; j++)
                {
                    if (BBs[j].Contains(cList[i].bb) == ContainmentType.Contains)
                    {
                        bbParts[j].Add(cList[i]);
                        cList.RemoveAt(i);
                        break;
                    }
                }
            }
            if (cList.Count > 0)
                mLists.Add(new VegetationMiniPartCollection(cList));
            for (int i = 0; i < 8; i++)
            {
                if (bbParts[i].Count > 0)
                {
                    bTree(bbParts[i], mLists, depth + 1, BBs[i]);
                }
            }
        }

        public static void clear()
        {
            lrentPatches = new List<VegetationMiniPartCollection>();
            foreach (OcTree.Oc o in trees)
                o.Dispose();
            trees = new List<OcTree.Oc>();
        }

        public static IList<Node> fetchNodes(BoundingFrustum bf)
        {
            IList<Node> ns = new List<Node>();
            //if (qT == null)
            //    return ns;
            //qT.Find(ref ns, ref bf);
            //return ns;
            foreach (OcTree.Oc o in trees)
                if (o.ContainerBox.Contains(bf) != ContainmentType.Disjoint)
                    o.Find(ref ns, ref bf);
            return ns;
        }
    }
}
