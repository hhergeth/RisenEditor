using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GameLibrary.IO;
using SlimDX;
using GameLibrary;
using System.ComponentModel;
using System.Collections.Specialized;
using System.Runtime.InteropServices;

namespace RisenEditor.Code.RisenTypes
{
    public class gCDynamicLayer : classData
    {
        long magic;
        short Version;
        public bCAccessorPropertyObject acc;

        public override void Serialize(IFile s)
        {
            s.Write<long>(magic);
            s.Write<short>(Version);
            acc.Serialize(s);
        }

        public override void deSerialize(IFile a_File)
        {
            magic = a_File.Read<long>();
            Version = a_File.Read<short>();
            acc = new bCAccessorPropertyObject(a_File);     
        }

        public override BinaryFileBlock Clone()
        {
            throw new NotImplementedException();
        }

        public override int Size
        {
            get { return 10 + acc.Size; }
        }
    }

    public class eCEntityDynamicContext : classData
    {
        short Version;
        public byte Enabled;
        float _unknownf1, _unknownf2;
        public BoundingBox ContextBox;
        public List<eCDynamicEntity> entitys;
        public List<System.Drawing.Point> ParentChild = new List<System.Drawing.Point>();

        public eCEntityDynamicContext() { }

        public eCEntityDynamicContext(IFile bs)
        {
            deSerialize(bs);
        }

        public override void Serialize(IFile s)
        {
            s.Write<short>(Version);
            if (Version >= 2)
                s.Write<byte>(Enabled);
            if ((Version >= 39) && (Version <= 211))
            {
                s.Write<float>(_unknownf1);
                s.Write<float>(_unknownf2);
            }
            if (Version >= 40)
                s.Write<BoundingBox>(ContextBox);
            s.Write<int>(entitys.Count);
            for (int i = 0; i < entitys.Count; i++)
                entitys[i].Serialize(s);
            foreach (System.Drawing.Point p in ParentChild)
                s.Write<System.Drawing.Point>(p);
        }

        public void addEntitys(List<eCDynamicEntity> ents)
        {
            this.entitys.AddRange(ents);
            for (int c = 0; c < ents.Count; c++)
            {
                int index = ParentChild.Count - 1;
                System.Drawing.Point p = new System.Drawing.Point(0, index + 1);
                ParentChild.Insert(index, p);
            }
        }

        public void removeEntitys(List<eCDynamicEntity> ents)
        {
            foreach (eCDynamicEntity e in ents)
            {
                int q = entitys.IndexOf(e);
                if (q != entitys.Count - 1)
                    ParentChild.RemoveAt(q);
                else
                {
                    ParentChild.RemoveAt(q - 1);
                }
                entitys.Remove(e);
            }

        }

        public override int Size
        {
            get
            {
                int b = 6 + ParentChild.Count * 8;
                if (Version >= 2)
                    b += 1;
                if ((Version >= 39) && (Version <= 211))
                    b += 8;
                if (Version >= 40)
                    b += ContextBox.GetBytes().Length;
                foreach (eCDynamicEntity e in entitys)
                    b += e.Size;
                return b;
            }
        }

        public override BinaryFileBlock Clone()
        {
            throw new NotImplementedException();
        }

        public override void deSerialize(IFile bs)
        {
            Version = (short)bs.Read<ushort>();
            if (Version >= 2)
                Enabled = bs.Read<byte>();
            if ((Version >= 39) && (Version <= 211))
            {
                _unknownf1 = bs.Read<float>();
                _unknownf2 = bs.Read<float>();
            }
            if (Version >= 40)
                ContextBox = bs.Read<BoundingBox>();
            uint Count = bs.Read<uint>();
            entitys = new List<eCDynamicEntity>();
            for (int i = 0; i < Count; i++)
                entitys.Add(new eCDynamicEntity(bs));
            while (true)
            {
                int Parent = bs.Read<int>();
                int Child = bs.Read<int>();
                ParentChild.Add(new System.Drawing.Point(Parent, Child));
                if ((-1 == Parent) && (-1 == Child))
                    break;
            }
        }
    }

    public class eCDynamicEntity : eCGeometryEntity
    {
        new short Version;
        short Version2;
        bCPropertyID ID;
        public bCString Name { get; set; }
        Matrix LocalMatrix;
        BoundingBox WorldNodeBoundary;
        bCSphere WorldNodeSphere;
        BoundingBox LocalNodeBoundary;
        eCEntityProxy creator;
        byte HasCreator;

        public eCDynamicEntity(IFile bs)
        {
            Version = bs.Read<short>();
            if (Version <= 210)
                Version2 = bs.Read<short>();
            ID = new bCPropertyID(bs);
            Name = new bCString(bs);
            LocalMatrix = bs.Read<Matrix>();
            WorldNodeBoundary = bs.Read<BoundingBox>();
            WorldNodeSphere = new bCSphere(bs);
            LocalNodeBoundary = bs.Read<BoundingBox>();
            if (Version >= 213)
                creator = new eCEntityProxy(bs);
            else
            {
                HasCreator = bs.Read<byte>();
                if (HasCreator != 0)
                {
                    bCPropertyID CreatorID = new bCPropertyID(bs);
                    creator = new eCEntityProxy(CreatorID);
                }
            }
            base.deSerialize(bs);
        }

        private eCDynamicEntity()
        {

        }

        new public static eCDynamicEntity StaticConstructor()
        {
            eCDynamicEntity e = new eCDynamicEntity();
            StaticConstructor(e);
            return e;
        }

        public static void StaticConstructor(eCDynamicEntity e)
        {
            e.Version = 214;
            e.ID = new bCPropertyID(Guid.NewGuid());
            e.Name = new bCString(string.Empty);
            e.LocalMatrix = e.geoMatrix;
            e.WorldNodeBoundary = e.geoNodeBoundary;
            e.WorldNodeSphere = e.geoWorldNodeSphere;
            e.LocalNodeBoundary = e.WorldNodeBoundary.Transform(e.LocalMatrix);
            e.creator = new eCEntityProxy(new bCPropertyID(new bCGuid(Guid.Empty, false)));
        }

        public override void Serialize(IFile s)
        {
            s.Write<short>(Version);
            if (Version <= 210)
                s.Write<short>(Version2);
            ID.Serialize(s);
            Name.Serialize(s);
            s.Write<Matrix>(LocalMatrix);
            s.Write<BoundingBox>(WorldNodeBoundary);
            WorldNodeSphere.Serialize(s);
            s.Write<BoundingBox>(LocalNodeBoundary);
            if (Version >= 213)
                creator.Serialize(s);
            else
            {
                s.Write<byte>(HasCreator);
                if (HasCreator != 0)
                    creator.ID.Serialize(s);
            }
            base.Serialize(s);
        }

        public void SetPosition_ST(Vector3 v)
        {
            Matrix = LocalMatrix.Position(v);
        }

        public void SetRotation_ST(Quaternion q)
        {
            Vector3 s = Vector3.Zero; Vector3 v = Vector3.Zero; Quaternion q2 = Quaternion.Identity;
            LocalMatrix.Decompose(out s, out q2, out v);
            Matrix = Matrix.Scaling(s) * Matrix.RotationQuaternion(q) * Matrix.Translation(v);
        }

        public void SetScale_ST(Vector3 ns)
        {
            Vector3 s = Vector3.Zero; Vector3 v = Vector3.Zero; Quaternion q2 = Quaternion.Identity;
            LocalMatrix.Decompose(out s, out q2, out v);
            Matrix = Matrix.Scaling(ns) * Matrix.RotationQuaternion(q2) * Matrix.Translation(v);
        }

        public override BinaryFileBlock Clone()
        {
            eCDynamicEntity e = new eCDynamicEntity();
            e.Version = Version;
            e.Version2 = Version2;
            e.ID = bCPropertyID.NewRandomID();
            e.Name = (bCString)Name.Clone();
            e.LocalMatrix = LocalMatrix;
            e.WorldNodeBoundary = WorldNodeBoundary;
            e.WorldNodeSphere = WorldNodeSphere.Clone() as bCSphere;
            e.LocalNodeBoundary = LocalNodeBoundary;
            if(creator != null)
                e.creator = creator.Clone() as eCEntityProxy;
            e.HasCreator = HasCreator;
            return base.Clone(e);
        }

        public override int Size
        {
            get
            {//134
                int b = Name.Size + 2 + LocalMatrix.GetBytes().Length + WorldNodeBoundary.GetBytes().Length + WorldNodeSphere.Size + LocalNodeBoundary.GetBytes().Length + base.Size + ID.Size;
                if (Version <= 210) b += 2;
                if (Version >= 213) b += creator.Size;
                else
                {
                    b += 1;
                    if (creator != null)
                         b += 20;
                }
                return b;
            }
        }

        public void ApplyBoundingVolume(BoundingBox a_LocalVolume)
        {
            LocalNodeBoundary = a_LocalVolume;
            WorldNodeBoundary = a_LocalVolume.Transform(LocalMatrix);
            WorldNodeSphere = new bCSphere(BoundingSphere.FromBox(WorldNodeBoundary));

            base.geoNodeBoundary = WorldNodeBoundary;
            base.geoWorldNodeSphere = WorldNodeSphere;
        }

        public void ApplyGlobalBoundingVolume(BoundingBox a_GlobalVolume)
        {
            BoundingBox b2 = a_GlobalVolume.Transform(Matrix.Invert(LocalMatrix));
            ApplyBoundingVolume(b2);
        }

        public bCPropertyID GUID
        {
            get
            {
                return this.ID;
            }
            set
            {
                this.ID = value;
            }
        }

        public BoundingBox BoundingBox_ABS
        {
            get
            {
                return WorldNodeBoundary;
            }
        }

        public BoundingBox BoundingBox_LOCAL
        {
            get
            {
                return LocalNodeBoundary;
            }
        }

        public Matrix Matrix
        {
            get
            {
                return LocalMatrix;
            }
            set
            {
                LocalMatrix = value;
                geoMatrix = value;
                ApplyBoundingVolume(LocalNodeBoundary);//update the globals
            }
        }
    }

    public class eCGeometryEntity : eCEntity
    {
        new protected short Version;
        protected float _unkownf1;
        protected Matrix geoMatrix;
        protected BoundingBox geoNodeBoundary;//abs
        protected bCSphere geoWorldNodeSphere;//abs
        protected float RenderAlphaValue;
        protected float ViewRange;
        protected char[] _unknownc1;
        protected float CacheInRange;

        protected eCGeometryEntity()
        {

        }

        new public static eCGeometryEntity StaticConstructor()
        {
            eCGeometryEntity e = new eCGeometryEntity();
            StaticConstructor(e);
            return e;
        }

        public static void StaticConstructor(eCGeometryEntity e)
        {
            eCEntity.StaticConstructor(e);
            e.Version = 214;
            e.geoMatrix = Matrix.Identity;
            e.geoNodeBoundary = new BoundingBox(new Vector3(-1.0f), new Vector3(1.0f));
            e.geoWorldNodeSphere = new bCSphere(BoundingSphere.FromBox(e.geoNodeBoundary));
            e.RenderAlphaValue = 1.0f;
            e.ViewRange = 25000.0f;
            e.CacheInRange = 0.0f;
        }

        public override void deSerialize(IFile bs)
        {
            Version = bs.Read<short>();
            if (Version <= 213)
                _unkownf1 = bs.Read<float>();
            geoMatrix = bs.Read<Matrix>();
            geoNodeBoundary = bs.Read<BoundingBox>();
            geoWorldNodeSphere = new bCSphere(bs);
            if (Version >= 213)
            {
                RenderAlphaValue = bs.Read<float>();
                ViewRange = bs.Read<float>();
            }
            else _unknownc1 = bs.Read<char>(16);
            if (Version >= 214)
                CacheInRange = bs.Read<float>();
            base.deSerialize(bs);
        }

        public override void Serialize(IFile s)
        {
            s.Write<short>(Version);
            if (Version <= 213)
                s.Write<float>(_unkownf1);
            s.Write<Matrix>(geoMatrix);
            s.Write<BoundingBox>(geoNodeBoundary);
            geoWorldNodeSphere.Serialize(s);
            if (Version >= 213)
            {
                s.Write<float>(RenderAlphaValue);
                s.Write<float>(ViewRange);
            }
            else s.Write<char>(_unknownc1);
            if (Version >= 214)
                s.Write<float>(CacheInRange);
            base.Serialize(s);
        }

        public override BinaryFileBlock Clone()
        {
            eCGeometryEntity e = new eCGeometryEntity();
            return Clone(e);
        }

        public BinaryFileBlock Clone(eCGeometryEntity e)
        {
            e.Version = Version;
            e._unkownf1 = _unkownf1;
            e.geoMatrix = geoMatrix;
            e.geoNodeBoundary = geoNodeBoundary;
            e.geoWorldNodeSphere = geoWorldNodeSphere.Clone() as bCSphere;
            e.RenderAlphaValue = RenderAlphaValue;
            e.ViewRange = ViewRange;
            e._unknownc1 = _unknownc1;
            e.CacheInRange = CacheInRange;
            return base.Clone(e);
        }

        public override int Size
        {
            get
            {
                int b = 2 + geoMatrix.GetBytes().Length + geoNodeBoundary.GetBytes().Length + geoWorldNodeSphere.Size + base.Size;
                if (Version <= 213)
                    b += 4;
                if (Version >= 213)
                    b += 8;
                else b += 16;
                if (Version >= 214)
                    b += 4;
                return b;
            }
        }

