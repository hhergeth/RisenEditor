using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SlimDX;
using GameLibrary;
using GameLibrary.Objekte;
using System.Drawing;
using GameLibrary.Rendering;
using MB = System.Windows.Forms.MouseButtons;
using CC = System.Windows.Forms.Control;
using SlimDX.Direct3D11;
using RisenEditor.Code;
using RisenEditor.Code.Loader;
using RisenEditor.Code.Renderer;
using RisenEditor.Code.RisenTypes;
using W32 = System.Windows.Forms;

namespace RisenEditor.Code.Renderer
{
    public class InformationDrawer : GameLibrary.Rendering.PostProcessor
    {
        int SPacing;
        AdvTextRenderer tRenderer;
        Form1 P;

        public InformationDrawer(Form1 F)
        {
            P = F;
            tRenderer = new AdvTextRenderer(new Font("Arial", 18), F.Device, Color.White);
            SPacing = (int)(tRenderer.textHeight * 1.1f);
        }

        public bool Enabled
        {
            get
            {
                return true;
            }

            set
            {

            }
        }

        public API_Device Device
        {
            get
            {
                return P.Device;
            }
        }

        public void DoProcess(GameLibrary.Rendering.RenderInformation rI)
        {
            tRenderer.StartStrip();
            P.BackBuffer.SetTarget();
            //dCam(rI);
            if (P.CurrNode != null)
            {
                ILrentObject n = P.CurrNode;
                string pos = "Position : " + n.Position.ToString();
                string name = "Name : " + n.Name;
                string lFile = "Lrent File : " + n.File.Name;

                if (P.BackBufferControl.Width > 1000)
                {
                    int w = tRenderer.getStringWidthInPixels(pos);
                    int x = P.BackBufferControl.Width - w - 10;
                    int y = 40;
                    tRenderer.DrawString(pos, x, y);
                    w = tRenderer.getStringWidthInPixels(name);
                    x = P.BackBufferControl.Width - w - 10;
                    y = 40 + SPacing;
                    tRenderer.DrawString(name, x, y);
                    w = tRenderer.getStringWidthInPixels(lFile);
                    x = P.BackBufferControl.Width - w - 10;
                    y = 40 + SPacing + SPacing;
                    tRenderer.DrawString(lFile, x, y);
                }
                else
                {

                }
            }
            tRenderer.Batch();
        }

        private void dCam(GameLibrary.Rendering.RenderInformation rI)
        {
            string pos = "Camera Position : " + rI.CameraPosition.ToString();
            string tar = "Camera target : " + rI.ViewLookAt.ToString();
            string fps = "FPS : " + GameLibrary.Rendering.DrawFrame.FPS.ToString();
            int x = 0;
            int y = 40;
            tRenderer.DrawString(pos, x + 0, y + 0);
            tRenderer.DrawString(tar, x + 0, y + SPacing);
            tRenderer.DrawString(fps, x + 0, y + SPacing + SPacing);
        }
    }

    public class GizmoComponent : GameLibrary.Rendering.PostProcessor
    {
        public struct colorVertex : IVertexLayout
        {
            private static InputElement[] ies = new InputElement[]
                {
                    new InputElement("TEXCOORD", 0, SlimDX.DXGI.Format.R32_UInt, 0, 0),
                    new InputElement("TEXCOORD", 1, SlimDX.DXGI.Format.R32_UInt, 4, 0),
                };
            private static Dictionary<string, InputLayout> bufferedLayouts = new Dictionary<string, InputLayout>();

            uint index;
            uint index2;
            public colorVertex(int i, int i2) { index = (uint)i; index2 = (uint)i2; }

            public InputLayout FetchILayout(EffectPassDescription d, API_Device D)
            {
                if (bufferedLayouts.ContainsKey(d.Name))
                    return bufferedLayouts[d.Name];
                else
                {
                    InputLayout ilayout = new InputLayout(D.HadrwareDevice(), d.Signature, ies);
                    bufferedLayouts.Add(d.Name, ilayout);
                    return ilayout;
                }
            }

