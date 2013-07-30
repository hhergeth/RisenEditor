using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SlimDX;
using GameLibrary;
using GameLibrary.Objekte;
using RisenEditor.Code;
using RisenEditor.Code.Loader;
using RisenEditor.Code.Renderer;
using RisenEditor.Code.RisenTypes;
using System.Reflection;

namespace RisenEditor.Code
{
    public static class TypeCache
    {
        static Dictionary<string, Type> m_Types_Short = new Dictionary<string, Type>();
        static Dictionary<string, Type> m_Types_Long = new Dictionary<string, Type>();
        public static Type GetType(string shortName)
        {
            Type t0 = Assembly.GetCallingAssembly().GetType(shortName);
            if (t0 != null)
                return t0;

            if (Assembly.GetEntryAssembly().GetType(shortName) != null)
                return Assembly.GetEntryAssembly().GetType(shortName);
            if (Assembly.GetAssembly(typeof(Vector3)).GetType("SlimDX." + shortName) != null)
                return Assembly.GetAssembly(typeof(Vector3)).GetType("SlimDX." + shortName);
            if (Assembly.GetAssembly(typeof(System.Int32)).GetType("System." + shortName) != null)
                return Assembly.GetAssembly(typeof(System.Int32)).GetType("System." + shortName);
            switch (shortName)
            {
                case "int":
                    return typeof(int);
                case "float":
                    return typeof(float);
                case "long":
                    return typeof(long);
                case "double":
                    return typeof(double);
                case "byte":
                    return typeof(byte);
                case "char":
                    return typeof(char);
                case "short":
                    return typeof(short);
                case "ushort":
                    return typeof(ushort);
                case "uint":
                    return typeof(uint);
            }

            if (m_Types_Short.Count == 0)
            {
                IEnumerable<Type> types = Assembly.GetEntryAssembly().GetTypes();
                foreach (Type t in types)
                {
                    if (!m_Types_Short.ContainsKey(t.Name))
                        m_Types_Short.Add(t.Name, t);
                    m_Types_Long.Add(t.FullName, t);
                }
            }
            if (m_Types_Short.ContainsKey(shortName))
                return m_Types_Short[shortName];
            if (m_Types_Long.ContainsKey(shortName))
                return m_Types_Long[shortName];

            return null;
        }

        public static string GetFriendlyTypeName(Type type)
        {
            if (type.IsGenericParameter)
            {
                return type.Name;
            }

            if (!type.IsGenericType)
            {
                return type.FullName;
            }

            var builder = new System.Text.StringBuilder();
            var name = type.Name;
            var index = name.IndexOf("`");
            builder.AppendFormat("{0}.{1}", type.Namespace, name.Substring(0, index));
            builder.Append('<');
            var first = true;
            foreach (var arg in type.GetGenericArguments())
            {
                if (!first)
                {
                    builder.Append(',');
                }
                builder.Append(GetFriendlyTypeName(arg));
                first = false;
            }
            builder.Append('>');
            return builder.ToString();
        }
    }

    public static class GENOMEMath
    {
        public static Vector3 fromGENOME(Vector3 v)
        {
            float q = v.X;
            v.X = v.Z;
            v.Z = q;
            return v;
        }

        public static Matrix fromGENOME(Matrix m)
        {/*
            Matrix copy = m;
            m.set_Columns(0, copy.get_Columns(2));
            m.set_Columns(2, copy.get_Columns(0));
            copy = m;
            m.set_Rows(0, copy.get_Columns(2));
            m.set_Rows(2, copy.get_Columns(0));
            return m;*/

            float q1 = m.M11;
            m.M11 = m.M13;
            m.M13 = q1;

            float q2 = m.M21;
            m.M21 = m.M23;
            m.M23 = q2;

            float q3 = m.M31;
            m.M31 = m.M33;
            m.M33 = q3;

            q1 = m.M11;
            m.M11 = m.M31;
            m.M31 = q1;

            q2 = m.M12;
            m.M12 = m.M32;
            m.M32 = q2;

            q3 = m.M13;
            m.M13 = m.M33;
            m.M33 = q3;

            float q = m.M41;
            m.M41 = m.M43;
            m.M43 = q;

            return m;
        }

        public static Vector3 toGENOME(Vector3 v)
        {
            return fromGENOME(v);
        }