        public float FadeOutRange
        {
            get
            {
                return ViewRange;
            }
            set
            {
                ViewRange = value;
            }
        }
    }

    public class eCEntity : classData
    {
        protected class AccessorWrapper
        {
            internal bCAccessorPropertyObject obj;
            internal short unknown;
            internal int magic;
        }

        protected short Version;
        protected eCNode Node;
        protected byte Enabled;
        protected byte RenderingEnabled;
        protected byte _unknownb1, _unknownb2, _unknownb3;
        protected byte Flag_00004000, Flag_00008000, Flag_00010000;
        protected byte PickingEnabled;
        protected byte CollisionEnabled;
        protected short InsertType;
        protected byte Locked;
        protected byte _unknownb4;
        protected byte Flag_00000200;
        protected byte _unknownb5;
        protected bCDateTime DataChangedTimeStamp;
        protected byte _unknownb6;
        protected byte _unknownb7;
        protected byte Flag_00040000;
        protected byte IsSaveGameRelevant;
        protected List<AccessorWrapper> m_Accs;

        protected eCEntity()
        {

        }

        public static eCEntity StaticConstructor()
        {
            eCEntity e = new eCEntity();
            StaticConstructor(e);
            return e;
        }

        public static void StaticConstructor(eCEntity e)
        {
            e.Version = 217;
            e.Node = new eCNode();
            e.Enabled = e.RenderingEnabled = 1;
            e._unknownb1 = 0;
            e.Flag_00004000 = 1;
            e.RenderingEnabled = 0;
            e.Flag_00010000 = 1;
            e.PickingEnabled = e.CollisionEnabled = 1;
            e.InsertType = e.Locked = 0;
            e.Flag_00000200 = 0;
            e.DataChangedTimeStamp = new bCDateTime();
            e.Flag_00040000 = 0;
            e.IsSaveGameRelevant = 1;
            e.m_Accs = new List<AccessorWrapper>();
        }

        public override void deSerialize(IFile bs)
        {
            Version = bs.Read<short>();
            Node = new eCNode(bs);
            Enabled = bs.Read<byte>();
            RenderingEnabled = bs.Read<byte>();
            _unknownb1 = bs.Read<byte>();
            if (Version >= 211)
            {
                Flag_00004000 = bs.Read<byte>();
                Flag_00008000 = bs.Read<byte>();
                Flag_00010000 = bs.Read<byte>();
            }
            if (Version <= 213)
            {
                _unknownb2 = bs.Read<byte>();
            }
            _unknownb3 = bs.Read<byte>();
            PickingEnabled = bs.Read<byte>();
            CollisionEnabled = bs.Read<byte>();
            InsertType = bs.Read<short>();
            Locked = bs.Read<byte>();
            if (Version <= 212)
                _unknownb4 = bs.Read<byte>();
            Flag_00000200 = bs.Read<byte>();
            if (Version <= 212)
                _unknownb5 = bs.Read<byte>();
            DataChangedTimeStamp = new bCDateTime(bs);
            if (Version <= 212)
            {
                _unknownb6 = bs.Read<byte>();
                _unknownb7 = bs.Read<byte>();
            }
            if (Version >= 212)
                Flag_00040000 = bs.Read<byte>();
            if (Version >= 216)
                IsSaveGameRelevant = bs.Read<byte>();
            m_Accs = new List<AccessorWrapper>(bs.Read<int>());
            for (int i = 0; i < m_Accs.Capacity; i++)
            {
                AccessorWrapper a = new AccessorWrapper();
                a.unknown = bs.Read<short>();
                a.obj = new bCAccessorPropertyObject(bs);
                a.magic = bs.Read<int>();
                m_Accs.Add(a);
            }
        }

        public override void Serialize(IFile s)
        {
            s.Write<short>(Version);
            Node.Serialize(s);
            s.Write<byte>(Enabled);
            s.Write<byte>(RenderingEnabled);
            s.Write<byte>(_unknownb1);
            if (Version >= 211)
            {
                s.Write<byte>(Flag_00004000);
                s.Write<byte>(Flag_00008000);
                s.Write<byte>(Flag_00010000);
            }
            if (Version <= 213)
            {
                s.Write<byte>(_unknownb2);
            }
            s.Write<byte>(_unknownb3);
            s.Write<byte>(PickingEnabled);
            s.Write<byte>(CollisionEnabled);
            s.Write<short>(InsertType);
            s.Write<byte>(Locked);
            if (Version <= 212)
                s.Write<byte>(_unknownb4);
            s.Write<byte>(Flag_00000200);
            if (Version <= 212)
                s.Write<byte>(_unknownb5);
            DataChangedTimeStamp.Serialize(s);
            if (Version <= 212)
            {
                s.Write<byte>(_unknownb6);
                s.Write<byte>(_unknownb7);
            }
            if (Version >= 212)
                s.Write<byte>(Flag_00040000);
            if (Version >= 216)
                s.Write<byte>(IsSaveGameRelevant);
            s.Write<int>(m_Accs.Count);
            for (int i = 0; i < m_Accs.Count; i++)
            {
                s.Write<short>(m_Accs[i].unknown);
                m_Accs[i].obj.Serialize(s);
                s.Write<int>(m_Accs[i].magic);
            }
        }

        public override BinaryFileBlock Clone()
        {
            eCEntity e = new eCEntity();
            return Clone(e);
        }

        public BinaryFileBlock Clone(eCEntity e)
        {
            e.Version = Version;
            e.Node = Node.Clone() as eCNode; ;
            e.Enabled = Enabled;
            e.RenderingEnabled = RenderingEnabled;
            e._unknownb1 = _unknownb1;
            e._unknownb2 = _unknownb2;
            e._unknownb3 = _unknownb3;
            e.Flag_00004000 = Flag_00004000;
            e.Flag_00008000 = Flag_00008000;
            e.Flag_00010000 = Flag_00010000;
            e.PickingEnabled = PickingEnabled;
            e.CollisionEnabled = CollisionEnabled;
            e.InsertType = InsertType;
            e.Locked = Locked;
            e._unknownb4 = _unknownb4;
            e.Flag_00000200 = Flag_00000200;
            e._unknownb5 = _unknownb5;
            e.DataChangedTimeStamp = DataChangedTimeStamp;
            e._unknownb6 = _unknownb6;
            e._unknownb7 = _unknownb7;
            e.Flag_00040000 = Flag_00040000;
            e.IsSaveGameRelevant = IsSaveGameRelevant;
            e.m_Accs = new List<AccessorWrapper>();
            for (int i = 0; i < m_Accs.Count; i++)
            {
                AccessorWrapper a = new AccessorWrapper();
                a.unknown = m_Accs[i].unknown;
                a.obj = (bCAccessorPropertyObject)m_Accs[i].obj.Clone();
                a.magic = m_Accs[i].magic;
                e.m_Accs.Add(a);
            }
            return e;
        }

        public override int Size
        {
            get
            {
                int b = 24 + Node.Size;
                if (Version >= 211)
                    b += 3;
                if (Version <= 213)
                    b += 1;
                if (Version <= 212)
                    b += 1;
                if (Version <= 212)
                    b += 1;
                if (Version <= 212)
                    b += 2;
                if (Version >= 212)
                    b += 1;
                if (Version >= 216)
                    b += 1;
                foreach (AccessorWrapper g in m_Accs)
                    b += 6 + g.obj.Size;
                return b;
            }
        }

        public void RemoveAccessor(string s)
        {
            for(int i = 0; i < m_Accs.Count; i++)
                if (m_Accs[i].obj.Class.ClassName == s)
                {
                    m_Accs.RemoveAt(i);
                    break;
                }
        }

        public void AddAccessor(bCAccessorPropertyObject obj)
        {
            AccessorWrapper a = new AccessorWrapper();
            a.unknown = 1;
            a.obj = obj;
            a.magic = -559038242;
            m_Accs.Add(a);
        }

        public T Query<T>() where T : classData
        {
            foreach (AccessorWrapper c in m_Accs)
                if (c.obj.Class is T)
                    return c.obj.Class as T;
            return null;
        }

        public bCAccessorPropertyObject this[int index]
        {
            get
            {
                return m_Accs[index].obj;
            }
        }

        public bCAccessorPropertyObject this[string s]
        {
            get
            {
                foreach (AccessorWrapper a in m_Accs)
                    if (a.obj.Class.ClassName == s)
                        return a.obj;
                return null;
            }
        }

        public int AccesorCount
        {
            get
            {
                return m_Accs.Count;
            }
        }
    }

    public class eCNode : classData
    {
        short Version;

        public eCNode()
        {
            Version = 210;
        }

        public eCNode(IFile bs)
        {
            deSerialize(bs); 
        }

        public override void deSerialize(IFile a_File)
        {
            Version = a_File.Read<short>();
        }

        public override void Serialize(IFile s)
        {
            s.Write<short>(Version);
        }

        public override BinaryFileBlock Clone()
        {
            eCNode q = new eCNode();
            q.Version = Version;
            return q;
        }

        public override int Size
        {
            get { return 2; }
        }
    }

    public class eCTemplateEntity : eCEntity
    {
        public eCTemplateEntity()
        {

        }

        public override BinaryFileBlock Clone()
        {
            throw new Exception();
        }

        public override void deSerialize(IFile bs)
        {
            throw new Exception();
        }

        public override void Serialize(IFile s)
        {
            throw new Exception();
        }

        public override int Size
        {
            get
            {
                throw new Exception();
            }
        }
    }

    #region ClassData
    public class gCInventory_PS : classData
    {
        short Version;
        short Version2;
        byte Version3;
        short Version4;
        public List<bCObjectBase> Stacks { get; private set; }
        public Dictionary<int,bCObjectBase> Slots { get; private set; }

        public override void deSerialize(IFile bs)
        {
            Version = bs.Read<short>();
            Version2 = bs.Read<short>();
            Version3 = bs.Read<byte>();
            int count = bs.Read<int>();
            Stacks = new List<bCObjectBase>();
            for (int i = 0; i < count; i++)
            {
                bCObjectBase q = new bCObjectBase(bs);
                Stacks.Add(q);
            }
            Version4 = bs.Read<short>();
            int TemplateEntityIDValid = bs.Read<int>();
            if (TemplateEntityIDValid != 0)
            {
                Slots = new Dictionary<int, bCObjectBase>();
                for (int i = 0; i < TemplateEntityIDValid; i++)
                {
                    int slotP = bs.Read<int>();
                    bCObjectBase p = new bCObjectBase(bs);
                    Slots.Add(slotP, p);
                }
            }
        }

        public override void Serialize(IFile s)
        {
            s.Write<short>(Version);
            s.Write<short>(Version2);
            s.Write<byte>(Version3);
            s.Write<int>(Stacks.Count);
            foreach (bCObjectBase q in Stacks)
                q.Serialize(s);
            s.Write<short>(Version4);
            if (Slots != null)
            {
                s.Write<int>(Slots.Count);
                foreach (KeyValuePair<int, bCObjectBase> kvp in this.Slots)
                {
                    s.Write<int>(kvp.Key);
                    kvp.Value.Serialize(s);
                }
            }
            else s.Write<int>(0);

        }

        public override BinaryFileBlock Clone()
        {
            gCInventory_PS q = new gCInventory_PS();
            q.Version = Version;
            q.Version2 = Version2;
            q.Version3 = Version3;
            q.Version4 = Version4;
            q.Stacks = new List<bCObjectBase>();
            foreach (bCObjectBase q2 in Stacks)
                q.Stacks.Add(q2);
            if (Slots != null)
            {
                q.Slots = new Dictionary<int, bCObjectBase>();
                foreach (KeyValuePair<int, bCObjectBase> kvp in Slots)
                    q.Slots.Add(kvp.Key, kvp.Value);
            }
            return q;
        }

        public bCObjectBase addItem(int Ammount, gEStackType T, gEEquipSlot T2, Guid g)
        {
            bCObjectBase n = new bCObjectBase();
            eCTemplateEntityProxy PR = new eCTemplateEntityProxy(2, new bCGuid(g));
            n.Properties.addProperty("Amount", "int", Ammount);
            n.Properties.addProperty("Selection", "int", 0);
            n.Properties.addProperty("SelectLimit", "int", 0);
            n.Properties.addProperty("QuickSlot", "int", (-1));
            n.Properties.addProperty("Type", "bTPropertyContainer<enum gEStackType>", T);
            n.Properties.addProperty("EquipSlot", "bTPropertyContainer<enum gEEquipSlot>", T2);
            n.Properties.addProperty("Template", "eCTemplateEntityProxy", PR);
            Stacks.Add(n);
            return n;
        }

        public void deleteItem(bCObjectBase itemRef)
        {
            int i = Stacks.IndexOf(itemRef);
            deleteItem(i);
        }

        public void deleteItem(int index)
        {
            Stacks.RemoveAt(index);
        }

        public override int Size
        {
            get
            {
                int b = 15;
                foreach (bCObjectBase q in Stacks)
                    b += q.Size;
                if (Slots != null)
                    foreach (KeyValuePair<int, bCObjectBase> k in Slots)
                        b += 4 + k.Value.Size;
                return b;
            }
        }
    }
    public class eCPortalRoom_PS : classData
    {
        public class BSPPolygon
        {
            internal Plane Plane;//RELEVANT
            internal List<Vector3> Points;//RELEVANT
            internal int ValidPointCount;

