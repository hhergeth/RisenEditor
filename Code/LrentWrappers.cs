using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using GameLibrary.Objekte;
using SlimDX;
using GameLibrary.IO;
using GameLibrary;
using System.Threading;
using System.ComponentModel;
using System.Reflection;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using RisenEditor.Code;
using RisenEditor.Code.Loader;
using RisenEditor.Code.Renderer;
using RisenEditor.Code.RisenTypes;

namespace RisenEditor.Code
{
    public class ILrentObject
    {
        public class ObjectTagger
        {
            public ILrentObject Object;
            public ShaderResourceTexture Icon;
            public bool AddedToList;
            public PortalRoomNode PortalRoom;
            public ObjectTagger(ILrentObject O)
            {
                Object = O;
            }
        }

        [Browsable(false)]
        public LrentFile File { get; private set; }
        [Browsable(false)]
        public List<GraphicNode> Nodes { get; private set; }
        public eCDynamicEntity Entity { get; private set; }
        [Browsable(false)]
        public List<PropertySetWrapper> SetWrappers { get; private set; }
        bool isFP;

        public unsafe ILrentObject(LrentFile f, eCDynamicEntity e)
        {
            File = f;
            Entity = e;
            Nodes = new List<GraphicNode>();
            SetWrappers = new List<PropertySetWrapper>();
            if (e == null)
                return;
            for (int i = 0; i < e.AccesorCount; i++)
            {
                PropertySetWrapper w = PropertySetWrapper.getWrapper(e[i], this);
                if (w != null)
                    SetWrappers.Add(w);
            }
            isFP = getSet<gCAIHelper_FreePoint_PS_Wrapper>() != null;
        }

        public virtual void LoadModels(API_Device D)
        {
            if (getSet<eCAudioEmitter_PS_Wrapper>() != null)
                addICON("EditorBilboard_EVT_Sound._ximg", D);
            if (getSet<gCEffect_PS_Wrapper>() != null)
                addICON("EditorBilboard_EVT_TriggerScript._ximg", D);
            if (getSet<eCStaticAmbientLight_PS_Wrapper>() != null || getSet<eCDirectionalLight_PS_Wrapper>() != null)
                addICON("EditorBilboard_PointLight._ximg", D);
            if (getSet<eCParticle_PS_Wrapper>() != null)
                addICON("Special_Billboard_Titanstorm_01_Diffuse_01._ximg", D);

            foreach (PropertySetWrapper s in SetWrappers)
                s.LoadModels(D);

            foreach (GraphicNode cNode in Nodes)
            {
                cNode.Initialize(null, Vector3.Zero, new Vector3(1), LrentImporter.DynamicNodes);
                Quaternion q = Quaternion.Identity;
                Vector3 vz1 = Vector3.Zero, vz2 = Vector3.Zero;
                Matrix m = Matrix;
                m.Decompose(out vz1, out q, out vz2);
                cNode.Orientation_LOCAL = q;
                cNode.Position_LOCAL = vz2;
                cNode.Size_LOCAL = vz1;
                if (!(cNode.Tag is ObjectTagger))
                    cNode.Tag = new ObjectTagger(this);
                else (cNode.Tag as ObjectTagger).Object = this;
            }
        }
        public void addICON(string s, API_Device D)
        {
            return;
            ShaderResourceTexture t = null;
            if (s.Contains("._xmat"))
            {
                Material m = D.Content.LoadMaterialFromFile(s);
                t = m.DiffuseTexture;
            }
            else t = new ShaderResourceTexture(s, D);
            GraphicNode GN = new GraphicNode();
            GN.Name = "ICON";
            GN.BoundingBox = new SlimDX.BoundingBox(new Vector3(-IconDrawer.halfSize), new Vector3(IconDrawer.halfSize));
            GN.Tag = new ObjectTagger(this);
            (GN.Tag as ObjectTagger).Icon = t;
            Nodes.Add(GN);
        }

        public void InvalidateModels()
        {
            if (Nodes.Count > 0)
            {
                ManagedWorld.NodeLibrary.AddToOcTree = true;
                API_Device D = Nodes[0].Device;
                foreach (GraphicNode gn in Nodes)
                    gn.Dispose();
                LoadModels(D);
                foreach(GraphicNode gn in Nodes)
                    ManagedWorld.NodeLibrary.OcTree.UpdateNode(gn);
            }
        }

        public T getSet<T>() where T : PropertySetWrapper
        {
            foreach (PropertySetWrapper p in SetWrappers)
                if (p is T)
                    return (T)p;
            return null;
        }

        public virtual Guid GUID
        {
            get
            {
                return Entity.GUID.Value;
            }

            set
            {
                Entity.GUID.Value = value;
            }
        }

        virtual public object Clone(LrentFile F, API_Device D)
        {
            eCDynamicEntity newEnt = this.Entity.Clone() as eCDynamicEntity;
            ILrentObject r = new ILrentObject(F, newEnt);
            r.LoadModels(D);
            F.addObject(r);
            RisenWorld.OnEntityAdded(r);
            return r;
        }

        virtual public BoundingBox BoundingBox
        {
            get
            {
                return GENOMEMath.fromGENOME(Entity.BoundingBox_ABS).nCalc();
            }
        }

        virtual public BoundingBox BoundingBox_LOCAL
        {
            get
            {
                return GENOMEMath.fromGENOME(Entity.BoundingBox_LOCAL).nCalc();
            }
        }

        virtual public Matrix Matrix
        {
            get
            {
                return GENOMEMath.fromGENOME(Entity.Matrix);
            }

            set
            {
                Entity.Matrix = GENOMEMath.toGENOME(value);
                foreach (Node n in Nodes)
                    n.ModelMatrix_LOCAL = value;
                foreach (PropertySetWrapper w in SetWrappers)
                    w.UpdateAfterTransformation();
                RisenWorld.OnEntityMoved(this);
            }
        }

        virtual public Vector3 Position
        {
            get
            {
                return this.Matrix.Position();
            }

            set
            {
                Vector3 v = value;
                value = GENOMEMath.toGENOME(value);
                Entity.SetPosition_ST(value);
                foreach (Node n in Nodes)
                    n.Position_LOCAL = v;
                foreach (PropertySetWrapper w in SetWrappers)
                    w.UpdateAfterTransformation();
                RisenWorld.OnEntityMoved(this);
            }
        }

        virtual public Quaternion Rotation
        {
            get
            {
                return this.Matrix.Orientation();
            }

            set
            {
                Quaternion q2 = Quaternion.RotationMatrix(GENOMEMath.fromGENOME(Matrix.RotationQuaternion(value)));
                Entity.SetRotation_ST(q2);
                foreach (Node n in Nodes)
                    n.Orientation_LOCAL = value;
                foreach (PropertySetWrapper w in SetWrappers)
                    w.UpdateAfterTransformation();
                RisenWorld.OnEntityMoved(this);
            }
        }

        virtual public Vector3 Size
        {
            get
            {
                return Matrix.Scale();
            }

            set
            {
                Entity.SetScale_ST(GENOMEMath.fromGENOME(value));
                foreach (Node n in Nodes)
                    n.Size_LOCAL = value;
                foreach (PropertySetWrapper w in SetWrappers)
                    w.UpdateAfterTransformation();
                RisenWorld.OnEntityMoved(this);
            }
        }

        virtual public string Name
        {
            get
            {
                return Entity.Name.pString;
            }

            set
            {
                Entity.Name.pString = value;
            }
        }

        //no virtual needed here because all this does is : change Position, etc
        public void Move(Vector3 v, eECoordinateSystem S)
        {
            if (S == eECoordinateSystem.eECoordinateSystem_Relative)
                v = Position + Vector3.Transform(v, Rotation).ToVec3();
            else v = Position + v;
            Position = v;
        }

        public void Rotate(Quaternion q, eECoordinateSystem S)
        {
            if (S == eECoordinateSystem.eECoordinateSystem_Independent)
                Rotation = Rotation * q;
            else Rotation = q * Rotation;
        }

        public void Scale(Vector3 s)
        {
            Vector3 q = new Vector3(s.X * Size.X, s.Y * Size.Y, s.Z * Size.Z);
            Size = q;
        }

        public bCAccessorPropertyObject getAccessor(string ClassName)
        {
            return Entity[ClassName];
        }

        public virtual void Delete()
        {
            File.removeObject(this);
            foreach (GraphicNode gn in Nodes)
            {
                gn.Visible = false;
                gn.Dispose();
            }
            foreach (PropertySetWrapper w in SetWrappers)
                w.Dispose();
            RisenWorld.OnEntityDeleted(this);
        }

        public override string ToString()
        {
            return Name;
        }

        public void RemoveSet(PropertySetWrapper w)
        {
            this.SetWrappers.Remove(w);
            Entity.RemoveAccessor(w.Set);
            w.OnRemove();
        }

        public void RemoveSet(string a_Name)
        {
            if (getAccessor(a_Name) != null)
            {
                for(int i = 0; i < SetWrappers.Count; i++)
                    if (SetWrappers[i].Set == a_Name)
                    {
                        SetWrappers[i].OnRemove();
                        SetWrappers.RemoveAt(i);
                        break;
                    }
                Entity.RemoveAccessor(a_Name);
            }
        }

        public static void MoveCameraTo(ILrentObject O)
        {
            float y = 0;
            if (O.getSet<gCNPC_PS_Wrapper>() != null)
                y = O.BoundingBox.Maximum.Y - O.Position.Y;
            Vector3 t = O.Position + new Vector3(0,y,0);
            Vector3 p = O.Matrix.Left() * 100.0f + t;
            ManagedWorld.NodeLibrary.Camera.Place(p, t);
            //ManagedWorld.NodeLibrary.Camera.Dispose();
            //ManagedWorld.NodeLibrary.Camera = new Camera(null, O.Position - new Vector3(0, 0, 300), O.Position, MathHelper.PiOver2 * 0.75f, ManagedWorld.NodeLibrary.Camera.HandleParent);
        }

        public bool IsFreePoint
        {
            get
            {
                return isFP;
            }
        }
    }

    public class LrentObjectCollection : ILrentObject
    {
        Vector3 lastGot = Vector3.Zero;
        Quaternion lastGot2 = Quaternion.Identity;
        public List<ILrentObject> Objects;

        public LrentObjectCollection()
            : base(null, null)
        {
            Objects = new List<ILrentObject>();
        }

        public LrentObjectCollection(params ILrentObject[] objs)
            : base(objs[0].File, null)
        {
            Objects = new List<ILrentObject>(objs);
            lastGot = Position;
            lastGot2 = Rotation;
        }

        public override BoundingBox BoundingBox
        {
            get
            {
                if (Objects.Count == 0)
                    return new SlimDX.BoundingBox();
                BoundingBox bb = Objects[0].BoundingBox;
                for(int i = 1; i < Objects.Count; i++)
                    bb = bb.Extend(Objects[i].BoundingBox);
                return bb;
            }
        }

        public override BoundingBox BoundingBox_LOCAL
        {
            get
            {
                if (Objects.Count == 0)
                    return new SlimDX.BoundingBox();
                BoundingBox bb = Objects[0].BoundingBox_LOCAL;
                for (int i = 1; i < Objects.Count; i++)
                    bb = bb.Extend(Objects[i].BoundingBox_LOCAL);
                return bb;
            }
        }

        public override object Clone(LrentFile F, API_Device D)
        {
            List<ILrentObject> objs = new List<ILrentObject>();
            foreach (ILrentObject o in Objects)
                if(!(o is RisenNavStick))
                    objs.Add(o.Clone(F, D) as ILrentObject);
            return new LrentObjectCollection(objs.ToArray());
        }

        public override void LoadModels(API_Device D)
        {
            foreach (ILrentObject r in Objects)
                r.LoadModels(D);
        }

        public override string Name
        {
            get
            {
                return "RisenObjectCollection";
            }
            set
            {

            }
        }

        public override Vector3 Position
        {
            get
            {
                if (Objects.Count == 0)
                    return Vector3.Zero;
                Vector3 v = new Vector3();
                foreach (ILrentObject n in Objects)
                        v += n.Position;
                lastGot = v / Objects.Count;
                return lastGot;
            }
            set
            {
                Vector3 q = value - lastGot;
                foreach (ILrentObject o in Objects)
                    if (!(o is RisenNavStick) || !Objects.Contains((o as RisenNavStick).P))
                        o.Position += q;
            }
        }

        public override Quaternion Rotation
        {
            get
            {
                Quaternion i = Quaternion.Identity;
                foreach (ILrentObject o in Objects)
                    i = Quaternion.Slerp(i, o.Rotation, 0.5f);
                lastGot2 = i;
                return i;
            }
            set
            {
                Quaternion q = value * Quaternion.Invert(lastGot2);
                foreach (ILrentObject o in Objects)
                    if (!(o is RisenNavStick) || !Objects.Contains((o as RisenNavStick).P))
                        o.Rotation *= q;
            }
        }