        public static Matrix toGENOME(Matrix m)
        {
            float q1 = m.M11;
            m.M11 = m.M31;
            m.M31 = q1;

            float q2 = m.M12;
            m.M12 = m.M32;
            m.M32 = q2;

            float q3 = m.M13;
            m.M13 = m.M33;
            m.M33 = q3;

            q1 = m.M11;
            m.M11 = m.M13;
            m.M13 = q1;

            q2 = m.M21;
            m.M21 = m.M23;
            m.M23 = q2;

            q3 = m.M31;
            m.M31 = m.M33;
            m.M33 = q3;

            float q = m.M41;
            m.M41 = m.M43;
            m.M43 = q;

            return m;
        }

        public static BoundingBox fromGENOME(BoundingBox bb)
        {
            return new BoundingBox(fromGENOME(bb.Minimum),  fromGENOME(bb.Maximum));
        }

        public static BoundingBox toGENOME(BoundingBox bb)
        {
            return new BoundingBox(toGENOME(bb.Minimum), toGENOME(bb.Maximum));
        }

        public static Vector3 tG(this Vector3 v)
        {
            return toGENOME(v);
        }

        public static Vector3 fG(this Vector3 v)
        {
            return fromGENOME(v);
        }

        public static Quaternion tG(this Quaternion q)
        {
            Matrix m = Matrix.RotationQuaternion(q);
            m = toGENOME(m);
            Vector3 v1, v2;
            m.Decompose(out v1, out q, out v2);
            return q;
        }

        public static int SizeOf<T>(this List<T> a_List) where T : BinaryFileBlock
        {
            int i = 4;
            for (int j = 0; j < a_List.Count; j++)
                i += a_List[j] != null ? a_List[j].Size : 0;
            return i;
        }
        public static List<T> DeepClone<T>(this List<T> a_List) where T : BinaryFileBlock
        {
            List<T> n = new List<T>();
            foreach (T t in a_List)
                n.Add((T)t.Clone());
            return n;
        }
        public static List<T> DeepClone_Struct<T>(this List<T> a_List) where T : struct
        {
            List<T> n = new List<T>();
            foreach (T t in a_List)
                n.Add(t);
            return n;
        }

        public static List<Type> FindAllDerivedTypes<T>()
        {
            return FindAllDerivedTypes<T>(Assembly.GetAssembly(typeof(T)));
        }

        public static List<Type> FindAllDerivedTypes<T>(Assembly assembly)
        {
            var derivedType = typeof(T);
            return assembly
                .GetTypes()
                .Where(t =>
                    t != derivedType &&
                    derivedType.IsAssignableFrom(t)
                    ).ToList();

        }
        public static object ConstructFromString<T>(string s)
        {
            Type t = typeof(T);
            if (t == typeof(int))
                return int.Parse(s);
            else if (t == typeof(float))
                return float.Parse(s);
            else if (t == typeof(Vector3))
            {
                string[] a = s.Replace("{", "").Replace("}", "").Split(',');
                return new Vector3(float.Parse(a[0]), float.Parse(a[1]), float.Parse(a[2]));
            }
            else if (t == typeof(bCString))
                return new bCString(s);
            else if (t.IsEnum)
                return Enum.Parse(t, s);
            return default(T);
        }
        public static string CreatebCAccessor(bCAccessorPropertyObject a, string Obj)
        {
            StringBuilder SB = new StringBuilder();
            foreach (bCProperty b in a.Properties)
            {
                string q = ((char)34).ToString();
                string c = Obj + ".Properties.addProperty(" + q + b.PropertyName + q + ", " + q + b.PropertyType + q + ", null);";
                SB.AppendLine(c);
            }
            return SB.ToString();
        }

        public static List<T> RepeatedDefault<T>(int count)
        {
            return Repeated(default(T), count);
        }

        public static List<T> Repeated<T>(T value, int count)
        {
            List<T> ret = new List<T>(count);
            ret.AddRange(Enumerable.Repeat(value, count));
            return ret;
        }
    }

    public delegate void WorldCleared();

    public delegate void FilesAdded(List<LrentFile> newFiles);

    public delegate void LoadingFinished();

    public delegate void EntityTransformed(ILrentObject a_Entity);

    public delegate void EntityAdded(ICollection<ILrentObject> a_Entity);

    public delegate void EntityDeleted(ILrentObject a_Entity);

    public static class RisenWorld
    {
        public const float PanSpeed = 100;

        public class Indexer : IEnumerable<LrentFile>
        {
            public LrentFile this[int index]
            {
                get
                {
                    return RisenWorld._Files[index];
                }
            }

            public LrentFile this[string s]
            {
                get
                {
                    foreach (LrentFile f in RisenWorld._Files)
                        if (f.Name == s)
                            return f;
                    return null;
                }
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return this.GetEnumerator();
            }

            public IEnumerator<LrentFile> GetEnumerator()
            {
                return (IEnumerator<LrentFile>)RisenWorld._Files.GetEnumerator();
            }
        }