            internal BSPPolygon(IFile S)
            {
                Plane = S.Read<Plane>();
                Points = new List<Vector3>(S.Read<Vector3>(9));
                ValidPointCount = S.Read<int>();
            }

            internal void serialize(IFile S)
            {
                S.Write<Plane>(Plane);
                S.Write<Vector3>(Points.ToArray());
                S.Write<int>(ValidPointCount);
            }

            internal unsafe int Size
            {
                get
                {
                    return Points.Count * sizeof(Vector3) + sizeof(Plane) + 4;
                }
            }
        }

        public class eCPortalBSP
        {
            int Magic;
            short Version;
            internal Plane Plane;//RELEVANT
            internal bCSphere Sphere;//RELEVANT
            byte Version2;
            internal List<BSPPolygon> Polygons = new List<BSPPolygon>();
            byte Unknown;
            byte HasChild1;
            byte HasChild2;
            internal eCPortalBSP child1;
            internal eCPortalBSP child2;

            internal eCPortalBSP(IFile S)
            {
                Magic = S.Read<int>();
                Version = S.Read<short>();
                Plane = S.Read<Plane>();
                Sphere = new bCSphere(S);
                Version2 = S.Read<byte>();
                int c = S.Read<int>();
                for (int i = 0; i < c; i++)
                {
                    BSPPolygon p = new BSPPolygon(S);
                    Polygons.Add(p);
                }
                Unknown = S.Read<byte>();
                HasChild1 = S.Read<byte>();
                if (HasChild1 != 0)
                    child1 = new eCPortalBSP(S);
                HasChild2 = S.Read<byte>();
                if (HasChild2 != 0)
                    child2 = new eCPortalBSP(S);
            }

            internal void serialize(IFile S)
            {
                S.Write<int>(Magic);
                S.Write<short>(Version);
                S.Write<Plane>(Plane);
                Sphere.Serialize(S);
                S.Write<byte>(Version2);
                S.Write<int>(Polygons.Count);
                for (int i = 0; i < Polygons.Count; i++)
                    Polygons[i].serialize(S);
                S.Write<byte>(Unknown);
                S.Write<byte>(HasChild1);
                if(child1 != null)
                    child1.serialize(S);
                S.Write<byte>(HasChild2);
                if (child2 != null)
                    child2.serialize(S);
            }

            internal unsafe int Size
            {
                get
                {
                    int i = 4 + 2 + sizeof(Plane) + Sphere.Size + 1 + 4 + 1 + 1 + 1;
                    foreach (BSPPolygon p in Polygons)
                        i += p.Size;
                    if(child1 != null)
                        i += child1.Size;
                    if (child2 != null)
                        i += child2.Size;
                    return i;
                }
            }

            internal BoundingSphere recalc(Matrix m_T)
            {
                Plane = eCPortalRoom_PS.TransformPlane(Plane, m_T);
                List<Vector3> Q = new List<Vector3>();
                for (int i = 0; i < Polygons.Count; i++)
                {
                    Polygons[i].Plane = eCPortalRoom_PS.TransformPlane(Polygons[i].Plane, m_T);
                    for (int j = 0; j < Polygons[i].ValidPointCount; j++)
                    {
                        Polygons[i].Points[j] = Polygons[i].Points[j].Transform(m_T);
                        Q.Add(Polygons[i].Points[j]);
                    }
                }
                BoundingSphere S = BoundingSphere.FromPoints(Q.ToArray());
                if(child1 != null)
                    S = BoundingSphere.Merge(S, child1.recalc(m_T));
                if (child2 != null)
                    S = BoundingSphere.Merge(S, child2.recalc(m_T));
                Sphere = new bCSphere(S);

                return S;
            }
        }

        public class eSPortalRect
        {
            internal Vector3[] Points;//RELEVANT
            internal Vector3 Normal;//RELEVANT

            internal eSPortalRect(IFile S)
            {
                Points = S.Read<Vector3>(4);
                Normal = S.Read<Vector3>();
            }

            internal void serialize(IFile S)
            {
                S.Write<Vector3>(Points);
                S.Write<Vector3>(Normal);
            }

            internal unsafe int Size
            {
                get
                {
                    return Points.Length * sizeof(Vector3) + sizeof(Vector3);
                }
            }

            private eSPortalRect() { }
            internal eSPortalRect Clone()
            {
                eSPortalRect e = new eSPortalRect();
                e.Normal = Normal;
                e.Points = new Vector3[4];
                for(int i = 0; i < 4; i++)
                    e.Points[i] = Points[i];
                return e;
            }
        }

        public class eCPortal
        {
            short Version;
            internal eSPortalRect PortalRect;
            internal eCEntityProxy CellProxy;
            internal BoundingBox Merged;//RELEVANT
            internal float Area;//RELEVANT

            internal eCPortal(IFile S)
            {
                Version = S.Read<short>();
                PortalRect = new eSPortalRect(S);
                CellProxy = new eCEntityProxy(S);
                Merged = S.Read<BoundingBox>();
                Area = S.Read<float>();
            }

            internal void serialize(IFile S)
            {
                S.Write<short>(Version);
                PortalRect.serialize(S);
                CellProxy.Serialize(S);
                S.Write<BoundingBox>(Merged);
                S.Write<float>(Area);
            }

            internal int Size
            {
                get
                {
                    return 2 + PortalRect.Size + CellProxy.Size + 24 + 4;
                }
            }

            internal void Update(Matrix m_T)
            {
                Merged = new BoundingBox(new Vector3(float.MaxValue), new Vector3(float.MinValue));
                for (int i = 0; i < 4; i++)
                {
                    PortalRect.Points[i] = PortalRect.Points[i].Transform(m_T);
                    Merged = Merged.Extend(PortalRect.Points[i]);
                }
                Merged = Merged.nCalc();
                Plane P = eCPortalRoom_PS.TransformPlane(new Plane(PortalRect.Normal, Area), m_T);
                PortalRect.Normal = P.Normal;
                Area = P.D;
            }
        }

        int Version;
        public BoundingBox Boundary;//RELEVANT
        public eCPortalBSP BSP;

        int Version2;
        byte Version3;
        public List<eCPortal> Portals = new List<eCPortal>();

        public eCPortalRoom_PS()
        {
            Version = 4;
            Version2 = 4;
            Version3 = 1;
            Portals = new List<eCPortal>();
            BSP = null;
            Boundary = new BoundingBox(new Vector3(float.MaxValue), new Vector3(float.MinValue));
        }

        public override void deSerialize(IFile a_File)
        {
            Version = a_File.Read<int>();
            Boundary = a_File.Read<BoundingBox>();
            byte HasPortalBSP = a_File.Read<byte>();
            if (HasPortalBSP != 0)
                BSP = new eCPortalBSP(a_File);
            else BSP = null;
            Version2 = a_File.Read<int>();
            Version3 = a_File.Read<byte>();
            int c = a_File.Read<int>();
            for (int i = 0; i < c; i++)
                Portals.Add(new eCPortal(a_File));
        }

        public override void Serialize(IFile s)
        {
            s.Write<int>(Version);
            s.Write<BoundingBox>(Boundary);
            s.Write<byte>(BSP != null ? (byte)1 : (byte)0);
            if (BSP != null)
                BSP.serialize(s);
            s.Write<int>(Version2);
            s.Write<byte>(Version3);
            s.Write<int>(Portals.Count);
            foreach (eCPortal e in Portals)
                e.serialize(s);
        }

        public override int Size
        {
            get 
            {
                int i = 4 + 24 + 1 + 4 + 1 + 4;
                if(BSP != null)
                    i += BSP.Size;
                foreach (eCPortal p in Portals)
                    i += p.Size;
                return i;
            }
        }

        public void Update(Matrix m_T)
        {
            if(BSP != null)
                BSP.recalc(m_T);
            foreach (eCPortal p in Portals)
                p.Update(m_T);
        }

        unsafe public static Plane TransformPlane(Plane P, Matrix M)
        {
            Vector3 point1 = ( P.Normal * -1.0f ) * P.D ;
            Vector3 point2 = point1 + P.Normal;
            point1 = point1.Transform(M);
            point2 = point2.Transform(M);
            Vector3 N = Vector3.Normalize(point2 - point1);
            Plane PP = new Plane(N, Vector3.Dot(N * -1.0f, point1));
            return PP;
        }
    }
    
    public class eCVegetation_Mesh : classData
    {
        struct int2
        {
            public int i1, i2;
            public int2(int i) { i1 = i2 = i; }
        }

        struct int3
        {
            public int i1, i2, i3;
            public int3(int i) { i1 = i2 = i3 = i; }
        }

        short Version;
        int MeshID;
        bCDateTime DateTime;
        bCString _Unknown_1;
        bCBox boxNotUsed;
        bTValArray<int3> arrNotUsed_1;
        bTValArray<int3> arrNotUsed_2;
        bTValArray<int2> arrNotUsed_3;
        bTValArray<int> arrNotUsed_4;

        public override void deSerialize(IFile bs)
        {
            Version = bs.Read<short>();
            MeshID = bs.Read<int>();
            DateTime = new bCDateTime(bs);
            if ((Version == 2) || (Version == 3))
            {
                _Unknown_1 = new bCString(bs);
                if (Version == 2)
                {
                    boxNotUsed = new bCBox(bs);
                    arrNotUsed_1 = new bTValArray<int3>(bs);
                    arrNotUsed_2 = new bTValArray<int3>(bs);
                    arrNotUsed_3 = new bTValArray<int2>(bs);
                    arrNotUsed_4 = new bTValArray<int>(bs);
                }
            }
        }

        public override void Serialize(IFile s)
        {
            s.Write<short>(Version);
            s.Write<int>(MeshID);
            DateTime.Serialize(s);
            if ((Version == 2) || (Version == 3))
            {
                _Unknown_1.Serialize(s);
                if (Version == 2)
                {
                    boxNotUsed.Serialize(s);
                    arrNotUsed_1.Serialize(s);
                    arrNotUsed_2.Serialize(s);
                    arrNotUsed_3.Serialize(s);
                    arrNotUsed_4.Serialize(s);
                }
            }
        }

        public override int Size
        {
            get
            {
                int q = 6 + DateTime.Size;
                if (Version == 2 || Version == 3)
                    q += _Unknown_1.Size;
                if (Version == 2)
                    q += boxNotUsed.Size + arrNotUsed_1.Size + arrNotUsed_2.Size + arrNotUsed_3.Size + arrNotUsed_4.Size;
                return q;
            }
        }
    }
    public class eCVegetation_PS : classData
    {
        class _Unknown_Struct_1 : BinaryFileBlock
        {
            short _Unknown_1;
            bCString _Unknown_2;

            public _Unknown_Struct_1(IFile s)
            {
                deSerialize(s);
            }

            private _Unknown_Struct_1() { }

            public override BinaryFileBlock Clone()
            {
                _Unknown_Struct_1 a = new _Unknown_Struct_1();
                a._Unknown_1 = _Unknown_1;
                a._Unknown_2 = (bCString)_Unknown_2.Clone();
                return a;
            }

            public override void deSerialize(IFile a_File)
            {
                _Unknown_1 = a_File.Read<short>();
                _Unknown_2 = new bCString(a_File);
            }

            public override void Serialize(IFile s)
            {
                s.Write<short>(_Unknown_1);
                _Unknown_2.Serialize(s);
            }

            public override int Size
            {
                get { return 2 + _Unknown_2.Size; }
            }
        }

        class eCVegetationGrid : BinaryFileBlock
        {
            class eCVegetation_GridNode : BinaryFileBlock
            {
                struct eSVegetationNodeEntry
                {
                    public Vector3 v1, v2, v3;
                    public Vector2 v4;
                    public eSVegetationNodeEntry(int i) { v1 = v2 = v3 = new Vector3(); v4 = new Vector2(); }
                }

                short Version;
                bCBox boxBoundingBox;
                List<eSVegetationNodeEntry> data;

                public eCVegetation_GridNode(IFile s)
                {
                    deSerialize(s);
                }

                private eCVegetation_GridNode() { }

                public override void deSerialize(IFile a_File)
                {
                    Version = a_File.Read<short>();
                    boxBoundingBox = new bCBox(a_File);
                    data = new List<eSVegetationNodeEntry>(a_File.Read<int>());
                    for (int i = 0; i < data.Capacity; i++)
                        data.Add(a_File.Read<eSVegetationNodeEntry>());
                }

                public override void Serialize(IFile s)
                {
                    s.Write<short>(Version);
                    boxBoundingBox.Serialize(s);
                    s.Write<int>(data.Count);
                    foreach (eSVegetationNodeEntry a in data)
                        s.Write<eSVegetationNodeEntry>(a);
                }

                public unsafe override int Size
                {
                    get { return 2 + boxBoundingBox.Size + 4 + data.Count * sizeof(eSVegetationNodeEntry); }
                }

                public override BinaryFileBlock Clone()
                {
                    eCVegetation_GridNode e = new eCVegetation_GridNode();
                    e.Version = Version;
                    e.boxBoundingBox = (bCBox)boxBoundingBox.Clone();
                    e.data = new List<eSVegetationNodeEntry>();
                    foreach (eSVegetationNodeEntry e2 in data)
                        e.data.Add(e2);
                    return e;
                }
            }