        public override Matrix Matrix
        {
            get
            {
                return Matrix.RotationQuaternion(Rotation) * Matrix.Translation(Position);
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public override Guid GUID
        {
            get
            {
                return Guid.Empty;
            }
            set
            {
                
            }
        }

        public override void Delete()
        {
            if(Objects != null)
                foreach (ILrentObject i in Objects)
                    i.Delete();
        }
        
        public static LrentObjectCollection CreateOrFuse(ILrentObject o)
        {
            if (o is LrentObjectCollection)
                return o as LrentObjectCollection;
            else return new LrentObjectCollection(o);
        }
    }

    public abstract class PropertySetWrapper : IDisposable
    {
        protected bCAccessorPropertyObject SET;
        protected string setName;
        protected ILrentObject Obj;

        public PropertySetWrapper()
        {
            setName = this.GetType().Name.Replace("_Wrapper", "");
        }

        public virtual void LoadModels(API_Device D)
        {

        }

        public virtual void OnRemove()
        {

        }

        public virtual void UpdateAfterTransformation()
        {
        }

        public virtual void Dispose()
        {

        }

        public override string ToString()
        {
            return setName;
        }

        protected virtual void Initialize(ILrentObject o)
        {
            Obj = o;
            SET = Obj.getAccessor(setName);
        }

        [Browsable(false)]
        public string Set
        {
            get
            {
                return setName;
            }
        }

        [Browsable(false)]
        public ILrentObject Object
        {
            get
            {
                return Obj;
            }
        }

        static List<string> done = new List<string>();
        public static PropertySetWrapper getWrapper(bCAccessorPropertyObject a, ILrentObject o)
        {
            string NAME = a.Class.ClassName;
            Type T = Assembly.GetCallingAssembly().GetType(typeof(PropertySetWrapper).Namespace + "." + NAME + "_Wrapper");
            if (T == null)
            {/*
                if (done.Contains(NAME)) return null;
                done.Add(NAME);
                string header = "//";
                StringBuilder SB = new StringBuilder();
                SB.Append("public class " + NAME + "_Wrapper : PropertySetWrapper" + Environment.NewLine + "{" + Environment.NewLine);
                SB.Append("bCPropertyAccessor SET;" + Environment.NewLine + Environment.NewLine);
                SB.Append("public " + NAME + "_Wrapper(ILrentObject O)" + Environment.NewLine + "{" + Environment.NewLine + "SET = O.getAccessor(" + (char)34 + NAME + (char)34 + ");" + Environment.NewLine + "}" + Environment.NewLine);
                foreach (bCProperty p in a.Properties.Properties)
                {
                    string X = (char)34 + p.gName.pString + (char)34;
                    Type T0 = p.Object.GetType();
                    string Y = T0.Name;
                    if (T0.IsGenericType)
                        Y = MyExtensions.GetFriendlyTypeName(T0);
                    //if (Y == "BUFFER")
                    //    return null;
                    SB.Append(Environment.NewLine + Environment.NewLine);
                    SB.Append("public " + Y + " " + p.gName + Environment.NewLine + "{" + Environment.NewLine + "get" + Environment.NewLine + "{");
                    SB.Append("return (" + Y + ")SET.PROPERTY[" + X + "].Object;");
                    SB.Append(Environment.NewLine + "}" + Environment.NewLine + "set" + Environment.NewLine + "{");
                    SB.Append("SET.PROPERTY[" + X + "].Object = value;");
                    SB.Append(Environment.NewLine + "}" + Environment.NewLine + "}");
                    if (p.Object is BinaryFileBlock && !Y.Contains("Proxy") && !Y.Contains("bCFloatColor") && !Y.Contains("String") && !Y.Contains("bCRange") && !Y.Contains("bCFloatAlphaColor")
                         && !Y.Contains("bCBox") && !Y.Contains("bCEulerAngles") && !Y.Contains("gCFlightPathBallistic") && !Y.Contains("gCFlightPathSeeking"))
                        header += ", " + Y + " " + p.gName;
                }
                SB.Append(Environment.NewLine + "}");
                if (header.Length > 2)
                    SB.Insert(0, header + Environment.NewLine);
                SB.Insert(0, File.ReadAllText("Wrapper.txt") + Environment.NewLine);
                File.WriteAllText("Wrapper.txt", SB.ToString());*/
                return null;
            }
            else
            {
                object o2 = Activator.CreateInstance(T);
                (o2 as PropertySetWrapper).Initialize(o);
                return o2 as PropertySetWrapper;
            }
        }
    }
     
    public abstract class INavBase : PropertySetWrapper
    {
        BoundingBox cachedBox;
        protected override void Initialize(ILrentObject o)
        {
            base.Initialize(o);
            cachedBox = o.BoundingBox_LOCAL;
        }
        public override void Dispose()
        {
            foreach (RisenNavStick s in getSticks())
                s.Dispose();
            base.Dispose();
        }
        public override void OnRemove()
        {
            foreach (RisenNavStick s in getSticks())
                s.Dispose();
            base.OnRemove();
        }
        public abstract void ChildMoved(RisenNavStick N);
        public abstract void ChildDeleted(RisenNavStick N);
        public abstract void setVisibilty(bool b);
        public abstract List<RisenNavStick> getSticks();

        protected void recalcBB(eCDynamicEntity Entity, List<RisenNavStick> Sticks, ILrentObject O)
        {
            BoundingBox bb = cachedBox;
            foreach (RisenNavStick n in Sticks)
            {
                bb = bb.Extend(n.offset);
                if (n.radius != -1)
                    bb = bb.Extend(n.BoundingBox_LOCAL);
            }
            BoundingBox bb2 = GENOMEMath.toGENOME(bb).nCalc();
            //Entity.LocalNodeBoundary = bb2;
            Entity.ApplyGlobalBoundingVolume(bb2);
            Entity.ApplyBoundingVolume(bb2);
            Entity.SetPosition_ST(GENOMEMath.toGENOME(Obj.Position));
            foreach (GraphicNode gn in Obj.Nodes)
                gn.BoundingBox = bb;
        }
        protected int getIndices(List<RisenNavStick> Sticks, Vector3 v/*offset*/)
        {
            float min = float.MaxValue;
            int index = 0;
            for (int i = 0; i < Sticks.Count - 1; i++)
            {
                float f = DistanceToPointSquared(v, Sticks[i].offset, Sticks[i + 1].offset);
                if (f < min)
                {
                    min = f;
                    index = i;
                }
            }
            float f2 = DistanceToPointSquared(v, Sticks[Sticks.Count - 1].offset, Sticks[0].offset);
            if (f2 < min)
            {
                min = f2;
                index = Sticks.Count - 1;
            }
            return index += 1;
        }
        protected int getIndicesP(List<RisenNavStick> Sticks, Vector3 v/*offset*/)
        {
            int i = getIndices(Sticks, v);
            if (i == 1)
            {
                Vector3 v0 = Sticks[0].offset - v;
                Vector3 v1 = Sticks[1].offset - v;
                if (Vector3.Dot(v0, v1) > 0)
                    return 0;
            }
            if (i == Sticks.Count)
            {
                Vector3 v0 = Sticks[Sticks.Count - 1].offset - v;
                Vector3 v1 = Sticks[Sticks.Count - 2].offset - v;
                if (Vector3.Dot(v0, v1) < 0)
                    return Sticks.Count - 1;
            }
            return i;
        }
        float DistanceToPointSquared(Vector3 P, Vector3 v1, Vector3 v2)
        {
          float vx = v1.X-P.X, vy = v1.Z-P.Z, ux = v2.X-v1.X, uy = v2.Z-v1.Z;
          float length = ux*ux+uy*uy;

          float det = (-vx*ux)+(-vy*uy); //if this is < 0 or > length then its outside the line segment
          if(det<0 || det>length)
          {
            ux=v2.X-P.X;
            uy=v2.Z-P.Z;
            return Math.Min(vx*vx+vy*vy, ux*ux+uy*uy);
          }

          det = ux*vy-uy*vx;
          return (det*det)/length;
        }
    }
    public class RisenNavStick : ILrentObject, IDisposable
    {
        public ILrentObject P;
        public Vector3 offset;
        public INavBase P2;
        public float radius;
        private bool ShowCircle;

        static ShaderResourceTexture TEX, TEX2;

        internal RisenNavStick(Vector3 v, ILrentObject parent, INavBase Set, float radius, bool ShowCircle)
            : base(parent.File, null)
        {
            this.ShowCircle = ShowCircle;
            P2 = Set;
            this.radius = radius;
            offset = v;
            P = parent;
        }

        public override void LoadModels(API_Device D)
        {
            if (TEX == null)
            {
                TEX = new ShaderResourceTexture(Color.Red, D);
                TEX2 = new ShaderResourceTexture(Color.DarkGreen, D);
            }
            GraphicNode gn = D.Content.LoadModelFromFile("resources/NavStick.obj", true);
            gn.Initialize(null, Vector3.Zero, new Vector3(1), LrentImporter.DynamicNodes);
            Quaternion q = Quaternion.Identity;
            Vector3 vz1 = Vector3.Zero, vz2 = Vector3.Zero;
            Matrix m = Matrix;
            m.Decompose(out vz1, out q, out vz2);
            gn.Orientation_LOCAL = q;
            gn.Position_LOCAL = vz2;
            gn.Size_LOCAL = vz1;
            gn.Mesh.Parts[0].Material = new Material(TEX);
            gn.Tag = new ILrentObject.ObjectTagger(this);
            gn.Visible = false;
            Nodes.Add(gn);

            if (radius != -1 && ShowCircle)
            {
                gn = D.Content.LoadModelFromFile("resources/circle.obj", true);
                gn.Initialize(null, Vector3.Zero, new Vector3(radius, Math.Min(radius / 10, 20), radius), LrentImporter.DynamicNodes);
                gn.Orientation_LOCAL = q;
                gn.Position_LOCAL = vz2;
                gn.Tag = new ILrentObject.ObjectTagger(this);
                gn.Mesh.Parts[0].Material = new Material(TEX2);
                gn.Visible = false;
                Nodes.Add(gn);
            }
        }

        public override BoundingBox BoundingBox
        {
            get
            {
                if (Nodes.Count == 1)
                    return Nodes[0].BoundingBox_ABS;
                else return Nodes[0].BoundingBox_ABS.Extend(Nodes[1].BoundingBox_ABS);
            }
        }

        public override BoundingBox BoundingBox_LOCAL
        {
            get
            {
                if (Nodes.Count == 1)
                    return Nodes[0].BoundingBox;
                else return Nodes[0].BoundingBox.Extend(Nodes[1].BoundingBox);
            }
        }

        public override Matrix Matrix
        {
            get
            {
                return Matrix.Translation(offset) * P.Matrix;
            }
            set
            {
                offset = (value * Matrix.Invert(P.Matrix)).Position();
                RisenWorld.OnEntityMoved(this);
            }
        }

        public override Vector3 Position
        {
            get
            {
                return Matrix.Position();
            }
            set
            {
                foreach (Node n in Nodes)
                {
                    n.Position_LOCAL = value;
                    ManagedWorld.NodeLibrary.OcTree.UpdateNode(n);
                }
                offset = (Matrix.Translation(value) * Matrix.Invert(P.Matrix)).Position();
                P2.ChildMoved(this);
                RisenWorld.OnEntityMoved(this);
            }
        }

        public override Quaternion Rotation
        {
            get
            {
                return P.Rotation;
            }
            set
            {

            }
        }

        internal void Update()
        {
            if (Nodes.Count == 0) return;
            Quaternion q = Quaternion.Identity;
            Vector3 vz1 = Vector3.Zero, vz2 = Vector3.Zero;
            Matrix m = Matrix;
            m.Decompose(out vz1, out q, out vz2);
            Nodes[0].Orientation_LOCAL = q;
            Nodes[0].Position_LOCAL = vz2;
            Nodes[0].Size_LOCAL = vz1;
            ManagedWorld.NodeLibrary.OcTree.UpdateNode(Nodes[0]);
            if (radius != -1 && ShowCircle)
            {
                Nodes[1].Orientation_LOCAL = q;
                Nodes[1].Position_LOCAL = vz2;
                Nodes[1].Size_LOCAL = new Vector3(radius, Math.Min(radius / 10, 20), radius);
                ManagedWorld.NodeLibrary.OcTree.UpdateNode(Nodes[1]);
            }
        }

        public void Dispose()
        {
            foreach (GraphicNode gn in Nodes)
                gn.Dispose();
        }

        public override object Clone(LrentFile F, API_Device D)
        {
            throw new Exception();
        }

        public override Guid GUID
        {
            get
            {
                return Guid.Empty;
            }
            set
            {
                
            }
        }

        public override string Name
        {
            get
            {
                return P + "::NavStick";
            }
            set
            {
                
            }
        }

        public override void Delete()
        {
            P2.ChildDeleted(this);
        }
    }
    public abstract class gIPath : INavBase
    {
        //this object has a plane as way and NavSticks at the ends
        //the bTObjArrays contains RISEN COORDS

        List<RisenNavStick> Sticks;
        API_Device D;
        Vector4[] m_Verties;

        protected override void Initialize(ILrentObject O)
        {
            base.Initialize(O);
            Sticks = new List<RisenNavStick>();
            bTObjArray<Vector3> Point = getPoints();
            bTObjArray<float> Radii = getRadii();
            for (int i = 0; i < Point.Length; i++)
                Sticks.Add(new RisenNavStick(GENOMEMath.fromGENOME(Point[i]), Obj, this, Radii[i], false));
        }
        
        public override void LoadModels(API_Device D)
        {
            this.D = D;
            base.LoadModels(D);
            MeshPart m2 = new MeshPart(null, null, D);
            Mesh m3 = new Mesh(Obj.BoundingBox_LOCAL, SlimDX.Direct3D11.CullMode.Back, m2);
            GraphicNode gn = new GraphicNode(string.Empty, m3, D);
            gn.Tag = new ILrentObject.ObjectTagger(Obj);
            Obj.Nodes.Add(gn);
            createBuffer();
            foreach (RisenNavStick s in Sticks)
                s.LoadModels(D);
        }

        public void createBuffer()
        {
            bTObjArray<Vector3> points = getPoints();
            bTObjArray<float> radii = getRadii();
            m_Verties = new Vector4[points.Length * 6 - 6];
            for (int i = 0; i < points.Length - 1; i++)
            {
                Vector3 v1 = GENOMEMath.fromGENOME(points[i]) + new Vector3(0, 10, 0);
                Vector3 v2 = GENOMEMath.fromGENOME(points[i + 1]) + new Vector3(0, 10, 0);
                Vector3 n = Vector3.Cross(Vector3.Normalize(v2 - v1), Vector3.UnitY);
                float r1 = radii[i];
                float r2 = radii[i + 1];
                m_Verties[6 * i + 0] = (v1 - n * r1).ToVec4(1);
                m_Verties[6 * i + 1] = (v1 + n * r1).ToVec4(1);
                m_Verties[6 * i + 2] = (v2 + n * r2).ToVec4(1);

                m_Verties[6 * i + 3] = (v2 + n * r2).ToVec4(1);
                m_Verties[6 * i + 4] = (v2 - n * r2).ToVec4(1);
                m_Verties[6 * i + 5] = (v1 - n * r1).ToVec4(1);
            }
        }

        //this will be called after a stick was moved -> calc box, update way
        public override void ChildMoved(RisenNavStick N)
        {
            recalcBB(Obj.Entity, Sticks, Obj);

            bTObjArray<Vector3> P = getPoints();
            P[Sticks.IndexOf(N)] = GENOMEMath.toGENOME(N.offset);

            createBuffer();
        }

        //this will be called after the object was moved -> move sticks, recalc way
        public override void UpdateAfterTransformation()
        {
            foreach (RisenNavStick n in Sticks)
                n.Update();
        }

        public RisenNavStick AddPoint(int index, Vector3 pos, float radius, bool show)
        {
            ManagedWorld.NodeLibrary.AddToOcTree = true;
            index = Math.Max(Math.Min(index, Sticks.Count), 0);
            getPoints().Insert(index, GENOMEMath.toGENOME(pos));
            getRadii().Insert(index, radius);
            RisenNavStick S = new RisenNavStick(pos,Obj,this,radius,false);
            S.LoadModels(D);
            Sticks.Insert(index, S);
            createBuffer();
            foreach (GraphicNode gn in S.Nodes)
            {
                gn.Visible = NavRenderer.DrawAllNav || show;
                ManagedWorld.NodeLibrary.OcTree.UpdateNode(gn);
            }
            return S;
        }

        public RisenNavStick AddPoint(Vector3 pos, float radius, bool show)
        {
            Vector3 offset = (Matrix.Translation(pos) * Matrix.Invert(Obj.Matrix)).Position();
            return AddPoint(base.getIndicesP(Sticks, offset), offset, radius, show);
        }

        public void RemovePoint(int index)
        {
            index = Math.Max(Math.Min(index, Sticks.Count), 0);
            getPoints().RemoveAt(index);
            getRadii().RemoveAt(index);
            Sticks[index].Dispose();
            Sticks.RemoveAt(index);
            createBuffer();
        }

        public void RemovePoint(RisenNavStick S)
        {
            if (Sticks.Count <= 2)
                return;
            RemovePoint(Sticks.IndexOf(S));
        }

        internal Vector4[] getBuffer()
        {
            return m_Verties;
        }

        public override void setVisibilty(bool b)
        {
            foreach (RisenNavStick s in Sticks)
                foreach (GraphicNode g in s.Nodes)
                    g.Visible = b;
        }

        public override List<RisenNavStick> getSticks()
        {
            return Sticks;
        }

        public void SetRadius(int index, float f)
        {
            Sticks[index].radius = f;
            getRadii()[index] = f;
            createBuffer();
        }

        public void SetRadius(RisenNavStick n, float f)
        {
            if (Sticks.Contains(n))
                SetRadius(Sticks.IndexOf(n), f);
        }

        public override void ChildDeleted(RisenNavStick N)
        {
            RemovePoint(N);
        }

        protected abstract bTObjArray<Vector3> getPoints();
        protected abstract bTObjArray<float> getRadii();
    }
    public abstract class gIZone : INavBase
    {
        List<RisenNavStick> Sticks;
        RisenNavStick centerStick;
        API_Device D;
        Vector4[] m_Verties;

        protected override void Initialize(ILrentObject O)
        {
            base.Initialize(O);
            Sticks = new List<RisenNavStick>();
            bTObjArray<Vector3> Point = getPoints();
            for (int i = 0; i < Point.Length; i++)
                Sticks.Add(new RisenNavStick(GENOMEMath.fromGENOME(Point[i]), Obj, this, -1, false));
            centerStick = new RisenNavStick(GENOMEMath.fromGENOME(getCenter()), Obj, this, getCenterRadius(), true);
        }

        public override void LoadModels(API_Device D)
        {
            this.D = D;
            MeshPart m2 = new MeshPart(null, null, D);
            Mesh m3 = new Mesh(Obj.BoundingBox_LOCAL, SlimDX.Direct3D11.CullMode.Back, m2);
            GraphicNode gn = new GraphicNode(string.Empty, m3, D);
            gn.Tag = new ILrentObject.ObjectTagger(Obj);
            Obj.Nodes.Add(gn);
            createBuffer();
            foreach (RisenNavStick s in Sticks)
                s.LoadModels(D);
            centerStick.LoadModels(D);
        }

        public override void ChildMoved(RisenNavStick N)
        {
            recalcBB(Obj.Entity, Sticks, Obj);

            if (Sticks.Contains(N))
            {
                bTObjArray<Vector3> P = getPoints();
                P[Sticks.IndexOf(N)] = GENOMEMath.toGENOME(N.offset);
                createBuffer();
            }
            else
            {
                setCenter(GENOMEMath.toGENOME(N.offset));
            }
            Vector3 v = GENOMEMath.fromGENOME(getCenter());
            float mR = 0;
            for (int i = 0; i < Sticks.Count; i++)
            {
                Vector3 q = Sticks[i].offset;
                float f = (float)Math.Sqrt(Math.Pow(v.X - q.X, 2) + Math.Pow(v.Z - q.Z, 2));
                mR = Math.Max(f, mR);
            }
            setCenterRadius(mR);
            centerStick.radius = mR;
            centerStick.Update();
        }

        public override void UpdateAfterTransformation()
        {
            base.UpdateAfterTransformation();
            foreach (RisenNavStick n in Sticks)
                n.Update();
            centerStick.Update();
        }

        public RisenNavStick AddPoint(int index, Vector3 pos/*offset*/, bool show)
        {
            ManagedWorld.NodeLibrary.AddToOcTree = true;
            index = Math.Max(Math.Min(index, Sticks.Count), 0);
            getPoints().Insert(index, GENOMEMath.toGENOME(pos));
            RisenNavStick S = new RisenNavStick(pos, Obj, this, -1, false);
            S.LoadModels(D);
            Sticks.Insert(index, S);
            createBuffer();
            foreach (GraphicNode gn in S.Nodes)
            {
                gn.Visible = NavRenderer.DrawAllNav || show;
                ManagedWorld.NodeLibrary.OcTree.UpdateNode(gn);
            }
            return S;
        }

        public RisenNavStick AddPoint(Vector3 pos, bool show)
        {
            Vector3 offset = (Matrix.Translation(pos) * Matrix.Invert(Obj.Matrix)).Position();
            return AddPoint(base.getIndices(Sticks, offset), offset, show);
        }

        public void RemovePoint(int index)
        {
            index = Math.Max(Math.Min(index, Sticks.Count), 0);
            getPoints().RemoveAt(index);
            Sticks[index].Dispose();
            Sticks.RemoveAt(index);
            createBuffer();
        }

        public void RemovePoint(RisenNavStick S)
        {
            if (S == centerStick || Sticks.Count <= 3)
                return;
            int i = Sticks.IndexOf(S);
            RemovePoint(i);
        }

        public void CalculateCenter()
        {
            Vector3 v = new Vector3();
            for (int i = 0; i < Sticks.Count; i++)
                v += Sticks[i].offset;
            v /= (float)Sticks.Count;
            centerStick.Position = Obj.Position + v;
        }

        internal Vector4[] getBuffer()
        {
            return m_Verties;
        }

        public void createBuffer()
        {
            bTObjArray<Vector3> P = getPoints();
            m_Verties = new Vector4[P.Length * 2];
            for (int i = 0; i < P.Length - 1; i++)
            {
                Vector3 v1 = GENOMEMath.fromGENOME(P[i + 0]) + new Vector3(0, 100, 0);
                Vector3 v2 = GENOMEMath.fromGENOME(P[i + 1]) + new Vector3(0, 100, 0);
                m_Verties[i * 2 + 0] = v1.ToVec4(1.0f);
                m_Verties[i * 2 + 1] = v2.ToVec4(1.0f);
            }
            m_Verties[m_Verties.Length - 2] = (GENOMEMath.fromGENOME(P[P.Length - 1]) + new Vector3(0, 100, 0)).ToVec4(1.0f);
            m_Verties[m_Verties.Length - 1] = m_Verties[0];
        }

        public override void setVisibilty(bool b)
        {
            foreach (RisenNavStick s in Sticks)
                foreach (GraphicNode g in s.Nodes)
                    g.Visible = b;
            foreach (GraphicNode g in centerStick.Nodes)
                g.Visible = b;
        }

        public override List<RisenNavStick> getSticks()
        {
            List<RisenNavStick> s = new List<RisenNavStick>(Sticks);
            s.Add(centerStick);
            return s;
        }

        public override void ChildDeleted(RisenNavStick N)
        {
            RemovePoint(N);
        }

        protected abstract bTObjArray<Vector3> getPoints();
        protected abstract Vector3 getCenter();
        protected abstract float getCenterRadius();
        protected abstract void setCenter(Vector3 v);
        protected abstract void setCenterRadius(float f);
    }
    public abstract class gIColl : INavBase
    {
        List<RisenNavStick> Sticks;
        API_Device D;

        protected override void Initialize(ILrentObject O)
        {
            base.Initialize(O);
            Sticks = new List<RisenNavStick>();
            bTObjArray<Vector3> Point = getPoints();
            bTObjArray<float> Radii = getRadii();
            for (int i = 0; i < Point.Length; i++)
                Sticks.Add(new RisenNavStick(GENOMEMath.fromGENOME(Point[i]), Obj, this, Radii[i], true));
        }

        public override void LoadModels(API_Device D)
        {
            this.D = D;
            foreach (RisenNavStick s in Sticks)
                s.LoadModels(D);
            foreach (RisenNavStick s in Sticks)
                foreach (GraphicNode g in s.Nodes)
                    g.Visible = false;
        }

        public override void ChildMoved(RisenNavStick N)
        {
            recalcBB(Obj.Entity, Sticks, Obj);

            bTObjArray<Vector3> P = getPoints();
            P[Sticks.IndexOf(N)] = GENOMEMath.toGENOME(N.offset);
        }

        public override void UpdateAfterTransformation()
        {
            base.UpdateAfterTransformation();
            foreach (RisenNavStick n in Sticks)
                n.Update();
        }

        public void AddCircle(Vector3 pos/*offset*/, float r, bool show)
        {
            Vector3 offset = (Matrix.Translation(pos) * Matrix.Invert(Obj.Matrix)).Position();

            getPoints().Add(GENOMEMath.toGENOME(offset));
            getRadii().Add(r);
            RisenNavStick S = new RisenNavStick(offset, Obj, this, r, true);
            S.LoadModels(D);
            Sticks.Add(S);
            foreach (GraphicNode gn in S.Nodes)
                gn.Visible = NavRenderer.DrawAllNav || show;
        }

        public void RemoveCircle(int index)
        {
            index = Math.Max(Math.Min(index, Sticks.Count), 0);
            getPoints().RemoveAt(index);
            getRadii().RemoveAt(index);
            Sticks[index].Dispose();
            Sticks.RemoveAt(index);
        }

        public void RemoveCircle(RisenNavStick S)
        {
            if(Sticks.Contains(S))
                RemoveCircle(Sticks.IndexOf(S));
        }

        public void SetRadius(RisenNavStick n, float f)
        {
            if (Sticks.Contains(n))
                SetRadius(Sticks.IndexOf(n), f);
        }

        public void SetRadius(int index, float f)
        {
            Sticks[index].radius = f;
            getRadii()[index] = f;
            if (Sticks[index].Nodes.Count == 2)
            {
                Sticks[index].Nodes[1].Size_LOCAL = new Vector3(f, Math.Min(f / 10, 20), f);
            }
        }

        public override void ChildDeleted(RisenNavStick N)
        {
            RemoveCircle(N);
        }

        public override void setVisibilty(bool b)
        {
            foreach (RisenNavStick s in Sticks)
                foreach (GraphicNode g in s.Nodes)
                    g.Visible = b;
        }

        public override List<RisenNavStick> getSticks()
        {
            return Sticks;
        }

        protected abstract bTObjArray<Vector3> getPoints();
        protected abstract bTObjArray<float> getRadii();
    }

    //PS Sets :

    //FUnctions : 
    //eCPortalRoom_PS : recalc portal data, load obj file and create room from it
    //eCOccluder_PS : recalc occluder data, load obj file and create room from it
    //gCInteraction_PS : show possible interactions, add, remove
    //eCMover_PS : show animations, add, remove
    //gCNavigation_PS : edit Routines, VisitedNavClusters, BackTrail
    //gCDialog_PS : edit skills the npc can teach (TeachSkills)
    //gCNavOffset_PS : ???
    //gCItem_PS : show Icon (IconImage), show/modify RequiredSkills and ModifySkills
    //gCRecipe_PS : show/modify skills needed (RequiredSkills) aswell as ingredients needed (CraftIngredients)
    //eCArea_StringProperty_PS : editable (PropertyNameList)
    //gCWaterZone_PS : editable (SubZones)

    //Models & reloads : 
    //gCNavigation_PS : show the trail or routine & react on changes
    //gCMapInfo_PS : (MapImage)
    //gCItem_PS : (IconImage)
    //gCWaterZone_PS

    #region SetWrapper

    #region Finished
    public class gCInventory_PS_Wrapper : PropertySetWrapper
    {
        public class Inventory
        {
            List<InventoryItem> Items;
            ILrentObject Char;
            gCInventory_PS Class;

            internal Inventory(ILrentObject C)
            {
                Char = C;
                Class = C.Entity["gCInventory_PS"].Class as gCInventory_PS;
                Items = new List<InventoryItem>();
                foreach (bCObjectBase q in Class.Stacks)
                {
                    InventoryItem t = new InventoryItem(q);
                    Items.Add(t);
                }
            }

            public InventoryItem addItem(string valueName, int Amount)
            {
                return addItem(valueName, Amount, gEStackType.gEStackType_Normal, gEEquipSlot.gEEquipSlot_None);
            }

            public InventoryItem addItem(string valueName, int Amount, gEStackType stackType, gEEquipSlot quickSlot)
            {
                Guid g = ResourceManager.getGuidFromName(valueName);
                return addItem(g, Amount, stackType, quickSlot);
            }

            public InventoryItem addItem(Guid g, int Amount, gEStackType stackType, gEEquipSlot quickSlot)
            {
                bCObjectBase b = Class.addItem(Amount, stackType, quickSlot, g);
                InventoryItem q = new InventoryItem(b);
                Items.Add(q);
                return q;
            }

            public void removeItem(InventoryItem value)
            {
                Items.Remove(value);
                Class.deleteItem(value.handle);
            }

            public bool GeneratedPlunder
            {
                get
                {
                    return (bool)Char.getAccessor("gCInventory_PS").Properties["GeneratedPlunder"].Object;
                }

                set
                {
                    Char.getAccessor("gCInventory_PS").Properties["GeneratedPlunder"].Object = value;
                }
            }

            public bool GeneratedTrade
            {
                get
                {
                    return (bool)Char.getAccessor("gCInventory_PS").Properties["GeneratedTrade"].Object;
                }

                set
                {
                    Char.getAccessor("gCInventory_PS").Properties["GeneratedTrade"].Object = value;
                }
            }

            public System.Collections.ObjectModel.ReadOnlyCollection<InventoryItem> InventoryItems
            {
                get
                {
                    return Items.AsReadOnly();
                }
            }
        }

        public class InventoryItem
        {
            internal bCObjectBase handle;

            internal InventoryItem(bCObjectBase B)
            {
                handle = B;
            }

            public string TypeName
            {
                get
                {
                    return (handle.Properties["Template"].Object as eCTemplateEntityProxy).ObjectName;
                }

                set
                {
                    Guid g = ResourceManager.getGuidFromName(value);
                    (handle.Properties["Template"].Object as eCTemplateEntityProxy).Guid = new bCGuid(g);
                }
            }

            public int Amount
            {
                get
                {
                    return (int)handle.Properties["Amount"].Object;
                }

                set
                {
                    handle.Properties["Amount"].Object = value;
                }
            }

            public int QuickSlot
            {
                get
                {
                    return (int)handle.Properties["QuickSlot"].Object;
                }

                set
                {
                    handle.Properties["QuickSlot"].Object = value;
                }
            }

            public gEStackType StackType
            {
                get
                {
                    return (gEStackType)handle.Properties["Type"].Object;
                }

                set
                {
                    handle.Properties["Type"].Object = value;
                }
            }

            public gEEquipSlot EquipSlot
            {
                get
                {
                    return (gEEquipSlot)handle.Properties["EquipSlot"].Object;
                }

                set
                {
                    handle.Properties["EquipSlot"].Object = value;
                }
            }

            public eCTemplateEntityProxy Proxy
            {
                get
                {
                    return handle.Properties["Template"].Object as eCTemplateEntityProxy;
                }
            }

            public Guid ItemGuid
            {
                get
                {
                    return Proxy.Guid;
                }
            }
        }

        public enum SlotIndices : int
        {
            Head = 10,
            Body = 11,
            Helmet = 12,
        }

        public class SlotManager
        {
            ILrentObject Char;
            gCInventory_PS Class;
            List<Slot> mSlots;

            internal SlotManager(ILrentObject C)
            {
                Char = C;
                Class = C.Entity["gCInventory_PS"].Class as gCInventory_PS;
                mSlots = new List<Slot>();
                if (Class.Slots == null)
                    return;
                foreach (KeyValuePair<int, bCObjectBase> k in Class.Slots)
                {
                    Slot s = new Slot(Class, (SlotIndices)k.Key);
                    mSlots.Add(s);
                }
            }

            public Slot addSlot(SlotIndices Index, string ObjectName)
            {
                if (Class.Slots.ContainsKey((int)Index))
                {
                    throw new Exception("It was tried to add a slot with the index : " + Index.ToString() + " ,  but there already was one present.");
                }
                bCObjectBase b = new bCObjectBase();
                Class.Slots.Add((int)Index, b);
                Slot s = new Slot(Class, Index);
                mSlots.Add(s);
                return s;
            }

            public void removeSlot(SlotIndices Index)
            {
                if (Class.Slots.ContainsKey((int)Index))
                    Class.Slots.Remove((int)Index);
            }

            public System.Collections.ObjectModel.ReadOnlyCollection<Slot> Slots
            {
                get
                {
                    return mSlots.AsReadOnly();
                }
            }

            public Slot HeadSlot
            {
                get
                {
                    foreach (Slot s in mSlots)
                        if (s.Index == SlotIndices.Head)
                            return s;
                    return null;
                }
                set
                {
                    for(int i = 0; i < mSlots.Count; i++)
                        if (mSlots[i].Index == SlotIndices.Head)
                        {
                            mSlots[i] = value;
                            return;
                        }
                    mSlots.Add(value);
                }
            }

            public Slot BodySlot
            {
                get
                {
                    foreach (Slot s in mSlots)
                        if (s.Index == SlotIndices.Body)
                            return s;
                    return null;
                }
                set
                {
                    for (int i = 0; i < mSlots.Count; i++)
                        if (mSlots[i].Index == SlotIndices.Body)
                        {
                            mSlots[i] = value;
                            return;
                        }
                    mSlots.Add(value);
                }
            }

            public Slot HelmetSlot
            {
                get
                {
                    foreach (Slot s in mSlots)
                        if (s.Index == SlotIndices.Helmet)
                            return s;
                    return null;
                }
                set
                {
                    for (int i = 0; i < mSlots.Count; i++)
                        if (mSlots[i].Index == SlotIndices.Helmet)
                        {
                            mSlots[i] = value;
                            return;
                        }
                    mSlots.Add(value);
                }
            }
        }

        public class Slot
        {
            gCInventory_PS Class;
            SlotIndices i;

            public Slot(gCInventory_PS c, SlotIndices index)
            {
                this.Class = c;
                i = index;
            }

            public string ItemName
            {
                get
                {
                    object o = Class.Slots[(int)i];
                    bCProperty p = (o as bCObjectBase).Properties[0];
                    return (p.Object as eCTemplateEntityProxy).ObjectName;
                }

                set
                {
                    Guid g = ResourceManager.getGuidFromName(value);
                    ((Class.Slots[(int)i] as bCObjectBase).Properties[0].Object as eCTemplateEntityProxy).Guid = new bCGuid(g);
                }
            }

            public SlotIndices Index
            {
                get
                {
                    return i;
                }

                set
                {
                    bCObjectBase q = (bCObjectBase)Class.Slots[(int)i];
                    Class.Slots.Remove((int)i);
                    i = value;
                    Class.Slots.Add((int)i, q);
                }
            }
        }

        class a00 : System.Drawing.Design.UITypeEditor
        {
            static ComboBox bodyBox, headBox;

            static a00()
            {
                bodyBox = new ComboBox();
                //bodyBox.Items.AddRange(ResourceManager.BodyGuids.Values.ToArray());
                foreach (KeyValuePair<string, string> k in ResourceManager.BodyGuids)
                    if (!bodyBox.Items.Contains(k.Value))
                        bodyBox.Items.Add(k.Value);
                headBox = new ComboBox();
                //headBox.Items.AddRange(ResourceManager.HeadGuids.Values.ToArray());
                foreach (KeyValuePair<string, string> k in ResourceManager.HeadGuids)
                    if (!headBox.Items.Contains(k.Value))
                        headBox.Items.Add(k.Value);
            }

            public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
            {
                IWindowsFormsEditorService edSvc = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));
                if (edSvc != null)
                {
                    if ((value as string).Contains("Head"))
                    {
                        headBox.SelectedIndex = headBox.Items.IndexOf(value);
                        edSvc.DropDownControl(headBox);
                        return headBox.SelectedItem.ToString();
                    }
                    if ((value as string).Contains("Armor"))
                    {
                        bodyBox.SelectedIndex = bodyBox.Items.IndexOf(value);
                        edSvc.DropDownControl(bodyBox);
                        return bodyBox.SelectedItem.ToString();
                    }
                }
                return "Empty";
            }

            public override System.Drawing.Design.UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
            {
                return System.Drawing.Design.UITypeEditorEditStyle.DropDown;
            }

            public override bool GetPaintValueSupported(ITypeDescriptorContext context)
            {
                return false;
            }
        }

        Inventory mInventory;
        SlotManager mSlotManager;
        ILrentObject O;
        [Browsable(false)]
        public gCInventory_PS Native
        {
            get
            {
                return (gCInventory_PS)O.getAccessor("gCInventory_PS").Class;
            }
        }

        protected override void Initialize(ILrentObject o)
        {
            base.Initialize(o);
            this.O = o;
            mInventory = new Inventory(O);
            mSlotManager = new SlotManager(O);
        }

        [Browsable(false)]
        public Inventory CharacterInventory
        {
            get
            {
                return mInventory;
            }
        }

        [Browsable(false)]
        public SlotManager CharacterSlotManager
        {
            get
            {
                return mSlotManager;
            }
        }

        public override void LoadModels(API_Device D)
        {
            List<string> ts = new List<string>();
            if (CharacterSlotManager.HeadSlot != null)
                ts.Add(CharacterSlotManager.HeadSlot.ItemName + "._xmac");
            if (CharacterSlotManager.BodySlot != null)
                ts.Add(CharacterSlotManager.BodySlot.ItemName + "._xmac");
            else
            {

            }

            if (mSlotManager.Slots.Count == 3)
                ts.Add(CharacterSlotManager.HelmetSlot.ItemName + "._xmsh");
            for (int i = 0; i < ts.Count; i++)
            {
                if (ts[i].Contains("/"))
                {
                    string[] qs = ts[i].Split("/".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                    ts[i] = qs[qs.Length - 1];
                }
            }
            foreach (string s in ts)
                if (s != "._xmac")
                    Obj.Nodes.Add(D.Content.LoadModelFromFile(s, true));
        }

        public bool GeneratedPlunder
        {
            get
            {
                return CharacterInventory.GeneratedPlunder;
            }

            set
            {
                CharacterInventory.GeneratedPlunder = value;
            }
        }

        public bool GeneratedTrade
        {
            get
            {
                return CharacterInventory.GeneratedTrade;
            }

            set
            {
                CharacterInventory.GeneratedTrade = value;
            }
        }

        [BrowsableAttribute(true)]
        [EditorAttribute(typeof(a00), typeof(System.Drawing.Design.UITypeEditor))]
        public string Head
        {
            get
            {
                if (CharacterSlotManager.HeadSlot == null)
                    return "Empty";
                return CharacterSlotManager.HeadSlot.ItemName;
            }

            set
            {
                if (CharacterSlotManager.HeadSlot == null)
                    CharacterSlotManager.HeadSlot = CharacterSlotManager.addSlot(SlotIndices.Head, value);
                else CharacterSlotManager.HeadSlot.ItemName = value;
                Obj.InvalidateModels();
            }
        }

        [BrowsableAttribute(true)]
        [EditorAttribute(typeof(a00), typeof(System.Drawing.Design.UITypeEditor))]
        public string Body
        {
            get
            {
                if (CharacterSlotManager.BodySlot == null)
                    return "Empty";
                return CharacterSlotManager.BodySlot.ItemName;
            }

            set
            {
                if (CharacterSlotManager.BodySlot == null)
                    return;
                CharacterSlotManager.BodySlot.ItemName = value;
                Obj.InvalidateModels();
            }
        }
    }
    public class gCSkills_PS_Wrapper : PropertySetWrapper
    {
        public void CopyFrom(gCSkills_PS_Wrapper a_Source)
        {
            SET.Query<gCSkills_PS>().CopyFrom(a_Source.SET.Query<gCSkills_PS>());
        }

        public int this[gESkill S]
        {
            get
            {
                return SET.Query<gCSkills_PS>().GetSkillValue(S);
            }
            set
            {
                SET.Query<gCSkills_PS>().SetSkillValue(S, value);
            }
        }

        public int gESkill_Atrib_HP
        {
            get
            {
                return SET.Query<gCSkills_PS>().GetSkillValue(gESkill.gESkill_Atrib_HP);
            }
            set
            {
                SET.Query<gCSkills_PS>().SetSkillValue(gESkill.gESkill_Atrib_HP, value);
            }
        }

        public int gESkill_Atrib_MP
        {
            get
            {
                return SET.Query<gCSkills_PS>().GetSkillValue(gESkill.gESkill_Atrib_MP);
            }
            set
            {
                SET.Query<gCSkills_PS>().SetSkillValue(gESkill.gESkill_Atrib_MP, value);
            }
        }

        public int gESkill_Stat_LV
        {
            get
            {
                return SET.Query<gCSkills_PS>().GetSkillValue(gESkill.gESkill_Stat_LV);
            }
            set
            {
                SET.Query<gCSkills_PS>().SetSkillValue(gESkill.gESkill_Stat_LV, value);
            }
        }

        public int gESkill_Stat_XP
        {
            get
            {
                return SET.Query<gCSkills_PS>().GetSkillValue(gESkill.gESkill_Stat_XP);
            }
            set
            {
                SET.Query<gCSkills_PS>().SetSkillValue(gESkill.gESkill_Stat_XP, value);
            }
        }

        public int gESkill_Stat_LP
        {
            get
            {
                return SET.Query<gCSkills_PS>().GetSkillValue(gESkill.gESkill_Stat_LP);
            }
            set
            {
                SET.Query<gCSkills_PS>().SetSkillValue(gESkill.gESkill_Stat_LP, value);
            }
        }

        public int gESkill_Stat_HP
        {
            get
            {
                return SET.Query<gCSkills_PS>().GetSkillValue(gESkill.gESkill_Stat_HP);
            }
            set
            {
                SET.Query<gCSkills_PS>().SetSkillValue(gESkill.gESkill_Stat_HP, value);
            }
        }

        public int gESkill_Stat_MP
        {
            get
            {
                return SET.Query<gCSkills_PS>().GetSkillValue(gESkill.gESkill_Stat_MP);
            }
            set
            {
                SET.Query<gCSkills_PS>().SetSkillValue(gESkill.gESkill_Stat_MP, value);
            }
        }

        public int gESkill_Stat_STR
        {
            get
            {
                return SET.Query<gCSkills_PS>().GetSkillValue(gESkill.gESkill_Stat_STR);
            }
            set
            {
                SET.Query<gCSkills_PS>().SetSkillValue(gESkill.gESkill_Stat_STR, value);
            }
        }

        public int gESkill_Stat_DEX
        {
            get
            {
                return SET.Query<gCSkills_PS>().GetSkillValue(gESkill.gESkill_Stat_DEX);
            }
            set
            {
                SET.Query<gCSkills_PS>().SetSkillValue(gESkill.gESkill_Stat_DEX, value);
            }
        }

        public int gESkill_Stat_INT
        {
            get
            {
                return SET.Query<gCSkills_PS>().GetSkillValue(gESkill.gESkill_Stat_INT);
            }
            set
            {
                SET.Query<gCSkills_PS>().SetSkillValue(gESkill.gESkill_Stat_INT, value);
            }
        }

        public int gESkill_Prot_Edge
        {
            get
            {
                return SET.Query<gCSkills_PS>().GetSkillValue(gESkill.gESkill_Prot_Edge);
            }
            set
            {
                SET.Query<gCSkills_PS>().SetSkillValue(gESkill.gESkill_Prot_Edge, value);
            }
        }

        public int gESkill_Prot_Blunt
        {
            get
            {
                return SET.Query<gCSkills_PS>().GetSkillValue(gESkill.gESkill_Prot_Blunt);
            }
            set
            {
                SET.Query<gCSkills_PS>().SetSkillValue(gESkill.gESkill_Prot_Blunt, value);
            }
        }

        public int gESkill_Prot_Point
        {
            get
            {
                return SET.Query<gCSkills_PS>().GetSkillValue(gESkill.gESkill_Prot_Point);
            }
            set
            {
                SET.Query<gCSkills_PS>().SetSkillValue(gESkill.gESkill_Prot_Point, value);
            }
        }

        public int gESkill_Prot_Fire
        {
            get
            {
                return SET.Query<gCSkills_PS>().GetSkillValue(gESkill.gESkill_Prot_Fire);
            }
            set
            {
                SET.Query<gCSkills_PS>().SetSkillValue(gESkill.gESkill_Prot_Fire, value);
            }
        }

        public int gESkill_Prot_Ice
        {
            get
            {
                return SET.Query<gCSkills_PS>().GetSkillValue(gESkill.gESkill_Prot_Ice);
            }
            set
            {
                SET.Query<gCSkills_PS>().SetSkillValue(gESkill.gESkill_Prot_Ice, value);
            }
        }

        public int gESkill_Prot_Magic
        {
            get
            {
                return SET.Query<gCSkills_PS>().GetSkillValue(gESkill.gESkill_Prot_Magic);
            }
            set
            {
                SET.Query<gCSkills_PS>().SetSkillValue(gESkill.gESkill_Prot_Magic, value);
            }
        }

        public int gESkill_Combat_Sword
        {
            get
            {
                return SET.Query<gCSkills_PS>().GetSkillValue(gESkill.gESkill_Combat_Sword);
            }
            set
            {
                SET.Query<gCSkills_PS>().SetSkillValue(gESkill.gESkill_Combat_Sword, value);
            }
        }

        public int gESkill_Combat_Axe
        {
            get
            {
                return SET.Query<gCSkills_PS>().GetSkillValue(gESkill.gESkill_Combat_Axe);
            }
            set
            {
                SET.Query<gCSkills_PS>().SetSkillValue(gESkill.gESkill_Combat_Axe, value);
            }
        }

        public int gESkill_Combat_Staff
        {
            get
            {
                return SET.Query<gCSkills_PS>().GetSkillValue(gESkill.gESkill_Combat_Staff);
            }
            set
            {
                SET.Query<gCSkills_PS>().SetSkillValue(gESkill.gESkill_Combat_Staff, value);
            }
        }

        public int gESkill_Combat_Bow
        {
            get
            {
                return SET.Query<gCSkills_PS>().GetSkillValue(gESkill.gESkill_Combat_Bow);
            }
            set
            {
                SET.Query<gCSkills_PS>().SetSkillValue(gESkill.gESkill_Combat_Bow, value);
            }
        }

        public int gESkill_Combat_CrossBow
        {
            get
            {
                return SET.Query<gCSkills_PS>().GetSkillValue(gESkill.gESkill_Combat_CrossBow);
            }
            set
            {
                SET.Query<gCSkills_PS>().SetSkillValue(gESkill.gESkill_Combat_CrossBow, value);
            }
        }

        public int gESkill_Magic_Circle
        {
            get
            {
                return SET.Query<gCSkills_PS>().GetSkillValue(gESkill.gESkill_Magic_Circle);
            }
            set
            {
                SET.Query<gCSkills_PS>().SetSkillValue(gESkill.gESkill_Magic_Circle, value);
            }
        }

        public int gESkill_Magic_Fireball
        {
            get
            {
                return SET.Query<gCSkills_PS>().GetSkillValue(gESkill.gESkill_Magic_Fireball);
            }
            set
            {
                SET.Query<gCSkills_PS>().SetSkillValue(gESkill.gESkill_Magic_Fireball, value);
            }
        }