        static List<LrentFile> _Files = new List<LrentFile>();

        public static void AddLrents(List<LrentFile> newFiles)
        {
            if (newFiles == null || newFiles.Count == 0)
                return;
            _Files.AddRange(newFiles);
            if (LrentFileAdded != null)
                LrentFileAdded(newFiles);
        }

        public static List<LrentFile> LrentFiles2
        {
            get
            {
                return _Files;
            }
        }

        public static Indexer Files
        {
            get
            {
                return new Indexer();
            }
        }

        public static void Save()
        {
            foreach (LrentFile lf in _Files)
            {
                lf.SaveFile();
            }
        }

        public static void Clear(DeviceApplication DA)
        {
            _Files.Clear();
            if (WorldCleared != null)
                WorldCleared();
            VegetationManager.clear();
            NodeLibrary nl = ManagedWorld.NodeLibrary;
            ManagedWorld.NodeLibrary = new NodeLibrary(DA.Device);
            nl.Dispose();
            ManagedWorld.NodeLibrary.Camera = new Camera(null, Vector3.Zero, new Vector3(0, 0, 100), MathHelper.PiOver2, DA);
            ManagedWorld.NodeLibrary.Camera.PanSpeed = PanSpeed;
        }

        public static void OnLoadingFinished()
        {
            if (LoadingFinished != null)
                LoadingFinished();
        }

        public static void OnEntityMoved(ILrentObject a_Entity)
        {
            if (EntityTransformed != null)
                EntityTransformed(a_Entity);
            foreach (GraphicNode gn in a_Entity.Nodes)
                ManagedWorld.NodeLibrary.OcTree.UpdateNode(gn);
            a_Entity.File.UpdateContextBox();
        }

        public static void OnEntityAdded(params ILrentObject[] a_Entity)
        {
            if (EntityAdded != null)
                EntityAdded(a_Entity);
        }

        public static void OnEntityDeleted(ILrentObject a_Entity)
        {
            if (EntityDeleted != null)
                EntityDeleted(a_Entity);
        }

        public static event EntityDeleted EntityDeleted;

        public static event EntityAdded EntityAdded;

        public static event EntityTransformed EntityTransformed;

        public static event WorldCleared WorldCleared;

        public static event FilesAdded LrentFileAdded;

        public static event LoadingFinished LoadingFinished;
    }

    public class RenderCollection
    {
        public List<GraphicNode> col;

        public RenderCollection()
        {
            col = new List<GraphicNode>();
            col.Capacity = 256;
        }

        public GraphicNode this[int index]
        {
            get
            {
                return col[index];
            }
            set
            {
                col[index] = value;
            }
        }

        public void Add(GraphicNode gn)
        {
            if (!gn.Visible) return;
            ILrentObject.ObjectTagger t = gn.Tag as ILrentObject.ObjectTagger;
            if (t == null || t.AddedToList) return;
            t.AddedToList = true;
            col.Add(gn);
        }

        public void AddRange(List<GraphicNode> GN)
        {
            foreach (GraphicNode gn in GN)
                Add(gn);
        }

        public void AddRange(List<GraphicNode> GN, ref BoundingFrustum F)
        {
            foreach (GraphicNode gn in GN)
                if(F.Contains(gn.BoundingBox_ABS) != ContainmentType.Disjoint)
                    Add(gn);
        }

        public void Render(GameLibrary.Rendering.RenderInformation RI, GameLibrary.Rendering.ObjektRenderer TR)
        {
            foreach (GraphicNode gn in col)
            {
                if (gn.Mesh == null)
                    continue;
                RI.WorldMatrix = gn.ModelMatrix_ABS;
                TR.PrepareForNode(RI, gn);
                foreach (MeshPart mp in gn.Mesh.Parts)
                    TR.DrawSubObjekt(RI, mp, gn as GraphicNode);
                GameLibrary.Rendering.DrawFrame.NodesDrawn++;
            }
        }

        public void Clear()
        {
            foreach (GraphicNode gn in col)
                (gn.Tag as ILrentObject.ObjectTagger).AddedToList = false;
            col.Clear();
        }

        public int NodeCount
        {
            get
            {
                return col.Count;
            }
        }

        public void FinishCollecting()
        {
            foreach (GraphicNode gn in col)
                if(gn.Tag is ILrentObject.ObjectTagger)
                    (gn.Tag as ILrentObject.ObjectTagger).AddedToList = false;
        }
    }

    internal class Triangulator
    {
        Vector3[] V;

        public Triangulator(ICollection<Vector3> v)
        {
            V = new List<Vector3>(v).ToArray();
        }