            public int SizeInBytes
            {
                get
                {
                    return 8;
                }
            }

            public Vector4 Position
            {
                get
                {
                    return new Vector4();
                }

                set
                {

                }
            }

            public InputElement[] InputElements
            {
                get
                {
                    return ies;
                }
            }
        }

        const float scale = 200;
        const float s = 0.25f;
        Form1 P;
        EffectWrapper E;
        AdvTextRenderer tRenderer;
        int selection = -1;//x, y, z, xz, zy, yx
        MeshBuffer meshBuffer;
        Vector4[] ColorA;
        bool didCopy;
        
        static public bool dragging;
        public bool enabledDragging;

        public GizmoComponent(Form1 Parent)
        {
            tRenderer = new AdvTextRenderer(new Font("Arial", 23), Parent.Device, Color.White);
            Parent.BackBufferControl.MouseMove += new System.Windows.Forms.MouseEventHandler(BackBufferControl_MouseMove);
            P = Parent;
            API_Device D = Parent.Device;
            E = new EffectWrapper("gizmo.fx", D);

            float f = scale; float q = scale * s;
            Vector4[] V = new Vector4[]
            {
                new Vector4(0, 0, 0, 1),

                new Vector4(f, 0, 0, 1), // 1
                new Vector4(0, f, 0, 1),
                new Vector4(0, 0, f, 1),

                new Vector4(q, 0, 0, 1), // 4
                new Vector4(0, q, 0, 1),
                new Vector4(0, 0, q, 1),

                new Vector4(q, q, 0, 1), // 7
                new Vector4(0, q, q, 1),
                new Vector4(q, 0, q, 1),
            };
            int[] I = new int[]
            {
                0, 1, 0, 2, 0, 3,
                4, 7, 5, 7,
                5, 8, 6, 8,
                6, 9, 4, 9,
            };
            int[] I2 = new int[]
            {
                0, 0, 1, 1, 2, 2,
                3, 3, 4, 4,
                5, 5, 6, 6,
                7, 7, 8, 8,
            };
            colorVertex[] Dy = new colorVertex[I.Length];
            for (int i = 0; i < Dy.Length; i++)
                Dy[i] = new colorVertex(I[i], I2[i]);
            meshBuffer = new MeshBuffer(Dy, Dy[0], D, PrimitiveTopology.LineList);
            E.Variables["PointA"].SetVariable(V);
            ColorA = new Vector4[10]; Vector4 x = new Vector4(1, 0, 0, 1); Vector4 y = new Vector4(0, 0, 1, 1); Vector4 z = new Vector4(0, 1, 0, 1);
            ColorA[0] = x;
            ColorA[1] = y;
            ColorA[2] = z;
            ColorA[3] = x;
            ColorA[4] = y;
            ColorA[5] = y;
            ColorA[6] = z;
            ColorA[7] = z;
            ColorA[8] = x;
        }