        public int gESkill_Magic_Frost
        {
            get
            {
                return SET.Query<gCSkills_PS>().GetSkillValue(gESkill.gESkill_Magic_Frost);
            }
            set
            {
                SET.Query<gCSkills_PS>().SetSkillValue(gESkill.gESkill_Magic_Frost, value);
            }
        }

        public int gESkill_Magic_Missile
        {
            get
            {
                return SET.Query<gCSkills_PS>().GetSkillValue(gESkill.gESkill_Magic_Missile);
            }
            set
            {
                SET.Query<gCSkills_PS>().SetSkillValue(gESkill.gESkill_Magic_Missile, value);
            }
        }

        public int gESkill_Misc_Scribe
        {
            get
            {
                return SET.Query<gCSkills_PS>().GetSkillValue(gESkill.gESkill_Misc_Scribe);
            }
            set
            {
                SET.Query<gCSkills_PS>().SetSkillValue(gESkill.gESkill_Misc_Scribe, value);
            }
        }

        public int gESkill_Misc_Alchemy
        {
            get
            {
                return SET.Query<gCSkills_PS>().GetSkillValue(gESkill.gESkill_Misc_Alchemy);
            }
            set
            {
                SET.Query<gCSkills_PS>().SetSkillValue(gESkill.gESkill_Misc_Alchemy, value);
            }
        }

        public int gESkill_Misc_Smith
        {
            get
            {
                return SET.Query<gCSkills_PS>().GetSkillValue(gESkill.gESkill_Misc_Smith);
            }
            set
            {
                SET.Query<gCSkills_PS>().SetSkillValue(gESkill.gESkill_Misc_Smith, value);
            }
        }

        public int gESkill_Misc_Mining
        {
            get
            {
                return SET.Query<gCSkills_PS>().GetSkillValue(gESkill.gESkill_Misc_Mining);
            }
            set
            {
                SET.Query<gCSkills_PS>().SetSkillValue(gESkill.gESkill_Misc_Mining, value);
            }
        }

        public int gESkill_Misc_Sneak
        {
            get
            {
                return SET.Query<gCSkills_PS>().GetSkillValue(gESkill.gESkill_Misc_Sneak);
            }
            set
            {
                SET.Query<gCSkills_PS>().SetSkillValue(gESkill.gESkill_Misc_Sneak, value);
            }
        }

        public int gESkill_Misc_Lockpick
        {
            get
            {
                return SET.Query<gCSkills_PS>().GetSkillValue(gESkill.gESkill_Misc_Lockpick);
            }
            set
            {
                SET.Query<gCSkills_PS>().SetSkillValue(gESkill.gESkill_Misc_Lockpick, value);
            }
        }

        public int gESkill_Misc_Pickpocket
        {
            get
            {
                return SET.Query<gCSkills_PS>().GetSkillValue(gESkill.gESkill_Misc_Pickpocket);
            }
            set
            {
                SET.Query<gCSkills_PS>().SetSkillValue(gESkill.gESkill_Misc_Pickpocket, value);
            }
        }

        public int gESkill_Misc_Acrobat
        {
            get
            {
                return SET.Query<gCSkills_PS>().GetSkillValue(gESkill.gESkill_Misc_Acrobat);
            }
            set
            {
                SET.Query<gCSkills_PS>().SetSkillValue(gESkill.gESkill_Misc_Acrobat, value);
            }
        }

        public int gESkill_Misc_Trophy
        {
            get
            {
                return SET.Query<gCSkills_PS>().GetSkillValue(gESkill.gESkill_Misc_Trophy);
            }
            set
            {
                SET.Query<gCSkills_PS>().SetSkillValue(gESkill.gESkill_Misc_Trophy, value);
            }
        }
    }
    public class eCMesh_PS_Wrapper : PropertySetWrapper
    {
        public override void LoadModels(API_Device D)
        {
            //return;
            Obj.Nodes.Add(D.Content.LoadModelFromFile(MeshFileName.pString, true));
        }

        public bCString MeshFileName
        {
            get
            {
                return (bCString)SET.Properties["MeshFileName"].Object;
            }
            set
            {
                SET.Properties["MeshFileName"].Object = value;
                Obj.InvalidateModels();
            }
        }

        public Int32 MaterialSwitch
        {
            get
            {
                return (Int32)SET.Properties["MaterialSwitch"].Object;
            }
            set
            {
                SET.Properties["MaterialSwitch"].Object = value;
            }
        }

        public Boolean SubMeshCulling
        {
            get
            {
                return (Boolean)SET.Properties["SubMeshCulling"].Object;
            }
            set
            {
                SET.Properties["SubMeshCulling"].Object = value;
            }
        }

        public Boolean Lightmaped
        {
            get
            {
                return (Boolean)SET.Properties["Lightmaped"].Object;
            }
            set
            {
                SET.Properties["Lightmaped"].Object = value;
            }
        }

        public Boolean EnableRadiosity
        {
            get
            {
                return (Boolean)SET.Properties["EnableRadiosity"].Object;
            }
            set
            {
                SET.Properties["EnableRadiosity"].Object = value;
            }
        }

        public Int32 MaxSubMeshTriangles
        {
            get
            {
                return (Int32)SET.Properties["MaxSubMeshTriangles"].Object;
            }
            set
            {
                SET.Properties["MaxSubMeshTriangles"].Object = value;
            }
        }

        public Single UnitsPerLightmapTexel
        {
            get
            {
                return (Single)SET.Properties["UnitsPerLightmapTexel"].Object;
            }
            set
            {
                SET.Properties["UnitsPerLightmapTexel"].Object = value;
            }
        }

        public Single LevelOfDetailRange0
        {
            get
            {
                return (Single)SET.Properties["LevelOfDetailRange0"].Object;
            }
            set
            {
                SET.Properties["LevelOfDetailRange0"].Object = value;
            }
        }

        public Single LevelOfDetailRange1
        {
            get
            {
                return (Single)SET.Properties["LevelOfDetailRange1"].Object;
            }
            set
            {
                SET.Properties["LevelOfDetailRange1"].Object = value;
            }
        }

        public Single LevelOfDetailRange2
        {
            get
            {
                return (Single)SET.Properties["LevelOfDetailRange2"].Object;
            }
            set
            {
                SET.Properties["LevelOfDetailRange2"].Object = value;
            }
        }

        public Boolean EnableDecals
        {
            get
            {
                return (Boolean)SET.Properties["EnableDecals"].Object;
            }
            set
            {
                SET.Properties["EnableDecals"].Object = value;
            }
        }

        public Boolean HitByProjectile
        {
            get
            {
                return (Boolean)SET.Properties["HitByProjectile"].Object;
            }
            set
            {
                SET.Properties["HitByProjectile"].Object = value;
            }
        }
    }
    public class gCCollisionCircle_PS_Wrapper : gIColl
    {
        protected override bTObjArray<Vector3> getPoints()
        {
            return Offset;
        }
        protected override bTObjArray<float> getRadii()
        {
            return Radius;
        }

        public Boolean EnabledAsObstacle
        {
            get
            {
                return (Boolean)SET.Properties["EnabledAsObstacle"].Object;
            }
            set
            {
                SET.Properties["EnabledAsObstacle"].Object = value;
            }
        }

        public Int32 CircleCount
        {
            get
            {
                return (Int32)SET.Properties["CircleCount"].Object;
            }
            set
            {
                SET.Properties["CircleCount"].Object = value;
            }
        }

        public Single DefaultRadius
        {
            get
            {
                return (Single)SET.Properties["DefaultRadius"].Object;
            }
            set
            {
                SET.Properties["DefaultRadius"].Object = value;
            }
        }

        public bTObjArray<SlimDX.Vector3> Offset
        {
            get
            {
                return (bTObjArray<SlimDX.Vector3>)SET.Properties["Offset"].Object;
            }
            set
            {
                SET.Properties["Offset"].Object = value;
            }
        }

        public bTObjArray<System.Single> Radius
        {
            get
            {
                return (bTObjArray<System.Single>)SET.Properties["Radius"].Object;
            }
            set
            {
                SET.Properties["Radius"].Object = value;
            }
        }

        public gENavObstacleType Type
        {
            get
            {
                return (gENavObstacleType)SET.Properties["Type"].Object;
            }
            set
            {
                SET.Properties["Type"].Object = value;
            }
        }

        public bTObjArray<eCEntityProxy> ZoneEntityProxies
        {
            get
            {
                return (bTObjArray<eCEntityProxy>)SET.Properties["ZoneEntityProxies"].Object;
            }
            set
            {
                SET.Properties["ZoneEntityProxies"].Object = value;
            }
        }
    }
    public class gCNavZone_PS_Wrapper : gIZone
    {
        protected override Vector3 getCenter()
        {
            return RadiusOffset;
        }
        protected override float getCenterRadius()
        {
            return Radius;
        }
        protected override bTObjArray<Vector3> getPoints()
        {
            return Point;
        }
        protected override void setCenter(Vector3 v)
        {
            RadiusOffset = v;
        }
        protected override void setCenterRadius(float f)
        {
            Radius = f;
        }

        public bTObjArray<SlimDX.Vector3> Point
        {
            get
            {
                return (bTObjArray<SlimDX.Vector3>)SET.Properties["Point"].Object;
            }
            set
            {
                SET.Properties["Point"].Object = value;
            }
        }

        public Single Radius
        {
            get
            {
                return (Single)SET.Properties["Radius"].Object;
            }
            set
            {
                SET.Properties["Radius"].Object = value;
            }
        }

        public Vector3 RadiusOffset
        {
            get
            {
                return (Vector3)SET.Properties["RadiusOffset"].Object;
            }
            set
            {
                SET.Properties["RadiusOffset"].Object = value;
            }
        }

        public Boolean ZoneIsCCW
        {
            get
            {
                return (Boolean)SET.Properties["ZoneIsCCW"].Object;
            }
            set
            {
                SET.Properties["ZoneIsCCW"].Object = value;
            }
        }

        public Single CostFactor
        {
            get
            {
                return (Single)SET.Properties["CostFactor"].Object;
            }
            set
            {
                SET.Properties["CostFactor"].Object = value;
            }
        }

        public Single TopToleranz
        {
            get
            {
                return (Single)SET.Properties["TopToleranz"].Object;
            }
            set
            {
                SET.Properties["TopToleranz"].Object = value;
            }
        }

        public Single BottomToleranz
        {
            get
            {
                return (Single)SET.Properties["BottomToleranz"].Object;
            }
            set
            {
                SET.Properties["BottomToleranz"].Object = value;
            }
        }

        public Boolean LinkInnerArea
        {
            get
            {
                return (Boolean)SET.Properties["LinkInnerArea"].Object;
            }
            set
            {
                SET.Properties["LinkInnerArea"].Object = value;
            }
        }

        public Boolean LinkInnerTopArea
        {
            get
            {
                return (Boolean)SET.Properties["LinkInnerTopArea"].Object;
            }
            set
            {
                SET.Properties["LinkInnerTopArea"].Object = value;
            }
        }

        public Boolean LinkInnerBottomArea
        {
            get
            {
                return (Boolean)SET.Properties["LinkInnerBottomArea"].Object;
            }
            set
            {
                SET.Properties["LinkInnerBottomArea"].Object = value;
            }
        }

        public gENavTestResult NavTestResult
        {
            get
            {
                return (gENavTestResult)SET.Properties["NavTestResult"].Object;
            }
            set
            {
                SET.Properties["NavTestResult"].Object = value;
            }
        }
    }
    public class gCNegZone_PS_Wrapper : gIZone
    {
        protected override Vector3 getCenter()
        {
            return RadiusOffset;
        }
        protected override float getCenterRadius()
        {
            return Radius;
        }
        protected override bTObjArray<Vector3> getPoints()
        {
            return Point;
        }
        protected override void setCenter(Vector3 v)
        {
            RadiusOffset = v;
        }
        protected override void setCenterRadius(float f)
        {
            Radius = f;
        }

        public bTObjArray<SlimDX.Vector3> Point
        {
            get
            {
                return (bTObjArray<SlimDX.Vector3>)SET.Properties["Point"].Object;
            }
            set
            {
                SET.Properties["Point"].Object = value;
            }
        }

        public Single Radius
        {
            get
            {
                return (Single)SET.Properties["Radius"].Object;
            }
            set
            {
                SET.Properties["Radius"].Object = value;
            }
        }

        public Vector3 RadiusOffset
        {
            get
            {
                return (Vector3)SET.Properties["RadiusOffset"].Object;
            }
            set
            {
                SET.Properties["RadiusOffset"].Object = value;
            }
        }

        public Boolean ZoneIsCCW
        {
            get
            {
                return (Boolean)SET.Properties["ZoneIsCCW"].Object;
            }
            set
            {
                SET.Properties["ZoneIsCCW"].Object = value;
            }
        }

        public eCEntityProxy ZoneEntityProxy
        {
            get
            {
                return (eCEntityProxy)SET.Properties["ZoneEntityProxy"].Object;
            }
            set
            {
                SET.Properties["ZoneEntityProxy"].Object = value;
            }
        }

        public gENavTestResult NavTestResult
        {
            get
            {
                return (gENavTestResult)SET.Properties["NavTestResult"].Object;
            }
            set
            {
                SET.Properties["NavTestResult"].Object = value;
            }
        }
    }
    public class gCNavPath_PS_Wrapper : gIPath
    {
        protected override bTObjArray<Vector3> getPoints()
        {
            return Point;
        }
        protected override bTObjArray<float> getRadii()
        {
            return Radius;
        }

        public bTObjArray<SlimDX.Vector3> Point
        {
            get
            {
                return (bTObjArray<SlimDX.Vector3>)SET.Properties["Point"].Object;
            }
            set
            {
                SET.Properties["Point"].Object = value;
            }
        }

        public bTObjArray<System.Single> Radius
        {
            get
            {
                return (bTObjArray<System.Single>)SET.Properties["Radius"].Object;
            }
            set
            {
                SET.Properties["Radius"].Object = value;
            }
        }

        public Boolean UnlimitedHeight
        {
            get
            {
                return (Boolean)SET.Properties["UnlimitedHeight"].Object;
            }
            set
            {
                SET.Properties["UnlimitedHeight"].Object = value;
            }
        }

        public Boolean OnlyPlayerAsTarget
        {
            get
            {
                return (Boolean)SET.Properties["OnlyPlayerAsTarget"].Object;
            }
            set
            {
                SET.Properties["OnlyPlayerAsTarget"].Object = value;
            }
        }

        public eCEntityProxy ZoneAEntityProxy
        {
            get
            {
                return (eCEntityProxy)SET.Properties["ZoneAEntityProxy"].Object;
            }
            set
            {
                SET.Properties["ZoneAEntityProxy"].Object = value;
            }
        }

        public eCEntityProxy ZoneBEntityProxy
        {
            get
            {
                return (eCEntityProxy)SET.Properties["ZoneBEntityProxy"].Object;
            }
            set
            {
                SET.Properties["ZoneBEntityProxy"].Object = value;
            }
        }

        public Vector3 ZoneAIntersectionMargin1
        {
            get
            {
                return (Vector3)SET.Properties["ZoneAIntersectionMargin1"].Object;
            }
            set
            {
                SET.Properties["ZoneAIntersectionMargin1"].Object = value;
            }
        }

        public Vector3 ZoneAIntersectionMargin2
        {
            get
            {
                return (Vector3)SET.Properties["ZoneAIntersectionMargin2"].Object;
            }
            set
            {
                SET.Properties["ZoneAIntersectionMargin2"].Object = value;
            }
        }

        public Vector3 ZoneAIntersectionCenter
        {
            get
            {
                return (Vector3)SET.Properties["ZoneAIntersectionCenter"].Object;
            }
            set
            {
                SET.Properties["ZoneAIntersectionCenter"].Object = value;
            }
        }

        public Vector3 ZoneBIntersectionMargin1
        {
            get
            {
                return (Vector3)SET.Properties["ZoneBIntersectionMargin1"].Object;
            }
            set
            {
                SET.Properties["ZoneBIntersectionMargin1"].Object = value;
            }
        }

        public Vector3 ZoneBIntersectionMargin2
        {
            get
            {
                return (Vector3)SET.Properties["ZoneBIntersectionMargin2"].Object;
            }
            set
            {
                SET.Properties["ZoneBIntersectionMargin2"].Object = value;
            }
        }

        public Vector3 ZoneBIntersectionCenter
        {
            get
            {
                return (Vector3)SET.Properties["ZoneBIntersectionCenter"].Object;
            }
            set
            {
                SET.Properties["ZoneBIntersectionCenter"].Object = value;
            }
        }

        public Single CostFactor
        {
            get
            {
                return (Single)SET.Properties["CostFactor"].Object;
            }
            set
            {
                SET.Properties["CostFactor"].Object = value;
            }
        }

        public Single TopToleranz
        {
            get
            {
                return (Single)SET.Properties["TopToleranz"].Object;
            }
            set
            {
                SET.Properties["TopToleranz"].Object = value;
            }
        }

        public Single BottomToleranz
        {
            get
            {
                return (Single)SET.Properties["BottomToleranz"].Object;
            }
            set
            {
                SET.Properties["BottomToleranz"].Object = value;
            }
        }

        public Boolean LinkInnerArea
        {
            get
            {
                return (Boolean)SET.Properties["LinkInnerArea"].Object;
            }
            set
            {
                SET.Properties["LinkInnerArea"].Object = value;
            }
        }

        public Boolean LinkInnerTopArea
        {
            get
            {
                return (Boolean)SET.Properties["LinkInnerTopArea"].Object;
            }
            set
            {
                SET.Properties["LinkInnerTopArea"].Object = value;
            }
        }

        public Boolean LinkInnerBottomArea
        {
            get
            {
                return (Boolean)SET.Properties["LinkInnerBottomArea"].Object;
            }
            set
            {
                SET.Properties["LinkInnerBottomArea"].Object = value;
            }
        }

        public gENavTestResult NavTestResult
        {
            get
            {
                return (gENavTestResult)SET.Properties["NavTestResult"].Object;
            }
            set
            {
                SET.Properties["NavTestResult"].Object = value;
            }
        }
    }
    public class gCPrefPath_PS_Wrapper : gIPath
    {
        protected override bTObjArray<Vector3> getPoints()
        {
            return Point;
        }
        protected override bTObjArray<float> getRadii()
        {
            return PointRadius;
        }

        public bTObjArray<SlimDX.Vector3> Point
        {
            get
            {
                return (bTObjArray<SlimDX.Vector3>)SET.Properties["Point"].Object;
            }
            set
            {
                SET.Properties["Point"].Object = value;
            }
        }

        public bTObjArray<System.Single> PointRadius
        {
            get
            {
                return (bTObjArray<System.Single>)SET.Properties["PointRadius"].Object;
            }
            set
            {
                SET.Properties["PointRadius"].Object = value;
            }
        }

        public Single Radius
        {
            get
            {
                return (Single)SET.Properties["Radius"].Object;
            }
            set
            {
                SET.Properties["Radius"].Object = value;
            }
        }

        public Vector3 RadiusOffset
        {
            get
            {
                return (Vector3)SET.Properties["RadiusOffset"].Object;
            }
            set
            {
                SET.Properties["RadiusOffset"].Object = value;
            }
        }

        public eCEntityProxy ZoneEntityProxy
        {
            get
            {
                return (eCEntityProxy)SET.Properties["ZoneEntityProxy"].Object;
            }
            set
            {
                SET.Properties["ZoneEntityProxy"].Object = value;
            }
        }

        public gENavTestResult NavTestResult
        {
            get
            {
                return (gENavTestResult)SET.Properties["NavTestResult"].Object;
            }
            set
            {
                SET.Properties["NavTestResult"].Object = value;
            }
        }
    }
    public class gCLock_PS_Wrapper : PropertySetWrapper
    {
        public gELockStatus Status
        {
            get
            {
                return (gELockStatus)SET.Properties["Status"].Object;
            }
            set
            {
                SET.Properties["Status"].Object = value;
            }
        }

        public eCTemplateEntityProxy Key
        {
            get
            {
                return (eCTemplateEntityProxy)SET.Properties["Key"].Object;
            }
            set
            {
                SET.Properties["Key"].Object = value;
            }
        }

        public eCScriptProxyScript OnLockStatusChanged
        {
            get
            {
                return (eCScriptProxyScript)SET.Properties["OnLockStatusChanged"].Object;
            }
            set
            {
                SET.Properties["OnLockStatusChanged"].Object = value;
            }
        }

        public Int32 Difficulty
        {
            get
            {
                return (Int32)SET.Properties["Difficulty"].Object;
            }
            set
            {
                SET.Properties["Difficulty"].Object = value;
            }
        }

        public int[] Difficulties
        {
            get
            {
                return SET.Query<gCLock_PS>().difficultLevels;
            }
        }
    }
    public class gCDoor_PS_Wrapper : PropertySetWrapper
    {
        public gEDoorStatus Status
        {
            get
            {
                return (gEDoorStatus)SET.Properties["Status"].Object;
            }
            set
            {
                SET.Properties["Status"].Object = value;
            }
        }
    }
    public class gCBook_PS_Wrapper : PropertySetWrapper
    {
        public gCBookLocString Header
        {
            get
            {
                return (gCBookLocString)SET.Properties["Header"].Object;
            }
            set
            {
                SET.Properties["Header"].Object = value;
            }
        }

        public gCBookLocString LeftText
        {
            get
            {
                return (gCBookLocString)SET.Properties["LeftText"].Object;
            }
            set
            {
                SET.Properties["LeftText"].Object = value;
            }
        }

        public gCBookLocString RightText
        {
            get
            {
                return (gCBookLocString)SET.Properties["RightText"].Object;
            }
            set
            {
                SET.Properties["RightText"].Object = value;
            }
        }
    }
    public class gCLetter_PS_Wrapper : PropertySetWrapper
    {
        public gCLetterLocString Header
        {
            get
            {
                return (gCLetterLocString)SET.Properties["Header"].Object;
            }
            set
            {
                SET.Properties["Header"].Object = value;
            }
        }

        public gCLetterLocString Text
        {
            get
            {
                return (gCLetterLocString)SET.Properties["Text"].Object;
            }
            set
            {
                SET.Properties["Text"].Object = value;
            }
        }
    }
    #endregion

    public class gCNavigation_PS_Wrapper : PropertySetWrapper
    {
        public override void UpdateAfterTransformation()
        {
            //LastUseableNavigationZoneProxy.ID = new bCPropertyID();
            //LastUseableNavigationPosition = Obj.Position.tG();
            //foreach (gSRoutine g in Routines)
            //    g.Clear();
            //return;
            
            base.UpdateAfterTransformation();
            ILrentObject zone = null;
            float bSize = float.MaxValue;
            foreach(LrentFile f in RisenWorld.Files)
                foreach (ILrentObject f2 in f)
                {
                    if (f2.getSet<gIZone>() == null)
                        continue;
                    float size = f2.BoundingBox_LOCAL.Size().Length();
                    BoundingBox bb = f2.BoundingBox;
                    Vector3 pos = Obj.Position;
                    bool b = ((bb.Minimum.X <= pos.X) && (bb.Maximum.X >= pos.X)) && ((bb.Minimum.Z <= pos.Z) && (bb.Maximum.Z >= pos.Z));
                    if (f2.getSet<gIZone>() != null && size < bSize && b)
                    {
                        zone = f2;
                        bSize = size;
                    }
                }
            foreach (gSRoutine g in Routines)
                g.Clear();
            LastUseableNavigationPosition = Obj.Position.tG();
            if (zone == null)
                LastUseableNavigationZoneProxy.ID = new bCPropertyID();
            else
            {
                LastUseableNavigationZoneProxy.ID = new bCPropertyID(zone.GUID);
            }
        }

        public eCScriptProxyScript OnEnterWater
        {
            get
            {
                return (eCScriptProxyScript)SET.Properties["OnEnterWater"].Object;
            }
            set
            {
                SET.Properties["OnEnterWater"].Object = value;
            }
        }

        public eCScriptProxyScript OnLeaveWater
        {
            get
            {
                return (eCScriptProxyScript)SET.Properties["OnLeaveWater"].Object;
            }
            set
            {
                SET.Properties["OnLeaveWater"].Object = value;
            }
        }

        public eCScriptProxyScript OnWater
        {
            get
            {
                return (eCScriptProxyScript)SET.Properties["OnWater"].Object;
            }
            set
            {
                SET.Properties["OnWater"].Object = value;
            }
        }

        public Vector3 StartPosition
        {
            get
            {
                return (Vector3)SET.Properties["StartPosition"].Object;
            }
            set
            {
                SET.Properties["StartPosition"].Object = value;
            }
        }

        public Int32 CurrentRoutine
        {
            get
            {
                return (Int32)SET.Properties["CurrentRoutine"].Object;
            }
            set
            {
                SET.Properties["CurrentRoutine"].Object = value;
            }
        }

        public bTObjArray<gSRoutine> Routines
        {
            get
            {
                return (bTObjArray<gSRoutine>)SET.Properties["Routines"].Object;
            }
            set
            {
                SET.Properties["Routines"].Object = value;
            }
        }

        public eCEntityProxy CurrentDestinationPointProxy
        {
            get
            {
                return (eCEntityProxy)SET.Properties["CurrentDestinationPointProxy"].Object;
            }
            set
            {
                SET.Properties["CurrentDestinationPointProxy"].Object = value;
            }
        }

        public eCEntityProxy CurrentZoneEntityProxy
        {
            get
            {
                return (eCEntityProxy)SET.Properties["CurrentZoneEntityProxy"].Object;
            }
            set
            {
                SET.Properties["CurrentZoneEntityProxy"].Object = value;
            }
        }

        public eCEntityProxy LastZoneEntityProxy
        {
            get
            {
                return (eCEntityProxy)SET.Properties["LastZoneEntityProxy"].Object;
            }
            set
            {
                SET.Properties["LastZoneEntityProxy"].Object = value;
            }
        }

        public Vector3 LastUseableNavigationPosition
        {
            get
            {
                return (Vector3)SET.Properties["LastUseableNavigationPosition"].Object;
            }
            set
            {
                SET.Properties["LastUseableNavigationPosition"].Object = value;
            }
        }

        public eCEntityProxy LastUseableNavigationZoneProxy
        {
            get
            {
                return (eCEntityProxy)SET.Properties["LastUseableNavigationZoneProxy"].Object;
            }
            set
            {
                SET.Properties["LastUseableNavigationZoneProxy"].Object = value;
            }
        }

        public Boolean LastUseableNavigationZoneIsPath
        {
            get
            {
                return (Boolean)SET.Properties["LastUseableNavigationZoneIsPath"].Object;
            }
            set
            {
                SET.Properties["LastUseableNavigationZoneIsPath"].Object = value;
            }
        }

        public bTObjArray<gSVisitedNavCluster> VisitedNavClusters
        {
            get
            {
                return (bTObjArray<gSVisitedNavCluster>)SET.Properties["VisitedNavClusters"].Object;
            }
            set
            {
                SET.Properties["VisitedNavClusters"].Object = value;
            }
        }

        public bTObjArray<gSTrailElement> BackTrail
        {
            get
            {
                return (bTObjArray<gSTrailElement>)SET.Properties["BackTrail"].Object;
            }
            set
            {
                SET.Properties["BackTrail"].Object = value;
            }
        }

        public gEWalkMode GuideWalkMode
        {
            get
            {
                return (gEWalkMode)SET.Properties["GuideWalkMode"].Object;
            }
            set
            {
                SET.Properties["GuideWalkMode"].Object = value;
            }
        }
    }  
    public class gCRecipe_PS_Wrapper : PropertySetWrapper
    {
        public gERecipeCategory Craft
        {
            get
            {
                return (gERecipeCategory)SET.Properties["Craft"].Object;
            }
            set
            {
                SET.Properties["Craft"].Object = value;
            }
        }

        public bTObjArray<gCCraftIngredient> CraftIngredients
        {
            get
            {
                return (bTObjArray<gCCraftIngredient>)SET.Properties["CraftIngredients"].Object;
            }
            set
            {
                SET.Properties["CraftIngredients"].Object = value;
            }
        }

        public bTObjArray<gCSkillValue> RequiredSkills
        {
            get
            {
                return (bTObjArray<gCSkillValue>)SET.Properties["RequiredSkills"].Object;
            }
            set
            {
                SET.Properties["RequiredSkills"].Object = value;
            }
        }

        public eCTemplateEntityProxy ResultItem
        {
            get
            {
                return (eCTemplateEntityProxy)SET.Properties["ResultItem"].Object;
            }
            set
            {
                SET.Properties["ResultItem"].Object = value;
            }
        }

        public Int32 ResultAmount
        {
            get
            {
                return (Int32)SET.Properties["ResultAmount"].Object;
            }
            set
            {
                SET.Properties["ResultAmount"].Object = value;
            }
        }
    }
    public class gCDialog_PS_Wrapper : PropertySetWrapper
    {
        public Single EndDialogTimestamp
        {
            get
            {
                return (Single)SET.Properties["EndDialogTimestamp"].Object;
            }
            set
            {
                SET.Properties["EndDialogTimestamp"].Object = value;
            }
        }

        public Single SaleModifier
        {
            get
            {
                return (Single)SET.Properties["SaleModifier"].Object;
            }
            set
            {
                SET.Properties["SaleModifier"].Object = value;
            }
        }

        public Single PurchaseModifier
        {
            get
            {
                return (Single)SET.Properties["PurchaseModifier"].Object;
            }
            set
            {
                SET.Properties["PurchaseModifier"].Object = value;
            }
        }

        public Boolean TradeEnabled
        {
            get
            {
                return (Boolean)SET.Properties["TradeEnabled"].Object;
            }
            set
            {
                SET.Properties["TradeEnabled"].Object = value;
            }
        }

        public Boolean TeachEnabled
        {
            get
            {
                return (Boolean)SET.Properties["TeachEnabled"].Object;
            }
            set
            {
                SET.Properties["TeachEnabled"].Object = value;
            }
        }

        public bTObjArray<gCSkillRange> TeachSkills
        {
            get
            {
                return (bTObjArray<gCSkillRange>)SET.Properties["TeachSkills"].Object;
            }
            set
            {
                SET.Properties["TeachSkills"].Object = value;
            }
        }