        public Vector3[] GetTriangles()
        {
            List<Vector3> v_R = new List<Vector3>();
            Vector3 v1 = V[0];
            for (int i = 0; i < V.Length - 2; ++i)
            {
                Vector3 v2 = V[i + 1];
                Vector3 v3 = V[i + 2];
                v_R.Add(v1); v_R.Add(v2); v_R.Add(v3);
            }
            return v_R.ToArray();
        }
    }

    public class PortalRoomTriangle
    {
        public PortalRoomNode owner;
        public Vector3 v0, v1, v2;
        public Vector3 normal2;
        public PortalRoomTriangle(Vector3 t0, Vector3 t1, Vector3 t2, Vector3 n, PortalRoomNode O)
        {
            v0 = t0; v1 = t1; v2 = t2; normal2 = n; owner = O;
        }
        public bool Intersect(ref Ray a_Ray, ref Vector3 end, ref PortalRoom.HitInfo a_Info)
        {
            Vector3 POSITION = a_Ray.Position;
            Vector3 TMP;
            Vector3.Subtract(ref POSITION, ref v0, out TMP);
            float f1 = Vector3.Dot(TMP, normal2);
            float f2 = Vector3.Dot(a_Ray.Direction, normal2);
            float f3 = -(f1 / f2);

            if ((f3 < 0) || (f3 > a_Info.m_Distance))
                return false;

            Vector3 pq = end;
            Vector3 pa;
            Vector3 pb;
            Vector3 pc;
            Vector3.Subtract(ref v0, ref POSITION, out pa);
            Vector3.Subtract(ref v1, ref POSITION, out pb);
            Vector3.Subtract(ref v2, ref POSITION, out pc);
            float u = 0, v = 0, w = 0;
            Vector3.Cross(ref pq, ref pc, out TMP);
            u = Vector3.Dot(TMP, pb);
            if (u < 0.0f)
                return false;
            Vector3.Cross(ref pq, ref pa, out TMP);
            v = Vector3.Dot(TMP, pc);
            if (v < 0.0f)
                return false;
            Vector3.Cross(ref pq, ref pb, out TMP);
            w = Vector3.Dot(TMP, pa);
            if (w < 0.0f)
                return false;

            Vector3 hit = a_Ray.Position + a_Ray.Direction * f3;
            foreach (GameLibrary.Tuple<Vector3[], BoundingBox> p in owner.m_SamplePoints)
                if (p.Object1.Contains(hit) == ContainmentType.Contains)
                    return false;

            a_Info.m_Distance = f3;
            a_Info.m_Primitive = this;
            return true;
        }
        public bool IntersectBack(ref Ray a_Ray, ref Vector3 end, ref PortalRoom.HitInfo a_Info)
        {
            Vector3 POSITION = a_Ray.Position;
            Vector3 TMP;
            Vector3.Subtract(ref POSITION, ref v0, out TMP);
            float f1 = Vector3.Dot(TMP, normal2);
            float f2 = Vector3.Dot(a_Ray.Direction, normal2);
            float f3 = -(f1 / f2);

            if ((f3 < 0) || (f3 > a_Info.m_Distance))
                return false;

            Vector3 pq = end;
            Vector3 pa;
            Vector3 pb;
            Vector3 pc;
            Vector3.Subtract(ref v2, ref POSITION, out pa);
            Vector3.Subtract(ref v1, ref POSITION, out pb);
            Vector3.Subtract(ref v0, ref POSITION, out pc);
            float u = 0, v = 0, w = 0;
            Vector3.Cross(ref pq, ref pc, out TMP);
            u = Vector3.Dot(TMP, pb);
            if (u < 0.0f)
                return false;
            Vector3.Cross(ref pq, ref pa, out TMP);
            v = Vector3.Dot(TMP, pc);
            if (v < 0.0f)
                return false;
            Vector3.Cross(ref pq, ref pb, out TMP);
            w = Vector3.Dot(TMP, pa);
            if (w < 0.0f)
                return false;

            Vector3 hit = a_Ray.Position + a_Ray.Direction * f3;
            foreach (GameLibrary.Tuple<Vector3[], BoundingBox> p in owner.m_SamplePoints)
                if (p.Object1.Contains(hit) == ContainmentType.Contains)
                    return false;

            a_Info.m_Distance = f3;
            a_Info.m_Primitive = this;
            return true;
        }
    }

    public class PortalRoomNode : Node
    {
        public ILrentObject m_Object;
        public List<ILrentObject> m_Children;
        public eCPortalRoom_PS m_Class;
        public List<GameLibrary.Tuple<Vector3[], BoundingBox>> m_SamplePoints;
        public BoundingBox m_Box;
        public PortalRoom m_Room;