        private bool isNear(Vector2 p1, Vector2 p2, float x, float y)
        {
            float m1 = (p2.Y - p1.Y) / (p2.X - p1.X);
            float b1 = p1.Y - m1 * p1.X;
            float m2 = -1.0f / m1;
            float b2 = y - m2 * x;
            float xs = (b2 - b1) / (m1 - m2);
            float ys = m1 * xs + b1;
            float l = (new Vector2(xs, ys) - new Vector2(x,y)).Length();
            return l < 10;
        }
        private bool isNear(Vector2 v, float x, float y)
        {
            float l = (v - new Vector2(x, y)).Length();
            return l < 100;
        }
        private bool SameSide(Vector3 p1, Vector3 p2, Vector3 a, Vector3 b)
        {
            Vector3 cp1 = Vector3.Cross(b - a, p1 - a);
            Vector3 cp2 = Vector3.Cross(b - a, p2 - a);
            return Vector3.Dot(cp1, cp2) >= 0.0f;
        }
        private bool PointInTriangle(Vector3 a, Vector3 b, Vector3 c, Vector3 p)
        {
            return (SameSide(p,a, b,c) && SameSide(p,b, a,c) && SameSide(p,c, a,b));
        }
        private bool PointInRect(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4, Vector3 p)
        {
            return PointInTriangle(p1, p2, p3, p) || PointInTriangle(p3, p4, p1, p);
        }
        private bool[] CheckAreas(Vector3 PO, Vector3 axisx, Vector3 axisy, Vector3 axisz, Vector3 Q, Ray r1)
        {
            Vector3 po = ManagedWorld.NodeLibrary.Camera.Project(PO, P.BackBuffer).ToVec3();
            Vector3 pxz = ManagedWorld.NodeLibrary.Camera.Project(PO + axisx * s + axisz * s, P.BackBuffer).ToVec3();
            Vector3 pyz = ManagedWorld.NodeLibrary.Camera.Project(PO + axisy * s + axisz * s, P.BackBuffer).ToVec3();
            Vector3 pyx = ManagedWorld.NodeLibrary.Camera.Project(PO + axisx * s + axisy * s, P.BackBuffer).ToVec3();
            Vector3 px = ManagedWorld.NodeLibrary.Camera.Project(axisx * s + PO, P.BackBuffer).ToVec3();
            Vector3 py = ManagedWorld.NodeLibrary.Camera.Project(axisy * s + PO, P.BackBuffer).ToVec3();
            Vector3 pz = ManagedWorld.NodeLibrary.Camera.Project(axisz * s + PO, P.BackBuffer).ToVec3();
            bool[] B = new bool[3];
            B[0] = PointInRect(pz, pxz, px, po, Q);
            B[1] = PointInRect(py, pyz, pz, po, Q);
            B[2] = PointInRect(po, px, pyx, py, Q);
            /*
            Bitmap BB = new Bitmap(P.BackBuffer.Width, P.BackBuffer.Height);
            Graphics G = Graphics.FromImage(BB);
            Pen PQQ = new Pen(new SolidBrush(Color.Red));
            G.DrawEllipse(PQQ, pz.X - 1, pz.Y - 1, 2, 2);
            G.DrawEllipse(PQQ, pxz.X - 1, pxz.Y - 1, 2, 2);
            G.DrawEllipse(PQQ, px.X - 1, px.Y - 1, 2, 2);
            G.DrawEllipse(PQQ, po.X - 1, po.Y - 1, 2, 2);
            G.DrawEllipse(PQQ, Q.X - 1, Q.Y - 1, 2, 2);
            G.Flush();
            BB.Save("1.jpg");*/

            if(!((B[0] && B[1]) || (B[0] && B[2]) || (B[1] && B[2])))
                return B;

            Plane xz = new Plane(PO, Vector3.UnitY);
            Plane zy = new Plane(PO, Vector3.UnitX);
            Plane yx = new Plane(PO, Vector3.UnitZ);
            float? fxz0 = r1.Intersects(xz);
            float? fzy0 = r1.Intersects(zy);
            float? fyx0 = r1.Intersects(yx);
            float fxz1 = (fxz0.HasValue && B[0]) ? fxz0.Value : float.MaxValue;
            float fzy1 = (fzy0.HasValue && B[1]) ? fzy0.Value : float.MaxValue;
            float fyx1 = (fyx0.HasValue && B[2]) ? fyx0.Value : float.MaxValue;
            if (B[0] && fxz0.HasValue && (fxz1 < fzy1 && fxz1 < fyx1))
                B[0] = true;
            else B[0] = false;
            if (B[1] && fzy0.HasValue && (fzy1 < fxz1 && fzy1 < fyx1))
                B[1] = true;
            else B[1] = false;
            if (B[2] && fyx0.HasValue && (fyx1 < fxz1 && fyx1 < fzy1))
                B[2] = true;
            else B[2] = false;
            
            return B;
        }
        private float calcDist(Vector3 v1, Vector3 d1)
        {
            float dp = Vector3.Distance(v1, ManagedWorld.NodeLibrary.Camera.Position_ABS) / (ManagedSettings.FarDepth - ManagedSettings.NearDepth);
            const float maxf = 1000;
            const float minf = 10;
            float r = dp * (maxf - minf) + minf;
            return r;
        }