        public Boolean TalkedToPlayer
        {
            get
            {
                return (Boolean)SET.Properties["TalkedToPlayer"].Object;
            }
            set
            {
                SET.Properties["TalkedToPlayer"].Object = value;
            }
        }

        public Boolean PartyEnabled
        {
            get
            {
                return (Boolean)SET.Properties["PartyEnabled"].Object;
            }
            set
            {
                SET.Properties["PartyEnabled"].Object = value;
            }
        }

        public Boolean MobEnabled
        {
            get
            {
                return (Boolean)SET.Properties["MobEnabled"].Object;
            }
            set
            {
                SET.Properties["MobEnabled"].Object = value;
            }
        }

        public Boolean SlaveryEnabled
        {
            get
            {
                return (Boolean)SET.Properties["SlaveryEnabled"].Object;
            }
            set
            {
                SET.Properties["SlaveryEnabled"].Object = value;
            }
        }

        public Boolean PickedPocket
        {
            get
            {
                return (Boolean)SET.Properties["PickedPocket"].Object;
            }
            set
            {
                SET.Properties["PickedPocket"].Object = value;
            }
        }
    }
    public class gCParty_PS_Wrapper : PropertySetWrapper
    {
        public gEPartyMemberType PartyMemberType
        {
            get
            {
                return (gEPartyMemberType)SET.Properties["PartyMemberType"].Object;
            }
            set
            {
                SET.Properties["PartyMemberType"].Object = value;
            }
        }

        public Boolean Waiting
        {
            get
            {
                return (Boolean)SET.Properties["Waiting"].Object;
            }
            set
            {
                SET.Properties["Waiting"].Object = value;
            }
        }

        public Boolean AutoRejoin
        {
            get
            {
                return (Boolean)SET.Properties["AutoRejoin"].Object;
            }
            set
            {
                SET.Properties["AutoRejoin"].Object = value;
            }
        }

        public bCString PartyArea
        {
            get
            {
                return (bCString)SET.Properties["PartyArea"].Object;
            }
            set
            {
                SET.Properties["PartyArea"].Object = value;
            }
        }

        public ILrentObject GroupLeader
        {
            get
            {
                return SET.Query<gCParty_PS>().proxy.Entity;
            }
            set
            {
                if (value != null)
                    SET.Query<gCParty_PS>().proxy.ID = new bCPropertyID(value.GUID);
            }
        }
    }
    public class gCInteraction_PS_Wrapper : PropertySetWrapper
    {
        public gEInteractionUseType UseType
        {
            get
            {
                return (gEInteractionUseType)SET.Properties["UseType"].Object;
            }
            set
            {
                SET.Properties["UseType"].Object = value;
            }
        }

        public eCScriptProxyScript EnterROIScript
        {
            get
            {
                return (eCScriptProxyScript)SET.Properties["EnterROIScript"].Object;
            }
            set
            {
                SET.Properties["EnterROIScript"].Object = value;
            }
        }

        public eCScriptProxyScript ExitROIScript
        {
            get
            {
                return (eCScriptProxyScript)SET.Properties["ExitROIScript"].Object;
            }
            set
            {
                SET.Properties["ExitROIScript"].Object = value;
            }
        }

        public eCScriptProxyScript TouchScript
        {
            get
            {
                return (eCScriptProxyScript)SET.Properties["TouchScript"].Object;
            }
            set
            {
                SET.Properties["TouchScript"].Object = value;
            }
        }

        public eCScriptProxyScript IntersectScript
        {
            get
            {
                return (eCScriptProxyScript)SET.Properties["IntersectScript"].Object;
            }
            set
            {
                SET.Properties["IntersectScript"].Object = value;
            }
        }

        public eCScriptProxyScript UntouchScript
        {
            get
            {
                return (eCScriptProxyScript)SET.Properties["UntouchScript"].Object;
            }
            set
            {
                SET.Properties["UntouchScript"].Object = value;
            }
        }

        public eCScriptProxyScript TriggerScript
        {
            get
            {
                return (eCScriptProxyScript)SET.Properties["TriggerScript"].Object;
            }
            set
            {
                SET.Properties["TriggerScript"].Object = value;
            }
        }

        public eCScriptProxyScript UntriggerScript
        {
            get
            {
                return (eCScriptProxyScript)SET.Properties["UntriggerScript"].Object;
            }
            set
            {
                SET.Properties["UntriggerScript"].Object = value;
            }
        }

        public eCScriptProxyScript DamageScript
        {
            get
            {
                return (eCScriptProxyScript)SET.Properties["DamageScript"].Object;
            }
            set
            {
                SET.Properties["DamageScript"].Object = value;
            }
        }

        public eCScriptProxyScript CanAttachSlotScript
        {
            get
            {
                return (eCScriptProxyScript)SET.Properties["CanAttachSlotScript"].Object;
            }
            set
            {
                SET.Properties["CanAttachSlotScript"].Object = value;
            }
        }

        public eCScriptProxyScript AttachedSlotScript
        {
            get
            {
                return (eCScriptProxyScript)SET.Properties["AttachedSlotScript"].Object;
            }
            set
            {
                SET.Properties["AttachedSlotScript"].Object = value;
            }
        }

        public eCScriptProxyScript DetachedSlotScript
        {
            get
            {
                return (eCScriptProxyScript)SET.Properties["DetachedSlotScript"].Object;
            }
            set
            {
                SET.Properties["DetachedSlotScript"].Object = value;
            }
        }

        public eCScriptProxyScript RoomChangedScript
        {
            get
            {
                return (eCScriptProxyScript)SET.Properties["RoomChangedScript"].Object;
            }
            set
            {
                SET.Properties["RoomChangedScript"].Object = value;
            }
        }

        public gCScriptProxyAIState RoutineTask
        {
            get
            {
                return (gCScriptProxyAIState)SET.Properties["RoutineTask"].Object;
            }
            set
            {
                SET.Properties["RoutineTask"].Object = value;
            }
        }

        public Boolean GroundBias
        {
            get
            {
                return (Boolean)SET.Properties["GroundBias"].Object;
            }
            set
            {
                SET.Properties["GroundBias"].Object = value;
            }
        }

        public gEFocusPriority FocusPriority
        {
            get
            {
                return (gEFocusPriority)SET.Properties["FocusPriority"].Object;
            }
            set
            {
                SET.Properties["FocusPriority"].Object = value;
            }
        }

        public gEFocusNameType FocusNameType
        {
            get
            {
                return (gEFocusNameType)SET.Properties["FocusNameType"].Object;
            }
            set
            {
                SET.Properties["FocusNameType"].Object = value;
            }
        }

        public bCString FocusNameBone
        {
            get
            {
                return (bCString)SET.Properties["FocusNameBone"].Object;
            }
            set
            {
                SET.Properties["FocusNameBone"].Object = value;
            }
        }

        public Vector3 FocusViewOffset
        {
            get
            {
                return (Vector3)SET.Properties["FocusViewOffset"].Object;
            }
            set
            {
                SET.Properties["FocusViewOffset"].Object = value;
            }
        }

        public Vector3 FocusWorldOffset
        {
            get
            {
                return (Vector3)SET.Properties["FocusWorldOffset"].Object;
            }
            set
            {
                SET.Properties["FocusWorldOffset"].Object = value;
            }
        }

        public eCScriptProxyScript FocusPriorityScript
        {
            get
            {
                return (eCScriptProxyScript)SET.Properties["FocusPriorityScript"].Object;
            }
            set
            {
                SET.Properties["FocusPriorityScript"].Object = value;
            }
        }

        public bTObjArray<gCInteractionSlot> Slots
        {
            get
            {
                return (bTObjArray<gCInteractionSlot>)SET.Properties["Slots"].Object;
            }
            set
            {
                SET.Properties["Slots"].Object = value;
            }
        }

        public Int32 InteractionCounter
        {
            get
            {
                return (Int32)SET.Properties["InteractionCounter"].Object;
            }
            set
            {
                SET.Properties["InteractionCounter"].Object = value;
            }
        }

        public eCEntityProxy Owner
        {
            get
            {
                return (eCEntityProxy)SET.Properties["Owner"].Object;
            }
            set
            {
                SET.Properties["Owner"].Object = value;
            }
        }

        public eCTemplateEntityProxy Spell
        {
            get
            {
                return (eCTemplateEntityProxy)SET.Properties["Spell"].Object;
            }
            set
            {
                SET.Properties["Spell"].Object = value;
            }
        }
    } 
    public class gCItem_PS_Wrapper : PropertySetWrapper
    {
        public Int32 Amount
        {
            get
            {
                return (Int32)SET.Properties["Amount"].Object;
            }
            set
            {
                SET.Properties["Amount"].Object = value;
            }
        }

        public Int32 GoldValue
        {
            get
            {
                return (Int32)SET.Properties["GoldValue"].Object;
            }
            set
            {
                SET.Properties["GoldValue"].Object = value;
            }
        }

        public Boolean MissionItem
        {
            get
            {
                return (Boolean)SET.Properties["MissionItem"].Object;
            }
            set
            {
                SET.Properties["MissionItem"].Object = value;
            }
        }

        public Boolean Permanent
        {
            get
            {
                return (Boolean)SET.Properties["Permanent"].Object;
            }
            set
            {
                SET.Properties["Permanent"].Object = value;
            }
        }

        public Int32 SortValue
        {
            get
            {
                return (Int32)SET.Properties["SortValue"].Object;
            }
            set
            {
                SET.Properties["SortValue"].Object = value;
            }
        }

        public gEItemCategory Category
        {
            get
            {
                return (gEItemCategory)SET.Properties["Category"].Object;
            }
            set
            {
                SET.Properties["Category"].Object = value;
            }
        }

        public eCGuiBitmapProxy2 IconImage
        {
            get
            {
                return (eCGuiBitmapProxy2)SET.Properties["IconImage"].Object;
            }
            set
            {
                SET.Properties["IconImage"].Object = value;
            }
        }

        public Vector3 HoldOffset
        {
            get
            {
                return (Vector3)SET.Properties["HoldOffset"].Object;
            }
            set
            {
                SET.Properties["HoldOffset"].Object = value;
            }
        }

        public Boolean Dropped
        {
            get
            {
                return (Boolean)SET.Properties["Dropped"].Object;
            }
            set
            {
                SET.Properties["Dropped"].Object = value;
            }
        }

        public eCTemplateEntityProxy ItemWorld
        {
            get
            {
                return (eCTemplateEntityProxy)SET.Properties["ItemWorld"].Object;
            }
            set
            {
                SET.Properties["ItemWorld"].Object = value;
            }
        }

        public eCTemplateEntityProxy ItemInventory
        {
            get
            {
                return (eCTemplateEntityProxy)SET.Properties["ItemInventory"].Object;
            }
            set
            {
                SET.Properties["ItemInventory"].Object = value;
            }
        }

        public eCTemplateEntityProxy Spell
        {
            get
            {
                return (eCTemplateEntityProxy)SET.Properties["Spell"].Object;
            }
            set
            {
                SET.Properties["Spell"].Object = value;
            }
        }

        public bTObjArray<gCSkillValue> RequiredSkills
        {
            get
            {
                return (bTObjArray<gCSkillValue>)SET.Properties["RequiredSkills"].Object;
            }
            set
            {
                SET.Properties["RequiredSkills"].Object = value;
            }
        }

        public bTObjArray<gCModifySkill> ModifySkills
        {
            get
            {
                return (bTObjArray<gCModifySkill>)SET.Properties["ModifySkills"].Object;
            }
            set
            {
                SET.Properties["ModifySkills"].Object = value;
            }
        }

        public gEItemUseType UseType
        {
            get
            {
                return (gEItemUseType)SET.Properties["UseType"].Object;
            }
            set
            {
                SET.Properties["UseType"].Object = value;
            }
        }

        public eCScriptProxyScript CanEquipScript
        {
            get
            {
                return (eCScriptProxyScript)SET.Properties["CanEquipScript"].Object;
            }
            set
            {
                SET.Properties["CanEquipScript"].Object = value;
            }
        }

        public eCScriptProxyScript EquipScript
        {
            get
            {
                return (eCScriptProxyScript)SET.Properties["EquipScript"].Object;
            }
            set
            {
                SET.Properties["EquipScript"].Object = value;
            }
        }

        public eCScriptProxyScript UnEquipScript
        {
            get
            {
                return (eCScriptProxyScript)SET.Properties["UnEquipScript"].Object;
            }
            set
            {
                SET.Properties["UnEquipScript"].Object = value;
            }
        }

        public bCString EffectMaterial
        {
            get
            {
                return (bCString)SET.Properties["EffectMaterial"].Object;
            }
            set
            {
                SET.Properties["EffectMaterial"].Object = value;
            }
        }

        public gEItemHoldType HoldType
        {
            get
            {
                return (gEItemHoldType)SET.Properties["HoldType"].Object;
            }
            set
            {
                SET.Properties["HoldType"].Object = value;
            }
        }

        public Boolean IsDangerousWeapon
        {
            get
            {
                return (Boolean)SET.Properties["IsDangerousWeapon"].Object;
            }
            set
            {
                SET.Properties["IsDangerousWeapon"].Object = value;
            }
        }

        public Single CombatHitRangeOffset
        {
            get
            {
                return (Single)SET.Properties["CombatHitRangeOffset"].Object;
            }
            set
            {
                SET.Properties["CombatHitRangeOffset"].Object = value;
            }
        }

        public void AddRequiredSkill(int a_Amount, gESkill a_Skill)
        {
            gCSkillValue g = new gCSkillValue();
            g.Amount = a_Amount;
            g.Skill = a_Skill;
            RequiredSkills.Add(g);
        }

        public void AddModifySkill(int a_Amount, gESkill a_Skill, gESkillModifier a_Modifier)
        {
            gCModifySkill g = new gCModifySkill();
            g.Amount = a_Amount;
            g.Skill = a_Skill;
            g.Modifier = a_Modifier;
            ModifySkills.Add(g);
        }
    }

    public class gCDynamicCollisionCircle_PS_Wrapper : PropertySetWrapper
    {
        public Single Radius
        {
            get
            {
                return (Single)SET.Properties["Radius"].Object;
            }
            set
            {
                SET.Properties["Radius"].Object = value;
            }
        }
    }
    public class eCRigidBody_PS_Wrapper : PropertySetWrapper
    {
        public Single TotalMass
        {
            get
            {
                return (Single)SET.Properties["TotalMass"].Object;
            }
            set
            {
                SET.Properties["TotalMass"].Object = value;
            }
        }

        public Vector3 MassSpaceInertia
        {
            get
            {
                return (Vector3)SET.Properties["MassSpaceInertia"].Object;
            }
            set
            {
                SET.Properties["MassSpaceInertia"].Object = value;
            }
        }

        public Vector3 StartVelocity
        {
            get
            {
                return (Vector3)SET.Properties["StartVelocity"].Object;
            }
            set
            {
                SET.Properties["StartVelocity"].Object = value;
            }
        }

        public Vector3 StartAngularVelocity
        {
            get
            {
                return (Vector3)SET.Properties["StartAngularVelocity"].Object;
            }
            set
            {
                SET.Properties["StartAngularVelocity"].Object = value;
            }
        }

        public Vector3 StartForce
        {
            get
            {
                return (Vector3)SET.Properties["StartForce"].Object;
            }
            set
            {
                SET.Properties["StartForce"].Object = value;
            }
        }

        public Vector3 StartTorque
        {
            get
            {
                return (Vector3)SET.Properties["StartTorque"].Object;
            }
            set
            {
                SET.Properties["StartTorque"].Object = value;
            }
        }

        public Single WakeUpCounter
        {
            get
            {
                return (Single)SET.Properties["WakeUpCounter"].Object;
            }
            set
            {
                SET.Properties["WakeUpCounter"].Object = value;
            }
        }

        public Single LinearDamping
        {
            get
            {
                return (Single)SET.Properties["LinearDamping"].Object;
            }
            set
            {
                SET.Properties["LinearDamping"].Object = value;
            }
        }

        public Single AngularDamping
        {
            get
            {
                return (Single)SET.Properties["AngularDamping"].Object;
            }
            set
            {
                SET.Properties["AngularDamping"].Object = value;
            }
        }

        public Single MaxAngularVelocity
        {
            get
            {
                return (Single)SET.Properties["MaxAngularVelocity"].Object;
            }
            set
            {
                SET.Properties["MaxAngularVelocity"].Object = value;
            }
        }

        public Vector3 CenterOfMass
        {
            get
            {
                return (Vector3)SET.Properties["CenterOfMass"].Object;
            }
            set
            {
                SET.Properties["CenterOfMass"].Object = value;
            }
        }

        public Single CCDMotionTreshold
        {
            get
            {
                return (Single)SET.Properties["CCDMotionTreshold"].Object;
            }
            set
            {
                SET.Properties["CCDMotionTreshold"].Object = value;
            }
        }

        public eERigidbody_Flag BodyFlag
        {
            get
            {
                return (eERigidbody_Flag)SET.Properties["BodyFlag"].Object;
            }
            set
            {
                SET.Properties["BodyFlag"].Object = value;
            }
        }

        public Boolean PhysicsEnabled
        {
            get
            {
                return (Boolean)SET.Properties["PhysicsEnabled"].Object;
            }
            set
            {
                SET.Properties["PhysicsEnabled"].Object = value;
            }
        }
    }
    public class eCCollisionShape_PS_Wrapper : PropertySetWrapper
    {
        public eECollisionGroup Group
        {
            get
            {
                return (eECollisionGroup)SET.Properties["Group"].Object;
            }
            set
            {
                SET.Properties["Group"].Object = value;
            }
        }

        public eEPhysicRangeType Range
        {
            get
            {
                return (eEPhysicRangeType)SET.Properties["Range"].Object;
            }
            set
            {
                SET.Properties["Range"].Object = value;
            }
        }

        public Boolean DisableCollision
        {
            get
            {
                return (Boolean)SET.Properties["DisableCollision"].Object;
            }
            set
            {
                SET.Properties["DisableCollision"].Object = value;
            }
        }

        public Boolean DisableResponse
        {
            get
            {
                return (Boolean)SET.Properties["DisableResponse"].Object;
            }
            set
            {
                SET.Properties["DisableResponse"].Object = value;
            }
        }

        public Boolean IgnoredByTraceRay
        {
            get
            {
                return (Boolean)SET.Properties["IgnoredByTraceRay"].Object;
            }
            set
            {
                SET.Properties["IgnoredByTraceRay"].Object = value;
            }
        }

        public Boolean IsUnique
        {
            get
            {
                return (Boolean)SET.Properties["IsUnique"].Object;
            }
            set
            {
                SET.Properties["IsUnique"].Object = value;
            }
        }

        public Boolean IsClimbable
        {
            get
            {
                return (Boolean)SET.Properties["IsClimbable"].Object;
            }
            set
            {
                SET.Properties["IsClimbable"].Object = value;
            }
        }

        public Boolean HitByProjectile
        {
            get
            {
                return (Boolean)SET.Properties["HitByProjectile"].Object;
            }
            set
            {
                SET.Properties["HitByProjectile"].Object = value;
            }
        }
    }
    public class gCCharacterMovement_PS_Wrapper : PropertySetWrapper
    {
        public Boolean DoHeightCorrection
        {
            get
            {
                return (Boolean)SET.Properties["DoHeightCorrection"].Object;
            }
            set
            {
                SET.Properties["DoHeightCorrection"].Object = value;
            }
        }

        public Single FallVelocity
        {
            get
            {
                return (Single)SET.Properties["FallVelocity"].Object;
            }
            set
            {
                SET.Properties["FallVelocity"].Object = value;
            }
        }

        public Boolean IsQuadruped
        {
            get
            {
                return (Boolean)SET.Properties["IsQuadruped"].Object;
            }
            set
            {
                SET.Properties["IsQuadruped"].Object = value;
            }
        }

        public Single QuadrupedSlopeInertia
        {
            get
            {
                return (Single)SET.Properties["QuadrupedSlopeInertia"].Object;
            }
            set
            {
                SET.Properties["QuadrupedSlopeInertia"].Object = value;
            }
        }

        public Single StepHeight
        {
            get
            {
                return (Single)SET.Properties["StepHeight"].Object;
            }
            set
            {
                SET.Properties["StepHeight"].Object = value;
            }
        }

        public Single FallDownMinGroundDist
        {
            get
            {
                return (Single)SET.Properties["FallDownMinGroundDist"].Object;
            }
            set
            {
                SET.Properties["FallDownMinGroundDist"].Object = value;
            }
        }

        public Single ForwardSpeedMax
        {
            get
            {
                return (Single)SET.Properties["ForwardSpeedMax"].Object;
            }
            set
            {
                SET.Properties["ForwardSpeedMax"].Object = value;
            }
        }

        public Single StrafeSpeedMax
        {
            get
            {
                return (Single)SET.Properties["StrafeSpeedMax"].Object;
            }
            set
            {
                SET.Properties["StrafeSpeedMax"].Object = value;
            }
        }

        public Single BackwardSpeedMax
        {
            get
            {
                return (Single)SET.Properties["BackwardSpeedMax"].Object;
            }
            set
            {
                SET.Properties["BackwardSpeedMax"].Object = value;
            }
        }

        public Single TurnSpeedMax
        {
            get
            {
                return (Single)SET.Properties["TurnSpeedMax"].Object;
            }
            set
            {
                SET.Properties["TurnSpeedMax"].Object = value;
            }
        }

        public Single TurnSpeedModifier
        {
            get
            {
                return (Single)SET.Properties["TurnSpeedModifier"].Object;
            }
            set
            {
                SET.Properties["TurnSpeedModifier"].Object = value;
            }
        }

        public Single MoveAcceleration
        {
            get
            {
                return (Single)SET.Properties["MoveAcceleration"].Object;
            }
            set
            {
                SET.Properties["MoveAcceleration"].Object = value;
            }
        }

        public Single MoveDecceleration
        {
            get
            {
                return (Single)SET.Properties["MoveDecceleration"].Object;
            }
            set
            {
                SET.Properties["MoveDecceleration"].Object = value;
            }
        }

        public Single TurnAcceleration
        {
            get
            {
                return (Single)SET.Properties["TurnAcceleration"].Object;
            }
            set
            {
                SET.Properties["TurnAcceleration"].Object = value;
            }
        }

        public Single TurnDecceleration
        {
            get
            {
                return (Single)SET.Properties["TurnDecceleration"].Object;
            }
            set
            {
                SET.Properties["TurnDecceleration"].Object = value;
            }
        }

        public Single SlowModifier
        {
            get
            {
                return (Single)SET.Properties["SlowModifier"].Object;
            }
            set
            {
                SET.Properties["SlowModifier"].Object = value;
            }
        }

        public Single FastModifier
        {
            get
            {
                return (Single)SET.Properties["FastModifier"].Object;
            }
            set
            {
                SET.Properties["FastModifier"].Object = value;
            }
        }

        public Single SneakModifier
        {
            get
            {
                return (Single)SET.Properties["SneakModifier"].Object;
            }
            set
            {
                SET.Properties["SneakModifier"].Object = value;
            }
        }

        public Single LevitationModifier
        {
            get
            {
                return (Single)SET.Properties["LevitationModifier"].Object;
            }
            set
            {
                SET.Properties["LevitationModifier"].Object = value;
            }
        }

        public Single SteepGroundAngleMin
        {
            get
            {
                return (Single)SET.Properties["SteepGroundAngleMin"].Object;
            }
            set
            {
                SET.Properties["SteepGroundAngleMin"].Object = value;
            }
        }

        public Single SteepGroundAngleMax
        {
            get
            {
                return (Single)SET.Properties["SteepGroundAngleMax"].Object;
            }
            set
            {
                SET.Properties["SteepGroundAngleMax"].Object = value;
            }
        }

        public Single WalkDownSpeedScale
        {
            get
            {
                return (Single)SET.Properties["WalkDownSpeedScale"].Object;
            }
            set
            {
                SET.Properties["WalkDownSpeedScale"].Object = value;
            }
        }

        public Single SensorAdvanceDuration
        {
            get
            {
                return (Single)SET.Properties["SensorAdvanceDuration"].Object;
            }
            set
            {
                SET.Properties["SensorAdvanceDuration"].Object = value;
            }
        }

        public Boolean SensorAffectsDirection
        {
            get
            {
                return (Boolean)SET.Properties["SensorAffectsDirection"].Object;
            }
            set
            {
                SET.Properties["SensorAffectsDirection"].Object = value;
            }
        }

        public Single SensorMinSlideAngle
        {
            get
            {
                return (Single)SET.Properties["SensorMinSlideAngle"].Object;
            }
            set
            {
                SET.Properties["SensorMinSlideAngle"].Object = value;
            }
        }

        public Single SensorInertia
        {
            get
            {
                return (Single)SET.Properties["SensorInertia"].Object;
            }
            set
            {
                SET.Properties["SensorInertia"].Object = value;
            }
        }

        public Single GroundSlopeTransInertia
        {
            get
            {
                return (Single)SET.Properties["GroundSlopeTransInertia"].Object;
            }
            set
            {
                SET.Properties["GroundSlopeTransInertia"].Object = value;
            }
        }

        public Boolean ForceGroundAlignment
        {
            get
            {
                return (Boolean)SET.Properties["ForceGroundAlignment"].Object;
            }
            set
            {
                SET.Properties["ForceGroundAlignment"].Object = value;
            }
        }

        public Single FallSteerScaleFactor
        {
            get
            {
                return (Single)SET.Properties["FallSteerScaleFactor"].Object;
            }
            set
            {
                SET.Properties["FallSteerScaleFactor"].Object = value;
            }
        }

        public Single FallXZDecceleration
        {
            get
            {
                return (Single)SET.Properties["FallXZDecceleration"].Object;
            }
            set
            {
                SET.Properties["FallXZDecceleration"].Object = value;
            }
        }

        public Single FallXZDeccelerationWarmUpTime
        {
            get
            {
                return (Single)SET.Properties["FallXZDeccelerationWarmUpTime"].Object;
            }
            set
            {
                SET.Properties["FallXZDeccelerationWarmUpTime"].Object = value;
            }
        }

        public Single DontStopFallAngleMin
        {
            get
            {
                return (Single)SET.Properties["DontStopFallAngleMin"].Object;
            }
            set
            {
                SET.Properties["DontStopFallAngleMin"].Object = value;
            }
        }

        public Single WaterWadeDepth
        {
            get
            {
                return (Single)SET.Properties["WaterWadeDepth"].Object;
            }
            set
            {
                SET.Properties["WaterWadeDepth"].Object = value;
            }
        }

        public Single WaterDeathDepth
        {
            get
            {
                return (Single)SET.Properties["WaterDeathDepth"].Object;
            }
            set
            {
                SET.Properties["WaterDeathDepth"].Object = value;
            }
        }

        public Boolean CanBePushedWhileIdle
        {
            get
            {
                return (Boolean)SET.Properties["CanBePushedWhileIdle"].Object;
            }
            set
            {
                SET.Properties["CanBePushedWhileIdle"].Object = value;
            }
        }

        public Single LastFallVelocity
        {
            get
            {
                return (Single)SET.Properties["LastFallVelocity"].Object;
            }
            set
            {
                SET.Properties["LastFallVelocity"].Object = value;
            }
        }

        public Boolean TreatWaterAsSolid
        {
            get
            {
                return (Boolean)SET.Properties["TreatWaterAsSolid"].Object;
            }
            set
            {
                SET.Properties["TreatWaterAsSolid"].Object = value;
            }
        }

        public Single ClimbHeightMin
        {
            get
            {
                return (Single)SET.Properties["ClimbHeightMin"].Object;
            }
            set
            {
                SET.Properties["ClimbHeightMin"].Object = value;
            }
        }

        public Single ClimbHeightLow
        {
            get
            {
                return (Single)SET.Properties["ClimbHeightLow"].Object;
            }
            set
            {
                SET.Properties["ClimbHeightLow"].Object = value;
            }
        }

        public Single ClimbHeightMid
        {
            get
            {
                return (Single)SET.Properties["ClimbHeightMid"].Object;
            }
            set
            {
                SET.Properties["ClimbHeightMid"].Object = value;
            }
        }

        public Single ClimbHeightHigh
        {
            get
            {
                return (Single)SET.Properties["ClimbHeightHigh"].Object;
            }
            set
            {
                SET.Properties["ClimbHeightHigh"].Object = value;
            }
        }

        public Single ClimbFrontDepth1
        {
            get
            {
                return (Single)SET.Properties["ClimbFrontDepth1"].Object;
            }
            set
            {
                SET.Properties["ClimbFrontDepth1"].Object = value;
            }
        }

        public Single ClimbFrontDepth2
        {
            get
            {
                return (Single)SET.Properties["ClimbFrontDepth2"].Object;
            }
            set
            {
                SET.Properties["ClimbFrontDepth2"].Object = value;
            }
        }

        public Single ClimbFrontDepth3
        {
            get
            {
                return (Single)SET.Properties["ClimbFrontDepth3"].Object;
            }
            set
            {
                SET.Properties["ClimbFrontDepth3"].Object = value;
            }
        }

        public Single ClimbTargetGroundAngleMax
        {
            get
            {
                return (Single)SET.Properties["ClimbTargetGroundAngleMax"].Object;
            }
            set
            {
                SET.Properties["ClimbTargetGroundAngleMax"].Object = value;
            }
        }

        public Single ClimbFlightGravity
        {
            get
            {
                return (Single)SET.Properties["ClimbFlightGravity"].Object;
            }
            set
            {
                SET.Properties["ClimbFlightGravity"].Object = value;
            }
        }

        public Single ClimbFlightForwardTime
        {
            get
            {
                return (Single)SET.Properties["ClimbFlightForwardTime"].Object;
            }
            set
            {
                SET.Properties["ClimbFlightForwardTime"].Object = value;
            }
        }

        public Single JumpHeight
        {
            get
            {
                return (Single)SET.Properties["JumpHeight"].Object;
            }
            set
            {
                SET.Properties["JumpHeight"].Object = value;
            }
        }

        public Single JumpFrontSpeedFactor
        {
            get
            {
                return (Single)SET.Properties["JumpFrontSpeedFactor"].Object;
            }
            set
            {
                SET.Properties["JumpFrontSpeedFactor"].Object = value;
            }
        }