        public PortalRoomNode(ILrentObject a_Entity)
        {
            m_Children = new List<ILrentObject>();
            ManagedWorld.NodeLibrary.RemoveNode(this);
            this.Initialize(null, a_Entity.Position, a_Entity.Size, LrentImporter.DynamicNodes);
            this.BoundingBox = a_Entity.BoundingBox_LOCAL;
            m_Object = a_Entity;
            m_Class = (a_Entity.getAccessor("eCPortalRoom_PS").Class as eCPortalRoom_PS);
            m_Box = GENOMEMath.fromGENOME(m_Class.Boundary).Transform(a_Entity.Matrix);
            m_Box = m_Box.Enlarge(0.1f);
            m_SamplePoints = new List<GameLibrary.Tuple<Vector3[], BoundingBox>>();
            foreach (eCPortalRoom_PS.eCPortal p in m_Class.Portals)
            {
                Vector3[] v = new Vector3[4];
                for (int i = 0; i < 4; i++)
                    v[i] = p.PortalRect.Points[i].fG();
                GameLibrary.Tuple<Vector3[], BoundingBox> t = new GameLibrary.Tuple<Vector3[], BoundingBox>(v, GENOMEMath.fromGENOME(p.Merged).Enlarge(1));
                m_SamplePoints.Add(t);
            }
            m_Room = new PortalRoom(m_Class.BSP, this);
        }

        public void SearchChildNodes()
        {
            IList<Node> N = ManagedWorld.NodeLibrary.OcTree.FindNodes(m_Object.BoundingBox);
            foreach (Node n in N)
            {
                GraphicNode gn = n as GraphicNode;
                if (gn == null || (gn.Path.Contains("L01") && !gn.Path.Contains("In_"))) continue;
                if (n.Tag is ILrentObject.ObjectTagger && testBox(gn))
                {
                    ILrentObject o = (n.Tag as ILrentObject.ObjectTagger).Object;
                    if (o.Name == "Levelmesh_Landscape_01") continue;
                    AddChild(o);
                }
            }
        }

        public void AddChild(ILrentObject a)
        {
            if (!m_Children.Contains(a))
            {
                m_Children.Add(a);
                foreach (GraphicNode gn in a.Nodes)
                    (gn.Tag as ILrentObject.ObjectTagger).PortalRoom = this;
            }
        }

        private bool testBox(GraphicNode gn)
        {
            PortalRoom.HitInfo h = new PortalRoom.HitInfo();
            float fn = 0, ff = float.MaxValue;
            Ray r = new Ray(gn.Position_ABS, m_Box.Center() - gn.Position_ABS);
            m_Room.Traverse(ref r, ref h, fn, ff);
            if (h.m_Primitive == null) return true;
            return Vector3.Dot(h.m_Primitive.normal2, h.m_Primitive.v0 - r.Position) < 0;
            return true;
        }
    }

    public class PortalRoom
    {
        public class HitInfo
        {
            public PortalRoomTriangle m_Primitive;
            public float m_Distance;
            public HitInfo() { m_Distance = float.MaxValue; }
        }

        int axis = -1;
        float splitpos = 0;
        PortalRoom leftChild, rightChild;

        List<PortalRoomTriangle> triangles;

        const int c_MaxDepth = 12;

        public PortalRoom(eCPortalRoom_PS.eCPortalBSP a_Native, PortalRoomNode a_Owner)
        {
            if (a_Native.child1 != null || a_Native.child2 != null)
            {
                Vector3 p = a_Native.Plane.Normal.fG().Abs();
                axis = p.X > p.Y ? (p.X > p.Z ? 0 : 2) : (p.Y > p.Z ? 1 : 2);
                splitpos = Math.Sign(a_Native.Plane.Normal.fG()[axis]) * a_Native.Plane.D;
                if (a_Native.child1 != null)
                    leftChild = new PortalRoom(a_Native.child1, a_Owner);
                if (a_Native.child2 != null)
                    rightChild = new PortalRoom(a_Native.child2, a_Owner);
            }
            else
            {
                triangles = new List<PortalRoomTriangle>();
                foreach (eCPortalRoom_PS.BSPPolygon p in a_Native.Polygons)
                {
                    if (p.ValidPointCount > 3)
                    {
                        Triangulator T = new Triangulator(p.Points.GetRange(0, p.ValidPointCount));
                        Vector3[] V = T.GetTriangles();
                        for(int i = 0; i < V.Length; i+=3)
                            triangles.Add(new PortalRoomTriangle(V[i + 0].fG(), V[i + 1].fG(), V[i + 2].fG(), p.Plane.Normal.fG(), a_Owner));
                    }
                    else triangles.Add(new PortalRoomTriangle(p.Points[0].fG(), p.Points[1].fG(), p.Points[2].fG(), p.Plane.Normal.fG(), a_Owner));
                }
            }
        }