        public void BackBufferControl_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (didCopy && (W32.Control.ModifierKeys & W32.Keys.Shift) != W32.Keys.Shift)
                didCopy = false;
            int lastSelection = selection;
            dragging = false;
            if (!enabledDragging) return;
            if (e.Button != MB.Left || P.CurrNode == null)
            {
                selection = -1;
                return;
            }
            Matrix m = P.CurrNode.Matrix;
            Vector3 po = P.CurrNode.Position;
            Quaternion q = P.CurrNode.Rotation;
            if (P.CoordSystem == eECoordinateSystem.eECoordinateSystem_Independent)
            {
                m = Matrix.Translation(po);
                q = Quaternion.Identity;
            }
            Vector3 d = ManagedWorld.NodeLibrary.Camera.Unproject(e.X, e.Y, ManagedSettings.FarDepth - 1, P.BackBuffer);
            Ray r1 = new Ray(ManagedWorld.NodeLibrary.Camera.Position_ABS, Vector3.Normalize(d - ManagedWorld.NodeLibrary.Camera.Position_ABS));
            if (selection == -1)
            {
                Vector3 axisx = Vector3.Transform(Vector3.UnitX * scale, q).ToVec3();
                Vector3 axisy = Vector3.Transform(Vector3.UnitY * scale, q).ToVec3();
                Vector3 axisz = Vector3.Transform(Vector3.UnitZ * scale, q).ToVec3();
                Vector2 o = ManagedWorld.NodeLibrary.Camera.Project(Vector3.Transform(Vector3.Zero, m).ToVec3(), P.BackBuffer);
                Vector2 p1 = ManagedWorld.NodeLibrary.Camera.Project(axisx + po, P.BackBuffer);
                Vector2 p2 = ManagedWorld.NodeLibrary.Camera.Project(axisy + po, P.BackBuffer);
                Vector2 p3 = ManagedWorld.NodeLibrary.Camera.Project(axisz + po, P.BackBuffer);
                Vector2 p2_xz = ManagedWorld.NodeLibrary.Camera.Project(po + axisx * s + axisz * s, P.BackBuffer);
                Vector2 p2_yz = ManagedWorld.NodeLibrary.Camera.Project(po + axisy * s + axisz * s, P.BackBuffer);
                Vector2 p2_yx = ManagedWorld.NodeLibrary.Camera.Project(po + axisx * s + axisy * s, P.BackBuffer);

                bool x = isNear(o, p1, e.X, e.Y);
                bool y = isNear(o, p2, e.X, e.Y);
                bool z = isNear(o, p3, e.X, e.Y);
                bool[] bb = CheckAreas(Vector3.Transform(Vector3.Zero, m).ToVec3(), axisx, axisy, axisz, new Vector3(e.X, e.Y, 0), r1);
                if (bb[0] || bb[1] || bb[2])
                    x = y = z = false;
                int SAT = Convert.ToInt32(x) + Convert.ToInt32(y) + Convert.ToInt32(z) + Convert.ToInt32(bb[0]) + Convert.ToInt32(bb[1]) + Convert.ToInt32(bb[2]);
                if(SAT == 0 || SAT > 1)
                    return;
                if (x) selection = 0; if (y) selection = 1; if (z) selection = 2; if (bb[0]) selection = 3; if (bb[1]) selection = 4; if (bb[2]) selection = 5;
            }
            if (selection < 3)
            {
                Vector3 v = new Vector3();
                if (selection == 0) v = Vector3.UnitX; if (selection == 1) v = Vector3.UnitY; if (selection == 2) v = Vector3.UnitZ;
                v = Vector3.Transform(v, q).ToVec3();
                Ray r2 = new Ray(po, v);
                Vector3? hit = r2.ClosestPoint(r1);
                float s0 = (hit.Value - P.CurrNode.Position).Length();
                float fk = calcDist(P.CurrNode.Position, v);
                if (!hit.HasValue || s0 > scale)
                {
                    //selection = -1;
                    return;
                }
                if (lastSelection == -1 && s0 > fk)
                {
                    dragging = true;
                    return;
                }
                float t = (hit.Value[selection] - r2.Position[selection]) / r2.Direction[selection];
                if ((W32.Control.ModifierKeys & W32.Keys.Shift) == W32.Keys.Shift && !didCopy)
                {
                    didCopy = true;
                    P.CurrNode = (ILrentObject)P.CurrNode.Clone(P.GetCopyFile(P.CurrNode.File), P.Device);
                }
                if (s0 < fk)
                    P.CurrNode.Position = hit.Value;
                else P.CurrNode.Position += r2.Direction * fk * (float)Math.Sign(t); 
                dragging = true;
            }
            else
            {
                Vector3 n = new Vector3();
                if (selection == 3) n = Vector3.UnitY; if (selection == 4) n = Vector3.UnitX; if (selection == 5) n = Vector3.UnitZ;
                n = Vector3.Transform(n, q).ToVec3();
                Plane p = new Plane(po, n);
                float? fq = r1.Intersects(p);
                if (!fq.HasValue)
                    return;
                Vector3 hit = r1.Position + r1.Direction * fq.Value;
                P.CurrNode.Position = hit;
                dragging = true;
            }
        }