        public Single JumpFlightGravity
        {
            get
            {
                return (Single)SET.Properties["JumpFlightGravity"].Object;
            }
            set
            {
                SET.Properties["JumpFlightGravity"].Object = value;
            }
        }

        public Single JumpTranslationCtrlFactor
        {
            get
            {
                return (Single)SET.Properties["JumpTranslationCtrlFactor"].Object;
            }
            set
            {
                SET.Properties["JumpTranslationCtrlFactor"].Object = value;
            }
        }

        public Single JumpRotationCtrlFactor
        {
            get
            {
                return (Single)SET.Properties["JumpRotationCtrlFactor"].Object;
            }
            set
            {
                SET.Properties["JumpRotationCtrlFactor"].Object = value;
            }
        }

        public Single SlideSpeed
        {
            get
            {
                return (Single)SET.Properties["SlideSpeed"].Object;
            }
            set
            {
                SET.Properties["SlideSpeed"].Object = value;
            }
        }

        public Single LevitationFallVelo
        {
            get
            {
                return (Single)SET.Properties["LevitationFallVelo"].Object;
            }
            set
            {
                SET.Properties["LevitationFallVelo"].Object = value;
            }
        }

        public Single LevitationUpVelo
        {
            get
            {
                return (Single)SET.Properties["LevitationUpVelo"].Object;
            }
            set
            {
                SET.Properties["LevitationUpVelo"].Object = value;
            }
        }

        public Single LevitationMaxUpwardMove
        {
            get
            {
                return (Single)SET.Properties["LevitationMaxUpwardMove"].Object;
            }
            set
            {
                SET.Properties["LevitationMaxUpwardMove"].Object = value;
            }
        }

        public Boolean DisableTranslation
        {
            get
            {
                return (Boolean)SET.Properties["DisableTranslation"].Object;
            }
            set
            {
                SET.Properties["DisableTranslation"].Object = value;
            }
        }

        public Boolean DisableRotation
        {
            get
            {
                return (Boolean)SET.Properties["DisableRotation"].Object;
            }
            set
            {
                SET.Properties["DisableRotation"].Object = value;
            }
        }
    }
    public class gCNPC_PS_Wrapper : PropertySetWrapper
    {
        public bCString Voice
        {
            get
            {
                return (bCString)SET.Properties["Voice"].Object;
            }
            set
            {
                SET.Properties["Voice"].Object = value;
            }
        }

        public bCString RoleDescription
        {
            get
            {
                return (bCString)SET.Properties["RoleDescription"].Object;
            }
            set
            {
                SET.Properties["RoleDescription"].Object = value;
            }
        }

        public gEGender Gender
        {
            get
            {
                return (gEGender)SET.Properties["Gender"].Object;
            }
            set
            {
                SET.Properties["Gender"].Object = value;
            }
        }

        public gEBraveryOverride BraveryOverride
        {
            get
            {
                return (gEBraveryOverride)SET.Properties["BraveryOverride"].Object;
            }
            set
            {
                SET.Properties["BraveryOverride"].Object = value;
            }
        }

        public gESpecies Species
        {
            get
            {
                return (gESpecies)SET.Properties["Species"].Object;
            }
            set
            {
                SET.Properties["Species"].Object = value;
            }
        }

        public gEAttitude AttitudeLock
        {
            get
            {
                return (gEAttitude)SET.Properties["AttitudeLock"].Object;
            }
            set
            {
                SET.Properties["AttitudeLock"].Object = value;
            }
        }

        public gECrime LastPlayerCrime
        {
            get
            {
                return (gECrime)SET.Properties["LastPlayerCrime"].Object;
            }
            set
            {
                SET.Properties["LastPlayerCrime"].Object = value;
            }
        }

        public gEComment LastPlayerComment
        {
            get
            {
                return (gEComment)SET.Properties["LastPlayerComment"].Object;
            }
            set
            {
                SET.Properties["LastPlayerComment"].Object = value;
            }
        }

        public gEReason Reason
        {
            get
            {
                return (gEReason)SET.Properties["Reason"].Object;
            }
            set
            {
                SET.Properties["Reason"].Object = value;
            }
        }

        public gEReason LastReason
        {
            get
            {
                return (gEReason)SET.Properties["LastReason"].Object;
            }
            set
            {
                SET.Properties["LastReason"].Object = value;
            }
        }

        public gEReason LastPlayerAR
        {
            get
            {
                return (gEReason)SET.Properties["LastPlayerAR"].Object;
            }
            set
            {
                SET.Properties["LastPlayerAR"].Object = value;
            }
        }

        public gEFight LastFightAgainstPlayer
        {
            get
            {
                return (gEFight)SET.Properties["LastFightAgainstPlayer"].Object;
            }
            set
            {
                SET.Properties["LastFightAgainstPlayer"].Object = value;
            }
        }

        public Single LastFightTimestamp
        {
            get
            {
                return (Single)SET.Properties["LastFightTimestamp"].Object;
            }
            set
            {
                SET.Properties["LastFightTimestamp"].Object = value;
            }
        }

        public Single PlayerWeaponTimestamp
        {
            get
            {
                return (Single)SET.Properties["PlayerWeaponTimestamp"].Object;
            }
            set
            {
                SET.Properties["PlayerWeaponTimestamp"].Object = value;
            }
        }

        public Single LastDistToTarget
        {
            get
            {
                return (Single)SET.Properties["LastDistToTarget"].Object;
            }
            set
            {
                SET.Properties["LastDistToTarget"].Object = value;
            }
        }

        public Boolean DefeatedByPlayer
        {
            get
            {
                return (Boolean)SET.Properties["DefeatedByPlayer"].Object;
            }
            set
            {
                SET.Properties["DefeatedByPlayer"].Object = value;
            }
        }

        public Boolean Ransacked
        {
            get
            {
                return (Boolean)SET.Properties["Ransacked"].Object;
            }
            set
            {
                SET.Properties["Ransacked"].Object = value;
            }
        }

        public Boolean Discovered
        {
            get
            {
                return (Boolean)SET.Properties["Discovered"].Object;
            }
            set
            {
                SET.Properties["Discovered"].Object = value;
            }
        }

        public eCEntityProxy CurrentTargetEntity
        {
            get
            {
                return (eCEntityProxy)SET.Properties["CurrentTargetEntity"].Object;
            }
            set
            {
                SET.Properties["CurrentTargetEntity"].Object = value;
            }
        }

        public eCEntityProxy CurrentAttackerEntity
        {
            get
            {
                return (eCEntityProxy)SET.Properties["CurrentAttackerEntity"].Object;
            }
            set
            {
                SET.Properties["CurrentAttackerEntity"].Object = value;
            }
        }

        public eCEntityProxy LastAttackerEntity
        {
            get
            {
                return (eCEntityProxy)SET.Properties["LastAttackerEntity"].Object;
            }
            set
            {
                SET.Properties["LastAttackerEntity"].Object = value;
            }
        }

        public eCEntityProxy GuardPoint
        {
            get
            {
                return (eCEntityProxy)SET.Properties["GuardPoint"].Object;
            }
            set
            {
                SET.Properties["GuardPoint"].Object = value;
            }
        }

        public gEGuardStatus GuardStatus
        {
            get
            {
                return (gEGuardStatus)SET.Properties["GuardStatus"].Object;
            }
            set
            {
                SET.Properties["GuardStatus"].Object = value;
            }
        }

        public Single LastDistToGuardPoint
        {
            get
            {
                return (Single)SET.Properties["LastDistToGuardPoint"].Object;
            }
            set
            {
                SET.Properties["LastDistToGuardPoint"].Object = value;
            }
        }

        public bCString AnimationBearing
        {
            get
            {
                return (bCString)SET.Properties["AnimationBearing"].Object;
            }
            set
            {
                SET.Properties["AnimationBearing"].Object = value;
            }
        }

        public eCEntityProxy SpellTarget
        {
            get
            {
                return (eCEntityProxy)SET.Properties["SpellTarget"].Object;
            }
            set
            {
                SET.Properties["SpellTarget"].Object = value;
            }
        }

        public gEDamageCalculationType DamageCalculationType
        {
            get
            {
                return (gEDamageCalculationType)SET.Properties["DamageCalculationType"].Object;
            }
            set
            {
                SET.Properties["DamageCalculationType"].Object = value;
            }
        }

        public gEGuild Guild
        {
            get
            {
                return (gEGuild)SET.Properties["Guild"].Object;
            }
            set
            {
                SET.Properties["Guild"].Object = value;
            }
        }

        public bCString Group
        {
            get
            {
                return (bCString)SET.Properties["Group"].Object;
            }
            set
            {
                SET.Properties["Group"].Object = value;
            }
        }

        public bCString EffectSpecies
        {
            get
            {
                return (bCString)SET.Properties["EffectSpecies"].Object;
            }
            set
            {
                SET.Properties["EffectSpecies"].Object = value;
            }
        }

        public bCString EffectMaterial
        {
            get
            {
                return (bCString)SET.Properties["EffectMaterial"].Object;
            }
            set
            {
                SET.Properties["EffectMaterial"].Object = value;
            }
        }
    }
    public class gCScriptRoutine_PS_Wrapper : PropertySetWrapper
    {
        public eCScriptProxyScript Routine
        {
            get
            {
                return (eCScriptProxyScript)SET.Properties["Routine"].Object;
            }
            set
            {
                SET.Properties["Routine"].Object = value;
            }
        }

        public bCString CurrentTask
        {
            get
            {
                return (bCString)SET.Properties["CurrentTask"].Object;
            }
            set
            {
                SET.Properties["CurrentTask"].Object = value;
            }
        }

        public Single TaskTime
        {
            get
            {
                return (Single)SET.Properties["TaskTime"].Object;
            }
            set
            {
                SET.Properties["TaskTime"].Object = value;
            }
        }

        public Int32 TaskPosition
        {
            get
            {
                return (Int32)SET.Properties["TaskPosition"].Object;
            }
            set
            {
                SET.Properties["TaskPosition"].Object = value;
            }
        }

        public Single StateTime
        {
            get
            {
                return (Single)SET.Properties["StateTime"].Object;
            }
            set
            {
                SET.Properties["StateTime"].Object = value;
            }
        }

        public Int32 StatePosition
        {
            get
            {
                return (Int32)SET.Properties["StatePosition"].Object;
            }
            set
            {
                SET.Properties["StatePosition"].Object = value;
            }
        }

        public Int32 CommandTime
        {
            get
            {
                return (Int32)SET.Properties["CommandTime"].Object;
            }
            set
            {
                SET.Properties["CommandTime"].Object = value;
            }
        }

        public Boolean LockAIInterrupt
        {
            get
            {
                return (Boolean)SET.Properties["LockAIInterrupt"].Object;
            }
            set
            {
                SET.Properties["LockAIInterrupt"].Object = value;
            }
        }

        public Boolean LockAIResult
        {
            get
            {
                return (Boolean)SET.Properties["LockAIResult"].Object;
            }
            set
            {
                SET.Properties["LockAIResult"].Object = value;
            }
        }

        public Int32 AIDelay
        {
            get
            {
                return (Int32)SET.Properties["AIDelay"].Object;
            }
            set
            {
                SET.Properties["AIDelay"].Object = value;
            }
        }

        public bCString LastTask
        {
            get
            {
                return (bCString)SET.Properties["LastTask"].Object;
            }
            set
            {
                SET.Properties["LastTask"].Object = value;
            }
        }

        public bCString CurrentState
        {
            get
            {
                return (bCString)SET.Properties["CurrentState"].Object;
            }
            set
            {
                SET.Properties["CurrentState"].Object = value;
            }
        }

        public Int32 CurrentBreakBlock
        {
            get
            {
                return (Int32)SET.Properties["CurrentBreakBlock"].Object;
            }
            set
            {
                SET.Properties["CurrentBreakBlock"].Object = value;
            }
        }

        public gEAniState AniState
        {
            get
            {
                return (gEAniState)SET.Properties["AniState"].Object;
            }
            set
            {
                SET.Properties["AniState"].Object = value;
            }
        }

        public gEAniState FallbackAniState
        {
            get
            {
                return (gEAniState)SET.Properties["FallbackAniState"].Object;
            }
            set
            {
                SET.Properties["FallbackAniState"].Object = value;
            }
        }

        public bCString ActionString
        {
            get
            {
                return (bCString)SET.Properties["ActionString"].Object;
            }
            set
            {
                SET.Properties["ActionString"].Object = value;
            }
        }

        public eCEntityProxy ActionTarget
        {
            get
            {
                return (eCEntityProxy)SET.Properties["ActionTarget"].Object;
            }
            set
            {
                SET.Properties["ActionTarget"].Object = value;
            }
        }

        public gEAmbientAction AmbientAction
        {
            get
            {
                return (gEAmbientAction)SET.Properties["AmbientAction"].Object;
            }
            set
            {
                SET.Properties["AmbientAction"].Object = value;
            }
        }

        public gEAIMode AIMode
        {
            get
            {
                return (gEAIMode)SET.Properties["AIMode"].Object;
            }
            set
            {
                SET.Properties["AIMode"].Object = value;
            }
        }

        public gEAIMode LastAIMode
        {
            get
            {
                return (gEAIMode)SET.Properties["LastAIMode"].Object;
            }
            set
            {
                SET.Properties["LastAIMode"].Object = value;
            }
        }

        public Single EndAttackTimestamp
        {
            get
            {
                return (Single)SET.Properties["EndAttackTimestamp"].Object;
            }
            set
            {
                SET.Properties["EndAttackTimestamp"].Object = value;
            }
        }

        public gEHitDirection HitDirection
        {
            get
            {
                return (gEHitDirection)SET.Properties["HitDirection"].Object;
            }
            set
            {
                SET.Properties["HitDirection"].Object = value;
            }
        }

        public Boolean RoutineChanged
        {
            get
            {
                return (Boolean)SET.Properties["RoutineChanged"].Object;
            }
            set
            {
                SET.Properties["RoutineChanged"].Object = value;
            }
        }
    }
    public class gCDamage_PS_Wrapper : PropertySetWrapper
    {
        public gEDamageType DamageType
        {
            get
            {
                return (gEDamageType)SET.Properties["DamageType"].Object;
            }
            set
            {
                SET.Properties["DamageType"].Object = value;
            }
        }

        public eCEntityProxy Agressor
        {
            get
            {
                return (eCEntityProxy)SET.Properties["Agressor"].Object;
            }
            set
            {
                SET.Properties["Agressor"].Object = value;
            }
        }

        public Int32 DamageBonus
        {
            get
            {
                return (Int32)SET.Properties["DamageBonus"].Object;
            }
            set
            {
                SET.Properties["DamageBonus"].Object = value;
            }
        }

        public Int32 DamageAmount
        {
            get
            {
                return (Int32)SET.Properties["DamageAmount"].Object;
            }
            set
            {
                SET.Properties["DamageAmount"].Object = value;
            }
        }

        public Single DamageMultiplier
        {
            get
            {
                return (Single)SET.Properties["DamageMultiplier"].Object;
            }
            set
            {
                SET.Properties["DamageMultiplier"].Object = value;
            }
        }

        public eCScriptProxyScript DamageScript
        {
            get
            {
                return (eCScriptProxyScript)SET.Properties["DamageScript"].Object;
            }
            set
            {
                SET.Properties["DamageScript"].Object = value;
            }
        }
    }
    public class eCIlluminated_PS_Wrapper : PropertySetWrapper
    {
        public eEStaticIlluminated StaticIlluminated
        {
            get
            {
                return (eEStaticIlluminated)SET.Properties["StaticIlluminated"].Object;
            }
            set
            {
                SET.Properties["StaticIlluminated"].Object = value;
            }
        }

        public eEShadowCasterType ShadowCasterType
        {
            get
            {
                return (eEShadowCasterType)SET.Properties["ShadowCasterType"].Object;
            }
            set
            {
                SET.Properties["ShadowCasterType"].Object = value;
            }
        }

        public Boolean CastDirLightShadows
        {
            get
            {
                return (Boolean)SET.Properties["CastDirLightShadows"].Object;
            }
            set
            {
                SET.Properties["CastDirLightShadows"].Object = value;
            }
        }

        public Boolean CastPntLightShadows
        {
            get
            {
                return (Boolean)SET.Properties["CastPntLightShadows"].Object;
            }
            set
            {
                SET.Properties["CastPntLightShadows"].Object = value;
            }
        }

        public Boolean CastStaticShadows
        {
            get
            {
                return (Boolean)SET.Properties["CastStaticShadows"].Object;
            }
            set
            {
                SET.Properties["CastStaticShadows"].Object = value;
            }
        }

        public eEBoolOverwrite CastDirLightShadowsOverwrite
        {
            get
            {
                return (eEBoolOverwrite)SET.Properties["CastDirLightShadowsOverwrite"].Object;
            }
            set
            {
                SET.Properties["CastDirLightShadowsOverwrite"].Object = value;
            }
        }

        public eEBoolOverwrite CastPntLightShadowsOverwrite
        {
            get
            {
                return (eEBoolOverwrite)SET.Properties["CastPntLightShadowsOverwrite"].Object;
            }
            set
            {
                SET.Properties["CastPntLightShadowsOverwrite"].Object = value;
            }
        }

        public eEBoolOverwrite CastStaticShadowsOverwrite
        {
            get
            {
                return (eEBoolOverwrite)SET.Properties["CastStaticShadowsOverwrite"].Object;
            }
            set
            {
                SET.Properties["CastStaticShadowsOverwrite"].Object = value;
            }
        }
    }
    public class gCEffect_PS_Wrapper : PropertySetWrapper
    {
        public bCString Effect
        {
            get
            {
                return (bCString)SET.Properties["Effect"].Object;
            }
            set
            {
                SET.Properties["Effect"].Object = value;
            }
        }

        public bCString EffectMedium
        {
            get
            {
                return (bCString)SET.Properties["EffectMedium"].Object;
            }
            set
            {
                SET.Properties["EffectMedium"].Object = value;
            }
        }

        public bCString EffectHigh
        {
            get
            {
                return (bCString)SET.Properties["EffectHigh"].Object;
            }
            set
            {
                SET.Properties["EffectHigh"].Object = value;
            }
        }

        public Vector3 Offset
        {
            get
            {
                return (Vector3)SET.Properties["Offset"].Object;
            }
            set
            {
                SET.Properties["Offset"].Object = value;
            }
        }

        public Boolean Static
        {
            get
            {
                return (Boolean)SET.Properties["Static"].Object;
            }
            set
            {
                SET.Properties["Static"].Object = value;
            }
        }

        public Boolean Enabled
        {
            get
            {
                return (Boolean)SET.Properties["Enabled"].Object;
            }
            set
            {
                SET.Properties["Enabled"].Object = value;
            }
        }

        public gEEffectTargetMode TargetMode
        {
            get
            {
                return (gEEffectTargetMode)SET.Properties["TargetMode"].Object;
            }
            set
            {
                SET.Properties["TargetMode"].Object = value;
            }
        }

        public eCScriptProxyScript TargetScript
        {
            get
            {
                return (eCScriptProxyScript)SET.Properties["TargetScript"].Object;
            }
            set
            {
                SET.Properties["TargetScript"].Object = value;
            }
        }

        public gEEffectLoopMode LoopMode
        {
            get
            {
                return (gEEffectLoopMode)SET.Properties["LoopMode"].Object;
            }
            set
            {
                SET.Properties["LoopMode"].Object = value;
            }
        }

        public gEEffectDecayMode DecayMode
        {
            get
            {
                return (gEEffectDecayMode)SET.Properties["DecayMode"].Object;
            }
            set
            {
                SET.Properties["DecayMode"].Object = value;
            }
        }

        public gEEffectStopMode StopMode
        {
            get
            {
                return (gEEffectStopMode)SET.Properties["StopMode"].Object;
            }
            set
            {
                SET.Properties["StopMode"].Object = value;
            }
        }

        public bCRange1 SecondsBetweenRepeats
        {
            get
            {
                return (bCRange1)SET.Properties["SecondsBetweenRepeats"].Object;
            }
            set
            {
                SET.Properties["SecondsBetweenRepeats"].Object = value;
            }
        }

        public bCRange1 RepeatProbability
        {
            get
            {
                return (bCRange1)SET.Properties["RepeatProbability"].Object;
            }
            set
            {
                SET.Properties["RepeatProbability"].Object = value;
            }
        }

        public Boolean UseMaxRepeats
        {
            get
            {
                return (Boolean)SET.Properties["UseMaxRepeats"].Object;
            }
            set
            {
                SET.Properties["UseMaxRepeats"].Object = value;
            }
        }

        public bCRange1 MaxNumRepeats
        {
            get
            {
                return (bCRange1)SET.Properties["MaxNumRepeats"].Object;
            }
            set
            {
                SET.Properties["MaxNumRepeats"].Object = value;
            }
        }
    }
    public class eCAnimation_PS_Wrapper : PropertySetWrapper
    {
        public override void LoadModels(API_Device D)
        {
            gCInventory_PS_Wrapper G = Obj.getSet<gCInventory_PS_Wrapper>();
            if (G == null || G.CharacterSlotManager.Slots.Count == 0)
            {
                base.Obj.Nodes.Add(D.Content.LoadModelFromFile(ResourceFilePath.pString, true));
            }
        }

        public bCString ResourceFilePath
        {
            get
            {
                return (bCString)SET.Properties["ResourceFilePath"].Object;
            }
            set
            {
                SET.Properties["ResourceFilePath"].Object = value;
                Obj.InvalidateModels();
            }
        }

        public Int32 MaterialSwitch
        {
            get
            {
                return (Int32)SET.Properties["MaterialSwitch"].Object;
            }
            set
            {
                SET.Properties["MaterialSwitch"].Object = value;
            }
        }

        public Boolean EnableRepositioning
        {
            get
            {
                return (Boolean)SET.Properties["EnableRepositioning"].Object;
            }
            set
            {
                SET.Properties["EnableRepositioning"].Object = value;
            }
        }

        public Single RagDollMass
        {
            get
            {
                return (Single)SET.Properties["RagDollMass"].Object;
            }
            set
            {
                SET.Properties["RagDollMass"].Object = value;
            }
        }
    }
    public class gCCombatSystem_PS_Wrapper : PropertySetWrapper
    {
        public class CSpec_TypeEditor : System.Drawing.Design.UITypeEditor
        {
            static ComboBox dispBox;

            static CSpec_TypeEditor()
            {
                dispBox = new ComboBox();
                dispBox.DropDownStyle = ComboBoxStyle.DropDownList; return;
                EFile F = FileManager.GetRoot("raw/library");
                F = F.GetChild("combatspecies");
                foreach (EFile f in F.Children)
                {
                    dispBox.Items.Add(f.Name);
                }
            }

            public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
            {
                ILrentObject O = (context.Instance as PropertySetWrapper).Object;
                IWindowsFormsEditorService edSvc = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));
                if (edSvc != null)
                {
                    dispBox.SelectedIndex = dispBox.Items.IndexOf(value);
                    edSvc.DropDownControl(dispBox);
                    return dispBox.SelectedItem;
                }
                return "Human";
            }

            public static void T() { }

            public override System.Drawing.Design.UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
            {
                return System.Drawing.Design.UITypeEditorEditStyle.DropDown;
            }

            public override bool GetPaintValueSupported(ITypeDescriptorContext context)
            {
                return false;
            }
        }

        [Editor(typeof(CSpec_TypeEditor), typeof(System.Drawing.Design.UITypeEditor))]
        public bCString CombatSpecies
        {
            get
            {
                return (bCString)SET.Properties["CombatSpecies"].Object;
            }
            set
            {
                SET.Properties["CombatSpecies"].Object = value;
            }
        }

        public bCString ActiveFightAI
        {
            get
            {
                return (bCString)SET.Properties["ActiveFightAI"].Object;
            }
            set
            {
                SET.Properties["ActiveFightAI"].Object = value;
            }
        }

        public bCString PassiveFightAI
        {
            get
            {
                return (bCString)SET.Properties["PassiveFightAI"].Object;
            }
            set
            {
                SET.Properties["PassiveFightAI"].Object = value;
            }
        }

        public gECombatFightAIMode FightAIMode
        {
            get
            {
                return (gECombatFightAIMode)SET.Properties["FightAIMode"].Object;
            }
            set
            {
                SET.Properties["FightAIMode"].Object = value;
            }
        }

        public eCEntityProxy ActiveAttacker
        {
            get
            {
                return (eCEntityProxy)SET.Properties["ActiveAttacker"].Object;
            }
            set
            {
                SET.Properties["ActiveAttacker"].Object = value;
            }
        }