        public PortalRoom(List<PortalRoomTriangle> a_Triangles, int depth, BoundingBox BB)
        {
            if (depth > c_MaxDepth || a_Triangles.Count < 8)
            {
                triangles = a_Triangles;
            }
            else
            {
                Vector3 p = BB.Maximum - BB.Minimum;
                axis = p.X > p.Y ? (p.X > p.Z ? 0 : 2) : (p.Y > p.Z ? 1 : 2);
                splitpos = (BB.Minimum[axis] + BB.Maximum[axis]) / 2.0f;
                BoundingBox bb_left = BB, bb_right = BB;
                bb_left.Maximum[axis] = splitpos; bb_right.Minimum[axis] = splitpos;
                List<PortalRoomTriangle> leftPolygons = new List<PortalRoomTriangle>();
                List<PortalRoomTriangle> rightPolygons = new List<PortalRoomTriangle>();
                foreach (PortalRoomTriangle p2 in a_Triangles)
                {
                    float min = Math.Min(Math.Min(p2.v0[axis], p2.v1[axis]), p2.v2[axis]);
                    float max = Math.Max(Math.Max(p2.v0[axis], p2.v1[axis]), p2.v2[axis]);
                    if (min <= splitpos)
                        leftPolygons.Add(p2);
                    if (max >= splitpos)
                        rightPolygons.Add(p2);
                }
                leftChild = new PortalRoom(leftPolygons, depth + 1, bb_left);
                rightChild = new PortalRoom(rightPolygons, depth + 1, bb_right);
            }
        }

        static public PortalRoom Create(List<PortalRoomNode> rooms)
        {
            List<PortalRoomTriangle> a_Polygons = new List<PortalRoomTriangle>();
            Stack<eCPortalRoom_PS.eCPortalBSP> v_Stack = new Stack<eCPortalRoom_PS.eCPortalBSP>();
            BoundingBox BB = new BoundingBox(new Vector3(float.MaxValue), new Vector3(float.MinValue));
            foreach (PortalRoomNode r2 in rooms)
            {
                BB = BB.Extend(r2.BoundingBox);
                eCPortalRoom_PS r = r2.m_Object.getAccessor("eCPortalRoom_PS").Class as eCPortalRoom_PS;
                v_Stack.Push(r.BSP);
                while (v_Stack.Count != 0)
                {
                    eCPortalRoom_PS.eCPortalBSP c = v_Stack.Pop();             
                    if (c.child1 != null)
                        v_Stack.Push(c.child1);
                    if (c.child2 != null)
                        v_Stack.Push(c.child2);
                    if (c.Polygons != null && c.Polygons.Count > 0)
                    {
                        foreach (eCPortalRoom_PS.BSPPolygon p in c.Polygons)
                        {

                            if (p.ValidPointCount > 3)
                            {
                                Triangulator T = new Triangulator(p.Points.GetRange(0, p.ValidPointCount));
                                Vector3[] V = T.GetTriangles();
                                for (int i = 0; i < V.Length; i += 3)
                                    a_Polygons.Add(new PortalRoomTriangle(V[i + 0].fG(), V[i + 1].fG(), V[i + 2].fG(), p.Plane.Normal.fG(), r2));
                            }
                            else a_Polygons.Add(new PortalRoomTriangle(p.Points[0].fG(), p.Points[1].fG(), p.Points[2].fG(), p.Plane.Normal.fG(), r2));
                        }
                    }
                }
            }
            return new PortalRoom(a_Polygons, 0, BB);
        }