        private Vector4[] doColors()
        {
            if (selection == -1) return ColorA;
            Vector4[] C = new Vector4[ColorA.Length];
            Array.Copy(ColorA, C, ColorA.Length);
            Vector4 A = new Color4(Color.Yellow).ToVector4();
            if (selection < 3 && selection >= 0)
                C[selection] = A;
            else
            {
                if (selection == 3)
                    C[0] = C[2] = C[8] = C[7] = A;
                else if (selection == 4)
                    C[1] = C[2] = C[6] = C[5] = A;
                else if (selection == 5)
                    C[0] = C[1] = C[4] = C[3] = A;
            }
            return C;
        }
        public void DoProcess(GameLibrary.Rendering.RenderInformation rI)
        {
            BoundingBox B = new BoundingBox(P.CurrNode.Position - new Vector3(scale), P.CurrNode.Position + new Vector3(scale));
            if (rI.BoundingFrustums[0].Contains(B) == ContainmentType.Disjoint)
                return;
            Matrix m = P.CurrNode.Matrix;
            Vector3 po = P.CurrNode.Position;
            Quaternion q = P.CurrNode.Rotation;
            if (P.CoordSystem == eECoordinateSystem.eECoordinateSystem_Independent)
            {
                m = Matrix.Translation(po);
                q = Quaternion.Identity;
            }
            Vector3 axisx = Vector3.Transform(Vector3.UnitX * scale, q).ToVec3();
            Vector3 axisy = Vector3.Transform(Vector3.UnitY * scale, q).ToVec3();
            Vector3 axisz = Vector3.Transform(Vector3.UnitZ * scale, q).ToVec3();
            Vector2 o = ManagedWorld.NodeLibrary.Camera.Project(Vector3.Transform(Vector3.Zero, m).ToVec3(), P.BackBuffer);
            Vector2 p1 = ManagedWorld.NodeLibrary.Camera.Project(axisx + po, P.BackBuffer);
            Vector2 p2 = ManagedWorld.NodeLibrary.Camera.Project(axisy + po, P.BackBuffer);
            Vector2 p3 = ManagedWorld.NodeLibrary.Camera.Project(axisz + po, P.BackBuffer);
            Vector2 p2_xz = ManagedWorld.NodeLibrary.Camera.Project(po + axisx * s + axisz * s, P.BackBuffer);
            Vector2 p2_yz = ManagedWorld.NodeLibrary.Camera.Project(po + axisy * s + axisz * s, P.BackBuffer);
            Vector2 p2_yx = ManagedWorld.NodeLibrary.Camera.Project(po + axisx * s + axisy * s, P.BackBuffer);

            E.Variables["ColorA"].SetVariable(doColors());
            E.Variables["WVP"].SetVariable(m * rI.ViewMatrix * rI.ProjectionMatrix);
            Device.OutputMerger.DepthComparison = Comparison.Always;
            meshBuffer.DrawBufferNonIndexed(E.Techniques[0].Passes[0]);
            tRenderer.StartStrip();
            tRenderer.DrawString("X", (int)p1.X - 15, (int)p1.Y, selection == 0 ? Color.Red : Color.White);
            tRenderer.DrawString("Y", (int)p2.X - 15, (int)p2.Y, selection == 1 ? Color.Red : Color.White);
            tRenderer.DrawString("Z", (int)p3.X - 15, (int)p3.Y, selection == 2 ? Color.Red : Color.White);
            if(selection == 3)
                tRenderer.DrawString("XZ", (int)p2_xz.X - 15, (int)p2_xz.Y, Color.Red);
            if (selection == 4)
                tRenderer.DrawString("YZ", (int)p2_yz.X - 15, (int)p2_yz.Y, Color.Red);
            if (selection == 5)
                tRenderer.DrawString("YX", (int)p2_yx.X - 15, (int)p2_yx.Y, Color.Red);
            Device.OutputMerger.DepthComparison = Comparison.LessEqual;
            tRenderer.Batch();
        }