            short Version;
            float fNodeDimension;
            bCRect rectGridRect;
            List<int> data0;
            List<eCVegetation_GridNode> data1;
            bCBox boxBoundingBox;

            public eCVegetationGrid(IFile bs)
            {
                deSerialize(bs);
            }

            private eCVegetationGrid() { }

            public override void deSerialize(IFile a_File)
            {
                Version = a_File.Read<short>();
                fNodeDimension = a_File.Read<float>();
                rectGridRect = new bCRect(a_File);
                int count = a_File.Read<int>();
                data0 = new List<int>(count);
                data1 = new List<eCVegetation_GridNode>(count);
                for (int i = 0; i < count; i++)
                {
                    data0.Add(a_File.Read<int>());
                    data1.Add(new eCVegetation_GridNode(a_File));
                }
                if (Version > 1)
                    boxBoundingBox = new bCBox(a_File);
            }

            public override void Serialize(IFile s)
            {
                s.Write<short>(Version);
                s.Write<float>(fNodeDimension);
                rectGridRect.Serialize(s);
                s.Write<int>(data0.Count);
                for (int i = 0; i < data0.Count; i++)
                {
                    s.Write<int>(data0[i]);
                    data1[i].Serialize(s);
                }
                if (Version > 1)
                    boxBoundingBox.Serialize(s);
            }

            public override int Size
            {
                get
                {
                    int q = 10 + rectGridRect.Size + data0.Count * 4;
                    foreach (eCVegetation_GridNode a in data1)
                        q += a.Size;
                    if (Version > 1)
                        q += boxBoundingBox.Size;
                    return q;
                }
            }

            public override BinaryFileBlock Clone()
            {
                eCVegetationGrid e = new eCVegetationGrid();
                e.Version = Version;
                e.fNodeDimension = fNodeDimension;
                e.rectGridRect = (bCRect)rectGridRect.Clone();
                e.boxBoundingBox = (bCBox)boxBoundingBox.Clone();
                e.data0 = data0.DeepClone_Struct();
                e.data1 = data1.DeepClone();
                return e;
            }
        }

        short Version;
        short Version2;
        bTRefPtrArray<_Unknown_Struct_1> arrUnknown_1s;
        int _Unknown_2;
        List<bCAccessorPropertyObject> arrVegetationMeshAccessors;
        eCVegetationGrid VegetationGrid;

        public override void deSerialize(IFile bs)
        {
            Version = bs.Read<short>();
            if (Version < 2)
                Version2 = bs.Read<short>();
            arrUnknown_1s = new bTRefPtrArray<_Unknown_Struct_1>(bs);
            _Unknown_2 = bs.Read<int>();
            arrVegetationMeshAccessors = new List<bCAccessorPropertyObject>(bs.Read<int>());
            for (int i = 0; i < arrVegetationMeshAccessors.Capacity; i++)
                arrVegetationMeshAccessors.Add(new bCAccessorPropertyObject(bs));
            VegetationGrid = new eCVegetationGrid(bs);
        }

        public override void Serialize(IFile s)
        {
            s.Write<short>(Version);
            if (Version < 2)
                s.Write<short>(Version2);
            arrUnknown_1s.Serialize(s);
            s.Write<int>(_Unknown_2);
            s.Write<int>(arrVegetationMeshAccessors.Count);
            for (int i = 0; i < arrVegetationMeshAccessors.Count; i++)
                arrVegetationMeshAccessors[i].Serialize(s);
            VegetationGrid.Serialize(s);
        }

        public override int Size
        {
            get
            {
                int q = 0;
                foreach (bCAccessorPropertyObject a in arrVegetationMeshAccessors)
                    q += a.Size;
                return ((Version < 2) ? 2 : 0) + arrUnknown_1s.Size + 10 + q + VegetationGrid.Size;
            }
        }
    }
    
    public class eCOccluder_PS : classData
    {
        internal class eCPortal : BinaryFileBlock
        {
            short Version;
            eCPortalRoom_PS.eSPortalRect PortalRect;
            eCEntityProxy CellProxy;
            bCBox Merged;
            float Area;

            internal eCPortal(IFile S)
            {
                deSerialize(S);
            }

            private eCPortal() { }

            public override void deSerialize(IFile a_File)
            {
                Version = a_File.Read<short>();
                PortalRect = new eCPortalRoom_PS.eSPortalRect(a_File);
                CellProxy = new eCEntityProxy(a_File);
                Merged = new bCBox(a_File);
                Area = a_File.Read<float>();
            }

            public override void Serialize(IFile s)
            {
                s.Write<short>(Version);
                PortalRect.serialize(s);
                CellProxy.Serialize(s);
                Merged.Serialize(s);
                s.Write<float>(Area);
            }

            public override int Size
            {
                get { return 2 + PortalRect.Size + CellProxy.Size + Merged.Size + 4; }
            }

            public override BinaryFileBlock Clone()
            {
                eCPortal e = new eCPortal();
                e.PortalRect = PortalRect.Clone();
                e.Version = Version;
                e.CellProxy = (eCEntityProxy)CellProxy.Clone();
                e.Merged = (bCBox)Merged.Clone();
                e.Area = Area;
                return e;
            }
        }

        internal class eCPortalCell : BinaryFileBlock
        {
            int Version2;
            byte Version3;
            List<eCPortal> Portals;

            internal eCPortalCell(IFile S)
            {
                deSerialize(S);
            }

            private eCPortalCell() { }

            public override void deSerialize(IFile a_File)
            {
                Portals = new List<eCPortal>();
                Version2 = a_File.Read<int>();
                Version3 = a_File.Read<byte>();
                int c = a_File.Read<int>();
                for (int i = 0; i < c; i++)
                    Portals.Add(new eCPortal(a_File));
            }

            public override void Serialize(IFile s)
            {
                s.Write<int>(Version2);
                s.Write<byte>(Version3);
                s.Write<int>(Portals.Count);
                foreach (eCPortal p in Portals)
                    p.Serialize(s);
            }

            public override int Size
            {
                get
                {
                    int q = 9;
                    foreach (eCPortal p in Portals)
                        q += p.Size;
                    return q;
                }
            }

            public override BinaryFileBlock Clone()
            {
                eCPortalCell e = new eCPortalCell();
                e.Version2 = Version2;
                e.Version3 = Version3;
                e.Portals = new List<eCPortal>();
                foreach (eCPortal p in Portals)
                    e.Portals.Add((eCPortal)p.Clone());
                return e;
            }
        }

        internal struct eSTriangle
        {
            internal Vector4 v1, v2;
            internal Vector3 v3;
            public eSTriangle(int a) { v1 = new Vector4(); v2 = new Vector4(); v3 = new Vector3(); }
        }

        internal struct _Unknown_Structure_1
        {
            internal Vector4 v1;
            public _Unknown_Structure_1(int a) { v1 = new Vector4(); }
        }

        internal struct _Unknown_Structure_2
        {
            internal Vector4 v1, v2;
            public _Unknown_Structure_2(int a) { v1 = new Vector4(); v2 = new Vector4(); }
        }

        internal struct _Unknown_Structure_3
        {
            public Vector4 v1;
            public float f1;
            internal _Unknown_Structure_3(int a) { v1 = new Vector4(); f1 = new float(); }
        }

        int Version;
        bTRefPtrArray<eCPortal> T0;
        bTValArray<Vector3> T1;
        bTValArray<eSTriangle> T2;
        bTValArray<_Unknown_Structure_1> T3;
        eCPortalCell T4;
        bCBox T5;
        int T6;
        bTValArray<_Unknown_Structure_2> T7;
        bTValArray<_Unknown_Structure_3> T8;

        public override void deSerialize(IFile bs)
        {
            Version = bs.Read<int>();
            if (Version < 2)
                T0 = new bTRefPtrArray<eCPortal>(bs);
            else if (Version == 2)
            {
                T5 = new bCBox(bs);
                T6 = bs.Read<int>();
                T1 = new bTValArray<Vector3>(bs);
                T2 = new bTValArray<eSTriangle>(bs);
                T3 = new bTValArray<_Unknown_Structure_1>(bs);
                T4 = new eCPortalCell(bs);
            }
            else
            {
                T5 = new bCBox(bs);
                T7 = new bTValArray<_Unknown_Structure_2>(bs);
                T1 = new bTValArray<Vector3>(bs);
                T2 = new bTValArray<eSTriangle>(bs);
                T8 = new bTValArray<_Unknown_Structure_3>(bs);
                T4 = new eCPortalCell(bs);
            }
        }

        public override void Serialize(IFile s)
        {
            s.Write<int>(Version);
            if (Version < 2)
                T0.Serialize(s);
            else if (Version == 2)
            {
                T5.Serialize(s);
                s.Write<int>(T6);
                T1.Serialize(s);
                T2.Serialize(s);
                T3.Serialize(s);
                T4.Serialize(s);
            }
            else
            {
                T5.Serialize(s);
                T7.Serialize(s);
                T1.Serialize(s);
                T2.Serialize(s);
                T8.Serialize(s);
                T4.Serialize(s);
            }
        }

        public override int Size
        {
            get
            {
                int q = 2;
                if (Version < 2)
                    q += T0.Size;
                else if (Version == 2)
                    q += T5.Size + 4 + T1.Size + T2.Size + T3.Size + T5.Size;
                else
                    q += T5.Size + T7.Size + T1.Size + T2.Size + T8.Size + T4.Size;
                return q;
            }
        }
    }
    public class gCInteraction_PS : classData
    {
        short Version;
        List<bCAccessorPropertyObject> data;

        public override void deSerialize(IFile bs)
        {
            Version = bs.Read<short>();
            data = new List<bCAccessorPropertyObject>(bs.Read<int>());
            for (int i = 0; i < data.Capacity; i++)
                data.Add(new bCAccessorPropertyObject(bs));
        }

        public override void Serialize(IFile s)
        {
            s.Write<short>(Version);
            s.Write<int>(data.Count);
            foreach (bCAccessorPropertyObject a in data)
                a.Serialize(s);
        }

        public override BinaryFileBlock Clone()
        {
            gCInteraction_PS D = new gCInteraction_PS();
            D.Version = Version;
            D.data = data.DeepClone();
            return D;
        }

        public override int Size
        {
            get
            {
                return 2 + data.SizeOf();
            }
        }
    }
    public class eCMover_PS : classData
    {
        short Version;
        public List<bCAccessorPropertyObject> list;
        public Matrix M;
        public eCEntityPropertySet E;

        public override BinaryFileBlock Clone()
        {
            eCMover_PS e = new eCMover_PS();
            e.Version = Version;
            e.M = M;
            if (this.E != null)
                e.E = (eCEntityPropertySet)E.Clone();
            e.list = new List<bCAccessorPropertyObject>(list.Count);
            foreach (bCAccessorPropertyObject a in list)
                e.list.Add((bCAccessorPropertyObject)a.Clone());
            return e;
        }

        public override void deSerialize(IFile bs)
        {
            Version = bs.Read<short>();
            list = new List<bCAccessorPropertyObject>(bs.Read<int>());
            for (int i = 0; i < list.Capacity; i++)
                list.Add(new bCAccessorPropertyObject(bs));
            if (Version > 1)
            {
                M = bs.Read<Matrix>();
                E = new eCEntityPropertySet(bs);
            }
        }

        public override void Serialize(IFile s)
        {
            s.Write<short>(Version);
            s.Write<int>(list.Count);
            foreach (bCAccessorPropertyObject a in list)
                a.Serialize(s);
            if (Version > 1)
            {
                s.Write<Matrix>(M);
                E.Serialize(s);
            }
        }

        public unsafe override int Size
        {
            get 
            {
                int q = 6;
                foreach (bCAccessorPropertyObject a in list)
                    q += a.Size;
                if(Version > 1)
                    q += E.Size + sizeof(Matrix);
                return q;
            }
        }
    }
    public class eCParticle_PS : classData
    {
        class eCFloatScale : BinaryFileBlock
        {
            int Version;
            List<Vector2> data;

            private eCFloatScale() { }

            public override BinaryFileBlock Clone()
            {
                eCFloatScale e = new eCFloatScale();
                e.Version = Version;
                e.data = new List<Vector2>(data);
                return e;
            }

            public eCFloatScale(IFile S)
            {
                deSerialize(S);
            }

            public override void deSerialize(IFile a_File)
            {
                Version = a_File.Read<int>();
                int ArraySize = a_File.Read<int>();
                if (ArraySize != 0)
                {
                    data = new List<Vector2>(a_File.Read<int>());
                    for (int i = 0; i < data.Capacity; i++)
                        data.Add(a_File.Read<Vector2>());
                }
            }

            public override void Serialize(IFile s)
            {
                s.Write<int>(Version);
                s.Write<int>(Convert.ToInt32(data != null));
                if (data != null)
                {
                    s.Write<int>(data.Count);
                    foreach (Vector2 q in data)
                        s.Write<Vector2>(q);
                }
            }

            public override int Size
            {
                get { return 8 + (data != null ? (data.Count * 8) + 4 : 0); }
            }
        }

        class eCVectorScale : BinaryFileBlock
        {
            int Version;
            List<Vector4> data;

            private eCVectorScale() { }

            public override BinaryFileBlock Clone()
            {
                eCVectorScale e = new eCVectorScale();
                e.Version = Version;
                e.data = new List<Vector4>(data);
                return e;
            }

            public eCVectorScale(IFile S)
            {
                deSerialize(S);
            }