        public bool Traverse(ref Ray a_ray, ref HitInfo a_Info, float t_Near, float t_Far)
        {
            if (triangles != null)
            {
                Vector3 end = a_ray.Position + a_ray.Direction * 100000000.0f;
                if (t_Near > t_Far) return false;
                bool res = false;
                for (int i = 0; i < triangles.Count; i++)
                    if (triangles[i].Intersect(ref a_ray, ref end, ref a_Info) || triangles[i].IntersectBack(ref a_ray, ref end, ref a_Info))
                        res = true;
                return res;
            }
            else
            {
                if (rightChild == null)
                    return leftChild.Traverse(ref a_ray, ref a_Info, t_Near, t_Far);
                if (leftChild == null)
                    return rightChild.Traverse(ref a_ray, ref a_Info, t_Near, t_Far);

                if (a_ray.Direction[axis] == 0)//Ray is parallal to the splitting plane?
                {
                    if (a_ray.Position[axis] <= splitpos)//(Left or Right ?)
                        return leftChild.Traverse(ref a_ray, ref a_Info, t_Near, t_Far);
                    else
                        return rightChild.Traverse(ref a_ray, ref a_Info, t_Near, t_Far);
                }
                float t_split = (splitpos - a_ray.Position[axis]) / a_ray.Direction[axis];
                if (t_split >= t_Far)
                {
                    if (a_ray.Direction[axis] > 0)
                        return leftChild.Traverse(ref a_ray, ref a_Info, t_Near, t_Far);
                    else
                        return rightChild.Traverse(ref a_ray, ref a_Info, t_Near, t_Far);
                }
                else if (t_split <= t_Near)
                {
                    if (a_ray.Direction[axis] > 0)
                        return rightChild.Traverse(ref a_ray, ref a_Info, t_Near, t_Far);
                    else
                        return leftChild.Traverse(ref a_ray, ref a_Info, t_Near, t_Far);
                }
                else if (t_split > t_Near && t_split < t_Far)//Ray hit the two childs
                {
                    if (a_ray.Direction[axis] > 0)
                    {
                        if (leftChild.Traverse(ref a_ray, ref a_Info, t_Near, t_split))
                            return true;
                        return rightChild.Traverse(ref a_ray, ref a_Info, t_split, t_Far);
                    }
                    else
                    {
                        if (rightChild.Traverse(ref a_ray, ref a_Info, t_Near, t_split))
                            return true;
                        return leftChild.Traverse(ref a_ray, ref a_Info, t_split, t_Far);
                    }
                }
            }
            return false;
        }

        public void Remove(eCPortalRoom_PS a_O)
        {
            if (triangles != null)
                for (int i = triangles.Count - 1; i > 0; i--)
                    if (triangles[i].owner.m_Class == a_O)
                        triangles.RemoveAt(i);
            if (leftChild != null)
                leftChild.Remove(a_O);
            if (rightChild != null)
                rightChild.Remove(a_O);
        }
    }

    public static class PortalRoomManager
    {
        static PortalRoom m_PortalTree;
        static OcTree m_Tree;
        static Dictionary<ILrentObject, PortalRoomNode> m_Rooms;//contains the same entities as the tree

        static PortalRoomManager()
        {/*
            m_Rooms = new Dictionary<ILrentObject, PortalRoomNode>();
            RisenWorld.WorldCleared += new WorldCleared(RisenWorld_WorldCleared);
            RisenWorld.LoadingFinished += new LoadingFinished(RisenWorld_LoadingFinished);
            RisenWorld.EntityTransformed += new EntityTransformed(RisenWorld_EntityTransformed);
            RisenWorld.EntityAdded += new EntityAdded(RisenWorld_EntityAdded);
            RisenWorld.EntityDeleted += new EntityDeleted(RisenWorld_EntityDeleted);
            RisenWorld_WorldCleared();*/
        }

        static void RisenWorld_LoadingFinished()
        {
            RisenWorld_WorldCleared();
            foreach (LrentFile f in RisenWorld.Files)
            {
                foreach (ILrentObject O in f)
                    if (O.getSet<eCPortalRoom_PS_Wrapper>() != null)
                        m_Rooms.Add(O, new PortalRoomNode(O));
            }
            m_Tree = new OcTree();
            m_Tree.NodeList = m_Rooms.Values.Cast<Node>().ToList();
            m_PortalTree = PortalRoom.Create(m_Rooms.Values.ToList());
            foreach (PortalRoomNode n in m_Rooms.Values)
                n.SearchChildNodes();
        }

        static void RisenWorld_EntityDeleted(ILrentObject a_Entity)
        {
            if(m_Rooms.ContainsKey(a_Entity))
            {
                PortalRoomNode n = m_Rooms[a_Entity];
                m_Rooms.Remove(a_Entity);
                m_Tree.RemoveNode(n);
                m_PortalTree.Remove(n.m_Class);
            }
        }

        static void RisenWorld_EntityAdded(ILrentObject a_Entity)
        {
            IList<Node> N = m_Tree.FindNodes(a_Entity.BoundingBox);
            if (N.Count > 0)
                foreach (Node n in N)
                    (n as PortalRoomNode).AddChild(a_Entity);
            if (a_Entity.getSet<eCPortalRoom_PS_Wrapper>() != null)
            {
                PortalRoomNode p = new PortalRoomNode(a_Entity);
                m_Rooms.Add(a_Entity, p);
                m_Tree.AddNode(p);
                //CREATE TREE
            }
        }