        public bool Enabled
        {
            get
            {
                return P.CurrNode != null;
            }

            set
            {

            }
        }

        public API_Device Device
        {
            get
            {
                return P.Device;
            }
        }
    }

    public class NodeDisplayerPostProc : PostProcessor
    {
        Form1 P;
        MeshBuffer mBuffer;
        EffectWrapper LineDrawer;

        public NodeDisplayerPostProc(Form1 P)
        {
            LineDrawer = new EffectWrapper("Line.fx", P.Device);
            this.P = P;
            short[] indices = new short[] { 
                0, 1, 0, 2, 0, 3, 1, 4, 1, 6, 2, 4, 2, 5, 3, 5, 
                3, 6, 4, 7, 5, 7, 6, 7
             };
            BoundingBox boundingBox = new BoundingBox(new Vector3(-1.0f), new Vector3(+1.0f));
            StaticVertex[] vertices = new StaticVertex[8];
            vertices[0].Position = new Vector4(boundingBox.Minimum.X, boundingBox.Minimum.Y, boundingBox.Minimum.Z, 1f);
            vertices[1].Position = new Vector4(boundingBox.Minimum.X, boundingBox.Minimum.Y, boundingBox.Maximum.Z, 1f);
            vertices[2].Position = new Vector4(boundingBox.Minimum.X, boundingBox.Maximum.Y, boundingBox.Minimum.Z, 1f);
            vertices[3].Position = new Vector4(boundingBox.Maximum.X, boundingBox.Minimum.Y, boundingBox.Minimum.Z, 1f);
            vertices[4].Position = new Vector4(boundingBox.Minimum.X, boundingBox.Maximum.Y, boundingBox.Maximum.Z, 1f);
            vertices[5].Position = new Vector4(boundingBox.Maximum.X, boundingBox.Maximum.Y, boundingBox.Minimum.Z, 1f);
            vertices[6].Position = new Vector4(boundingBox.Maximum.X, boundingBox.Minimum.Y, boundingBox.Maximum.Z, 1f);
            vertices[7].Position = new Vector4(boundingBox.Maximum.X, boundingBox.Maximum.Y, boundingBox.Maximum.Z, 1f);
            mBuffer = new MeshBuffer(vertices, indices, SlimDX.DXGI.Format.R16_UInt, vertices[0], P.Device, SlimDX.Direct3D11.PrimitiveTopology.LineList, SlimDX.Direct3D11.CpuAccessFlags.Write);    
        }

        public void DrawBox(ILrentObject o, RenderInformation RI, bool abs)
        {
            Vector3 p = o.BoundingBox_LOCAL.Center();
            Matrix m_World = Matrix.Scaling(o.BoundingBox.Size() / 2.0f) * Matrix.Translation(o.BoundingBox.Center());
            if(!abs)
                m_World = Matrix.Scaling(o.BoundingBox_LOCAL.Size() / 2.0f) * (Matrix.Translation(p) * Matrix.RotationQuaternion(o.Rotation)) * Matrix.Translation(o.Position);
            Matrix m_WorldViewProjection = m_World * RI.ViewMatrix * RI.ProjectionMatrix;
            LineDrawer.Variables["WorldViewProj"].SetVariable(m_WorldViewProjection);
            LineDrawer.Variables["Color"].SetVariable(new Color4(abs ? Color.Red : Color.Sienna).ToVector4());
            mBuffer.DrawBufferIndexed(LineDrawer.Techniques[0].Passes[0]);
        }

        public API_Device Device
        {
            get
            {
                return P.Device;
            }
        }

        public bool Enabled
        {
            get
            {
                return true;
            }

            set
            {

            }
        }

        public void DoProcess(RenderInformation RI)
        {
            if (P.CurrNode == null)
                return;
            bool b = P.CoordSystem == eECoordinateSystem.eECoordinateSystem_Independent;
            if (P.CurrNode is LrentObjectCollection)
            {
                for (int i = 0; i < (P.CurrNode as LrentObjectCollection).Objects.Count; i++)
                    DrawBox((P.CurrNode as LrentObjectCollection).Objects[i], RI, b);
            }
            else DrawBox(P.CurrNode, RI, b);
        }
    }

    public class IconDrawer : PostProcessor
    {
        Form1 P;
        EffectWrapper E;
        MeshBuffer M;
        public const float halfSize = 50;

        public IconDrawer(Form1 F)
        {
            P = F;
            E = new EffectWrapper("IconDrawer.fx", F.Device);
            DynamicIndexVertex[] D = new DynamicIndexVertex[] { new DynamicIndexVertex(0), new DynamicIndexVertex(1), new DynamicIndexVertex(2), 
                                                                new DynamicIndexVertex(2), new DynamicIndexVertex(3), new DynamicIndexVertex(0)};
            M = new MeshBuffer(D, D[0], Device, SlimDX.Direct3D11.PrimitiveTopology.TriangleList);
        }

        public void DoProcess(RenderInformation RI)
        {
            IList<Node> N = ManagedWorld.NodeLibrary.OcTree.FindNodes(RI.BoundingFrustums[0]);
            E.Variables["m_Projection"].SetVariable(RI.ProjectionMatrix);
            E.Variables["K"].SetVariable(halfSize);
            foreach (Node n in N)
            {
                if (!(n.Tag is ILrentObject.ObjectTagger) || (n.Tag as ILrentObject.ObjectTagger).Icon == null) continue;
                E.Variables["Pos"].SetVariable(Vector3.Transform(n.Position_ABS, RI.ViewMatrix).ToVec3());
                E.Variables["DiffuseTexture"].SetVariable((n.Tag as ILrentObject.ObjectTagger).Icon.ShaderResourceView);
                M.DrawBufferNonIndexed(E.Techniques[0].Passes[0]);
            }
        }

        public API_Device Device
        {
            get
            {
                return P.Device;
            }
        }

        public bool Enabled
        {
            get
            {
                return true;
            }

            set
            {

            }
        }
    }
}