            public override void deSerialize(IFile a_File)
            {
                Version = a_File.Read<int>();
                int ArraySize = a_File.Read<int>();
                if (ArraySize != 0)
                {
                    data = new List<Vector4>(a_File.Read<int>());
                    for (int i = 0; i < data.Capacity; i++)
                        data.Add(a_File.Read<Vector4>());
                }
            }

            public override void Serialize(IFile s)
            {
                s.Write<int>(Version);
                s.Write<int>(Convert.ToInt32(data != null));
                if (data != null)
                {
                    s.Write<int>(data.Count);
                    foreach (Vector4 q in data)
                        s.Write<Vector4>(q);
                }
            }

            public override int Size
            {
                get { return 8 + (data != null ? (data.Count * 16) + 4 : 0); }
            }
        }

        class eCColorScale : BinaryFileBlock
        {
            struct Vector5
            {
                public float f1, f2, f3, f4, f5;
                public Vector5(int i) { f1 = f2 = f3 = f4 = f5 = i; }
            }

            int Version;
            List<Vector5> data;

            private eCColorScale() { }

            public override BinaryFileBlock Clone()
            {
                eCColorScale e = new eCColorScale();
                e.Version = Version;
                e.data = new List<Vector5>(data);
                return e;
            }

            public eCColorScale(IFile S)
            {
                deSerialize(S);
            }

            public override void deSerialize(IFile a_File)
            {
                Version = a_File.Read<int>();
                int ArraySize = a_File.Read<int>();
                if (ArraySize != 0)
                {
                    data = new List<Vector5>(a_File.Read<int>());
                    for (int i = 0; i < data.Capacity; i++)
                        data.Add(a_File.Read<Vector5>());
                } 
            }

            public override void Serialize(IFile s)
            {
                s.Write<int>(Version);
                s.Write<int>(Convert.ToInt32(data != null));
                if (data != null)
                {
                    s.Write<int>(data.Count);
                    foreach (Vector5 q in data)
                        s.Write<Vector5>(q);
                }
            }

            public override int Size
            {
                get { return 8 + (data != null ? (data.Count * 20) + 4 : 0); }
            }
        }

        short Version;
        eCFloatScale floatScale1;
        eCVectorScale vectorScale1;
        eCVectorScale vectorScale2;
        eCColorScale colorScale1;
        eCVectorScale vectorScale3;
        int notUsed;
        byte[] NotUsed2;

        public override void deSerialize(IFile bs)
        {
            Version = bs.Read<short>();
            if (Version == 2 || Version == 3)
            {
                floatScale1 = new eCFloatScale(bs);
                vectorScale1 = new eCVectorScale(bs);
                vectorScale2 = new eCVectorScale(bs);
                colorScale1 = new eCColorScale(bs);
                vectorScale3 = new eCVectorScale(bs);
                if (Version == 2)
                {
                    notUsed = bs.Read<int>();
                    NotUsed2 = bs.Read<byte>(bs.Read<int>());
                }
            }
        }

        public override void Serialize(IFile s)
        {
            s.Write<short>(Version);
            if (Version == 2 || Version == 3)
            {
                floatScale1.Serialize(s);
                vectorScale1.Serialize(s);
                vectorScale2.Serialize(s);
                colorScale1.Serialize(s);
                vectorScale3.Serialize(s);
                if (Version == 2)
                {
                    s.Write<int>(notUsed);
                    s.Write<int>(NotUsed2.Length);
                    if(NotUsed2.Length > 0)
                        s.Write<byte>(NotUsed2);
                }
            }
        }

        public override BinaryFileBlock Clone()
        {
            eCParticle_PS P = new eCParticle_PS();
            P.Version = Version;
            if (Version == 2 || Version == 3)
            {
                P.floatScale1 = (eCFloatScale)floatScale1.Clone();
                P.vectorScale1 = (eCVectorScale)vectorScale1.Clone();
                P.vectorScale2 = (eCVectorScale)vectorScale2.Clone();
                P.colorScale1 = (eCColorScale)colorScale1.Clone();
                P.vectorScale3 = (eCVectorScale)vectorScale3.Clone();
                if (Version == 2)
                {
                    P.notUsed = notUsed;
                    P.NotUsed2 = new byte[NotUsed2.Length];
                    Array.Copy(NotUsed2, P.NotUsed2, NotUsed2.Length);
                }
            }
            return P;
        }

        public override int Size
        {
            get 
            {
                int q = 2;
                if (Version == 2 || Version == 3)
                {
                    q += floatScale1.Size + vectorScale1.Size + vectorScale2.Size + colorScale1.Size + vectorScale3.Size;
                    if (Version == 2)
                        q += 4 + NotUsed2.Length;
                }
                return q;
            }
        }
    }
    public class gCStateGraphEventFilter : classData
    {
        public override void Serialize(IFile a_File)
        {
            
        }

        public override void deSerialize(IFile a_File)
        {
            
        }

        public override int Size
        {
            get { return 0; }
        }
    }
    public class gCStateGraphEventFilterDamageAmount : classData
    {
        short Version;

        public override void Serialize(IFile a_File)
        {
            a_File.Write(Version);
        }

        public override void deSerialize(IFile a_File)
        {
            Version = a_File.Read<short>();
        }

        public override int Size
        {
            get { return 2; }
        }
    }
    public class gCStateGraphEventFilterIsPlayer : classData
    {
        short Version;

        public override void Serialize(IFile a_File)
        {
            a_File.Write(Version);
        }

        public override void deSerialize(IFile a_File)
        {
            Version = a_File.Read<short>();
        }

        public override int Size
        {
            get { return 2; }
        }
    }
    public class gCStateGraphEventFilterIsNPC : classData
    {
        short Version;

        public override void Serialize(IFile a_File)
        {
            a_File.Write(Version);
        }

        public override void deSerialize(IFile a_File)
        {
            Version = a_File.Read<short>();
        }

        public override int Size
        {
            get { return 2; }
        }
    }
    public class gCStateGraphEventFilterScript : classData
    {
        short Version;

        public override void Serialize(IFile a_File)
        {
            a_File.Write(Version);
        }

        public override void deSerialize(IFile a_File)
        {
            Version = a_File.Read<short>();
        }

        public override int Size
        {
            get { return 2; }
        }
    }
    public class gCStateGraphAction : classData
    {
        short Version;
        List<bCAccessorPropertyObject> arrEventFilterAccessors;

        public override void Serialize(IFile bs)
        {
            bs.Write<short>(Version);
            bs.Write<int>(arrEventFilterAccessors.Count);
            foreach (bCAccessorPropertyObject a in arrEventFilterAccessors)
                a.Serialize(bs);
        }

        public override void deSerialize(IFile s)
        {
            Version = s.Read<short>();
            arrEventFilterAccessors = new List<bCAccessorPropertyObject>(s.Read<int>());
            for (int i = 0; i < arrEventFilterAccessors.Capacity; i++)
                arrEventFilterAccessors.Add(new bCAccessorPropertyObject(s));
        }

        public override int Size
        {
            get 
            {
                int q = 6;
                foreach (bCAccessorPropertyObject a in arrEventFilterAccessors)
                    q += a.Size;
                return q;
            }
        }
    }
    public class gCStateGraphState : classData
    {
        class gCStateGraphTransition : BinaryFileBlock
        {
            short Version;
            bCString _Unknown_1;

            public gCStateGraphTransition(IFile S)
            {
                deSerialize(S);
            }

            private gCStateGraphTransition() { }

            public override BinaryFileBlock Clone()
            {
                gCStateGraphTransition a = new gCStateGraphTransition();
                a.Version = Version;
                a._Unknown_1 = (bCString)_Unknown_1.Clone();
                return a;
            }

            public override void deSerialize(IFile a_File)
            {
                Version = a_File.Read<short>();
                _Unknown_1 = new bCString(a_File);
            }

            public override void Serialize(IFile s)
            {
                s.Write<short>(Version);
                _Unknown_1.Serialize(s);
            }

            public override int Size
            {
                get { return 2 + _Unknown_1.Size; }
            }
        }

        short Version;
        bCRect rectEditorLayout;
        List<bCAccessorPropertyObject> arrActionAccessors;
        List<gCStateGraphTransition> arrTransitions;

        public override void deSerialize(IFile bs)
        {
            Version = bs.Read<short>();
            rectEditorLayout = new bCRect(bs);
            arrActionAccessors = new List<bCAccessorPropertyObject>(bs.Read<int>());
            for (int i = 0; i < arrActionAccessors.Capacity; i++)
                arrActionAccessors.Add(new bCAccessorPropertyObject(bs));
            arrTransitions = new List<gCStateGraphTransition>(bs.Read<int>());
            for (int i = 0; i < arrTransitions.Capacity; i++)
                arrTransitions.Add(new gCStateGraphTransition(bs));
            int a = Size;
        }

        public override void Serialize(IFile s)
        {
            s.Write<short>(Version);
            rectEditorLayout.Serialize(s);
            s.Write<int>(arrActionAccessors.Count);
            foreach (bCAccessorPropertyObject a in arrActionAccessors)
                a.Serialize(s);
            s.Write<int>(arrTransitions.Count);
            foreach (gCStateGraphTransition a in arrTransitions)
                a.Serialize(s);
        }

        public override BinaryFileBlock Clone()
        {
            gCStateGraphState A = new gCStateGraphState();
            A.Version = Version;
            A.rectEditorLayout = (bCRect)rectEditorLayout.Clone();
            A.arrActionAccessors = arrActionAccessors.DeepClone();
            A.arrTransitions = arrTransitions.DeepClone();
            return A;
        }

        public override int Size
        {
            get 
            {
                int q = 10 + rectEditorLayout.Size;
                foreach (bCAccessorPropertyObject a in arrActionAccessors)
                    q += a.Size;
                foreach (gCStateGraphTransition a in arrTransitions)
                    q += a.Size;
                return q;
            }
        }
    }
    public class gCStateGraph_PS : classData
    {
        short Version;
        List<bCAccessorPropertyObject> arrStateAccessors;

        public override void deSerialize(IFile bs)
        {
            Version = bs.Read<short>();
            arrStateAccessors = new List<bCAccessorPropertyObject>(bs.Read<int>());
            for (int i = 0; i < arrStateAccessors.Capacity; i++)
                arrStateAccessors.Add(new bCAccessorPropertyObject(bs));
        }

        public override void Serialize(IFile s)
        {
            s.Write<short>(Version);
            s.Write<int>(arrStateAccessors.Count);
            foreach (bCAccessorPropertyObject a in arrStateAccessors)
                a.Serialize(s);
        }

        public override BinaryFileBlock Clone()
        {
            gCStateGraph_PS a = new gCStateGraph_PS();
            a.Version = Version;
            a.arrStateAccessors = new List<bCAccessorPropertyObject>();
            foreach (bCAccessorPropertyObject b in arrStateAccessors)
                a.arrStateAccessors.Add((bCAccessorPropertyObject)b.Clone());
            return a;
        }

        public override int Size
        {
            get 
            {
                int q = 6;
                foreach (bCAccessorPropertyObject q2 in arrStateAccessors)
                    q += q2.Size;
                return q;
            }
        }
    }
    
    public class eCCollisionShape : classData
    {
        ushort u16FileVersion;
        eECollisionShapeType enumShapeType;
        bCObjectRefBase baseObj;
        bCBox m_boxOuterAABBLocal;
        Vector3 m_vecLastPosition;
        bCMatrix3 m_matOrientation;

        bCString pstrMeshResourceName;
        ushort m_u16ResourceIndex;

        bCOrientedBox boxBox;

        bCSphere psphSphere;

        bCCapsule pcapCapsule;
        float fCenterX;
        float fCenterY;

        Vector3 pvecPoint;