        public bCString FightVoice
        {
            get
            {
                return (bCString)SET.Properties["FightVoice"].Object;
            }
            set
            {
                SET.Properties["FightVoice"].Object = value;
            }
        }
    }
    public class gCNavOffset_PS_Wrapper : PropertySetWrapper
    {
        public Boolean OffsetCircle
        {
            get
            {
                return (Boolean)SET.Properties["OffsetCircle"].Object;
            }
            set
            {
                SET.Properties["OffsetCircle"].Object = value;
            }
        }

        public bTObjArray<bCMotion> OffsetPose
        {
            get
            {
                return (bTObjArray<bCMotion>)SET.Properties["OffsetPose"].Object;
            }
            set
            {
                SET.Properties["OffsetPose"].Object = value;
            }
        }

        public bTObjArray<gEDirection> AniDirection
        {
            get
            {
                return (bTObjArray<gEDirection>)SET.Properties["AniDirection"].Object;
            }
            set
            {
                SET.Properties["AniDirection"].Object = value;
            }
        }

        public gENavTestResult NavTestResult
        {
            get
            {
                return (gENavTestResult)SET.Properties["NavTestResult"].Object;
            }
            set
            {
                SET.Properties["NavTestResult"].Object = value;
            }
        }
    }
    public class gCAIHelper_FreePoint_PS_Wrapper : PropertySetWrapper
    {
        public override void LoadModels(API_Device D)
        {
            Obj.Nodes.Add(D.Content.LoadModelFromFile("resources/FP.obj", true));
            base.LoadModels(D);
        }

        public gENavTestResult NavTestResult
        {
            get
            {
                return (gENavTestResult)SET.Properties["NavTestResult"].Object;
            }
            set
            {
                SET.Properties["NavTestResult"].Object = value;
            }
        }
    }
    public class gCAnchor_PS_Wrapper : PropertySetWrapper
    {
        public gEAnchorType AnchorType
        {
            get
            {
                return (gEAnchorType)SET.Properties["AnchorType"].Object;
            }
            set
            {
                SET.Properties["AnchorType"].Object = value;
            }
        }

        public Int32 MaxUsers
        {
            get
            {
                return (Int32)SET.Properties["MaxUsers"].Object;
            }
            set
            {
                SET.Properties["MaxUsers"].Object = value;
            }
        }

        public Int32 PatrolIndex
        {
            get
            {
                return (Int32)SET.Properties["PatrolIndex"].Object;
            }
            set
            {
                SET.Properties["PatrolIndex"].Object = value;
            }
        }
    }
    public class gCAIHelper_Label_PS_Wrapper : PropertySetWrapper
    {
    }
    public class eCMover_PS_Wrapper : PropertySetWrapper
    {
        public override void UpdateAfterTransformation()
        {
            base.UpdateAfterTransformation();
            (SET.Class as eCMover_PS).M = GENOMEMath.toGENOME(Obj.Matrix);
        }

        public bCString DefaultAnimation
        {
            get
            {
                return (bCString)SET.Properties["DefaultAnimation"].Object;
            }
            set
            {
                SET.Properties["DefaultAnimation"].Object = value;
            }
        }
    }
    public class gCMapInfo_PS_Wrapper : PropertySetWrapper
    {
        public eCGuiBitmapProxy2 MapImage
        {
            get
            {
                return (eCGuiBitmapProxy2)SET.Properties["MapImage"].Object;
            }
            set
            {
                SET.Properties["MapImage"].Object = value;
            }
        }

        public eCLocString ToolTip
        {
            get
            {
                return (eCLocString)SET.Properties["ToolTip"].Object;
            }
            set
            {
                SET.Properties["ToolTip"].Object = value;
            }
        }

        public Boolean CalcRotation
        {
            get
            {
                return (Boolean)SET.Properties["CalcRotation"].Object;
            }
            set
            {
                SET.Properties["CalcRotation"].Object = value;
            }
        }
    }
    public class eCAudioEmitter_PS_Wrapper : PropertySetWrapper
    {
        public bCString Sound
        {
            get
            {
                return (bCString)SET.Properties["Sound"].Object;
            }
            set
            {
                SET.Properties["Sound"].Object = value;
            }
        }

        public Boolean Enabled
        {
            get
            {
                return (Boolean)SET.Properties["Enabled"].Object;
            }
            set
            {
                SET.Properties["Enabled"].Object = value;
            }
        }

        public Boolean AutoReset
        {
            get
            {
                return (Boolean)SET.Properties["AutoReset"].Object;
            }
            set
            {
                SET.Properties["AutoReset"].Object = value;
            }
        }

        public Single Volume
        {
            get
            {
                return (Single)SET.Properties["Volume"].Object;
            }
            set
            {
                SET.Properties["Volume"].Object = value;
            }
        }

        public Single MinDistance
        {
            get
            {
                return (Single)SET.Properties["MinDistance"].Object;
            }
            set
            {
                SET.Properties["MinDistance"].Object = value;
            }
        }

        public Single MaxDistance
        {
            get
            {
                return (Single)SET.Properties["MaxDistance"].Object;
            }
            set
            {
                SET.Properties["MaxDistance"].Object = value;
            }
        }

        public eEAudioChannelFallOff FallOff
        {
            get
            {
                return (eEAudioChannelFallOff)SET.Properties["FallOff"].Object;
            }
            set
            {
                SET.Properties["FallOff"].Object = value;
            }
        }

        public bCString RollOffPreset
        {
            get
            {
                return (bCString)SET.Properties["RollOffPreset"].Object;
            }
            set
            {
                SET.Properties["RollOffPreset"].Object = value;
            }
        }

        public Boolean UseRollOffPreset
        {
            get
            {
                return (Boolean)SET.Properties["UseRollOffPreset"].Object;
            }
            set
            {
                SET.Properties["UseRollOffPreset"].Object = value;
            }
        }

        public Boolean UseVolumeFromPreset
        {
            get
            {
                return (Boolean)SET.Properties["UseVolumeFromPreset"].Object;
            }
            set
            {
                SET.Properties["UseVolumeFromPreset"].Object = value;
            }
        }

        public eEAudioEmitterMode SpawningMode
        {
            get
            {
                return (eEAudioEmitterMode)SET.Properties["SpawningMode"].Object;
            }
            set
            {
                SET.Properties["SpawningMode"].Object = value;
            }
        }

        public bCRange1 SecondsBetweenRepeats
        {
            get
            {
                return (bCRange1)SET.Properties["SecondsBetweenRepeats"].Object;
            }
            set
            {
                SET.Properties["SecondsBetweenRepeats"].Object = value;
            }
        }

        public bCRange1 RepeatProbability
        {
            get
            {
                return (bCRange1)SET.Properties["RepeatProbability"].Object;
            }
            set
            {
                SET.Properties["RepeatProbability"].Object = value;
            }
        }

        public Boolean UseMaxRepeats
        {
            get
            {
                return (Boolean)SET.Properties["UseMaxRepeats"].Object;
            }
            set
            {
                SET.Properties["UseMaxRepeats"].Object = value;
            }
        }

        public bCRange1 MaxNumRepeats
        {
            get
            {
                return (bCRange1)SET.Properties["MaxNumRepeats"].Object;
            }
            set
            {
                SET.Properties["MaxNumRepeats"].Object = value;
            }
        }

        public eEAudioEmitterShape Shape
        {
            get
            {
                return (eEAudioEmitterShape)SET.Properties["Shape"].Object;
            }
            set
            {
                SET.Properties["Shape"].Object = value;
            }
        }

        public bCBox BoxShape
        {
            get
            {
                return (bCBox)SET.Properties["BoxShape"].Object;
            }
            set
            {
                SET.Properties["BoxShape"].Object = value;
            }
        }

        public bCRange1 Spread
        {
            get
            {
                return (bCRange1)SET.Properties["Spread"].Object;
            }
            set
            {
                SET.Properties["Spread"].Object = value;
            }
        }
    }
    public class eCDecal_PS_Wrapper : PropertySetWrapper
    {
        public override void LoadModels(API_Device D)
        {
             Obj.addICON(ImageOrMaterial.pString, D);
        }

        public bCImageOrMaterialResourceString ImageOrMaterial
        {
            get
            {
                return (bCImageOrMaterialResourceString)SET.Properties["ImageOrMaterial"].Object;
            }
            set
            {
                SET.Properties["ImageOrMaterial"].Object = value;
                Obj.InvalidateModels();
            }
        }

        public Single LifeTime
        {
            get
            {
                return (Single)SET.Properties["LifeTime"].Object;
            }
            set
            {
                SET.Properties["LifeTime"].Object = value;
            }
        }

        public Vector3 Size
        {
            get
            {
                return (Vector3)SET.Properties["Size"].Object;
            }
            set
            {
                SET.Properties["Size"].Object = value;
            }
        }

        public Vector3 Offset
        {
            get
            {
                return (Vector3)SET.Properties["Offset"].Object;
            }
            set
            {
                SET.Properties["Offset"].Object = value;
            }
        }

        public Boolean UseEntityDirection
        {
            get
            {
                return (Boolean)SET.Properties["UseEntityDirection"].Object;
            }
            set
            {
                SET.Properties["UseEntityDirection"].Object = value;
            }
        }

        public bCEulerAngles DirectionOffset
        {
            get
            {
                return (bCEulerAngles)SET.Properties["DirectionOffset"].Object;
            }
            set
            {
                SET.Properties["DirectionOffset"].Object = value;
            }
        }

        public Single FadeInTime
        {
            get
            {
                return (Single)SET.Properties["FadeInTime"].Object;
            }
            set
            {
                SET.Properties["FadeInTime"].Object = value;
            }
        }

        public Single FadeOutTime
        {
            get
            {
                return (Single)SET.Properties["FadeOutTime"].Object;
            }
            set
            {
                SET.Properties["FadeOutTime"].Object = value;
            }
        }

        public Boolean CreateOnDynamicEntities
        {
            get
            {
                return (Boolean)SET.Properties["CreateOnDynamicEntities"].Object;
            }
            set
            {
                SET.Properties["CreateOnDynamicEntities"].Object = value;
            }
        }

        public Int32 TileCountX
        {
            get
            {
                return (Int32)SET.Properties["TileCountX"].Object;
            }
            set
            {
                SET.Properties["TileCountX"].Object = value;
            }
        }

        public Int32 TileCountY
        {
            get
            {
                return (Int32)SET.Properties["TileCountY"].Object;
            }
            set
            {
                SET.Properties["TileCountY"].Object = value;
            }
        }

        public Int32 TileIndex
        {
            get
            {
                return (Int32)SET.Properties["TileIndex"].Object;
            }
            set
            {
                SET.Properties["TileIndex"].Object = value;
            }
        }

        public Boolean RandomTile
        {
            get
            {
                return (Boolean)SET.Properties["RandomTile"].Object;
            }
            set
            {
                SET.Properties["RandomTile"].Object = value;
            }
        }

        public Single BackfaceThreshold
        {
            get
            {
                return (Single)SET.Properties["BackfaceThreshold"].Object;
            }
            set
            {
                SET.Properties["BackfaceThreshold"].Object = value;
            }
        }
    }
    public class eCBillboard_PS_Wrapper : PropertySetWrapper
    {
        public bCImageOrMaterialResourceString ImageOrMaterial
        {
            get
            {
                return (bCImageOrMaterialResourceString)SET.Properties["ImageOrMaterial"].Object;
            }
            set
            {
                SET.Properties["ImageOrMaterial"].Object = value;
                Obj.InvalidateModels();
            }
        }

        public Int32 BillboardCount
        {
            get
            {
                return (Int32)SET.Properties["BillboardCount"].Object;
            }
            set
            {
                SET.Properties["BillboardCount"].Object = value;
            }
        }

        public Boolean UniformSizeVariation
        {
            get
            {
                return (Boolean)SET.Properties["UniformSizeVariation"].Object;
            }
            set
            {
                SET.Properties["UniformSizeVariation"].Object = value;
            }
        }

        public Vector2 Size
        {
            get
            {
                return (Vector2)SET.Properties["Size"].Object;
            }
            set
            {
                SET.Properties["Size"].Object = value;
            }
        }

        public Vector2 SizeVariation
        {
            get
            {
                return (Vector2)SET.Properties["SizeVariation"].Object;
            }
            set
            {
                SET.Properties["SizeVariation"].Object = value;
            }
        }

        public Vector2 Center
        {
            get
            {
                return (Vector2)SET.Properties["Center"].Object;
            }
            set
            {
                SET.Properties["Center"].Object = value;
            }
        }

        public Vector2 CenterVariation
        {
            get
            {
                return (Vector2)SET.Properties["CenterVariation"].Object;
            }
            set
            {
                SET.Properties["CenterVariation"].Object = value;
            }
        }

        public Vector3 Offset
        {
            get
            {
                return (Vector3)SET.Properties["Offset"].Object;
            }
            set
            {
                SET.Properties["Offset"].Object = value;
            }
        }

        public Vector3 OffsetVariation
        {
            get
            {
                return (Vector3)SET.Properties["OffsetVariation"].Object;
            }
            set
            {
                SET.Properties["OffsetVariation"].Object = value;
            }
        }

        public Single Rotation
        {
            get
            {
                return (Single)SET.Properties["Rotation"].Object;
            }
            set
            {
                SET.Properties["Rotation"].Object = value;
            }
        }

        public Single RotationVariation
        {
            get
            {
                return (Single)SET.Properties["RotationVariation"].Object;
            }
            set
            {
                SET.Properties["RotationVariation"].Object = value;
            }
        }

        public Int32 TileCountX
        {
            get
            {
                return (Int32)SET.Properties["TileCountX"].Object;
            }
            set
            {
                SET.Properties["TileCountX"].Object = value;
            }
        }

        public Int32 TileCountY
        {
            get
            {
                return (Int32)SET.Properties["TileCountY"].Object;
            }
            set
            {
                SET.Properties["TileCountY"].Object = value;
            }
        }

        public Int32 TileIndex
        {
            get
            {
                return (Int32)SET.Properties["TileIndex"].Object;
            }
            set
            {
                SET.Properties["TileIndex"].Object = value;
            }
        }

        public Boolean RandomTile
        {
            get
            {
                return (Boolean)SET.Properties["RandomTile"].Object;
            }
            set
            {
                SET.Properties["RandomTile"].Object = value;
            }
        }

        public eEShaderMaterialBlendMode BlendMode
        {
            get
            {
                return (eEShaderMaterialBlendMode)SET.Properties["BlendMode"].Object;
            }
            set
            {
                SET.Properties["BlendMode"].Object = value;
            }
        }

        public Int32 MaterialSwitch
        {
            get
            {
                return (Int32)SET.Properties["MaterialSwitch"].Object;
            }
            set
            {
                SET.Properties["MaterialSwitch"].Object = value;
            }
        }

        public Boolean EditorBillboard
        {
            get
            {
                return (Boolean)SET.Properties["EditorBillboard"].Object;
            }
            set
            {
                SET.Properties["EditorBillboard"].Object = value;
            }
        }

        public Boolean PlaceOnTargetMesh
        {
            get
            {
                return (Boolean)SET.Properties["PlaceOnTargetMesh"].Object;
            }
            set
            {
                SET.Properties["PlaceOnTargetMesh"].Object = value;
            }
        }

        public eEBillboardTargetMode TargetMode
        {
            get
            {
                return (eEBillboardTargetMode)SET.Properties["TargetMode"].Object;
            }
            set
            {
                SET.Properties["TargetMode"].Object = value;
            }
        }

        public eCEntityProxy TargetEntity
        {
            get
            {
                return (eCEntityProxy)SET.Properties["TargetEntity"].Object;
            }
            set
            {
                SET.Properties["TargetEntity"].Object = value;
            }
        }

        public Vector3 MeshFaceDirection
        {
            get
            {
                return (Vector3)SET.Properties["MeshFaceDirection"].Object;
            }
            set
            {
                SET.Properties["MeshFaceDirection"].Object = value;
            }
        }

        public Single MeshFaceDirTollerance
        {
            get
            {
                return (Single)SET.Properties["MeshFaceDirTollerance"].Object;
            }
            set
            {
                SET.Properties["MeshFaceDirTollerance"].Object = value;
            }
        }
    }
    public class eCStaticAmbientLight_PS_Wrapper : PropertySetWrapper
    {
        /*
        public override void LoadModels(API_Device D)
        {
            base.LoadModels(D);
            return;
            Vector4 c = new Vector4(Color.Color.Red, Color.Color.Green, Color.Color.Blue, 1.0f);
            float s = Intensity;
            GameLibrary.Objekte.OmniLight OL = new OmniLight(Obj.Nodes[0], Offset, c, c, 0.005f, 0.00025f, 0.000000001f, CastShadows, D);
            OL.Tag = Range;
            OL.Attenuation /= Intensity;
        }
        */
        public bCFloatColor Color
        {
            get
            {
                return (bCFloatColor)SET.Properties["Color"].Object;
            }
            set
            {
                SET.Properties["Color"].Object = value;
            }
        }

        public Single Intensity
        {
            get
            {
                return (Single)SET.Properties["Intensity"].Object;
            }
            set
            {
                SET.Properties["Intensity"].Object = value;
            }
        }

        public Single Range
        {
            get
            {
                return (Single)SET.Properties["Range"].Object;
            }
            set
            {
                SET.Properties["Range"].Object = value;
            }
        }

        public Boolean CastShadows
        {
            get
            {
                return (Boolean)SET.Properties["CastShadows"].Object;
            }
            set
            {
                SET.Properties["CastShadows"].Object = value;
            }
        }

        public Vector3 Offset
        {
            get
            {
                return (Vector3)SET.Properties["Offset"].Object;
            }
            set
            {
                SET.Properties["Offset"].Object = value;
            }
        }
    }
    public class gCNavHelper_PS_Wrapper : PropertySetWrapper
    {
        public eCEntityProxy LinkedToEntityProxy
        {
            get
            {
                return (eCEntityProxy)SET.Properties["LinkedToEntityProxy"].Object;
            }
            set
            {
                SET.Properties["LinkedToEntityProxy"].Object = value;
            }
        }

        public eCEntityProxy LinkedToSecondEntityProxy
        {
            get
            {
                return (eCEntityProxy)SET.Properties["LinkedToSecondEntityProxy"].Object;
            }
            set
            {
                SET.Properties["LinkedToSecondEntityProxy"].Object = value;
            }
        }
    }
    public class gCCastInfo_PS_Wrapper : PropertySetWrapper
    {
        public eCTemplateEntityProxy CastItem
        {
            get
            {
                return (eCTemplateEntityProxy)SET.Properties["CastItem"].Object;
            }
            set
            {
                SET.Properties["CastItem"].Object = value;
            }
        }

        public eCTemplateEntityProxy CastAmmo
        {
            get
            {
                return (eCTemplateEntityProxy)SET.Properties["CastAmmo"].Object;
            }
            set
            {
                SET.Properties["CastAmmo"].Object = value;
            }
        }

        public eCTemplateEntityProxy CastFail
        {
            get
            {
                return (eCTemplateEntityProxy)SET.Properties["CastFail"].Object;
            }
            set
            {
                SET.Properties["CastFail"].Object = value;
            }
        }

        public Boolean ConsumeItem
        {
            get
            {
                return (Boolean)SET.Properties["ConsumeItem"].Object;
            }
            set
            {
                SET.Properties["ConsumeItem"].Object = value;
            }
        }

        public Int32 ManaCost
        {
            get
            {
                return (Int32)SET.Properties["ManaCost"].Object;
            }
            set
            {
                SET.Properties["ManaCost"].Object = value;
            }
        }

        public bCString StartTask
        {
            get
            {
                return (bCString)SET.Properties["StartTask"].Object;
            }
            set
            {
                SET.Properties["StartTask"].Object = value;
            }
        }

        public bCString FocusMode
        {
            get
            {
                return (bCString)SET.Properties["FocusMode"].Object;
            }
            set
            {
                SET.Properties["FocusMode"].Object = value;
            }
        }

        public bCString CastInfo
        {
            get
            {
                return (bCString)SET.Properties["CastInfo"].Object;
            }
            set
            {
                SET.Properties["CastInfo"].Object = value;
            }
        }
    }
    public class eCStrip_PS_Wrapper : PropertySetWrapper
    {
        public bCFloatColor StartColor
        {
            get
            {
                return (bCFloatColor)SET.Properties["StartColor"].Object;
            }
            set
            {
                SET.Properties["StartColor"].Object = value;
            }
        }

        public Single StartAlpha
        {
            get
            {
                return (Single)SET.Properties["StartAlpha"].Object;
            }
            set
            {
                SET.Properties["StartAlpha"].Object = value;
            }
        }

        public bCFloatColor EndColor
        {
            get
            {
                return (bCFloatColor)SET.Properties["EndColor"].Object;
            }
            set
            {
                SET.Properties["EndColor"].Object = value;
            }
        }

        public Single EndAlpha
        {
            get
            {
                return (Single)SET.Properties["EndAlpha"].Object;
            }
            set
            {
                SET.Properties["EndAlpha"].Object = value;
            }
        }

        public Vector3 OffsetLeft
        {
            get
            {
                return (Vector3)SET.Properties["OffsetLeft"].Object;
            }
            set
            {
                SET.Properties["OffsetLeft"].Object = value;
            }
        }

        public Vector3 OffsetRight
        {
            get
            {
                return (Vector3)SET.Properties["OffsetRight"].Object;
            }
            set
            {
                SET.Properties["OffsetRight"].Object = value;
            }
        }

        public Single SegmentsPerSecond
        {
            get
            {
                return (Single)SET.Properties["SegmentsPerSecond"].Object;
            }
            set
            {
                SET.Properties["SegmentsPerSecond"].Object = value;
            }
        }

        public Single SegmentLifeTime
        {
            get
            {
                return (Single)SET.Properties["SegmentLifeTime"].Object;
            }
            set
            {
                SET.Properties["SegmentLifeTime"].Object = value;
            }
        }

        public Int32 MaxSegmentCount
        {
            get
            {
                return (Int32)SET.Properties["MaxSegmentCount"].Object;
            }
            set
            {
                SET.Properties["MaxSegmentCount"].Object = value;
            }
        }

        public Single SegmentLenght
        {
            get
            {
                return (Single)SET.Properties["SegmentLenght"].Object;
            }
            set
            {
                SET.Properties["SegmentLenght"].Object = value;
            }
        }

        public eEStripSpawning SpawnMode
        {
            get
            {
                return (eEStripSpawning)SET.Properties["SpawnMode"].Object;
            }
            set
            {
                SET.Properties["SpawnMode"].Object = value;
            }
        }

        public bCString Material
        {
            get
            {
                return (bCString)SET.Properties["Material"].Object;
            }
            set
            {
                SET.Properties["Material"].Object = value;
            }
        }

        public Boolean Enabled
        {
            get
            {
                return (Boolean)SET.Properties["Enabled"].Object;
            }
            set
            {
                SET.Properties["Enabled"].Object = value;
            }
        }

        public Int32 MaterialSwitch
        {
            get
            {
                return (Int32)SET.Properties["MaterialSwitch"].Object;
            }
            set
            {
                SET.Properties["MaterialSwitch"].Object = value;
            }
        }

        public Single FadeOutInSec
        {
            get
            {
                return (Single)SET.Properties["FadeOutInSec"].Object;
            }
            set
            {
                SET.Properties["FadeOutInSec"].Object = value;
            }
        }

        public Single Opacity
        {
            get
            {
                return (Single)SET.Properties["Opacity"].Object;
            }
            set
            {
                SET.Properties["Opacity"].Object = value;
            }
        }

        public Single VelocityLeft
        {
            get
            {
                return (Single)SET.Properties["VelocityLeft"].Object;
            }
            set
            {
                SET.Properties["VelocityLeft"].Object = value;
            }
        }

        public Single VelocityRight
        {
            get
            {
                return (Single)SET.Properties["VelocityRight"].Object;
            }
            set
            {
                SET.Properties["VelocityRight"].Object = value;
            }
        }
    }
    public class gCProjectile2_PS_Wrapper : PropertySetWrapper
    {
        public Single StartVelocity
        {
            get
            {
                return (Single)SET.Properties["StartVelocity"].Object;
            }
            set
            {
                SET.Properties["StartVelocity"].Object = value;
            }
        }

        public Single PivotOffset
        {
            get
            {
                return (Single)SET.Properties["PivotOffset"].Object;
            }
            set
            {
                SET.Properties["PivotOffset"].Object = value;
            }
        }

        public Single PivotStuckOffset
        {
            get
            {
                return (Single)SET.Properties["PivotStuckOffset"].Object;
            }
            set
            {
                SET.Properties["PivotStuckOffset"].Object = value;
            }
        }

        public gEFlightPathType FlightPathType
        {
            get
            {
                return (gEFlightPathType)SET.Properties["FlightPathType"].Object;
            }
            set
            {
                SET.Properties["FlightPathType"].Object = value;
            }
        }

        public gCFlightPathBallistic FlightPathBallistic
        {
            get
            {
                return (gCFlightPathBallistic)SET.Properties["FlightPathBallistic"].Object;
            }
            set
            {
                SET.Properties["FlightPathBallistic"].Object = value;
            }
        }

        public gCFlightPathSeeking FlightPathSeeking
        {
            get
            {
                return (gCFlightPathSeeking)SET.Properties["FlightPathSeeking"].Object;
            }
            set
            {
                SET.Properties["FlightPathSeeking"].Object = value;
            }
        }

        public Boolean EnableSweepTest
        {
            get
            {
                return (Boolean)SET.Properties["EnableSweepTest"].Object;
            }
            set
            {
                SET.Properties["EnableSweepTest"].Object = value;
            }
        }

        public Single SweepTestRadius
        {
            get
            {
                return (Single)SET.Properties["SweepTestRadius"].Object;
            }
            set
            {
                SET.Properties["SweepTestRadius"].Object = value;
            }
        }

        public bCString ShootEffect
        {
            get
            {
                return (bCString)SET.Properties["ShootEffect"].Object;
            }
            set
            {
                SET.Properties["ShootEffect"].Object = value;
            }
        }

        public eCScriptProxyScript ShootScript
        {
            get
            {
                return (eCScriptProxyScript)SET.Properties["ShootScript"].Object;
            }
            set
            {
                SET.Properties["ShootScript"].Object = value;
            }
        }

        public eCScriptProxyScript OnIntersectScript
        {
            get
            {
                return (eCScriptProxyScript)SET.Properties["OnIntersectScript"].Object;
            }
            set
            {
                SET.Properties["OnIntersectScript"].Object = value;
            }
        }

        public eCScriptProxyScript OnHitScript
        {
            get
            {
                return (eCScriptProxyScript)SET.Properties["OnHitScript"].Object;
            }
            set
            {
                SET.Properties["OnHitScript"].Object = value;
            }
        }

        public Single DistanceToImpact
        {
            get
            {
                return (Single)SET.Properties["DistanceToImpact"].Object;
            }
            set
            {
                SET.Properties["DistanceToImpact"].Object = value;
            }
        }

        public Boolean EnableSpining
        {
            get
            {
                return (Boolean)SET.Properties["EnableSpining"].Object;
            }
            set
            {
                SET.Properties["EnableSpining"].Object = value;
            }
        }

        public Vector3 SpiningSpeed
        {
            get
            {
                return (Vector3)SET.Properties["SpiningSpeed"].Object;
            }
            set
            {
                SET.Properties["SpiningSpeed"].Object = value;
            }
        }
    }
    public class gCAIZone_PS_Wrapper : PropertySetWrapper
    {
        public gEGuild Guild
        {
            get
            {
                return (gEGuild)SET.Properties["Guild"].Object;
            }
            set
            {
                SET.Properties["Guild"].Object = value;
            }
        }

        public bCString Group
        {
            get
            {
                return (bCString)SET.Properties["Group"].Object;
            }
            set
            {
                SET.Properties["Group"].Object = value;
            }
        }

        public eCScriptProxyScript SecurityLevelFunc
        {
            get
            {
                return (eCScriptProxyScript)SET.Properties["SecurityLevelFunc"].Object;
            }
            set
            {
                SET.Properties["SecurityLevelFunc"].Object = value;
            }
        }
    }
    public class eCSpeedTree_PS_Wrapper : PropertySetWrapper
    {
        public bCSpeedTreeResourceString ResourceFilePath
        {
            get
            {
                return (bCSpeedTreeResourceString)SET.Properties["ResourceFilePath"].Object;
            }
            set
            {
                SET.Properties["ResourceFilePath"].Object = value;
            }
        }

        public Boolean EnableWind
        {
            get
            {
                return (Boolean)SET.Properties["EnableWind"].Object;
            }
            set
            {
                SET.Properties["EnableWind"].Object = value;
            }
        }

        public Boolean InfluenceGlobalAmbientOcclusion
        {
            get
            {
                return (Boolean)SET.Properties["InfluenceGlobalAmbientOcclusion"].Object;
            }
            set
            {
                SET.Properties["InfluenceGlobalAmbientOcclusion"].Object = value;
            }
        }

        public Single LoDNearDistance
        {
            get
            {
                return (Single)SET.Properties["LoDNearDistance"].Object;
            }
            set
            {
                SET.Properties["LoDNearDistance"].Object = value;
            }
        }

        public Single LoDFarDistance
        {
            get
            {
                return (Single)SET.Properties["LoDFarDistance"].Object;
            }
            set
            {
                SET.Properties["LoDFarDistance"].Object = value;
            }
        }

        public Single LoDFactor
        {
            get
            {
                return (Single)SET.Properties["LoDFactor"].Object;
            }
            set
            {
                SET.Properties["LoDFactor"].Object = value;
            }
        }
    }
    public class eCVegetation_PS_Wrapper : PropertySetWrapper
    {
        public override void LoadModels(API_Device D)
        {
            base.LoadModels(D);
            Stream S = Obj.File.File.Handle.Open(FileAccess.Read);
            BinaryReader B = new BinaryReader(S);
            ReadGrass(D, Obj.File, B, Obj.File.File.m_Strings);
            Obj.File.File.Handle.Close();
        }

        void ReadGrass(API_Device D, LrentFile lFile, BinaryReader bReader, List<string> StringTable)
        {
            List<GrassNode> gNodes = new List<GrassNode>();
            List<UInt16> refs = new List<UInt16>();
            if (bReader.BaseStream.Length <= 926L)
                return;
            bReader.BaseStream.Position = 926L;
            UInt32 prototypeCount = bReader.ReadUInt32();
            for (int i = 0; i < prototypeCount; i++)
            {
                bReader.BaseStream.Position += 15L;
                UInt32 size = bReader.ReadUInt32();
                bReader.BaseStream.Position += 16L;
                UInt16 refToXMSH = bReader.ReadUInt16();
                refs.Add(refToXMSH);
                bReader.BaseStream.Position += size - 18L;
            }
            bReader.BaseStream.Position += 22L;
            UInt32 listCount = bReader.ReadUInt32();
            for (int i = 0; i < listCount; i++)
            {
                bReader.ReadBytes(6);
                BoundingBox bb = bReader.ReadBytes(6 * 4).GetStructure<BoundingBox>();
                float xo1 = bb.Maximum.X; float xo2 = bb.Minimum.X;
                bb.Maximum.X = bb.Maximum.Z; bb.Maximum.Z = xo1;
                bb.Minimum.X = bb.Minimum.Z; bb.Minimum.Z = xo2;
                UInt32 nMinitEntitys = bReader.ReadUInt32();
                List<GraphicNode> ents = new List<GraphicNode>();
                for (int j = 0; j < nMinitEntitys; j++)
                {
                    bReader.ReadBytes(2);
                    UInt16 prototype_num = bReader.ReadUInt16();
                    Vector3 position = bReader.ReadBytes(12).GetStructure<Vector3>();
                    float xo = position.X;
                    position.X = position.Z;
                    position.Z = xo;
                    float[] rr = new float[] { bReader.ReadSingle(), bReader.ReadSingle(), bReader.ReadSingle(), bReader.ReadSingle(), bReader.ReadSingle(), bReader.ReadSingle() };
                    bReader.ReadBytes(4);

                    Quaternion q = new Quaternion(rr[0], rr[1], rr[2], rr[3]);
                    Matrix m = GENOMEMath.fromGENOME(Matrix.RotationQuaternion(q));
                    q = Quaternion.RotationMatrix(m);
                    Vector3 s = new Vector3(rr[4], rr[5], rr[4]);

                    if (refs.Count <= prototype_num)
                        prototype_num = (UInt16)(refs.Count - 1);
                    //Entity e = new Entity(Texts[refs[prototype_num]], position, Matrix.RotationQuaternion(q));
                    //e.T = new Text(e.T.T.Replace(@"E:\Workspace\Piranha Bytes\Gothic III\work\data\common\", ""));
                    string path = StringTable[refs[prototype_num]].Replace(@"E:\Workspace\Piranha Bytes\Gothic III\work\data\common\", "");

                    GraphicNode gn = null;
                    string[] sq = path.Split(@"\".ToCharArray());
                    gn = D.Content.LoadModelFromFile(sq[sq.Length - 1].Replace(".xmsh", "._xmsh"), true);
                    if (gn != null)
                    {
                        ents.Add(gn);
                        gn.Position_LOCAL = position;
                        gn.Size_LOCAL = s;
                        gn.Orientation_LOCAL = q;
                    }
                }
                if (ents.Count == 0)
                    continue;

                Vector3 cPos = Vector3.Zero;
                BoundingBox cBB = new BoundingBox(new Vector3(float.MaxValue), new Vector3(float.MinValue));
                foreach (GraphicNode gn in ents)
                    cBB = gn.BoundingBox_ABS.Extend(cBB);
                cPos = cBB.Minimum + (cBB.Maximum - cBB.Minimum) / 2;

                GraphicNodeCollection gnc = new GrassNode(ents, D, this.Obj);
                gnc.Initialize(null, cPos, new Vector3(1), false);
                gnc.BoundingBox = new BoundingBox(cBB.Minimum - cPos, cBB.Maximum - cPos);
                gNodes.Add(gnc as GrassNode);
            }
            VegetationManager.addLrent(gNodes, D);
        }

        public Boolean UseDefaultViewRange
        {
            get
            {
                return (Boolean)SET.Properties["UseDefaultViewRange"].Object;
            }
            set
            {
                SET.Properties["UseDefaultViewRange"].Object = value;
            }
        }

        public Single ViewRange
        {
            get
            {
                return (Single)SET.Properties["ViewRange"].Object;
            }
            set
            {
                SET.Properties["ViewRange"].Object = value;
            }
        }

        public Single FadeOutStart
        {
            get
            {
                return (Single)SET.Properties["FadeOutStart"].Object;
            }
            set
            {
                SET.Properties["FadeOutStart"].Object = value;
            }
        }

        public Single GridNodeSize
        {
            get
            {
                return (Single)SET.Properties["GridNodeSize"].Object;
            }
            set
            {
                SET.Properties["GridNodeSize"].Object = value;
            }
        }

        public Boolean UseQuality
        {
            get
            {
                return (Boolean)SET.Properties["UseQuality"].Object;
            }
            set
            {
                SET.Properties["UseQuality"].Object = value;
            }
        }
    }
    public class eCParticle_PS_Wrapper : PropertySetWrapper
    {
        public Int32 MaxNumParticles
        {
            get
            {
                return (Int32)SET.Properties["MaxNumParticles"].Object;
            }
            set
            {
                SET.Properties["MaxNumParticles"].Object = value;
            }
        }

        public Single ParticlesPerSecond
        {
            get
            {
                return (Single)SET.Properties["ParticlesPerSecond"].Object;
            }
            set
            {
                SET.Properties["ParticlesPerSecond"].Object = value;
            }
        }

        public Boolean RespawnDeadParticles
        {
            get
            {
                return (Boolean)SET.Properties["RespawnDeadParticles"].Object;
            }
            set
            {
                SET.Properties["RespawnDeadParticles"].Object = value;
            }
        }

        public Boolean AutomaticSpawning
        {
            get
            {
                return (Boolean)SET.Properties["AutomaticSpawning"].Object;
            }
            set
            {
                SET.Properties["AutomaticSpawning"].Object = value;
            }
        }

        public Boolean AutoReset
        {
            get
            {
                return (Boolean)SET.Properties["AutoReset"].Object;
            }
            set
            {
                SET.Properties["AutoReset"].Object = value;
            }
        }

        public Boolean Enabled
        {
            get
            {
                return (Boolean)SET.Properties["Enabled"].Object;
            }
            set
            {
                SET.Properties["Enabled"].Object = value;
            }
        }

        public bCRange1 Lifetime
        {
            get
            {
                return (bCRange1)SET.Properties["Lifetime"].Object;
            }
            set
            {
                SET.Properties["Lifetime"].Object = value;
            }
        }

        public bCRange1 InitialAge
        {
            get
            {
                return (bCRange1)SET.Properties["InitialAge"].Object;
            }
            set
            {
                SET.Properties["InitialAge"].Object = value;
            }
        }

        public bCRange1 InitialDelay
        {
            get
            {
                return (bCRange1)SET.Properties["InitialDelay"].Object;
            }
            set
            {
                SET.Properties["InitialDelay"].Object = value;
            }
        }

        public Single TicksPerSecond
        {
            get
            {
                return (Single)SET.Properties["TicksPerSecond"].Object;
            }
            set
            {
                SET.Properties["TicksPerSecond"].Object = value;
            }
        }

        public Single SecondsBeforeInactive
        {
            get
            {
                return (Single)SET.Properties["SecondsBeforeInactive"].Object;
            }
            set
            {
                SET.Properties["SecondsBeforeInactive"].Object = value;
            }
        }

        public Single RelativeWarmupTime
        {
            get
            {
                return (Single)SET.Properties["RelativeWarmupTime"].Object;
            }
            set
            {
                SET.Properties["RelativeWarmupTime"].Object = value;
            }
        }

        public Single WarmupTicksPerSecond
        {
            get
            {
                return (Single)SET.Properties["WarmupTicksPerSecond"].Object;
            }
            set
            {
                SET.Properties["WarmupTicksPerSecond"].Object = value;
            }
        }

        public eELocationShape StartLocationShape
        {
            get
            {
                return (eELocationShape)SET.Properties["StartLocationShape"].Object;
            }
            set
            {
                SET.Properties["StartLocationShape"].Object = value;
            }
        }

        public eELocationTarget StartLocationTarget
        {
            get
            {
                return (eELocationTarget)SET.Properties["StartLocationTarget"].Object;
            }
            set
            {
                SET.Properties["StartLocationTarget"].Object = value;
            }
        }

        public bCRange1 StartBoxLocation
        {
            get
            {
                return (bCRange1)SET.Properties["StartBoxLocation"].Object;
            }
            set
            {
                SET.Properties["StartBoxLocation"].Object = value;
            }
        }

        public bCRange1 StartSphereRadius
        {
            get
            {
                return (bCRange1)SET.Properties["StartSphereRadius"].Object;
            }
            set
            {
                SET.Properties["StartSphereRadius"].Object = value;
            }
        }

        public Vector3 StartLocationOffset
        {
            get
            {
                return (Vector3)SET.Properties["StartLocationOffset"].Object;
            }
            set
            {
                SET.Properties["StartLocationOffset"].Object = value;
            }
        }

        public eECoordinateSystem CoordinateSystem
        {
            get
            {
                return (eECoordinateSystem)SET.Properties["CoordinateSystem"].Object;
            }
            set
            {
                SET.Properties["CoordinateSystem"].Object = value;
            }
        }

        public bCRange1 StartVelocity
        {
            get
            {
                return (bCRange1)SET.Properties["StartVelocity"].Object;
            }
            set
            {
                SET.Properties["StartVelocity"].Object = value;
            }
        }

        public bCRange1 VelocityLoss
        {
            get
            {
                return (bCRange1)SET.Properties["VelocityLoss"].Object;
            }
            set
            {
                SET.Properties["VelocityLoss"].Object = value;
            }
        }

        public Vector3 Acceleration
        {
            get
            {
                return (Vector3)SET.Properties["Acceleration"].Object;
            }
            set
            {
                SET.Properties["Acceleration"].Object = value;
            }
        }

        public Vector3 MaxAbsoluteVelocity
        {
            get
            {
                return (Vector3)SET.Properties["MaxAbsoluteVelocity"].Object;
            }
            set
            {
                SET.Properties["MaxAbsoluteVelocity"].Object = value;
            }
        }

        public Boolean UseVelocityScale
        {
            get
            {
                return (Boolean)SET.Properties["UseVelocityScale"].Object;
            }
            set
            {
                SET.Properties["UseVelocityScale"].Object = value;
            }
        }

        public Single VelocityScaleRepeats
        {
            get
            {
                return (Single)SET.Properties["VelocityScaleRepeats"].Object;
            }
            set
            {
                SET.Properties["VelocityScaleRepeats"].Object = value;
            }
        }

        public eEVelocityDirectionFrom VelocityDirectionFrom
        {
            get
            {
                return (eEVelocityDirectionFrom)SET.Properties["VelocityDirectionFrom"].Object;
            }
            set
            {
                SET.Properties["VelocityDirectionFrom"].Object = value;
            }
        }

        public Single AddVelocityFromOwnerFactor
        {
            get
            {
                return (Single)SET.Properties["AddVelocityFromOwnerFactor"].Object;
            }
            set
            {
                SET.Properties["AddVelocityFromOwnerFactor"].Object = value;
            }
        }

        public Single Stiffness
        {
            get
            {
                return (Single)SET.Properties["Stiffness"].Object;
            }
            set
            {
                SET.Properties["Stiffness"].Object = value;
            }
        }

        public eCEntityProxy Target
        {
            get
            {
                return (eCEntityProxy)SET.Properties["Target"].Object;
            }
            set
            {
                SET.Properties["Target"].Object = value;
            }
        }

        public Boolean SpinParticles
        {
            get
            {
                return (Boolean)SET.Properties["SpinParticles"].Object;
            }
            set
            {
                SET.Properties["SpinParticles"].Object = value;
            }
        }

        public bCRange1 StartSpin
        {
            get
            {
                return (bCRange1)SET.Properties["StartSpin"].Object;
            }
            set
            {
                SET.Properties["StartSpin"].Object = value;
            }
        }

        public bCRange1 SpinsPerSecond
        {
            get
            {
                return (bCRange1)SET.Properties["SpinsPerSecond"].Object;
            }
            set
            {
                SET.Properties["SpinsPerSecond"].Object = value;
            }
        }

        public Single SpinDirection
        {
            get
            {
                return (Single)SET.Properties["SpinDirection"].Object;
            }
            set
            {
                SET.Properties["SpinDirection"].Object = value;
            }
        }

        public eEFacingDirection FacingDirection
        {
            get
            {
                return (eEFacingDirection)SET.Properties["FacingDirection"].Object;
            }
            set
            {
                SET.Properties["FacingDirection"].Object = value;
            }
        }

        public Vector3 ProjectionNormal
        {
            get
            {
                return (Vector3)SET.Properties["ProjectionNormal"].Object;
            }
            set
            {
                SET.Properties["ProjectionNormal"].Object = value;
            }
        }

        public Vector3 RelativeSpinPivot
        {
            get
            {
                return (Vector3)SET.Properties["RelativeSpinPivot"].Object;
            }
            set
            {
                SET.Properties["RelativeSpinPivot"].Object = value;
            }
        }

        public eERotationFrom UseRotationFrom
        {
            get
            {
                return (eERotationFrom)SET.Properties["UseRotationFrom"].Object;
            }
            set
            {
                SET.Properties["UseRotationFrom"].Object = value;
            }
        }

        public Boolean UseRevolution
        {
            get
            {
                return (Boolean)SET.Properties["UseRevolution"].Object;
            }
            set
            {
                SET.Properties["UseRevolution"].Object = value;
            }
        }

        public bCRange1 RevolutionsPerSecond
        {
            get
            {
                return (bCRange1)SET.Properties["RevolutionsPerSecond"].Object;
            }
            set
            {
                SET.Properties["RevolutionsPerSecond"].Object = value;
            }
        }

        public bCRange1 RevolutionCenterOffset
        {
            get
            {
                return (bCRange1)SET.Properties["RevolutionCenterOffset"].Object;
            }
            set
            {
                SET.Properties["RevolutionCenterOffset"].Object = value;
            }
        }

        public Boolean UseRevolutionScale
        {
            get
            {
                return (Boolean)SET.Properties["UseRevolutionScale"].Object;
            }
            set
            {
                SET.Properties["UseRevolutionScale"].Object = value;
            }
        }

        public Single RevolutionScaleRepeats
        {
            get
            {
                return (Single)SET.Properties["RevolutionScaleRepeats"].Object;
            }
            set
            {
                SET.Properties["RevolutionScaleRepeats"].Object = value;
            }
        }

        public Boolean UniformSize
        {
            get
            {
                return (Boolean)SET.Properties["UniformSize"].Object;
            }
            set
            {
                SET.Properties["UniformSize"].Object = value;
            }
        }

        public bCRange1 StartSize
        {
            get
            {
                return (bCRange1)SET.Properties["StartSize"].Object;
            }
            set
            {
                SET.Properties["StartSize"].Object = value;
            }
        }

        public Boolean UseSizeScale
        {
            get
            {
                return (Boolean)SET.Properties["UseSizeScale"].Object;
            }
            set
            {
                SET.Properties["UseSizeScale"].Object = value;
            }
        }

        public Single SizeScaleRepeats
        {
            get
            {
                return (Single)SET.Properties["SizeScaleRepeats"].Object;
            }
            set
            {
                SET.Properties["SizeScaleRepeats"].Object = value;
            }
        }

        public Boolean StretchToEmitter
        {
            get
            {
                return (Boolean)SET.Properties["StretchToEmitter"].Object;
            }
            set
            {
                SET.Properties["StretchToEmitter"].Object = value;
            }
        }

        public bCImageOrMaterialResourceString Material
        {
            get
            {
                return (bCImageOrMaterialResourceString)SET.Properties["Material"].Object;
            }
            set
            {
                SET.Properties["Material"].Object = value;
            }
        }

        public bCString Texture
        {
            get
            {
                return (bCString)SET.Properties["Texture"].Object;
            }
            set
            {
                SET.Properties["Texture"].Object = value;
            }
        }

        public eETextureDrawStyle DrawStyle
        {
            get
            {
                return (eETextureDrawStyle)SET.Properties["DrawStyle"].Object;
            }
            set
            {
                SET.Properties["DrawStyle"].Object = value;
            }
        }

        public Int32 NumUSubdivisions
        {
            get
            {
                return (Int32)SET.Properties["NumUSubdivisions"].Object;
            }
            set
            {
                SET.Properties["NumUSubdivisions"].Object = value;
            }
        }

        public Int32 NumVSubdivisions
        {
            get
            {
                return (Int32)SET.Properties["NumVSubdivisions"].Object;
            }
            set
            {
                SET.Properties["NumVSubdivisions"].Object = value;
            }
        }

        public Int32 SubdivisionStart
        {
            get
            {
                return (Int32)SET.Properties["SubdivisionStart"].Object;
            }
            set
            {
                SET.Properties["SubdivisionStart"].Object = value;
            }
        }

        public Int32 SubdivisionEnd
        {
            get
            {
                return (Int32)SET.Properties["SubdivisionEnd"].Object;
            }
            set
            {
                SET.Properties["SubdivisionEnd"].Object = value;
            }
        }

        public Boolean RandomSubdivision
        {
            get
            {
                return (Boolean)SET.Properties["RandomSubdivision"].Object;
            }
            set
            {
                SET.Properties["RandomSubdivision"].Object = value;
            }
        }

        public Boolean BlendBetweenSubdivisions
        {
            get
            {
                return (Boolean)SET.Properties["BlendBetweenSubdivisions"].Object;
            }
            set
            {
                SET.Properties["BlendBetweenSubdivisions"].Object = value;
            }
        }

        public Boolean SubdivisionRowMajor
        {
            get
            {
                return (Boolean)SET.Properties["SubdivisionRowMajor"].Object;
            }
            set
            {
                SET.Properties["SubdivisionRowMajor"].Object = value;
            }
        }

        public Boolean UseSubdivisionScale
        {
            get
            {
                return (Boolean)SET.Properties["UseSubdivisionScale"].Object;
            }
            set
            {
                SET.Properties["UseSubdivisionScale"].Object = value;
            }
        }

        public Single SubdivisionScaleRepeats
        {
            get
            {
                return (Single)SET.Properties["SubdivisionScaleRepeats"].Object;
            }
            set
            {
                SET.Properties["SubdivisionScaleRepeats"].Object = value;
            }
        }

        public Boolean FadeIn
        {
            get
            {
                return (Boolean)SET.Properties["FadeIn"].Object;
            }
            set
            {
                SET.Properties["FadeIn"].Object = value;
            }
        }

        public Boolean FadeOut
        {
            get
            {
                return (Boolean)SET.Properties["FadeOut"].Object;
            }
            set
            {
                SET.Properties["FadeOut"].Object = value;
            }
        }

        public Single FadeInEndTime
        {
            get
            {
                return (Single)SET.Properties["FadeInEndTime"].Object;
            }
            set
            {
                SET.Properties["FadeInEndTime"].Object = value;
            }
        }

        public Single FadeOutStartTime
        {
            get
            {
                return (Single)SET.Properties["FadeOutStartTime"].Object;
            }
            set
            {
                SET.Properties["FadeOutStartTime"].Object = value;
            }
        }

        public bCFloatAlphaColor FaceInFactor
        {
            get
            {
                return (bCFloatAlphaColor)SET.Properties["FaceInFactor"].Object;
            }
            set
            {
                SET.Properties["FaceInFactor"].Object = value;
            }
        }

        public bCFloatAlphaColor FaceOutFactor
        {
            get
            {
                return (bCFloatAlphaColor)SET.Properties["FaceOutFactor"].Object;
            }
            set
            {
                SET.Properties["FaceOutFactor"].Object = value;
            }
        }

        public Boolean UseColorScale
        {
            get
            {
                return (Boolean)SET.Properties["UseColorScale"].Object;
            }
            set
            {
                SET.Properties["UseColorScale"].Object = value;
            }
        }

        public Single ColorScaleRepeats
        {
            get
            {
                return (Single)SET.Properties["ColorScaleRepeats"].Object;
            }
            set
            {
                SET.Properties["ColorScaleRepeats"].Object = value;
            }
        }

        public Boolean DisableFogging
        {
            get
            {
                return (Boolean)SET.Properties["DisableFogging"].Object;
            }
            set
            {
                SET.Properties["DisableFogging"].Object = value;
            }
        }

        public Boolean AlphaTest
        {
            get
            {
                return (Boolean)SET.Properties["AlphaTest"].Object;
            }
            set
            {
                SET.Properties["AlphaTest"].Object = value;
            }
        }

        public Char AlphaReference
        {
            get
            {
                return (Char)SET.Properties["AlphaReference"].Object;
            }
            set
            {
                SET.Properties["AlphaReference"].Object = value;
            }
        }

        public Boolean DepthTest
        {
            get
            {
                return (Boolean)SET.Properties["DepthTest"].Object;
            }
            set
            {
                SET.Properties["DepthTest"].Object = value;
            }
        }

        public Boolean DepthWrite
        {
            get
            {
                return (Boolean)SET.Properties["DepthWrite"].Object;
            }
            set
            {
                SET.Properties["DepthWrite"].Object = value;
            }
        }

        public Boolean Instanced
        {
            get
            {
                return (Boolean)SET.Properties["Instanced"].Object;
            }
            set
            {
                SET.Properties["Instanced"].Object = value;
            }
        }

        public Single DepthSortOffset
        {
            get
            {
                return (Single)SET.Properties["DepthSortOffset"].Object;
            }
            set
            {
                SET.Properties["DepthSortOffset"].Object = value;
            }
        }

        public eELightingStyle LightingStyle
        {
            get
            {
                return (eELightingStyle)SET.Properties["LightingStyle"].Object;
            }
            set
            {
                SET.Properties["LightingStyle"].Object = value;
            }
        }

        public Boolean ResetOnTrigger
        {
            get
            {
                return (Boolean)SET.Properties["ResetOnTrigger"].Object;
            }
            set
            {
                SET.Properties["ResetOnTrigger"].Object = value;
            }
        }

        public bCRange1 TriggerNumParticles
        {
            get
            {
                return (bCRange1)SET.Properties["TriggerNumParticles"].Object;
            }
            set
            {
                SET.Properties["TriggerNumParticles"].Object = value;
            }
        }

        public Single TriggerParticlesPerSecond
        {
            get
            {
                return (Single)SET.Properties["TriggerParticlesPerSecond"].Object;
            }
            set
            {
                SET.Properties["TriggerParticlesPerSecond"].Object = value;
            }
        }

        public Boolean UseCollision
        {
            get
            {
                return (Boolean)SET.Properties["UseCollision"].Object;
            }
            set
            {
                SET.Properties["UseCollision"].Object = value;
            }
        }

        public Boolean UseMaxCollisions
        {
            get
            {
                return (Boolean)SET.Properties["UseMaxCollisions"].Object;
            }
            set
            {
                SET.Properties["UseMaxCollisions"].Object = value;
            }
        }

        public bCRange1 MaxCollisions
        {
            get
            {
                return (bCRange1)SET.Properties["MaxCollisions"].Object;
            }
            set
            {
                SET.Properties["MaxCollisions"].Object = value;
            }
        }

        public bCRange1 DampingFactor
        {
            get
            {
                return (bCRange1)SET.Properties["DampingFactor"].Object;
            }
            set
            {
                SET.Properties["DampingFactor"].Object = value;
            }
        }

        public Boolean UseRotationDamping
        {
            get
            {
                return (Boolean)SET.Properties["UseRotationDamping"].Object;
            }
            set
            {
                SET.Properties["UseRotationDamping"].Object = value;
            }
        }

        public bCRange1 RotationDampingFactor
        {
            get
            {
                return (bCRange1)SET.Properties["RotationDampingFactor"].Object;
            }
            set
            {
                SET.Properties["RotationDampingFactor"].Object = value;
            }
        }

        public Vector3 ExtentMultiplier
        {
            get
            {
                return (Vector3)SET.Properties["ExtentMultiplier"].Object;
            }
            set
            {
                SET.Properties["ExtentMultiplier"].Object = value;
            }
        }

        public Single MinSquaredVelocity
        {
            get
            {
                return (Single)SET.Properties["MinSquaredVelocity"].Object;
            }
            set
            {
                SET.Properties["MinSquaredVelocity"].Object = value;
            }
        }

        public Boolean CollideWithCharacters
        {
            get
            {
                return (Boolean)SET.Properties["CollideWithCharacters"].Object;
            }
            set
            {
                SET.Properties["CollideWithCharacters"].Object = value;
            }
        }

        public Boolean CollideWithDynamic
        {
            get
            {
                return (Boolean)SET.Properties["CollideWithDynamic"].Object;
            }
            set
            {
                SET.Properties["CollideWithDynamic"].Object = value;
            }
        }

        public Boolean CollideWithStatic
        {
            get
            {
                return (Boolean)SET.Properties["CollideWithStatic"].Object;
            }
            set
            {
                SET.Properties["CollideWithStatic"].Object = value;
            }
        }

        public Boolean CollideWithTransparent
        {
            get
            {
                return (Boolean)SET.Properties["CollideWithTransparent"].Object;
            }
            set
            {
                SET.Properties["CollideWithTransparent"].Object = value;
            }
        }

        public eCEntityProxy SpawnFromOtherEmitter
        {
            get
            {
                return (eCEntityProxy)SET.Properties["SpawnFromOtherEmitter"].Object;
            }
            set
            {
                SET.Properties["SpawnFromOtherEmitter"].Object = value;
            }
        }

        public bCRange1 SpawnNumParticles
        {
            get
            {
                return (bCRange1)SET.Properties["SpawnNumParticles"].Object;
            }
            set
            {
                SET.Properties["SpawnNumParticles"].Object = value;
            }
        }

        public Boolean UseSpawnedVelocityScale
        {
            get
            {
                return (Boolean)SET.Properties["UseSpawnedVelocityScale"].Object;
            }
            set
            {
                SET.Properties["UseSpawnedVelocityScale"].Object = value;
            }
        }

        public bCRange1 SpawnedVelocityScale
        {
            get
            {
                return (bCRange1)SET.Properties["SpawnedVelocityScale"].Object;
            }
            set
            {
                SET.Properties["SpawnedVelocityScale"].Object = value;
            }
        }

        public bCRange1 GlobalSizeScale
        {
            get
            {
                return (bCRange1)SET.Properties["GlobalSizeScale"].Object;
            }
            set
            {
                SET.Properties["GlobalSizeScale"].Object = value;
            }
        }
    }
    public class gCArena_PS_Wrapper : PropertySetWrapper
    {
        public gEArenaStatus Status
        {
            get
            {
                return (gEArenaStatus)SET.Properties["Status"].Object;
            }
            set
            {
                SET.Properties["Status"].Object = value;
            }
        }
    }
    public class eCArea_StringProperty_PS_Wrapper : PropertySetWrapper
    {
        public bCRange1 AreaBox
        {
            get
            {
                return (bCRange1)SET.Properties["AreaBox"].Object;
            }
            set
            {
                SET.Properties["AreaBox"].Object = value;
            }
        }

        public bTObjArray<bCString> PropertyNameList
        {
            get
            {
                return (bTObjArray<bCString>)SET.Properties["PropertyNameList"].Object;
            }
            set
            {
                SET.Properties["PropertyNameList"].Object = value;
            }
        }
    }
    public class eCWeatherZone_PS_Wrapper : PropertySetWrapper
    {
        public eEWeatherZoneShape Shape
        {
            get
            {
                return (eEWeatherZoneShape)SET.Properties["Shape"].Object;
            }
            set
            {
                SET.Properties["Shape"].Object = value;
            }
        }

        public Single InnerRadius
        {
            get
            {
                return (Single)SET.Properties["InnerRadius"].Object;
            }
            set
            {
                SET.Properties["InnerRadius"].Object = value;
            }
        }

        public Single OuterRadius
        {
            get
            {
                return (Single)SET.Properties["OuterRadius"].Object;
            }
            set
            {
                SET.Properties["OuterRadius"].Object = value;
            }
        }

        public Vector3 InnerExtends
        {
            get
            {
                return (Vector3)SET.Properties["InnerExtends"].Object;
            }
            set
            {
                SET.Properties["InnerExtends"].Object = value;
            }
        }

        public Vector3 OuterExtends
        {
            get
            {
                return (Vector3)SET.Properties["OuterExtends"].Object;
            }
            set
            {
                SET.Properties["OuterExtends"].Object = value;
            }
        }

        public bCString WeatherEnvironment
        {
            get
            {
                return (bCString)SET.Properties["WeatherEnvironment"].Object;
            }
            set
            {
                SET.Properties["WeatherEnvironment"].Object = value;
            }
        }

        public eEWeatherZoneOverwrite LightIntensityOverwrite
        {
            get
            {
                return (eEWeatherZoneOverwrite)SET.Properties["LightIntensityOverwrite"].Object;
            }
            set
            {
                SET.Properties["LightIntensityOverwrite"].Object = value;
            }
        }

        public Single LightIntensity
        {
            get
            {
                return (Single)SET.Properties["LightIntensity"].Object;
            }
            set
            {
                SET.Properties["LightIntensity"].Object = value;
            }
        }

        public eEWeatherZoneOverwrite LightDiffuseOverwrite
        {
            get
            {
                return (eEWeatherZoneOverwrite)SET.Properties["LightDiffuseOverwrite"].Object;
            }
            set
            {
                SET.Properties["LightDiffuseOverwrite"].Object = value;
            }
        }

        public bCFloatColor LightDiffuseColor
        {
            get
            {
                return (bCFloatColor)SET.Properties["LightDiffuseColor"].Object;
            }
            set
            {
                SET.Properties["LightDiffuseColor"].Object = value;
            }
        }

        public eEWeatherZoneOverwrite LightSpecularOverwrite
        {
            get
            {
                return (eEWeatherZoneOverwrite)SET.Properties["LightSpecularOverwrite"].Object;
            }
            set
            {
                SET.Properties["LightSpecularOverwrite"].Object = value;
            }
        }

        public bCFloatColor LightSpecularColor
        {
            get
            {
                return (bCFloatColor)SET.Properties["LightSpecularColor"].Object;
            }
            set
            {
                SET.Properties["LightSpecularColor"].Object = value;
            }
        }

        public eEWeatherZoneOverwrite AmbientBackLightOverwrite
        {
            get
            {
                return (eEWeatherZoneOverwrite)SET.Properties["AmbientBackLightOverwrite"].Object;
            }
            set
            {
                SET.Properties["AmbientBackLightOverwrite"].Object = value;
            }
        }

        public bCFloatColor AmbientBackLightColor
        {
            get
            {
                return (bCFloatColor)SET.Properties["AmbientBackLightColor"].Object;
            }
            set
            {
                SET.Properties["AmbientBackLightColor"].Object = value;
            }
        }

        public eEWeatherZoneOverwrite AmbientGeneralOverwrite
        {
            get
            {
                return (eEWeatherZoneOverwrite)SET.Properties["AmbientGeneralOverwrite"].Object;
            }
            set
            {
                SET.Properties["AmbientGeneralOverwrite"].Object = value;
            }
        }

        public bCFloatColor AmbientGeneralColor
        {
            get
            {
                return (bCFloatColor)SET.Properties["AmbientGeneralColor"].Object;
            }
            set
            {
                SET.Properties["AmbientGeneralColor"].Object = value;
            }
        }

        public eEWeatherZoneOverwrite AmbientIntensityOverwrite
        {
            get
            {
                return (eEWeatherZoneOverwrite)SET.Properties["AmbientIntensityOverwrite"].Object;
            }
            set
            {
                SET.Properties["AmbientIntensityOverwrite"].Object = value;
            }
        }

        public Single AmbientIntensity
        {
            get
            {
                return (Single)SET.Properties["AmbientIntensity"].Object;
            }
            set
            {
                SET.Properties["AmbientIntensity"].Object = value;
            }
        }

        public eEWeatherZoneOverwrite FogColorOverwrite
        {
            get
            {
                return (eEWeatherZoneOverwrite)SET.Properties["FogColorOverwrite"].Object;
            }
            set
            {
                SET.Properties["FogColorOverwrite"].Object = value;
            }
        }

        public bCFloatColor FogColor
        {
            get
            {
                return (bCFloatColor)SET.Properties["FogColor"].Object;
            }
            set
            {
                SET.Properties["FogColor"].Object = value;
            }
        }

        public eEWeatherZoneOverwrite FogStartrOverwrite
        {
            get
            {
                return (eEWeatherZoneOverwrite)SET.Properties["FogStartrOverwrite"].Object;
            }
            set
            {
                SET.Properties["FogStartrOverwrite"].Object = value;
            }
        }

        public Single FogStart
        {
            get
            {
                return (Single)SET.Properties["FogStart"].Object;
            }
            set
            {
                SET.Properties["FogStart"].Object = value;
            }
        }

        public eEWeatherZoneOverwrite FogEndrOverwrite
        {
            get
            {
                return (eEWeatherZoneOverwrite)SET.Properties["FogEndrOverwrite"].Object;
            }
            set
            {
                SET.Properties["FogEndrOverwrite"].Object = value;
            }
        }

        public Single FogEnd
        {
            get
            {
                return (Single)SET.Properties["FogEnd"].Object;
            }
            set
            {
                SET.Properties["FogEnd"].Object = value;
            }
        }

        public eEWeatherZoneOverwrite FogDensityOverwrite
        {
            get
            {
                return (eEWeatherZoneOverwrite)SET.Properties["FogDensityOverwrite"].Object;
            }
            set
            {
                SET.Properties["FogDensityOverwrite"].Object = value;
            }
        }

        public Single FogDensity
        {
            get
            {
                return (Single)SET.Properties["FogDensity"].Object;
            }
            set
            {
                SET.Properties["FogDensity"].Object = value;
            }
        }

        public eEWeatherZoneOverwrite SkyColorOverwrite
        {
            get
            {
                return (eEWeatherZoneOverwrite)SET.Properties["SkyColorOverwrite"].Object;
            }
            set
            {
                SET.Properties["SkyColorOverwrite"].Object = value;
            }
        }

        public bCFloatColor SkyColor
        {
            get
            {
                return (bCFloatColor)SET.Properties["SkyColor"].Object;
            }
            set
            {
                SET.Properties["SkyColor"].Object = value;
            }
        }

        public eEWeatherZoneOverwrite HazeColorOverwrite
        {
            get
            {
                return (eEWeatherZoneOverwrite)SET.Properties["HazeColorOverwrite"].Object;
            }
            set
            {
                SET.Properties["HazeColorOverwrite"].Object = value;
            }
        }

        public bCFloatColor HazeColor
        {
            get
            {
                return (bCFloatColor)SET.Properties["HazeColor"].Object;
            }
            set
            {
                SET.Properties["HazeColor"].Object = value;
            }
        }

        public eEWeatherZoneOverwrite CloudColorOverwrite
        {
            get
            {
                return (eEWeatherZoneOverwrite)SET.Properties["CloudColorOverwrite"].Object;
            }
            set
            {
                SET.Properties["CloudColorOverwrite"].Object = value;
            }
        }

        public bCFloatColor CloudColor
        {
            get
            {
                return (bCFloatColor)SET.Properties["CloudColor"].Object;
            }
            set
            {
                SET.Properties["CloudColor"].Object = value;
            }
        }

        public eEWeatherZoneOverwrite CloudThicknessrOverwrite
        {
            get
            {
                return (eEWeatherZoneOverwrite)SET.Properties["CloudThicknessrOverwrite"].Object;
            }
            set
            {
                SET.Properties["CloudThicknessrOverwrite"].Object = value;
            }
        }

        public Single CloudThickness
        {
            get
            {
                return (Single)SET.Properties["CloudThickness"].Object;
            }
            set
            {
                SET.Properties["CloudThickness"].Object = value;
            }
        }

        public eCScriptProxyScript InZoneScript
        {
            get
            {
                return (eCScriptProxyScript)SET.Properties["InZoneScript"].Object;
            }
            set
            {
                SET.Properties["InZoneScript"].Object = value;
            }
        }
    }
    public class eCPortalRoom_PS_Wrapper : PropertySetWrapper
    {
        //Matrix initial;

        //Matrix m_T = Matrix.Invert(oldMatrix) * LocalMatrix;
        //oldMatrix = LocalMatrix;
        //(acc.Class as eCPortalRoom_PS).Update(m_T) 

        public eCPortalRoom_PS_Wrapper() { }
        public eCPortalRoom_PS_Wrapper(ILrentObject O, string preset = "indoor_temple")
        {
            eCPortalRoom_PS s = new eCPortalRoom_PS();
            base.setName = "eCPortalRoom_PS";
            base.SET = new bCAccessorPropertyObject(s);
            base.SET.Properties.addProperty("MeshFileName", "bCString", new bCString("INVALID"));
            base.SET.Properties.addProperty("ReverbPreset", "bCString", new bCString(preset));
            base.Obj = O;
            O.Entity.AddAccessor(base.SET);
        }

        protected override void Initialize(ILrentObject o)
        {
            base.Initialize(o);
            RisenWorld.EntityTransformed += new EntityTransformed(RisenWorld_EntityTransformed);
        }

        public override void OnRemove()
        {
            RisenWorld.EntityTransformed -= RisenWorld_EntityTransformed;
            base.OnRemove();
        }

        void RisenWorld_EntityTransformed(ILrentObject a_Entity)
        {
            if (a_Entity == base.Obj)
            {
                base.Obj.SetWrappers.Remove(this);
                Obj.Entity.RemoveAccessor("eCPortalRoom_PS");
                RisenWorld.EntityTransformed -= RisenWorld_EntityTransformed;
            }
        }

        private Vector3 trans(Vector3 v, ref Matrix m)
        {
            Vector3 r = GENOMEMath.fromGENOME(v);
            return Vector3.Transform(r, m).ToVec3();
        }
        public override void LoadModels(API_Device D)
        {
            return;
            base.LoadModels(D);
            eCPortalRoom_PS C = SET.Class as eCPortalRoom_PS;
            List<StaticVertex> v_Vertices = new List<StaticVertex>();
            Stack<eCPortalRoom_PS.eCPortalBSP> v_Stack = new Stack<eCPortalRoom_PS.eCPortalBSP>();
            v_Stack.Push(C.BSP);
            Matrix m_Inv = Matrix.Invert(Obj.Matrix);
            while (v_Stack.Count != 0)
            {
                eCPortalRoom_PS.eCPortalBSP cP = v_Stack.Pop();
                if (cP.child1 != null)
                    v_Stack.Push(cP.child1);
                if (cP.child2 != null)
                    v_Stack.Push(cP.child2);
                foreach (eCPortalRoom_PS.BSPPolygon p in cP.Polygons)
                {
                    Vector3[] V = null;
                    if (p.ValidPointCount > 3)
                    {
                        Triangulator T = new Triangulator(p.Points.GetRange(0, p.ValidPointCount).ToArray());
                        V = T.GetTriangles();
                    }
                    else V = p.Points.GetRange(0, p.ValidPointCount).ToArray();
                    foreach (Vector3 v in V)
                        v_Vertices.Add(new StaticVertex(trans(v, ref m_Inv)));
                }
            }
            List<StaticVertex> v_Vertices2 = new List<StaticVertex>();
            foreach (eCPortalRoom_PS.eCPortal p in C.Portals)
            {
                v_Vertices2.Add(new StaticVertex(trans(p.PortalRect.Points[0], ref m_Inv)));
                v_Vertices2.Add(new StaticVertex(trans(p.PortalRect.Points[1], ref m_Inv)));
                v_Vertices2.Add(new StaticVertex(trans(p.PortalRect.Points[2], ref m_Inv)));

                v_Vertices2.Add(new StaticVertex(trans(p.PortalRect.Points[2], ref m_Inv)));
                v_Vertices2.Add(new StaticVertex(trans(p.PortalRect.Points[3], ref m_Inv)));
                v_Vertices2.Add(new StaticVertex(trans(p.PortalRect.Points[0], ref m_Inv)));
            }

            MeshBuffer mb = new MeshBuffer(v_Vertices.ToArray(), v_Vertices[0], D, SlimDX.Direct3D11.PrimitiveTopology.TriangleList);
            MeshPart mP = new MeshPart(mb, new Material("tst", true, D), D);
            MeshBuffer mb2 = new MeshBuffer(v_Vertices2.ToArray(), v_Vertices2[0], D, SlimDX.Direct3D11.PrimitiveTopology.TriangleList);
            MeshPart mP2 = new MeshPart(mb2, new Material("tst2", true, D), D); mP2.Material = new Material(ShaderResourceTexture.RedTexture);
            Mesh M = new Mesh(GENOMEMath.fromGENOME(C.Boundary), SlimDX.Direct3D11.CullMode.Back, mP, mP2);
            GraphicNode gn = new GraphicNode("irr", M, D);
            gn.Visible = true;
            Obj.Nodes.Add(gn);
        }

        public bCString MeshFileName
        {
            get
            {
                return (bCString)SET.Properties["MeshFileName"].Object;
            }
            set
            {
                SET.Properties["MeshFileName"].Object = value;
            }
        }

        public bCString ReverbPreset
        {
            get
            {
                return (bCString)SET.Properties["ReverbPreset"].Object;
            }
            set
            {
                SET.Properties["ReverbPreset"].Object = value;
            }
        }
    }
    public class eCOccluder_PS_Wrapper : PropertySetWrapper
    {
        public override void LoadModels(API_Device D)
        {
            string s = MeshFileName.pString;
            if (s != "Levelmesh_WaitingRoom_Occluder.xmsh")
            {
                Obj.Nodes.Add(D.Content.LoadModelFromFile(s, true));
            }
        }

        public bCString MeshFileName
        {
            get
            {
                return (bCString)SET.Properties["MeshFileName"].Object;
            }
            set
            {
                SET.Properties["MeshFileName"].Object = value;
                Obj.InvalidateModels();
            }
        }
    }
    public class gCWaterZone_PS_Wrapper : PropertySetWrapper
    {
        public bTObjArray<gCWaterSubZone_PS> SubZones
        {
            get
            {
                return (bTObjArray<gCWaterSubZone_PS>)SET.Properties["SubZones"].Object;
            }
            set
            {
                SET.Properties["SubZones"].Object = value;
            }
        }

        public Boolean UseWadeMovement
        {
            get
            {
                return (Boolean)SET.Properties["UseWadeMovement"].Object;
            }
            set
            {
                SET.Properties["UseWadeMovement"].Object = value;
            }
        }

        public bCString EffectMaterial
        {
            get
            {
                return (bCString)SET.Properties["EffectMaterial"].Object;
            }
            set
            {
                SET.Properties["EffectMaterial"].Object = value;
            }
        }

        public Single DepthRange
        {
            get
            {
                return (Single)SET.Properties["DepthRange"].Object;
            }
            set
            {
                SET.Properties["DepthRange"].Object = value;
            }
        }

        public eCScriptProxyScript OnContact
        {
            get
            {
                return (eCScriptProxyScript)SET.Properties["OnContact"].Object;
            }
            set
            {
                SET.Properties["OnContact"].Object = value;
            }
        }

        public eCTemplateEntityProxy EmitterTemplate
        {
            get
            {
                return (eCTemplateEntityProxy)SET.Properties["EmitterTemplate"].Object;
            }
            set
            {
                SET.Properties["EmitterTemplate"].Object = value;
            }
        }

        public eCEntityProxy Emitter
        {
            get
            {
                return (eCEntityProxy)SET.Properties["Emitter"].Object;
            }
            set
            {
                SET.Properties["Emitter"].Object = value;
            }
        }
    }   
    public class gCClock_PS_Wrapper : PropertySetWrapper
    {
        public Int32 SecondsPlayed
        {
            get
            {
                return (Int32)SET.Properties["SecondsPlayed"].Object;
            }
            set
            {
                SET.Properties["SecondsPlayed"].Object = value;
            }
        }

        public Int32 DaysPlayed
        {
            get
            {
                return (Int32)SET.Properties["DaysPlayed"].Object;
            }
            set
            {
                SET.Properties["DaysPlayed"].Object = value;
            }
        }

        public Int32 Year
        {
            get
            {
                return (Int32)SET.Properties["Year"].Object;
            }
            set
            {
                SET.Properties["Year"].Object = value;
            }
        }

        public Int32 Day
        {
            get
            {
                return (Int32)SET.Properties["Day"].Object;
            }
            set
            {
                SET.Properties["Day"].Object = value;
            }
        }

        public Int32 Hour
        {
            get
            {
                return (Int32)SET.Properties["Hour"].Object;
            }
            set
            {
                SET.Properties["Hour"].Object = value;
            }
        }

        public Int32 Minute
        {
            get
            {
                return (Int32)SET.Properties["Minute"].Object;
            }
            set
            {
                SET.Properties["Minute"].Object = value;
            }
        }

        public Int32 Second
        {
            get
            {
                return (Int32)SET.Properties["Second"].Object;
            }
            set
            {
                SET.Properties["Second"].Object = value;
            }
        }

        public Single Factor
        {
            get
            {
                return (Single)SET.Properties["Factor"].Object;
            }
            set
            {
                SET.Properties["Factor"].Object = value;
            }
        }
    }
    public class gCSectorPersistence_PS_Wrapper : PropertySetWrapper
    {
    }
    public class gCGameScript_PS_Wrapper : PropertySetWrapper
    {
        public eCScriptProxyScript OnGameInit
        {
            get
            {
                return (eCScriptProxyScript)SET.Properties["OnGameInit"].Object;
            }
            set
            {
                SET.Properties["OnGameInit"].Object = value;
            }
        }

        public eCScriptProxyScript OnGameStart
        {
            get
            {
                return (eCScriptProxyScript)SET.Properties["OnGameStart"].Object;
            }
            set
            {
                SET.Properties["OnGameStart"].Object = value;
            }
        }

        public eCScriptProxyScript OnGameEnd
        {
            get
            {
                return (eCScriptProxyScript)SET.Properties["OnGameEnd"].Object;
            }
            set
            {
                SET.Properties["OnGameEnd"].Object = value;
            }
        }

        public eCScriptProxyScript OnGameProcess
        {
            get
            {
                return (eCScriptProxyScript)SET.Properties["OnGameProcess"].Object;
            }
            set
            {
                SET.Properties["OnGameProcess"].Object = value;
            }
        }

        public eCScriptProxyScript OnGamePauseStart
        {
            get
            {
                return (eCScriptProxyScript)SET.Properties["OnGamePauseStart"].Object;
            }
            set
            {
                SET.Properties["OnGamePauseStart"].Object = value;
            }
        }

        public eCScriptProxyScript OnGamePauseProcess
        {
            get
            {
                return (eCScriptProxyScript)SET.Properties["OnGamePauseProcess"].Object;
            }
            set
            {
                SET.Properties["OnGamePauseProcess"].Object = value;
            }
        }

        public eCScriptProxyScript OnGamePauseEnd
        {
            get
            {
                return (eCScriptProxyScript)SET.Properties["OnGamePauseEnd"].Object;
            }
            set
            {
                SET.Properties["OnGamePauseEnd"].Object = value;
            }
        }
    }
    public class eCSpeedTreeWind_PS_Wrapper : PropertySetWrapper
    {
        public Single WindResponse
        {
            get
            {
                return (Single)SET.Properties["WindResponse"].Object;
            }
            set
            {
                SET.Properties["WindResponse"].Object = value;
            }
        }

        public Single WindResponseLimit
        {
            get
            {
                return (Single)SET.Properties["WindResponseLimit"].Object;
            }
            set
            {
                SET.Properties["WindResponseLimit"].Object = value;
            }
        }

        public Single WindStrength
        {
            get
            {
                return (Single)SET.Properties["WindStrength"].Object;
            }
            set
            {
                SET.Properties["WindStrength"].Object = value;
            }
        }

        public Single MaxBendAngle
        {
            get
            {
                return (Single)SET.Properties["MaxBendAngle"].Object;
            }
            set
            {
                SET.Properties["MaxBendAngle"].Object = value;
            }
        }

        public Single BranchExponent
        {
            get
            {
                return (Single)SET.Properties["BranchExponent"].Object;
            }
            set
            {
                SET.Properties["BranchExponent"].Object = value;
            }
        }

        public Single LeafExponent
        {
            get
            {
                return (Single)SET.Properties["LeafExponent"].Object;
            }
            set
            {
                SET.Properties["LeafExponent"].Object = value;
            }
        }

        public Vector2 GustStrength
        {
            get
            {
                return (Vector2)SET.Properties["GustStrength"].Object;
            }
            set
            {
                SET.Properties["GustStrength"].Object = value;
            }
        }

        public Single GustFrequency
        {
            get
            {
                return (Single)SET.Properties["GustFrequency"].Object;
            }
            set
            {
                SET.Properties["GustFrequency"].Object = value;
            }
        }

        public Vector2 GustDuration
        {
            get
            {
                return (Vector2)SET.Properties["GustDuration"].Object;
            }
            set
            {
                SET.Properties["GustDuration"].Object = value;
            }
        }

        public Vector2 BHWindAngle
        {
            get
            {
                return (Vector2)SET.Properties["BHWindAngle"].Object;
            }
            set
            {
                SET.Properties["BHWindAngle"].Object = value;
            }
        }

        public Vector2 BHWindSpeed
        {
            get
            {
                return (Vector2)SET.Properties["BHWindSpeed"].Object;
            }
            set
            {
                SET.Properties["BHWindSpeed"].Object = value;
            }
        }

        public Vector2 BVWindAngle
        {
            get
            {
                return (Vector2)SET.Properties["BVWindAngle"].Object;
            }
            set
            {
                SET.Properties["BVWindAngle"].Object = value;
            }
        }

        public Vector2 BVWindSpeed
        {
            get
            {
                return (Vector2)SET.Properties["BVWindSpeed"].Object;
            }
            set
            {
                SET.Properties["BVWindSpeed"].Object = value;
            }
        }

        public Vector2 LRockWindAngle
        {
            get
            {
                return (Vector2)SET.Properties["LRockWindAngle"].Object;
            }
            set
            {
                SET.Properties["LRockWindAngle"].Object = value;
            }
        }

        public Vector2 LRockWindSpeed
        {
            get
            {
                return (Vector2)SET.Properties["LRockWindSpeed"].Object;
            }
            set
            {
                SET.Properties["LRockWindSpeed"].Object = value;
            }
        }

        public Vector2 LRustleWindAngle
        {
            get
            {
                return (Vector2)SET.Properties["LRustleWindAngle"].Object;
            }
            set
            {
                SET.Properties["LRustleWindAngle"].Object = value;
            }
        }

        public Vector2 LRustleWindSpeed
        {
            get
            {
                return (Vector2)SET.Properties["LRustleWindSpeed"].Object;
            }
            set
            {
                SET.Properties["LRustleWindSpeed"].Object = value;
            }
        }
    }
    public class eCPhysicsScene_PS_Wrapper : PropertySetWrapper
    {
        public Boolean IsPhysicsEnabled
        {
            get
            {
                return (Boolean)SET.Properties["IsPhysicsEnabled"].Object;
            }
            set
            {
                SET.Properties["IsPhysicsEnabled"].Object = value;
            }
        }

        public Vector3 GravityVector
        {
            get
            {
                return (Vector3)SET.Properties["GravityVector"].Object;
            }
            set
            {
                SET.Properties["GravityVector"].Object = value;
            }
        }

        public Single DefaultSkinWidth
        {
            get
            {
                return (Single)SET.Properties["DefaultSkinWidth"].Object;
            }
            set
            {
                SET.Properties["DefaultSkinWidth"].Object = value;
            }
        }

        public Single DefaultSleepLinVeloSquared
        {
            get
            {
                return (Single)SET.Properties["DefaultSleepLinVeloSquared"].Object;
            }
            set
            {
                SET.Properties["DefaultSleepLinVeloSquared"].Object = value;
            }
        }

        public Single DefaultSleepAngVeloSquared
        {
            get
            {
                return (Single)SET.Properties["DefaultSleepAngVeloSquared"].Object;
            }
            set
            {
                SET.Properties["DefaultSleepAngVeloSquared"].Object = value;
            }
        }

        public Single BounceTreshold
        {
            get
            {
                return (Single)SET.Properties["BounceTreshold"].Object;
            }
            set
            {
                SET.Properties["BounceTreshold"].Object = value;
            }
        }

        public Single DynFrictionScaling
        {
            get
            {
                return (Single)SET.Properties["DynFrictionScaling"].Object;
            }
            set
            {
                SET.Properties["DynFrictionScaling"].Object = value;
            }
        }

        public Single StatFrictionScaling
        {
            get
            {
                return (Single)SET.Properties["StatFrictionScaling"].Object;
            }
            set
            {
                SET.Properties["StatFrictionScaling"].Object = value;
            }
        }

        public Single MaximumAngularVelocity
        {
            get
            {
                return (Single)SET.Properties["MaximumAngularVelocity"].Object;
            }
            set
            {
                SET.Properties["MaximumAngularVelocity"].Object = value;
            }
        }

        public Single SimulationFPS
        {
            get
            {
                return (Single)SET.Properties["SimulationFPS"].Object;
            }
            set
            {
                SET.Properties["SimulationFPS"].Object = value;
            }
        }
    }
    public class eCDirectionalLight_PS_Wrapper : PropertySetWrapper
    {
        public bCFloatColor Color
        {
            get
            {
                return (bCFloatColor)SET.Properties["Color"].Object;
            }
            set
            {
                SET.Properties["Color"].Object = value;
            }
        }

        public bCFloatColor SpecularColor
        {
            get
            {
                return (bCFloatColor)SET.Properties["SpecularColor"].Object;
            }
            set
            {
                SET.Properties["SpecularColor"].Object = value;
            }
        }

        public Single Intensity
        {
            get
            {
                return (Single)SET.Properties["Intensity"].Object;
            }
            set
            {
                SET.Properties["Intensity"].Object = value;
            }
        }

        public bCEulerAngles DirectionOffset
        {
            get
            {
                return (bCEulerAngles)SET.Properties["DirectionOffset"].Object;
            }
            set
            {
                SET.Properties["DirectionOffset"].Object = value;
            }
        }

        public Boolean Enabled
        {
            get
            {
                return (Boolean)SET.Properties["Enabled"].Object;
            }
            set
            {
                SET.Properties["Enabled"].Object = value;
            }
        }
    }
    public class eCHemisphere_PS_Wrapper : PropertySetWrapper
    {
        public bCFloatColor GeneralAmbient
        {
            get
            {
                return (bCFloatColor)SET.Properties["GeneralAmbient"].Object;
            }
            set
            {
                SET.Properties["GeneralAmbient"].Object = value;
            }
        }

        public bCFloatColor BackLight
        {
            get
            {
                return (bCFloatColor)SET.Properties["BackLight"].Object;
            }
            set
            {
                SET.Properties["BackLight"].Object = value;
            }
        }

        public bCFloatColor GroundLight
        {
            get
            {
                return (bCFloatColor)SET.Properties["GroundLight"].Object;
            }
            set
            {
                SET.Properties["GroundLight"].Object = value;
            }
        }

        public bCEulerAngles BackLightAxisDirectionOffset
        {
            get
            {
                return (bCEulerAngles)SET.Properties["BackLightAxisDirectionOffset"].Object;
            }
            set
            {
                SET.Properties["BackLightAxisDirectionOffset"].Object = value;
            }
        }

        public Single Intensity
        {
            get
            {
                return (Single)SET.Properties["Intensity"].Object;
            }
            set
            {
                SET.Properties["Intensity"].Object = value;
            }
        }

        public bCFloatColor SunLight
        {
            get
            {
                return (bCFloatColor)SET.Properties["SunLight"].Object;
            }
            set
            {
                SET.Properties["SunLight"].Object = value;
            }
        }

        public bCEulerAngles SunLightAxisDirectionOffset
        {
            get
            {
                return (bCEulerAngles)SET.Properties["SunLightAxisDirectionOffset"].Object;
            }
            set
            {
                SET.Properties["SunLightAxisDirectionOffset"].Object = value;
            }
        }

        public Boolean Enabled
        {
            get
            {
                return (Boolean)SET.Properties["Enabled"].Object;
            }
            set
            {
                SET.Properties["Enabled"].Object = value;
            }
        }
    }
    public class eCSkydome_PS_Wrapper : PropertySetWrapper
    {

        public bCImageResourceString StarTexture
        {
            get
            {
                return (bCImageResourceString)SET.Properties["StarTexture"].Object;
            }
            set
            {
                SET.Properties["StarTexture"].Object = value;
            }
        }

        public Int32 StarCount
        {
            get
            {
                return (Int32)SET.Properties["StarCount"].Object;
            }
            set
            {
                SET.Properties["StarCount"].Object = value;
            }
        }

        public bCFloatColor SunColor
        {
            get
            {
                return (bCFloatColor)SET.Properties["SunColor"].Object;
            }
            set
            {
                SET.Properties["SunColor"].Object = value;
            }
        }

        public bCImageResourceString SunTexture
        {
            get
            {
                return (bCImageResourceString)SET.Properties["SunTexture"].Object;
            }
            set
            {
                SET.Properties["SunTexture"].Object = value;
            }
        }

        public Single SunSize
        {
            get
            {
                return (Single)SET.Properties["SunSize"].Object;
            }
            set
            {
                SET.Properties["SunSize"].Object = value;
            }
        }

        public bCFloatColor MoonColor
        {
            get
            {
                return (bCFloatColor)SET.Properties["MoonColor"].Object;
            }
            set
            {
                SET.Properties["MoonColor"].Object = value;
            }
        }

        public bCImageResourceString MoonTexture
        {
            get
            {
                return (bCImageResourceString)SET.Properties["MoonTexture"].Object;
            }
            set
            {
                SET.Properties["MoonTexture"].Object = value;
            }
        }

        public Single MoonSize
        {
            get
            {
                return (Single)SET.Properties["MoonSize"].Object;
            }
            set
            {
                SET.Properties["MoonSize"].Object = value;
            }
        }

        public bCImageResourceString CloudTexture1
        {
            get
            {
                return (bCImageResourceString)SET.Properties["CloudTexture1"].Object;
            }
            set
            {
                SET.Properties["CloudTexture1"].Object = value;
            }
        }

        public bCImageResourceString CloudTexture2
        {
            get
            {
                return (bCImageResourceString)SET.Properties["CloudTexture2"].Object;
            }
            set
            {
                SET.Properties["CloudTexture2"].Object = value;
            }
        }

        public Single CloudThickness
        {
            get
            {
                return (Single)SET.Properties["CloudThickness"].Object;
            }
            set
            {
                SET.Properties["CloudThickness"].Object = value;
            }
        }

        public bCFloatColor CloudColor
        {
            get
            {
                return (bCFloatColor)SET.Properties["CloudColor"].Object;
            }
            set
            {
                SET.Properties["CloudColor"].Object = value;
            }
        }

        public Vector2 CloudDirection
        {
            get
            {
                return (Vector2)SET.Properties["CloudDirection"].Object;
            }
            set
            {
                SET.Properties["CloudDirection"].Object = value;
            }
        }

        public Single CloudSize1
        {
            get
            {
                return (Single)SET.Properties["CloudSize1"].Object;
            }
            set
            {
                SET.Properties["CloudSize1"].Object = value;
            }
        }

        public Single CloudSize2
        {
            get
            {
                return (Single)SET.Properties["CloudSize2"].Object;
            }
            set
            {
                SET.Properties["CloudSize2"].Object = value;
            }
        }
    }
    public class eCPrecipitation_PS_Wrapper : PropertySetWrapper
    {
        public override void LoadModels(API_Device D)
        {
            base.LoadModels(D);
            string q0 = Texture.pString;
            Obj.addICON(q0, D);
        }

        public bCImageResourceString Texture
        {
            get
            {
                return (bCImageResourceString)SET.Properties["Texture"].Object;
            }
            set
            {
                SET.Properties["Texture"].Object = value;
                Obj.InvalidateModels();
            }
        }

        public Single PrecipitationCubeSize
        {
            get
            {
                return (Single)SET.Properties["PrecipitationCubeSize"].Object;
            }
            set
            {
                SET.Properties["PrecipitationCubeSize"].Object = value;
            }
        }

        public Int32 MaxParticles
        {
            get
            {
                return (Int32)SET.Properties["MaxParticles"].Object;
            }
            set
            {
                SET.Properties["MaxParticles"].Object = value;
            }
        }

        public Int32 ParticlesPerSecond
        {
            get
            {
                return (Int32)SET.Properties["ParticlesPerSecond"].Object;
            }
            set
            {
                SET.Properties["ParticlesPerSecond"].Object = value;
            }
        }

        public Vector3 Direction
        {
            get
            {
                return (Vector3)SET.Properties["Direction"].Object;
            }
            set
            {
                SET.Properties["Direction"].Object = value;
            }
        }

        public Vector2 SpeedMinMax
        {
            get
            {
                return (Vector2)SET.Properties["SpeedMinMax"].Object;
            }
            set
            {
                SET.Properties["SpeedMinMax"].Object = value;
            }
        }

        public Vector2 Size
        {
            get
            {
                return (Vector2)SET.Properties["Size"].Object;
            }
            set
            {
                SET.Properties["Size"].Object = value;
            }
        }

        public Int32 MaxSpawnPoints
        {
            get
            {
                return (Int32)SET.Properties["MaxSpawnPoints"].Object;
            }
            set
            {
                SET.Properties["MaxSpawnPoints"].Object = value;
            }
        }

        public Single DirectionScale
        {
            get
            {
                return (Single)SET.Properties["DirectionScale"].Object;
            }
            set
            {
                SET.Properties["DirectionScale"].Object = value;
            }
        }

        public Int32 TextureTileCountU
        {
            get
            {
                return (Int32)SET.Properties["TextureTileCountU"].Object;
            }
            set
            {
                SET.Properties["TextureTileCountU"].Object = value;
            }
        }

        public Int32 TextureTileCountV
        {
            get
            {
                return (Int32)SET.Properties["TextureTileCountV"].Object;
            }
            set
            {
                SET.Properties["TextureTileCountV"].Object = value;
            }
        }

        public Int32 CurrentTextureTile
        {
            get
            {
                return (Int32)SET.Properties["CurrentTextureTile"].Object;
            }
            set
            {
                SET.Properties["CurrentTextureTile"].Object = value;
            }
        }

        public Single SpawnPointDizzer
        {
            get
            {
                return (Single)SET.Properties["SpawnPointDizzer"].Object;
            }
            set
            {
                SET.Properties["SpawnPointDizzer"].Object = value;
            }
        }

        public Single JitterPower
        {
            get
            {
                return (Single)SET.Properties["JitterPower"].Object;
            }
            set
            {
                SET.Properties["JitterPower"].Object = value;
            }
        }

        public Single JitterSpeed
        {
            get
            {
                return (Single)SET.Properties["JitterSpeed"].Object;
            }
            set
            {
                SET.Properties["JitterSpeed"].Object = value;
            }
        }

        public Single SplashDuration
        {
            get
            {
                return (Single)SET.Properties["SplashDuration"].Object;
            }
            set
            {
                SET.Properties["SplashDuration"].Object = value;
            }
        }

        public Int32 SplashTextureTile
        {
            get
            {
                return (Int32)SET.Properties["SplashTextureTile"].Object;
            }
            set
            {
                SET.Properties["SplashTextureTile"].Object = value;
            }
        }

        public Vector2 SplashSizeScale
        {
            get
            {
                return (Vector2)SET.Properties["SplashSizeScale"].Object;
            }
            set
            {
                SET.Properties["SplashSizeScale"].Object = value;
            }
        }
    }
    public class eCSpeedTreeBBMesh_PS_Wrapper : PropertySetWrapper
    {
        public bCString MeshFileName
        {
            get
            {
                return (bCString)SET.Properties["MeshFileName"].Object;
            }
            set
            {
                SET.Properties["MeshFileName"].Object = value;
            }
        }

        public Int32 MaxSubMeshTriangles
        {
            get
            {
                return (Int32)SET.Properties["MaxSubMeshTriangles"].Object;
            }
            set
            {
                SET.Properties["MaxSubMeshTriangles"].Object = value;
            }
        }
    }
    public class gCStateGraph_PS_Wrapper : PropertySetWrapper
    {
        public BUFFER States
        {
            get
            {
                return (BUFFER)SET.Properties["States"].Object;
            }
            set
            {
                SET.Properties["States"].Object = value;
            }
        }

        public bCString DefaultState
        {
            get
            {
                return (bCString)SET.Properties["DefaultState"].Object;
            }
            set
            {
                SET.Properties["DefaultState"].Object = value;
            }
        }

        public RisenEditor.Code.RisenTypes.bTObjArray<RisenEditor.Code.RisenTypes.eCEntityProxy> TriggerTargets
        {
            get
            {
                return (RisenEditor.Code.RisenTypes.bTObjArray<RisenEditor.Code.RisenTypes.eCEntityProxy>)SET.Properties["TriggerTargets"].Object;
            }
            set
            {
                SET.Properties["TriggerTargets"].Object = value;
            }
        }

        public eCScriptProxyScript OnEnterState
        {
            get
            {
                return (eCScriptProxyScript)SET.Properties["OnEnterState"].Object;
            }
            set
            {
                SET.Properties["OnEnterState"].Object = value;
            }
        }

        public eCScriptProxyScript OnExitState
        {
            get
            {
                return (eCScriptProxyScript)SET.Properties["OnExitState"].Object;
            }
            set
            {
                SET.Properties["OnExitState"].Object = value;
            }
        }

        public BUFFER TriggerEventFilter
        {
            get
            {
                return (BUFFER)SET.Properties["TriggerEventFilter"].Object;
            }
            set
            {
                SET.Properties["TriggerEventFilter"].Object = value;
            }
        }

        public BUFFER TouchEventFilter
        {
            get
            {
                return (BUFFER)SET.Properties["TouchEventFilter"].Object;
            }
            set
            {
                SET.Properties["TouchEventFilter"].Object = value;
            }
        }
    }
    #endregion
}