        static void RisenWorld_EntityTransformed(ILrentObject a_Entity)
        {
            if (a_Entity.getSet<eCPortalRoom_PS_Wrapper>() != null)
            {
                PortalRoomNode n = m_Rooms[a_Entity];
                m_Tree.UpdateNode(n);
                //CREATE TREE
            }
            else
            {
                IList<Node> N = m_Tree.FindNodes(a_Entity.BoundingBox);
                if (N.Count > 0)
                    foreach (Node n in N)
                        (n as PortalRoomNode).SearchChildNodes();
                foreach (KeyValuePair<ILrentObject, PortalRoomNode> k in m_Rooms)
                    if (k.Value.m_Children.Contains(a_Entity))
                        k.Value.SearchChildNodes();
            }
        }

        static void RisenWorld_WorldCleared()
        {
            m_Tree = new OcTree();
            m_Rooms.Clear();
            m_PortalTree = null;
        }

        public static void TraverseTree(RenderCollection a_StaticNodes, RenderCollection a_DynamicNodes, BoundingFrustum F)
        {
            //List<StaticVertex> v_Vertices = new List<StaticVertex>();
            //List<StaticVertex> v_Vertices2 = new List<StaticVertex>();

            Vector3 cPos = ManagedWorld.NodeLibrary.Camera.Position_ABS;
            IList<Node> N = m_Tree.FindNodes(F);
            foreach (PortalRoomNode n in N)
            {
                if (n.m_Box.Contains(cPos) == ContainmentType.Contains)
                {
                    a_StaticNodes.AddRange(n.m_Object.Nodes, ref F);
                    if (n.m_Children != null)
                        for(int i = 0; i < n.m_Children.Count; i++)
                            a_DynamicNodes.AddRange(n.m_Children[i].Nodes, ref F);
                    continue;
                }
                foreach (GameLibrary.Tuple<Vector3[], BoundingBox> t in n.m_SamplePoints)
                {
                    if (F.Contains(t.Object1) == ContainmentType.Disjoint)
                        continue;
                    Vector3 tar = t.Object1.Center();
                    Vector3 dir = Vector3.Normalize(tar - cPos);
                    float le = (tar.X - cPos.X) / dir.X;
                    Ray r = new Ray(cPos, dir);
                    float le3 = 0, le4 = float.MaxValue;
                    PortalRoom.HitInfo v_Info = new PortalRoom.HitInfo();
                    if (m_PortalTree == null)
                        return;
                    m_PortalTree.Traverse(ref r, ref v_Info, le3, le4);
                    Vector3 hit = v_Info.m_Distance * r.Direction + r.Position;

                    //v_Vertices.Add(new StaticVertex(cPos));
                    //v_Vertices.Add(new StaticVertex(hit));
                    //v_Vertices2.Add(new StaticVertex(hit));
                    //v_Vertices2.Add(new StaticVertex(tar));

                    if (v_Info.m_Distance > le * 0.95f && n.m_Box.Contains(hit) == ContainmentType.Contains)
                    {
                        a_StaticNodes.AddRange(n.m_Object.Nodes, ref F);
                        if(n.m_Children != null)
                            for (int i = 0; i < n.m_Children.Count; i++)
                                a_DynamicNodes.AddRange(n.m_Children[i].Nodes, ref F);
                        break;
                    }
                }
            }/*
            if (v_Vertices.Count == 0) return;

            MeshBuffer MB = new MeshBuffer(v_Vertices.ToArray(), v_Vertices[0], Form1.TD, SlimDX.Direct3D11.PrimitiveTopology.LineList);
            MeshPart mb = new MeshPart(MB, new Material("", true, Form1.TD), Form1.TD); mb.Material.DiffuseTexture = ShaderResourceTexture.WhiteTexture;

            MeshBuffer MB2 = new MeshBuffer(v_Vertices2.ToArray(), v_Vertices2[0], Form1.TD, SlimDX.Direct3D11.PrimitiveTopology.LineList);
            MeshPart mb2 = new MeshPart(MB2, new Material("", true, Form1.TD), Form1.TD); mb2.Material.DiffuseTexture = ShaderResourceTexture.RedTexture;

            Mesh M = new Mesh(new BoundingBox(), SlimDX.Direct3D11.CullMode.None, mb, mb2);
            GraphicNode gn = new GraphicNode("", M, Form1.TD);
            gn.Tag = new ILrentObject.ObjectTagger(null);
            a_StaticNodes.Add(gn);*/
        }

        public static void T() { }
    }
}