        public override void deSerialize(IFile bs)
        {
            enumShapeType = (eECollisionShapeType)base.Container.Properties["ShapeType"].Object;
            u16FileVersion = bs.Read<ushort>();
            if (u16FileVersion < 28)
                enumShapeType = eECollisionShapeType.eECollisionShapeType_TriMesh;
            switch (enumShapeType)
            {
                case eECollisionShapeType.eECollisionShapeType_TriMesh:
                case eECollisionShapeType.eECollisionShapeType_ConvexHull:
                    pstrMeshResourceName = new bCString(bs);
                    if (u16FileVersion > 31)
                        m_u16ResourceIndex = bs.Read<ushort>();
                    break;
                case eECollisionShapeType.eECollisionShapeType_Box:
                    if (u16FileVersion <= 34)
                    {
                        boxBox = new bCOrientedBox(bs);
                    }
                    else
                    {
                        boxBox = new bCOrientedBox(bs);
                    }
                    //if (u16FileVersion < 64)
                    //    ;//m_vecExtent * 0.5f;
                    break;
                case eECollisionShapeType.eECollisionShapeType_Sphere:
                    psphSphere = new bCSphere(bs);
                    break;
                case eECollisionShapeType.eECollisionShapeType_Capsule:
                    if (u16FileVersion < 39)
                    {
                        fCenterX = bs.Read<float>();
                        fCenterY = bs.Read<float>();
                    }
                    else
                        pcapCapsule = new bCCapsule(bs);
                    break;
                case eECollisionShapeType.eECollisionShapeType_Point:
                    pvecPoint = bs.Read<Vector3>();
                    break;
            }
            if (u16FileVersion >= 33)
                baseObj = new bCObjectRefBase(bs);
            if (u16FileVersion >= 34)
            {
                m_boxOuterAABBLocal = new bCBox(bs);
                if (u16FileVersion < 66)
                    if (enumShapeType == eECollisionShapeType.eECollisionShapeType_Box)
                        m_boxOuterAABBLocal.Scale(2.0f);
            }
            if (u16FileVersion >= 61)
                m_vecLastPosition = bs.Read<Vector3>();
            if (u16FileVersion < 74 && enumShapeType == eECollisionShapeType.eECollisionShapeType_Box)
                m_matOrientation = new bCMatrix3(bs);
        }
        /*
        public override void Serialize(IFile s)
        {
            s.Write<ushort>(u16FileVersion);
             switch (enumShapeType)
            {
                case eECollisionShapeType.eECollisionShapeType_TriMesh:
                case eECollisionShapeType.eECollisionShapeType_ConvexHull:
                    pstrMeshResourceName.Serialize(s);
                    if (u16FileVersion > 31)
                        s.Write<ushort>(m_u16ResourceIndex);
                    break;
                case eECollisionShapeType.eECollisionShapeType_Box:
                    boxBox.Serialize(s);
                    break;
                case eECollisionShapeType.eECollisionShapeType_Sphere:
                    psphSphere.Serialize(s);
                    break;
                case eECollisionShapeType.eECollisionShapeType_Capsule:
                    if (u16FileVersion < 39)
                    {
                        s.Write<float>(fCenterX);
                        s.Write<float>(fCenterY);
                    }
                    else
                        pcapCapsule.Serialize(s);
                    break;
                case eECollisionShapeType.eECollisionShapeType_Point:
                    s.Write<Vector3>(pvecPoint);
                    break;
            }
             if (u16FileVersion >= 33)
                 baseObj.Serialize(s);
             if (u16FileVersion >= 34)
                 m_boxOuterAABBLocal.Serialize(s);
             if (u16FileVersion >= 61)
                 s.Write<Vector3>(m_vecLastPosition);
             if (u16FileVersion < 74 && enumShapeType == eECollisionShapeType.eECollisionShapeType_Box)
                 m_matOrientation.Serialize(s);
        }
        */

        public override void Serialize(IFile a_File)
        {
            a_File.Write<short>(74);
            switch (enumShapeType)
            {
                case eECollisionShapeType.eECollisionShapeType_ConvexHull :
                case eECollisionShapeType.eECollisionShapeType_TriMesh:
                    pstrMeshResourceName.Serialize(a_File);
                    a_File.Write<ushort>(m_u16ResourceIndex);
                    break;
                case eECollisionShapeType.eECollisionShapeType_Box:
                    boxBox.Serialize(a_File);
                    break;
                case eECollisionShapeType.eECollisionShapeType_Sphere:
                    psphSphere.Serialize(a_File);
                    break;
                case eECollisionShapeType.eECollisionShapeType_Capsule:
                    pcapCapsule.Serialize(a_File);
                    break;
                case eECollisionShapeType.eECollisionShapeType_Point:
                    a_File.Write<Vector3>(pvecPoint);
                    break;
            }
            baseObj.Serialize(a_File);
            m_boxOuterAABBLocal.Serialize(a_File);
            a_File.Write<Vector3>(m_vecLastPosition);
        }

        public override BinaryFileBlock Clone()
        {
            eCCollisionShape D = new eCCollisionShape();
            D.u16FileVersion = u16FileVersion;
            D.enumShapeType = enumShapeType;
            D.baseObj = (bCObjectRefBase)baseObj.Clone();
            D.m_boxOuterAABBLocal = m_boxOuterAABBLocal;
            D.m_vecLastPosition = m_vecLastPosition;
            D.m_matOrientation = m_matOrientation;
            if (pstrMeshResourceName != null)
                D.pstrMeshResourceName = (bCString)pstrMeshResourceName.Clone();
            D.m_u16ResourceIndex = m_u16ResourceIndex;
            if (boxBox != null)
                D.boxBox = (bCOrientedBox)boxBox.Clone();
            if (psphSphere != null)
                D.psphSphere = (bCSphere)psphSphere.Clone();
            if (pcapCapsule != null)
                D.pcapCapsule = (bCCapsule)pcapCapsule.Clone();
            D.fCenterX = fCenterX;
            D.fCenterY = fCenterY;
            D.pvecPoint = pvecPoint;
            return D;
        }

        public override int Size
        {
            get 
            {
                int q = 2;
                switch (enumShapeType)
                {
                case eECollisionShapeType.eECollisionShapeType_TriMesh:
                case eECollisionShapeType.eECollisionShapeType_ConvexHull:
                    q += pstrMeshResourceName.Size;
                    if (u16FileVersion > 31)
                        q += 2;
                    break;
                case eECollisionShapeType.eECollisionShapeType_Box:
                    q += boxBox.Size;
                    break;
                case eECollisionShapeType.eECollisionShapeType_Sphere:
                    q += psphSphere.Size;
                    break;
                case eECollisionShapeType.eECollisionShapeType_Capsule:
                    if (u16FileVersion < 39)
                        q += 8;
                    else
                        q += pcapCapsule.Size;
                    break;
                case eECollisionShapeType.eECollisionShapeType_Point:
                    q += 12;
                    break;
                }
                if (u16FileVersion >= 33)
                    q += baseObj.Size;
                if (u16FileVersion >= 34)
                {
                    q += m_boxOuterAABBLocal.Size;
                }
                if (u16FileVersion >= 61)
                    q += 12;
                if (u16FileVersion < 74 && enumShapeType == eECollisionShapeType.eECollisionShapeType_Box)
                    q += m_matOrientation.Size;
                return q;
            }
        }

        public void SetXColMesh(string s)
        {
            if (enumShapeType == eECollisionShapeType.eECollisionShapeType_ConvexHull || enumShapeType == eECollisionShapeType.eECollisionShapeType_TriMesh)
            {
                pstrMeshResourceName.pString = s;
            }
        }
    }
    public class eCCollisionShape_PS : classData
    {
        short Version;
        List<bCAccessorPropertyObject> arrShapeAccessors;
        List<bCAccessorPropertyObject> arrNotUsed;
        byte bNotUsed;

        public override void deSerialize(IFile bs)
        {
            Version = bs.Read<short>();
            arrShapeAccessors = new List<bCAccessorPropertyObject>(bs.Read<int>());
            for (int i = 0; i < arrShapeAccessors.Capacity; i++)
                arrShapeAccessors.Add(new bCAccessorPropertyObject(bs));
            if ((Version >= 41) && (Version <= 62))
            {
                arrNotUsed = new List<bCAccessorPropertyObject>(bs.Read<int>());
                for (int i = 0; i < arrNotUsed.Capacity; i++)
                    arrNotUsed.Add(new bCAccessorPropertyObject(bs));
                if (Version == 62)
                    bNotUsed = bs.Read<byte>();
            }
        }

        public override void Serialize(IFile s)
        {
            s.Write<short>(Version);
            s.Write<int>(arrShapeAccessors.Count);
            foreach (bCAccessorPropertyObject a in arrShapeAccessors)
                a.Serialize(s);
            if ((Version >= 41) && (Version <= 62))
            {
                s.Write<int>(arrNotUsed.Count);
                foreach (bCAccessorPropertyObject a in arrNotUsed)
                    a.Serialize(s);
                if (Version == 62)
                    s.Write<byte>(bNotUsed);
            }
        }

        public override int Size
        {
            get 
            {
                int q = 2 + arrShapeAccessors.SizeOf();
                if ((Version >= 41) && (Version <= 62))
                {
                    q += arrNotUsed.SizeOf();
                    if (Version == 62)
                        q += 1;
                }
                return q;
            }
        }

        public void RemoveShape(int index)
        {
            arrShapeAccessors.RemoveAt(index);
        }

        public eCCollisionShape this[int index]
        {
            get
            {
                return arrShapeAccessors[index].Class as eCCollisionShape;
            }
        }
    }
    
    public class eCSpeedTreeBBMesh_PS : classData
    {
        short Version;
        bTValArray<Vector4> B0;
        bTValArray<Vector3> B1;
        eCEntityPropertySet E;

        public override void deSerialize(IFile bs)
        {
            Version = bs.Read<short>();
            B0 = new bTValArray<Vector4>(bs);
            B1 = new bTValArray<Vector3>(bs);
            E = new eCEntityPropertySet(bs);
        }

        public override void Serialize(IFile s)
        {
            s.Write<short>(Version);
            B0.Serialize(s);
            B1.Serialize(s);
            E.Serialize(s);
        }

        public override int Size
        {
            get { return 2 + B0.Size + B1.Size + E.Size; }
        }
    }
    public class eCSpeedTreeWind_PS : classData
    {
        short Version;
        eCEntityPropertySet B;

        public override void deSerialize(IFile bs)
        {
            Version = bs.Read<short>();
            B = new eCEntityPropertySet(bs);
        }

        public override void Serialize(IFile s)
        {
            s.Write<short>(Version);
            B.Serialize(s);
        }

        public override int Size
        {
            get { return 2 + B.Size; }
        }
    }

    public class eCPhysicsScene_PS : classData
    {
        short Version;

        public override void deSerialize(IFile bs)
        {
            Version = bs.Read<short>();
        }

        public override void Serialize(IFile s)
        {
            s.Write<short>(Version);
        }

        public override int Size
        {
            get { return 2; }
        }
    }
    public class gCSectorPersistence_PS : classData
    {
        class c0 : BinaryFileBlock
        {
            public short s0;
            public bCString b0;
            public byte b1;

            public c0(string s)
            {
                b0 = new bCString(s);
                s0 = 1;
                b1 = 1;
            }

            public c0(IFile a_File)
            {
                deSerialize(a_File);
            }

            private c0() { }

            public override BinaryFileBlock Clone()
            {
                c0 c = new c0();
                c.s0 = s0;
                c.b0 = (bCString)b0.Clone();
                c.b1 = b1;
                return c;
            }

            public override void deSerialize(IFile a_File)
            {
                s0 = a_File.Read<short>();
                b0 = new bCString(a_File);
                b1 = a_File.Read<byte>();
            }

            public override void Serialize(IFile a_File)
            {
                a_File.Write(s0);
                b0.Serialize(a_File);
                a_File.Write(b1);
            }

            public override int Size
            {
                get { return 3 + b0.Size; }
            }
        }

        short Version;
        List<c0> mArray;

        public override void deSerialize(IFile bs)
        {
            Version = bs.Read<short>();
            bs.Position++;
            mArray = new List<c0>(bs.Read<int>());
            for (int i = 0; i < mArray.Capacity; i++)
                mArray.Add(new c0(bs));
        }

        public override void Serialize(IFile s)
        {
            s.Write<short>(Version);
            s.Write<byte>(1);
            s.Write<int>(mArray.Count);
            foreach (c0 c in mArray)
                c.Serialize(s);
        }

        public override int Size
        {
            get 
            {
                return 3 + mArray.SizeOf();
            }
        }

        public void AddSector(string a_SectorName)
        {
            a_SectorName = a_SectorName.Replace(".sec", "");
            c0 c = new c0(a_SectorName);
            mArray.Add(c);
        }

        public void Clear()
        {
            mArray.Clear();
        }
    }
    
    public class eCRigidBody_PS : classData
    {
        ushort Version;
        byte GEBool_0;
        byte GEBool_1;
        byte GEBool_2;
        public Vector3 bCVector_0;
        public Vector3 bCVector_1;
        byte GEBool_3;
        public bCMotion bm0, bm1;

        public override void deSerialize(IFile S)
        {
            Version = S.Read<ushort>();
            if (Version >= 40)
            {
                GEBool_0 = S.Read<byte>();
                GEBool_1 = S.Read<byte>();
            }
            if(Version >= 41)
                GEBool_2 = S.Read<byte>();
            if(Version >= 51)
            {
                bCVector_0 = S.Read<Vector3>();
                bCVector_1 = S.Read<Vector3>();
            }
            if (Version >= 55)
            {
                bm0 = new bCMotion(S);
                bm1 = new bCMotion(S);
            }
            if(Version >= 65)
                GEBool_3 = S.Read<byte>();
        }
        public override void Serialize(IFile S)
        {
            S.Write<ushort>(Version);
            if (Version >= 40)
            {
                S.Write<byte>(GEBool_0);
                S.Write<byte>(GEBool_1);
            }
            if (Version >= 41)
                S.Write<byte>(GEBool_2);
            if (Version >= 51)
            {
                S.Write<Vector3>(bCVector_0);
                S.Write<Vector3>(bCVector_1);
            }
            if (Version >= 55)
            {
                bm0.Serialize(S);
                bm1.Serialize(S);
            }
            if (Version >= 65)
                S.Write<byte>(GEBool_3);
        }
        public override int Size
        {
            get
            {
                int q = 2;
                if (Version >= 40)
                    q += 2;
                if (Version >= 41)
                    q += 1;
                if (Version >= 51)
                    q += 24;
                if (Version >= 55)
                    q += 56;
                if (Version >= 65)
                    q += 1;
                return q;
            }
        }
    }
    public class eCIlluminated_PS : classData
    {
        ushort GEU16_0;
        byte GEU8_0;
        Vector4[] vec4;
        byte GEBool_0;
        Vector4[] vec4_2;
        float GEFloat_0;

        public override void deSerialize(IFile S)
        {
            GEU16_0 = S.Read<ushort>();
            GEU8_0 = S.Read<byte>();
            vec4 = S.Read<Vector4>(8);
            GEBool_0 = S.Read<byte>();
            vec4_2 = S.Read<Vector4>(7);
            GEFloat_0 = S.Read<float>();
        }
        public override void Serialize(IFile S)
        {
            S.Write<ushort>(GEU16_0);
            S.Write<byte>(GEU8_0);
            S.Write<Vector4>(vec4);
            S.Write<byte>(GEBool_0);
            S.Write<Vector4>(vec4_2);
            S.Write<float>(GEFloat_0);
        }
        public override int Size
        {
            get
            {
                return 0 + Marshal.SizeOf(typeof(ushort)) + Marshal.SizeOf(typeof(byte)) + 128 + Marshal.SizeOf(typeof(byte)) + 112 + Marshal.SizeOf(typeof(float));
            }
        }
    }
    public class gCLock_PS : classData
    {
        ushort GEU16_0;
        public int[] difficultLevels;

        public override void deSerialize(IFile S)
        {
            GEU16_0 = S.Read<ushort>();
            difficultLevels = S.Read<int>(8);
        }
        public override void Serialize(IFile S)
        {
            S.Write<ushort>(GEU16_0);
            S.Write<int>(difficultLevels);
        }
        public override int Size
        {
            get
            {
                return 0 + Marshal.SizeOf(typeof(ushort)) + difficultLevels.Length * 4;
            }
        }
    }
    public class gCParty_PS : classData
    {
        ushort GEU16_0;
        public bCString bCString_0;
        byte GEBool_0;
        public eCEntityProxy proxy;

        public override void deSerialize(IFile S)
        {
            GEU16_0 = S.Read<ushort>();
            bCString_0 = new bCString(S);
            GEBool_0 = S.Read<byte>();
            proxy = new eCEntityProxy(S);
        }
        public override void Serialize(IFile S)
        {
            S.Write<ushort>(GEU16_0);
            bCString_0.Serialize(S);
            S.Write<byte>(GEBool_0);
            proxy.Serialize(S);
        }
        public override int Size
        {
            get
            {
                return 3 + bCString_0.Size + proxy.Size;
            }
        }
    }
    public class gCSkills_PS : classData
    {
        public struct SkillValue
        {
            public gESkill Skill;
            public int Value;
            public override string ToString()
            {
                return Skill.ToString() + " : " + Value.ToString();
            }
        }

        ushort GEU16_0;
        int[] skills;

        public override void deSerialize(IFile S)
        {
            GEU16_0 = S.Read<ushort>();
            skills = S.Read<int>(136 / 4);
        }
        public override void Serialize(IFile S)
        {
            S.Write<ushort>(GEU16_0);
            S.Write<int>(skills);
        }
        public override int Size
        {
            get
            {
                return 0 + Marshal.SizeOf(typeof(ushort)) + skills.Length * 4;
            }
        }

        public SkillValue[] Skills
        {
            get
            {
                Array V = Enum.GetValues(typeof(gESkill));
                SkillValue[] V2 = new SkillValue[V.Length];
                for (int i = 0; i < V.Length; i++)
                {
                    gESkill s = (gESkill)V.GetValue(i);
                    int q = GetSkillValue(s);
                    V2[i].Skill = s;
                    V2[i].Value = q;
                }
                return V2;
            }
        }

        public int GetSkillValue(gESkill S)
        {
            return skills[(int)S];
        }

        public void SetSkillValue(gESkill S, int V)
        {
            skills[(int)S] = V;
        }

        public void CopyFrom(gCSkills_PS a_Source)
        {
            Array.Copy(a_Source.skills, skills, skills.Length);
        }
    }
    public class eCBodyPart_PS : classData
    {
        eCEntityPropertySet set;

        public override void deSerialize(IFile a_File)
        {
            set = new eCEntityPropertySet(a_File);
        }

        public override void Serialize(IFile a_File)
        {
            set.Serialize(a_File);
        }

        public override int Size
        {
            get { return set.Size; }
        }
    }

    public class eCSkydome_PS : classData
    {
        ushort GEU16_0;

        public override void deSerialize(IFile S)
        {
            GEU16_0 = S.Read<ushort>();
        }
        public override void Serialize(IFile S)
        {
            S.Write<ushort>(GEU16_0);
        }
        public override int Size
        {
            get
            {
                return 0 + Marshal.SizeOf(typeof(ushort));
            }
        }
    }
    public class eCPrecipitation_PS : classData
    {
        ushort GEU16_0;

        public override void deSerialize(IFile S)
        {
            GEU16_0 = S.Read<ushort>();
        }
        public override void Serialize(IFile S)
        {
            S.Write<ushort>(GEU16_0);
        }
        public override int Size
        {
            get
            {
                return 0 + Marshal.SizeOf(typeof(ushort));
            }
        }
    }
    public class eCHemisphere_PS : classData
    {
        ushort GEU16_0;

        public override void deSerialize(IFile S)
        {
            GEU16_0 = S.Read<ushort>();
        }
        public override void Serialize(IFile S)
        {
            S.Write<ushort>(GEU16_0);
        }
        public override int Size
        {
            get
            {
                return 0 + Marshal.SizeOf(typeof(ushort));
            }
        }
    }
    public class eCDirectionalLight_PS : classData
    {
        ushort GEU16_0;

        public override void deSerialize(IFile S)
        {
            GEU16_0 = S.Read<ushort>();
        }
        public override void Serialize(IFile S)
        {
            S.Write<ushort>(GEU16_0);
        }
        public override int Size
        {
            get
            {
                return 0 + Marshal.SizeOf(typeof(ushort));
            }
        }
    }
    public class eCAnimation_PS : classData
    {
        eCEntityPropertySet set;

        public override void deSerialize(IFile S)
        {
            set = new eCEntityPropertySet(S);
        }
        public override void Serialize(IFile S)
        {
            set.Serialize(S);
        }
        public override int Size
        {
            get
            {
                return set.Size;
            }
        }
    }
    public class eCArea_StringProperty_PS : classData
    {
        ushort GEU16_0;

        public override void deSerialize(IFile S)
        {
            GEU16_0 = S.Read<ushort>();
        }
        public override void Serialize(IFile S)
        {
            S.Write<ushort>(GEU16_0);
        }
        public override int Size
        {
            get
            {
                return 0 + Marshal.SizeOf(typeof(ushort));
            }
        }
    }
    public class eCAudioEmitter_PS : classData
    {
        ushort GEU16_0;

        public override void deSerialize(IFile S)
        {
            GEU16_0 = S.Read<ushort>();
        }
        public override void Serialize(IFile S)
        {
            S.Write<ushort>(GEU16_0);
        }
        public override int Size
        {
            get
            {
                return 0 + Marshal.SizeOf(typeof(ushort));
            }
        }
    }
    public class eCBillboard_PS : classData
    {
        ushort GEU16_0;

        public override void deSerialize(IFile S)
        {
            GEU16_0 = S.Read<ushort>();
        }
        public override void Serialize(IFile S)
        {
            S.Write<ushort>(GEU16_0);
        }
        public override int Size
        {
            get
            {
                return 0 + Marshal.SizeOf(typeof(ushort));
            }
        }
    }
    public class eCDecal_PS : classData
    {
        ushort GEU16_0;

        public override void deSerialize(IFile S)
        {
            GEU16_0 = S.Read<ushort>();
        }
        public override void Serialize(IFile S)
        {
            S.Write<ushort>(GEU16_0);
        }
        public override int Size
        {
            get
            {
                return 0 + Marshal.SizeOf(typeof(ushort));
            }
        }
    }
    public class eCMesh_PS : classData
    {
        ushort GEU16_0;
        bCBox box;
        eCEntityPropertySet set;

        public override void deSerialize(IFile S)
        {
            GEU16_0 = S.Read<ushort>();
            if (GEU16_0 == 2)
                box = new bCBox(S);
            set = new eCEntityPropertySet(S);

        }
        public override void Serialize(IFile S)
        {
            S.Write<ushort>(GEU16_0);
            if (GEU16_0 == 2)
                box.Serialize(S);
            set.Serialize(S);
        }
        public override int Size
        {
            get
            {
                if (GEU16_0 == 2)
                    return 2 + box.Size + set.Size;
                else return 2 + set.Size;
            }
        }
    }
    public class eCSpeedTree_PS : classData
    {
        ushort GEU16_0;
        eCEntityPropertySet set;

        public override void deSerialize(IFile S)
        {
            GEU16_0 = S.Read<ushort>();
            set = new eCEntityPropertySet(S);
        }
        public override void Serialize(IFile S)
        {
            S.Write<ushort>(GEU16_0);
            set.Serialize(S);
        }
        public override int Size
        {
            get
            {
                return 2 + set.Size;
            }
        }
    }
    public class eCStaticAmbientLight_PS : classData
    {
        ushort GEU16_0;
        public Vector3 bCVector_0;

        public override void deSerialize(IFile S)
        {
            GEU16_0 = S.Read<ushort>();
            if(GEU16_0 > 1)
                bCVector_0 = S.Read<Vector3>();
        }
        public override void Serialize(IFile S)
        {
            S.Write<ushort>(GEU16_0);
            if (GEU16_0 > 1)
                S.Write<Vector3>(bCVector_0);
        }
        public override int Size
        {
            get
            {
                if (GEU16_0 > 1)
                    return 2 + 12;
                else return 2;
            }
        }
    }
    public class eCStrip_PS : classData
    {
        ushort GEU16_0;

        public override void deSerialize(IFile S)
        {
            GEU16_0 = S.Read<ushort>();
        }
        public override void Serialize(IFile S)
        {
            S.Write<ushort>(GEU16_0);
        }
        public override int Size
        {
            get
            {
                return 0 + Marshal.SizeOf(typeof(ushort));
            }
        }
    }
    public class eCWeatherZone_PS : classData
    {
        ushort GEU16_0;

        public override void deSerialize(IFile S)
        {
            GEU16_0 = S.Read<ushort>();
        }
        public override void Serialize(IFile S)
        {
            S.Write<ushort>(GEU16_0);
        }
        public override int Size
        {
            get
            {
                return 0 + Marshal.SizeOf(typeof(ushort));
            }
        }
    }
    public class gCAIHelper_FreePoint_PS : classData
    {
        ushort GEU16_0;

        public override void deSerialize(IFile S)
        {
            GEU16_0 = S.Read<ushort>();
        }
        public override void Serialize(IFile S)
        {
            S.Write<ushort>(GEU16_0);
        }
        public override int Size
        {
            get
            {
                return 0 + Marshal.SizeOf(typeof(ushort));
            }
        }
    }
    public class gCAIHelper_Label_PS : classData
    {
        ushort GEU16_0;

        public override void deSerialize(IFile S)
        {
            GEU16_0 = S.Read<ushort>();
        }
        public override void Serialize(IFile S)
        {
            S.Write<ushort>(GEU16_0);
        }
        public override int Size
        {
            get
            {
                return 0 + Marshal.SizeOf(typeof(ushort));
            }
        }
    }
    public class gCAIZone_PS : classData
    {
        ushort GEU16_0;

        public override void deSerialize(IFile S)
        {
            GEU16_0 = S.Read<ushort>();
        }
        public override void Serialize(IFile S)
        {
            S.Write<ushort>(GEU16_0);
        }
        public override int Size
        {
            get
            {
                return 0 + Marshal.SizeOf(typeof(ushort));
            }
        }
    }
    public class gCAnchor_PS : classData
    {
        ushort GEU16_0;
        bTRefPtrArray<eCEntityProxy> m_Array;

        public override void deSerialize(IFile S)
        {
            GEU16_0 = S.Read<ushort>();
            if (GEU16_0 == 1)
                m_Array = new bTRefPtrArray<eCEntityProxy>(S);
        }
        public override void Serialize(IFile S)
        {
            S.Write<ushort>(GEU16_0);
            if (GEU16_0 == 1)
                m_Array.Serialize(S);
        }
        public override int Size
        {
            get
            {
                if (GEU16_0 == 1)
                    return 2 + m_Array.Size;
                else return 2;
            }
        }
    }
    public class gCArena_PS : classData
    {
        ushort GEU16_0;

        public override void deSerialize(IFile S)
        {
            GEU16_0 = S.Read<ushort>();
        }
        public override void Serialize(IFile S)
        {
            S.Write<ushort>(GEU16_0);
        }
        public override int Size
        {
            get
            {
                return 0 + Marshal.SizeOf(typeof(ushort));
            }
        }
    }
    public class gCBook_PS : classData
    {
        ushort GEU16_0;

        public override void deSerialize(IFile S)
        {
            GEU16_0 = S.Read<ushort>();
        }
        public override void Serialize(IFile S)
        {
            S.Write<ushort>(GEU16_0);
        }
        public override int Size
        {
            get
            {
                return 0 + Marshal.SizeOf(typeof(ushort));
            }
        }
    }
    public class gCCastInfo_PS : classData
    {
        ushort GEU16_0;

        public override void deSerialize(IFile S)
        {
            GEU16_0 = S.Read<ushort>();
        }
        public override void Serialize(IFile S)
        {
            S.Write<ushort>(GEU16_0);
        }
        public override int Size
        {
            get
            {
                return 0 + Marshal.SizeOf(typeof(ushort));
            }
        }
    }
    public class gCCharacterMovement_PS : classData
    {
        ushort GEU16_0;

        public override void deSerialize(IFile S)
        {
            GEU16_0 = S.Read<ushort>();
        }
        public override void Serialize(IFile S)
        {
            S.Write<ushort>(GEU16_0);
        }
        public override int Size
        {
            get
            {
                return 0 + Marshal.SizeOf(typeof(ushort));
            }
        }
    }
    public class gCCollisionCircle_PS : classData
    {
        ushort GEU16_0;
        public ulong bCDateTime_0;

        public override void deSerialize(IFile S)
        {
            GEU16_0 = S.Read<ushort>();
            bCDateTime_0 = S.Read<ulong>();
        }
        public override void Serialize(IFile S)
        {
            S.Write<ushort>(GEU16_0);
            S.Write<ulong>(bCDateTime_0);
        }
        public override int Size
        {
            get
            {
                return 0 + Marshal.SizeOf(typeof(ushort)) + Marshal.SizeOf(typeof(ulong));
            }
        }
    }
    public class gCCombatSystem_PS : classData
    {
        ushort GEU16_0;
        byte GEBool_0;

        public override void deSerialize(IFile S)
        {
            GEU16_0 = S.Read<ushort>();
            GEBool_0 = S.Read<byte>();
        }
        public override void Serialize(IFile S)
        {
            S.Write<ushort>(GEU16_0);
            S.Write<byte>(GEBool_0);
        }
        public override int Size
        {
            get
            {
                return 0 + Marshal.SizeOf(typeof(ushort)) + Marshal.SizeOf(typeof(byte));
            }
        }
    }
    public class gCDamage_PS : classData
    {
        ushort GEU16_0;

        public override void deSerialize(IFile S)
        {
            GEU16_0 = S.Read<ushort>();
        }
        public override void Serialize(IFile S)
        {
            S.Write<ushort>(GEU16_0);
        }
        public override int Size
        {
            get
            {
                return 0 + Marshal.SizeOf(typeof(ushort));
            }
        }
    }
    public class gCDialog_PS : classData
    {
        ushort GEU16_0;

        public override void deSerialize(IFile S)
        {
            GEU16_0 = S.Read<ushort>();
        }
        public override void Serialize(IFile S)
        {
            S.Write<ushort>(GEU16_0);
        }
        public override int Size
        {
            get
            {
                return 0 + Marshal.SizeOf(typeof(ushort));
            }
        }
    }
    public class gCDoor_PS : classData
    {
        ushort GEU16_0;

        public override void deSerialize(IFile S)
        {
            GEU16_0 = S.Read<ushort>();
        }
        public override void Serialize(IFile S)
        {
            S.Write<ushort>(GEU16_0);
        }
        public override int Size
        {
            get
            {
                return 0 + Marshal.SizeOf(typeof(ushort));
            }
        }
    }
    public class gCDynamicCollisionCircle_PS : classData
    {
        ushort GEU16_0;

        public override void deSerialize(IFile S)
        {
            GEU16_0 = S.Read<ushort>();
        }
        public override void Serialize(IFile S)
        {
            S.Write<ushort>(GEU16_0);
        }
        public override int Size
        {
            get
            {
                return 0 + Marshal.SizeOf(typeof(ushort));
            }
        }
    }
    public class gCEffect_PS : classData
    {
        ushort GEU16_0;

        public override void deSerialize(IFile S)
        {
            GEU16_0 = S.Read<ushort>();
        }
        public override void Serialize(IFile S)
        {
            S.Write<ushort>(GEU16_0);
        }
        public override int Size
        {
            get
            {
                return 0 + Marshal.SizeOf(typeof(ushort));
            }
        }
    }
    public class gCItem_PS : classData
    {
        ushort GEU16_0;
        uint GEU32_0;
        byte GEBool_0;

        public override void deSerialize(IFile S)
        {
            GEU16_0 = S.Read<ushort>();
            GEU32_0 = S.Read<uint>();
            GEBool_0 = S.Read<byte>();
        }
        public override void Serialize(IFile S)
        {
            S.Write<ushort>(GEU16_0);
            S.Write<uint>(GEU32_0);
            S.Write<byte>(GEBool_0);
        }
        public override int Size
        {
            get
            {
                return 0 + Marshal.SizeOf(typeof(ushort)) + Marshal.SizeOf(typeof(uint)) + Marshal.SizeOf(typeof(byte));
            }
        }
    }
    public class gCLetter_PS : classData
    {
        ushort GEU16_0;

        public override void deSerialize(IFile S)
        {
            GEU16_0 = S.Read<ushort>();
        }
        public override void Serialize(IFile S)
        {
            S.Write<ushort>(GEU16_0);
        }
        public override int Size
        {
            get
            {
                return 0 + Marshal.SizeOf(typeof(ushort));
            }
        }
    }
    public class gCMapInfo_PS : classData
    {
        ushort GEU16_0;

        public override void deSerialize(IFile S)
        {
            GEU16_0 = S.Read<ushort>();
        }
        public override void Serialize(IFile S)
        {
            S.Write<ushort>(GEU16_0);
        }
        public override int Size
        {
            get
            {
                return 0 + Marshal.SizeOf(typeof(ushort));
            }
        }
    }
    public class gCNavHelper_PS : classData
    {
        ushort GEU16_0;

        public override void deSerialize(IFile S)
        {
            GEU16_0 = S.Read<ushort>();
        }
        public override void Serialize(IFile S)
        {
            S.Write<ushort>(GEU16_0);
        }
        public override int Size
        {
            get
            {
                return 0 + Marshal.SizeOf(typeof(ushort));
            }
        }
    }
    public class gCNavigation_PS : classData
    {
        ushort GEU16_0;

        public override void deSerialize(IFile S)
        {
            GEU16_0 = S.Read<ushort>();
        }
        public override void Serialize(IFile S)
        {
            S.Write<ushort>(GEU16_0);
        }
        public override int Size
        {
            get
            {
                return 0 + Marshal.SizeOf(typeof(ushort));
            }
        }
    }
    public class gCNavOffset_PS : classData
    {
        ushort GEU16_0;

        public override void deSerialize(IFile S)
        {
            GEU16_0 = S.Read<ushort>();
        }
        public override void Serialize(IFile S)
        {
            S.Write<ushort>(GEU16_0);
        }
        public override int Size
        {
            get
            {
                return 0 + Marshal.SizeOf(typeof(ushort));
            }
        }
    }
    public class gCNavPath_PS : classData
    {
        ushort GEU16_0;
        public ulong bCDateTime_0;

        public override void deSerialize(IFile S)
        {
            GEU16_0 = S.Read<ushort>();
            bCDateTime_0 = S.Read<ulong>();
        }
        public override void Serialize(IFile S)
        {
            S.Write<ushort>(GEU16_0);
            S.Write<ulong>(bCDateTime_0);
        }
        public override int Size
        {
            get
            {
                return 0 + Marshal.SizeOf(typeof(ushort)) + Marshal.SizeOf(typeof(ulong));
            }
        }
    }
    public class gCNavZone_PS : classData
    {
        ushort GEU16_0;
        public ulong bCDateTime_0;

        public override void deSerialize(IFile S)
        {
            GEU16_0 = S.Read<ushort>();
            bCDateTime_0 = S.Read<ulong>();
        }
        public override void Serialize(IFile S)
        {
            S.Write<ushort>(GEU16_0);
            S.Write<ulong>(bCDateTime_0);
        }
        public override int Size
        {
            get
            {
                return 0 + Marshal.SizeOf(typeof(ushort)) + Marshal.SizeOf(typeof(ulong));
            }
        }
    }
    public class gCNegZone_PS : classData
    {
        ushort GEU16_0;
        public ulong bCDateTime_0;

        public override void deSerialize(IFile S)
        {
            GEU16_0 = S.Read<ushort>();
            bCDateTime_0 = S.Read<ulong>();
        }
        public override void Serialize(IFile S)
        {
            S.Write<ushort>(GEU16_0);
            S.Write<ulong>(bCDateTime_0);
        }
        public override int Size
        {
            get
            {
                return 0 + Marshal.SizeOf(typeof(ushort)) + Marshal.SizeOf(typeof(ulong));
            }
        }
    }
    public class gCNPC_PS : classData
    {
        ushort GEU16_0;

        public override void deSerialize(IFile S)
        {
            GEU16_0 = S.Read<ushort>();
        }
        public override void Serialize(IFile S)
        {
            S.Write<ushort>(GEU16_0);
        }
        public override int Size
        {
            get
            {
                return 0 + Marshal.SizeOf(typeof(ushort));
            }
        }
    }
    public class gCPrefPath_PS : classData
    {
        ushort GEU16_0;
        public ulong bCDateTime_0;

        public override void deSerialize(IFile S)
        {
            GEU16_0 = S.Read<ushort>();
            bCDateTime_0 = S.Read<ulong>();
        }
        public override void Serialize(IFile S)
        {
            S.Write<ushort>(GEU16_0);
            S.Write<ulong>(bCDateTime_0);
        }
        public override int Size
        {
            get
            {
                return 0 + Marshal.SizeOf(typeof(ushort)) + Marshal.SizeOf(typeof(ulong));
            }
        }
    }
    public class gCProjectile2_PS : classData
    {
        public override void deSerialize(IFile S)
        {
        }
        public override void Serialize(IFile S)
        {
        }
        public override int Size
        {
            get
            {
                return 0;
            }
        }
    }
    public class gCRecipe_PS : classData
    {
        ushort GEU16_0;

        public override void deSerialize(IFile S)
        {
            GEU16_0 = S.Read<ushort>();
        }
        public override void Serialize(IFile S)
        {
            S.Write<ushort>(GEU16_0);
        }
        public override int Size
        {
            get
            {
                return 0 + Marshal.SizeOf(typeof(ushort));
            }
        }
    }
    public class gCScriptRoutine_PS : classData
    {
        ushort GEU16_0;

        public override void deSerialize(IFile S)
        {
            GEU16_0 = S.Read<ushort>();
        }
        public override void Serialize(IFile S)
        {
            S.Write<ushort>(GEU16_0);
        }
        public override int Size
        {
            get
            {
                return 0 + Marshal.SizeOf(typeof(ushort));
            }
        }
    }
    public class gCWaterZone_PS : classData
    {
        ushort GEU16_0;

        public override void deSerialize(IFile S)
        {
            GEU16_0 = S.Read<ushort>();
        }
        public override void Serialize(IFile S)
        {
            S.Write<ushort>(GEU16_0);
        }
        public override int Size
        {
            get
            {
                return 0 + Marshal.SizeOf(typeof(ushort));
            }
        }
    }
    public class gCClock_PS : classData
    {
        short Version;

        public override void deSerialize(IFile bs)
        {
            Version = bs.Read<short>();
        }

        public override void Serialize(IFile s)
        {
            s.Write<short>(Version);
        }

        public override int Size
        {
            get { return 2; }
        }
    }
    public class gCGameScript_PS : classData
    {
        short Version;

        public override void deSerialize(IFile bs)
        {
            Version = bs.Read<short>();
        }

        public override void Serialize(IFile s)
        {
            s.Write<short>(Version);
        }

        public override int Size
        {
            get { return 2; }
        }
    }
    public class eCPointLight_PS : classData
    {
        short Version;

        public override void deSerialize(IFile a_File)
        {
            Version = a_File.Read<short>();
        }

        public override void Serialize(IFile a_File)
        {
            a_File.Write(Version);
        }

        public override int Size
        {
            get { return 2; }
        }
    }
    public class eCStaticPointLight_PS : classData
    {
        short Version;
        Vector3 pos;

        public override void deSerialize(IFile a_File)
        {
            Version = a_File.Read<short>();
            if (Version > 1)
                pos = a_File.Read<Vector3>();
        }

        public override void Serialize(IFile a_File)
        {
            a_File.Write(Version);
            if (Version > 1)
                a_File.Write(pos);
        }

        public override int Size
        {
            get {
                if (Version > 1)
                    return 14;
                else return 2;
            }
        }
    }
    public class gCMap_PS : classData
    {
        short Version;
        List<GameLibrary.Tuple<bCString, byte>> data;
        int i0;

        public override void deSerialize(IFile a_File)
        {
            Version = a_File.Read<short>();
            if (Version == 1)
            {
                data = new List<GameLibrary.Tuple<bCString, byte>>(a_File.Read<int>());
                i0 = a_File.Read<int>();
                for (int i = 0; i < data.Capacity; i++)
                {
                    GameLibrary.Tuple<bCString, byte> t = new GameLibrary.Tuple<bCString, byte>();
                    t.Object0 = new bCString(a_File);
                    t.Object1 = a_File.Read<byte>();
                    data.Add(t);
                }
            }
        }

        public override void Serialize(IFile a_File)
        {
            a_File.Write(Version);
            if (Version == 1)
            {
                a_File.Write(data.Count);
                a_File.Write(i0);
                foreach (GameLibrary.Tuple<bCString, byte> t in data)
                {
                    t.Object0.Serialize(a_File);
                    a_File.Write(t.Object1);
                }
            }
        }

        public override int Size
        {
            get 
            {
                int q = 2;
                if (Version == 1)
                {
                    q += 8;
                    foreach (GameLibrary.Tuple<bCString, byte> t in data)
                        q += t.Object0.Size + 1;
                }
                return q;
            }
        }
    }
    #endregion
}
